/*******
 Copyright 2017-2021 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.

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
using System.Text;
using NCMB.Internal;
using UnityEngine;
using System.Threading;

namespace NCMB
{
	/// <summary>
	/// クエリを操作するクラスです。
	/// </summary>
	public class NCMBQuery<T> where T : NCMBObject
	{
		//where以降のパスを保存
		private readonly Dictionary<string, object> _where;
		private readonly string WHERE_URL = "?";
		private string _className;
		//取得件数
		private int _limit;
		//取得開始位置
		private int _skip;
		//降順、昇順
		private List<string> _order;
		//子の情報の取得有無
		private List<string> _include;

		/// <summary>
		/// コンストラクター。
		/// </summary>
		/// <param name="theClassName">  クラス名</param>
		public NCMBQuery (string theClassName)
		{
			this._className = theClassName;
			//デフォルト設定
			this._limit = -1;
			this._skip = 0;
			this._order = new List<string> ();
			this._include = new List<string> ();
			this._where = new Dictionary<string, object> ();

		}

		/// <summary>
		/// 取得開始位置の取得、または設定を行います。
		/// </summary>
		/// <returns> 取得開始位置</returns>
		public int Skip {
			get {
				return this._skip;
			}set {
				this._skip = value;
			}
		}

		/// <summary>
		/// 取得件数の取得、または設定を行います。
		/// </summary>
		/// <returns> 取得件数</returns>
		public int Limit {
			get {
				return this._limit;
			}set {
				this._limit = value;
			}
		}

		/// <summary>
		/// クラス名の取得を行います。
		/// </summary>
		/// <returns> クラス名</returns>
		public string ClassName {
			get {
				return this._className;
			}
		}

		/// <summary>
		/// orクエリの生成を行います。
		/// </summary>
		/// <param name="queries">or条件とするNCMBQueryをリストで指定</param>
		/// <returns> or条件を作成したクエリ</returns>
		public static NCMBQuery<T> Or (List<NCMBQuery<T>> queries)
		{
			List<NCMBQuery<T>> localList = new List<NCMBQuery<T>> ();//queries内のクラス名取得
			string className = null;

			//nullのリストを渡された場合はExceptionを出す
			if (queries == null) {
				throw new NCMBException (new ArgumentException ("queries may not be null."));
			}

			//空のリストを渡された場合はExceptionを出す
			if (queries.Count == 0) {
				throw new NCMBException (new ArgumentException ("Can't take an or of an empty list of queries"));
			}

			//localListにqueriesの中の各クラス名を追加
			for (int i = 0; i < queries.Count; i++) {
				//違うクラス名同士のor結合は出来ない
				if ((className != null) && (!((NCMBQuery<T>)queries [i])._className.Equals (className))) {
					throw new NCMBException (new ArgumentException ("All of the queries in an or query must be on the same class "));
				}
				//渡されたリストの各NCMBQueryクラスのクラス名の取得
				className = ((NCMBQuery<T>)queries [i])._className;
				localList.Add ((NCMBQuery<T>)queries [i]);
			}
			//Or条件の追加
			NCMBQuery<T> value = new NCMBQuery<T> (className);
			return value._whereSatifiesAnyOf (localList);
		}

		/// <summary>
		/// Or条件の追加
		/// </summary>
		private NCMBQuery<T> _whereSatifiesAnyOf (List<NCMBQuery<T>> queries)
		{
			this._where ["$or"] = queries;
			return this;
		}


		/// <summary>
		/// 結果を昇順で取得します。<br/>
		/// 単一のソート条件を使用する場合はこちらを使用します。
		/// </summary>
		/// <param name="key"> 昇順に指定するフィールド名</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> OrderByAscending (string key)
		{
			this._order.Clear ();
			this._order.Add (key);
			return this;
		}

		/// <summary>
		/// 結果を降順で取得します。<br/>
		/// 単一のソート条件を使用する場合はこちらを使用します。
		/// </summary>
		/// <param name="key"> 降順に指定するフィールド名</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> OrderByDescending (string key)
		{
			this._order.Clear ();
			this._order.Add ("-" + key);
			return this;
		}

		/// <summary>
		/// 昇順条件の追加を行います。<br/>
		/// 複数のソート条件を使用する場合はこちらを使用します。
		/// </summary>
		/// <param name="key">  昇順に指定するフィールド名</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> AddAscendingOrder (string key)
		{
			if (this._order.Count == 0 || this._order [0].Equals ("")) {//単一の場合
				this._order.Clear ();
			}
			//二回目(複数)以降実行される場合はクリアしない
			this._order.Add (key);
			return this;
		}

		/// <summary>
		/// 降順条件の追加を行います。<br/>
		/// 複数のソート条件を使用する場合はこちらを使用します。
		/// </summary>
		/// <param name="key">  降順に指定するフィールド名</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> AddDescendingOrder (string key)
		{
			if (this._order.Count == 0 || this._order [0].Equals ("")) {//単一の場合
				this._order.Clear ();
			}
			//二回目(複数)以降実行される場合はクリアしない
			this._order.Add ("-" + key);
			return this;
		}


		/// <summary>
		/// 一致する。
		/// </summary>
		/// <param name="key"> 条件指定するフィールド名</param>
		/// <param name="value">  値</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereEqualTo (string key, object value)
		{
			value = NCMBUtility._maybeEncodeJSONObject (value, true);
			_where [key] = value;
			return this;
		}


		/// <summary>
		/// 一致しない。
		/// </summary>
		/// <param name="key"> 条件指定するフィールド名</param>
		/// <param name="value">  値</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereNotEqualTo (string key, object value)
		{
			_addCondition (key, "$ne", value);
			return this;
		}

		/// <summary>
		/// より、大きい。<br/>
		/// valueの数を含まない。
		/// </summary>
		/// <param name="key">  条件指定するフィールド名</param>
		/// <param name="value">  値</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereGreaterThan (string key, object value)
		{
			_addCondition (key, "$gt", value);
			return this;
		}

		/// <summary>
		/// 以上。<br/>
		/// valueの数を含む。
		/// </summary>
		/// <param name="key">  条件指定するフィールド名</param>
		/// <param name="value">  値</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereGreaterThanOrEqualTo (string key, object value)
		{
			_addCondition (key, "$gte", value);
			return this;
		}



		/// <summary>
		/// より、小さい。<br/>
		/// valueの数をまない。
		/// </summary>
		/// <param name="key">  条件指定するフィールド名</param>
		/// <param name="value">  値</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereLessThan (string key, object value)
		{
			_addCondition (key, "$lt", value);
			return this;
		}

		/// <summary>
		/// 以下。<br/>
		/// valueの数を含む。
		/// </summary>
		/// <param name="key">  条件指定するフィールド名</param>
		/// <param name="value">  値</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereLessThanOrEqualTo (string key, object value)
		{
			_addCondition (key, "$lte", value);
			return this;
		}

		/// <summary>
		/// いずれかと一致。
		/// </summary>
		/// <param name="key">  条件指定するフィールド名</param>
		/// <param name="values">  値(List)</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereContainedIn (string key, IEnumerable values)
		{
			List<object> valuesList = new List<object> ();
			foreach (object value in values) {
				valuesList.Add (value);
			}
			_addCondition (key, "$in", valuesList);
			return this;
		}

		/// <summary>
		/// いずれとも一致しない。
		/// </summary>
		/// <param name="key">  条件指定するフィールド名</param>
		/// <param name="values">  値(List)</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereNotContainedIn (string key, IEnumerable values)
		{
			List<object> valuesList = new List<object> ();
			foreach (object value in values) {
				valuesList.Add (value);
			}
			_addCondition (key, "$nin", valuesList);
			return this;
		}



		/// <summary>
		/// いずれかが含まれる。
		/// </summary>
		/// <param name="key">  条件指定するフィールド名</param>
		/// <param name="values">  値(List)</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereContainedInArray (string key, IEnumerable values)
		{
			List<object> valuesList = new List<object> ();
			foreach (object value in values) {
				valuesList.Add (value);
			}
			_addCondition (key, "$inArray", valuesList);
			return this;
		}


		//現状WhereNotContainedInと全く同じ機能のためコメントアウトしています
		/*
		/// <summary>
		/// いずれも含まれない。
		/// </summary>
		/// <param name="key">  条件指定するフィールド名</param>
		/// <param name="values">  値(List)</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereNotContainedInArray (string key, IEnumerable values)
		{
			List<object> valuesList = new List<object> ();
			foreach (object value in values) {
				valuesList.Add (value);
			}
			_addCondition (key, "$ninArray", valuesList);
			return this;
		}
		*/


		/// <summary>
		/// すべて含まれる。
		/// </summary>
		/// <param name="key">  条件指定するフィールド名</param>
		/// <param name="values">  値(List)</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereContainsAll (string key, IEnumerable values)
		{
			List<object> valuesList = new List<object> ();
			foreach (object value in values) {
				valuesList.Add (value);
			}
			_addCondition (key, "$all", valuesList);
			return this;
		}


		/// <summary>
		/// 副問合わせと合致する、データの取得を行います。
		/// </summary>
		/// <param name="key">  フィールド名</param>
		/// <param name="query">  副問い合わせに使用するクエリ</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereMatchesQuery<TOther> (string key, NCMBQuery<TOther> query)where TOther : NCMBObject
		{
			_addCondition (key, "$inQuery", query);
			return this;
		}

		/// <summary>
		/// 副問合わせと合致する、データの取得を行います。
		/// </summary>
		/// <param name="mainKey">  メインクエリのフィールド名</param>
		/// <param name="subKey">  サブクエリのフィールド名</param>
		/// <param name="query">  サブクエリ</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereMatchesKeyInQuery<TOther> (string mainKey, string subKey, NCMBQuery<TOther> query)where TOther : NCMBObject
		{
			Dictionary<string , object> con = new Dictionary<string, object> ();

			con ["query"] = query;
			con ["key"] = subKey;

			_addCondition (mainKey, "$select", con);
			return this;
		}

		/// <summary>
		/// 子の情報も含めて親情報の取得を行ないます。<br/>
		/// ポインター先の取得を行います。
		/// </summary>
		/// <param name="key">  子オブジェクトのフィールド名</param>
		public void Include (string key)
		{
			this._include.Add (key);
		}

		/// <summary>
		/// 指定位置から近い順にオブジェクト取得を行います。
		/// </summary>
		/// <param name="key">  フィールド名</param>
		/// <param name="point">  指定位置(NCMBGeoPoint)</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereNearGeoPoint (string key, NCMBGeoPoint point)
		{
			object geoPoint = this._geoPointToObject (point);
			_addCondition (key, "$nearSphere", geoPoint);
			return this;
		}

		/// <summary>
		/// 指定位置から指定距離(キロメートル)までの範囲に含まれるオブジェクトの取得を行います。<br/>
		/// 指定位置から近い順に取得します。
		/// </summary>
		/// <param name="key">  フィールド名</param>
		/// <param name="point">  指定位置(NCMBGeoPoint)</param>
		/// <param name="maxDistance">  指定距離</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereGeoPointWithinKilometers (string key, NCMBGeoPoint point, double maxDistance)
		{
			Dictionary<string,object> geoDictionary = this._geoPointToObject (point);
			_addCondition (key, "$nearSphere", geoDictionary);
			_addCondition (key, "$maxDistanceInKilometers", maxDistance);
			return this;
		}


		/// <summary>
		/// 指定位置から指定距離(マイル)までの範囲に含まれるオブジェクトの取得を行います。<br/>
		/// 指定位置から近い順に取得します。
		/// </summary>
		/// <param name="key">  フィールド名</param>
		/// <param name="point">  指定位置(NCMBGeoPoint)</param>
		/// <param name="maxDistance">  指定距離(double)</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereGeoPointWithinMiles (string key, NCMBGeoPoint point, double maxDistance)
		{
			Dictionary<string,object> geoDictionary = this._geoPointToObject (point);
			_addCondition (key, "$nearSphere", geoDictionary);
			_addCondition (key, "$maxDistanceInMiles", maxDistance);

			return this;
		}

		/// <summary>
		/// 指定位置から指定距離(ラジアン)までの範囲に含まれるオブジェクトの取得を行います。<br/>
		/// 指定位置から近い順に取得します。
		/// </summary>
		/// <param name="key">  フィールド名</param>
		/// <param name="point">  指定位置(NCMBGeoPoint)</param>
		/// <param name="maxDistance">  指定距離(double)</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereGeoPointWithinRadians (string key, NCMBGeoPoint point, double maxDistance)
		{
			Dictionary<string,object> geoDictionary = this._geoPointToObject (point);
			_addCondition (key, "$nearSphere", geoDictionary);
			_addCondition (key, "$maxDistanceInRadians", maxDistance);

			return this;
		}

		/// <summary>
		/// 指定された矩形範囲に含まれるオブジェクトの取得を行います。
		/// </summary>
		/// <param name="key">  フィールド名</param>
		/// <param name="southwest">  左下（南西）</param>
		/// <param name="northeast">  右上（北東）</param>
		/// <returns> クエリ</returns>
		public NCMBQuery<T> WhereWithinGeoBox (string key, NCMBGeoPoint southwest, NCMBGeoPoint northeast)
		{
			Dictionary<string,object> geoDictionary = _geoPointToObjectWithinBox (southwest, northeast);
			_addCondition (key, "$within", geoDictionary);
			return this;
		}

		/// <summary>
		/// JSON化するDictionaryの作成。キーは$box　値は南西GeoPointと北東GeoPointが入ったリスト。
		/// </summary>
		private Dictionary<string,object> _geoPointToObjectWithinBox (NCMBGeoPoint southwest, NCMBGeoPoint northeast)
		{
			Dictionary<string,object> geoDic = new Dictionary<string,object> ();
			Dictionary<string,object> jSouthwest = _geoPointToObject (southwest);
			Dictionary<string,object> jNortheast = _geoPointToObject (northeast);

			List<object> List = new List<object> ();
			List.Add (jSouthwest);
			List.Add (jNortheast);

			geoDic ["$box"] = List;
			return geoDic;
		}


		/// <summary>
		/// NCMBGeoPointのJSON化するDictionaryの作成。
		/// </summary>
		private Dictionary<string,object> _geoPointToObject (NCMBGeoPoint point)
		{
			Dictionary<string,object> geoDic = new Dictionary<string,object> ();
			geoDic.Add ("__type", "GeoPoint");
			geoDic.Add ("longitude", point.Longitude);
			geoDic.Add ("latitude", point.Latitude);
			return geoDic;
		}


		/// <summary>
		/// _whereに検索条件($neなど)の追加。
		/// </summary>
		private void _addCondition (string key, string condition, object value)
		{
			Dictionary<string, object> whereValue = null;
			value = NCMBUtility._maybeEncodeJSONObject (value, true);
			if (_where.ContainsKey (key)) {
				//キーの前回条件がWhereEqualToだった場合はint型 それ以外はDictionary型が入る
				//WhereEqualToのみ_addCondition経由で保存されないためint型※仕様上[$]指定がないので
				object existingValue = _where [key];
				if (existingValue is IDictionary) {
					whereValue = (Dictionary<string, object>)existingValue;
				}
			}
			//初回またはキーの前回条件がWhereEqualToだった場合はこちら
			if (whereValue == null) {
				whereValue = new Dictionary<string, object> ();
			}
			whereValue [condition] = value;
			this._where [key] = whereValue;
			NCMBDebug.Log ("【_addCondition】where : " + Json.Serialize (this._where));
		}

		/// <summary>
		/// クエリにマッチするオブジェクトを取得を行います。<br/>
		/// 通信結果を受け取るために必ずコールバックを設定を行います。
		/// </summary>
		/// <param name="callback">  コールバック</param>
		public void FindAsync (NCMBQueryCallback<T> callback)
		{
			if (callback == null) {
				throw new ArgumentException ("It is necessary to always set a callback.");
			}
			this.Find (callback);
		}

		/// <summary>
		/// 同期用の検索メソッド。FindeAsyncで呼び出される。またNCMBObjecとのFetchAllAsyncで扱う。
		/// </summary>
		internal void Find (NCMBQueryCallback<T> callback)
		{
			string url = _getSearchUrl (this._className);//クラス毎のURL作成
			url += WHERE_URL;//「?」を末尾に追加する。
			//条件データの作成
			Dictionary<string, object> beforeJsonData = _getFindParams ();
			url = _makeWhereUrl (url, beforeJsonData);//URLの結合
			ConnectType type = ConnectType.GET;//メソッドタイプの設定

			NCMBDebug.Log ("【url】:" + url + Environment.NewLine + "【type】:" + type);
			//通信処理
			NCMBConnection con = new NCMBConnection (url, type, null, NCMBUser._getCurrentSessionToken ());
			con.Connect (delegate(int statusCode, string responseData, NCMBException error) {
				NCMBDebug.Log ("【StatusCode】:" + statusCode + Environment.NewLine + "【Error】:" + error + Environment.NewLine + "【ResponseData】:" + responseData);

				Dictionary<string , object> resultObj;
				List<T> resultList = new List<T> ();
				ArrayList convertResultList = null;
				//中でエラー処理いるかも
				try {
					if (error == null) {
						resultObj = MiniJSON.Json.Deserialize (responseData) as Dictionary<string, object>;
						convertResultList = _convertFindResponse (resultObj);
						foreach (T obj in convertResultList) {
							resultList.Add (obj);
						}
					}
				} catch (Exception e) {
					error = new NCMBException (e);
				}

				callback (resultList, error);

				return;
			});
		}

		/// <summary>
		/// <para>FindAsync用レスポンス処理</para>
		/// <para>_mergeFromServerを呼びレスポンスデータ(JSONテキスト)からNCMBObject(リスト)を作成する</para>
		/// </summary>
		private ArrayList _convertFindResponse (Dictionary<string , object> response)
		{
			ArrayList answer = new ArrayList ();
			List<object> results = (List<object>)response ["results"];
			if (results == null) {
				Debug.Log ("null results in find response");
			} else {
				object objectClassName = null;
				string resultClassName = null;
				if (response.TryGetValue ("className", out objectClassName)) {
					resultClassName = (string)objectClassName;
				}

				if (resultClassName == null) {
					resultClassName = this._className;
				}

				for (int i = 0; i < results.Count; i++) {
					NCMBObject obj = null;
					if (resultClassName.Equals ("user")) {
						obj = new NCMBUser ();
					} else if (resultClassName.Equals ("role")) {
						obj = new NCMBRole ();
					} else if (resultClassName.Equals ("installation")) {
						obj = new NCMBInstallation ();
					} else if (resultClassName.Equals ("push")) {
						obj = new NCMBPush ();
					} else if (resultClassName.Equals ("file")) {
						obj = new NCMBFile ();
					} else {
						obj = new NCMBObject (resultClassName);
					}
					obj._mergeFromServer ((Dictionary<string , object>)results [i], true);
					answer.Add (obj);
				}
			}
			return answer;
		}

		/// <summary>
		/// 指定IDのオブジェクトを取得を行います。<br/>
		/// 通信結果を受け取るために必ずコールバックを設定を行います。
		/// </summary>
		/// <param name="objectId">  オブジェクトID</param>
		/// <param name="callback">  コールバック</param>
		public void GetAsync (string objectId, NCMBGetCallback<T> callback)
		{
			if (callback == null) {
				throw new ArgumentException ("It is necessary to always set a callback.");
			}

			string url = _getSearchUrl (this._className);//クラス毎のURL作成
			//オブジェクト取得API
			url += "/" + objectId;
			ConnectType type = ConnectType.GET;//メソッドタイプの設定
			//通信処理
			NCMBConnection con = new NCMBConnection (url, type, null, NCMBUser._getCurrentSessionToken ());
			con.Connect (delegate(int statusCode, string responseData, NCMBException error) {
				Dictionary<string , object> resultObj;
				NCMBObject objectData = null;
				try {
					if (error == null) {
						resultObj = MiniJSON.Json.Deserialize (responseData) as Dictionary<string, object>;
						objectData = _convertGetResponse (resultObj);
					}
				} catch (Exception e) {
					error = new NCMBException (e);
				}
				//引数はリスト(中身NCMBObject)とエラーをユーザーに返す
				callback ((T)objectData, error);
				return;
			});

		}


		/// <summary>
		/// <para>GetAsync用レスポンス処理</para>
		/// <para>_mergeFromServerを呼びレスポンスデータ(JSONテキスト)からNCMBObjectを作成する</para>
		/// </summary>
		private NCMBObject _convertGetResponse (Dictionary<string , object> response)
		{
			NCMBObject answer = null;
			if (response == null) {
				Debug.Log ("null results in get response");
			} else {
				string resultClassName = this._className;
				NCMBObject obj = null;
				if (resultClassName.Equals ("user")) {
					obj = new NCMBUser ();
				} else {
					obj = new NCMBObject (resultClassName);
				}
				obj._mergeFromServer (response, true);
				answer = obj;
			}
			return answer;
		}



		/// <summary>
		/// クエリにマッチするオブジェクト数の取得を行います。<br/>
		/// 通信結果を受け取るために必ずコールバックの設定を行います。
		/// </summary>
		/// <param name="callback">  コールバック</param>
		public void CountAsync (NCMBCountCallback callback)
		{
			if (callback == null) {
				throw new ArgumentException ("It is necessary to always set a callback.");
			}


			string url = _getSearchUrl (this._className);//クラス毎のURL作成
			url += WHERE_URL;//「?」をつける

			Dictionary<string, object> beforeJsonData = _getFindParams ();//パラメータDictionaryの作成

			beforeJsonData ["count"] = 1;// カウント条件を追加する

			url = _makeWhereUrl (url, beforeJsonData);//urlにパラメータDictionaryを変換して結合

			ConnectType type = ConnectType.GET;//メソッドタイプの設定
			//通信処理
			NCMBConnection con = new NCMBConnection (url, type, null, NCMBUser._getCurrentSessionToken ());
			con.Connect (delegate(int statusCode, string responseData, NCMBException error) {

				Dictionary<string, object> resultObj;
				int count = 0;
				if (error == null) {
					try {
						resultObj = MiniJSON.Json.Deserialize (responseData) as Dictionary<string, object>;
						object objectCount = null;
						if (resultObj.TryGetValue ("count", out objectCount)) {
							count = Convert.ToInt32 (objectCount);//キーcountの値は必ず同じ型なので型チェック時の変換チェックは無し
						}
					} catch (Exception e) {
						error = new NCMBException (e);
					}
				}
				//引数は検索条件のカウント数とエラーをユーザーに返す
				callback (count, error);

				return;
			});
		}

		//beforejsonDataの各値をJSON化→エンコードしurlに結合する
		private string _makeWhereUrl (string url, Dictionary<string, object> beforejsonData)
		{

			StringBuilder sb = new StringBuilder ();
			sb.Append (url);
			foreach (string key in beforejsonData.Keys) {
				if (key.Equals ("className")) {
					continue;
				}

				Dictionary<string, object> whereDic;
				int intValue;//Json化前のkeyの値【limit】
				string jsonValue;//Json化後のKeyの値
				//where の valueはDictionary型　limit の　valueはint型　
				if (beforejsonData [key] is IDictionary) {
					//whre
					whereDic = (Dictionary<string, object>)beforejsonData [key];
					jsonValue = Json.Serialize (whereDic);
				} else if (beforejsonData [key] is int) {
					//limit
					intValue = (int)beforejsonData [key];
					jsonValue = Json.Serialize (intValue);
				} else {
					//その他
					jsonValue = (string)beforejsonData [key];
				}
				string encodeJsonValue = Uri.EscapeUriString (jsonValue);//JSON化された値をエンコードされた文字列
				encodeJsonValue = encodeJsonValue.Replace (":", "%3A");

				//結合
				sb.Append (key).Append ("=").Append (encodeJsonValue).Append ("&");
			}

			if (beforejsonData.Count > 0) {
				// 最後に追加した&を削除
				sb.Remove (sb.Length - 1, 1);
			}

			return sb.ToString ();
		}


		/// <summary>
		/// URLに結合する(where以降)データの作成。<br/>
		/// URLと結合するときにJSON化を行う。
		/// </summary>
		private Dictionary<string, object> _getFindParams ()
		{
			Dictionary<string, object> beforeJsonData = new Dictionary<string, object> ();
			Dictionary<string, object> whereData = new Dictionary<string, object> ();
			beforeJsonData ["className"] = this._className;

			//$or検索も出来るようにする
			foreach (string key in this._where.Keys) {
				if (key.Equals ("$or")) {
					List<NCMBQuery<T>> queries = (List<NCMBQuery<T>>)this._where [key];

					List<object> array = new List<object> ();
					foreach (NCMBQuery<T> query in queries) {
						if (query._limit >= 0) {
							throw new ArgumentException ("Cannot have limits in sub queries of an 'OR' query");
						}
						if (query._skip > 0) {
							throw new ArgumentException ("Cannot have skips in sub queries of an 'OR' query");
						}
						if (query._order.Count > 0) {
							throw new ArgumentException ("Cannot have an order in sub queries of an 'OR' query");
						}
						if (query._include.Count > 0) {
							throw new ArgumentException ("Cannot have an include in sub queries of an 'OR' query");
						}
						Dictionary<string,object> dic = query._getFindParams ();
						if (dic ["where"] != null) {
							//where=有
							array.Add (dic ["where"]);
						} else {
							//where=無
							array.Add (new Dictionary<string, object> ());
						}
					}
					whereData [key] = array;
				} else {

					object value = _encodeSubQueries (this._where [key]);
					whereData [key] = NCMBUtility._maybeEncodeJSONObject (value, true);//※valueの値に注意
				}
			}
			beforeJsonData ["where"] = whereData;
			//各オプション毎の設定

			if (this._limit >= 0) {
				beforeJsonData ["limit"] = this._limit;
			}

			if (this._skip > 0) {
				beforeJsonData ["skip"] = this._skip;
			}

			if (this._order.Count > 0) {
				beforeJsonData ["order"] = _join (this._order, ",");
			}

			if (this._include.Count > 0) {
				beforeJsonData ["include"] = _join (this._include, ",");
			}

			NCMBDebug.Log ("【_getFindParams】beforeJsonData : " + Json.Serialize (beforeJsonData));
			return beforeJsonData;
		}

		private object _encodeSubQueries (object value)
		{
			if (!(value is IDictionary)) {
				return value;
			}

			Dictionary<string , object> jsonBefore = (Dictionary<string , object>)value;
			//Dictionary<string , object> jsonAfter = null;//foreachの中でjsonBeforeに値を追加しているため永遠とforeachが回るのを防ぐ

			Dictionary<string , object> jsonAfter = new Dictionary<string,object> ();
			foreach (KeyValuePair<string, object> pair in jsonBefore) {

				if (pair.Value is NCMBQuery<NCMBObject>) {
					NCMBQuery<NCMBObject> query = (NCMBQuery<NCMBObject>)pair.Value;
					Dictionary<string , object> realData = query._getFindParams ();
					realData ["where"] = realData ["where"];
					jsonAfter [pair.Key] = realData;
				} else if (pair.Value is NCMBQuery<NCMBUser>) {
					NCMBQuery<NCMBUser> query = (NCMBQuery<NCMBUser>)pair.Value;
					Dictionary<string , object> realData = query._getFindParams ();
					realData ["where"] = realData ["where"];
					jsonAfter [pair.Key] = realData;
				} else if (pair.Value is NCMBQuery<NCMBRole>) {
					NCMBQuery<NCMBRole> query = (NCMBQuery<NCMBRole>)pair.Value;
					Dictionary<string , object> realData = query._getFindParams ();
					realData ["where"] = realData ["where"];
					jsonAfter [pair.Key] = realData;
				} else if (pair.Value is NCMBQuery<NCMBInstallation>) {
					NCMBQuery<NCMBInstallation> query = (NCMBQuery<NCMBInstallation>)pair.Value;
					Dictionary<string , object> realData = query._getFindParams ();
					realData ["where"] = realData ["where"];
					jsonAfter [pair.Key] = realData;
				} else if (pair.Value is NCMBQuery<NCMBPush>) {
					NCMBQuery<NCMBPush> query = (NCMBQuery<NCMBPush>)pair.Value;
					Dictionary<string , object> realData = query._getFindParams ();
					realData ["where"] = realData ["where"];
					jsonAfter [pair.Key] = realData;
				} else if (pair.Value is IDictionary) {
					jsonAfter [pair.Key] = _encodeSubQueries (pair.Value);
				} else {
					jsonAfter [pair.Key] = pair.Value;//前
				}
			}
			return jsonAfter;
		}

		/// <summary>
		/// <para>_includeの設定を取り出し文字列に変換する。_includeが複数合った場合は「,」で連結して返す</para>
		///<para>_orderも同様の仕様に変更</para>
		/// </summary>
		private string _join (List<string> items, string delimiter)
		{
			StringBuilder buffer = new StringBuilder ();

			foreach (string iter in items) {
				//itemsの要素数が2個以上なら「,」で区切る
				if (buffer.Length > 0) {
					buffer.Append (delimiter);
				}
				buffer.Append (iter);
			}
			return buffer.ToString ();
		}


		/// <summary>
		/// 各型毎のURL作成
		/// </summary>
		private string _getSearchUrl (string className)
		{
			string url = "";
			if (className == null || className.Equals ("")) {
				throw new ArgumentException ("Not class name error. Please be sure to specify the class name.");
			} else if (className.Equals ("push")) {
				// プッシュ検索API
				url = new NCMBPush ()._getBaseUrl ();
			} else if (className.Equals ("installation")) {
				// 配信端末検索API
				url = new NCMBInstallation ()._getBaseUrl ();
			} else if (className.Equals ("file")) {
				// ファイル検索API
				url = new NCMBFile ()._getBaseUrl ();
			} else if (className.Equals ("user")) {
				// 会員検索API
				//url = new NCMBUser().getBaseUrl(NCMBUser.URL_TYPE_USER);
				url = new NCMBUser ()._getBaseUrl ();
			} else if (className.Equals ("role")) {
				// ロール検索API
				url = new NCMBRole ()._getBaseUrl ();
			} else {
				// オブジェクト検索API
				url = new NCMBObject (_className)._getBaseUrl ();
			}
			return url;
		}


		/// <summary>
		/// クラスの新しいクエリを作成します。
		/// </summary>
		/// <param name="className"> クラス名</param>
		/// <returns> クエリ</returns>
		public static NCMBQuery<T> GetQuery (string className)
		{
			return new NCMBQuery<T> (className);
		}

		internal NCMBQuery<T> _whereRelatedTo (NCMBObject parent, String key)
		{
			this._addCondition ("$relatedTo", "object", NCMBUtility._maybeEncodeJSONObject (parent, true));
			this._addCondition ("$relatedTo", "key", key);
			return this;
		}


	}
}
