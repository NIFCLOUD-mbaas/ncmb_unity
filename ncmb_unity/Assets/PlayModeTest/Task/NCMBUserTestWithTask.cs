#if NET_4_6
using UnityEngine.TestTools;
using NUnit.Framework;
using NCMB;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using NCMB.Tasks;

public class NCMBUserTestWithTask
{
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

    [SetUp]
    public void Init()
    {
        NCMBTestSettings.Initialize();
    }

    /**
     * - 内容：LogInWithAuthDataTaskAsyncがFacebookで成功する事を確認する
     * - 結果：各パラメータが正しく取得できること
     */
    [UnityTest]
    public IEnumerator LogInWithAuthDataTaskAsyncFacebook()
    {
        yield return LogInWithAuthDataTaskAsyncFacebookTask().ToEnumerator();
    }

    public async Task LogInWithAuthDataTaskAsyncFacebookTask()
    {
        // テストデータ作成
        NCMBUser user = new NCMBUser();
        user.AuthData = facebookParams.param;

        // authData登録
        await user.LogInWithAuthDataTaskAsync();

        Assert.AreEqual("dummyObjectId", user.ObjectId);

        // facebookパラメータ確認
        Dictionary<string, object> authData = user.GetAuthDataForProvider("facebook");
        Assert.AreEqual("facebookDummyId", authData["id"]);
        Assert.AreEqual("facebookDummyAccessToken", authData["access_token"]);
        Assert.AreEqual("2017-02-07T01:02:03.004Z", authData["expiration_date"]);

        // 登録成功の確認
        Assert.IsNotEmpty(NCMBUser._getCurrentSessionToken());
        Assert.True(user.IsLinkWith("facebook"));
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    /**
     * - 内容：LogInWithAuthDataAsyncが無効なリクエストの時にFacebookで失敗する事を確認する
     * - 結果：session Tokenがnullであること
     */
    [UnityTest]
    public IEnumerator LogInWithAuthDataAsyncInvalidFacebook()
    {
        yield return LogInWithAuthDataAsyncInvalidFacebookTask().ToEnumerator();
    }

    public async Task LogInWithAuthDataAsyncInvalidFacebookTask()
    {
        // テストデータ作成
        NCMBUser user = new NCMBUser();
        user.AuthData = invalidFacebookParams.param;

        try
        {
            // authData登録
            await user.LogInWithAuthDataTaskAsync();
            Assert.Fail("Exception not thrown");
        }
        catch (NCMBException e)
        {
            Assert.AreEqual(NCMBException.OAUTH_ERROR, e.ErrorCode);
        }

        // 登録失敗の確認
        Assert.IsEmpty(NCMBUser._getCurrentSessionToken());
        Assert.False(user.IsLinkWith("facebook"));
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    /**
     * - 内容：LogInWithAuthDataAsyncがTwitterで成功する事を確認する
     * - 結果：各パラメータが正しく取得できること
     */
    [UnityTest]
    public IEnumerator LogInWithAuthDataTaskAsyncTwitter()
    {
        yield return LogInWithAuthDataTaskAsyncTwitterTask().ToEnumerator();
    }

    public async Task LogInWithAuthDataTaskAsyncTwitterTask()
    {
        // テストデータ作成
        NCMBUser user = new NCMBUser();
        user.AuthData = twitterParams.param;

        // authData登録
        await user.LogInWithAuthDataTaskAsync();

        Assert.AreEqual("dummyObjectId", user.ObjectId);

        // twitterパラメータ確認
        Dictionary<string, object> authData = user.GetAuthDataForProvider("twitter");
        Assert.AreEqual("twitterDummyId", authData["id"]);
        Assert.AreEqual("twitterDummyScreenName", authData["screen_name"]);
        Assert.AreEqual("twitterDummyConsumerKey", authData["oauth_consumer_key"]);
        Assert.AreEqual("twitterDummyConsumerSecret", authData["consumer_secret"]);
        Assert.AreEqual("twitterDummyAuthToken", authData["oauth_token"]);
        Assert.AreEqual("twitterDummyAuthSecret", authData["oauth_token_secret"]);

        // 登録成功の確認
        Assert.NotNull(NCMBUser._getCurrentSessionToken());
        Assert.True(user.IsLinkWith("twitter"));
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    /**
     * - 内容：LogInWithAuthDataAsyncが無効なリクエストの時にTwitterで失敗する事を確認する
     * - 結果：session Tokenがnullであること
     */
    [UnityTest]
    public IEnumerator LogInWithAuthDataTaskAsyncInvalidTwitter()
    {
        yield return LogInWithAuthDataTaskAsyncInvalidTwitterTask().ToEnumerator();
    }

    public async Task LogInWithAuthDataTaskAsyncInvalidTwitterTask()
    {
        // テストデータ作成
        NCMBUser user = new NCMBUser();
        user.AuthData = invalidTwitterParams.param;

        try
        {
            // authData登録
            await user.LogInWithAuthDataTaskAsync();
            Assert.Fail("Exception not thrown");
        }
        catch (NCMBException e)
        {
            Assert.AreEqual(NCMBException.OAUTH_ERROR, e.ErrorCode);
        }

        // 登録失敗の確認
        Assert.IsEmpty(NCMBUser._getCurrentSessionToken());
        Assert.False(user.IsLinkWith("twitter"));
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    /**
     * - 内容：LinkWithAuthDataAsyncがFacebookで成功する事を確認する
     * - 結果：リンクしているauth dataがFacebookであること
     */
    [UnityTest]
    public IEnumerator LinkWithAuthDataTaskAsyncFacebook()
    {
        yield return LinkWithAuthDataTaskAsyncFacebookTask().ToEnumerator();
    }

    public async Task LinkWithAuthDataTaskAsyncFacebookTask()
    {
        // テストデータ作成
        NCMBUser user = new NCMBUser();
        user.AuthData = twitterParams.param;

        // authData登録
        await user.LogInWithAuthDataTaskAsync();

        // authData追加
        await user.LinkWithAuthDataTaskAsync(facebookParams.param);

        // 追加成功の確認
        Assert.IsNotEmpty(NCMBUser._getCurrentSessionToken());
        Assert.True(user.IsLinkWith("twitter"));
        Assert.True(user.IsLinkWith("facebook"));
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    /**
     * - 内容：LinkWithAuthDataAsyncがFacebookで無効なリクエストで失敗する事を確認する
     * - 結果：リンクしているauth dataがFacebookでないこと
     */
    [UnityTest]
    public IEnumerator LinkWithAuthDataTaskAsyncInvalidFacebook()
    {
        yield return LinkWithAuthDataTaskAsyncInvalidFacebookTask().ToEnumerator();
    }

    public async Task LinkWithAuthDataTaskAsyncInvalidFacebookTask()
    {
        // テストデータ作成
        NCMBUser user = new NCMBUser();
        user.AuthData = twitterParams.param;

        // authData登録
        await user.LogInWithAuthDataTaskAsync();

        try
        {
            // authData追加
            await user.LinkWithAuthDataTaskAsync(invalidFacebookParams.param);
            Assert.Fail("Exception not thrown");
        }
        catch (NCMBException e)
        {
            Assert.AreEqual(NCMBException.OAUTH_ERROR, e.ErrorCode);
        }

        // 追加失敗の確認
        Assert.IsNotEmpty(NCMBUser._getCurrentSessionToken());
        Assert.True(user.IsLinkWith("twitter"));
        Assert.False(user.IsLinkWith("facebook"));
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    /**
     * - 内容：LinkWithAuthDataAsyncがTwitterで成功する事を確認する
     * - 結果：リンクしているauth dataがTwitterであること
     */
    [UnityTest]
    public IEnumerator LinkWithAuthDataTaskAsyncTwitter()
    {
        yield return LinkWithAuthDataTaskAsyncTwitterTask().ToEnumerator();
    }

    public async Task LinkWithAuthDataTaskAsyncTwitterTask()
    {
        // テストデータ作成
        NCMBUser user = new NCMBUser();
        user.AuthData = facebookParams.param;

        // authData登録
        await user.LogInWithAuthDataTaskAsync();

        // authData追加
        await user.LinkWithAuthDataTaskAsync(twitterParams.param);

        // 追加成功の確認
        Assert.IsNotEmpty(NCMBUser._getCurrentSessionToken());
        Assert.True(user.IsLinkWith("facebook"));
        Assert.True(user.IsLinkWith("twitter"));
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    /**
     * - 内容：LinkWithAuthDataAsyncがTwitterで無効なリクエストで失敗する事を確認する
     * - 結果：リンクしているauth dataがTwitterでないこと
     */
    [UnityTest]
    public IEnumerator LinkWithAuthDataTaskAsyncInvalidTwitter()
    {
        yield return LinkWithAuthDataTaskAsyncInvalidTwitterTask().ToEnumerator();
    }

    public async Task LinkWithAuthDataTaskAsyncInvalidTwitterTask()
    {
        // テストデータ作成
        NCMBUser user = new NCMBUser();
        user.AuthData = facebookParams.param;

        // authData登録
        await user.LogInWithAuthDataTaskAsync();

        try
        {
            // authData追加
            await user.LinkWithAuthDataTaskAsync(invalidTwitterParams.param);
            Assert.Fail("Exception not thrown");
        }
        catch (NCMBException e)
        {
            Assert.AreEqual(NCMBException.OAUTH_ERROR, e.ErrorCode);
        }

        // 追加失敗の確認
        Assert.IsNotEmpty(NCMBUser._getCurrentSessionToken());
        Assert.True(user.IsLinkWith("facebook"));
        Assert.False(user.IsLinkWith("twitter"));
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    /**
     * - 内容：UnLinkがFacebookで成功する事を確認する
     * - 結果：リンクしているauth dataがFacebookでないこと
     */
    [UnityTest]
    public IEnumerator UnLinkWithAuthDataTaskAsyncFacebook()
    {
        yield return UnLinkWithAuthDataTaskAsyncFacebookTask().ToEnumerator();
    }


    public async Task UnLinkWithAuthDataTaskAsyncFacebookTask()
    {
        // テストデータ作成
        NCMBUser user = new NCMBUser();
        user.AuthData = facebookParams.param;

        // authData登録
        await user.LogInWithAuthDataTaskAsync();

        Assert.True(user.IsLinkWith("facebook"));

        // authData削除
        await user.UnLinkWithAuthDataTaskAsync("facebook");

        // 削除成功の確認
        Assert.False(user.IsLinkWith("facebook"));
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    /**
     * - 内容：UnLinkがTwitterで成功する事を確認する
     * - 結果：リンクしているauth dataがTwitterでないこと
     */
    [UnityTest]
    public IEnumerator UnLinkWithAuthDataTaskAsyncTwitter()
    {
        yield return UnLinkWithAuthDataTaskAsyncTwitterTask().ToEnumerator();
    }

    public async Task UnLinkWithAuthDataTaskAsyncTwitterTask()
    {
        // テストデータ作成
        NCMBUser user = new NCMBUser();
        user.AuthData = twitterParams.param;

        // authData登録
        await user.LogInWithAuthDataTaskAsync();

        Assert.True(user.IsLinkWith("twitter"));

        // authData削除
        await user.UnLinkWithAuthDataTaskAsync("twitter");

        // 削除成功の確認
        Assert.False(user.IsLinkWith("twitter"));
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    /**
     * - 内容：facebookでloginし、twitterでlinkした後、facebookがunlinkできるかを確認する
     * - 結果：auth dataにfacebookの値がないこと
     */
    [UnityTest]
    public IEnumerator UnLinkFacebookLinkTwitter()
    {
        yield return UnLinkFacebookLinkTwitterTask().ToEnumerator();
    }

    public async Task UnLinkFacebookLinkTwitterTask()
    {
        // テストデータ作成
        NCMBUser user = new NCMBUser();
        user.AuthData = facebookParams.param;

        // authData登録
        await user.LogInWithAuthDataTaskAsync();

        // authData追加
        await user.LinkWithAuthDataTaskAsync(twitterParams.param);

        Assert.True(user.IsLinkWith("twitter"));

        // authData削除
        await user.UnLinkWithAuthDataTaskAsync("facebook");

        // 削除成功の確認
        Assert.IsNotEmpty(NCMBUser._getCurrentSessionToken());
        Assert.True(user.IsLinkWith("twitter"));
        Assert.False(user.IsLinkWith("facebook"));
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    /**
     * - 内容：SignUpAsyncが成功する事を確認する
     * - 結果：各パラメータが正しく取得できること
     */
    [UnityTest]
    public IEnumerator SignUpTaskAsyncTest()
    {
        yield return SignUpTaskAsyncTestTask().ToEnumerator();
    }

    public async Task SignUpTaskAsyncTestTask()
    {
        // テストデータ作成
        NCMBUser user = new NCMBUser();
        user.UserName = "tarou";
        user.Password = "tarou";
        user.Email = "sample@example.com";

        // 会員登録
        var result = await user.SignUpTaskAsync();

        Assert.AreEqual("dummyObjectId", result.ObjectId);

        // 登録成功の確認
        Assert.IsNotEmpty(NCMBUser._getCurrentSessionToken());
        Assert.True(NCMBTestSettings.CallbackFlag);
    }

    /**
    * - 内容：LogInAsync
    * - 結果：各パラメータが正しく取得できること
    */
    [UnityTest]
    public IEnumerator LogInTaskAsync()
    {
        yield return LogInTaskAsyncTask().ToEnumerator();
    }


    public async Task LogInTaskAsyncTask()
    {
        // テストデータ作成
        var user = await NCMBUser.LogInTaskAsync("tarou", "tarou");

        // 登録成功の確認
        Assert.AreEqual("dummySessionToken", NCMBUser._getCurrentSessionToken());
        Assert.AreEqual("tarou", user.UserName);
        Assert.AreSame(user, NCMBUser.CurrentUser);
        Assert.True(NCMBTestSettings.CallbackFlag);
    }
}
#endif
