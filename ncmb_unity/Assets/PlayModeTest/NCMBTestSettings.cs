using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NCMB;
using System;
using System.Reflection;

public class NCMBTestSettings
{
	public static readonly string APP_KEY = "YOUR_APPLICATION_KEY";
	public static readonly string CLIENT_KEY = "YOUR_CLIENT_KEY";
	public static readonly string DOMAIN_URL = "http://localhost:3000";
	public static readonly string API_VERSION = "2013-09-01";
	private static bool _callbackFlag = false;

	public static bool CallbackFlag {
		get {
			return _callbackFlag;
		}
		set {
			_callbackFlag = value;
		}
	}

	// 初期化
	public static void Initialize ()
	{
		
		if (GameObject.Find ("NCMBManager") == null) {
			GameObject manager = new GameObject ();
			manager.name = "NCMBManager";
			manager.AddComponent<NCMBManager> ();
		}

		if (GameObject.Find ("NCMBSettings") == null) {
			GameObject settings = new GameObject ();
			settings.name = "NCMBSettings";
			settings.AddComponent<NCMBSettings> ();
		}

		NCMBSettings.Initialize (
			APP_KEY,
			CLIENT_KEY,
			DOMAIN_URL,
			API_VERSION
		);
		CallbackFlag = false;

		if (GameObject.Find ("settings") == null) {
			GameObject o = new GameObject ("settings");
			System.Object obj = o.AddComponent<NCMBSettings> ();
			FieldInfo field = obj.GetType ().GetField ("filePath", BindingFlags.Static | BindingFlags.NonPublic);
			field.SetValue (obj, Application.persistentDataPath);
		}

		NCMBUser.LogOutAsync ();

		MockServer.startMock ();
	}

	public static IEnumerator AwaitAsync ()
	{
		while (NCMBTestSettings.CallbackFlag == false) {
			//yield return new WaitForEndOfFrame ();
			yield return new WaitForSeconds (0.2f); 
		}
		yield break;
	}
}