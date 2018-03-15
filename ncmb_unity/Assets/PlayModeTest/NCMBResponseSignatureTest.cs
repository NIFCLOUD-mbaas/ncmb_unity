using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using NCMB;
using System.Reflection;

public class NCMBResponseSignatureTest
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
    public IEnumerator DoubleQuoteInData()
    {
        // テストデータ検索
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("ABC");
        query.WhereEqualTo("objectId", "eFyOet7e3rOVLD1Z");
        query.FindAsync((List<NCMBObject> list, NCMBException e) => {
            if (e == null)
            {
                Assert.AreEqual("\"value\"", list[0]["name"]);
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
     * - 内容：ダブルクォーテーションが含まれた文字列が正しく保存出来るか確認する
     * - 結果：値が正しく設定されていること
     */
    [UnityTest]
    public IEnumerator EmojiInData()
    {
        // テストデータ検索
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("ABC");
        query.WhereEqualTo("objectId", "cuvYjyyLzRzXoqm5");
        query.FindAsync((List<NCMBObject> list, NCMBException e) => {
            if (e == null)
            {
                Assert.AreEqual("\uD83D\uDE03", list[0]["name"]);
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
     * - 内容：ダブルクォーテーションが含まれた文字列が正しく保存出来るか確認する
     * - 結果：値が正しく設定されていること
     */
    [UnityTest]
    public IEnumerator DoubleQuoteAndEmojiInData()
    {
        // テストデータ検索
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("ABC");
        query.WhereEqualTo("objectId", "WvFit1DQ68qDC6E4");
        query.FindAsync((List<NCMBObject> list, NCMBException e) => {
            if (e == null)
            {
                Assert.AreEqual("\"test\"\uD83D\uDE03", list[0]["name"]);
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