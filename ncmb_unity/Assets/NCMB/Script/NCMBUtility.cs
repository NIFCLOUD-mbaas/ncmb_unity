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


using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using System.Globalization;

namespace NCMB.Internal
{
	internal static class NCMBUtility
	{

		internal static string GetClassName (object t)
		{
			try {
				NCMBClassNameAttribute customAttribute = (NCMBClassNameAttribute)t.GetType ().GetCustomAttributes (true) [0];
				if ((customAttribute == null) || (customAttribute.ClassName == null)) {
					throw new NCMBException (new ArgumentException ("No ClassName attribute specified on the given subclass."));
				}
				return customAttribute.ClassName;
			} catch (Exception e) {
				throw new NCMBException (e);
			}
		}

		internal static void CopyDictionary (IDictionary<string, object> listSouce, IDictionary<string, object> listDestination)
		{
			try {
				foreach (KeyValuePair<string, object> pair in listSouce) {
					listDestination [pair.Key] = pair.Value;
				}
			} catch (Exception e) {
				throw new NCMBException (e);
			}
		}



		//estimatedDataのvalueの値がオブジェクト型の場合はここで適切に変換
		private static IDictionary<string, object> _encodeJSONObject (object value, bool allowNCMBObjects)
		{
			//日付型をNcmb仕様に変更してクラウドに保存
			if (value is DateTime) {
				DateTime dt = (DateTime)value;
				Dictionary<string, object> Datedic = new Dictionary<string, object> ();
				string iso = dt.ToString ("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
				Datedic.Add ("__type", "Date");
				Datedic.Add ("iso", iso);
				return Datedic;
			}
			if (value is NCMBObject) {
				NCMBObject obj = (NCMBObject)value;
				if (!allowNCMBObjects) {
					throw new ArgumentException ("NCMBObjects not allowed here.");
				}
				//GetRelationなどしたオブジェクトが未保存の場合はエラー
				if (obj.ObjectId == null) {
					throw new ArgumentException ("Cannot create a pointer to an object without an objectId.");
				}
				Dictionary<string, object> NCMBDic = new Dictionary<string, object> ();
				NCMBDic.Add ("__type", "Pointer");
				NCMBDic.Add ("className", obj.ClassName);
				NCMBDic.Add ("objectId", obj.ObjectId);
				return NCMBDic;
			}
			if (value is NCMBGeoPoint) {
				NCMBGeoPoint point = (NCMBGeoPoint)value;
				Dictionary<string, object> GeoDic = new Dictionary<string, object> ();
				GeoDic.Add ("__type", "GeoPoint");
				GeoDic.Add ("latitude", point.Latitude);
				GeoDic.Add ("longitude", point.Longitude);
				return GeoDic;
			}

			//MiniJsonで処理できる
			if (value is IDictionary) {
				Dictionary<string, object> beforeDictionary = new Dictionary<string, object> ();
				IDictionary afterDictionary = (IDictionary)value;
				foreach (object key in afterDictionary.Keys) {
					if (key is string) {
						beforeDictionary [(string)key] = _maybeEncodeJSONObject (afterDictionary [key], allowNCMBObjects);
					} else {
						throw new NCMBException (new ArgumentException ("Invalid type for key: " + key.GetType ().ToString () + ".key type string only."));
					}
				}

				return beforeDictionary;
			}

			if (value is NCMBRelation<NCMBObject>) {
				NCMBRelation<NCMBObject> relation = (NCMBRelation<NCMBObject>)value;
				return relation._encodeToJSON ();
			}

			if (value is NCMBACL) {

				NCMBACL acl = (NCMBACL)value;
				return acl._toJSONObject ();

			}
			return null;
		}

		//リストの値を全てJSON形式に変換
		//各値を_maybeEncodeJSONObjectで各JSON形式に変換
		internal static List<object> _encodeAsJSONArray (IList list, bool allowNCMBObjects)
		{
			List<object> new_array = new List<object> ();
			foreach (object obj in list) {
				if (!NCMBObject._isValidType (obj)) {
					throw new NCMBException (new ArgumentException ("invalid type for value in array: " + obj.GetType ().ToString ()));
				}
				new_array.Add (_maybeEncodeJSONObject (obj, allowNCMBObjects));
			}

			return new_array;
		}

		//各オペレーション毎のencodeを実施する。各オブジェクトのJSONを作成する。
		//valueが配列,オブジェクトだった場合はさらに値の奥までエンコードを行なう。
		internal static object _maybeEncodeJSONObject (object value, bool allowNCMBObjects)
		{

			//save経由はます最初は必ずこちらを通る。
			//各オペレーションのエンコードで再びこのメソッド実行される
			if ((value is INCMBFieldOperation)) {
				return ((INCMBFieldOperation)value).Encode ();
			}

			//リストの値を全てJSON形式に変換
			//各値を_maybeEncodeJSONObjectで各JSON形式に変換
			if (value is IList) {
				//return _encodeAsJSONArray ((List<object>)value, allowNCMBObjects);
				return _encodeAsJSONArray ((IList)value, allowNCMBObjects);
			}
			//次に各operationクラスのエンコードで呼ばれるので二度目はこちらを通る
			//各オブジェクト毎に変換(特殊型)が必要であれば変換、なければvalueを返す
			IDictionary<string, object> jsonDic = _encodeJSONObject (value, allowNCMBObjects);
			if (jsonDic != null) {
				return jsonDic;
			}
			//特にオブジェクト変換が必要ない場合はこちら
			return value;
		}

		internal static Object decodeJSONObject (object jsonDicParameter)
		{
			//check array
			if (jsonDicParameter is IList) {
				ArrayList tmpArrayList = new ArrayList ();
				List<object> objList = new List<object> ();
				objList = (List<object>)jsonDicParameter;
				//NCMBDebug.Log (" Check list!!");
				object objTmp = null;
				for (int i = 0; i < objList.Count; i++) { // Loop through List with for
					//NCMBDebug.Log (" List item " + objList [i]);
					objTmp = decodeJSONObject (objList [i]);
					if (objTmp != null) {
						tmpArrayList.Add (decodeJSONObject (objList [i]));
					} else {
						tmpArrayList.Add (objList [i]);
					}
				}
				return tmpArrayList;
			}

			//check if json or not
			Dictionary<string, object> jsonDic;
			if ((jsonDicParameter is IDictionary)) {
				jsonDic = (Dictionary<string, object>)jsonDicParameter;
			} else {
				return null;
			}
			object typeString;
			jsonDic.TryGetValue ("__type", out typeString);

			/*
						if (typeString == null) {
								return jsonDic;
						}
						*/

			if (typeString == null) { //Dictionary
				Dictionary<string, object> tmpDic = new Dictionary<string, object> ();
				object decodeObj;
				foreach (KeyValuePair<string, object> pair in jsonDic) {
					decodeObj = decodeJSONObject (pair.Value);
					if (decodeObj != null) {
						//NCMBDebug.Log ("[TEST：" + pair.Key + "　VALUE:" + pair.Value);
						tmpDic.Add (pair.Key, decodeObj);
					} else {
						tmpDic.Add (pair.Key, pair.Value);
					}

				}
				//return jsonDic;
				return tmpDic;
			}


			if (typeString.Equals ("Date")) {
				object iso;
				jsonDic.TryGetValue ("iso", out iso);
				return parseDate ((string)iso);
			}

			if (typeString.Equals ("Pointer")) {
				object className;
				jsonDic.TryGetValue ("className", out className);
				object objectId;
				jsonDic.TryGetValue ("objectId", out objectId);
				return NCMBObject.CreateWithoutData ((string)className, (string)objectId);
			}

			if (typeString.Equals ("GeoPoint")) {

				double latitude = 0;
				double longitude = 0;
				try {
					object latitudeString;
					jsonDic.TryGetValue ("latitude", out latitudeString);
					latitude = (double)Convert.ToDouble (latitudeString);
					object longitudeString;
					jsonDic.TryGetValue ("longitude", out longitudeString);
					longitude = Convert.ToDouble (longitudeString);
				} catch (Exception e) {
					throw new NCMBException (e);
				}
				return new NCMBGeoPoint (latitude, longitude);
			}

			if (typeString.Equals ("Object")) {
				object className;
				jsonDic.TryGetValue ("className", out className);
				NCMBObject output = NCMBObject.CreateWithoutData ((string)className, null);
				output._handleFetchResult (true, jsonDic);
				return output;
			}

			//Relation対象クラスを増やす時は要注意
			if (typeString.Equals ("Relation")) {
				if (jsonDic ["className"].Equals ("user")) {
					return new NCMBRelation<NCMBUser> ((string)jsonDic ["className"]);
				} else if (jsonDic ["className"].Equals ("role")) {
					return new NCMBRelation<NCMBRole> ((string)jsonDic ["className"]);
				} else {
					return new NCMBRelation<NCMBObject> ((string)jsonDic ["className"]);
				}
			}

			return null;
		}

		static internal DateTime parseDate (string dateString)
		{
			string format = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
			return DateTime.ParseExact (dateString, format, null);
		}

		static internal string encodeDate (DateTime dateObject)
		{
			string dateString = dateObject.ToString ("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
			return dateString;
		}

		static bool isContainerObject (Object object1)
		{
			return  ((object1 is NCMBGeoPoint)) || ((object1 is IDictionary));
		}

		static internal string _encodeString (string str)
		{
			StringBuilder builder = new StringBuilder ();
			char[] charArray = str.ToCharArray ();
			foreach (var c in charArray) {
				switch (c) {
				case '"':
					builder.Append ("\\\"");
					break;
				case '\\':
					builder.Append ("\\\\");
					break;
				case '\b':
					builder.Append ("\\b");
					break;
				case '\f':
					builder.Append ("\\f");
					break;
				case '\n':
					builder.Append ("\\n");
					break;
				case '\r':
					builder.Append ("\\r");
					break;
				case '\t':
					builder.Append ("\\t");
					break;
				default:
					int codepoint = Convert.ToInt32 (c);
					if ((codepoint >= 32) && (codepoint <= 126)) {
						builder.Append (c);
					} else {
						builder.Append ("\\u");
						builder.Append (codepoint.ToString ("x4"));
					}
					break;
				}
			}
			return builder.ToString ();
		}

        //文字列中、4桁の16進数で表記されたUnicode文字(\uXXXX)のみをデコード
        static internal string unicodeUnescape(string targetText)
        {
            string retval = Regex.Replace
            (
                targetText,
                @"\\[Uu]([0-9A-Fa-f]{4})",
                x =>
                {
                    ushort code = ushort.Parse(x.Groups[1].Value, NumberStyles.AllowHexSpecifier);
                    return ((char)code).ToString();
                }
            );

            return retval.ToString();
        }

	}

}
