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
using System.Threading;
using System.Collections.Generic;
using System.IO;
using MiniJSON;
using NCMB.Internal;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NCMB
{
	/// <summary>
	/// オブジェクトを操作するクラスです。
	/// </summary>
	public class NCMBObject
	{
		private static readonly string AUTO_CLASS_NAME = "_NewAutoClass";
		private bool _dirty;
		private readonly IDictionary<string, bool> dataAvailability = new Dictionary<string, bool> ();
		internal IDictionary<string, object> estimatedData = new Dictionary<string, object> ();
		internal IDictionary<string, object> serverData = new Dictionary<string, object> ();
		internal readonly object mutex = new object ();
		internal readonly static object fileMutex = new object ();
		private readonly LinkedList<IDictionary<string, INCMBFieldOperation>> operationSetQueue = new LinkedList<IDictionary<string, INCMBFieldOperation>> ();

		private string _className;
		private string _objectId;
		private DateTime? _updateDate;
		private DateTime? _createDate;

		/// <summary>
		/// オブジェクトの取得、または設定を行います。
		/// </summary>
		public virtual object this [string key] {
			get {
				object obj;
				Monitor.Enter (obj = this.mutex);//ロック取得
				try {
					//クリティカルセクション
					//キーのfetch必要かどうかのチェック
					this._checkGetAccess (key);

					//キーが無かったらExceptionを返す
					if (!this.estimatedData.ContainsKey (key)) {
						throw new NCMBException (new ArgumentException ("The given key was not present in the dictionary."));
					}
					object val = this.estimatedData [key];

					//変換出来ないときはnullを返す
					if (val as NCMBRelation<NCMBObject> != null) {
						((NCMBRelation<NCMBObject>)val)._ensureParentAndKey (this, key);
					} else if (val as NCMBRelation<NCMBUser> != null) {
						((NCMBRelation<NCMBUser>)val)._ensureParentAndKey (this, key);
					} else if (val as NCMBRelation<NCMBRole> != null) {
						((NCMBRelation<NCMBRole>)val)._ensureParentAndKey (this, key);
					}

					return val;
				} finally {
					Monitor.Exit (obj);//ロック解放。ここに書くことでtryの中でエラーが発生しても正しく解放される。
				}
			}
			set {
				object obj;
				Monitor.Enter (obj = this.mutex);
				try {
					//nullチェック&Roleオーバーライド用
					this._onSettingValue (key, value);

					//型チェック
					if (!_isValidType (value)) {
						throw new NCMBException (new ArgumentException ("Invalid type for value: " + value.GetType ().ToString ()));
					}
					this.estimatedData [key] = value;
					this._performOperation (key, new NCMBSetOperation (value));
				} finally {
					Monitor.Exit (obj);
				}
			}
		}

		internal virtual void _onSettingValue (string key, object value)
		{
			if (key == null) {
				throw new NCMBException (new ArgumentNullException ("key"));
			}
		}

		/// <summary>
		/// オブジェクトクラス名の取得を行います。
		/// </summary>
		public string ClassName {
			get {
				return this._className;
			}
			private set {
				this._className = value;
			}
		}

		/// <summary>
		/// objectIdの取得、または設定を行います。
		/// </summary>
		public string ObjectId {
			get {
				return  this._objectId;
			}
			set {
				object obj;
				Monitor.Enter (obj = this.mutex);
				try {
					this._dirty = true;
					this._objectId = value;
				} finally {
					Monitor.Exit (obj);
				}
			}
		}

		/// <summary>
		/// オブジェクト更新時刻の取得を行います。
		/// </summary>
		public DateTime? UpdateDate {
			get {
				return this._updateDate;
			}
			private set {
				this._updateDate = value;
			}
		}

		/// <summary>
		/// オブジェクト登録日時の取得を行います。
		/// </summary>
		public DateTime? CreateDate {
			get {
				return this._createDate;
			}
			set {
				this._createDate = value;
			}
		}

		/// <summary>
		/// ACLの取得、または設定を行います。
		/// </summary>
		public NCMBACL ACL {
			set {
				this ["acl"] = value;
			}
			get {
				object acl = null;
				this.estimatedData.TryGetValue ("acl", out acl);
				if (acl == null) {
					return null;
				}

				if (!(acl is NCMBACL)) {
					throw new NCMBException (new ArgumentException ("only ACLs can be stored in the ACL key"));
				}

				NCMBACL dstAcl = (NCMBACL)acl;
				if (dstAcl._isShared ()) {
					NCMBACL copy = dstAcl._copy ();
					this.estimatedData ["acl"] = copy;
					return copy;
				}
				return dstAcl;
			}
		}
		//_checkGetAccess(This)で扱う
		//dataAvailabilityに指定したキーがない場合はfalseを返す
		private bool _checkIsDataAvailable (string key)
		{
			bool result = (this.dataAvailability.ContainsKey (key) && this.dataAvailability [key]);
			return result;
		}
		//ThisのGetで扱う　キーのフェッチの必要性チェック
		//_checkIsDataAvailableがfalseの場合はfetchが必要とExceptionを吐く
		private void _checkGetAccess (string key)
		{
			if (!this._checkIsDataAvailable (key)) {
				throw new NCMBException (new InvalidOperationException ("NCMBObject has no data for this key.  Call FetchAsync() to get the data."));
			}
		}

		/// <summary>
		/// オブジェクトが変更済みかどうか、判定の取得を行います。
		/// </summary>
		/// <returns>変更可否　true:変更済　false : 未変更 </returns>
		public bool IsDirty {
			get {
				object obj;
				Monitor.Enter (obj = this.mutex);
				bool result;
				try {
					result = this._checkIsDirty (true);
				} finally {
					Monitor.Exit (obj);
				}
				return result;
			}
			internal set {
				object obj;
				Monitor.Enter (obj = this.mutex);
				try {
					this._dirty = value;
				} finally {
					Monitor.Exit (obj);
				}
			}
		}
		//saveする必要がある場合(変更有)はtrue、saveする必要がない場合(変更無)はfalseを返却
		private bool _checkIsDirty (bool considerChildren)
		{
			return this._dirty || this._currentOperations.Count > 0 || (considerChildren && this._hasDirtyChildren ());
		}
		//保存が必要(Dirtyがtrue)なNCMBObjectが追加されたリストを返す
		private bool _hasDirtyChildren ()
		{
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				List<NCMBObject> unsavedChildren = new List<NCMBObject> ();
				NCMBObject._findUnsavedChildren (this.estimatedData, unsavedChildren);
				return unsavedChildren.Count > 0;
			} finally {
				Monitor.Exit (obj);
			}
		}
		//NCMBObjectがないか検索をかける
		private static void _findUnsavedChildren (object data, List<NCMBObject> unsaved)
		{
			if (data is IList) {
				IList list = (IList)data;
				foreach (object value in list) {
					NCMBObject._findUnsavedChildren (value, unsaved);
				}
			} else if (data is IDictionary) {
				IDictionary dic = (IDictionary)data;
				foreach (object value in dic.Values) {
					NCMBObject._findUnsavedChildren (value, unsaved);
				}
			}
			//IsDirtyがtrue(保存が必要)なオブジェクトをリストに追加
			else if (data is NCMBObject) {
				NCMBObject obj = (NCMBObject)data;
				if (obj.IsDirty) {
					unsaved.Add (obj);
				}
			}
		}

		/// <summary>
		/// オブジェクトに格納されている、Keyの取得を行います。
		/// </summary>
		public ICollection<string> Keys {
			get {
				ICollection<string> keys;
				object obj;
				Monitor.Enter (obj = this.mutex);
				try {
					keys = this.estimatedData.Keys;
				} finally {
					Monitor.Exit (obj);
				}
				return keys;
			}
		}
		//userのようなNCMBObjectを継承するクラスのコンストラクタを実装するため
		internal NCMBObject () : this (NCMBObject.AUTO_CLASS_NAME)
		{
		}

		/// <summary>
		/// 指定キーのNCMBRelationを生成します。
		/// </summary>
		/// <param name="key">フィールド名</param>
		/// <returns>keyに対応したNCMBRelationを生成して返します。<br/>もし、フィールドに既にNCMBRelationが存在した場合、情報を引き継いだNCMBRelationを生成して返します。</returns>
		public NCMBRelation<T> GetRelation<T> (String key) where T : NCMBObject
		{
			NCMBRelation<T> relation = new NCMBRelation<T> (this, key);
			object value = null;
			this.estimatedData.TryGetValue (key, out value);
			//すでに指定キーにリレーションがあった場合は、リレーション先のクラス名を保持する
			if ((value is NCMBRelation<T>)) {
				relation.TargetClass = (((NCMBRelation<T>)value).TargetClass);
			}
			return relation;
		}

		/// <summary>
		/// コンストラクター。<br/>
		/// 指定クラス名のオブジェクトを生成します。
		/// </summary>
		/// <param name="className">クラス名</param>
		public NCMBObject (string className)
		{
			//クラス名を処理する
			if ((className == "") || (className == null)) {
				throw new NCMBException ("You must specify class name or invalid classname");
			}
			if (className == NCMBObject.AUTO_CLASS_NAME) {
				this.ClassName = NCMBUtility.GetClassName (this);
			} else {
				this.ClassName = className;
			}
			//初期化
			this.operationSetQueue.AddLast (new Dictionary<string, INCMBFieldOperation> ());
			this.estimatedData = new Dictionary<string, object> ();
			this.IsDirty = true;
			this.dataAvailability = new Dictionary<string, bool> ();

			this._setDefaultValues ();

		}
		//渡されたオペレーション処理の実行
		internal void _performOperation (string key, INCMBFieldOperation operation)
		{
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				object oldValue;
				this.estimatedData.TryGetValue (key, out oldValue);
				object newValue = operation.Apply (oldValue, this, key);
				if (newValue != null) {
					this.estimatedData [key] = newValue;
				} else {
					this.estimatedData.Remove (key);
				}
				INCMBFieldOperation oldOperation;
				//最後の履歴の取り出し。oldOperation
				this._currentOperations.TryGetValue (key, out oldOperation);
				//新規履歴の作成。newOperation
				INCMBFieldOperation newOperation = operation.MergeWithPrevious (oldOperation);
				this._currentOperations [key] = newOperation;
				this.dataAvailability [key] = true;
			} catch (Exception e) {
				throw new NCMBException (e);
			} finally {
				Monitor.Exit (obj);
			}
		}
		//一番最後の履歴の取り出し
		internal IDictionary<string, INCMBFieldOperation> _currentOperations {
			get {
				return this.operationSetQueue.Last.Value;
			}
		}
		//一番最後の履歴の取り出し、空のリスト(次に入る履歴の入れ物)の追加
		internal IDictionary<string, INCMBFieldOperation> StartSave ()
		{
			object obj;
			Monitor.Enter (obj = this.mutex);
			IDictionary<string, INCMBFieldOperation> result = null;
			try {
				IDictionary<string, INCMBFieldOperation> currentOperations = this._currentOperations;
				this.operationSetQueue.AddLast (new Dictionary<string, INCMBFieldOperation> ());
				result = currentOperations;
			} finally {
				Monitor.Exit (obj);
			}
			return result;
		}
		//入力値の型チェックを行なう
		internal static bool _isValidType (object value)
		{
			return value == null || value is string || value is NCMBObject || value is NCMBGeoPoint || value is DateTime ||
			value is IDictionary || value is IList || value is NCMBACL || value.GetType ().IsPrimitive () || value is NCMBRelation<NCMBObject>;
		}
		//リストの型チェックを行う
		//RemoveRangeFromList,AddRangeToList,AddRangeUniqueToListの三カ所で使用
		internal void  _listIsValidType (IEnumerable values)
		{
			IEnumerator validCheck = values.GetEnumerator ();
			while (validCheck.MoveNext ()) {
				object val = (object)validCheck.Current;
				if (!_isValidType (val)) {
					throw new NCMBException (new ArgumentException ("invalid type for value: " + val.GetType ().ToString ()));
				}
			}
		}

		/// <summary>
		/// 最後に保存を行った状態に戻します。
		/// </summary>
		public void Revert ()
		{
			if (this._currentOperations.Count > 0) {
				this._currentOperations.Clear ();
				this.operationSetQueue.Clear ();
				this.operationSetQueue.AddLast (new Dictionary<string, INCMBFieldOperation> ());
				this._rebuildEstimatedData ();
				this._dirty = false;
			}
		}

		private void _rebuildEstimatedData ()
		{
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				this.estimatedData.Clear ();
				//get data from serverData
				foreach (KeyValuePair<string, object> current in this.serverData) {
					this.estimatedData.Add (current);
				}
			} finally {
				Monitor.Exit (obj);
			}
		}

		private void _updateLatestEstimatedData ()
		{
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				//apply operation set queue

				if (this.operationSetQueue.Count > 0) {
					foreach (IDictionary<string, INCMBFieldOperation> current in this.operationSetQueue) {
						this._applyOperations (current, this.estimatedData);
					}
				}
			} finally {
				Monitor.Exit (obj);
			}
		}

		private void _applyOperations (IDictionary<string, INCMBFieldOperation> operations, IDictionary<string, object> map)
		{
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				foreach (KeyValuePair<string, INCMBFieldOperation> current in operations) {
					object oldValue;
					map.TryGetValue (current.Key, out oldValue);
					object obj2 = current.Value.Apply (oldValue, this, current.Key);
					if (obj2 != null) {
						map [current.Key] = obj2;
					} else {
						map.Remove (current.Key);
					}
				}
			} finally {
				Monitor.Exit (obj);
			}
		}

		/// <summary>
		/// 指定したキーに値を設定します。<br/>
		/// すでにあるキーを指定した場合はエラーを返します。
		/// </summary>
		/// <param name="key">キー</param>
		/// <param name="value">値</param>
		public virtual void Add (string key, object value)
		{
			object obj;
			Monitor.Enter (obj = this.mutex);//排他的処理
			try {
				//keyとvalueのnullチェック
				if (key == null) {
					throw new NCMBException (new ArgumentException ("key may not be null."));
				}
				if (value == null) {
					throw new NCMBException (new ArgumentException ("value may not be null."));
				}

				//すでにある場合
				if (this.estimatedData.ContainsKey (key)) {
					throw new NCMBException (new ArgumentException ("Key already exists", key));
				}
				//thisメソッド(items)にてsetOperationで履歴作成
				this [key] = value;
			} finally {
				Monitor.Exit (obj);
			}
		}

		/// <summary>
		/// 指定したキーのフィールドが存在する場合、フィールドを削除します。<br/>
		/// 指定キーが無い場合でも、エラーは返しません。
		/// </summary>
		/// <param name="key">フィールド名</param>
		public virtual void Remove (string key)
		{
			object obj;
			object keyObj;
			Monitor.Enter (obj = this.mutex);
			try {
				if (this.estimatedData.TryGetValue (key, out keyObj)) {

					estimatedData.Remove (key);
					//operationSetQueueに　key:key value:NCMBDeleteOperation　追加
					this._currentOperations [key] = new NCMBDeleteOperation ();
				}
			} finally {
				Monitor.Exit (obj);
			}
		}

		/// <summary>
		/// キーで指定された配列から一致する複数のオブジェクトを削除します。<br/>
		/// saveAsync()実行時に指定したオブジェクトの削除を行います。
		/// </summary>
		/// <param name="key">フィールド名</param>
		/// <param name="values">削除するオブジェクト</param>
		public void RemoveRangeFromList (string key, IEnumerable values)
		{
			//リストの型チェック
			this._listIsValidType (values);
			//保存済みかどうか
			if (this._objectId == null || this._objectId.Equals ("")) {
				// 登録時
				IList value = null;//リムーブ対象のデータが入る
				object objectKey;//ローカルデータが入る
				//	リムーブ対象のローカルデータ取り出し
				if (this.estimatedData.TryGetValue (key, out objectKey)) {
					//valにローカルデータの全要素を追加し、リムーブ対象のデータ作成を行なう
					if ((objectKey is IList)) {
						value = new ArrayList ((IList)objectKey);
					} else {
						throw new NCMBException (new ArgumentException ("Old value is not an array."));
					}


					//１．取り出したローカルデータから今回の引数で渡されたオブジェクトの削除
					//例：estimatedData(result)＝{1,NCMBObject}　引数(values)={2,NCMBObject}の時,結果:{1}
					ArrayList result = new ArrayList (value);
					foreach (object removeObj in values) {
						while (result.Contains (removeObj)) {//removeAllと同等
							result.Remove (removeObj);
						}
					}

					//ここから下は引数の中で「NCMBObjectが保存されているかつ、
					//estimatedData(result)の中の一致するNCMBObjectが消せなかった」時の処理です。
					//つまり　「上で消せなかったNCMBObject=インスタンスが違う」
					//estimatedData(result)の中のNCMBObjectと引数のNCMBObjectがどちらもnewで作られたものなら上で消せるが、
					//どちらかがnewでどちらかがCreateWithoutDataで作られた場合は上で消せない。
					//そのため下の処理はobjectIdで検索をかけてobjectIdが一致するNCMBObjectの削除を行う

					//２．今回引数で渡されたオブジェクトから１.のオブジェクトの削除
					//例：引数(objectsToBeRemoved)＝{2,NCMBObject}　1の結果={1}の時,結果:{2,NCMBObject}
					ArrayList objectsToBeRemoved = new ArrayList ((IList)values);
					foreach (object removeObj2 in result) {
						while (objectsToBeRemoved.Contains (removeObj2)) {//removeAllと同等
							objectsToBeRemoved.Remove (removeObj2);
						}
					}

					//３．２の結果のリスト（引数）の中のNCMBObjectがすでに保存されている場合はobjectIdを返す
					//まだ保存されていない場合はnullを返す
					//例：CreateWithoutDataの場合「objectIds　Value:ppmQNGZahXpO8YSV」newの場合「objectIds　Value:null」
					HashSet<object> objectIds = new HashSet<object> ();
					foreach (object hashSetValue in objectsToBeRemoved) {
						if (hashSetValue is NCMBObject) {
							NCMBObject valuesNCMBObject = (NCMBObject)hashSetValue;
							objectIds.Add (valuesNCMBObject.ObjectId);
						}

						//４．resultの中のNCMBObjectからobjectIdsの中にあるObjectIdと一致するNCMBObjectの削除
						//ここだけfor文で対応している理由は,
						//「foreach文により要素を列挙している最中には、そのリスト(result)から要素を削除することはできない(Exception吐く)」
						//例：上記の例の場合の結果result = {1}
						object resultValue;
						for (int i = 0; i < result.Count; i++) {
							resultValue = result [i];
							if (resultValue is NCMBObject) {
								NCMBObject resultNCMBObject = (NCMBObject)resultValue;
								if (objectIds.Contains (resultNCMBObject.ObjectId)) {
									result.RemoveAt (i);
								}
							}
						}
					}

					NCMBSetOperation operation = new NCMBSetOperation (result);
					this._performOperation (key, operation);
				} else {
					throw new NCMBException (new ArgumentException ("Does not have a value."));
				}
			} else {
				// 更新時
				NCMBRemoveOperation operation = new NCMBRemoveOperation (values);
				this._performOperation (key, operation);
			}
		}

		/// <summary>
		/// キーで指定された配列にオブジェクトを追加します。<br/>
		/// 挿入位置は最後に追加します。
		/// </summary>
		/// <param name="key">フィールド名</param>
		/// <param name="value">配列に追加する値</param>
		public void AddToList (string key, object value)
		{
			this.AddRangeToList (key, new ArrayList{ value });
		}

		/// <summary>
		/// キーで指定された配列に複数のオブジェクトを追加します。<br/>
		/// 挿入位置は最後に追加します。
		/// </summary>
		/// <param name="key">フィールド名</param>
		/// <param name="values">配列に追加する値のリスト</param>
		public void AddRangeToList (string key, IEnumerable values)
		{
			//リストの型チェック
			this._listIsValidType (values);
			//保存済みかどうか
			if (this._objectId == null || this._objectId.Equals ("")) {
				// 登録時
				ArrayList val = null;//追加
				object objectKey;
				if (this.estimatedData.TryGetValue (key, out objectKey)) {
					if ((objectKey is IList)) {
						val = new ArrayList ((IList)objectKey);
					} else {
						throw new NCMBException (new ArgumentException ("Old value is not an array."));
					}

					//vauesの各要素を前回のデータに追加する
					IEnumerator localEnumerator = values.GetEnumerator ();
					while (localEnumerator.MoveNext ()) {
						object objectValue = (object)localEnumerator.Current;
						val.Add (objectValue);
					}
					//データ操作二回目以降かつobjectIdがない時
					NCMBSetOperation operation = new NCMBSetOperation (val);
					this._performOperation (key, operation);
				} else {
					//初回
					NCMBSetOperation operation = new NCMBSetOperation (values);
					this._performOperation (key, operation);
				}
			} else {
				// 更新時
				NCMBAddOperation operation = new NCMBAddOperation (values);
				this._performOperation (key, operation);
			}
		}

		/// <summary>
		/// キーで指定された配列にオブジェクトを追加します。<br/>
		/// 今までに登録されていない値のみの追加を行います。<br/>
		/// 挿入位置は保証されません。
		/// </summary>
		/// <param name="key">フィールド名</param>
		/// <param name="value">配列に追加する値</param>
		public void AddUniqueToList (string key, object value)
		{
			this.AddRangeUniqueToList (key, new ArrayList{ value });
		}

		/// <summary>
		/// キーで指定された配列に複数のオブジェクトを追加します。<br/>
		/// 今までに登録されていない値のみの追加を行います。<br/>
		/// 挿入位置は保証されません。
		/// </summary>
		/// <param name="key">フィールド名</param>
		/// <param name="values">配列に追加する値のリスト</param>
		public void AddRangeUniqueToList (string key, IEnumerable values)
		{
			//リストの型チェック
			this._listIsValidType (values);
			//保存済みかどうか
			if (this._objectId == null || this._objectId.Equals ("")) {
				// 登録時
				//List<object> val = null;
				ArrayList val = null;//追加
				object objectKey;
				//AddUniqe対象のローカルデータの取り出し。objectKeyに入る
				if (this.estimatedData.TryGetValue (key, out objectKey)) {
					//取り出した値(前回の値)をvalに追加
					if ((objectKey is IList)) {//追加
						val = new ArrayList ((IList)objectKey);//追加
					} else {
						throw new NCMBException (new ArgumentException ("Old value is not an array."));
					}

					//前回のオブジェクトのobjectIDを補完する。　key : objectId value : int(連番)
					Hashtable existingObjectIds = new Hashtable ();
					//全要素検索
					foreach (object resultValue in val) {
						int i = 0;
						//前回のオブジェクトからNCMBObjectの要素を検索
						if (resultValue is NCMBObject) {
							//あればkeyにobjectId,valueに連番を追加
							NCMBObject resultNCMBObject = (NCMBObject)resultValue;
							existingObjectIds.Add (resultNCMBObject.ObjectId, i);//追加したいNCMBObjectのid
						}
					}

					IEnumerator localEnumerator = values.GetEnumerator ();
					while (localEnumerator.MoveNext ()) {
						object objectsValue = (object)localEnumerator.Current;
						if ((objectsValue is NCMBObject)) {
							//objrcts2のobjectIdと先ほど生成したexistingObjectIdsのobjectIDが一致した場合、
							//existingObjectIdsのvalue:連番を返す。なければnull
							NCMBObject objectsNCMBObject = (NCMBObject)objectsValue;

							if (existingObjectIds.ContainsKey (objectsNCMBObject.ObjectId)) {
								//すでにある
								int index = Convert.ToInt32 (existingObjectIds [objectsNCMBObject.ObjectId]);
								val.Insert (index, objectsValue);
							} else {
								//ユニークなのでadd。追加する
								val.Add (objectsValue);
							}
						} else if (!val.Contains (objectsValue)) {
							//更新時。基本的にこちら。重複していない値のみaddする
							val.Add (objectsValue);
						}
					}

					//データ操作二回目以降かつobjectIdがない時
					NCMBSetOperation operation = new NCMBSetOperation (val);
					this._performOperation (key, operation);
				} else {
					//初回
					NCMBSetOperation operation = new NCMBSetOperation (values);
					this._performOperation (key, operation);
				}
			} else {
				// 更新時 PUT
				NCMBAddUniqueOperation operation = new NCMBAddUniqueOperation (values);
				_performOperation (key, operation);
			}
		}

		/// <summary>
		/// オブジェクトに対し、インクリメントを行います。<br/>
		/// インクリメントした結果の型はlong(Int64)型になります。
		/// </summary>
		/// <param name="key">インクリメントを行うオブジェクトのkey</param>
		public void Increment (string key)
		{
			this.Increment (key, 1L);
		}

		/// <summary>
		/// オブジェクトに対し、インクリメントを行います。<br/>
		/// インクリメントした結果の型はlong(Int64)型になります。
		/// </summary>
		/// <param name="key">インクリメントを行うオブジェクトのkey</param>
		/// <param name="amount"> 増加量(long) 減算する場合は「-」を使用</param>
		public void Increment (string key, long amount)
		{
			this._incrementMerge (key, amount);
		}

		/// <summary>
		/// オブジェクトに対し、インクリメントを行います。<br/>
		/// インクリメントした結果の型はdouble型になります。
		/// </summary>
		/// <param name="key">インクリメントを行うオブジェクトのkey</param>
		/// <param name="amount"> 増加量(double) 減算する場合は「-」を使用</param>
		public void Increment (string key, double amount)
		{
			this._incrementMerge (key, amount);
		}

		/// <summary>
		/// インクリメントの実際の処理を行います。
		/// </summary>
		private void _incrementMerge (string key, object amount)
		{
			if ((this._objectId == null) || (this._objectId.Equals (""))) {
				// 登録時
				NCMBSetOperation operation;
				object objectKey;
				//increment対象のローカルデータの取り出し。objectKeyに入る
				if (this.estimatedData.TryGetValue (key, out objectKey)) {
					if (objectKey is string || objectKey == null) {
						throw new NCMBException (new InvalidOperationException ("Old value is not an number."));
					}
					operation = new NCMBSetOperation (_addNumbers (objectKey, amount));
				} else {
					operation = new NCMBSetOperation (amount);
				}
				this._performOperation (key, operation);
			} else {
				NCMBIncrementOperation operation = new NCMBIncrementOperation (amount);
				this._performOperation (key, operation);
			}
		}
		//前回の値と今回のユーザーが指定した数値を足す
		//firstがestimatedDataに保存されていた前回の値、secondが今回ユーザーが引数に指定した値
		//ここにくるsecondの型は「Double」か「Long」のみ
		internal static object _addNumbers (object first, object second)
		{
			try {
				object result = null;
				if (second is long) {
					//Long時
					if (first is Double) {
						//結果がlongの時は前回Double型は小数点以下は切り捨て
						first = Math.Truncate (Convert.ToDouble (first));
					}
					result = Convert.ToInt64 (first) + (long)second;
				} else {
					//Double時
					result = Convert.ToDouble (first) + (double)second;
				}
				return result;
			} catch {
				//firstがキャスト出来ない時
				throw new NCMBException (new  InvalidOperationException ("Value is invalid."));
			}

		}
		//通信URLの取得
		internal virtual string _getBaseUrl ()
		{
			return NCMBSettings.DomainURL + "/" + NCMBSettings.APIVersion + "/classes/" + ClassName;
		}

		/// <summary>
		/// オブジェクトの削除を行います。<br/>
		///通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="callback">コールバック</param>
		public virtual void DeleteAsync (NCMBCallback callback)
		{
			string url = _getBaseUrl ();
			url += "/" + this._objectId;
			ConnectType type = ConnectType.DELETE;//メソッドタイプの設定
			//NCMBObject.printLogConnectBefore ("DEBUG DELETE ASYNC", type.ToString (), url, null);
			NCMBDebug.Log ("【url】:" + url + Environment.NewLine + "【type】:" + type);
			NCMBConnection con = new NCMBConnection (url, type, null, NCMBUser._getCurrentSessionToken ());
			con.Connect (delegate(int statusCode, string responseData, NCMBException error) {
				//引数はリスト(中身NCMBObject)とエラーをユーザーに返す
				NCMBDebug.Log ("【StatusCode】:" + statusCode + Environment.NewLine + "【Error】:" + error + Environment.NewLine + "【ResponseData】:" + responseData);

				try {
					if (error != null) {
						this._handleDeleteResult (false);
					} else {
						this._handleDeleteResult (true);
					}
				} catch (Exception e) {
					error = new NCMBException (e);
				}

				_afterDelete (error);
				if (callback != null) {
					callback (error);
				}
				return;
			});
		}

		/// <summary>
		/// オブジェクトの削除を行います。<br/>
		///通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		public virtual void DeleteAsync ()
		{
			this.DeleteAsync (null);
		}

		/// <summary>
		/// 非同期処理でオブジェクトの保存を行います。<br/>
		/// SaveAsync()を実行してから編集などをしていなく、保存をする必要が無い場合は通信を行いません。<br/>
		/// オブジェクトIDが登録されていない新規オブジェクトなら登録を行います。<br/>
		///オブジェクトIDが登録されている既存オブジェクトなら更新を行います。<br/>
		///通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="callback">コールバック</param>
		public virtual void SaveAsync (NCMBCallback callback)
		{
			this.Save (callback);
		}

		/// <summary>
		/// 非同期処理でオブジェクトの保存を行います。<br/>
		/// SaveAsync()を実行してから編集などをしていなく、保存をする必要が無い場合は通信を行いません。<br/>
		/// オブジェクトIDが登録されていない新規オブジェクトなら登録を行います。<br/>
		///オブジェクトIDが登録されている既存オブジェクトなら更新を行います。<br/>
		///通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		public virtual void SaveAsync ()
		{
			this.SaveAsync (null);
		}

		/// <summary>
		/// 同期通信。関連オブジェクトの保存時に使用する。
		/// </summary>
		internal void Save ()
		{
			this.Save (null);
		}

		/// <summary>
		/// 同期通信。関連オブジェクトの保存時に使用する
		/// </summary>
		internal void Save (NCMBCallback callback)
		{
			if (!this.IsDirty) {
				// 保存する必要がないなら終了
				if (callback != null) {
					callback (null);
				}
				return;
			}

			//関連オブジェクトでSaveが必要なオブジェクトのみ保存
			List<NCMBObject> unsaveList = new List<NCMBObject> ();
			NCMBObject._findUnsavedChildren (this.estimatedData, unsaveList);
			if (unsaveList.Count > 0) {
				foreach (NCMBObject child in unsaveList) {
					try {
						child.Save ();
					} catch (NCMBException error) {
						if (callback != null) {
							callback (error);
						}
						return;
					}
				}
			}

			//オーバーライド用
			this._beforeSave ();

			string url = _getBaseUrl ();//URL作成
			ConnectType type;
			// オブジェクトIDがある場合は更新
			if (this._objectId != null) {
				url += "/" + this._objectId;
				type = ConnectType.PUT;
			} else {
				type = ConnectType.POST;
			}
			//履歴の取り出し
			IDictionary<string, INCMBFieldOperation> currentOperations = null;
			currentOperations = this.StartSave ();
			string content = _toJSONObjectForSaving (currentOperations);

			//ログを確認（通信前）
			NCMBDebug.Log ("【url】:" + url + Environment.NewLine + "【type】:" + type + Environment.NewLine + "【content】:" + content);
			// 通信処理
			NCMBConnection con = new NCMBConnection (url, type, content, NCMBUser._getCurrentSessionToken ());
			con.Connect (delegate(int statusCode, string responseData, NCMBException error) {
				try {
					NCMBDebug.Log ("【StatusCode】:" + statusCode + Environment.NewLine + "【Error】:" + error + Environment.NewLine + "【ResponseData】:" + responseData);
					if (error != null) {
						this._handleSaveResult (false, null, currentOperations); //失敗
					} else {
						Dictionary<string, object> responseDic = MiniJSON.Json.Deserialize (responseData) as Dictionary<string, object>;
						this._handleSaveResult (true, responseDic, currentOperations);
					}
				} catch (Exception e) {
					error = new NCMBException (e);
				}

				_afterSave (statusCode, error);
				if (callback != null) {
					callback (error);
				}
				return;
			});
		}
		//save前処理 　オーバーライド用
		internal virtual void _beforeSave ()
		{
		}
		//save後処理 　オーバーライド用
		internal virtual void _afterSave (int statusCode, NCMBException error)
		{
		}
		//delete後処理 　オーバーライド用
		internal virtual void _afterDelete (NCMBException error)
		{
		}
		//saveメソッド用のJSONオブジェクトを生成する
		internal string _toJSONObjectForSaving (IDictionary<string, INCMBFieldOperation> operations)
		{
			string jsonString = "";
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				Dictionary<string, object> dictionary = new Dictionary<string, object> ();
				foreach (KeyValuePair<string, INCMBFieldOperation> current in operations) {
					INCMBFieldOperation value = current.Value;

					//Batchオペレーションが追加され次第削除 RelationOperationの各リストが空の時は何もせず次のキーへ
					if (value is NCMBRelationOperation<NCMBObject>) {
						NCMBRelationOperation<NCMBObject> operation = (NCMBRelationOperation<NCMBObject>)value;
						if (operation._relationsToAdd.Count == 0 && operation._relationsToRemove.Count == 0) {
							continue;
						}
					} else if (value is NCMBRelationOperation<NCMBUser>) {
						NCMBRelationOperation<NCMBUser> operation = (NCMBRelationOperation<NCMBUser>)value;
						if (operation._relationsToAdd.Count == 0 && operation._relationsToRemove.Count == 0) {
							continue;
						}
					} else if (value is NCMBRelationOperation<NCMBRole>) {
						NCMBRelationOperation<NCMBRole> operation = (NCMBRelationOperation<NCMBRole>)value;
						if (operation._relationsToAdd.Count == 0 && operation._relationsToRemove.Count == 0) {
							continue;
						}
					}

					dictionary [current.Key] = NCMBUtility._maybeEncodeJSONObject (value, true);
				}
				jsonString = Json.Serialize (dictionary);
			} finally {
				Monitor.Exit (obj);
			}
			return jsonString;
		}

		/// <summary>
		/// 非同期処理でオブジェクトの取得を行います。<br/>
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="callback">コールバック</param>
		public virtual void FetchAsync (NCMBCallback callback)
		{
			//object id をチェック処理
			if ((this._objectId == null) || (this._objectId == "")) {
				throw new NCMBException ("Object ID must be set to be fetched.");
			}


			String url = _getBaseUrl ();
			url += "/" + this._objectId;
			ConnectType type = ConnectType.GET;//メソッドタイプの設定
			//NCMBObject.printLogConnectBefore ("DEBUG FETCH ASYNC CONNECT", type.ToString (), url, null);
			NCMBDebug.Log ("【url】:" + url + Environment.NewLine + "【type】:" + type);
			NCMBConnection con = new NCMBConnection (url, type, null, NCMBUser._getCurrentSessionToken ());
			con.Connect (delegate(int statusCode, string responseData, NCMBException error) {
				NCMBDebug.Log ("【StatusCode】:" + statusCode + Environment.NewLine + "【Error】:" + error + Environment.NewLine + "【ResponseData】:" + responseData);
				try {
					if (error != null) {
						this._handleFetchResult (false, null);
						//this.printLog ("DEBUG FETCHASYNC AFTER (FAIL)", null, null);
					} else {
						Dictionary<string, object> responseDic = null;
						if ((responseData != null) && (responseData != ""))
						{
							responseDic = MiniJSON.Json.Deserialize(responseData) as Dictionary<string, object>;
						}
						this._handleFetchResult (true, responseDic);
						//this.printLog ("DEBUG FETCHASYNC AFTER (SUCCESS)", null, null);
					}
				} catch (Exception e) {
					throw new NCMBException (e);
				}
				if (callback != null) {
					callback (error);

				}
				return;
			});
		}

		/// <summary>
		/// 非同期処理でオブジェクトの取得を行います。<br/>
		/// 通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		// オブジェクト保存
		public virtual void FetchAsync ()
		{
			this.FetchAsync (null);
		}
		/*
		private static List<string> fetchAllIds (List<NCMBObject> objects)
		{
			List<string> ids = new List<string> ();
			String className = ((NCMBObject)objects [0]).ClassName;
			String id;
			for (int i = 0; i< objects.Count(); i++) {
				if (!((NCMBObject)objects [i]).ClassName.Equals (className)) {
					throw new NCMBException ("All objects should have the same class!");
				}
				id = ((NCMBObject)objects [i]).ObjectId;
				if (id == null) {
					throw new NCMBException ("All objects must exist on the server!");
				}
				ids.Add (id);
			}
			return ids;
		}
		*/
		/*
		/// <summary>
		/// 非同期処理で複数オブジェクトの取得を行います。<br/>
		/// 通信結果を受け取るために必ずコールバックを設定を行います。
		/// </summary>
		/// <param name="objects">取得するオブジェクトのリスト</param>
		/// <param name="callback">コールバック</param>
		public static void FetchAllAsync<T> (List<NCMBObject> objects, NCMBQueryCallback<NCMBObject> callback)
		{

			if (objects.Count == 0) {
				//return new List<NCMBObject>();
				callback (new List<NCMBObject> (), null);
				return;
			}
			List<string> ids = fetchAllIds (objects);
			NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject> (((NCMBObject)objects [0]).ClassName);
			query.WhereContainedIn ("objectId", ids);
			query.FindAsync (callback);
			return;

		}
		*/
		/// <summary>
		/// オブジェクトに指定したkeyが、存在しているかの判断を行います。
		/// </summary>
		/// <param name="key">キー</param>
		/// <returns>存在有無　true:有　false : 無 </returns>
		public bool ContainsKey (string key)
		{
			bool result;
			lock (this.mutex) {
				result = this.estimatedData.ContainsKey (key);
			}
			return result;
		}

		/// <summary>
		/// 指定したクラス名,ObjectIdのオブジェクト生成を行います。<br/>
		/// </summary>
		/// <param name="className">クラス名</param>
		/// <param name="objectId">ObjectId</param>
		/// <returns> 生成したオブジェクト </returns>
		static public NCMBObject CreateWithoutData (String className, String objectId)
		{
			NCMBObject localNCMBObject = null;
			try {
				if (className == "user") {
					NCMBUser result = new NCMBUser ();
					result.ObjectId = objectId;
					localNCMBObject = result;
				} else {
					NCMBObject result = new NCMBObject (className);
					result.ObjectId = objectId;
					localNCMBObject = result;
				}
			} catch (Exception e) {
				throw new NCMBException (e);
			}
			return localNCMBObject;
		}

		internal virtual void _mergeFromServer (Dictionary<string, object> responseDic, bool completeData)
		{
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				this.IsDirty = false;
				object value;
				if (responseDic.TryGetValue ("objectId", out value)) {
					this._objectId = (string)value;
					responseDic.Remove ("objectId");
				}
				if (responseDic.TryGetValue ("createDate", out value)) {
					this._setCreateDate ((string)value);
					responseDic.Remove ("createDate");
				}
				if (responseDic.TryGetValue ("updateDate", out value)) {
					this._setUpdateDate ((string)value);
					responseDic.Remove ("updateDate");
				}

				//更新日時を更新する
				if ((!this._updateDate.HasValue) && this._createDate != null) {
					this._updateDate = this._createDate;
				}

				//iterate to get Data from responseDic
				foreach (KeyValuePair<string, object> pair in responseDic) {
					this.dataAvailability [pair.Key] = true; //set for dataAvailability
					object valueObj = pair.Value;
					object decodedObj = NCMBUtility.decodeJSONObject (valueObj);
					if (decodedObj != null) {
						this.serverData [pair.Key] = decodedObj;
					} else {
						this.serverData [pair.Key] = valueObj;
					}
				}

				//この処理を上の処理に持っていくと不具合が発生
				//dataAvailabilityにキー"acl"を設定前にresponseDicからRemoveされThisのGetのチェックでエラー
				if (responseDic.TryGetValue ("acl", out value)) {//今回はなし
					NCMBACL acl = NCMBACL._createACLFromJSONObject ((Dictionary<string,object>)value);
					this.serverData ["acl"] = acl;
					responseDic.Remove ("acl");
				}

				this._rebuildEstimatedData (); //create Estimate data from serverData

			} finally {
				Monitor.Exit (obj);
			}
		}
		//After Saveの処理
		internal void _handleSaveResult (bool success, Dictionary <string, object> responseDic, IDictionary <string, INCMBFieldOperation> operationBeforeSave)
		{
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				if (success) {
					//remove saved operations in operationSetQueue:
					this._applyOperations (operationBeforeSave, this.serverData);
					this.operationSetQueue.Remove (operationBeforeSave); //setOperationQueue
					this._mergeFromServer (responseDic, success); //サーバーデータとの同期、estimatedData
					this._rebuildEstimatedData ();
					this._updateLatestEstimatedData (); //add only to update the estimate data save process

				} else {
					//通信失敗時の処理。追加した空の履歴データにデータが入っていればマージする。チケット#34927参照
					LinkedListNode<IDictionary<string, INCMBFieldOperation>> linkedListNode = this.operationSetQueue.Find (operationBeforeSave);
					IDictionary<string, INCMBFieldOperation> value = linkedListNode.Next.Value;
					this.operationSetQueue.Remove (linkedListNode);
					foreach (KeyValuePair<string, INCMBFieldOperation> current in operationBeforeSave) {
						INCMBFieldOperation currentOpelation = current.Value;
						INCMBFieldOperation NCMBFieldOperation = null;
						value.TryGetValue (current.Key, out NCMBFieldOperation);
						if (NCMBFieldOperation != null) {
							NCMBFieldOperation = NCMBFieldOperation.MergeWithPrevious (currentOpelation);
						} else {
							NCMBFieldOperation = currentOpelation;
						}
						value [current.Key] = NCMBFieldOperation;
					}
				}
			} finally {
				Monitor.Exit (obj);
			}
		}
		//After Fetch の処理
		internal void _handleFetchResult (bool success, Dictionary<string, object> responseDic)
		{
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				if (success) {
					this._mergeFromServer (responseDic, success);
					this._rebuildEstimatedData ();
				} else {

				}
			} finally {
				Monitor.Exit (obj);
			}
		}
		//After Delete の処理
		private void _handleDeleteResult (bool success)
		{
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				if (success) {
					this.estimatedData ["objectId"] = null;
					this.operationSetQueue.Clear ();
					this.operationSetQueue.AddLast (new Dictionary<string, INCMBFieldOperation> ());
					this.serverData.Clear ();
					this.estimatedData.Clear ();
					this._dirty = true;
				} else {

				}
			} finally {
				Monitor.Exit (obj);
			}
		}
		//作成日時の設定
		private void _setCreateDate (string resultDate)
		{
			string targetDateFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
			this._createDate = DateTime.ParseExact ((string)resultDate, targetDateFormat, null);
		}
		//更新日時の設定
		private void _setUpdateDate (string resultDate)
		{
			string targetDateFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
			this._updateDate = DateTime.ParseExact ((string)resultDate, targetDateFormat, null);
		}
		//This method is to save object Json data to disk by using file stream **** Using global variable
		internal void _saveToVariable ()
		{
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				string jsonData = this._toJsonDataForDataFile ();
				NCMBSettings.CurrentUser = jsonData;
			} catch (Exception e) {
				throw new NCMBException (e);
			} finally {
				Monitor.Exit (obj);
			}
		}
		//this method is to get data and create NCMBObject **** Using global variable
		internal static NCMBObject _getFromVariable ()
		{
			try {
				string dataString = NCMBSettings.CurrentUser;
				//create Dictionary
				Dictionary<string, object> dataDic = new Dictionary<string, object> ();
				if ((dataString != null) && (dataString != "")) {
					dataDic = MiniJSON.Json.Deserialize (dataString) as Dictionary<string, object>;
					NCMBObject dataObject = CreateWithoutData ((string)dataDic ["className"], null);
					dataObject._mergeFromServer (dataDic, true);
					return dataObject;
				}
			} catch (Exception e) {
				throw new NCMBException (e);
			}
			return null;
		}
		//This method is to save object Json data to disk by using file stream **** Using disk storage
		internal void _saveToDisk (string fileName)
		{
			string filePath = NCMBSettings.filePath + "/" + fileName;
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				string jsonData = this._toJsonDataForDataFile ();
				//save to file
				using (StreamWriter sw = new StreamWriter (filePath, false, Encoding.UTF8)) {
					sw.Write (jsonData);
					sw.Close ();
				}
			} catch (Exception e) {
				throw new NCMBException (e);
			} finally {
				Monitor.Exit (obj);
			}
		}
		//this method is to get data and create NCMBObject **** Using disk storage
		internal static NCMBObject _getFromDisk (string fileName)
		{

			try {
				string filePath = NCMBSettings.filePath + "/" + fileName;
				string dataString = _getDiskData (filePath);
				//create Dictionary
				Dictionary<string, object> dataDic = new Dictionary<string, object> ();
				if ((dataString != null) && (dataString != "")) {
					dataDic = MiniJSON.Json.Deserialize (dataString) as Dictionary<string, object>;
					NCMBObject dataObject = CreateWithoutData ((string)dataDic ["className"], null);
					dataObject._mergeFromServer (dataDic, true);
					return dataObject;
				}
			} catch (Exception e) {
				throw new NCMBException (e);
			}
			return null;
		}

		internal static string _getDiskData (string fileName)
		{
			// Handle any problems that might arise when reading the text
			try {
				string line;
				string result = "";
				// Create a new StreamReader, tell it which file to read and what encoding the file
				// was saved as

				//check if file exist
				if (File.Exists (fileName)) {
					StreamReader theReader = new StreamReader (fileName, Encoding.UTF8);
					// Immediately clean up the reader after this block of code is done.
					// You generally use the "using" statement for potentially memory-intensive objects
					// instead of relying on garbage collection.
					// (Do not confuse this with the using directive for namespace at the
					// beginning of a class!)
					using (theReader) {
						// While there's lines left in the text file, do this:
						do {
							line = theReader.ReadLine ();
							if (line != null) {
								// Do whatever you need to do with the text line, it's a string now
								result += line;
							}
						} while (line != null);

						// Done reading, close the reader and return true to broadcast success
						theReader.Close ();
					}
				}
				return result;

			}

			// If anything broke in the try block, we throw an exception with information
			// on what didn't work
			catch (Exception e) {
				throw new NCMBException (e);
			}

		}
		//create json from this object data
		internal string _toJsonDataForDataFile ()
		{
			string jsonString = "";
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				Dictionary<string, object> dictionary = new Dictionary<string, object> ();
				foreach (KeyValuePair<string, object> current in this.serverData) {
					object value = current.Value;
					dictionary [current.Key] = NCMBUtility._maybeEncodeJSONObject (value, true);
				}
				if (this._createDate != null) {
					dictionary ["createDate"] = NCMBUtility.encodeDate (this._createDate.Value);
				}
				if (this._updateDate != null) {
					dictionary ["updateDate"] = NCMBUtility.encodeDate (this._updateDate.Value);
				}
				if (this._objectId != null) {
					dictionary ["objectId"] = this._objectId;
				}
				dictionary ["className"] = this._className;
				jsonString = Json.Serialize (dictionary);
			} finally {
				Monitor.Exit (obj);
			}
			return jsonString;

		}
		/*
		private void printLog (string debugString1, string debugString2, string debugString3)
		{
			//NCMBDebug.Log ("[" + debugString1 + "] [ClassName] :" + this.ClassName);
			//NCMBDebug.Log ("[" + debugString1 + "] [OperationSetQueue] count ：" + operationSetQueue.Count);
			int i = 1;
			foreach (IDictionary<string, INCMBFieldOperation> o in operationSetQueue) {
				foreach (KeyValuePair<string, INCMBFieldOperation> pair in o) {
					//NCMBDebug.Log ("[" + debugString1 + "] [OperationSetQueue][" + i + "]" + "KEY:" + pair.Key + "　VALUE:" + pair.Value);
				}
				i++;
			}
			foreach (KeyValuePair<string, object> pair in estimatedData) {
				//NCMBDebug.Log ("[" + debugString1 + "] [estimatedData]：" + "KEY:" + pair.Key + "　VALUE:" + pair.Value);
			}
			foreach (KeyValuePair<string, object> pair in serverData) {
				//NCMBDebug.Log ("[" + debugString1 + "] [ServerData]：" + "KEY:" + pair.Key + "　VALUE:" + pair.Value);
			}
			foreach (KeyValuePair<string, INCMBFieldOperation > pair in _currentOperations) {
				//NCMBDebug.Log ("[" + debugString1 + "] [_currentOperations]：" + "KEY:" + pair.Key + "　VALUE:" + pair.Value);
			}
			foreach (KeyValuePair<string, bool> pair in dataAvailability) {
				//NCMBDebug.Log ("[" + debugString1 + "] [dataAvailability]：" + "KEY:" + pair.Key + "　VALUE:" + pair.Value);
			}
		}

		private void printLogObj (string printString1, string printString2)
		{
			//NCMBDebug.Log ("[" + printString1 + "]");
			//NCMBDebug.Log ("[" + printString1 + "] [" + printString2 + "] _dirty : " + this._dirty);
			//NCMBDebug.Log ("[" + printString1 + "] [" + printString2 + "] _objectId : " + this._objectId);
			//NCMBDebug.Log ("[" + printString1 + "] [" + printString2 + "] _createDate : " + this._createDate);
			//NCMBDebug.Log ("[" + printString1 + "] [" + printString2 + "] _updateDate : " + this._updateDate);

		}

		internal static void printLogConnectBefore (string printString, string type, string url, string content)
		{
			//NCMBDebug.Log ("[" + printString + "] type: " + type);
			//NCMBDebug.Log ("[" + printString + "] url: " + url);
			//NCMBDebug.Log ("[" + printString + "] content: " + content);
		}

		internal static void printLogConnectAfter (string printString, string statusCode, string responseData, string error)
		{
			//NCMBDebug.Log ("[" + printString + "] Status Code: " + statusCode);
			//NCMBDebug.Log ("[" + printString + "] Response data: " + responseData);
			//NCMBDebug.Log ("[" + printString + "] Error: " + error);
		}
		*/
		void _setDefaultValues ()
		{
			if (NCMBACL._getDefaultACL () != null) {
				ACL = NCMBACL._getDefaultACL ();
			}
		}
	}
}
