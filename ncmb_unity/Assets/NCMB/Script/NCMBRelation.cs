/*******
 Copyright 2017-2022 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.

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
using System;
using System.Collections;
using System.Collections.Generic;
using NCMB.Internal;

namespace NCMB
{
	/// <summary>
	/// リレーションを操作するクラスです。
	/// </summary>
	public class NCMBRelation<T> where T : NCMBObject
	{
		private NCMBObject _parent;//GetRelationしたNCMBObject。親。
		private string _key;//リレーションがあるフィールド(親)のキー
		private string _targetClass;//リレーション先のクラス名

		//NCMBObjectやNCMBRelationOperationからset/getを行なう
		internal string TargetClass {
			get {
				return this._targetClass;
			}
			set {
				this._targetClass = value;
			}
		}

		//コンストラクター
		internal NCMBRelation (NCMBObject parent, string key)
		{
			this._parent = parent;
			this._key = key;
			this._targetClass = null;
		}


		//コンストラクター
		//decodeJSONObjectのみ仕様
		internal NCMBRelation (string targetClass)
		{
			this._parent = null;
			this._key = null;
			this._targetClass = targetClass;
		}

		/// <summary>
		/// リレーションにオブジェクトの追加を行います。<br/>
		/// 前回の保存からRemove操作を行っていた場合、<br/>
		/// Remove操作時と違うオブジェクトをAddする場合は、一度SaveAsync()を実行してください。
		/// </summary>
		/// <param name="obj">オブジェクト</param>
		public void Add (T obj)
		{
			//すでにRemoveが実行されていた場合、Add時の引数と違うobjectならばエラー
			this._addDuplicationCheck (obj);

			HashSet<T> addObj = new HashSet<T> ();
			addObj.Add (obj);
			NCMBRelationOperation<T> operation = new NCMBRelationOperation<T> (addObj, null);
			this._targetClass = operation.TargetClass;
			this._parent._performOperation (this._key, operation);
		}

		/// <summary>
		/// リレーションからオブジェクトの削除を行います。<br/>
		/// 前回の保存からAdd操作を行っていた場合、<br/>
		/// Add操作時と違うオブジェクトをRemoveする場合は、一度SaveAsync()を実行してください。
		/// </summary>
		/// <param name="obj">オブジェクト</param>
		public void Remove (T obj)
		{
			//すでにAddが実行されていた場合、Remove時の引数と違うobjectならばエラー
			this._removeDuplicationCheck (obj);

			HashSet<T> removeObj = new HashSet<T> ();
			removeObj.Add (obj);
			NCMBRelationOperation<T> operation = new NCMBRelationOperation<T> (null, removeObj);
			this._targetClass = operation.TargetClass;
			this._parent._performOperation (this._key, operation);
		}

		//前回Addした中身のオブジェクトをチェック。重複しなければエラー。
		private void _removeDuplicationCheck (T obj)
		{
			//1.履歴取り出し 2.NCMBRelationOperationかチェック 3.Operationが保持しているaddListのオブジェクトをチェック
			//4.引数のobjectIdとList内のobjectIdが一つでも同じものであればOK、なければエラー
			if (this._parent._currentOperations.ContainsKey (_key)) {
				if (this._parent._currentOperations [_key] is NCMBRelationOperation<T>) {
					NCMBRelationOperation<T> relationOperation = (NCMBRelationOperation<T>)this._parent._currentOperations [_key];
					if (relationOperation._relationsToAdd.Count > 0) {
						bool duplication = false;//true:同じオブジェクト false:違うオブジェクト
						foreach (string objectId in relationOperation._relationsToAdd) {
							if (objectId == obj.ObjectId) {
								duplication = true;
							}
						}
						if (!duplication) {//違うオブジェクトの削除を行おうとしている場合はエラー
							throw new NCMBException (new ArgumentException ("Remove objects in a Add Must be the same. Call SaveAsync() to send the data."));
						}
					}
				}
			}
		}

		//前回Removeした中身のオブジェクトのチェック。重複しなければエラー。
		private void _addDuplicationCheck (T obj)
		{
			//1.履歴取り出し 2.NCMBRelationOperationかチェック 3.Operationが保持しているremoveListのオブジェクトをチェック
			//4.引数のobjectIdとList内のobjectIdが一つでも同じものであればOK、なければエラー
			if (this._parent._currentOperations.ContainsKey (_key)) {
				if (this._parent._currentOperations [_key] is NCMBRelationOperation<T>) {
					NCMBRelationOperation<T> relationOperation = (NCMBRelationOperation<T>)this._parent._currentOperations [_key];
					if (relationOperation._relationsToRemove.Count > 0) {
						bool duplication = false;//true:同じオブジェクト false:違うオブジェクト
						foreach (string objectId in relationOperation._relationsToRemove) {
							if (objectId == obj.ObjectId) {
								duplication = true;
							}
						}
						if (!duplication) {//違うオブジェクトの追加を行おうとしている場合はエラー
							throw new NCMBException (new ArgumentException ("Add objects in a Remove Must be the same. Call SaveAsync() to send the data."));
						}
					}
				}
			}
		}


		internal void _ensureParentAndKey (NCMBObject someParent, string someKey)
		{
			if (this._parent == null) {
				this._parent = someParent;
			}
			if (this._key == null) {
				this._key = someKey;
			}
			if (this._parent != someParent) {
				throw new NCMBException (new ArgumentException ("IInternal error. One NCMBRelation retrieved from two different NCMBObjects."));
			}
			if (!this._key.Equals (someKey)) {
				throw new NCMBException (new ArgumentException ("Internal error. One NCMBRelation retrieved from two different keys."));
			}
		}

		//_encodeJSONObjectで使用　※Add,Remove操作以外
		internal Dictionary<string, object> _encodeToJSON ()
		{
			Dictionary<string, object> relation = new Dictionary<string, object> ();
			relation.Add ("__type", "Relation");
			relation.Add ("className", this._targetClass);
			return relation;
		}

		/// <summary>
		/// リレーション内のオブジェクトで使用出来るクエリを取得します。
		/// </summary>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> GetQuery ()
		{
			NCMBQuery<T> query;
			if (this._targetClass == null) {
				query = NCMBQuery<T>.GetQuery (this._parent.ClassName);
			} else {
				query = NCMBQuery<T>.GetQuery (this._targetClass);
			}
			query._whereRelatedTo (this._parent, this._key);
			return query;
		}

	}
}
