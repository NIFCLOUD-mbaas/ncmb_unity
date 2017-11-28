using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using NCMB;
using NUnit.Framework;
using System.IO;
using System.Text;
using System.Reflection;

public class NCMBFileTest
{

	[SetUp]
	public void Init ()
	{
		NCMBTestSettings.Initialize ();
	}

	/**
     * - 内容：コンストラクターに設定したファイル名が取得できること
     * - 結果：ファイル名が正しく取得できること
     */
	[Test]
	public void ConstructorArgumentOneTest ()
	{
		string fileName = "test.txt";
		NCMBFile file = new NCMBFile (fileName);

		Assert.AreEqual (fileName, file.FileName);
	}

	/**
     * - 内容：コンストラクターに設定したファイル名およびファイルデータが取得できること
     * - 結果：ファイル名およびファイルデータが正しく取得できること
     */
	[Test]
	public void ConstructorArgumentTwoTest ()
	{
		string fileName = "test.txt";
		byte[] fileData = System.Text.Encoding.UTF8.GetBytes ("hello");
		NCMBFile file = new NCMBFile (fileName, fileData);

		Assert.AreEqual (fileName, file.FileName);
		Assert.AreEqual (fileData, file.FileData);
	}

	/**
     * - 内容：コンストラクターに設定したファイル名、ACLおよびファイルデータが取得できること
     * - 結果：ファイル名、ACLおよびファイルデータが正しく取得できること
     */
	[Test]
	public void ConstructorArgumentThreeTest ()
	{
		string fileName = "test.txt";
		byte[] fileData = System.Text.Encoding.UTF8.GetBytes ("hello");
		NCMBACL acl = new NCMBACL ();
		acl.PublicWriteAccess = false;
		NCMBFile file = new NCMBFile (fileName, fileData, acl);

		Assert.AreEqual (fileName, file.FileName);
		Assert.AreEqual (fileData, file.FileData);
		Assert.AreEqual (false, file.ACL.PublicWriteAccess);
	}

	/**
     * - 内容：ファイルのアップロードが成功することを確認する(テキスト)
     * - 結果：CreateDateが設定されていること
     */
	[UnityTest]
	public IEnumerator FileUploadTextTest ()
	{
		byte[] data = System.Text.Encoding.UTF8.GetBytes ("hello");
		NCMBFile file = new NCMBFile ("test.txt", data);
		file.SaveAsync ((NCMBException error) => {
			Assert.Null (error);
			NCMBTestSettings.CallbackFlag = true;
		});

		yield return NCMBTestSettings.AwaitAsync ();
		Assert.NotNull (file.CreateDate);
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：ファイルのアップロードが成功することを確認する(画像)
     * - 結果：CreateDateが設定されていること
     */
	[UnityTest]
	public IEnumerator FileUploadImageTest ()
	{
		FileStream fileStream = new FileStream ("Assets/PlayModeTest/logo.png", FileMode.Open, FileAccess.Read);
		BinaryReader bin = new BinaryReader (fileStream);
		byte[] data = bin.ReadBytes ((int)bin.BaseStream.Length);
		bin.Close ();
		NCMBFile file = new NCMBFile ("logo.png", data);
		file.SaveAsync ((NCMBException error) => {
			Assert.Null (error);
			NCMBTestSettings.CallbackFlag = true;
		});

		yield return NCMBTestSettings.AwaitAsync ();
		Assert.NotNull (file.CreateDate);
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：日本語ファイル名のアップロードが成功することを確認する
     * - 結果：CreateDateが設定されていること
     */
	[UnityTest]
	public IEnumerator FileUploadFileNameTest ()
	{		
		byte[] data = System.Text.Encoding.UTF8.GetBytes ("hello");
		NCMBFile file = new NCMBFile ("日本語.txt", data);
		file.SaveAsync ((NCMBException error) => {
			Assert.Null (error);
			NCMBTestSettings.CallbackFlag = true;
		});

		yield return NCMBTestSettings.AwaitAsync ();
		Assert.NotNull (file.CreateDate);
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：ファイルのダウンロードが成功することを確認する
     * - 結果：FileDataが設定されていること
     */
	[UnityTest]
	public IEnumerator FileDownloadTest ()
	{		
		byte[] data = System.Text.Encoding.UTF8.GetBytes ("hello");
		NCMBFile file = new NCMBFile ("test.txt", data);
		file.SaveAsync ((NCMBException error) => {
			Assert.Null (error);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		NCMBFile getFile = new NCMBFile ("test.txt");
		getFile.FetchAsync ((byte[] fileData, NCMBException error) => {
			Assert.Null (error);
			Assert.AreEqual ("hello", Encoding.UTF8.GetString (fileData));
			NCMBTestSettings.CallbackFlag = true;
		});

		yield return NCMBTestSettings.AwaitAsync ();
		Assert.NotNull (getFile.FileData);
		Assert.AreEqual ("hello", Encoding.UTF8.GetString (getFile.FileData));
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：ファイルの検索が成功することを確認する
     * - 結果：検索結果が1件以上であること
     */
	[UnityTest]
	public IEnumerator FileQueryTest ()
	{
		byte[] data = System.Text.Encoding.UTF8.GetBytes ("hello");
		NCMBFile file = new NCMBFile ("test.txt", data);
		file.SaveAsync ((NCMBException error) => {
			Assert.Null (error);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;
		Assert.NotNull (file.CreateDate);

		NCMBQuery<NCMBFile> query = NCMBFile.GetQuery ();
		query.FindAsync ((List<NCMBFile> objList, NCMBException error) => {
			Assert.LessOrEqual (1, objList.Count);
			Assert.Null (error);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：ファイルの削除が成功することを確認する
     * - 結果：ファイルが削除されていること
     */
	[UnityTest]
	public IEnumerator FileDeleteTest ()
	{		
		byte[] data = System.Text.Encoding.UTF8.GetBytes ("delete test");
		NCMBFile file = new NCMBFile ("delete.txt", data);
		file.SaveAsync ((NCMBException error) => {
			Assert.Null (error);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;
		Assert.NotNull (file.CreateDate);

		file.DeleteAsync ((NCMBException error) => {
			Assert.Null (error);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		NCMBQuery<NCMBFile> query = NCMBFile.GetQuery ();
		query.WhereEqualTo ("fileName", "delete.txt");
		query.FindAsync ((List<NCMBFile> objList, NCMBException error) => {
			Assert.Null (error);
			Assert.AreEqual (0, objList.Count);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
	 * - 内容：ファイルにACLが正しく設定されていることを確認する
     * - 結果：ACLが正しく設定されていること
	 */
	[UnityTest]
	public IEnumerator FileACLTest ()
	{
		byte[] data = System.Text.Encoding.UTF8.GetBytes ("acl test");
		NCMBACL acl = new NCMBACL ();
		acl.PublicReadAccess = true;
		NCMBFile file = new NCMBFile ("ACL.txt", data, acl);
		file.SaveAsync ((NCMBException error) => {
			NCMBTestSettings.CallbackFlag = true;
		});

		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		NCMBQuery<NCMBFile> query = NCMBFile.GetQuery ();
		query.WhereEqualTo ("fileName", "ACL.txt");
		query.FindAsync ((List<NCMBFile> objList, NCMBException error) => {
			Assert.Null (error);
			NCMBFile getFile = objList [0];
			Assert.True (getFile.ACL.PublicReadAccess);
			Assert.False (getFile.ACL.PublicWriteAccess);
			NCMBTestSettings.CallbackFlag = true;
		});

		yield return NCMBTestSettings.AwaitAsync ();

		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	/**
     * - 内容：レスポンスシグネチャの検証が成功することを確認する
     * - 結果：エラーが発生しないこと
     */
	[UnityTest]
	public IEnumerator FileResponseSignatureTest ()
	{
		NCMBSettings.EnableResponseValidation (true);

		byte[] data = System.Text.Encoding.UTF8.GetBytes ("hello");
		NCMBFile file = new NCMBFile ("test.txt", data);
		file.SaveAsync ((NCMBException error) => {
			Assert.Null (error);
			NCMBTestSettings.CallbackFlag = true;
		});
		yield return NCMBTestSettings.AwaitAsync ();
		NCMBTestSettings.CallbackFlag = false;

		NCMBFile getFile = new NCMBFile ("test.txt");
		getFile.FetchAsync ((byte[] fileData, NCMBException error) => {
			Assert.Null (error);
			NCMBTestSettings.CallbackFlag = true;
		});


		yield return NCMBTestSettings.AwaitAsync ();
		Assert.NotNull (file.CreateDate);
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
		string fileName = "test.txt";
		NCMBFile file = new NCMBFile (fileName);

		// internal methodの呼び出し
		MethodInfo method = file.GetType ().GetMethod ("_getBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.AreEqual ("http://localhost:3000/2013-09-01/files/test.txt", method.Invoke (file, null).ToString ());
	}
}
