using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using SimpleJSON;

public class MockServerObject {
    
    public string successJson = "";
    public string failJson = "";
    public string content = null;
    public int status = 404;
    public string method = "GET";
    public HttpListenerRequest request;

    public string GetResponseJson()
    {
        if (status == 200 || status == 201){
            return successJson;
        }
        return failJson; 
    }

    public void validate()
    {
        //Check method type
        if (!method.Equals(request.HttpMethod))
        {
            status = 404;
            return;
        }


        //check Header 
        for (int i = 0; i < request.Headers.Count; i++)
        {
            string key = request.Headers.GetKey(i);
            string[] headerValue = request.Headers.GetValues(i);
            return;

        }

        //Check Content 
        if(request.ContentEncoding != null){
            if(content == null || !request.ContentEncoding.Equals(content)){
                status = 404;
                return;
            }
        }

    }
}
