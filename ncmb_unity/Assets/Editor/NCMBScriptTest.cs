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
using System.Net;

public class NCMBScriptTest {

	string appKey = "6145f91061916580c742f806bab67649d10f45920246ff459404c46f00ff3e56";
	string clientKey = "1343d198b510a0315db1c03f3aa0e32418b7a743f8e4b47cbff670601345cf75";

	[TestFixtureSetUp]
	public void Init ()
	{
		//set sample API Key (from document)
		NCMBSettings.Initialize (
			appKey,
			clientKey
		);
	}

    [Test]
    public void NCMBSettingsShouldReturnAPIKeyTest ()
    {
		Assert.AreEqual (NCMBSettings.ApplicationKey, appKey);
		Assert.AreEqual (NCMBSettings.ClientKey, clientKey);
    }

	[Test]
	public void NCMBScriptShouldExecuteCallbackWhenExecuteScriptTest ()
	{
		HttpWebResponse res = new HttpWebResponse ();


		/*
		bool callbackFlag = false;

		NCMBScript script = new NCMBScript ("testScript.js", "GET", "http://localhost:3000");
		await Task.Run (() => script.ExecuteAsync (null, null, null, (byte[] result, NCMBException e) => {
			callbackFlag = true;
		})); 
		Assert.True (callbackFlag);
		*/	
	}
}
