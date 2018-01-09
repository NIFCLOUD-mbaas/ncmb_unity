using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using System.Collections;
using System;
using System.Reflection;
using NUnit.Framework;
using NCMB;

public class NCMBRoleTest
{

	[SetUp]
	public void Init ()
	{
		NCMBTestSettings.Initialize ();
	}


	/**
     * - 内容：空ロール検索時にユーザーの追加ができる事を確認する
     * - 結果：追加されたユーザーをロールから取得し、ローカルのユーザーとobjectIdが一致すること
     */
	[UnityTest]
	public IEnumerator AddRoleUserTest ()
	{
		// ユーザー作成
		NCMBUser expertUser = new NCMBUser ();
		expertUser.UserName = "expertUser";
		expertUser.Password = "pass";
		expertUser.SignUpAsync ((error) => {
			if (error != null) {
				Assert.Fail (error.ErrorMessage);
			}
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;
		Assert.NotNull (expertUser.ObjectId);

		// ロール作成
		NCMBRole expertPlanRole = new NCMBRole ("expertPlan");
		expertPlanRole.SaveAsync ((error) => {
			if (error != null) {
				Assert.Fail (error.ErrorMessage);
			}
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;
		Assert.NotNull (expertPlanRole.ObjectId);

		// 空のロールを検索
		NCMBRole expertPlan = null;
		NCMBRole.GetQuery ().WhereEqualTo ("roleName", "expertPlan").FindAsync ((roleList, error) => {
			if (error != null) {
				Assert.Fail (error.ErrorMessage);
			} else {
				expertPlan = roleList.FirstOrDefault ();
			}
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;
		Assert.NotNull (expertPlan.ObjectId);

		// 空のロールにユーザーを追加
		expertPlan.Users.Add (expertUser);
		expertPlan.SaveAsync ((error) => {
			if (error != null) {
				Assert.Fail (error.ErrorMessage);
			}
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		// ロールを検索
		expertPlan = null;
		NCMBRole.GetQuery ().WhereEqualTo ("roleName", "expertPlan").FindAsync ((roleList, error) => {
			if (error != null) {
				Assert.Fail (error.ErrorMessage);
			} else {
				expertPlan = roleList.FirstOrDefault ();
			}
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		// ロールに所属するユーザーを検索
		expertPlan.Users.GetQuery ().FindAsync ((userList, error) => {
			if (error != null) {
				Assert.Fail (error.ErrorMessage);
			} else {
				Assert.AreEqual (expertUser.ObjectId, userList.FirstOrDefault ().ObjectId);
				NCMBTestSettings.CallbackFlag = true;		
			}
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：_getBaseUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void GetBaseUrlTest ()
	{
		// テストデータ作成
		NCMBRole expertPlanRole = new NCMBRole ("expertPlan");

		// internal methodの呼び出し
		MethodInfo method = expertPlanRole.GetType ().GetMethod ("_getBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/roles", method.Invoke (expertPlanRole, null).ToString ());
	}
}
