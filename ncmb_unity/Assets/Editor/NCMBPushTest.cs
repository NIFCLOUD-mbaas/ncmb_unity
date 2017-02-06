using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using NCMB;
using System.Reflection;

public class NCMBPushTest {

	[TestFixtureSetUp]
	public void Init ()
	{
		NCMBTestSettings.Initialize ();
	}

	/**
     * - 内容：_getBaseUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void getBaseUrlTest ()
	{
		// テストデータ作成
		NCMBPush push = new NCMBPush();

		// internal methodの呼び出し
		MethodInfo method = push.GetType ().GetMethod ("_getBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/push", method.Invoke(push, null).ToString());
	}
}
