using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using NCMB;
using System.Reflection;
using System;
using MiniJSON;
using System.Collections.Generic;
using System.Collections;

public class NCMBAnonymousTest
{
	[SetUp]
	public void Init ()
	{
		NCMBTestSettings.Initialize ();
	}

    /**
     * - 内容：update curent user info after loging in by anonymous user, 
     * - 結果：current user info has updated
     */
    [UnityTest]
    public IEnumerator UpdateCurrentUser()
    {
        // テストデータ作成
        NCMBUser anonymousUser = new NCMBUser();
        Dictionary<string, object> param = new Dictionary<string, object>();
        Dictionary<string, object> anonymousParam = new Dictionary<string, object>() {
                { "id",  "anonymousId"}
            };
        param.Add("anonymous", anonymousParam);

        anonymousUser.AuthData = param;
        anonymousUser.SignUpAsync((NCMBException e) => {
            Assert.Null(e);
            Assert.NotNull(NCMBUser.CurrentUser);

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
        Assert.IsTrue(NCMBUser.CurrentUser.IsLinkWith("anonymous"));
    }

    /**
     * - 内容：create a new user after loging in by anonymous user, 
     * - 結果：current user has no change
     */
    [UnityTest]
    public IEnumerator CreateNewUser()
    {
        // テストデータ作成
        NCMBUser anonymousUser = new NCMBUser();
        Dictionary<string, object> param = new Dictionary<string, object>();
        Dictionary<string, object> anonymousParam = new Dictionary<string, object>() {
                { "id",  "anonymousId"}
            };
        param.Add("anonymous", anonymousParam);

        anonymousUser.AuthData = param;
        anonymousUser.SignUpAsync((NCMBException e) => {
            Assert.Null(e);
            Assert.NotNull(NCMBUser.CurrentUser);
            Assert.IsTrue(NCMBUser.CurrentUser.IsLinkWith("anonymous"));
            NCMBTestSettings.CallbackFlag = true;
        });

        yield return NCMBTestSettings.AwaitAsync();
        NCMBTestSettings.CallbackFlag = false;

        NCMBUser user = new NCMBUser();
        user.ObjectId = "anotherObjectId";
        user.UserName = "newUserName";
        user.SignUpAsync((NCMBException e1) =>
        {
            Assert.Null(e1);
            NCMBTestSettings.CallbackFlag = true;

        });

        yield return NCMBTestSettings.AwaitAsync();
        Assert.NotNull(NCMBUser.CurrentUser);
        Assert.IsTrue(NCMBUser.CurrentUser.IsLinkWith("anonymous"));
    }

    /**
     * - 内容：delete current user after loging in by anonymous user, 
     * - 結果：current user is null
     */
    [UnityTest]
    public IEnumerator DeleteCurrentUser()
    {
        // テストデータ作成
        NCMBUser anonymousUser = new NCMBUser();
        Dictionary<string, object> param = new Dictionary<string, object>();
        Dictionary<string, object> anonymousParam = new Dictionary<string, object>() {
                { "id",  "anonymousId"}
            };
        param.Add("anonymous", anonymousParam);

        anonymousUser.AuthData = param;
        anonymousUser.SignUpAsync((NCMBException e) => {
            Assert.Null(e);
            Assert.NotNull(NCMBUser.CurrentUser);
            Assert.IsTrue(NCMBUser.CurrentUser.IsLinkWith("anonymous"));
            NCMBTestSettings.CallbackFlag = true;
        });

        yield return NCMBTestSettings.AwaitAsync();
        NCMBTestSettings.CallbackFlag = false;

        NCMBUser.CurrentUser.DeleteAsync((NCMBException e3) => {
            Assert.Null(e3);
            NCMBTestSettings.CallbackFlag = true;
        });
        yield return NCMBTestSettings.AwaitAsync();
        Assert.Null(NCMBUser.CurrentUser);
    }

    /**
     * - 内容：create datastore object after loging in by anonymous user, 
     * - 結果：current user has no change
     */
    [UnityTest]
    public IEnumerator CreateObject()
    {
        // テストデータ作成
        NCMBUser anonymousUser = new NCMBUser();
        Dictionary<string, object> param = new Dictionary<string, object>();
        Dictionary<string, object> anonymousParam = new Dictionary<string, object>() {
                { "id",  "anonymousId"}
            };
        param.Add("anonymous", anonymousParam);

        anonymousUser.AuthData = param;
        anonymousUser.SignUpAsync((NCMBException e) => {
            Assert.Null(e);
            Assert.NotNull(NCMBUser.CurrentUser);
            Assert.IsTrue(NCMBUser.CurrentUser.IsLinkWith("anonymous"));
            NCMBTestSettings.CallbackFlag = true;
        });

        yield return NCMBTestSettings.AwaitAsync();
        NCMBTestSettings.CallbackFlag = false;

        NCMBObject obj = new NCMBObject("TestClass");
        obj["key"] = "\"test\"";
        obj.SaveAsync((NCMBException e) => {
            if (e != null)
            {
                Assert.Fail(e.ErrorMessage);
            }
            NCMBTestSettings.CallbackFlag = true;
        });

        yield return NCMBTestSettings.AwaitAsync();
        Assert.NotNull(NCMBUser.CurrentUser);
        Assert.IsTrue(NCMBUser.CurrentUser.IsLinkWith("anonymous"));
    }

    /**
     * - 内容：update datastore object after loging in by anonymous user, 
     * - 結果：current user has no change
     */
    [UnityTest]
    public IEnumerator UpdateObject()
    {
        // テストデータ作成
        NCMBUser anonymousUser = new NCMBUser();
        Dictionary<string, object> param = new Dictionary<string, object>();
        Dictionary<string, object> anonymousParam = new Dictionary<string, object>() {
                { "id",  "anonymousId"}
            };
        param.Add("anonymous", anonymousParam);

        anonymousUser.AuthData = param;
        anonymousUser.SignUpAsync((NCMBException e) => {
            Assert.Null(e);
            Assert.NotNull(NCMBUser.CurrentUser);
            Assert.IsTrue(NCMBUser.CurrentUser.IsLinkWith("anonymous"));

            NCMBObject obj = new NCMBObject("TestClass");
            obj.ObjectId = "dummyObjectId";
            obj.Add("key", "newValue");
            obj.SaveAsync((NCMBException ex) => {
                Assert.Null(ex);
                NCMBTestSettings.CallbackFlag = true;
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        Assert.NotNull(NCMBUser.CurrentUser);
        Assert.IsTrue(NCMBUser.CurrentUser.IsLinkWith("anonymous"));
    }

    /**
     * - 内容：delete datastore object after loging in by anonymous user, 
     * - 結果：current user has no change
     */
    [UnityTest]
    public IEnumerator DeleteObject()
    {
        // テストデータ作成
        NCMBUser anonymousUser = new NCMBUser();
        Dictionary<string, object> param = new Dictionary<string, object>();
        Dictionary<string, object> anonymousParam = new Dictionary<string, object>() {
                { "id",  "anonymousId"}
            };
        param.Add("anonymous", anonymousParam);

        anonymousUser.AuthData = param;
        anonymousUser.SignUpAsync((NCMBException e) => {
            Assert.Null(e);
            Assert.NotNull(NCMBUser.CurrentUser);
            Assert.IsTrue(NCMBUser.CurrentUser.IsLinkWith("anonymous"));

            NCMBObject obj = new NCMBObject("TestClass");
            obj.ObjectId = "dummyObjectId";
            obj.DeleteAsync((NCMBException ex) => {
                Assert.Null(ex);

                NCMBTestSettings.CallbackFlag = true;
            });
        });

        yield return NCMBTestSettings.AwaitAsync();
        Assert.NotNull(NCMBUser.CurrentUser);
        Assert.IsTrue(NCMBUser.CurrentUser.IsLinkWith("anonymous"));
    }

}