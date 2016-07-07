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
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace NCMB.Internal
{
	//add操作の履歴管理を扱う
	internal class NCMBRelationOperation<T> : INCMBFieldOperation where T : NCMBObject
	{

		private String _targetClass;//Add,Remove対象のオブジェクトClassName(子)
		internal HashSet<string> _relationsToAdd;//add対象(子)のobjectId
		internal HashSet<string> _relationsToRemove;//remove対象(子)のobjectId

		//NCMBRelationのAdd,Removeのみで扱われる
		internal string TargetClass {
			get {
				return this._targetClass;
			}
		}

		internal NCMBRelationOperation (HashSet<T> newRelationsToAdd, HashSet<T> newRelationsToRemove)
		{
			this._targetClass = null;
			this._relationsToAdd = new HashSet<String> ();
			this._relationsToRemove = new HashSet<String> ();

			//Add操作時
			if (newRelationsToAdd != null) {
				foreach (NCMBObject obj in newRelationsToAdd) {
					if (obj.ObjectId == null) {//add対象(子)を保存していなければエラー
						throw new NCMBException (new  ArgumentException ("All objects in a relation must have object ids."));
					}
					this._relationsToAdd.Add (obj.ObjectId);//add対象(子)のobjectIdを追加

					if (this._targetClass == null) {//add対象(子)のクラス名が無い場合,そのまま追加
						this._targetClass = obj.ClassName;
					} else if (!this._targetClass.Equals (obj.ClassName)) {//add対象(子)のクラス名がある場合,クラス名が違う場合エラー
						throw new NCMBException (new  ArgumentException ("All objects in a relation must be of the same class."));
					}
				}
			}

			//Remove操作時
			if (newRelationsToRemove != null) {
				foreach (NCMBObject obj in newRelationsToRemove) {
					if (obj.ObjectId == null) {//remove対象(子)を保存していなければエラー
						throw new NCMBException (new  ArgumentException ("All objects in a relation must have object ids."));
					}
					this._relationsToRemove.Add (obj.ObjectId);//remove対象(子)のobjectIdを追加

					if (this._targetClass == null) {//remove対象(子)のクラス名が無い場合,そのまま追加
						this._targetClass = obj.ClassName;
					} else if (!this._targetClass.Equals (obj.ClassName)) {//remove対象(親)のクラス名がある場合,クラス名が違う場合エラー
						throw new NCMBException (new  ArgumentException ("All objects in a relation must be of the same class."));
					}
				}
			}

			//add,remove経由以外はエラー
			if (this._targetClass == null) {
				throw new NCMBException (new  ArgumentException ("Cannot create a NCMBRelationOperation with no objects."));
			}
		}

		//Merge用コンストラクタ
		private NCMBRelationOperation (string newTargetClass, HashSet<String> newRelationsToAdd, HashSet<String> newRelationsToRemove)
		{
			this._targetClass = newTargetClass;
			this._relationsToAdd = new HashSet<string> (newRelationsToAdd);
			this._relationsToRemove = new HashSet<string> (newRelationsToRemove);
		}

		public object Encode ()
		{
			Dictionary<string,object> adds = null;
			Dictionary<string,object> removes = null;

			if (this._relationsToAdd.Count > 0) {
				adds = new Dictionary<string,object> ();
				adds.Add ("__op", "AddRelation");
				adds.Add ("objects", _convertSetToArray (this._relationsToAdd));
			}
			
			if (this._relationsToRemove.Count > 0) {
				removes = new Dictionary<string,object> ();
				removes.Add ("__op", "RemoveRelation");
				removes.Add ("objects", _convertSetToArray (this._relationsToRemove));
			}

			if (adds != null) {
				return adds;
			}
			
			if (removes != null) {
				return removes;
			}

			return null;
		}

		//上記エンコードで呼ぶ
		ArrayList _convertSetToArray (HashSet<String> set)
		{
			ArrayList array = new ArrayList ();
			foreach (string id in set) {
				Dictionary<string,object> pointer = new Dictionary<string,object> ();
				pointer.Add ("__type", "Pointer");
				pointer.Add ("className", this._targetClass);
				pointer.Add ("objectId", id);
				array.Add (pointer);
			}
			return array;
		}


		public INCMBFieldOperation MergeWithPrevious (INCMBFieldOperation previous)
		{
			if (previous == null) {
				return this;
			}
			if ((previous is NCMBDeleteOperation)) {
				throw new NCMBException (new  ArgumentException ("You can't modify a relation after deleting it."));
			}
			if ((previous is NCMBRelationOperation<T>)) {
				NCMBRelationOperation<T> previousOperation = (NCMBRelationOperation<T>)previous;
				
				if ((previousOperation._targetClass != null) && (!previousOperation._targetClass.Equals (this._targetClass))) {
					throw new NCMBException (new  ArgumentException ("Related object object must be of class " + previousOperation._targetClass + ", but " + this._targetClass + " was passed in."));
				}

				//最後にSaveしてから今までAddまたはRemoveしたオブジェクトIDをそれぞれ保持する
				HashSet<string> newRelationsToAdd = new HashSet<string> (previousOperation._relationsToAdd);
				HashSet<string> newRelationsToRemove = new HashSet<string> (previousOperation._relationsToRemove);

				//Add時
				if (this._relationsToAdd.Count > 0) {
					//Removeがまだ実行されてない時のみ、Add対象をリストに追加する
					if (newRelationsToRemove.Count == 0) {
						foreach (string str in _relationsToAdd) {
							newRelationsToAdd.Add (str);
						}
					} else {
						foreach (string str in _relationsToAdd) {
							newRelationsToRemove.Remove (str);
						}
					}
				}

				//Remove時
				if (this._relationsToRemove.Count > 0) {
					//Addがまだ実行されてない時のみ、Remove対象をリストに追加する
					if (newRelationsToAdd.Count == 0) {
						foreach (string str in _relationsToRemove) {
							newRelationsToRemove.Add (str);
						}
					} else {
						foreach (string str in _relationsToRemove) {
							newRelationsToAdd.Remove (str);
						}
					}
				}

				return new NCMBRelationOperation<T> (this._targetClass, newRelationsToAdd, newRelationsToRemove);
			}
			throw new NCMBException (new  ArgumentException ("Operation is invalid after previous operation."));
		}

		//前回のローカルデータから新規ローカルデータの作成
		public object Apply (object oldValue, NCMBObject obj, string key)
		{				
			//前回のローカルデータ(estimatedDataに指定のキーが無い場合)がNullの場合
			if (oldValue == null || oldValue is IList && ((IList)oldValue).Count == 0) {
				NCMBRelation<T> relation = new NCMBRelation<T> (obj, key);//親のNCMBObjectと指定キーでrelation作成
				relation.TargetClass = this._targetClass;//親のクラス名をセット
				return relation;
			}

			if ((oldValue is NCMBRelation<T>)) {
				NCMBRelation<T> relation = (NCMBRelation<T>)oldValue;
				if ((this._targetClass != null) && (relation.TargetClass != null)) {
					if (!relation.TargetClass.Equals (this._targetClass)) {
						throw new ArgumentException ("Related object object must be of class " + relation.TargetClass + ", but " + this._targetClass + " was passed in.");
					}
					relation.TargetClass = this._targetClass;
				}
				return relation;
			}
			throw new NCMBException (new  ArgumentException ("Operation is invalid after previous operation."));
		}

	}
}