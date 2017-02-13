using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using NCMB;
using System.Reflection;
using System;

public class NCMBUserTest {

	DateTime dummyDate;

	// facebookのダミー認証情報
	NCMBFacebookParameters facebookParams = new NCMBFacebookParameters(
		"facebookDummyId",
		"facebookDummyAccessToken",
		dummyDate.ToString("2017-02-07T01:02:03.004Z")
	);
	NCMBFacebookParameters invalidFacebookParams = new NCMBFacebookParameters(
		"invalidfacebookDummyId",
		"invalidfacebookDummyAccessToken",
		dummyDate.ToString("2017-02-07T01:02:03.004Z")
	);

	// Twittterのダミー認証情報
	NCMBTwitterParameters twitterParams = new NCMBTwitterParameters(
		"twitterDummyId",
		"twitterDummyScreenName",
		"twitterDummyConsumerKey",
		"twitterDummyConsumerSecret",
		"twitterDummyOauthToken",
		"twitterDummyOauthSecret"
	);
	NCMBTwitterParameters invalidTwitterParams = new NCMBTwitterParameters(
		"invalidTwitterDummyId",
		"invalidTwitterDummyScreenName",
		"invalidTwitterDummyConsumerKey",
		"invalidTwitterDummyConsumerSecret",
		"invalidTwitterDummyOauthToken",
		"invalidTwitterDummyOauthSecret"
	);

	// Googleのダミー認証情報
	NCMBGoogleParameters googleParams = new NCMBGoogleParameters(
		"googleDummyId",
		"googleDummyAccessToken"
	);
	NCMBGoogleParameters invalidGoogleParams = new NCMBGoogleParameters(
		"invalidGoogleDummyId",
		"invalidGoogleDummyAccessToken"
	);

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

	/**
     * - 内容：LogInWithOauthAsyncがFacebookで成功する事を確認する
     * - 結果：各パラメータが正しく取得できること
     */
	[Test]
	public void LogInWithOauthAsyncFacebook()
	{
		NCMBUser.LogInWithOauthAsync ((facebookParams, error) => {
			Assert.Null(error);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		NCMBUser user = NCMBUser.CurrentUser ();

		Assert.AreEqual (user.ObjectId(), "dummyObjectId");
		Assert.AreEqual (facebookParams.userId, user.getAuthData("facebook").getString("id"));
		Assert.AreEqual (facebookParams.accessToken, user.getAuthData("facebook").getString("access_token"));
		Assert.AreEqual (facebookParams.expirationDate, user.getAuthData("facebook").getJSONObject("expiration_date").getString("iso"));

		Assert.NotNull (NCMBUser._getCurrentSessionToken());
		Assert.True (NCMBUser.CurrentUser().IsLinkWith("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LogInWithOauthAsyncが無効なリクエストの時にFacebookで失敗する事を確認する
     * - 結果：session Tokenがnullであること
     */
	[Test]
	public void LogInWithOauthAsyncInvalidFacebook()
	{
		NCMBUser.LogInWithOauthAsync ((invalidFacebookParams, error) => {
			Assert.AreEqual(NCMBException.OAUTH_ERROR, error.ErrorCode());
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.Null (NCMBUser._getCurrentSessionToken());
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LogInWithOauthAsyncがTwitterで成功する事を確認する
     * - 結果：各パラメータが正しく取得できること
     */
	[Test]
	public void LogInWithOauthAsyncTwitter()
	{
		NCMBUser.LogInWithOauthAsync ((twitterParams, error) => {
			Assert.Null(error);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		NCMBUser user = NCMBUser.CurrentUser ();

		Assert.AreEqual (user.ObjectId(), "dummyObjectId");
		Assert.AreEqual (twitterParams.userId, user.getAuthData("twitter").getString("id"));
		Assert.AreEqual (twitterParams.screenName, user.getAuthData("twitter").getString("screen_name"));
		Assert.AreEqual (twitterParams.consumerKey, user.getAuthData("twitter").getString("oauth_consumer_key"));
		Assert.AreEqual (twitterParams.accessToken, user.getAuthData("twitter").getString("oauth_token"));
		Assert.AreEqual (twitterParams.accessTokenSecret, user.getAuthData("twitter").getString("oauth_token_secret"));

		Assert.NotNull (NCMBUser._getCurrentSessionToken());
		Assert.True (NCMBUser.CurrentUser().IsLinkWith("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LogInWithOauthAsyncが無効なリクエストの時にTwitterで失敗する事を確認する
     * - 結果：session Tokenがnullであること
     */
	[Test]
	public void LogInWithOauthAsyncInvalidTwitter()
	{
		NCMBUser.LogInWithOauthAsync ((invalidTwitterParams, error) => {
			Assert.AreEqual(NCMBException.OAUTH_ERROR, error.ErrorCode());
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();
		NCMBException err = "";

		Assert.Null (NCMBUser._getCurrentSessionToken());
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LogInWithOauthAsyncがGoogleで成功する事を確認する
     * - 結果：各パラメータが正しく取得できること
     */
	[Test]
	public void LogInWithOauthAsyncGoogle()
	{
		NCMBUser.LogInWithOauthAsync ((googleParams, error) => {
			Assert.Null(error);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		NCMBUser user = NCMBUser.CurrentUser ();

		Assert.AreEqual (user.ObjectId(), "dummyObjectId");
		Assert.AreEqual (googleParams.userId, user.getAuthData("google").getString("id"));
		Assert.AreEqual (googleParams.accessToken, user.getAuthData("google").getString("access_token"));
		Assert.AreEqual (facebookParams.expirationDate, user.getAuthData("facebook").getJSONObject("expiration_date").getString("iso"));

		Assert.NotNull (NCMBUser._getCurrentSessionToken());
		Assert.True (NCMBUser.CurrentUser().IsLinkWith("google"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LogInWithOauthAsyncが無効なリクエストの時にGoogleで失敗する事を確認する
     * - 結果：session Tokenがnullであること
     */
	[Test]
	public void LogInWithOauthAsyncInvalidGoogle()
	{
		NCMBUser.LogInWithOauthAsync ((invalidGoogleParams, error) => {
			Assert.AreEqual(NCMBException.OAUTH_ERROR, error.ErrorCode());
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.Null (NCMBUser._getCurrentSessionToken());
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LinkWithOauthAsyncがFacebookで成功する事を確認する
     * - 結果：リンクしているauth dataがFacebookであること
     */
	[Test]
	public void LinkWithOauthAsyncFacebook()
	{
		NCMBUser user = new NCMBUser ();
		user.ObjectId = "dummyUserId";

		NCMBUser.LinkWithOauthAsync ((facebookParams, error) => {
			Assert.Null(error);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.True (NCMBUser.CurrentUser().IsLinkWith("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LinkWithOauthAsyncがFacebookで無効なリクエストで失敗する事を確認する
     * - 結果：リンクしているauth dataがFacebookでないこと
     */
	[Test]
	public void LinkWithOauthAsyncInvalidFacebook()
	{
		NCMBUser user = new NCMBUser ();
		user.ObjectId = "dummyUserId";

		NCMBUser.LinkWithOauthAsync ((invalidFacebookParams, error) => {
			Assert.AreEqual(NCMBException.OAUTH_ERROR, error.ErrorCode());
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.True (NCMBUser.CurrentUser().IsLinkWith("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LinkWithOauthAsyncがTwitterで成功する事を確認する
     * - 結果：リンクしているauth dataがTwitterであること
     */
	[Test]
	public void LinkWithOauthAsyncTwitter()
	{
		NCMBUser user = new NCMBUser ();
		user.ObjectId = "dummyUserId";

		NCMBUser.LinkWithOauthAsync ((twitterParams, error) => {
			Assert.Null(error);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.True (NCMBUser.CurrentUser().IsLinkWith("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LinkWithOauthAsyncがTwitterで無効なリクエストで失敗する事を確認する
     * - 結果：リンクしているauth dataがTwitterでないこと
     */
	[Test]
	public void LinkWithOauthAsyncInvalidTwitter()
	{
		NCMBUser user = new NCMBUser ();
		user.ObjectId = "dummyUserId";

		NCMBUser.LinkWithOauthAsync ((invalidTwitterParams, error) => {
			Assert.AreEqual(NCMBException.OAUTH_ERROR, error.ErrorCode());
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.True (NCMBUser.CurrentUser().IsLinkWith("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LinkWithOauthAsyncがGoogleで成功する事を確認する
     * - 結果：リンクしているauth dataがGoogleであること
     */
	[Test]
	public void LinkWithOauthAsyncGoogle()
	{
		NCMBUser user = new NCMBUser ();
		user.ObjectId = "dummyUserId";

		NCMBUser.LinkWithOauthAsync ((googleParams, error) => {
			Assert.Null(error);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.True (NCMBUser.CurrentUser().IsLinkWith("google"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LinkWithOauthAsyncがGoogleで無効なリクエストで失敗する事を確認する
     * - 結果：リンクしているauth dataがGoogleでないこと
     */
	[Test]
	public void LinkWithOauthAsyncInvalidGoogle()
	{
		NCMBUser user = new NCMBUser ();
		user.ObjectId = "dummyUserId";

		NCMBUser.LinkWithOauthAsync ((invalidGoogleParams, error) => {
			Assert.AreEqual(NCMBException.OAUTH_ERROR, error.ErrorCode());
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.True (NCMBUser.CurrentUser().IsLinkWith("google"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：UnLinkがFacebookで成功する事を確認する
     * - 結果：リンクしているauth dataがFacebookでないこと
     */
	[Test]
	public void UnLinkWithOauthAsyncFacebook()
	{
		NCMBUser.LogInWithOauthAsync ((facebookParams, error) => {
			Assert.Null(error);
		});
		NCMBTestSettings.AwaitAsync ();

		NCMBUser user = NCMBUser.CurrentUser ();
		Assert.True (user.IsLinkWith("facebook"));

		user.UnLinkWithOauthAsync (("facebook", error) => {
			Assert.null(error);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.False (IsLinkWith("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：UnLinkがTwitterで成功する事を確認する
     * - 結果：リンクしているauth dataがTwitterでないこと
     */
	[Test]
	public void UnLinkWithOauthAsyncTwitter()
	{
		NCMBUser.LogInWithOauthAsync ((twitterParams, error) => {
			Assert.Null(error);
		});
		NCMBTestSettings.AwaitAsync ();

		NCMBUser user = NCMBUser.CurrentUser ();
		Assert.True (user.IsLinkWith("twitter"));

		user.UnLinkWithOauthAsync (("twitter", error) => {
			Assert.null(error);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.False (IsLinkWith("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：UnLinkがGoogleで成功する事を確認する
     * - 結果：リンクしているauth dataがGoogleでないこと
     */
	[Test]
	public void UnLinkWithOauthAsyncGoogle()
	{
		NCMBUser.LogInWithOauthAsync ((googleParams, error) => {
			Assert.Null(error);
		});
		NCMBTestSettings.AwaitAsync ();

		NCMBUser user = NCMBUser.CurrentUser ();
		Assert.True (user.IsLinkWith("google"));

		user.UnLinkWithOauthAsync (("google", error) => {
			Assert.null(error);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.False (IsLinkWith("google"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

}
