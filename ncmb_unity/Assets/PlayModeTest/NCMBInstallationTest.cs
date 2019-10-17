using UnityEngine;
using NUnit.Framework;
using NCMB;
using System.Reflection;
using UnityEngine.TestTools;
using System.Collections;

public class NCMBInstallationTest
{

	[SetUp]
	public void Init ()
	{
		NCMBTestSettings.Initialize ();
	}

	/**
     * - 内容：_getBaseUrlが返すURLが正しいことを確認する
     * - 結果：返り値のURLが正しく取得できる事
     */
	[Test]
	public void GetBaseUrlTest ()
	{
		// テストデータ作成
		NCMBInstallation installation = new NCMBInstallation ();

		// internal methodの呼び出し
		MethodInfo method = installation.GetType ().GetMethod ("_getBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/installations", method.Invoke (installation, null).ToString ());
	}

    [UnityTest]
    public IEnumerator FetchAsyncAuthenticationError()
    {
        // テストデータ作成
        NCMBUser.LogInAsync("tarou", "tarou", (e) => {
            Assert.Null(e);

            NCMBUser.CurrentUser.SessionToken = "invalidToken";
            NCMBUser.CurrentUser._currentOperations.Clear();

            NCMBInstallation installation = new NCMBInstallation();
            installation.ObjectId = "instllDummyObjectId";

            installation.FetchAsync((NCMBException ex) =>
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
        NCMBInstallation installation = new NCMBInstallation();
        installation.ObjectId = "instllInvalidObjectId";

        installation.FetchAsync((NCMBException ex) =>
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
