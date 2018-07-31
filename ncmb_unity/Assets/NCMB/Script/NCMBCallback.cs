/*******
 Copyright 2017-2018 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.
 
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
using System.Collections.Generic;

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
	public delegate void NCMBQueryCallback<T> (List<T> objects, NCMBException error) ;


	/// <summary>
	/// オブジェクトと通信エラーを返すコールバックです。
	/// </summary>
	/// <param name="obj"> 取得したオブジェクト</param>
	/// <param name="error"> 通信エラー</param>
	public delegate void NCMBGetCallback<T> (T obj, NCMBException error);

	/// <summary>
	/// クエリにマッチするオブジェクトの数と通信エラーを返すコールバックです。
	/// </summary>
	/// <param name="count"> クエリにマッチするオブジェクトの数</param>
	/// <param name="error"> 通信エラー</param>
	public delegate void NCMBCountCallback (int count, NCMBException error);

	//通信後内部の処理に返すコールバックです(通常)
	internal delegate void HttpClientCallback (int statusCode, string responseData, NCMBException e);
	//通信後内部の処理に返すコールバックです(File_GET)
	internal delegate void HttpClientFileDataCallback (int statusCode, byte[] responseData, NCMBException e);

	/// <summary>
	/// スクリプト実行後のデータとエラーを返すコールバックです。
	/// </summary>
	/// <param name="data"> 通信結果</param>
	/// <param name="error"> 通信エラー</param>
	public delegate void NCMBExecuteScriptCallback (byte[] data, NCMBException error);

	/// <summary>
	/// ファイルダウンロード後のデータとエラーを返すコールバックです。
	/// </summary>
	/// <param name="data"> 通信結果</param>
	/// <param name="error"> 通信エラー</param>
	public delegate void NCMBGetFileCallback (byte[] data, NCMBException error);

	/// <summary>
	/// オブジェクトとデバイストークンを返すコールバックです。
	/// </summary>
	/// <param name="token"> デバイストークン</param>
	/// <param name="error"> 通信エラー</param>
	public delegate void NCMBGetTokenCallback(String token, NCMBException error);

}