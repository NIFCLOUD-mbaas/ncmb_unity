using System.Collections.Generic;
using System.Net;
using System;
using System.IO;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using NCMB;

public class MockServer
{
	public static String SERVER = NCMBTestSettings.DOMAIN_URL + "/";
	private HttpListener listener = null;
	private static MockServer instance = null;
	//Dictionary to store all mock data
	Dictionary<string, List<MockServerObject>> mockObjectDic = new Dictionary<string, List<MockServerObject>> ();

	public MockServer ()
	{
		listener = new HttpListener ();
		//Add allowed prefixes for listener
		listener.Prefixes.Add (SERVER);
		listener.Prefixes.Add (SERVER + "2013-09-01/users/");
		listener.Prefixes.Add (SERVER + "2013-09-01/file/");
		listener.Prefixes.Add (SERVER + "2015-09-01/script/");
		listener.Prefixes.Add (SERVER + "2013-09-01/classes/");
		listener.Prefixes.Add (SERVER + "2013-09-01/installation/");
		listener.Start ();
		instance = this;
		//Call to listen request
		WaitForNewRequest ();
	}

	public static void startMock ()
	{
		if (instance == null) {
			instance = new MockServer ();
			instance.ReadMockData ();
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
		NCMBSettings._responseValidationFlag = false;
		MockServerObject mockObj = null;

		StreamReader stream = new StreamReader (request.InputStream);
		string bodyJson = stream.ReadToEnd ();
		if (request.HttpMethod.Equals ("GET") && request.Url.ToString ().Equals (SERVER)) {
			mockObj = new MockServerObject ();
			mockObj.status = 200;

		} else {
			foreach (MockServerObject mock in mockObjectDic[request.HttpMethod]) {
				if (request.Url.ToString ().Equals (mock.url)) {
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
		string yamlString = LoadFileData ("PlayModeTest/mbaas.yaml", false, null);
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
				return content.Replace ("\n", "");
			}
			return content;
		}

		return defaultString;
	}
}