/*******
 Copyright 2017-2020 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.

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

//#define DEBUG
using System.Diagnostics;
using System;

//using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;

//自作したDebugクラス
//DebugでラッピングするとすべてのDebugがラッピングされるためNCMBDebugで作成
//ログを消したい時はNCMBDebugクラスの#define DEBUGをコメントアウトしてください
namespace NCMB.Internal
{
	internal static class NCMBDebug
	{

		//改行
		private static string newLine = "\n";

		/// <summary>
		/// ログを出力する
		/// </summary>
		/// <param name="message">メッセージ</param>
		[Conditional("DEBUG")]
		public static void Log (string message)
		{
			UnityEngine.Debug.Log (message);
		}

		/// <summary>
		/// 警告ログを出力する
		/// </summary>
		/// <param name="message">メッセージ</param>
		[Conditional("DEBUG")]
		public static void LogWarning (string message)
		{
			UnityEngine.Debug.LogWarning (message);
		}

		/// <summary>
		/// エラーログを出力する
		/// </summary>
		/// <param name="message">メッセージ</param>
		[Conditional("DEBUG")]
		public static void LogError (string message)
		{
			UnityEngine.Debug.LogError (message);
		}

		/// <summary>
		/// エラーログを出力する
		/// </summary>
		/// <param name="message">メッセージ</param>
		/// <param name="context">ログを出力したオブジェクト</param>
		[Conditional("DEBUG")]
		public static void LogError (object message, object context)
		{
			//UnityEngine.Debug.LogError(message, context);
		}

		/// <summary>
		/// listの中身をログに出力する
		/// </summary>
		/// <param name="title">title</param>
		/// <param name="list">list</param>
		[Conditional("DEBUG")]
		public static void List (string title, IList list)
		{
			string result = null;
			result += String.Format (title + newLine);
			for (int i = 0; i < list.Count; i++) {
				result += String.Format ("【" + i + "】" + list [i].ToString () + "{0}", i < list.Count - 1 ? newLine : "");
			}
			UnityEngine.Debug.Log (result);
		}

		/// <summary>
		/// Dictionaryの中身をログに表示します
		/// </summary>
		/// <param name="title">任意のtitle</param>
		/// <param name="dictionary">表示したいDictionary</param>
		[Conditional("DEBUG")]
		public static void Dictionary<T, K> (string title, Dictionary<T, K> dictionary)
		{
			int i = 0;
			string result = null;
			result += String.Format (title + newLine);
			foreach (KeyValuePair<T, K> d in dictionary) {
				result += String.Format ("【" + i + "】" + " Key : " + d.Key.ToString () + " Value : " + d.Value.ToString () + "{0}", i < dictionary.Count - 1 ? newLine : "");
				i++;
			}
			UnityEngine.Debug.Log (result);
		}

	}
}
