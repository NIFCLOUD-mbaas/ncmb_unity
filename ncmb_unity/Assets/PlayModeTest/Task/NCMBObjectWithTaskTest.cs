using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using NCMB;
using System.Threading.Tasks;
using NCMB.Tasks;

public class NCMBObjectWithTaskTest
{
    [SetUp]
    public void Init()
    {
        NCMBTestSettings.Initialize();
    }

    /**
     * - 内容：ダブルクォーテーションが含まれた文字列が正しく保存出来るか確認する
     * - 結果：値が正しく設定されていること
     */
    [UnityTest]
    public IEnumerator DoubleQuotationUnescapeTest()
    {
        yield return DoubleQuotationUnescape().ToEnumerator();
    }

    private async Task DoubleQuotationUnescape()
    {
        // テストデータ作成
        NCMBObject obj = new NCMBObject("TestClass");
        obj["key"] = "\"test\"";

        await obj.SaveTaskAsync();

        // テストデータ検索
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("TestClass");
        query.WhereEqualTo("objectId", obj.ObjectId);

        var list = await query.FindTaskAsync();

        Assert.AreEqual("\"test\"", list[0]["key"]);
    }

}