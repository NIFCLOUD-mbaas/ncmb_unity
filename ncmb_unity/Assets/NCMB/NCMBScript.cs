using NCMB.Internal;
using MiniJSON;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace NCMB
{
	public class NCMBScript
	{
		private static readonly string SERVICE_PATH = "script";
		private static readonly string DEFAULT_SCRIPT_DOMAIN_URL = "https://logic.mb.cloud.nifty.com";
		private static readonly string DEFAULT_SCRIPT_API_VERSION = "2015-09-01";
		private string _scriptName;
		private MethodType _method;
		private string _baseUrl;

		delegate void AsyncDelegate ();

		/// <summary>
		/// メソッドタイプ。
		/// </summary>
		public enum MethodType
		{
			POST,
			PUT,
			GET,
			DELETE
		}

		/// <summary>
		/// スクリプト名の取得、または設定を行います。
		/// </summary>
		/// <value>The name of the script.</value>
		public string ScriptName {
			get { return this._scriptName; }
			set { this._scriptName = value; }
		}

		/// <summary>
		/// メソッドタイプの取得、または設定を行います。
		/// </summary>
		/// <value>.メソッドタイプ</value>
		public MethodType Method {
			get { return this._method; }
			set { this._method = value; }
		}

		/// <summary>
		/// エンドポイントの取得、または設定を行います。
		/// </summary>
		/// <value>.メソッドタイプ</value>
		public string BaseUrl {
			get { return this._baseUrl; }
			set { this._baseUrl = value; }
		}

		/// <summary>
		/// コンストラクター。
		/// </summary>
		/// <param name="scriptName">スクリプト名</param>
		/// <param name="method">HTTPメソッド</param>
		public NCMBScript (string scriptName, MethodType method)
			: this (scriptName, method, null)
		{
		}

		/// <summary>
		/// コンストラクター。
		/// </summary>
		/// <param name="scriptName">スクリプト名</param>
		/// <param name="method">HTTPメソッド</param>
		/// <param name="baseUrl">エンドポイント</param>
		public NCMBScript (string scriptName, MethodType method, string baseUrl)
		{
			_scriptName = scriptName;
			_method = method;
			_baseUrl = baseUrl;
		}

		public void ExecuteAsync (IDictionary<string, object> header, IDictionary<string, object> body, IDictionary<string, object> query, NCMBExecuteCallback callback)
		{
			new AsyncDelegate (delegate {
				//URL作成
				String scriptUrl;
				String domain;
				if (this._baseUrl != null && this._baseUrl.Length > 0) {
					domain = _baseUrl;
					scriptUrl = this._baseUrl + "/" + this._scriptName;
				} else {
					domain = DEFAULT_SCRIPT_DOMAIN_URL;
					scriptUrl = DEFAULT_SCRIPT_DOMAIN_URL + "/" + DEFAULT_SCRIPT_API_VERSION + "/" + SERVICE_PATH + "/" + this._scriptName;
				}

				//メソッド作成
				ConnectType type;
				switch (_method) {
				case MethodType.POST:
					type = ConnectType.POST;
					break;
				case MethodType.PUT:
					type = ConnectType.PUT;
					break;
				case MethodType.GET:
					type = ConnectType.GET;
					break;
				case MethodType.DELETE:
					type = ConnectType.DELETE;
					break;
				default:
					throw new ArgumentException ("Invalid methodType.");
				}

				//コンテント作成
				String content = null;
				if (body != null) {
					content = Json.Serialize (body);
				}

				//クエリ文字列作成
				String queryString = "?";
				if (query != null && query.Count > 0) {
					int count = 0;
					foreach (KeyValuePair<string, object> pair in query) {
						queryString += pair.Key + "=" + pair.Value.ToString ();
						if (count != 0) {
							queryString += "&";
						}
					}
					scriptUrl += Uri.EscapeUriString (queryString);
				}

				ServicePointManager.ServerCertificateValidationCallback = delegate {
					return true;
				}; 

				//コネクション作成
				NCMBConnection connection = new NCMBConnection (scriptUrl, type, content, NCMBUser._getCurrentSessionToken (), domain);
				HttpWebRequest request = connection._returnRequest ();

				//コンテント設定
				if (content != null) {
					byte[] postDataBytes = Encoding.Default.GetBytes (content); 
					Stream stream = null;
					try {
						stream = request.GetRequestStream ();
						stream.Write (postDataBytes, 0, postDataBytes.Length);
					} finally {
						if (stream != null) {
							stream.Close ();
						}
					}
				}

				//オリジナルヘッダー設定
				if (header != null && header.Count > 0) {
					foreach (KeyValuePair<string, object> pair in header) {
						request.Headers.Add (pair.Key, pair.Value.ToString ());
					}
				}

				//通信
				Connect (connection, request, callback);

			}).BeginInvoke ((IAsyncResult r) => {
			}, null);
		}

		//通信
		internal void Connect (NCMBConnection connection, HttpWebRequest request, NCMBExecuteCallback callback)
		{
			string responseData = null;
			NCMBException error = null;
			HttpWebResponse httpResponse = null;
			Stream streamResponse = null;
			StreamReader streamRead = null;
			byte[] result = new byte[32768];
			try {
				//レスポンスデータの書き込み
				httpResponse = (HttpWebResponse)request.GetResponse ();
				streamResponse = httpResponse.GetResponseStream ();
				for (; ;) {
					int readSize = streamResponse.Read (result, 0, result.Length);
					if (readSize == 0) {
						break;
					}
				}
			} catch (WebException ex) {
				//失敗
				using (WebResponse webResponse = ex.Response) {//WebExceptionからWebResponseを取得
					error = new NCMBException ();
					error.ErrorMessage = ex.Message;
					if (webResponse != null) {
						streamResponse = webResponse.GetResponseStream ();
						streamRead = new StreamReader (streamResponse);
						responseData = streamRead.ReadToEnd ();//データを全てstringに書き出し

						error.ErrorMessage = responseData;
						httpResponse = (HttpWebResponse)webResponse;
						error.ErrorCode = httpResponse.StatusCode.ToString ();

						var jsonData = MiniJSON.Json.Deserialize (responseData) as Dictionary<string,object>;//Dictionaryに変換
						if (jsonData != null) {
							var hashtableData = new Hashtable (jsonData);
							//statusCode
							if (hashtableData.ContainsKey ("code")) {
								error.ErrorCode = (hashtableData ["code"].ToString ());
							} else if (hashtableData.ContainsKey ("status")) {
								error.ErrorCode = (hashtableData ["status"].ToString ());
							}
							//message
							if (hashtableData.ContainsKey ("error")) {
								error.ErrorMessage = (hashtableData ["error"].ToString ());
							}
						}
					}
				}
			} finally {
				//close
				if (httpResponse != null) {
					httpResponse.Close ();
				}
				if (streamResponse != null) {
					streamResponse.Close ();
				}
				if (streamRead != null) {
					streamRead.Close ();
				}
				//check E401001 error
				if (error != null) {
					connection._checkInvalidSessionToken (error.ErrorCode);
				}
				//enqueue callback
				if (callback != null) {
					Platform.RunOnMainThread (delegate {
						callback (result, error);
					});
				}
			}
		}
	}
}
