using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NCMB;
using System.Reflection;
using UnityEngine.TestTools;

public class NCMBPushTest
{
	[SetUp]
	public void Init ()
	{
		NCMBTestSettings.Initialize ();
	}

	/**
     * - 内容：絞り込み条件が正しく設定されている事を確認する
     * - 結果：絞り込み条件が正しく取得できる事
     */
	[Test]
	public void searchConditionTest ()
	{
		// 絞り込み条件
		Dictionary<string, IDictionary> searchCondition = new Dictionary<string, IDictionary> () { { "score", new Dictionary<string, int> () {
					{ "$gte", 1000 },
					{ "$lte", 3000 }
				}
			}
		};
		
		NCMBPush push = new NCMBPush ();
		push.SearchCondition = searchCondition;
		
		Assert.AreEqual (1000, ((IDictionary)((IDictionary)push.SearchCondition) ["score"]) ["$gte"]);
		Assert.AreEqual (3000, ((IDictionary)((IDictionary)push.SearchCondition) ["score"]) ["$lte"]);
	}

	/**
     * - 内容：_getBaseUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void GetBaseUrlTest ()
	{
		// テストデータ作成
		NCMBPush push = new NCMBPush ();
		
		// internal methodの呼び出し
		MethodInfo method = push.GetType ().GetMethod ("_getBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance);
		
		Assert.AreEqual ("http://localhost:3000/2013-09-01/push", method.Invoke (push, null).ToString ());
	}

    [UnityTest]
    public IEnumerator FetchAsyncAuthenticationError()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) => {
            Assert.Null(e);

            NCMBUser.CurrentUser.SessionToken = "invalidToken";
            NCMBUser.CurrentUser._currentOperations.Clear();

            NCMBPush push = new NCMBPush();
            push.ObjectId = "pushDummyObjectId";

            push.FetchAsync((NCMBException ex) =>
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
        NCMBPush push = new NCMBPush();
        push.ObjectId = "pushInvalidObjectId";

        push.FetchAsync((NCMBException ex) =>
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