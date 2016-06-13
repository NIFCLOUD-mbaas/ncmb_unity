using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NCMB;

public class NCMBObjectTest : MonoBehaviour
{
	[TestFixtureSetUp]
	public void Init ()
	{
		NCMBTestSettings.Initialize ();
	}

	/**
     * - 内容：フィールドにダブルクウォーテーションを含む文字列の検索が成功することを確認する
     * - 結果：フィールドの値「"test"」が正しく取得できる事
     */
	[Test]
	public void doubleQuotationUnescapeTest ()
	{
		// テストデータ作成
		NCMBObject obj = new NCMBObject ("TestClass");
		obj ["key"] = "\"test\"";
		obj.SaveAsync ((NCMBException e) => {
			if (e != null) {
				Assert.Fail (e.ErrorMessage);
			}
		});
		NCMBTestSettings.AwaitAsync ();

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

		NCMBTestSettings.AwaitAsync ();
		Assert.True (NCMBTestSettings.CallbackFlag);

		// テストデータ削除
		obj.DeleteAsync ();
	}
}
