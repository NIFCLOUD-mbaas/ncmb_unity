using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NCMB.Internal;
using System.Threading;
using System;

namespace NCMB
{
	/// <summary>
	/// ファイルを操作するクラスです。
	/// </summary>
	[NCMBClassName ("files")]
	public class NCMBFile : NCMBObject
	{
		
		/// <summary>
		/// ファイル名の取得、または設定を行います。
		/// </summary>
		public string FileName {
			get {
				object fileName = null;
				this.estimatedData.TryGetValue ("fileName", out fileName);
				if (fileName == null) {
					return null;
				}
				return (string)this ["fileName"];
			}
			set { this ["fileName"] = value; }
		}


		/// <summary>
		/// ファイルデータの取得、または設定を行います。
		/// </summary>
		public byte[] FileData {
			get {
				object fileData = null;
				this.estimatedData.TryGetValue ("fileData", out fileData);
				if (fileData == null) {
					return null;
				}
				return (byte[])this ["fileData"];
			}
			set { this ["fileData"] = value; }
		}

		/// <summary>
		/// コンストラクター。
		/// </summary>
		public NCMBFile () : this (null)
		{
		}

		/// <summary>
		/// コンストラクター。<br/>
		/// ファイル名を指定してファイルの作成を行います。
		/// </summary>
		public NCMBFile (string fileName) : this (fileName, null)
		{
		}


		/// <summary>
		/// コンストラクター。<br/>
		/// ファイル名、ファイルデータを指定してファイルの作成を行います。
		/// </summary>
		public NCMBFile (string fileName, byte[] fileData) : this (fileName, fileData, null)
		{
		}

		/// <summary>
		/// コンストラクター。<br/>
		/// ファイル名、ファイルデータ、ACLを指定してファイルの作成を行います。
		/// </summary>
		public NCMBFile (string fileName, byte[] fileData, NCMBACL acl) : base ()
		{
			this.FileName = fileName;
			this.FileData = fileData;
			this.ACL = acl;
		}

		/// <summary>
		/// 非同期処理でファイルの保存を行います。<br/>
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="callback">コールバック</param>
		public override void SaveAsync (NCMBCallback callback)
		{
			if ((this.FileName == null)) {
				throw new NCMBException ("fileName must not be null.");
			}
				
			ConnectType type;
			if (this.CreateDate != null) {
				type = ConnectType.PUT;
			} else {
				type = ConnectType.POST;
			}
			IDictionary<string, INCMBFieldOperation> currentOperations = null;
			currentOperations = this.StartSave ();
			string content = _toJSONObjectForSaving (currentOperations);
			NCMBConnection con = new NCMBConnection (_getBaseUrl (), type, content, NCMBUser._getCurrentSessionToken (), this);
			con.Connect (delegate(int statusCode, string responseData, NCMBException error) {
				try {
					NCMBDebug.Log ("【StatusCode】:" + statusCode + Environment.NewLine + "【Error】:" + error + Environment.NewLine + "【ResponseData】:" + responseData);
					if (error != null) {
						// 失敗
						this._handleSaveResult (false, null, currentOperations);
					} else {
						Dictionary<string, object> responseDic = MiniJSON.Json.Deserialize (responseData) as Dictionary<string, object>;
						this._handleSaveResult (true, responseDic, currentOperations);
					}
				} catch (Exception e) {
					error = new NCMBException (e);
				}

				if (callback != null) {
					callback (error);
				}
				return;
			});	
	
		}

		/// <summary>
		/// 非同期処理でファイルの保存を行います。<br/>
		/// 通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		public override void SaveAsync ()
		{	
			this.SaveAsync (null);
		}

		/// <summary>
		/// 非同期処理でファイルのダウンロードを行います。<br/>
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="callback">コールバック</param>
		public void FetchAsync (NCMBGetFileCallback callback)
		{
			// fileName必須
			if ((this.FileName == null)) {
				throw new NCMBException ("fileName must not be null.");
			}
				
			// 通信処理
			NCMBConnection con = new NCMBConnection (_getBaseUrl (), ConnectType.GET, null, NCMBUser._getCurrentSessionToken (), this);
			con.Connect (delegate(int statusCode, byte[] responseData, NCMBException error) {
				NCMBDebug.Log ("【StatusCode】:" + statusCode + Environment.NewLine + "【Error】:" + error + Environment.NewLine + "【ResponseData】:" + responseData);
				this.estimatedData ["fileData"] = responseData;
				if (callback != null) {
					callback (responseData, error);
				}
				return;
			});
		}

		/// <summary>
		/// 非同期処理でファイルのダウンロードを行います。<br/>
		/// 通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		public override void FetchAsync ()
		{
			this.FetchAsync (null);
		}

		/// <summary>
		/// file内のオブジェクトで使用出来るクエリを取得します。
		/// </summary>
		/// <returns> クエリ</returns>
		public static NCMBQuery<NCMBFile> GetQuery ()
		{
			return NCMBQuery<NCMBFile>.GetQuery ("file");
		}

		//通信URLの取得
		internal override string _getBaseUrl ()
		{
			if (this.FileName != null) {
				return NCMBSettings.DomainURL + "/" + NCMBSettings.APIVersion + "/files/" + FileName;
			}
			return NCMBSettings.DomainURL + "/" + NCMBSettings.APIVersion + "/files";
		}

	}
}