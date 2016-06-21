using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NCMB.Internal;
using System.Threading;
using System;

namespace NCMB
{
	[NCMBClassName ("files")]
	public class NCMBFile : NCMBObject
	{
		
		/// <summary>
		/// ファイル名の取得、または設定を行います。
		/// </summary>
		public string FileName {
			get {
				return (string)this ["fileName"];
			}
			set { this ["fileName"] = value; }
		}


		/// <summary>
		/// ファイルデータの取得、または設定を行います。
		/// </summary>
		public byte[] FileData {
			get {
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
		/// オブジェクトIDが登録されていない新規ファイルなら登録を行います。<br/>
		/// オブジェクトIDが登録されている既存ファイルなら更新を行います。<br/>
		///通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="callback">コールバック</param>
		public override void SaveAsync (NCMBCallback callback)
		{
		}

		/// <summary>
		/// 非同期処理でファイルの保存を行います。<br/>
		/// オブジェクトIDが登録されていない新規ファイルなら登録を行います。<br/>
		/// オブジェクトIDが登録されている既存ファイルなら更新を行います。<br/>
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
			if (this.ContainsKey ("fileName")) {
				return CommonConstant.DOMAIN_URL + "/" + CommonConstant.API_VERSION + "/files/" + FileName;
			}
			return CommonConstant.DOMAIN_URL + "/" + CommonConstant.API_VERSION + "/files";
		}

	}
}