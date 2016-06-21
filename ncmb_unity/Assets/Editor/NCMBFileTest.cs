using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NCMB;
using NUnit.Framework;
using System.IO;
using System.Text;

public class NCMBFileTest : MonoBehaviour
{
	bool _callbackFlag = false;

	[TestFixtureSetUp]
	public void Init ()
	{
		//NCMBTestSettings.Initialize ();
		NCMBSettings.Initialize ("applicationKey", "clientKey");
		_callbackFlag = false;
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
	[Test]
	public void FileUploadTextTest ()
	{
		byte[] data = System.Text.Encoding.UTF8.GetBytes ("hello");
		NCMBFile file = new NCMBFile ("test.txt", data);
		file.SaveAsync ((NCMBException error) => {
			Assert.Null (error);
			_callbackFlag = true;
		});

		NCMBScriptTest.AwaitAsync ();
		Assert.NotNull (file.CreateDate);
		Assert.True (_callbackFlag);
	}

	/**
     * - 内容：ファイルのアップロードが成功することを確認する(画像)
     * - 結果：CreateDateが設定されていること
     */
	[Test]
	public void FileUploadImageTest ()
	{
		FileStream fileStream = new FileStream ("Assets/Editor/logo.png", FileMode.Open, FileAccess.Read);
		BinaryReader bin = new BinaryReader (fileStream);
		byte[] data = bin.ReadBytes ((int)bin.BaseStream.Length);
		bin.Close ();
		NCMBFile file = new NCMBFile ("logo.png", data);
		file.SaveAsync ((NCMBException error) => {
			Assert.Null (error);
			_callbackFlag = true;
		});

		NCMBScriptTest.AwaitAsync ();
		Assert.NotNull (file.CreateDate);
		Assert.True (_callbackFlag);
	}

	/**
     * - 内容：日本語ファイル名のアップロードが成功することを確認する
     * - 結果：CreateDateが設定されていること
     */
	[Test]
	public void FileUploadFileNameTest ()
	{		
		byte[] data = System.Text.Encoding.UTF8.GetBytes ("hello");
		NCMBFile file = new NCMBFile ("日本語.txt", data);
		file.SaveAsync ((NCMBException error) => {
			Assert.Null (error);
			_callbackFlag = true;
		});

		NCMBScriptTest.AwaitAsync ();
		Assert.NotNull (file.CreateDate);
		Assert.True (_callbackFlag);
	}

	/**
     * - 内容：ファイルのダウンロードが成功することを確認する
     * - 結果：FileDataが設定されていること
     */
	[Test]
	public void FileDownloadTest ()
	{		
		byte[] data = System.Text.Encoding.UTF8.GetBytes ("hello");
		NCMBFile file = new NCMBFile ("test.txt", data);
		file.SaveAsync ((NCMBException error) => {
			Assert.Null (error);
		});
		NCMBScriptTest.AwaitAsync ();

		NCMBFile getFile = new NCMBFile ("test.txt");
		getFile.FetchAsync ((byte[] fileData, NCMBException error) => {
			Assert.Null (error);
			_callbackFlag = true;
		});

		NCMBScriptTest.AwaitAsync ();
		Assert.NotNull (getFile.FileData);
		Assert.AreEqual ("hello", Encoding.UTF8.GetString (file.FileData));
		Assert.True (_callbackFlag);
	}

	/**
     * - 内容：ファイルの検索が成功することを確認する
     * - 結果：検索結果が1件以上であること
     */
	[Test]
	public void FileQueryTest ()
	{
		byte[] data = System.Text.Encoding.UTF8.GetBytes ("hello");
		NCMBFile file = new NCMBFile ("test.txt", data);
		file.SaveAsync ((NCMBException error) => {
			Assert.Null (error);
		});
		NCMBScriptTest.AwaitAsync ();
		Assert.NotNull (file.CreateDate);

		NCMBQuery<NCMBFile> query = NCMBFile.GetQuery ();
		query.FindAsync ((List<NCMBFile> objList, NCMBException error) => {
			Assert.LessOrEqual (1, objList.Count);
			Assert.Null (error);
			_callbackFlag = true;
		});
		NCMBScriptTest.AwaitAsync ();
		Assert.True (_callbackFlag);
	}

	/**
     * - 内容：ファイルの削除が成功することを確認する
     * - 結果：ファイルが削除されていること
     */
	[Test]
	public void FileDeleteTest ()
	{		
		byte[] data = System.Text.Encoding.UTF8.GetBytes ("delete test");
		NCMBFile file = new NCMBFile ("delete.txt", data);
		file.SaveAsync ((NCMBException error) => {
			Assert.Null (error);
		});
		NCMBScriptTest.AwaitAsync ();
		Assert.NotNull (file.CreateDate);

		file.DeleteAsync ((NCMBException error) => {
			Assert.Null (error);
		});
		NCMBScriptTest.AwaitAsync ();

		NCMBQuery<NCMBFile> query = NCMBFile.GetQuery ();
		query.WhereEqualTo ("fileName", "delete.txt");
		query.FindAsync ((List<NCMBFile> objList, NCMBException error) => {
			Assert.Null (error);
			Assert.AreEqual (0, objList.Count);
			_callbackFlag = true;
		});
		NCMBScriptTest.AwaitAsync ();
		Assert.True (_callbackFlag);
	}

	/**
	 * - 内容：ファイルにACLが正しく設定されていることを確認する
     * - 結果：ACLが正しく設定されていること
	 */
	[Test]
	public void FileACLTest ()
	{
		byte[] data = System.Text.Encoding.UTF8.GetBytes ("acl test");
		NCMBFile file = new NCMBFile ("ACL.txt", data);
		NCMBACL acl = new NCMBACL ();
		acl.PublicReadAccess = true;
		file.ACL = acl;
		file.SaveAsync ((NCMBException error) => {
		});
		NCMBScriptTest.AwaitAsync ();

		NCMBQuery<NCMBFile> query = NCMBFile.GetQuery ();
		query.WhereEqualTo ("fileName", "ACL.txt");
		query.FindAsync ((List<NCMBFile> objList, NCMBException error) => {
			Assert.Null (error);
			NCMBFile getFile = objList [0];
			Assert.True (getFile.ACL.PublicReadAccess);
			Assert.False (getFile.ACL.PublicWriteAccess);
			_callbackFlag = true;
		});
		NCMBScriptTest.AwaitAsync ();
		Assert.True (_callbackFlag);
	}
}
