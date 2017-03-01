﻿using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using NCMB;
using System.Reflection;
using System;
using MiniJSON;
using System.Collections.Generic;

public class NCMBUserTest {

	public static DateTime dummyDate = new DateTime(2017, 2, 7, 1, 2, 3, 4);

	// facebookのダミー認証情報
	NCMBFacebookParameters facebookParams = new NCMBFacebookParameters(
		"facebookDummyId",
		"facebookDummyAccessToken",
		dummyDate
	);
	NCMBFacebookParameters invalidFacebookParams = new NCMBFacebookParameters(
		"invalidFacebookDummyId",
		"invalidFacebookDummyAccessToken",
		dummyDate
	);

	// Twittterのダミー認証情報
	NCMBTwitterParameters twitterParams = new NCMBTwitterParameters(
		"twitterDummyId",
		"twitterDummyScreenName",
		"twitterDummyConsumerKey",
		"twitterDummyConsumerSecret",
		"twitterDummyAuthToken",
		"twitterDummyAuthSecret"
	);
	NCMBTwitterParameters invalidTwitterParams = new NCMBTwitterParameters(
		"invalidTwitterDummyId",
		"invalidTwitterDummyScreenName",
		"invalidTwitterDummyConsumerKey",
		"invalidTwitterDummyConsumerSecret",
		"invalidTwitterDummyAuthToken",
		"invalidTwitterDummyAuthSecret"
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
	public void LogInWithAuthAsyncFacebook()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = facebookParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.Null(e);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.AreEqual ("dummyObjectId", user.ObjectId);

		// facebookパラメータ確認
		Dictionary<string, object> authData = user.getAuthDataForProvider ("facebook");
		Assert.AreEqual ("facebookDummyId", authData["id"]);
		Assert.AreEqual ("facebookDummyAccessToken", authData["access_token"]);
		Assert.AreEqual ("2017-02-07T01:02:03.004Z", authData["expiration_date"]);

		// 登録成功の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken());
		Assert.True (user.IsLinkWith("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：LogInWithOauthAsyncが無効なリクエストの時にFacebookで失敗する事を確認する
     * - 結果：session Tokenがnullであること
     */
	[Test]
	public void LogInWithAuthAsyncInvalidFacebook()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = invalidFacebookParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.AreEqual(NCMBException.OAUTH_ERROR, e.ErrorCode);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		// 登録失敗の確認
		Assert.IsEmpty (NCMBUser._getCurrentSessionToken());
		Assert.False (user.IsLinkWith("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：LogInWithOauthAsyncがTwitterで成功する事を確認する
     * - 結果：各パラメータが正しく取得できること
     */
	[Test]
	public void LogInWithAuthAsyncTwitter()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = twitterParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.Null(e);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.AreEqual ("dummyObjectId", user.ObjectId);

		// twitterパラメータ確認
		Dictionary<string, object> authData = user.getAuthDataForProvider ("twitter");
		Assert.AreEqual ("twitterDummyId", authData["id"]);
		Assert.AreEqual ("twitterDummyScreenName", authData["screen_name"]);
		Assert.AreEqual ("twitterDummyConsumerKey", authData["oauth_consumer_key"]);
		Assert.AreEqual ("twitterDummyConsumerSecret", authData["consumer_secret"]);
		Assert.AreEqual ("twitterDummyAuthToken", authData["oauth_token"]);
		Assert.AreEqual ("twitterDummyAuthSecret", authData["oauth_token_secret"]);

		// 登録成功の確認
		Assert.NotNull (NCMBUser._getCurrentSessionToken());
		Assert.True (user.IsLinkWith("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：LogInWithOauthAsyncが無効なリクエストの時にTwitterで失敗する事を確認する
     * - 結果：session Tokenがnullであること
     */
	[Test]
	public void LogInWithAuthAsyncInvalidTwitter()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = invalidTwitterParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.AreEqual(NCMBException.OAUTH_ERROR, e.ErrorCode);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		// 登録失敗の確認
		Assert.IsEmpty (NCMBUser._getCurrentSessionToken());
		Assert.False (user.IsLinkWith("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：LogInWithOauthAsyncがGoogleで成功する事を確認する
     * - 結果：各パラメータが正しく取得できること
     */
	[Test]
	public void LogInWithAuthAsyncGoogle()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = googleParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.Null(e);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.AreEqual ("dummyObjectId", user.ObjectId);

		// googleパラメータ確認
		Dictionary<string, object> authData = user.getAuthDataForProvider ("google");
		Assert.AreEqual ("googleDummyId", authData["id"]);
		Assert.AreEqual ("googleDummyAccessToken", authData["access_token"]);

		// 登録成功の確認
		Assert.NotNull (NCMBUser._getCurrentSessionToken());
		Assert.True (user.IsLinkWith("google"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：LogInWithOauthAsyncが無効なリクエストの時にGoogleで失敗する事を確認する
     * - 結果：session Tokenがnullであること
     */
	[Test]
	public void LogInWithAuthAsyncInvalidGoogle()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = invalidGoogleParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.AreEqual(NCMBException.OAUTH_ERROR, e.ErrorCode);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		// 登録失敗の確認
		Assert.IsEmpty (NCMBUser._getCurrentSessionToken());
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：LinkWithOauthAsyncがFacebookで成功する事を確認する
     * - 結果：リンクしているauth dataがFacebookであること
     */
	[Test]
	public void LinkWithAuthAsyncFacebook()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = twitterParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.Null(e);
		});
		NCMBTestSettings.AwaitAsync ();

		// authData追加
		user.LinkWithAuthAsync (facebookParams.param, (NCMBException e1) => {
			Assert.Null(e1);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		// 追加成功の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken());
		Assert.True (user.IsLinkWith("twitter"));
		Assert.True (user.IsLinkWith("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：LinkWithOauthAsyncがFacebookで無効なリクエストで失敗する事を確認する
     * - 結果：リンクしているauth dataがFacebookでないこと
     */
	[Test]
	public void LinkWithAuthAsyncInvalidFacebook()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = twitterParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.Null(e);
		});
		NCMBTestSettings.AwaitAsync ();

		// authData追加
		user.LinkWithAuthAsync (invalidFacebookParams.param, (NCMBException e) => {
			Assert.AreEqual(NCMBException.OAUTH_ERROR, e.ErrorCode);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		// 追加失敗の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken());
		Assert.True (user.IsLinkWith("twitter"));
		Assert.False (user.IsLinkWith("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：LinkWithOauthAsyncがTwitterで成功する事を確認する
     * - 結果：リンクしているauth dataがTwitterであること
     */
	[Test]
	public void LinkWithAuthAsyncTwitter()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = googleParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.Null(e);
		});
		NCMBTestSettings.AwaitAsync ();

		// authData追加
		user.LinkWithAuthAsync (twitterParams.param, (NCMBException e1) => {
			Assert.Null(e1);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		// 追加成功の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken());
		Assert.True (user.IsLinkWith("google"));
		Assert.True (user.IsLinkWith("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：LinkWithOauthAsyncがTwitterで無効なリクエストで失敗する事を確認する
     * - 結果：リンクしているauth dataがTwitterでないこと
     */
	[Test]
	public void LinkWithAuthAsyncInvalidTwitter()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = googleParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.Null(e);
		});
		NCMBTestSettings.AwaitAsync ();

		// authData追加
		user.LinkWithAuthAsync (invalidTwitterParams.param, (NCMBException e) => {
			Assert.AreEqual(NCMBException.OAUTH_ERROR, e.ErrorCode);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		// 追加失敗の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken());
		Assert.True (user.IsLinkWith("google"));
		Assert.False (user.IsLinkWith("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：LinkWithOauthAsyncがGoogleで成功する事を確認する
     * - 結果：リンクしているauth dataがGoogleであること
     */
	[Test]
	public void LinkWithAuthAsyncGoogle()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = facebookParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.Null(e);
		});
		NCMBTestSettings.AwaitAsync ();

		// authData追加
		user.LinkWithAuthAsync (googleParams.param, (NCMBException e1) => {
			Assert.Null(e1);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		// 登録成功の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken());
		Assert.True (user.IsLinkWith("facebook"));
		Assert.True (user.IsLinkWith("google"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：LinkWithOauthAsyncがGoogleで無効なリクエストで失敗する事を確認する
     * - 結果：リンクしているauth dataがGoogleでないこと
     */
	[Test]
	public void LinkWithAuthAsyncInvalidGoogle()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = facebookParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.Null(e);
		});
		NCMBTestSettings.AwaitAsync ();

		// authData追加
		user.LinkWithAuthAsync (invalidGoogleParams.param, (NCMBException e) => {
			Assert.AreEqual(NCMBException.OAUTH_ERROR, e.ErrorCode);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		// 追加失敗の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken());
		Assert.True (user.IsLinkWith("facebook"));
		Assert.False (user.IsLinkWith("google"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：UnLinkがFacebookで成功する事を確認する
     * - 結果：リンクしているauth dataがFacebookでないこと
     */
	[Test]
	public void UnLinkWithAuthAsyncFacebook()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = facebookParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.Null(e);
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.True (user.IsLinkWith("facebook"));

		// authData削除
		user.UnLinkWithAuthAsync ("facebook", (NCMBException e1) => {
			Assert.Null(e1);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		// 削除成功の確認
		Assert.False (user.IsLinkWith("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：UnLinkがTwitterで成功する事を確認する
     * - 結果：リンクしているauth dataがTwitterでないこと
     */
	[Test]
	public void UnLinkWithAuthAsyncTwitter()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = twitterParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.Null(e);
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.True (user.IsLinkWith("twitter"));

		// authData削除
		user.UnLinkWithAuthAsync ("twitter", (NCMBException e1) => {
			Assert.Null(e1);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		// 削除成功の確認
		Assert.False (user.IsLinkWith("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：UnLinkがGoogleで成功する事を確認する
     * - 結果：リンクしているauth dataがGoogleでないこと
     */
	[Test]
	public void UnLinkWithAuthAsyncGoogle()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = googleParams.param;

		// authData作成
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.Null(e);
		});
		NCMBTestSettings.AwaitAsync ();

		Assert.True (user.IsLinkWith("google"));

		// authData削除
		user.UnLinkWithAuthAsync ("google", (NCMBException e1) => {
			Assert.Null(e1);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		// 削除成功の確認
		Assert.False (user.IsLinkWith("google"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}

	/**
     * - 内容：facebookでloginし、twitterでlinkした後、facebookがunlinkできるかを確認する
     * - 結果：auth dataにfacebookの値がないこと
     */
	[Test]
	public void UnLinkFacebookLinkTwitter()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = facebookParams.param;

		// authData登録
		user.LogInWithAuthAsync ((NCMBException e) => {
			Assert.Null(e);
		});
		NCMBTestSettings.AwaitAsync ();

		// authData追加
		user.LinkWithAuthAsync (twitterParams.param, (NCMBException e1) => {
			Assert.Null(e1);
		});
		NCMBTestSettings.AwaitAsync ();
		Assert.True (user.IsLinkWith("twitter"));

		// authData削除
		user.UnLinkWithAuthAsync ("facebook", (NCMBException e2) => {
			Assert.Null(e2);
			NCMBTestSettings.CallbackFlag = true;
		});
		NCMBTestSettings.AwaitAsync ();

		// 削除成功の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken());
		Assert.True (user.IsLinkWith("twitter"));
		Assert.False (user.IsLinkWith("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
		NCMBUser.LogOutAsync ();
	}
}