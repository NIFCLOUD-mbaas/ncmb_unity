#if UNITY_IOS
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;
using System.Collections;

public class UpdateXcodeProject
{

	[PostProcessBuildAttribute (0)]
	public static void OnPostprocessBuild (BuildTarget buildTarget, string pathToBuiltProject)
	{
		// Stop processing if targe is NOT iOS
		if (buildTarget != BuildTarget.iOS)
			return;
		// Stop processing if NCMB_ENABLE_TWITTER and not have STTwitter
#if NCMB_ENABLE_TWITTER
	if (!Directory.Exists("Assets/Plugins/iOS/STTwitter")) {
		Debug.LogWarning("Can not found STTwitter, please copy STTwitter library to Assets/Plugins/iOS");
		throw new UnityEditor.Build.BuildFailedException("Can not found STTwitter, please copy STTwitter library to Assets/Plugins/iOS");
	}
#endif
		UpdateXcode(pathToBuiltProject);
	}

	static void UpdateXcode(string pathToBuiltProject) 
	{
		var projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);

		if (!File.Exists(projectPath))
		{
			throw new Exception(string.Format("projectPath is null {0}", projectPath));
		}
			
		// Initialize PbxProject
		PBXProject pbxProject = new PBXProject();
		pbxProject.ReadFromFile(projectPath);

		string targetGuid = pbxProject.GetUnityMainTargetGuid();

#if UNITY_2019_3_OR_NEWER
		pbxProject.AddFrameworkToProject(pbxProject.GetUnityFrameworkTargetGuid(), "UserNotifications.framework", false);
		pbxProject.AddFrameworkToProject(pbxProject.GetUnityFrameworkTargetGuid(), "WebKit.framework", false);
		pbxProject.AddFrameworkToProject(pbxProject.GetUnityFrameworkTargetGuid(), "AuthenticationServices.framework", false);
#else
		// Adding required framework
		pbxProject.AddFrameworkToProject(targetGuid, "UserNotifications.framework", false);
		pbxProject.AddFrameworkToProject(targetGuid, "WebKit.framework", false);
		pbxProject.AddFrameworkToProject(targetGuid, "AuthenticationServices.framework", false);
#endif

		// Check for enable NCMB Twitter
#if NCMB_ENABLE_TWITTER
#if UNITY_2019_3_OR_NEWER
			pbxProject.AddFrameworkToProject(pbxProject.GetUnityFrameworkTargetGuid(), "Social.framework", false);
			pbxProject.AddFrameworkToProject(pbxProject.GetUnityFrameworkTargetGuid(), "Accounts.framework", false);
#else
		pbxProject.AddFrameworkToProject(targetGuid, "Social.framework", false);
		pbxProject.AddFrameworkToProject(targetGuid, "Accounts.framework", false);
#endif
#else
		string[] filesToRemove = {
			"Libraries/Plugins/iOS/TwitterAPI.h",
			"Libraries/Plugins/iOS/TwitterAPI.mm"
		};
		foreach (string name in filesToRemove)
		{
			string fileGuid = pbxProject.FindFileGuidByProjectPath(name);
			pbxProject.RemoveFile(fileGuid);
		}
#endif

		// Apply settings
		File.WriteAllText (projectPath, pbxProject.WriteToString());
	}
}
#endif
