using System.IO;
using UnityEditor;
using UnityEngine;

namespace NCMB
{
	internal class NCMBMenu
	{
		[MenuItem ("NCMB/DeleteCurrentUserCache")]
		private static void DeleteCurrentUserCache ()
		{
			File.Delete (Application.persistentDataPath + "/" + "currentUser");
			Debug.Log ("CurrentUser cache is deleted");
		}

		[MenuItem ("NCMB/DeleteCurrentUserCache", true)]
		private static bool DeleteCurrentUserCacheValidation ()
		{
			return (File.Exists (Application.persistentDataPath + "/" + "currentUser"));
		}
	}
}