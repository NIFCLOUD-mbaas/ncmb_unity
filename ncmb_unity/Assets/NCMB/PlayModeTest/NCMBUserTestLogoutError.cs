using UnityEngine.TestTools;
using NUnit.Framework;
using NCMB;
using System;
using System.Collections;

public class NCMBUserTestLogoutError
{
	[SetUp]
	public void Init ()
	{
        NCMBTestSettings.Initialize ("NCMB/PlayModeTest/mbaasErrors.yaml");
	}
	/**
	* - 内容：LogoutAsyncError
	*/
	[UnityTest]
	public IEnumerator LogoutAsyncError()
	{
		NCMBTestSettings.CallbackFlag = false;
		// テストデータ作成
		NCMBUser.LogInAsync("tarou", "tarou", (e) => {
			Assert.Null(e);
			NCMBTestSettings.CallbackFlag = true;
		});

		yield return NCMBTestSettings.AwaitAsync();

		NCMBTestSettings.CallbackFlag = false;
		NCMBUser.LogOutAsync((e) =>
		{
			Assert.NotNull(e);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync();
		Assert.True(NCMBTestSettings.CallbackFlag);
	}
}