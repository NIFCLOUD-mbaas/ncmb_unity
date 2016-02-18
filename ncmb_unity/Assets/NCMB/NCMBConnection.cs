/*******
 Copyright 2014 NIFTY Corporation All Rights Reserved.
 
 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at
 
 http://www.apache.org/licenses/LICENSE-2.0
 
 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
 **********/

using System.Collections;
using System.Net;
using System.Collections.Specialized;
using System.Net.Security;
using System.Text;
using System.Threading;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;


//Dictionary
using System.IO;

//strem
using System.Security.Cryptography;
using NCMB.Internal;

namespace NCMB.Internal
{
	internal class NCMBConnection
	{
		private static readonly string RESPONSE_SIGNATURE = "X-NCMB-Response-Signature";
		//レスポンスシグネチャ　キー
		private static readonly string SIGNATURE_METHOD_KEY = "SignatureMethod";
		//シグネチャメソッド　キー
		private static readonly string SIGNATURE_METHOD_VALUE = "HmacSHA256";
		//シグネチャメソッド　値
		private static readonly string SIGNATURE_VERSION_KEY = "SignatureVersion";
		//シグネチャバージョン　キー
		private static readonly string SIGNATURE_VERSION_VALUE = "2";
		//シグネチャバージョン　値
		private static readonly string HEADER_SIGNATURE = "X-NCMB-Signature";
		//シグネチャヘッダー　キー
		private static readonly string HEADER_APPLICATION_KEY = "X-NCMB-Application-Key";
		//アプリケションキー　キー
		private static readonly string HEADER_TIMESTAMP_KEY = "X-NCMB-Timestamp";
		//タイムスタンプ　キー
		private static readonly string HEADER_ACCESS_CONTROL_ALLOW_ORIGIN = "Access-Control-Allow-Origin";
		//Access-Control　キー
		private static readonly string HEADER_SESSION_TOKEN = "X-NCMB-Apps-Session-Token";
		//セッショントークン
		private static readonly int REQUEST_TIME_OUT = 10000;
		private string _applicationKey = "";
		private string _clientKey = "";
		private string _headerTimestamp = "";
		//タイムスタンプ　値
		private ConnectType _method;
		//コンテントタイプ(POST,PUT,GET,DELETE)
		private string _url = "";
		//リクエスト先URL
		private string _content = "";
		//JSON化された送信データ
		private string _sessionToken = "";
		//domain Uri
		private Uri _domainUri = null;
		//コンストラクタ
		internal NCMBConnection (String url, ConnectType method, string content, string sessionToken)
			: this (url, method, content, sessionToken, CommonConstant.DOMAIN_URL)
		{
		}

		//コンストラクタ
		internal NCMBConnection (String url, ConnectType method, string content, string sessionToken, string domain)
		{
			this._method = method;
			this._content = content;
			this._url = url;
			this._sessionToken = sessionToken;
			this._applicationKey = NCMBSettings.ApplicationKey;
			this._clientKey = NCMBSettings.ClientKey;
			this._domainUri = new Uri (domain);
		}
		//通信処理(同期通)
		internal void Connect (HttpClientCallback callback)
		{
			//try {
			//証明書更新　更新しないとSSLサイトにアクセス出来ない
			ServicePointManager.ServerCertificateValidationCallback = delegate {
				return true;
			}; 
			//リクエストの作成
			HttpWebRequest req = _returnRequest ();
			//非同期データ送信　BeginGetRequestStreamでくくらなければ同期通信
			_Connection (req, callback);
		}

		private void _Connection (HttpWebRequest req, HttpClientCallback callback)
		{
			int statusCode = 0;
			string responseData = null;
			NCMBException error = null;

			//Post,Put時のコンテントデータ書き込み
			if (_method == ConnectType.POST || _method == ConnectType.PUT) {
				req = this._sendRequest (req, ref error);
				//書き込みでエラーがあれば終了
				if (error != null) {
					callback (statusCode, responseData, error);
					return;
				}
			}

			HttpWebResponse httpResponse = null;
			Stream streamResponse = null;
			StreamReader streamRead = null;

			try {
				//通常処理
				httpResponse = (HttpWebResponse)req.GetResponse ();//①通信
				streamResponse = httpResponse.GetResponseStream (); //②通信結果からResponseデータ作成
				statusCode = (int)httpResponse.StatusCode; //③Responseデータからステータスコード取得
				streamRead = new StreamReader (streamResponse); //④Responseデータからデータ取得
				responseData = streamRead.ReadToEnd ();//⑤書き出したデータを全てstringに書き出し

			} catch (WebException ex) {

				//API側からのエラー処理
				using (WebResponse webResponse = ex.Response) {//①WebExceptionからWebResponseを取得
					error = new NCMBException ();
					error.ErrorMessage = ex.Message;

					streamResponse = webResponse.GetResponseStream ();//②WebResponsからResponseデータ作成
					streamRead = new StreamReader (streamResponse); //③Responseデータからデータ取得
					responseData = streamRead.ReadToEnd ();//④書き出したデータを全てstringに書き出し

					var jsonData = MiniJSON.Json.Deserialize (responseData) as Dictionary<string,object>;//⑤Dictionaryに変換
					var hashtableData = new Hashtable (jsonData);//⑥Hashtableに変換　必要？

					error.ErrorCode = (hashtableData ["code"].ToString ());//⑦Hashtableから各keyのvalue取得
					error.ErrorMessage = (hashtableData ["error"].ToString ());

					httpResponse = (HttpWebResponse)webResponse;//⑧WebResponseをHttpWebResponseに変換
					statusCode = (int)httpResponse.StatusCode;//⑨httpWebResponseからステータスコード取得
				}
			} finally {
				if (httpResponse != null) {
					httpResponse.Close ();
				}
				if (streamResponse != null) {
					streamResponse.Close ();
				}
				if (streamRead != null) {
					streamRead.Close ();
				}
				//check if session token error have or not
				if (error != null) {
					_checkInvalidSessionToken (error.ErrorCode);
				}

				//レスポンスデータにエスケープシーケンスがあればアンエスケープし、mobile backend上と同一にします
				if (responseData != null) {
					if (responseData != Regex.Unescape (responseData)) {
						responseData = Regex.Unescape (responseData);
					}
				}  

				//レスポンスシグネチャのチェック　Flagがtureかつエラーコードが以下の三つ以外の時はチェックを行う
				//if (NCMBSettings._responseValidationFlag && (errorCode != "E404002" && errorCode != "E405001" && errorCode != "E415001")) {
				if (NCMBSettings._responseValidationFlag && httpResponse != null) {

					//レスポンスシグネチャが無い場合はE100001エラー
					if (httpResponse.Headers.GetValues (RESPONSE_SIGNATURE) != null) {
						string responseSignature = httpResponse.Headers.GetValues (RESPONSE_SIGNATURE) [0];
						_signatureCheck (responseSignature, ref statusCode, ref responseData, ref error);
					} else {
						statusCode = 100;
						responseData = "{}";
						error = new NCMBException ();
						error.ErrorCode = "E100001";
						error.ErrorMessage = "Authentication error by response signature incorrect.";
					}
				}

				callback (statusCode, responseData, error);
			}
		}

		private void _signatureCheck (string responseSignature, ref int statusCode, ref string responseData, ref NCMBException error)
		{
			//hashデータ作成
			StringBuilder stringHashData = _makeSignatureHashData ();

			//レスポンスデータ追加 Delete時はレスポンスデータが無いためチェックする
			if (responseData != "") {
				stringHashData.Append ("\n");
				stringHashData.Append (responseData);
			}

			//シグネチャ再生成
			string responseMakeSignature = _makeSignature (stringHashData.ToString ());

			//レスポンスシグネチャと生成したシグネチャが違う場合はエラー
			if (responseSignature != responseMakeSignature) {
				statusCode = 100;
				responseData = "{}";
				error = new NCMBException ();
				error.ErrorCode = "E100001";
				error.ErrorMessage = "Authentication error by response signature incorrect.";
			}
			NCMBDebug.Log ("【responseSignature】　" + responseSignature);
			NCMBDebug.Log ("【responseMakeSignature】　" + responseMakeSignature);
		}
		/*
		//通信処理(非同期通)
		internal void ConnectAsync (HttpClientCallback callback)
		{
			//証明書更新　更新しないとSSLサイトにアクセス出来ない
			ServicePointManager.ServerCertificateValidationCallback = delegate {
				return true;
			}; 
			//リクエストの作成
			HttpWebRequest req = _returnRequest ();
			//非同期データ送信　BeginGetRequestStreamでくくらなければ同期通信
			if (_method == ConnectType.POST || _method == ConnectType.PUT) {
				//リクエスト非同期処理
				IAsyncResult requestResult = req.BeginGetRequestStream (ar => {
					Stream postStream = req.EndGetRequestStream (ar);                //非同期要求を終了
					byte[] postDataBytes = Encoding.Default.GetBytes (_content);    //送信データ作成。バイト型配列に変換
					postStream.Write (postDataBytes, 0, postDataBytes.Length);      //送信
					postStream.Close ();                                           //リリース
					IAsyncResult responsResult = req.BeginGetResponse (ar2 => {
						HttpWebResponse response = (HttpWebResponse)req.EndGetResponse (ar2); //非同期要求を終了
						Stream streamResponse = response.GetResponseStream (); //応答データを受信するためのStreamを取得
						int statusCode = (int)response.StatusCode; //ステータスコード取得
						StreamReader streamRead = new StreamReader (streamResponse); //レスポンスデータ取得
						string responseData = streamRead.ReadToEnd ();
						// 閉じる.リリース
						streamResponse.Close ();
						streamRead.Close ();
						response.Close ();
						callback (statusCode, responseData, null);//コールバックを返す
					}, null);
				}, null);
			} else if (_method == ConnectType.GET || _method == ConnectType.DELETE) {  //コールバックをメソッドにしなくてもこう言う書き方も有りです
				IAsyncResult responseResult = req.BeginGetResponse (ar => {
					try {
						HttpWebResponse res = (HttpWebResponse)req.EndGetResponse (ar);
						int statusCode = (int)res.StatusCode;
						Stream streamResponse = res.GetResponseStream ();
						StreamReader streamRead = new StreamReader (streamResponse); //レスポンスデータ取得
						string responseData = streamRead.ReadToEnd ();
						callback (statusCode, responseData, null);
					} catch (WebException e) {
						NCMBDebug.LogError ("失敗error:" + e);
					}
				}, null);
			}
		}
		*/
		//同期データ送信
		private HttpWebRequest _sendRequest (HttpWebRequest req, ref NCMBException error)
		{
			byte[] postDataBytes = Encoding.Default.GetBytes (_content); 
			Stream stream = null;
			try {
				stream = req.GetRequestStream ();
				stream.Write (postDataBytes, 0, postDataBytes.Length);
			} catch (SystemException cause) {
				//エラー処理
				//throw new NCMBException (cause);
				error = new NCMBException (cause);
			} finally {
				if (stream != null) {
					stream.Close ();
				}
			}
			return req;
		}

		/// <summary>
		/// リクエストの生成を行う
		/// </summary>
		internal HttpWebRequest _returnRequest ()
		{
			//URLをエンコード
			var uri = new Uri (_url);
			_url = uri.AbsoluteUri;

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create (_url); //デフォルトの生成
			_makeTimeStamp (); //タイムスタンプの生成
			req.Timeout = REQUEST_TIME_OUT;
			StringBuilder stringHashData = _makeSignatureHashData ();
			string result = _makeSignature (stringHashData.ToString ()); //シグネチャ生成
			//ヘッダー設定 
			//メソッド追加
			switch (_method) {
			case ConnectType.POST:
				req.Method = "POST";
				break;
			case ConnectType.PUT:
				req.Method = "PUT";
				break;
			case ConnectType.GET:
				req.Method = "GET";
				break;
			case ConnectType.DELETE:
				req.Method = "DELETE";
				break;
			}
			req.ContentType = "application/json";
			req.Headers.Add (HEADER_APPLICATION_KEY, _applicationKey);
			req.Headers.Add (HEADER_SIGNATURE, result);
			req.Headers.Add (HEADER_TIMESTAMP_KEY, _headerTimestamp);
			if ((_sessionToken != null) && (_sessionToken != "")) {
				req.Headers.Add (HEADER_SESSION_TOKEN, _sessionToken);
				NCMBDebug.Log ("Session token :" + _sessionToken);
			}
			req.Headers.Add (HEADER_ACCESS_CONTROL_ALLOW_ORIGIN, "*");
			return req;
		}

		private StringBuilder _makeSignatureHashData ()
		{
			StringBuilder data = new StringBuilder (); //シグネチャ（ハッシュ化）するデータの生成
			String path = _url.Substring (this._domainUri.OriginalString.Length); // パス以降の設定,取得
			String[] temp = path.Split ('?');
			path = temp [0];
			String parameter = null;
			if (temp.Length > 1) {
				parameter = temp [1];
			}
			Hashtable hashValue = new Hashtable (); //昇順に必要なデータを格納するリスト
			hashValue [SIGNATURE_METHOD_KEY] = SIGNATURE_METHOD_VALUE;//シグネチャキー 
			hashValue [SIGNATURE_VERSION_KEY] = SIGNATURE_VERSION_VALUE; // シグネチャバージョン
			hashValue [HEADER_APPLICATION_KEY] = _applicationKey;
			hashValue [HEADER_TIMESTAMP_KEY] = _headerTimestamp;
			String[] tempParameter;
			if (parameter != null) {
				foreach (string param in parameter.Split('&')) {
					tempParameter = param.Split ('=');
					hashValue [tempParameter [0]] = tempParameter [1];
				}
			}
			//sort hashTable base on key
			List<string> tmpAscendingList = new List<string> (); //昇順に必要なデータを格納するリスト
			foreach (DictionaryEntry s in hashValue) {
				tmpAscendingList.Add (s.Key.ToString ());
			}
			StringComparer cmp = StringComparer.Ordinal;
			tmpAscendingList.Sort (cmp);
			//Create data
			data.Append (_method); //メソッド追加
			data.Append ("\n");
			data.Append (this._domainUri.Host); //ドメインの追加
			data.Append ("\n");
			data.Append (path); //パスの追加
			data.Append ("\n");
			foreach (string tmp in tmpAscendingList) {
				data.Append (tmp + "=" + hashValue [tmp] + "&"); 
			}
			data.Remove (data.Length - 1, 1); //最後の&を削除
			return data;
		}

		/// <summary>
		/// ハッシュデータを元にシグネチャの生成を行う
		/// </summary>
		//シグネチャ（ハッシュデータ）生成
		private string _makeSignature (string stringData)
		{

			//署名(シグネチャ)生成
			string result = null; //シグネチャ結果の収納
			byte[] secretKeyBArr = Encoding.UTF8.GetBytes (_clientKey); //秘密鍵(クライアントキー)
			byte[] contentBArr = Encoding.UTF8.GetBytes (stringData); //認証データ
			//秘密鍵と認証データより署名作成
			HMACSHA256 HMACSHA256 = new HMACSHA256 ();
			HMACSHA256.Key = secretKeyBArr;
			byte[] final = HMACSHA256.ComputeHash (contentBArr);
			//Base64実行。シグネチャ完成。
			result = System.Convert.ToBase64String (final);
			return result;
		}

		/// <summary>
		/// タイムスタンプの生成を行う
		/// </summary>
		private void _makeTimeStamp ()
		{
			//TimeStanp(世界協定時刻)の生成
			DateTime utcTime = DateTime.UtcNow;//追加
			string timestamp = utcTime.ToString ("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"); // 指定した書式で日付を文字列に変換する・ミリ秒まで取得。最後にZをつける
			_headerTimestamp = timestamp.Replace (":", "%3A"); //文字列の置換
		}

		/// <summary>
		/// セッショントークン有効稼働かの処理を行う
		/// </summary>
		internal void _checkInvalidSessionToken (string code)
		{
			if (NCMBException.INCORRECT_HEADER.Equals (code)) {
				if ((this._sessionToken != null) && (this._sessionToken.Equals (NCMBUser._getCurrentSessionToken ())))
					NCMBUser._logOutEvent ();
				NCMBDebug.Log ("CurrentUser is found, sessionToken info error, delete localdata");
			}
		}
	}
}
