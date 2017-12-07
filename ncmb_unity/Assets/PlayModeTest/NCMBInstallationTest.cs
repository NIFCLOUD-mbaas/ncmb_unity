using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using NCMB;
using System.Reflection;

public class NCMBInstallationTest
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
		NCMBInstallation installation = new NCMBInstallation ();

		// internal methodの呼び出し
		MethodInfo method = installation.GetType ().GetMethod ("_getBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/installations", method.Invoke (installation, null).ToString ());
	}
}
