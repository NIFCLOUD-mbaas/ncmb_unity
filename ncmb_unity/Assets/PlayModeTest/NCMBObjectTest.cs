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


    /**
     * - 内容：正規表現を使ってオブジェクトを検索する
     * - 結果：値が正しく設定されていること
     */
    [UnityTest]
    public IEnumerator FindObjectWithRegexTest()
    {
        // テストデータ作成
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("TestClass");
        Dictionary<string, string> regex = new Dictionary<string, string>()
        {
            {"$regex", "(?i)test"}
        };
        query.WhereEqualTo("key", regex);

        // テストデータ検索
        query.FindAsync((List<NCMBObject> list, NCMBException e) => {
            if (e == null)
            {
                Assert.AreEqual("\"test\"", list[0]["key"]);
            }
            else
            {
                Assert.Fail(e.ErrorMessage);
            }
            NCMBTestSettings.CallbackFlag = true;
        });
        yield return NCMBTestSettings.AwaitAsync();

        Assert.True(NCMBTestSettings.CallbackFlag);

        yield return null;
    }

    /**
     * - 内容：疑問符を含む文字列
     * - 結果：値が正しく設定されていること
     */
    [UnityTest]
    public IEnumerator FindObjectContainQuestionMarkTest()
    {
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("TestClass");
        query.WhereEqualTo("key", "test?");
        query.FindAsync((List<NCMBObject> list, NCMBException e) => {
            if (e == null)
            {
                Assert.AreEqual("test?", list[0]["key"]);
            }
            else
            {
                Assert.Fail(e.ErrorMessage);
            }
            NCMBTestSettings.CallbackFlag = true;
        });
        yield return NCMBTestSettings.AwaitAsync();

        Assert.True(NCMBTestSettings.CallbackFlag);

        yield return null;
    }
}