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
	public void GetBaseUrlTest ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser();

		// internal methodの呼び出し
		MethodInfo method = user.GetType ().GetMethod ("_getBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/users", method.Invoke(user, null).ToString());
	}

	/**
     * - 内容：_getLogInUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void GetLogInUrlTest ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser();

		// internal methodの呼び出し
		MethodInfo method = user.GetType ().GetMethod ("_getLogInUrl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/login", method.Invoke(user, null).ToString());
	}

	/**
     * - 内容：_getLogOutUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void GetLogOutUrlTest ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser();

		// internal methodの呼び出し
		MethodInfo method = user.GetType ().GetMethod ("_getLogOutUrl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/logout", method.Invoke(user, null).ToString());
	}

	/**
     * - 内容：_getRequestPasswordResetUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void GetRequestPasswordResetUrlTest ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser();

		// internal methodの呼び出し
		MethodInfo method = user.GetType ().GetMethod ("_getRequestPasswordResetUrl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/requestPasswordReset", method.Invoke(user, null).ToString());
	}

	/**
     * - 内容：_getmailAddressUserEntryUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void GetMailAddressUserEntryUrlTest ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser();

		// internal methodの呼び出し
		MethodInfo method = user.GetType ().GetMethod ("_getmailAddressUserEntryUrl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/requestMailAddressUserEntry", method.Invoke(user, null).ToString());
	}
}
