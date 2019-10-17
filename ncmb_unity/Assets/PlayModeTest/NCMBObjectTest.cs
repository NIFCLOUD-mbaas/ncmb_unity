using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using NCMB;
using System.Reflection;

public class NCMBObjectTest
{
	[SetUp]
	public void Init ()
	{
		NCMBTestSettings.Initialize ();
	}

	/**
     * - 内容：ダブルクォーテーションが含まれた文字列が正しく保存出来るか確認する
     * - 結果：値が正しく設定されていること
     */
	[UnityTest]
	public IEnumerator DoubleQuotationUnescapeTest ()
	{
		// テストデータ作成
		NCMBObject obj = new NCMBObject ("TestClass");
		obj ["key"] = "\"test\"";
		obj.SaveAsync ((NCMBException e) => {
			if (e != null) {
				Assert.Fail (e.ErrorMessage);
			}
			NCMBTestSettings.CallbackFlag = true;
		});
	
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		// テストデータ検索
		NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject> ("TestClass");
		query.WhereEqualTo ("objectId", obj.ObjectId);
		query.FindAsync ((List<NCMBObject> list, NCMBException e) => {
			if (e == null) {
				Assert.AreEqual ("\"test\"", list [0] ["key"]);
			} else {
				Assert.Fail (e.ErrorMessage);
			}
			NCMBTestSettings.CallbackFlag = true;
		});
	
		yield return NCMBTestSettings.AwaitAsync ();
	
		Assert.True (NCMBTestSettings.CallbackFlag);
	
		yield return null;
	}

	/**
     * - 内容：_getBaseUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void GetBaseUrlTest ()
	{
		// テストデータ作成
		NCMBObject obj = new NCMBObject ("TestClass");
		// internal methodの呼び出し
		MethodInfo method = obj.GetType ().GetMethod ("_getBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.AreEqual ("http://localhost:3000/2013-09-01/classes/TestClass", method.Invoke (obj, null).ToString ());
	}

    [UnityTest]
    public IEnumerator FetchAsyncAuthenticationError()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) => {
            Assert.Null(e);

            NCMBUser.CurrentUser.SessionToken = "invalidToken";
            NCMBUser.CurrentUser._currentOperations.Clear();

            NCMBObject testObject = new NCMBObject("testclass");
            testObject.ObjectId = "testclassDummyObjectId";

            testObject.FetchAsync((NCMBException ex) =>
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
    public IEnumerator FetchAsyncDataNotAvailable()
    {
        // テストデータ作成
        NCMBObject testObject = new NCMBObject("testclass");
        testObject.ObjectId = "testclassInvalidObjectId";

        testObject.FetchAsync((NCMBException ex) =>
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


    [UnityTest]
    public IEnumerator FetchObjectAfterLogin()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) => {
            Assert.Null(e);
            NCMBObject obj = new NCMBObject("TestClass");
            obj.ObjectId = "testclassDummyObjectId";
            obj.FetchAsync((NCMBException ex) => {
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
    public IEnumerator AddObjectAfterLogin()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) => {
            Assert.Null(e);
            NCMBObject obj = new NCMBObject("TestClass");
            obj.Add("key", "value");
            obj.SaveAsync((NCMBException ex) => {
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
    public IEnumerator UpdateObjectAfterLogin()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) => {
            Assert.Null(e);
            NCMBObject obj = new NCMBObject("TestClass");
            obj.ObjectId = "dummyObjectId";
            obj.Add("key", "newValue");
            obj.SaveAsync((NCMBException ex) => {
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
    public IEnumerator DeleteObjectAfterLogin()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) => {
            Assert.Null(e);
            NCMBObject obj = new NCMBObject("TestClass");
            obj.ObjectId = "dummyObjectId";
            obj.DeleteAsync((NCMBException ex) => {
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



}