using System.Collections.Generic;
using System.Net;
using System;
using System.IO;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using NCMB;
using System.Collections;
using System.Text;
using NCMB.Internal;
using System.Security.Cryptography;

public class MockServer
{
    // シグネチャメソッド　キー
    private static readonly string SIGNATURE_METHOD_KEY = "SignatureMethod";
    // シグネチャメソッド　値
    private static readonly string SIGNATURE_METHOD_VALUE = "HmacSHA256";
    // シグネチャバージョン　キー
    private static readonly string SIGNATURE_VERSION_KEY = "SignatureVersion";
    // シグネチャバージョン　値
    private static readonly string SIGNATURE_VERSION_VALUE = "2";
    // アプリケションキー　キー
    private static readonly string HEADER_APPLICATION_KEY = "X-NCMB-Application-Key";
    // タイムスタンプ　キー
    private static readonly string HEADER_TIMESTAMP_KEY = "X-NCMB-Timestamp";

	public static String SERVER = NCMBTestSettings.DOMAIN_URL + "/";
	private HttpListener listener = null;
	private static MockServer instance = null;
	private static string pathFileLoad = "";
	private static string DATA_PATH_DEFAULT = "PlayModeTest/mbaas.yaml";
	//Dictionary to store all mock data
	Dictionary<string, List<MockServerObject>> mockObjectDic = new Dictionary<string, List<MockServerObject>> ();
    Uri _domainUri = new Uri(NCMBTestSettings.DOMAIN_URL);

	public MockServer()
	{
		listener = new HttpListener();
		//Add allowed prefixes for listener
		listener.Prefixes.Add(SERVER);
		listener.Prefixes.Add(SERVER + "2013-09-01/login/");
		listener.Prefixes.Add(SERVER + "2013-09-01/users/");
		listener.Prefixes.Add(SERVER + "2013-09-01/file/");
		listener.Prefixes.Add(SERVER + "2015-09-01/script/");
		listener.Prefixes.Add(SERVER + "2013-09-01/classes/");
		listener.Prefixes.Add(SERVER + "2013-09-01/installation/");
		listener.Start();
		instance = this;
		//Call to listen request
		WaitForNewRequest();

        //Use to test signature
        NCMBSettings.ApplicationKey = "APP_KEY";
        NCMBSettings.ClientKey = "CLIENT_KEY";
	}

	public static void startMock (string filePath = null)
	{
		bool isNeedReadMockData = false;
		if (filePath == null) {
			filePath = DATA_PATH_DEFAULT;
		}
		if(!String.IsNullOrEmpty(pathFileLoad)) {
			if (!pathFileLoad.Equals(filePath)) {
				isNeedReadMockData = true;
			} else {
				isNeedReadMockData = false;
			}
		} else {
			isNeedReadMockData = true;
		}
		if (instance == null) {
			instance = new MockServer ();
		}
		//If the file path is changed, We will load data from the file.
		if(isNeedReadMockData){
			pathFileLoad = filePath;
			instance.ReadMockData();
        }
	}

	private void WaitForNewRequest ()
	{
		try {
			listener.BeginGetContext (ar => {
				HttpListener l = (HttpListener)ar.AsyncState;
				HttpListenerContext context = l.EndGetContext (ar);
				HttpListenerRequest request = context.Request;
				//Check request with mock data for response 
				checkAndResponse (request, context.Response);
				//Listen new request
				WaitForNewRequest ();
			}, listener);
		} catch (Exception e) {
			Debug.Log (e.Message);
		}
	}

	private void checkAndResponse (HttpListenerRequest request, HttpListenerResponse response)
	{
		NCMBSettings._responseValidationFlag = true;
		MockServerObject mockObj = null;

		StreamReader stream = new StreamReader (request.InputStream);
		string bodyJson = stream.ReadToEnd ();
		if (request.HttpMethod.Equals ("GET") && request.Url.ToString ().Equals (SERVER)) {
			mockObj = new MockServerObject ();
			mockObj.status = 200;

		} else {
			String decodeUrl = WWW.UnEscapeURL(request.Url.ToString());
			foreach (MockServerObject mock in mockObjectDic[request.HttpMethod]) {
				if (decodeUrl.Equals (mock.url)) {
					if (bodyJson.Length > 0) {
						if (bodyJson.Equals (mock.body) || request.ContentType.Equals ("multipart/form-data; boundary=_NCMBBoundary")) {
							mockObj = mock;
							mockObj.request = request;
							mockObj.validate ();
							if (mockObj.status == 200 || mockObj.status == 201) {
								break;
							}
						}
					} else {
						mockObj = mock;
						mockObj.request = request;
						mockObj.validate ();
						if (mockObj.status == 200 || mockObj.status == 201) {
							break;
						}
					}
				}
			}
		}

		if (mockObj == null) {
			mockObj = new MockServerObject ();
		}

        //Set response signature
        if (mockObj.responseSignature)
        {
            string signature = _makeResponseSignature(request, mockObj.responseJson);
            response.AddHeader("X-NCMB-Response-Signature", signature);
        }
        //Set status code
		response.StatusCode = mockObj.status;
		byte[] buffer = System.Text.Encoding.UTF8.GetBytes (mockObj.responseJson);
		// Get a response stream and write the response to it.
		response.ContentLength64 = buffer.Length;
		System.IO.Stream output = response.OutputStream;
		output.Write (buffer, 0, buffer.Length);
		// You must close the output stream.
		response.Close ();
	}

	private void ReadMockData ()
	{
		
		//Read mock data from yaml and json files 
		mockObjectDic.Clear ();
		mockObjectDic.Add ("GET", new List<MockServerObject> ());
		mockObjectDic.Add ("POST", new List<MockServerObject> ());
		mockObjectDic.Add ("PUT", new List<MockServerObject> ());
		mockObjectDic.Add ("DELETE", new List<MockServerObject> ());
		//Get yaml string 
		string yamlString = LoadFileData(pathFileLoad, false, null);
		// Setup the input
		var input = new StringReader (yamlString);
		// Load the stream
		var yaml = new YamlStream ();
		yaml.Load (input);
		int docCount = yaml.Documents.Count;

		for (int i = 0; i < docCount; i++) {
			var mapping = (YamlMappingNode)yaml.Documents [i].RootNode;

			MockServerObject mock = new MockServerObject ();
			var request = (YamlMappingNode)mapping.Children [new YamlScalarNode ("request")];
			var response = (YamlMappingNode)mapping.Children [new YamlScalarNode ("response")];
			YamlScalarNode method = (YamlScalarNode)request.Children [new YamlScalarNode ("method")];
			mock.method = method.Value;

			if (request.Children.Keys.Contains (new YamlScalarNode ("body"))) {
				var body = request.Children [new YamlScalarNode ("body")];
				mock.body = body.ToJson ().Replace ("\"[", "[").Replace ("]\"", "]");
			}

			if (request.Children.Keys.Contains (new YamlScalarNode ("query"))) {
				var query = request.Children [new YamlScalarNode ("query")];
				String queryString = "";

				if (query is YamlMappingNode) {
					YamlMappingNode queryMapNode = (YamlMappingNode)query;
					foreach (var item in queryMapNode.Children) {
						if (queryString.Length > 0) {
							queryString += "&";
						}
						queryString += item.Key + "=" + (item.Value is YamlMappingNode ? item.Value.ToJson () : item.Value);
					}
				}
				mock.query = queryString;
			}

			if (request.Children.Keys.Contains (new YamlScalarNode ("header"))) {
				var header = request.Children [new YamlScalarNode ("header")];
				mock.header = header.ToJson ();
			}

			YamlScalarNode url = (YamlScalarNode)request.Children [new YamlScalarNode ("url")];
			if (mock.query != null && mock.query.Length > 0) {
				mock.url = MockServer.SERVER + url.Value + "?" + mock.query;
			} else {
				mock.url = MockServer.SERVER + url.Value;
			}


            if (response.Children.Keys.Contains(new YamlScalarNode("X-NCMB-Response-Signature")))
            {
                mock.responseSignature = true;
            }

			YamlScalarNode status = (YamlScalarNode)response.Children [new YamlScalarNode ("status")];
			mock.status = Convert.ToInt32 (status.Value);

			YamlScalarNode file = (YamlScalarNode)response.Children [new YamlScalarNode ("file")];
			mock.responseJson = LoadFileData ("PlayModeTest" + file.Value, true, "");
			mockObjectDic [mock.method].Add (mock);
		}
	}

	private string LoadFileData (String path, bool removeBreakline, String defaultString)
	{
		string filePath = Path.Combine (Application.dataPath, path);
		if (File.Exists (filePath)) {
			// Read from file into a string
			string content = File.ReadAllText (filePath);
			if (removeBreakline) {
				return content.Replace ("\n", "").Replace("\r","");
			}
			return content;
		}

		return defaultString;
	}

        private string _makeResponseSignature(HttpListenerRequest request, string responseData)
        {
            string _url = request.Url.AbsoluteUri;
            ConnectType _method = ConnectType.GET;
            string _applicationKey = NCMBSettings.ApplicationKey;
            string _headerTimestamp = request.Headers["X-NCMB-Timestamp"];
            byte[] responseByte = System.Text.Encoding.UTF8.GetBytes(responseData);
            
            StringBuilder data = new StringBuilder(); //シグネチャ（ハッシュ化）するデータの生成
            String path = _url.Substring(this._domainUri.OriginalString.Length); // パス以降の設定,取得
            String[] temp = path.Split('?');
            path = temp[0];
            String parameter = null;
            if (temp.Length > 1)
            {
                parameter = temp[1];
            }
            Hashtable hashValue = new Hashtable(); //昇順に必要なデータを格納するリスト
            hashValue[SIGNATURE_METHOD_KEY] = SIGNATURE_METHOD_VALUE;//シグネチャキー
            hashValue[SIGNATURE_VERSION_KEY] = SIGNATURE_VERSION_VALUE; // シグネチャバージョン
            hashValue[HEADER_APPLICATION_KEY] = _applicationKey;
            hashValue[HEADER_TIMESTAMP_KEY] = _headerTimestamp;
            String[] tempParameter;
            if (parameter != null)
            {
                if (_method == ConnectType.GET)
                {
                    foreach (string param in parameter.Split('&'))
                    {
                        tempParameter = param.Split('=');
                        hashValue[tempParameter[0]] = tempParameter[1];
                    }
                }
            }
            //sort hashTable base on key
            List<string> tmpAscendingList = new List<string>(); //昇順に必要なデータを格納するリスト
            foreach (DictionaryEntry s in hashValue)
            {
                tmpAscendingList.Add(s.Key.ToString());
            }
            StringComparer cmp = StringComparer.Ordinal;
            tmpAscendingList.Sort(cmp);
            //Create data
            data.Append(_method); //メソッド追加
            data.Append("\n");
            data.Append(this._domainUri.Host); //ドメインの追加
            data.Append("\n");
            data.Append(path); //パスの追加
            data.Append("\n");
            foreach (string tmp in tmpAscendingList)
            {
                data.Append(tmp + "=" + hashValue[tmp] + "&");
            }
            data.Remove(data.Length - 1, 1); //最後の&を削除


            StringBuilder stringHashData = data;

            //レスポンスデータ追加 Delete時はレスポンスデータが無いため追加しない
            if (responseByte != null && responseData != "")
            {
                // 通常時
                stringHashData.Append("\n" + responseData);
            }
            else if (responseByte != null)
            {
                // ファイル取得時など
                stringHashData.Append("\n" + NCMBConnection.AsHex(responseByte));
            }

            //シグネチャ再生成
            String _clientKey = NCMBSettings.ClientKey;
            String stringData = stringHashData.ToString();
            //署名(シグネチャ)生成
            string result = null; //シグネチャ結果の収納
            byte[] secretKeyBArr = Encoding.UTF8.GetBytes(_clientKey); //秘密鍵(クライアントキー)
            byte[] contentBArr = Encoding.UTF8.GetBytes(stringData); //認証データ
                                                                     //秘密鍵と認証データより署名作成
            HMACSHA256 HMACSHA256 = new HMACSHA256();
            HMACSHA256.Key = secretKeyBArr;
            byte[] final = HMACSHA256.ComputeHash(contentBArr);
            //Base64実行。シグネチャ完成。
            result = System.Convert.ToBase64String(final);

            return result;
        }
}