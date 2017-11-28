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



}