/*******
 Copyright 2016 NIFTY Corporation All Rights Reserved.
 
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
using UnityEditor;
using NUnit.Framework;
using NCMB;
using NCMB.Internal;
using System.Collections.Generic;
using System.Net;
using System;
using System.Threading;
using System.Reflection;


public class NCMBScriptTest
{
	
	static readonly string _appKey = "6145f91061916580c742f806bab67649d10f45920246ff459404c46f00ff3e56";
	static readonly string _clientKey = "1343d198b510a0315db1c03f3aa0e32418b7a743f8e4b47cbff670601345cf75";
	static readonly string _endPoint = "http://localhost:3000/2015-09-01/script";

	bool _callbackFlag = false;

	delegate void AsyncDelegate ();

	[TestFixtureSetUp]
	public void Init ()
	{
		//set sample API Key (from document)
		NCMBSettings.Initialize (
			_appKey,
			_clientKey
		);
		_callbackFlag = false;
	}

	[Test]
	public void ReturnAPIKeyTest ()
	{
		Assert.AreEqual (NCMBSettings.ApplicationKey, _appKey);
		Assert.AreEqual (NCMBSettings.ClientKey, _clientKey);
	}

	[Test]
	public void ExecuteCallbackWhenExecuteScriptTest_POST ()
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
			_callbackFlag = true;
		}); 

		AwaitAsync ();
		Assert.True (_callbackFlag);
	}

	[Test]
	public void ExecuteCallbackWhenExecuteScriptTest_PUT ()
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
			_callbackFlag = true;
		}); 
			
		AwaitAsync ();
		Assert.True (_callbackFlag);
	}

	[Test]
	public void ExecuteCallbackWhenExecuteScriptTest_GET ()
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
			_callbackFlag = true;
		}); 

		AwaitAsync ();
		Assert.True (_callbackFlag);
	}


	[Test]
	public void ExecuteCallbackWhenExecuteScriptTest_DELETE ()
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
			_callbackFlag = true;
		}); 

		AwaitAsync ();
		Assert.True (_callbackFlag);
	}

	[Test]
	public void ExecuteCallbackWhenExecuteScriptTest_Error ()
	{
		NCMBScript script = new NCMBScript ("testScript_Error.js", NCMBScript.MethodType.GET, _endPoint);
		script.ExecuteAsync (null, null, null, (byte[] result, NCMBException e) => {
			if (e == null) {
				Assert.Fail ("Always test case to fail.");
			} else {
				Assert.AreEqual ("name must not be null.", e.ErrorMessage);
				Assert.AreEqual (HttpStatusCode.BadRequest.ToString (), e.ErrorCode);
			}
			_callbackFlag = true;
		}); 

		AwaitAsync ();
		Assert.True (_callbackFlag);
	}

	[Test]
	public void ExecuteCallbackWhenExecuteScriptTest_Header ()
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
			_callbackFlag = true;
		}); 

		AwaitAsync ();
		Assert.True (_callbackFlag);
	}

	// 非同期のコールバックが実行されるまで待機
	public static void AwaitAsync ()
	{
		//Platformクラスを取得
		Type platform = null;
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
			foreach (Type type in assembly.GetTypes()) {
				if (type.Name == "Platform") {
					platform = type;
				}
			}
		}

		//コールバックが実行されるか指定時間経過するまでループ
		if (platform != null) {
			var field = platform.GetField ("Queue", BindingFlags.NonPublic | BindingFlags.Static); 
			Queue<Action> queue = (Queue<Action>)field.GetValue (platform);
			DateTime loopTime = DateTime.Now.AddSeconds (15);
			while (true) {
				if (queue.Count >= 1) {
					queue.Dequeue () ();
					if (queue.Count == 0) {
						break;
					}
				}
				if (loopTime < DateTime.Now) {
					Assert.Fail ("Infinite loop timeout.");
					break;
				}
			}
		}
	}

}


