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

using System.Collections;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using MiniJSON;
using NCMB.Internal;
using System.Linq;
using UnityEngine;

namespace  NCMB
{

	/// <summary>
	/// 会員管理を操作するクラスです。
	/// </summary>
	[NCMBClassName("user")]
	public class NCMBUser : NCMBObject
	{


		private static NCMBUser _currentUser = null;
		internal bool _isCurrentUser;
		
		/// <summary>
		/// ユーザ名の取得、または設定を行います。
		/// </summary>
		public string UserName {
			get {
				return (string)this ["userName"];
			}
			set {
				this ["userName"] = value;
			}
		}
		
		/// <summary>
		/// Eメールの取得、または設定を行います。
		/// </summary>		
		public string Email {
			get {
				return (string)this ["mailAddress"];
			}
			set {
				this ["mailAddress"] = value;
			}
		}
		
		/// <summary>
		/// パスワードの設定を行います。
		/// </summary>		
		public string Password {
			private get {
				return (string)this ["password"];
			}
			set {
				object obj;
				Monitor.Enter (obj = this.mutex);
				try {
					this ["password"] = value;
					this.IsDirty = true;
				} finally {
					Monitor.Exit (obj);
				}
			}
		}
		
		/// <summary>
		/// ログイン中のユーザセッショントークンを取得を行います。
		/// </summary>
		public string SessionToken {
			get {
				return (string)this ["sessionToken"];
			}
			internal set {//注意：下記コード実行で履歴データ(NCMBSetOperation)がセットされる
				this ["sessionToken"] = value;
			}
		}
		
		/// <summary>
		/// ログイン中のユーザ情報の取得を行います。
		/// </summary>		
		public static NCMBUser CurrentUser {
			get {
				if (_currentUser != null) {
					return _currentUser;
				}
				NCMBUser objUser = null;
				objUser = (NCMBUser)_getFromVariable (); //Get from variable first
				if (objUser == null) {
					objUser = (NCMBUser)_getFromDisk ("currentUser"); //If not exist from global variable, then get from disk
				}
				if (objUser != null) {
					_currentUser = objUser;
					_currentUser._isCurrentUser = true;
					return _currentUser;
				}
				return null;	
			}
		}
		
		/// <summary>
		/// コンストラクター。
		/// </summary>	
		public NCMBUser () : base()
		{
			this._isCurrentUser = false;
		}
		
		// キーを設定するときのバリデーション
		internal override void _onSettingValue (string key, object value)
		{
			base._onSettingValue (key, value);		
		}
		
		/// <summary>
		/// ユーザを追加します。<br/>
		/// すでにあるキーを指定した場合はExceptionを投げます。
		/// </summary>
		/// <param name="key">キー</param>
		/// <param name="value">値</param>
		public override void Add (string key, object value)
		{
			if ("userName".Equals (key)) {
				throw new NCMBException ("userName key is already exist. Use this.UserName to set it");
				//remove Anonymous login information (if needed)
			}
			if ("password".Equals (key)) {
				throw new NCMBException ("password key is already exist. Use this.Password to set it");
			}
			if ("mailAddress".Equals (key)) {
				throw new NCMBException ("mailAdress key is already exist. Use this.Email to set it");
			}
			base.Add (key, value);
		}
		
		/// <summary>
		/// 指定したキーのフィールドが存在する場合、フィールドを削除します。
		/// </summary>
		/// <param name="key">フィールド名</param>
		public override void Remove (string key)
		{
			if ("userName".Equals (key)) {
				throw new NCMBException ("Can not remove the userName key");
			}
			if ("password".Equals (key)) {
				throw new NCMBException ("Can not remove the Password key");
			}
			base.Remove (key);
		}
		
		/// <summary>
		/// ユーザ内のオブジェクトで使用出来るクエリを取得します。
		/// </summary>
		/// <returns> クエリ</returns>
		public static NCMBQuery<NCMBUser> GetQuery ()
		{
			return NCMBQuery<NCMBUser>.GetQuery ("user");
		}
		
		internal override string _getBaseUrl ()
		{
			return CommonConstant.DOMAIN_URL + "/" + CommonConstant.API_VERSION + "/users";
		}
		
		internal static string _getLogInUrl ()
		{
			return CommonConstant.DOMAIN_URL + "/" + CommonConstant.API_VERSION + "/login";
		}
		
		internal static string _getLogOutUrl ()
		{
			return CommonConstant.DOMAIN_URL + "/" + CommonConstant.API_VERSION + "/logout";
		}
		
		internal static string _getRequestPasswordResetUrl ()
		{
			return CommonConstant.DOMAIN_URL + "/" + CommonConstant.API_VERSION + "/requestPasswordReset";
		}

		private static string _getmailAddressUserEntryUrl ()
		{
			return CommonConstant.DOMAIN_URL + "/" + CommonConstant.API_VERSION + "/requestMailAddressUserEntry";
		}

		//save後処理 　オーバーライド用　新規登録時のみログインを行う
		internal override void _afterSave (int statusCode, NCMBException error)
		{
			if (statusCode == 201 && error == null) {
				_saveCurrentUser ((NCMBUser)this);
			}
		}

		//delete後処理 　オーバーライド用
		internal override void _afterDelete (NCMBException error)
		{
			if (error == null) {
				_logOutEvent ();
			}
		}
		
		/// <summary>
		/// ユーザの削除を行います。<br/>
		/// 通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		public override void DeleteAsync ()
		{
			this.DeleteAsync (null);
		}
		
		/// <summary>
		/// ユーザの削除を行います。<br/>
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="callback">コールバック</param>
		public override void DeleteAsync (NCMBCallback callback)
		{	
			base.DeleteAsync (callback);
			//Cleanup authdata for other Services if needed
		}


		/// <summary>
		/// 非同期処理でユーザを登録します。<br/>
		/// オブジェクトIDが登録されていない新規会員ならログインし、登録を行います。<br/>
		/// オブジェクトIDが登録されている既存会員ならログインせず、更新を行います。<br/>
		/// 既存会員のログインはLogInAsyncメソッドをご利用下さい。<br/>
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="callback">コールバック</param>
		public void SignUpAsync (NCMBCallback callback)
		{
			base.SaveAsync (callback);
			//Cleanup authdata for other Services if needed
		}
		
		/// <summary>
		/// 非同期処理でユーザを登録します。<br/>
		/// ユーザ登録が成功の場合、自動的にログインの状態になります。<br/>
		///通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		public void SignUpAsync ()
		{
			this.SignUpAsync (null);
		}
		
		/// <summary>
		/// 非同期処理でユーザの保存を行います。<br/>
		/// SaveAsync()を実行してから編集などをしていなく、保存をする必要が無い場合は通信を行いません。<br/>
		/// オブジェクトIDが登録されていない新規会員ならログインし、登録を行います。<br/>
		/// オブジェクトIDが登録されている既存会員ならログインせず、更新を行います。<br/>
		/// 既存会員のログインはLogInAsyncメソッドをご利用下さい。<br/>
		/// 通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		public override void SaveAsync ()
		{
			this.SaveAsync (null);
		}
		
		/// <summary>
		/// 非同期処理でユーザの保存を行います。<br/>
		/// SaveAsync()を実行してから編集などをしていなく、保存をする必要が無い場合は通信を行いません。<br/>
		/// オブジェクトIDが登録されていない新規会員ならログインし、登録を行います。<br/>
		/// オブジェクトIDが登録されている既存会員ならログインせず、更新を行います。<br/>
		/// 既存会員のログインはLogInAsyncメソッドをご利用下さい。<br/>
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="callback">コールバック</param>
		public override void SaveAsync (NCMBCallback callback)
		{	
			base.SaveAsync (callback);
		}
		
		internal static void _saveCurrentUser (NCMBUser user)
		{
			object obj;
			if (_currentUser != null) {
				Monitor.Enter (obj = _currentUser.mutex);
				try {
					if ((_currentUser != null) && (_currentUser != user)) {
						_logOutEvent ();
					}
				} finally {
					Monitor.Exit (obj);
				}
			}
			object obj_user;
			Monitor.Enter (obj_user = user.mutex);
			try {
				user._isCurrentUser = true;
				//synchronize all auth data of other services
				user._saveToDisk ("currentUser"); //Save disk
				user._saveToVariable (); //Save to variable
				_currentUser = user;
			} finally {
				Monitor.Exit (obj_user);
			}
		}
		
		internal static void _logOutEvent ()
		{
			string filePath = NCMBSettings.filePath + "/" + "currentUser";
			if (_currentUser != null) {
				//logOut with other linked services
				_currentUser._isCurrentUser = false;
			}
			_currentUser = null;
			//delete from disk "currentUser"
			try {
				if (File.Exists (filePath)) {
					File.Delete (filePath);
				}
				NCMBSettings.CurrentUser = "";
			} catch (Exception e) {
				throw new NCMBException (e);
			}
		}
		
		
		internal static string _getCurrentSessionToken ()
		{	
			if (CurrentUser != null) {
				return CurrentUser.SessionToken;
			}
			return "";
		}
		
		/// <summary>
		/// 認証済みか判定を行います。
		/// </summary>
		/// <returns> true:認証済　false:未認証 </returns>
		public bool IsAuthenticated ()
		{
			return ((this.SessionToken != null) && (CurrentUser != null) && (CurrentUser.ObjectId.Equals (this.ObjectId)));
		}
		
		/// <summary>
		/// 非同期処理でユーザのパスワード再発行依頼を行います。<br/>
		/// 通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		/// <param name="email">メールアドレス</param>
		public static void RequestPasswordResetAsync (string email)
		{
			RequestPasswordResetAsync (email, null);
		}
		
		
		/// <summary>
		/// 非同期処理でユーザのパスワード再発行依頼を行います。<br/>
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="email">メールアドレス</param>
		/// <param name="callback">コールバック</param>
		public static void RequestPasswordResetAsync (string email, NCMBCallback callback)
		{
			//request通信を実施
			new AsyncDelegate (delegate {
				_requestPasswordReset (email, callback);
			}).BeginInvoke ((IAsyncResult r) => {
			}, null);
		}
		
		internal static void _requestPasswordReset (string email, NCMBCallback callback)
		{
			string url = _getRequestPasswordResetUrl ();//URL
			ConnectType type = ConnectType.POST;
			//set username, password
			NCMBUser pwresetUser = new NCMBUser ();
			pwresetUser.Email = email;
			string content = pwresetUser._toJSONObjectForSaving (pwresetUser.StartSave ());
			//ログを確認（通信前）
			NCMBDebug.Log ("【url】:" + url + Environment.NewLine + "【type】:" + type + Environment.NewLine + "【content】:" + content);
			//通信処理
			NCMBConnection con = new NCMBConnection (url, type, content, NCMBUser._getCurrentSessionToken ());
			con.Connect (delegate(int statusCode , string responseData, NCMBException error) {
				try {
					NCMBDebug.Log ("【StatusCode】:" + statusCode + Environment.NewLine + "【Error】:" + error + Environment.NewLine + "【ResponseData】:" + responseData);
					if (error != null) {		
						NCMBDebug.Log ("[DEBUG AFTER CONNECT] Error: " + error.ErrorMessage);
					} else {
						NCMBDebug.Log ("[DEBUG AFTER CONNECT] Successful: ");
					}
				} catch (Exception e) {
					error = new NCMBException (e);
				}
				if (callback != null) {
					Platform.RunOnMainThread (delegate {
						callback (error);
					});
				}
				return;
			});
		}
		
		
		/// <summary>
		/// 非同期処理でユーザ名とパスワードを指定して、ユーザのログインを行います。<br/>
		/// 通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		/// <param name="name">ユーザ名</param>
		/// <param name="password">パスワード</param>
		public static void LogInAsync (string name, string password)
		{
			LogInAsync (name, password, null);		
		}
		
		/// <summary>
		/// 非同期処理でユーザ名とパスワードを指定して、ユーザのログインを行います。<br/>
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="name">ユーザ名</param>
		/// <param name="password">パスワード</param>
		/// <param name="callback">コールバック</param>
		public static void LogInAsync (string name, string password, NCMBCallback callback)
		{
			//logIn通信を実施
			new AsyncDelegate (delegate {
				_ncmbLogIn (name, password, null, callback);
			}).BeginInvoke ((IAsyncResult r) => {
			}, null);
		}

		private  static void _ncmbLogIn (string name, string password, string email, NCMBCallback callback)
		{
			string url = _getLogInUrl ();//URL作成
			ConnectType type = ConnectType.GET;
			//set username, password
			NCMBUser logInUser = new NCMBUser ();

			logInUser.Password = password;

			//nameがあればLogInAsync経由　無ければLogInWithMailAddressAsync経由、どちらも無ければエラー
			if (name != null) {
				logInUser.UserName = name;
			} else if (email != null) {
				logInUser.Email = email;
			} else {
				throw new NCMBException (new ArgumentException ("UserName or Email can not be null."));
			}

			string content = logInUser._toJSONObjectForSaving (logInUser.StartSave ());
			Dictionary<string, object> paramDic = (Dictionary<string, object>)MiniJSON.Json.Deserialize (content);
			url = _makeParamUrl (url + "?", paramDic);
			//ログを確認（通信前）
			NCMBDebug.Log ("【url】:" + url + Environment.NewLine + "【type】:" + type + Environment.NewLine + "【content】:" + content);
			//通信処理
			NCMBConnection con = new NCMBConnection (url, type, content, NCMBUser._getCurrentSessionToken ());
			con.Connect (delegate(int statusCode , string responseData, NCMBException error) {
				try {
					NCMBDebug.Log ("【StatusCode】:" + statusCode + Environment.NewLine + "【Error】:" + error + Environment.NewLine + "【ResponseData】:" + responseData);
					if (error != null) {		
						NCMBDebug.Log ("[DEBUG AFTER CONNECT] Error: " + error.ErrorMessage);
					} else {
						Dictionary<string, object> responseDic = MiniJSON.Json.Deserialize (responseData) as Dictionary<string, object>;
						logInUser._handleFetchResult (true, responseDic);
						//save Current user
						_saveCurrentUser (logInUser);
						
					}
				} catch (Exception e) {
					error = new NCMBException (e);
				}
				if (callback != null) {
					Platform.RunOnMainThread (delegate {
						callback (error);
					});
				}
				return;
			});	
		}
		
		private static string _makeParamUrl (string url, Dictionary<string, object> parameter)
		{
			string result = url;
			foreach (KeyValuePair<string, object> pair in parameter) {
				//result += pair.Key + "=" + NCMBUtility._encodeString ((string)pair.Value) + "&"; //**Encoding が必要
				result += pair.Key + "=" + WWW.EscapeURL ((string)pair.Value) + "&"; //**Encoding が必要
			}
			if (parameter.Count > 0) {
				result = result.Remove (result.Length - 1);
			}
			return result;
		}

		/// <summary>
		/// 非同期処理でメールアドレスとパスワードを指定して、ユーザのログインを行います。<br/>
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="email">メールアドレス</param>
		/// <param name="password">パスワード</param>
		/// <param name="callback">コールバック</param>
		public static void LogInWithMailAddressAsync (string email, string password, NCMBCallback callback)
		{
			new AsyncDelegate (delegate {
				_ncmbLogIn (null, password, email, callback);
			}).BeginInvoke ((IAsyncResult r) => {
			}, null);
		}

		/// <summary>
		/// 非同期処理でメールアドレスとパスワードを指定して、ユーザのログインを行います。<br/>
		/// 通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		/// <param name="email">メールアドレス</param>
		/// <param name="password">パスワード</param>
		public static void LogInWithMailAddressAsync (string email, string password)
		{
			new AsyncDelegate (delegate {
				_ncmbLogIn (null, password, email, null);
			}).BeginInvoke ((IAsyncResult r) => {
			}, null);
		}

		/// <summary>
		/// 非同期処理で指定したメールアドレスに対して、<br/>
		/// 会員登録を行うためのメールを送信するよう要求します。<br/>
		/// 通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		/// <param name="email">メールアドレス</param>
		public static void RequestAuthenticationMailAsync (string email)
		{
			RequestAuthenticationMailAsync (email, null);
		}

		/// <summary>
		/// 非同期処理で指定したメールアドレスに対して、<br/>
		/// 会員登録を行うためのメールを送信するよう要求します。<br/>
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="email">メールアドレス</param>
		/// <param name="callback">コールバック</param>
		public static void RequestAuthenticationMailAsync (string email, NCMBCallback callback)
		{
			new AsyncDelegate (delegate {
				//URL
				string url = _getmailAddressUserEntryUrl ();//URL

				//コンテント
				NCMBUser user = new NCMBUser ();
				user.Email = email;
				string content = user._toJSONObjectForSaving (user.StartSave ());

				//Type	
				ConnectType type = ConnectType.POST;

				NCMBConnection con = new NCMBConnection (url, type, content, NCMBUser._getCurrentSessionToken ());
				NCMBDebug.Log ("【url】:" + url + Environment.NewLine + "【type】:" + type + Environment.NewLine + "【content】:" + content);
				con.Connect (delegate(int statusCode , string responseData, NCMBException error) {
					NCMBDebug.Log ("【StatusCode】:" + statusCode + Environment.NewLine + "【Error】:" + error + Environment.NewLine + "【ResponseData】:" + responseData);
					if (callback != null) {
						Platform.RunOnMainThread (delegate {
							callback (error);
						});
					}
					return;
				});	
			}).BeginInvoke ((IAsyncResult r) => {
			}, null);
		}


		/// <summary>
		/// 非同期処理でユーザのログアウトを行います。<br/>
		/// 通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		public static void LogOutAsync ()
		{
			LogOutAsync (null);
		}
		
		
		/// <summary>
		/// 非同期処理でユーザのログアウトを行います。<br/>
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="callback">コールバック</param>
		public static void LogOutAsync (NCMBCallback callback)
		{
			if (_currentUser != null) {
				//logOut通信を実施
				new AsyncDelegate (delegate {
					_logOut (callback);
				}).BeginInvoke ((IAsyncResult r) => {
				}, null);
			} else {
				try {
					_logOutEvent ();
				} catch (NCMBException e) {
					if (callback != null) {
						Platform.RunOnMainThread (delegate {
							callback (e);
						});
					}
					return;
				}
				if (callback != null) {
					Platform.RunOnMainThread (delegate {
						callback (null);
					});
				}
				
			}
		}
		
		internal static void _logOut (NCMBCallback callback)
		{
			string url = _getLogOutUrl ();//URL作成
			ConnectType type = ConnectType.GET;
			string content = null;
			//ログを確認（通信前）
			NCMBDebug.Log ("【url】:" + url + Environment.NewLine + "【type】:" + type + Environment.NewLine + "【content】:" + content);
			//通信処理
			NCMBConnection con = new NCMBConnection (url, type, content, NCMBUser._getCurrentSessionToken ());
			con.Connect (delegate(int statusCode , string responseData, NCMBException error) {
				try {
					NCMBDebug.Log ("【StatusCode】:" + statusCode + Environment.NewLine + "【Error】:" + error + Environment.NewLine + "【ResponseData】:" + responseData);
					if (error != null) {		
						NCMBDebug.Log ("[DEBUG AFTER CONNECT] Error: " + error.ErrorMessage);
					} else {
						_logOutEvent ();
					}
				} catch (Exception e) {
					error = new NCMBException (e);
				}
				if (callback != null) {
					Platform.RunOnMainThread (delegate {
						callback (error);
					});
				}
				return;
			});	
		}
		
		internal override void _mergeFromServer (Dictionary<string, object> responseDic, bool completeData)
		{
			
			base._mergeFromServer (responseDic, completeData);
		}
		
	}
}

