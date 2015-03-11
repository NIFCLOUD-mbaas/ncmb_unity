/*******
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace NCMB.Internal
{
	/// <summary>
	/// 拡張メソッド用のクラスです
	/// </summary>
	internal static class ExtensionsClass
	{
		/// <summary>
		/// ライブラリ内の指定した型の情報を取得します。 
		/// </summary>
		public static Type GetTypeInfo (this Type t)
		{
			return t;//タイプ取得
		}

		/// <summary>
		/// Type がプリミティブ型の 1 つである場合は true。それ以外の場合は false。
		/// 下記の型チェック
		/// Boolean、Int16、UInt16、Int32、UInt32、Int64、UInt64、IntPtr、UIntPtr、Char、Double、
		/// </summary>
		internal static bool IsPrimitive (this Type type)
		{
			return type.GetTypeInfo ().IsPrimitive;//取得したタイプを判定
		}
	}
}
