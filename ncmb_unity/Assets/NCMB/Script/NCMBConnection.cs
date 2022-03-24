/*******
 Copyright 2017-2022 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.

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
using MiniJSON;
using MimeTypes;
using UnityEngine;
using UnityEngine.Networking;

using System.Linq;

//Dictionary
using System.IO;

//strem
using System.Security.Cryptography;
using NCMB.Internal;

using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo ("Assembly-CSharp-Editor")]
namespace NCMB.Internal
{
	public class NCMBConnection
	{
		// レスポンスシグネチャ　キー
		private static readonly string RESPONSE_SIGNATURE = "X-NCMB-Response-Signature";
		// シグネチャメソッド　キー
		private static readonly string SIGNATURE_METHOD_KEY = "SignatureMethod";
		// シグネチャメソッド　値
		private static readonly string SIGNATURE_METHOD_VALUE = "HmacSHA256";
		// シグネチャバージョン　キー
		private static readonly string SIGNATURE_VERSION_KEY = "SignatureVersion";
		// シグネチャバージョン　値
		private static readonly string SIGNATURE_VERSION_VALUE = "2";
		// シグネチャヘッダー　キー
		private static readonly string HEADER_SIGNATURE = "X-NCMB-Signature";
		// アプリケションキー　キー
		private static readonly string HEADER_APPLICATION_KEY = "X-NCMB-Application-Key";
		// タイムスタンプ　キー
		private static readonly string HEADER_TIMESTAMP_KEY = "X-NCMB-Timestamp";
		// セッショントークン
		private static readonly string HEADER_SESSION_TOKEN = "X-NCMB-Apps-Session-Token";
		// UserAgent キー
		private static readonly string HEADER_USER_AGENT_KEY = "X-NCMB-SDK-Version";
		// UserAgent 値
		private static readonly string HEADER_USER_AGENT_VALUE = "unity-" + CommonConstant.SDK_VERSION;

		// time out 10 sec
		private static readonly int REQUEST_TIME_OUT = 10;

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
		private NCMBFile _file = null;
		internal UnityWebRequest _request = null;


		//コンストラクタ(通常)
		internal NCMBConnection (String url, ConnectType method, string content, string sessionToken)
			: this (url, method, content, sessionToken, null, NCMBSettings.DomainURL)
		{
		}

		//コンストラクタ(NCMBFile)
		internal NCMBConnection (String url, ConnectType method, string content, string sessionToken, NCMBFile file)
			: this (url, method, content, sessionToken, file, NCMBSettings.DomainURL)
		{
		}

		//コンストラクタ
		internal NCMBConnection (String url, ConnectType method, string content, string sessionToken, NCMBFile file, string domain)
		{
			this._method = method;
			this._content = content;
			this._url = url;
			this._sessionToken = sessionToken;
			this._applicationKey = NCMBSettings.ApplicationKey;
			this._clientKey = NCMBSettings.ClientKey;
			this._domainUri = new Uri (domain);
			this._file = file;
			this._request = _returnRequest ();

		}

		// 通信処理(File_GET)
		internal void Connect (HttpClientFileDataCallback callback)
		{
			_Connection (callback);
		}

		// 通信処理(通常)
		internal void Connect (HttpClientCallback callback)
		{
			_Connection (callback);
		}



		private void _Connection (object callback)
		{
			GameObject gameObj = GameObject.Find ("NCMBSettings");
			NCMBSettings settings = gameObj.GetComponent<NCMBSettings> ();
			settings.Connection (this, callback);
		}

		private void _signatureCheck (string responseSignature, string statusCode, string responseData, byte[] responseByte, ref NCMBException error)
		{
			//hashデータ作成
			StringBuilder stringHashData = _makeSignatureHashData ();

			//レスポンスデータ追加 Delete時はレスポンスデータが無いため追加しない
			if (responseByte.Any () && responseData != "") {
				// 通常時
				stringHashData.Append ("\n" + responseData);
			} else if (responseByte.Any ()) {
				// ファイル取得時など
				stringHashData.Append ("\n" + AsHex (responseByte));
			}

			//シグネチャ再生成
			string responseMakeSignature = _makeSignature (stringHashData.ToString ());

			//レスポンスシグネチャと生成したシグネチャが違う場合はエラー
			if (responseSignature != responseMakeSignature) {
				statusCode = "100";
				responseData = "{}";
				error = new NCMBException ();
				error.ErrorCode = "E100001";
				error.ErrorMessage = "Authentication error by response signature incorrect.";
			}
			NCMBDebug.Log ("【responseSignature】　" + responseSignature);
			NCMBDebug.Log ("【responseMakeSignature】　" + responseMakeSignature);
		}

		// バイナリデータを16進数文字列に変換
		public static string AsHex (byte[] bytes)
		{
			StringBuilder sb = new StringBuilder (bytes.Length * 2);
			foreach (byte b in bytes) {
				if (b < 16) {
					sb.Append ('0');
				}
				sb.Append (Convert.ToString (b, 16));
			}
			return sb.ToString ();
		}

		// ファイルデータ設定
		private UnityWebRequest _setUploadHandlerForFile (UnityWebRequest req)
		{
			string newLine = "\r\n";
			string boundary = "_NCMBBoundary";
			string formData = "--" + boundary + newLine;
			byte[] endBoundary = Encoding.Default.GetBytes (newLine + "--" + boundary + "--");

			formData += "Content-Disposition: form-data; name=\"file\"; filename=" + Uri.EscapeUriString (_file.FileName) + newLine;
			formData += "Content-Type: " + MimeTypeMap.GetMimeType (System.IO.Path.GetExtension (_file.FileName)) + newLine + newLine;
			byte[] fileFormData = Encoding.Default.GetBytes (formData);

			// BodyData連結
			if (_file.FileData != null) {
				fileFormData = Enumerable.Concat (fileFormData, _file.FileData).ToArray ();
			}

			// ACL更新処理
			if (_file.ACL != null && _file.ACL._toJSONObject ().Count > 0) {
				string aclString = Json.Serialize (_file.ACL._toJSONObject ());
				formData = newLine + "--" + boundary + newLine;
				formData += "Content-Disposition: form-data; name=acl; filename=acl" + newLine + newLine;
				formData += aclString;
				byte[] aclFormData = Encoding.Default.GetBytes (formData);
				fileFormData = Enumerable.Concat (fileFormData, aclFormData).ToArray ();
			}

			fileFormData = Enumerable.Concat (fileFormData, endBoundary).ToArray ();
			req.uploadHandler = (UploadHandler)new UploadHandlerRaw (fileFormData);

			return req;
		}

		/// <summary>
		/// リクエストの生成を行う
		/// </summary>
		internal UnityWebRequest _returnRequest ()
		{
			//URLをエンコード
			var uri = new Uri (_url);
			_url = uri.AbsoluteUri;

			//メソッド追加
			String method = "";
			switch (_method) {
			case ConnectType.POST:
				method = "POST";
				break;
			case ConnectType.PUT:
				method = "PUT";
				break;
			case ConnectType.GET:
				method = "GET";
				break;
			case ConnectType.DELETE:
				method = "DELETE";
				break;
			}

			UnityWebRequest req = new UnityWebRequest (_url, method);
			_makeTimeStamp (); //タイムスタンプの生成
			StringBuilder stringHashData = _makeSignatureHashData ();
			string result = _makeSignature (stringHashData.ToString ()); //シグネチャ生成

			// ContentType設定
			if (req.method.Equals ("POST") && _file != null || req.method.Equals ("PUT") && _file != null) {
				req.SetRequestHeader ("Content-Type", "multipart/form-data; boundary=_NCMBBoundary");
			} else {
				req.SetRequestHeader ("Content-Type", "application/json");
			}

			//ヘッダー設定
			req.SetRequestHeader (HEADER_APPLICATION_KEY, _applicationKey);
			req.SetRequestHeader (HEADER_SIGNATURE, result);
			req.SetRequestHeader (HEADER_TIMESTAMP_KEY, _headerTimestamp);
			req.SetRequestHeader (HEADER_USER_AGENT_KEY, HEADER_USER_AGENT_VALUE);
			if ((_sessionToken != null) && (_sessionToken != "")) {
				req.SetRequestHeader (HEADER_SESSION_TOKEN, _sessionToken);
				NCMBDebug.Log ("Session token :" + _sessionToken);
			}
			//req.SetRequestHeader (HEADER_ACCESS_CONTROL_ALLOW_ORIGIN, "*");
			if (req.GetRequestHeader ("Content-Type").Equals ("multipart/form-data; boundary=_NCMBBoundary")) {
				_setUploadHandlerForFile (req);
			} else if ((req.method.Equals ("POST") || req.method.Equals ("PUT")) && _content != null) {
				byte[] bodyRaw = Encoding.UTF8.GetBytes (_content);
				req.uploadHandler = (UploadHandler)new UploadHandlerRaw (bodyRaw);
			}
			req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer ();
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
				if (_method == ConnectType.GET) {
					foreach (string param in parameter.Split('&')) {
						tempParameter = param.Split ('=');
						hashValue [tempParameter [0]] = tempParameter [1];
					}
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

		/// <summary>
		/// セッショントークン有効稼働かの処理を行う
		/// </summary>
		internal void _checkResponseSignature (string code, string responseData, UnityWebRequest req, ref NCMBException error)
		{
			//レスポンスシグネチャのチェック
			if (NCMBSettings._responseValidationFlag && req.error == null && error == null && req.GetResponseHeader (RESPONSE_SIGNATURE) != null) {
				string responseSignature = req.GetResponseHeader (RESPONSE_SIGNATURE).ToString ();
				//データに絵文字があればUnicodeアンエスケープし、レスポンスシグネチャ計算用に対応する
				//一般のエスケープ表記データ(ダブルクォーテーション..)はこの処理をしないのが正しいです
                var unescapeResponseData = responseData;

                if(unescapeResponseData != null ){
                    unescapeResponseData = NCMBUtility.unicodeUnescape(unescapeResponseData);
                }
				_signatureCheck (responseSignature, code, unescapeResponseData, req.downloadHandler.data, ref error);
			}
		}

		internal static IEnumerator SendRequest (NCMBConnection connection, UnityWebRequest req, object callback)
		{
			NCMBException error = null;
			byte[] byteData = new byte[32768];
			string json = "";
			string responseCode = "";

			// 通信実行
			// yield return req.Send ();

			// 通信実行
			#if UNITY_2017_2_OR_NEWER
			req.SendWebRequest ();
			#else
			req.Send ();
			#endif

			// タイムアウト処理
			float elapsedTime = 0.0f;
			float waitTime = 0.2f;
			while (!req.isDone) {
				//elapsedTime += Time.deltaTime;
				elapsedTime += waitTime;
				if (elapsedTime >= REQUEST_TIME_OUT) {
					req.Abort ();
					error = new NCMBException ();
					break;
				}
				//yield return new WaitForEndOfFrame ();
				yield return new WaitForSecondsRealtime (waitTime);
			}

			// 通信結果判定
			if (error != null) {
				// タイムアウト
				error.ErrorCode = "408";
				error.ErrorMessage = "Request Timeout.";
			#if UNITY_2020_2_OR_NEWER
			} else if (req.result == UnityWebRequest.Result.ConnectionError) {
			#else
				#if UNITY_2017_1_OR_NEWER
			} else if (req.isNetworkError) {
				#else
			} else if (req.isError) {
				#endif
			#endif
				// 通信エラー
				error = new NCMBException ();
				error.ErrorCode = req.responseCode.ToString ();
				error.ErrorMessage = req.error;
			} else if (req.responseCode != 200 && req.responseCode != 201) {
				// mBaaSエラー
				error = new NCMBException ();
				var jsonData = MiniJSON.Json.Deserialize (req.downloadHandler.text) as Dictionary<string,object>;
				error.ErrorCode = jsonData ["code"].ToString ();
				error.ErrorMessage = jsonData ["error"].ToString ();
			} else {
				// 通信成功
				byteData = req.downloadHandler.data;
				json = req.downloadHandler.text;
			}

			//check E401001 error
			if (error != null) {
				connection._checkInvalidSessionToken (error.ErrorCode);
			}

			// check response signature
			if (callback != null && !(callback is NCMBExecuteScriptCallback)) {
				// スクリプト機能はレスポンスシグネチャ検証外
				responseCode = req.responseCode.ToString ();
				string responsText = req.downloadHandler.text;
				if (callback is HttpClientFileDataCallback) {
					// NCMBFileのGETではbyteでシグネチャ計算を行うよう空文字にする
					responsText = "";
				}
				connection._checkResponseSignature (responseCode, responsText, req, ref error);
			}

			if (callback != null) {
				if (callback is NCMBExecuteScriptCallback) {
					((NCMBExecuteScriptCallback)callback) (byteData, error);
				} else if (callback is HttpClientCallback) {
					((HttpClientCallback)callback) ((int)req.responseCode, json, error);
				} else if (callback is HttpClientFileDataCallback) {
					((HttpClientFileDataCallback)callback) ((int)req.responseCode, byteData, error);
				}
			}

		}
	}
}
