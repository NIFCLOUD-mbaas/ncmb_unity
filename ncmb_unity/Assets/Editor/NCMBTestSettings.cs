using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NCMB;
using System;
using System.Reflection;
using NUnit.Framework;

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
		NCMBSettings.Initialize (
			APP_KEY,
			CLIENT_KEY,
			DOMAIN_URL,
			API_VERSION
		);
		CallbackFlag = false;

		GameObject o = new GameObject ("settings");
		System.Object obj = o.AddComponent<NCMBSettings> ();
		FieldInfo field = obj.GetType ().GetField ("filePath", BindingFlags.Static | BindingFlags.NonPublic);
		field.SetValue (obj, Application.persistentDataPath);

		NCMBUser.LogOutAsync ();
	}

	// 非同期のコールバックが実行されるまで待機
	public static void AwaitAsync ()
	{
		//Platformクラスを取得
		Type platform = null;
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
			foreach (Type type in assembly.GetTypes()) {
				if (type.Name == "Platform") {
					platform = type;
				}
			}
		}
		//コールバックが実行されるか指定時間経過するまでループ
		if (platform != null) {
			var field = platform.GetField ("Queue", BindingFlags.NonPublic | BindingFlags.Static); 
			Queue<Action> queue = (Queue<Action>)field.GetValue (platform);
			DateTime loopTime = DateTime.Now.AddSeconds (15);
			while (true) {
				if (queue.Count >= 1) {
					queue.Dequeue () ();
					if (queue.Count == 0) {
						break;
					}
				}
				if (loopTime < DateTime.Now) {
					Assert.Fail ("Infinite loop timeout.");
					break;
				}
			}
		}
	}
}