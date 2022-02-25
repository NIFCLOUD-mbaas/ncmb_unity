#if UNITY_ANDROID && UNITY_2017_1_OR_NEWER

using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System.IO;
using SimpleJSON;

class PreBuildAndroid : IPreprocessBuild
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        string androidPath = Application.dataPath + "/Plugins/Android";
        string gsJsonPath = androidPath + "/google-services.json";

        if (!File.Exists(gsJsonPath))
        {
            Debug.LogWarning("Can not found google-services.json, please copy your google-services.json to " + androidPath);
            return;
        }
        string gsJson = File.ReadAllText(gsJsonPath);
        //Covert google json to xml 
        string gsXml = parseGoogleJsonToXml(gsJson);

        string valuesFolder = androidPath + "/res/values";
        if (!Directory.Exists(valuesFolder))
        {
            Directory.CreateDirectory(valuesFolder);
        }

        File.WriteAllText(valuesFolder + "/google-services.xml", gsXml);

    }

    private string parseGoogleJsonToXml(string jsonString)
    {

        int OAUTH_CLIENT_TYPE_WEB = 3;

        string gsxml = "<?xml version='1.0' encoding='utf-8'?>\n";
        gsxml += "<resources tools:keep=\"@string/firebase_database_url,@string/gcm_defaultSenderId,@string/google_storage_bucket,@string/project_id,@string/google_api_key,@string/google_crash_reporting_api_key,@string/google_app_id,@string/default_web_client_id\" xmlns:tools=\"http://schemas.android.com/tools\">\n";

        var root = JSON.Parse(jsonString);
        var project = root["project_info"];
        var client = root["client"][0];

        //Project- project_info
        var firebase_database_url = project["firebase_url"];
        gsxml += genString("firebase_database_url", firebase_database_url);

        var gcm_defaultSenderId = project["project_number"];
        gsxml += genString("gcm_defaultSenderId", gcm_defaultSenderId);

        var google_storage_bucket = project["storage_bucket"];
        gsxml += genString("google_storage_bucket", google_storage_bucket);

        var project_id = project["project_id"];
        gsxml += genString("project_id", project_id);

        //Client - api_key
        var google_api_key = client["api_key"][0]["current_key"];
        gsxml += genString("google_api_key", google_api_key);

        var google_crash_reporting_api_key = client["api_key"][0]["current_key"];
        gsxml += genString("google_crash_reporting_api_key", google_crash_reporting_api_key);

        //Client - client_info
        var google_app_id = client["client_info"]["mobilesdk_app_id"];
        gsxml += genString("google_app_id", google_app_id);


        //Client - oauth_client

        foreach (JSONNode oauthNode in client["oauth_client"])
        {
            var type = oauthNode["client_type"];
            int abcd = oauthNode.AsInt;
        }

        for (int i = 0; i < client["oauth_client"].Count; i++)
        {
            if (client["oauth_client"][i]["client_type"].AsInt == OAUTH_CLIENT_TYPE_WEB)
            {
                var default_web_client_id = client["oauth_client"][i]["client_id"];
                gsxml += genString("default_web_client_id", default_web_client_id);
                break;
            }
        }
        //Client - services
        var test_banner_ad_unit_id = client["services"]["ads_service"]["test_banner_ad_unit_id"];
        gsxml += genString("test_banner_ad_unit_id", test_banner_ad_unit_id);

        var test_interstitial_ad_unit_id = client["services"]["ads_service"]["test_interstitial_ad_unit_id"];
        gsxml += genString("test_interstitial_ad_unit_id", test_interstitial_ad_unit_id);

        var ga_trackingId = client["services"]["analytics_service"]["ga_trackingId"];
        gsxml += genString("ga_trackingId", ga_trackingId);

        var google_maps_key = client["services"]["maps_service"]["api_key"];
        gsxml += genString("google_maps_key", google_maps_key);

        gsxml += "</resources>";
        // return File.ReadAllText("/Volumes/Data/NifSamples/google-services.xml");
        return gsxml;
    }

    private string genString(string key, object jvalue)
    {
        JSONNode node = (JSONNode)jvalue;
        if (node.Value.Length == 0)
        {
            return "";
        }
        return string.Format("  <string name=\"{0}\" translatable=\"false\">{1}</string>\n", key, node.Value);
    }
}

#endif

