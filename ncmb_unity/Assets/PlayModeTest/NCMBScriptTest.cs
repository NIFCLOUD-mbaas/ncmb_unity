/*******
 Copyright 2017 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at
 
 http://www.apache.org/licenses/LICENSE-2.0
 
 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
 **********/

using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using NUnit.Framework;
using NCMB;
using NCMB.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.Threading;
using System.Reflection;


public class NCMBScriptTest
{
	//node.jsのエンドポイント
	static readonly string _endPoint = "http://localhost:3000/2015-09-01/script";

	delegate void AsyncDelegate ();

	[SetUp]
	public void Init ()
	{
		NCMBTestSettings.Initialize ();
	}

	[Test]
	public void ReturnAPIKeyTest ()
	{
		Assert.AreEqual (NCMBSettings.ApplicationKey, NCMBTestSettings.APP_KEY);
		Assert.AreEqual (NCMBSettings.ClientKey, NCMBTestSettings.CLIENT_KEY);
	}

	[UnityTest]
	public IEnumerator ExecuteCallbackWhenExecuteScriptTest_POST ()
	{
		NCMBScript script = new NCMBScript ("testScript_POST.js", NCMBScript.MethodType.POST, _endPoint);
		Dictionary<string, object> body = new Dictionary<string, object> (){ { "name", "tarou" } };
		script.ExecuteAsync (null, body, null, (byte[] result, NCMBException e) => {
			if (e == null) {
				string cmd = System.Text.Encoding.UTF8.GetString (result);
				cmd = cmd.TrimEnd ('\0');//終端文字の削除
				Assert.AreEqual ("hello,tarou", cmd);
			} else {
				Assert.Fail (e.ErrorMessage);
			}
			NCMBTestSettings.CallbackFlag = true;
		}); 
		yield return NCMBTestSettings.AwaitAsync ();
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	[UnityTest]
	public IEnumerator ExecuteCallbackWhenExecuteScriptTest_PUT ()
	{
		NCMBScript script = new NCMBScript ("testScript_PUT.js", NCMBScript.MethodType.PUT, _endPoint);
		Dictionary<string, object> body = new Dictionary<string, object> (){ { "name", "tarou" } };
		script.ExecuteAsync (null, body, null, (byte[] result, NCMBException e) => {
			if (e == null) {
				string cmd = System.Text.Encoding.UTF8.GetString (result);
				cmd = cmd.TrimEnd ('\0');//終端文字の削除
				Assert.AreEqual ("hello,tarou", cmd);
			} else {
				Assert.Fail (e.ErrorMessage);
			}
			NCMBTestSettings.CallbackFlag = true;
		}); 
			
		yield return NCMBTestSettings.AwaitAsync ();
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	[UnityTest]
	public IEnumerator ExecuteCallbackWhenExecuteScriptTest_GET ()
	{
		NCMBScript script = new NCMBScript ("testScript_GET.js", NCMBScript.MethodType.GET, _endPoint);
		Dictionary<string, object> query = new Dictionary<string, object> (){ { "name", "tarou" } };
		script.ExecuteAsync (null, null, query, (byte[] result, NCMBException e) => {
			if (e == null) {
				string cmd = System.Text.Encoding.UTF8.GetString (result);
				cmd = cmd.TrimEnd ('\0');//終端文字の削除
				Assert.AreEqual ("hello,tarou", cmd);
			} else {
				Assert.Fail (e.ErrorMessage);
			}
			NCMBTestSettings.CallbackFlag = true;
		}); 

		yield return NCMBTestSettings.AwaitAsync ();
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	[UnityTest]
	public IEnumerator ExecuteCallbackWhenExecuteScriptTest_GET_Count ()
	{
		NCMBScript script = new NCMBScript ("testScript_GET.js", NCMBScript.MethodType.GET, _endPoint);
		Dictionary<string, object> query = new Dictionary<string, object> (){ { "name", "tarou" }, { "message","hello" } };
		script.ExecuteAsync (null, null, query, (byte[] result, NCMBException e) => {
			if (e == null) {
				string cmd = System.Text.Encoding.UTF8.GetString (result);
				cmd = cmd.TrimEnd ('\0');//終端文字の削除
				Assert.AreEqual ("{\"count:2\"}", cmd);
			} else {
				Assert.Fail (e.ErrorMessage);
			}
			NCMBTestSettings.CallbackFlag = true;
		}); 

		yield return NCMBTestSettings.AwaitAsync ();
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	[UnityTest]
	public IEnumerator ExecuteCallbackWhenExecuteScriptTest_DELETE ()
	{
		NCMBScript script = new NCMBScript ("testScript_DELETE.js", NCMBScript.MethodType.DELETE, _endPoint);
		script.ExecuteAsync (null, null, null, (byte[] result, NCMBException e) => {
			if (e == null) {
				string cmd = System.Text.Encoding.UTF8.GetString (result);
				cmd = cmd.TrimEnd ('\0');//終端文字の削除
				Assert.AreEqual ("", cmd);
			} else {
				Assert.Fail (e.ErrorMessage);
			}
			NCMBTestSettings.CallbackFlag = true;
		}); 

		yield return NCMBTestSettings.AwaitAsync ();
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	[UnityTest]
	public IEnumerator ExecuteCallbackWhenExecuteScriptTest_Error ()
	{
		NCMBScript script = new NCMBScript ("testScript_Error.js", NCMBScript.MethodType.GET, _endPoint);
		script.ExecuteAsync (null, null, null, (byte[] result, NCMBException e) => {
			if (e == null) {
				Assert.Fail ("Always test case to fail.");
			} else {
				Assert.AreEqual ("name must not be null.", e.ErrorMessage);
				Assert.AreEqual (HttpStatusCode.BadRequest.ToString (), e.ErrorCode);
			}
			NCMBTestSettings.CallbackFlag = true;
		}); 

		yield return NCMBTestSettings.AwaitAsync ();
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	[UnityTest]
	public IEnumerator ExecuteCallbackWhenExecuteScriptTest_Header ()
	{
		NCMBScript script = new NCMBScript ("testScript_Header.js", NCMBScript.MethodType.POST, _endPoint);
		Dictionary<string, object> header = new Dictionary<string, object> (){ { "key", "value" } };
		script.ExecuteAsync (header, null, null, (byte[] result, NCMBException e) => {
			if (e == null) {
				string cmd = System.Text.Encoding.UTF8.GetString (result);
				cmd = cmd.TrimEnd ('\0');//終端文字の削除
				Assert.AreEqual ("value", cmd);
			} else {
				Assert.Fail (e.ErrorMessage);
			}
			NCMBTestSettings.CallbackFlag = true;
		}); 

		yield return NCMBTestSettings.AwaitAsync ();
		Assert.True (NCMBTestSettings.CallbackFlag);
	}

	[UnityTest]
	public IEnumerator ExecuteCallbackWhenExecuteScriptObjectTest_GET ()
	{
		string[] strArray = { "tarou1", "tarou2" };
		NCMBScript script = new NCMBScript ("testScriptObject_GET.js", NCMBScript.MethodType.GET, _endPoint);
		Dictionary<string, object> query = new Dictionary<string, object> () { { "name", strArray } };
		script.ExecuteAsync (null, null, query, (byte[] result, NCMBException e) => {
			if (e == null) {
				string cmd = System.Text.Encoding.UTF8.GetString (result);
				cmd = cmd.TrimEnd ('\0');//終端文字の削除
				Assert.AreEqual ("{\"name\":\"[\\\"tarou1\\\",\\\"tarou2\\\"]\"}", cmd);
			} else {
				Assert.Fail (e.ErrorMessage);
			}
			NCMBTestSettings.CallbackFlag = true;
		});

		yield return NCMBTestSettings.AwaitAsync ();
		Assert.True (NCMBTestSettings.CallbackFlag);
	}
}


