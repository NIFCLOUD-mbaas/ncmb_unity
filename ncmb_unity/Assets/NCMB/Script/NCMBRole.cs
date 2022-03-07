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

using System;
using NCMB.Internal;
using System.Text.RegularExpressions;

namespace NCMB
{
	/// <summary>
	/// ロールを操作するクラスです。
	/// </summary>
	[NCMBClassName("role")]
	public class NCMBRole : NCMBObject
	{
		//入力された文字列がroleNameとして適当か調べるなど。
		private static readonly Regex namePattern = new Regex ("^[0-9a-zA-Z_\\- ]+$");

		// コンストラクター
		// NCMBQueryの_getSearchUrlで使用。※_getBaseUrlにアクセスするため
		internal NCMBRole ()
		{
		}

		/// <summary>
		/// コンストラクター。<br/>
		/// ロールの作成を行います。
		/// </summary>
		/// <param name="roleName">ロール名</param>
		public NCMBRole (string roleName)
		{
			Name = roleName;
		}

		/// <summary>
		/// コンストラクター。<br/>
		/// ACLを指定してロールの作成を行います。
		/// </summary>
		/// <param name="roleName">ロール名</param>
		/// <param name="acl">ACL</param>
		public NCMBRole (string roleName, NCMBACL acl)
		{
			Name = roleName;
			ACL = acl;
		}

		/// <summary>
		/// ロール名の取得、または設定を行います。<br/>
		/// ロール名は保存後、変更は出来ません。
		/// </summary>
		public string Name {
			set {
				this ["roleName"] = value;
			}
			get {
				return (string)this ["roleName"];
			}
		}


		/// <summary>
		/// ロールに所属するユーザのリレーション取得を行います。
		/// </summary>
		/// <returns> リレーション</returns>
		public  NCMBRelation<NCMBUser> Users {
			get {
				return GetRelation<NCMBUser> ("belongUser");
			}
		}

		/// <summary>
		/// ロールに所属するロールのリレーション取得を行います。
		/// </summary>
		/// <returns> リレーション</returns>
		public  NCMBRelation<NCMBRole> Roles {
			get {
				return GetRelation<NCMBRole> ("belongRole");
			}
		}

		/// <summary>
		/// ロール内のオブジェクトで使用出来るクエリを取得します。
		/// </summary>
		/// <returns> クエリ</returns>
		public static NCMBQuery<NCMBRole> GetQuery ()
		{
			return NCMBQuery<NCMBRole>.GetQuery ("role");
		}


		//オーバーライド
		//Thisのsetでの不正キーチェック
		internal override void _onSettingValue (string key, object value)
		{
			base._onSettingValue (key, value);
			if ("roleName".Equals (key)) {
				if (ObjectId != null) {
					//save後のロール名変更不可
					throw new NCMBException (new ArgumentException ("A role's name can only be set before it has been saved."));
				}
				if (!(value is String)) {
					//roleNameは文字列のみ
					throw new NCMBException (new ArgumentException ("A role's name must be a String."));
				}
				if (!NCMBRole.namePattern.IsMatch ((string)value)) {
					//namePatternの禁止文字は使用不可
					throw new NCMBException (new ArgumentException ("A role's name can only contain alphanumeric characters, _, -, and spaces."));
				}
			}

			if ("belongUser".Equals (key)) {
				throw new NCMBException ("belongUser key is already exist. Use this.Users to set it");
			}

			if ("belongRole".Equals (key)) {
				throw new NCMBException ("belongRole key is already exist. Use this.Roles to set it");
			}
		}

		//オーバーライド
		internal override void _beforeSave ()
		{
			if ((ObjectId == null) && (Name == null)) {
				throw new NCMBException (new  ArgumentException ("New roles must specify a name."));
			}
		}

		//オーバーライド
		internal override string _getBaseUrl ()
		{
			return NCMBSettings.DomainURL + "/" + NCMBSettings.APIVersion + "/roles";
		}
	}
}
