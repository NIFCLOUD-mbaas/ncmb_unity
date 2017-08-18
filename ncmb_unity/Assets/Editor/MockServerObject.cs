
using System.Net;
using System;
using System.IO;
using System.Reflection;

using YamlDotNet.RepresentationModel;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
public class MockServerObject {

    public string   method  = "GET";
    public string   body    = null;
    public string   query   = null;
    public string   header  = null;

    public string   url { get; set; }
    public int      status  = 404;

	public string   responseJson = "";

    public HttpListenerRequest request;

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
            Dictionary<string, string> headerDic = request.Headers.AllKeys.ToDictionary(t => t, t => request.Headers[t]);

            if(header != null){
				string pattern = @"\""(?<key>[^\""]+)\""\:\""?(?<value>[^\"",}]+)\""?\,?";
				foreach (Match m in Regex.Matches(header, pattern))
				{
					if (m.Success)
					{
                        if(headerDic.ContainsKey(m.Groups["key"].Value)){
                            if(!String.Equals(m.Groups["value"].Value, headerDic[m.Groups["key"].Value])){
                                status = 404;
                                return;
                            }
                        } else {
                            status = 404;
                            return;
                        }
					}
				}
            }
			
        }

    }
}