using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using NCMB;
using System.Reflection;

public class NCMBUserTest {

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
		NCMBUser user = new NCMBUser();

		// internal methodの呼び出し
		MethodInfo method = user.GetType ().GetMethod ("_getBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/users", method.Invoke(user, null).ToString());
	}
}
