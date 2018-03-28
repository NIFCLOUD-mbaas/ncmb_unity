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
using System.Collections;
using System.Collections.Generic;

namespace NCMB.Internal
{
	//AddUnique案連の履歴操作を扱う
	internal class NCMBAddUniqueOperation : INCMBFieldOperation
	{


		ArrayList objects = new ArrayList ();

		public NCMBAddUniqueOperation (object values)
		{
			//List等
			if (values is IEnumerable) {
				IEnumerable newValues = (IEnumerable)values;
				IEnumerator obj = newValues.GetEnumerator ();
				while (obj.MoveNext()) {
					object val = (object)obj.Current;
					objects.Add (val);
				}
			} else {
				this.objects.Add (values);
			}
		}

		//オペレーションエンコード処理
		public object Encode ()
		{
			Dictionary<string, object> dic = new Dictionary<string, object> ();
			dic.Add ("__op", "AddUnique");
			dic.Add ("objects", NCMBUtility._maybeEncodeJSONObject (this.objects, true));
			return dic;
		}

		//前回の履歴データから新しい履歴データを作成する
		public INCMBFieldOperation MergeWithPrevious (INCMBFieldOperation previous)
		{
			if (previous == null) {
				return this;
			}

			if ((previous is NCMBDeleteOperation)) {
				return new NCMBSetOperation (this.objects);
			}

			if ((previous is NCMBSetOperation)) {
				object value = ((NCMBSetOperation)previous).getValue ();	
				if ((value is IList)) {	
					return new NCMBSetOperation (Apply (value, null, null));
				}
				throw new  InvalidOperationException ("You can only add an item to a List.");
			}

			if ((previous is NCMBAddUniqueOperation)) {
				IList result = new ArrayList (((NCMBAddUniqueOperation)previous).objects);	
				return new NCMBAddUniqueOperation ((IList)Apply (result, null, null));
			}
			throw new  InvalidOperationException ("Operation is invalid after previous operation.");
		}
		
		public object Apply (object oldValue, NCMBObject obj, string key)
		{
			//初回 estimatedDataに対象のデータが無かった場合
			if (oldValue == null) {
				return new ArrayList (this.objects);//追加
			}

			//estimatedDataにすでに対象データがあり,配列だった場合
			if ((oldValue is IList)) {
				ArrayList result = new ArrayList ((IList)oldValue);	//追加


				//前回のオブジェクトのobjectIDを補完する。　key : objectId value : int(連番)
				Hashtable existingObjectIds = new Hashtable ();
				//全要素検索
				foreach (object resultValue in result) {
					int i = 0;
					//前回のオブジェクトからNCMBObjectの要素を検索
					if (resultValue is NCMBObject) {
						//あればkeyにobjectId,valueに連番を追加
						NCMBObject resultNCMBObject = (NCMBObject)resultValue;
						existingObjectIds.Add (resultNCMBObject.ObjectId, i);//追加したいNCMBObjectのid
					}
				}

				//同じNCMBObjectだったら重複しないようAPI側でさばいているかも
				IEnumerator localEnumerator = this.objects.GetEnumerator ();
				while (localEnumerator.MoveNext()) {
					object objectsValue = (object)localEnumerator.Current;
					if ((objectsValue is NCMBObject)) {
						//objrcts2のobjectIdと先ほど生成したexistingObjectIdsのobjectIDが一致した場合、
						//existingObjectIdsのvalue:連番を返す。なければnull
						NCMBObject objectsNCMBObject = (NCMBObject)objectsValue;
						if (existingObjectIds.ContainsKey (objectsNCMBObject.ObjectId)) {
							//すでにある
							int index = Convert.ToInt32 (existingObjectIds [objectsNCMBObject.ObjectId]);	
							result.Insert (index, objectsValue);
						} else {
							//ユニークなのでadd。追加する
							result.Add (objectsValue);
						}
					} else if (!result.Contains (objectsValue)) {
						//基本的にこちら。重複していない値のみaddする
						result.Add (objectsValue);
					}
				}
				return result;
			}
			//対象データが配列以外だった場合
			throw new  InvalidOperationException ("Operation is invalid after previous operation.");
		}
	}
}
