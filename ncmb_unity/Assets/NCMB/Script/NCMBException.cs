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
using NCMB.Internal;
using UnityEngine;

namespace NCMB
{
	/// <summary>
	/// エラーを操作するクラスです。
	/// </summary>
	public class NCMBException : Exception
	{
		/// <summary>エラーコード。</summary>
		private string _errorCode;

		/// <summary>エラーメッセージ。</summary>
		private string _errorMessage;

		/// <summary>E400000　不正なリクエストです。</summary>
		public static readonly string BAD_REQUEST = "E400000";

		/// <summary>E400001　JSON形式不正です。</summary>
		public static readonly string INVALID_JSON = "E400001";

		/// <summary>E400002　型が不正です。</summary>
		public static readonly string INVALID_TYPE = "E400002";

		/// <summary>E400003　必須項目で未入力です。</summary>
		public static readonly string REQUIRED = "E400003";

		/// <summary>E400004　フォーマットが不正です。</summary>
		public static readonly string INVALID_FORMAT = "E400004";

		/// <summary>E400005　有効な値ではありません。</summary>
		public static readonly string INVALID_VALUE = "E400005";

		/// <summary>E400006　存在しない値です。</summary>
		public static readonly string NOT_EXIST = "E400006";

		/// <summary>E400008　相関関係でエラーです。</summary>
		public static readonly string RELATION_ERROR = "E400008";

		/// <summary>E400009　指定桁数を超えています。</summary>
		public static readonly string INVALID_SIZE = "E400009";

		/// <summary>E401001　Header不正による認証エラーです。</summary>
		public static readonly string INCORRECT_HEADER = "E401001";

		/// <summary>E401002　ID/Pass認証エラーです。</summary>
		public static readonly string INCORRECT_PASSWORD = "E401002";

		/// <summary>E401003　OAuth認証エラーです。</summary>
		public static readonly string OAUTH_ERROR = "E401003";

		/// <summary>E403001　ＡＣＬによるアクセス権がありません。</summary>
		public static readonly string INVALID_ACL = "E403001";

		/// <summary>E403002　コラボレータ/管理者（サポート）権限がありません。</summary>
		public static readonly string INVALID_OPERATION = "E403002";

		/// <summary>E403003　禁止されているオペレーションです。</summary>
		public static readonly string FORBIDDEN_OPERATION = "E403003";

		/// <summary>E403005　設定不可の項目です。</summary>
		public static readonly string INVALID_SETTING = "E403005";

		/// <summary>E403006　GeoPoint型フィールドに対してGeoPoint型以外のデータ登録/更新を実施（逆も含む）不正なGeoPoint検索を実施エラーです。</summary>
		public static readonly string INVALID_GEOPOINT = "E403006";

		/// <summary>E405001　リクエストURI/メソッドが不許可です。</summary>
		public static readonly string INVALID_METHOD = "E405001";

		/// <summary>E409001　重複エラーです。</summary>
		public static readonly string DUPPLICATION_ERROR = "E409001";

		/// <summary>E413001　ファイルサイズ上限チェック	エラーです。</summary>
		public static readonly string FILE_SIZE_ERROR = "E413001";

		/// <summary>E413002　MongoDBドキュメントのサイズ上限エラーです。</summary>
		public static readonly string DOCUMENT_SIZE_ERROR = "E413002";

		/// <summary>E413003　複数オブジェクト一括操作の上限エラーです。</summary>
		public static readonly string REQUEST_LIMIT_ERROR = "E413003";

		/// <summary>E415001　サポート対象外のContentTypeの指定エラーです。</summary>
		public static readonly string UNSUPPORT_MEDIA = "E415001";

		/// <summary>E429001　使用制限（APIコール数、PUSH通知数、ストレージ容量）超過エラーです</summary>
		public static readonly string REQUEST_OVERLOAD = "E429001";

		/// <summary>E500001　内部エラーです。</summary>
		public static readonly string SYSTEM_ERROR = "E500001";

		/// <summary>E502001　ストレージエラーです。NIFCLOUD ストレージでエラーが発生した場合のエラーです。</summary>
		public static readonly string STORAGE_ERROR = "E502001";

		/// <summary>E502002　メール送信エラーです。</summary>
		public static readonly string MAIL_ERROR = "E502002";

		/// <summary>E502003　DBエラーです。</summary>
		public static readonly string DATABASE_ERROR = "E502003";

		/// <summary>E404001　該当データなし</summary>
		public static readonly string DATA_NOT_FOUND = "E404001";

		/// <summary>コンストラクター。</summary>
		public NCMBException ()
		{
			this._errorCode = "";
			this.ErrorMessage = "";
		}

		/// <summary>
		/// コンストラクター。<br/>
		/// Exceptionを設定します。
		/// </summary>
		/// <param name="error">Exception</param>
		public NCMBException (Exception error)
		{
			this._errorCode = "";
			this.ErrorMessage = error.Message;
			Debug.Log ("Error occurred: " + error.Message + " \n with: " + error.Data + " ; \n " + error.StackTrace);
		}

		/// <summary>
		/// コンストラクター。<br/>
		/// ErrorMessageを設定します。
		/// </summary>
		/// <param name="message">エラーメッセージ</param>
		public NCMBException (string message)
		{
			this._errorCode = "";
			this.ErrorMessage = message;
			Debug.Log ("Error occurred: " + message);
		}

		/// <summary>
		/// エラーコードの取得、または設定を行います。
		/// </summary>
		public string ErrorCode {
			get {
				return _errorCode;
			}
			set {
				_errorCode = value;
			}
		}

		/// <summary>
		/// エラーメッセージの取得、または設定を行います。
		/// </summary>
		public string ErrorMessage {
			get {
				if ((_errorMessage != null) && (_errorMessage != ""))
					return _errorMessage;
				else
					return Message;
			}
			set {
				_errorMessage = value;
			}
		}


		/// <summary>
		/// 現在の例外を説明するメッセージを取得します。
		/// </summary>
		public override string Message {
			get {
				return _errorMessage;
			}
		}

	}
}
