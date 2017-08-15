using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
using System.Diagnostics;
using System.Linq;

public class MockServer
{
    
    private static MockServer instance = null;
    private HttpListener listener = null;
    public event EventHandler CommandReceived;

    public static String SERVER = "http://localhost:3000/";

    Dictionary<string, MockServerObject> jsonDictionary;

    public MockServer()
    {
		listener = new HttpListener();
		listener.Prefixes.Add(SERVER);
        listener.Prefixes.Add(SERVER + "2013-09-01/users/");
        listener.Prefixes.Add(SERVER + "2013-09-01/file/");
		listener.Prefixes.Add(SERVER + "2015-09-01/script/");
        listener.Prefixes.Add(SERVER + "2013-09-01/classes/");
        listener.Prefixes.Add(SERVER + "2013-09-01/installation/");
		listener.Start();

        instance = this;
        jsonDictionary = new Dictionary<string, MockServerObject>();

        StartListen();
        InitJson();
    }

    public static void startMock(){
        if(instance == null){
            instance = new MockServer();
        }
    }

    public void StartListen()
    {
        try
        {
            listener.BeginGetContext(ar =>
            {
                HttpListener l = (HttpListener)ar.AsyncState;
                HttpListenerContext context = l.EndGetContext(ar);
                HttpListenerRequest request = context.Request;
                checkAndResponse(request, context.Response);
                StartListen();
            }, listener);
        }
        catch (Exception e)
        {

        }

    }

    public void StopListen()
    {
        listener.Stop();
    }

    private void checkAndResponse(HttpListenerRequest request, HttpListenerResponse response)
    {
        response.Headers.Clear();
        response.SendChunked = false;
        response.Headers.Add("Server", String.Empty);
        response.Headers.Add("Date", String.Empty);
        // Construct a response.
        MockServerObject mockObj = null;

        //Test Connection
        if(request.HttpMethod.Equals("GET") && request.Url.ToString().Equals(SERVER)){
            mockObj = new MockServerObject();
            mockObj.status = 200;
            
        } else {
			foreach (var element in jsonDictionary)
			{
                if (String.Equals(element.Key, request.Url.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					mockObj = element.Value;
					mockObj.request = request;
					mockObj.validate();
					break;
				}
			}
        }

        if (mockObj == null){
            mockObj = new MockServerObject();
        }

        response.StatusCode = mockObj.status;
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(mockObj.GetResponseJson());
		// Get a response stream and write the response to it.
		response.ContentLength64 = buffer.Length;
		System.IO.Stream output =  response.OutputStream;
		output.Write(buffer, 0, buffer.Length);
		// You must close the output stream.
		response.Close();
    }

    public void InitJson(){
        
		//** TEST testScript_GET COUNT **//
		MockServerObject mock = new MockServerObject();
        mock.successJson = "{\"count:2\"}";
        mock.failJson = "{\"count:0\"}";
        mock.method = "GET";
        mock.status = 200;
        jsonDictionary.Add(SERVER + "2015-09-01/script/testScript_GET.js?name=tarou&message=hello", mock);

        //** FILE ACL TEST **/
        mock = new MockServerObject();
		mock.successJson = "{\"results\":[{\"fileName\":\"ACL.txt\",\"mimeType\":\"text/plain\",\"fileSize\":8,\"createDate\":\"2017-07-19T02:27:55.867Z\",\"updateDate\":\"2017-07-19T02:27:55.867Z\",\"acl\":{\"*\":{\"read\":true}}}]}";
		mock.failJson = "{\"code\":\"E403001\",\"error\":\"No access with ACL.\"}";
		mock.method = "POST";
		mock.status = 200;
        mock.content = "{\"fileName\":\"ACL.txt\",\"fileData\":[97,99,108,32,116,101,115,116],\"acl\":{\"*\":{\"read\":true}}}";
		jsonDictionary.Add(SERVER + "2013-09-01/files/ACL.txt", mock);

        mock = new MockServerObject();
		mock.successJson = "{\"results\":[{\"fileName\":\"ACL.txt\",\"mimeType\":\"text/plain\",\"fileSize\":8,\"createDate\":\"2017-07-19T02:27:55.867Z\",\"updateDate\":\"2017-07-19T02:27:55.867Z\",\"acl\":{\"*\":{\"read\":true}}}]}";
		mock.failJson = "{\"code\":\"E403001\",\"error\":\"No access with ACL.\"}";
		mock.method = "GET";
		mock.status = 200;
		jsonDictionary.Add(SERVER + "2013-09-01/files?where={\"fileName\":\"ACL.txt\"}", mock);

        // LINK DATA ASYNC
		mock = new MockServerObject();
		mock.successJson = "{\"createDate\":\"2017-01-01T00:00:00.000Z\",\"objectId\":\"dummyObjectId\",\"userName\":\"Nifty Tarou\",\"authData\":{\"twitter\":{\"id\":\"twitterDummyId\",\"screen_name\":\"twitterDummyScreenName\",\"oauth_consumer_key\":\"twitterDummyConsumerKey\",\"consumer_secret\":\"twitterDummyConsumerSecret\",\"oauth_token\":\"twitterDummyAuthToken\",\"oauth_token_secret\":\"twitterDummyAuthSecret\"}},\"sessionToken\":\"dummySessionToken\"}";
		mock.failJson = "{\"code\":\"E403001\",\"error\":\"No access with ACL.\"}";
		mock.method = "POST";
		mock.status = 201;
        mock.content = "{\"authData\":{\"twitter\":{\"id\":\"twitterDummyId\",\"screen_name\":\"twitterDummyScreenName\",\"oauth_consumer_key\":\"twitterDummyConsumerKey\",\"consumer_secret\":\"twitterDummyConsumerSecret\",\"oauth_token\":\"twitterDummyAuthToken\",\"oauth_token_secret\":\"twitterDummyAuthSecret\"}}}";
		jsonDictionary.Add(SERVER + "2013-09-01/users", mock);

		mock = new MockServerObject();
		mock.successJson = "{\"updateDate\":\"2017-02-04T11:28:30.348Z\"}";
		mock.failJson = "{\"code\":\"E403001\",\"error\":\"No access with ACL.\"}";
		mock.method = "PUT";
		mock.status = 200;
        mock.content = "{\"authData\":{\"facebook\":{\"id\":\"facebookDummyId\",\"access_token\":\"facebookDummyAccessToken\",\"expiration_date\":{\"__type\":\"Date\",\"iso\":\"2017-02-07T01:02:03.004Z\"}}}}";
		jsonDictionary.Add(SERVER + "2013-09-01/users/dummyObjectId", mock);
	}


}