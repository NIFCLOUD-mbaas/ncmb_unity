using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using NCMB;
using System.Reflection;
using System;

public class NCMBAnalyticsTest
{

	[SetUp]
	public void Init ()
	{
		NCMBTestSettings.Initialize ();
	}

	/**
     * - 内容：_getBaseUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void GetBaseUrlTest ()
	{
		// テストデータ作成
		NCMBAnalytics analytics = new NCMBAnalytics ();
		MethodInfo method = analytics.GetType ().GetMethod ("_getBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

		// メソッドのパラメータ作成
		object[] param = new object[1];
		param [0] = "testId";

		Assert.AreEqual ("http://localhost:3000/2013-09-01/push/testId/openNumber", method.Invoke (analytics, param).ToString ());
	}
}
