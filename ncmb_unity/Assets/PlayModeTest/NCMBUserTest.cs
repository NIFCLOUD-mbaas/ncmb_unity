using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using NCMB;
using System.Reflection;
using System;
using MiniJSON;
using System.Collections.Generic;
using System.Collections;

public class NCMBUserTest
{

	public static DateTime dummyDate = new DateTime (2017, 2, 7, 1, 2, 3, 4);

	// facebookのダミー認証情報
	NCMBFacebookParameters facebookParams = new NCMBFacebookParameters (
		                                        "facebookDummyId",
		                                        "facebookDummyAccessToken",
		                                        dummyDate
	                                        );
	NCMBFacebookParameters invalidFacebookParams = new NCMBFacebookParameters (
		                                               "invalidFacebookDummyId",
		                                               "invalidFacebookDummyAccessToken",
		                                               dummyDate
	                                               );

	// Twittterのダミー認証情報
	NCMBTwitterParameters twitterParams = new NCMBTwitterParameters (
		                                      "twitterDummyId",
		                                      "twitterDummyScreenName",
		                                      "twitterDummyConsumerKey",
		                                      "twitterDummyConsumerSecret",
		                                      "twitterDummyAuthToken",
		                                      "twitterDummyAuthSecret"
	                                      );
	NCMBTwitterParameters invalidTwitterParams = new NCMBTwitterParameters (
		                                             "invalidTwitterDummyId",
		                                             "invalidTwitterDummyScreenName",
		                                             "invalidTwitterDummyConsumerKey",
		                                             "invalidTwitterDummyConsumerSecret",
		                                             "invalidTwitterDummyAuthToken",
		                                             "invalidTwitterDummyAuthSecret"
	                                             );

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
		NCMBUser user = new NCMBUser ();

		// internal methodの呼び出し
		MethodInfo method = user.GetType ().GetMethod ("_getBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/users", method.Invoke (user, null).ToString ());
	}

	/**
     * - 内容：_getLogInUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void GetLogInUrlTest ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();

		// internal methodの呼び出し
		MethodInfo method = user.GetType ().GetMethod ("_getLogInUrl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/login", method.Invoke (user, null).ToString ());
	}

	/**
     * - 内容：_getLogOutUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void GetLogOutUrlTest ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();

		// internal methodの呼び出し
		MethodInfo method = user.GetType ().GetMethod ("_getLogOutUrl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/logout", method.Invoke (user, null).ToString ());
	}

	/**
     * - 内容：_getRequestPasswordResetUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void GetRequestPasswordResetUrlTest ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();

		// internal methodの呼び出し
		MethodInfo method = user.GetType ().GetMethod ("_getRequestPasswordResetUrl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/requestPasswordReset", method.Invoke (user, null).ToString ());
	}

	/**
     * - 内容：_getmailAddressUserEntryUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void GetMailAddressUserEntryUrlTest ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();

		// internal methodの呼び出し
		MethodInfo method = user.GetType ().GetMethod ("_getmailAddressUserEntryUrl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/requestMailAddressUserEntry", method.Invoke (user, null).ToString ());
	}

	/**
     * - 内容：LogInWithAuthDataAsyncがFacebookで成功する事を確認する
     * - 結果：各パラメータが正しく取得できること
     */
	[UnityTest]
	public IEnumerator LogInWithAuthDataAsyncFacebook ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = facebookParams.param;

		// authData登録
		user.LogInWithAuthDataAsync ((NCMBException e) => {
			Assert.Null (e);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();

		Assert.AreEqual ("dummyObjectId", user.ObjectId);

		// facebookパラメータ確認
		Dictionary<string, object> authData = user.GetAuthDataForProvider ("facebook");
		Assert.AreEqual ("facebookDummyId", authData ["id"]);
		Assert.AreEqual ("facebookDummyAccessToken", authData ["access_token"]);
		Assert.AreEqual ("2017-02-07T01:02:03.004Z", authData ["expiration_date"]);

		// 登録成功の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken ());
		Assert.True (user.IsLinkWith ("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LogInWithAuthDataAsyncが無効なリクエストの時にFacebookで失敗する事を確認する
     * - 結果：session Tokenがnullであること
     */
	[UnityTest]
	public IEnumerator LogInWithAuthDataAsyncInvalidFacebook ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = invalidFacebookParams.param;

		// authData登録
		user.LogInWithAuthDataAsync ((NCMBException e) => {
			Assert.AreEqual (NCMBException.OAUTH_ERROR, e.ErrorCode);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();

		// 登録失敗の確認
		Assert.IsEmpty (NCMBUser._getCurrentSessionToken ());
		Assert.False (user.IsLinkWith ("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LogInWithAuthDataAsyncがTwitterで成功する事を確認する
     * - 結果：各パラメータが正しく取得できること
     */
	[UnityTest]
	public IEnumerator LogInWithAuthDataAsyncTwitter ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = twitterParams.param;

		// authData登録
		user.LogInWithAuthDataAsync ((NCMBException e) => {
			Assert.Null (e);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();

		Assert.AreEqual ("dummyObjectId", user.ObjectId);

		// twitterパラメータ確認
		Dictionary<string, object> authData = user.GetAuthDataForProvider ("twitter");
		Assert.AreEqual ("twitterDummyId", authData ["id"]);
		Assert.AreEqual ("twitterDummyScreenName", authData ["screen_name"]);
		Assert.AreEqual ("twitterDummyConsumerKey", authData ["oauth_consumer_key"]);
		Assert.AreEqual ("twitterDummyConsumerSecret", authData ["consumer_secret"]);
		Assert.AreEqual ("twitterDummyAuthToken", authData ["oauth_token"]);
		Assert.AreEqual ("twitterDummyAuthSecret", authData ["oauth_token_secret"]);

		// 登録成功の確認
		Assert.NotNull (NCMBUser._getCurrentSessionToken ());
		Assert.True (user.IsLinkWith ("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LogInWithAuthDataAsyncが無効なリクエストの時にTwitterで失敗する事を確認する
     * - 結果：session Tokenがnullであること
     */
	[UnityTest]
	public IEnumerator LogInWithAuthDataAsyncInvalidTwitter ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = invalidTwitterParams.param;

		// authData登録
		user.LogInWithAuthDataAsync ((NCMBException e) => {
			Assert.AreEqual (NCMBException.OAUTH_ERROR, e.ErrorCode);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();

		// 登録失敗の確認
		Assert.IsEmpty (NCMBUser._getCurrentSessionToken ());
		Assert.False (user.IsLinkWith ("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LinkWithAuthDataAsyncがFacebookで成功する事を確認する
     * - 結果：リンクしているauth dataがFacebookであること
     */
	[UnityTest]
	public IEnumerator LinkWithAuthDataAsyncFacebook ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = twitterParams.param;

		// authData登録
		user.LogInWithAuthDataAsync ((NCMBException e) => {
			Assert.Null (e);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		// authData追加
		user.LinkWithAuthDataAsync (facebookParams.param, (NCMBException e1) => {
			Assert.Null (e1);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();

		// 追加成功の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken ());
		Assert.True (user.IsLinkWith ("twitter"));
		Assert.True (user.IsLinkWith ("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LinkWithAuthDataAsyncがFacebookで無効なリクエストで失敗する事を確認する
     * - 結果：リンクしているauth dataがFacebookでないこと
     */
	[UnityTest]
	public IEnumerator LinkWithAuthDataAsyncInvalidFacebook ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = twitterParams.param;

		// authData登録
		user.LogInWithAuthDataAsync ((NCMBException e) => {
			Assert.Null (e);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		// authData追加
		user.LinkWithAuthDataAsync (invalidFacebookParams.param, (NCMBException e) => {
			Assert.AreEqual (NCMBException.OAUTH_ERROR, e.ErrorCode);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();

		// 追加失敗の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken ());
		Assert.True (user.IsLinkWith ("twitter"));
		Assert.False (user.IsLinkWith ("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LinkWithAuthDataAsyncがTwitterで成功する事を確認する
     * - 結果：リンクしているauth dataがTwitterであること
     */
	[UnityTest]
	public IEnumerator LinkWithAuthDataAsyncTwitter ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = facebookParams.param;

		// authData登録
		user.LogInWithAuthDataAsync ((NCMBException e) => {
			Assert.Null (e);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		// authData追加
		user.LinkWithAuthDataAsync (twitterParams.param, (NCMBException e1) => {
			Assert.Null (e1);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();

		// 追加成功の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken ());
		Assert.True (user.IsLinkWith ("facebook"));
		Assert.True (user.IsLinkWith ("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：LinkWithAuthDataAsyncがTwitterで無効なリクエストで失敗する事を確認する
     * - 結果：リンクしているauth dataがTwitterでないこと
     */
	[UnityTest]
	public IEnumerator LinkWithAuthDataAsyncInvalidTwitter ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = facebookParams.param;

		// authData登録
		user.LogInWithAuthDataAsync ((NCMBException e) => {
			Assert.Null (e);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		// authData追加
		user.LinkWithAuthDataAsync (invalidTwitterParams.param, (NCMBException e) => {
			Assert.AreEqual (NCMBException.OAUTH_ERROR, e.ErrorCode);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();

		// 追加失敗の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken ());
		Assert.True (user.IsLinkWith ("facebook"));
		Assert.False (user.IsLinkWith ("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：UnLinkがFacebookで成功する事を確認する
     * - 結果：リンクしているauth dataがFacebookでないこと
     */
	[UnityTest]
	public IEnumerator UnLinkWithAuthDataAsyncFacebook ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = facebookParams.param;

		// authData登録
		user.LogInWithAuthDataAsync ((NCMBException e) => {
			Assert.Null (e);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		Assert.True (user.IsLinkWith ("facebook"));

		// authData削除
		user.UnLinkWithAuthDataAsync ("facebook", (NCMBException e1) => {
			Assert.Null (e1);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();

		// 削除成功の確認
		Assert.False (user.IsLinkWith ("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：UnLinkがTwitterで成功する事を確認する
     * - 結果：リンクしているauth dataがTwitterでないこと
     */
	[UnityTest]
	public IEnumerator UnLinkWithAuthDataAsyncTwitter ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = twitterParams.param;

		// authData登録
		user.LogInWithAuthDataAsync ((NCMBException e) => {
			Assert.Null (e);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		Assert.True (user.IsLinkWith ("twitter"));

		// authData削除
		user.UnLinkWithAuthDataAsync ("twitter", (NCMBException e1) => {
			Assert.Null (e1);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();

		// 削除成功の確認
		Assert.False (user.IsLinkWith ("twitter"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：facebookでloginし、twitterでlinkした後、facebookがunlinkできるかを確認する
     * - 結果：auth dataにfacebookの値がないこと
     */
	[UnityTest]
	public IEnumerator UnLinkFacebookLinkTwitter ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.AuthData = facebookParams.param;

		// authData登録
		user.LogInWithAuthDataAsync ((NCMBException e) => {
			Assert.Null (e);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		// authData追加
		user.LinkWithAuthDataAsync (twitterParams.param, (NCMBException e1) => {
			Assert.Null (e1);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;
		Assert.True (user.IsLinkWith ("twitter"));

		// authData削除
		user.UnLinkWithAuthDataAsync ("facebook", (NCMBException e2) => {
			Assert.Null (e2);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();

		// 削除成功の確認
		Assert.IsNotEmpty (NCMBUser._getCurrentSessionToken ());
		Assert.True (user.IsLinkWith ("twitter"));
		Assert.False (user.IsLinkWith ("facebook"));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：SignUpAsyncが成功する事を確認する
     * - 結果：各パラメータが正しく取得できること
     */
	[UnityTest]
	public IEnumerator SignUpAsyncTest ()
	{
		// テストデータ作成
		NCMBUser user = new NCMBUser ();
		user.UserName = "tarou";
		user.Password = "tarou";
		user.Email = "sample@example.com";

		// 会員登録
		user.SignUpAsync ((NCMBException e) => {
			Assert.Null (e);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();

		Assert.AreEqual ("dummyObjectId", user.ObjectId);

		// 登録成功の確認
		Assert.IsEmpty (NCMBUser._getCurrentSessionToken ());
		Assert.True (NCMBTestSettings.CallbackFlag);
	}
    /**
     * - 内容：LogInWithMailAddressAsync 
     * - 結果：各パラメータが正しく取得できること
     */
    [UnityTest]
    public IEnumerator LogInWithMailAddressAsync()
    {
        // テストデータ作成
        NCMBUser.LogInWithMailAddressAsync("sample@example.com", "password", (e) => {
            Assert.Null(e);
            NCMBTestSettings.CallbackFlag = true;
        });

        yield return NCMBTestSettings.AwaitAsync();
        // 登録成功の確認
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
        Assert.AreEqual("sample@example.com", NCMBUser.CurrentUser.Email);
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    /**
    * - 内容：LogInAsync
    * - 結果：各パラメータが正しく取得できること
    */
    [UnityTest]
    public IEnumerator LogInAsync()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) => {
            Assert.Null(e);
            NCMBTestSettings.CallbackFlag = true;
        });

        yield return NCMBTestSettings.AwaitAsync();
        // 登録成功の確認
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
        Assert.AreEqual("tarou", NCMBUser.CurrentUser.UserName);
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    [UnityTest]
    public IEnumerator FetchCurrentUserAfterLogin()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) => {
            Assert.Null(e);

            NCMBUser.CurrentUser.FetchAsync((NCMBException ex) =>
            {
                Assert.Null(ex);
                NCMBTestSettings.CallbackFlag = true;
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        // 登録成功の確認
        Assert.True(NCMBTestSettings.CallbackFlag);
        Assert.AreEqual("tarou", NCMBUser.CurrentUser.UserName);
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
        Assert.AreEqual("sample@example.com", NCMBUser.CurrentUser.Email);
    }

    [UnityTest]
    public IEnumerator UpdateCurrentUserAfterLogin()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) =>
        {
            Assert.Null(e);

            NCMBUser.CurrentUser.UserName = "newUserName";

            NCMBUser.CurrentUser.SaveAsync((NCMBException ex) =>
            {
                Assert.Null(ex);

                NCMBTestSettings.CallbackFlag = true;
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        // 登録成功の確認
        Assert.True(NCMBTestSettings.CallbackFlag);
        Assert.AreEqual("newUserName", NCMBUser.CurrentUser.UserName);
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
        Assert.AreEqual("sample@example.com", NCMBUser.CurrentUser.Email);
    }

    [UnityTest]
    public IEnumerator UpdateCurrentUserTwoTimesAfterLogin()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) =>
        {
            Assert.Null(e);

            NCMBUser.CurrentUser.UserName = "newUserName";

            NCMBUser.CurrentUser.SaveAsync((NCMBException ex1) =>
            {
                Assert.Null(ex1);

                NCMBUser.CurrentUser.UserName = "newUserName";
                NCMBUser.CurrentUser.SaveAsync((NCMBException ex2) =>
                {
                    Assert.Null(ex2);

                    NCMBTestSettings.CallbackFlag = true;
                });
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        // 登録成功の確認
        Assert.True(NCMBTestSettings.CallbackFlag);
        Assert.AreEqual("newUserName", NCMBUser.CurrentUser.UserName);
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
        Assert.AreEqual("sample@example.com", NCMBUser.CurrentUser.Email);
        Assert.AreEqual(0, NCMBUser.CurrentUser._currentOperations.Count);
    }

    [UnityTest]
    public IEnumerator SignUpUseCurrentUserAfterLogin()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) =>
        {
            Assert.Null(e);

            NCMBUser.CurrentUser.ObjectId = null;
            NCMBUser.CurrentUser.UserName = "testuser";
            NCMBUser.CurrentUser.Password = "password";

            // 会員登録
            NCMBUser.CurrentUser.SignUpAsync((NCMBException ex) => {
                Assert.Null(ex);
                NCMBTestSettings.CallbackFlag = true;
            });
        });

        yield return NCMBTestSettings.AwaitAsync();

        // 登録成功の確認
        Assert.True(NCMBTestSettings.CallbackFlag);
        Assert.AreEqual("testuser", NCMBUser.CurrentUser.UserName);
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
    }

    [UnityTest]
    public IEnumerator DeleteUseCurrentUserAfterLogin()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) =>
        {
            Assert.Null(e);

            NCMBUser.CurrentUser.DeleteAsync((NCMBException ex) => {
                Assert.Null(ex);
                NCMBTestSettings.CallbackFlag = true;
            });
        });

        yield return NCMBTestSettings.AwaitAsync();

        // 登録成功の確認
        Assert.True(NCMBTestSettings.CallbackFlag);
        Assert.IsNull(NCMBUser.CurrentUser);
    }

    [UnityTest]
    public IEnumerator FetchOtherUserAfterLogin()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) => {
            Assert.Null(e);

            NCMBUser user = new NCMBUser();
            user.ObjectId = "anotherObjectId";
            user.FetchAsync((NCMBException ex) =>
            {
                Assert.Null(ex);
                NCMBTestSettings.CallbackFlag = true;
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        // 登録成功の確認
        Assert.True(NCMBTestSettings.CallbackFlag);
        Assert.AreEqual("tarou", NCMBUser.CurrentUser.UserName);
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
    }

    [UnityTest]
    public IEnumerator UpdateOtherUserAfterLogin()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) =>
        {
            Assert.Null(e);

            NCMBUser user = new NCMBUser();
            user.ObjectId = "anotherObjectId";
            user.UserName = "newUserName";
            user.SaveAsync((NCMBException ex) =>
            {
                Assert.Null(ex);

                NCMBTestSettings.CallbackFlag = true;
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        // 登録成功の確認
        Assert.True(NCMBTestSettings.CallbackFlag);
        Assert.IsNotNull(NCMBUser.CurrentUser);
        Assert.AreEqual("tarou", NCMBUser.CurrentUser.UserName);
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
    }

    [UnityTest]
    public IEnumerator DeleteOtherUserAfterLogin()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) =>
        {
            Assert.Null(e);

            NCMBUser user = new NCMBUser();
            user.ObjectId = "anotherObjectId";
            user.DeleteAsync((NCMBException ex) => {
                Assert.Null(ex);
                NCMBTestSettings.CallbackFlag = true;
            });
        });

        yield return NCMBTestSettings.AwaitAsync();

        Assert.IsNotNull(NCMBUser.CurrentUser);
        Assert.AreEqual("tarou", NCMBUser.CurrentUser.UserName);
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
    }

    [UnityTest]
    public IEnumerator SignUpOtherUserAfterLogin()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) =>
        {
            Assert.Null(e);

            NCMBUser user = new NCMBUser();
            user.UserName = "testuser";
            user.Password = "password";

            // 会員登録
            user.SignUpAsync((NCMBException ex) => {
                Assert.Null(ex);
                NCMBTestSettings.CallbackFlag = true;
            });
        });

        yield return NCMBTestSettings.AwaitAsync();

        Assert.True(NCMBTestSettings.CallbackFlag);
        Assert.AreEqual("tarou", NCMBUser.CurrentUser.UserName);
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
    }

    [UnityTest]
    public IEnumerator LoginLogoutFetchUser()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e1) =>
        {
            Assert.Null(e1);
            NCMBTestSettings.CallbackFlag = true;
            NCMBUser.LogOutAsync();
            NCMBUser.LogInAsync("tarou", "tarou", (e2) =>
            {
                Assert.Null(e2);
                NCMBUser user = new NCMBUser();
                user.ObjectId = "anotherObjectId";
                user.FetchAsync((NCMBException e3) =>
                {
                    Assert.Null(e3);
                    NCMBTestSettings.CallbackFlag = true;
                });
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        Assert.True(NCMBTestSettings.CallbackFlag);
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
        Assert.AreEqual("tarou", NCMBUser.CurrentUser.UserName);
    }

    [UnityTest]
    public IEnumerator LoginLogoutUpdateUser()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e1) =>
        {
            Assert.Null(e1);
            NCMBTestSettings.CallbackFlag = true;
            NCMBUser.LogOutAsync();
            NCMBUser.LogInAsync("tarou", "tarou", (e2) =>
            {
                Assert.Null(e2);
                NCMBUser user = new NCMBUser();
                user.ObjectId = "anotherObjectId";
                user.UserName = "newUserName";
                user.SaveAsync((NCMBException e3) =>
                {
                    Assert.Null(e3);

                    NCMBTestSettings.CallbackFlag = true;
                });
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        Assert.True(NCMBTestSettings.CallbackFlag);
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
        Assert.AreEqual("tarou", NCMBUser.CurrentUser.UserName);
    }

    [UnityTest]
    public IEnumerator LoginLogoutAddUser()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e1) =>
        {
            Assert.Null(e1);
            NCMBTestSettings.CallbackFlag = true;
            NCMBUser.LogOutAsync();
            NCMBUser.LogInAsync("tarou", "tarou", (e2) =>
            {
                Assert.Null(e2);
                NCMBUser user = new NCMBUser();
                user.UserName = "testuser";
                user.Password = "password";
                user.SignUpAsync((NCMBException e3) => {
                    Assert.Null(e3);
                    NCMBTestSettings.CallbackFlag = true;
                });
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        Assert.True(NCMBTestSettings.CallbackFlag);
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
        Assert.AreEqual("tarou", NCMBUser.CurrentUser.UserName);
    }

    [UnityTest]
    public IEnumerator LoginLogoutDeleteUser()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e1) =>
        {
            Assert.Null(e1);
            NCMBTestSettings.CallbackFlag = true;
            NCMBUser.LogOutAsync();
            NCMBUser.LogInAsync("tarou", "tarou", (e2) =>
            {
                Assert.Null(e2);
                NCMBUser user = new NCMBUser();
                user.ObjectId = "anotherObjectId";
                user.DeleteAsync((NCMBException e3) => {
                    Assert.Null(e3);
                    NCMBTestSettings.CallbackFlag = true;
                });
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        Assert.True(NCMBTestSettings.CallbackFlag);
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
        Assert.AreEqual("tarou", NCMBUser.CurrentUser.UserName);
    }

        [UnityTest]
    public IEnumerator FetchAsyncCurrentUserAuthenticationError()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) => {
            Assert.Null(e);

            NCMBUser.CurrentUser.SessionToken = "invalidToken";
            NCMBUser.CurrentUser.ObjectId = "dummyObjectIdError";
            NCMBUser.CurrentUser._currentOperations.Clear();

            NCMBUser user = NCMBUser.CurrentUser;
            user.FetchAsync((NCMBException ex) =>
            {
                Assert.NotNull(ex);
                Assert.AreEqual("E401001", ex.ErrorCode);
                Assert.AreEqual("Authentication error by header incorrect.", ex.ErrorMessage);
                NCMBTestSettings.CallbackFlag = true;
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        // 登録成功の確認
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    [UnityTest]
    public IEnumerator FetchAsyncCurrentUserDataNotAvailable()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) => {
            Assert.Null(e);

            NCMBUser.CurrentUser.SessionToken = "invalidToken";
            NCMBUser.CurrentUser.ObjectId = "invalidObjectId";
            NCMBUser.CurrentUser._currentOperations.Clear();

            NCMBUser user = NCMBUser.CurrentUser;
            user.FetchAsync((NCMBException ex) =>
            {
                Assert.NotNull(ex);
                Assert.AreEqual("E404001", ex.ErrorCode);
                Assert.AreEqual("No data available.", ex.ErrorMessage);
                NCMBTestSettings.CallbackFlag = true;
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        // 登録成功の確認
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    [UnityTest]
    public IEnumerator FetchAsyncUserAuthenticationError()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) => {
            Assert.Null(e);

            NCMBUser.CurrentUser.SessionToken = "invalidToken";
            NCMBUser.CurrentUser._currentOperations.Clear();

            NCMBUser user = new NCMBUser();
            user.ObjectId = "dummyObjectIdError";
            user.FetchAsync((NCMBException ex) =>
            {
                Assert.NotNull(ex);
                Assert.AreEqual("E401001", ex.ErrorCode);
                Assert.AreEqual("Authentication error by header incorrect.", ex.ErrorMessage);
                NCMBTestSettings.CallbackFlag = true;
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        // 登録成功の確認
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    [UnityTest]
    public IEnumerator FetchAsyncUserDataNotAvailable()
    {
        // テストデータ作成
        NCMBUser user = new NCMBUser();
        user.ObjectId = "invalidObjectId";
        user.FetchAsync((NCMBException ex) =>
        {
            Assert.NotNull(ex);
            Assert.AreEqual("E404001", ex.ErrorCode);
            Assert.AreEqual("No data available.", ex.ErrorMessage);
            NCMBTestSettings.CallbackFlag = true;
        });

        yield return NCMBTestSettings.AwaitAsync();
        // 登録成功の確認
        Assert.True(NCMBTestSettings.CallbackFlag);
    }
}