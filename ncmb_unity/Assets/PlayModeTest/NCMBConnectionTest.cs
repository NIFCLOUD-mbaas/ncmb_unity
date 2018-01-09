using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using NCMB;
using System.Reflection;
using NCMB.Internal;
using System;

public class NCMBConnectionTest
{

	[SetUp]
	public void Init ()
	{
		NCMBTestSettings.Initialize ();
	}

	/**
     * - 内容：コンストラクタに正しく値が代入できることを確認する
     * - 結果：コンストラクタが正しく実行できる事
     */
	[Test]
	public void ConstructorTest ()
	{
		// テストデータ作成
		string url = "http://dummylocalhost/";
		ConnectType connectType = ConnectType.GET;
		string content = "dummyContent";
		string sessionToken = "dummySessionToken";
		NCMBFile file = new NCMBFile ();

		// フィールド値の読み込み
		NCMBConnection connection_normal = new NCMBConnection (url, connectType, content, sessionToken);
		Type type_normal = connection_normal.GetType ();
		FieldInfo field_normal = type_normal.GetField ("_domainUri", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

		NCMBConnection connection_file = new NCMBConnection (url, connectType, content, sessionToken, file);
		Type type_file = connection_file.GetType ();
		FieldInfo field_file = type_file.GetField ("_domainUri", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

		Assert.AreEqual ("http://localhost:3000/", field_normal.GetValue (connection_normal).ToString ());
		Assert.AreEqual ("http://localhost:3000/", field_file.GetValue (connection_file).ToString ());
	}
}
