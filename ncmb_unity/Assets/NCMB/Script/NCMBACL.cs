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

using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using MiniJSON;
using NCMB.Internal;
using System.Linq;
using System.Text;

namespace NCMB
{
	/// <summary>
	/// アクセス制御を操作するクラスです。
	/// </summary>
	public class NCMBACL
	{
		private static NCMBACL defaultACL; //オブジェクト作成時に設定されるACL
		private Dictionary<string,object> permissionsById; //string⇒ObjectIdまたはrole:ロール名,object⇒permissions
		private bool shared; //デフォルトACL設定かどうか
		private static NCMBACL defaultACLWithCurrentUser; //デフォルトACLでログイン中ユーザに権限を付与する場合に利用
		private static bool defaultACLUsesCurrentUser; //デフォルトACLでログイン中ユーザに権限を付与するかどうか

		/// <summary>
		/// パブリック読込権限の取得、または設定を行います。
		/// </summary>
		public bool PublicReadAccess {
			set {
				SetReadAccess ("*", value);
			}
			get {
				return GetReadAccess ("*");
			}
		}

		/// <summary>
		/// パブリック書込権限の取得、または設定を行います。
		/// </summary>
		public bool PublicWriteAccess {
			set {
				SetWriteAccess ("*", value);
			}
			get {
				return GetWriteAccess ("*");
			}
		}

		/// <summary>
		/// コンストラクター。
		/// </summary>
		public NCMBACL ()
		{
			this.permissionsById = new Dictionary<string, object> ();
		}

		/// <summary>
		/// コンストラクター。<br/>
		/// 指定したobjectIdのNCMBUserに読み込み、書き込み権限を付与します。
		/// </summary>
		/// <param name="objectId">NCMBUserのobjectId</param>
		public NCMBACL (string objectId):this()
		{
			if (objectId == null) {
				throw new NCMBException (new ArgumentException ("objectId may not be null "));
			}
			SetWriteAccess (objectId, true);
			SetReadAccess (objectId, true);
		}


		internal bool _isShared ()
		{
			return this.shared;
		}

		internal void _setShared (bool shared)
		{
			this.shared = shared;
		}

		//現在のACL設定を複製
		internal NCMBACL _copy ()
		{
			NCMBACL copy = new NCMBACL ();
			try {
				copy.permissionsById = new Dictionary<string, object> (this.permissionsById);
			} catch (NCMBException e) {
				throw new NCMBException (e);
			}
			//unresolvedユーザ処理必要ならここに書く
			return copy;
		}

		/// <summary>
		/// ユーザ読込権限の設定を行います。
		/// </summary>
		/// <param name="objectId">NCMBUserのobjectId</param>
		/// <param name="allowed">true:許可　false:不許可</param>
		public void SetReadAccess (String objectId, bool allowed)
		{
			if (objectId == null) {
				throw new NCMBException (new ArgumentException ("cannot SetReadAccess for null objectId "));
			}
			_setAccess ("read", objectId, allowed);
		}

		/// <summary>
		/// ユーザ書込権限の設定を行います。
		/// </summary>
		/// <param name="objectId">NCMBUserのObjectId</param>
		/// <param name="allowed">true:許可　false:不許可</param>
		public void SetWriteAccess (String objectId, bool allowed)
		{
			if (objectId == null) {
				throw new NCMBException (new ArgumentException ("cannot SetWriteAccess for null objectId "));

			}
			_setAccess ("write", objectId, allowed);
		}

		/// <summary>
		/// ロール読込権限の設定を行います。
		/// </summary>
		/// <param name="roleName">NCMBRoleのName</param>
		/// <param name="allowed">true:許可　false:不許可</param>
		public void SetRoleReadAccess (string roleName, bool allowed)
		{
			SetReadAccess ("role:" + roleName, allowed);
		}

		/// <summary>
		/// ロール書込権限の設定を行います。
		/// </summary>
		/// <param name="roleName">NCMBRoleのName</param>
		/// <param name="allowed">true:許可　false:不許可</param>
		public void SetRoleWriteAccess (String roleName, bool allowed)
		{
			SetWriteAccess ("role:" + roleName, allowed);
		}

		/// <summary>
		/// デフォルトACLの設定を行います。
		/// </summary>
		/// <param name="acl">ACL</param>
		/// <param name="withAccessForCurrentUser">true:オブジェクト作成者に対するフルアクセスをデフォルトで許可する</param>
		public static void SetDefaultACL (NCMBACL acl, bool withAccessForCurrentUser)
		{
			defaultACLWithCurrentUser = null;
			if (acl != null) {
				defaultACL = acl._copy ();
				defaultACL._setShared (true);
				defaultACLUsesCurrentUser = withAccessForCurrentUser;
			} else {
				defaultACL = null;
			}
		}

		//permissionsByIdにobjectIdのaccessType（read,write）権限allowed（true,false）を設定
		private void _setAccess (string accessType, string objectId, bool allowed)
		{
			try {
				Dictionary<string,object> permissions = null;
				Object value;
				if (this.permissionsById.TryGetValue (objectId, out value)) {
					permissions = (Dictionary<string,object>)value;
				}

				if (permissions == null) {
					if (!allowed) {
						return;
					}
					permissions = new Dictionary<string, object> ();
					this.permissionsById [objectId] = permissions;
				}

				if (allowed) {
					permissions [accessType] = true;
				} else {
					permissions.Remove (accessType);
					if (permissions.Count == 0) {
						this.permissionsById.Remove (objectId);
					}
				}
			} catch (NCMBException e) {
				throw new NCMBException (new ArgumentException ("JSON failure with ACL: " + e.GetType ().ToString ()));

			}
		}

		/// <summary>
		/// ユーザ読込権限の取得を行います。
		/// </summary>
		/// <param name="objectId">NCMBUserのobjectId</param>
		/// <returns> true:許可　false:不許可 </returns>
		public bool GetReadAccess (String objectId)
		{
			if (objectId == null) {
				throw new NCMBException (new ArgumentException ("cannot GetReadAccess for null objectId "));

			}
			return _getAccess ("read", objectId);
		}

		/// <summary>
		/// ユーザ書込権限の取得を行います。
		/// </summary>
		/// <param name="objectId">NCMBUserのobjectId</param>
		/// <returns> true:許可　false:不許可 </returns>
		public bool GetWriteAccess (String objectId)
		{
			if (objectId == null) {
				throw new NCMBException (new ArgumentException ("cannot GetWriteAccess for null objectId "));

			}
			return _getAccess ("write", objectId);
		}

		/// <summary>
		/// ロール読込権限の取得を行います。
		/// </summary>
		/// <param name="roleName">NCMBRoleのName</param>
		/// <returns> true:許可　false:不許可 </returns>
		public bool GetRoleReadAccess (string roleName)
		{
			return GetReadAccess ("role:" + roleName);
		}

		/// <summary>
		/// ロール書込権限の取得を行います。
		/// </summary>
		/// <param name="roleName">NCMBRoleのName</param>
		/// <returns> true:許可　false:不許可 </returns>
		public bool GetRoleWriteAccess (string roleName)
		{
			return GetWriteAccess ("role:" + roleName);
		}

		//オブジェクト生成時にデフォルトACLを付与
		internal static NCMBACL _getDefaultACL ()
		{
			//ログイン中ユーザの権限付与をする場合
			if ((defaultACLUsesCurrentUser) && (defaultACL != null)) {
				//ログイン中ユーザが存在しないか、ストア登録されていない場合
				if (NCMBUser.CurrentUser == null || NCMBUser.CurrentUser.ObjectId == null) {
					return defaultACL;
				}
				//ログイン中ユーザが存在し、ストア登録されている場合
				defaultACLWithCurrentUser = defaultACL._copy ();
				defaultACLWithCurrentUser._setShared (true);
				defaultACLWithCurrentUser.SetReadAccess (NCMBUser.CurrentUser.ObjectId, true);
				defaultACLWithCurrentUser.SetWriteAccess (NCMBUser.CurrentUser.ObjectId, true);

				return defaultACLWithCurrentUser;
			}
			return defaultACL;
		}

		//permissionsByIdからobjectIdのaccessType（read,write）の権限情報を（true,false）を取得
		private bool _getAccess (string accessType, string objectId)
		{
			try {
				Dictionary<string,object> permissions = null;
				Object value;
				if (this.permissionsById.TryGetValue (objectId, out value)) {
					permissions = (Dictionary<string,object>)value;
				}

				if (permissions == null) {
					return false;
				}

				if (!permissions.TryGetValue (accessType, out value)) {
					return false;
				}
				return (bool)value;

			} catch (NCMBException e) {
				throw new NCMBException (new ArgumentException ("JSON failure with ACL: " + e.GetType ().ToString ()));
			}
		}


		internal IDictionary<string, object> _toJSONObject ()
		{
			return this.permissionsById;
		}

		//サーバーから取得したACL(各権限)データをNCMBACLに保持する
		internal static NCMBACL _createACLFromJSONObject (Dictionary<string,object> aclValue)
		{
			//例:aclValue = {"hPRdWgQILfsyZ6zE":{"write":true,"read":true},"*":{"read":true}}
			//falseは無くtrueのみサーバーから返却される仕様
			NCMBACL acl = new NCMBACL ();

			if (aclValue != null) {
				//aclValueのvalueを保持する
				//例：{"write":true,"read":true}や{"read":true}を保持する。tureのみ。
				Dictionary<string,object> trueDic;

				//各権限(User,Role,public=*)をpermissionsByIdに設定する
				//例：objPair　key:hPRdWgQILfsyZ6zE value:{"write":true,"read":true}　など
				foreach (KeyValuePair<string, object> objPair in aclValue) {
					trueDic = (Dictionary<string,object>)objPair.Value;

					//例：permissionsPair　key:write value:true　など
					foreach (KeyValuePair<string, object> permissionsPair in trueDic) {
						//サーバーからはtureのものしか返却されないので第三引数はtrueで固定
						acl._setAccess (permissionsPair.Key, objPair.Key, true);
					}
				}
			}
			return acl;
		}

	}
}
