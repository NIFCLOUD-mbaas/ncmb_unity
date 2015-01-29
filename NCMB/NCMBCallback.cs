﻿/*******
 Copyright 2014 NIFTY Corporation All Rights Reserved.
 
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

using System;
using System.Collections.Generic;//List用

namespace NCMB
{
	/// <summary>
	/// 通信エラーを返すコールバックです。
	/// </summary>
	/// <param name="error"> 通信エラー</param>
	public delegate void NCMBCallback (NCMBException error);


	/// <summary>
	/// オブジェクトのリストと通信エラーを返すコールバックです。
	/// </summary>
	/// <param name="objects"> 取得したオブジェクトのリスト</param>
	/// <param name="error"> 通信エラー</param>
	public delegate void NCMBQueryCallback<T> (List<T> objects,NCMBException error) ;//findAsync用コールバック

	/// <summary>
	/// オブジェクトと通信エラーを返すコールバックです。
	/// </summary>
	/// <param name="obj"> 取得したオブジェクト</param>
	/// <param name="error"> 通信エラー</param>
	public delegate void NCMBGetCallback<T> (T obj,NCMBException error);//GetAsync用コールバック

	/// <summary>
	/// クエリにマッチするオブジェクトの数と通信エラーを返すコールバックです。
	/// </summary>
	/// <param name="count"> クエリにマッチするオブジェクトの数</param>
	/// <param name="error"> 通信エラー</param>
	public delegate void NCMBCountCallback (int count,NCMBException error);//CountAsync用コールバック

	//通信後内部の処理に返すコールバックです
	internal delegate void HttpClientCallback (int statusCode,string responseData,NCMBException e);
}