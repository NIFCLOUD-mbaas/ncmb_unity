using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NCMB;

public class NCMBObjectTest : MonoBehaviour
{

	static readonly string _appKey = "Set the test appKey";
	static readonly string _clientKey = "Set the test clientKey";
	bool _callbackFlag = false;

	[TestFixtureSetUp]
	public void Init ()
	{
		NCMBSettings.Initialize (
			_appKey,
			_clientKey
		);
		_callbackFlag = false;
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
		NCMBScriptTest.AwaitAsync ();

		// テストデータ検索
		NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject> ("TestClass");
		query.WhereEqualTo ("objectId", obj.ObjectId);
		query.FindAsync ((List<NCMBObject> list, NCMBException e) => {
			if (e == null) {
				Assert.AreEqual ("\"test\"", list [0] ["key"]);
			} else {
				Assert.Fail (e.ErrorMessage);
			}
			_callbackFlag = true;
		});

		NCMBScriptTest.AwaitAsync ();
		Assert.True (_callbackFlag);

		// テストデータ削除
		obj.DeleteAsync ();
	}
}
