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
		UpdateXcode(pathToBuiltProject);
	}

	static void UpdateXcode(string pathToBuiltProject) 
	{
		var projectPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";

		if (!File.Exists(projectPath))
		{
			throw new Exception(string.Format("projectPath is null {0}", projectPath));
		}
			
		// Initialize PbxProject
		PBXProject pbxProject = new PBXProject();
		pbxProject.ReadFromFile(projectPath);

		string targetGuid = pbxProject.TargetGuidByName("Unity-iPhone");

		// Adding required framework
		pbxProject.AddFrameworkToProject(targetGuid, "UserNotifications.framework", false);

		// Apply settings
		File.WriteAllText (projectPath, pbxProject.WriteToString());
	}
}
#endif


