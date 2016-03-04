using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildPlayer : MonoBehaviour
{
	[MenuItem("Build/Linux - 32bit")]
	public static void BuildLinux32() { PerformBuild(BuildTarget.StandaloneLinux, "Builds/linux/AssetRender.x86_32", false); }
	[MenuItem("Build/Linux - 64bit")]
	public static void BuildLinux64() { PerformBuild(BuildTarget.StandaloneLinux64, "Builds/linux/AssetRender.x86_64", false); }
	[MenuItem("Build/Osx - 32bit")]
	public static void BuildOsx32() { PerformBuild(BuildTarget.StandaloneOSXIntel, "Builds/osx/AssetRender32.dmg", false); }
	[MenuItem("Build/Osx - 64bit")]
	public static void BuildOsx64() { PerformBuild(BuildTarget.StandaloneOSXIntel64, "Builds/osx/AssetRender.dmg", false); }
	[MenuItem("Build/Windows - 32bit")]
	public static void BuildWindows32() { PerformBuild(BuildTarget.StandaloneWindows, "Builds/windows/AssetRender32.exe", false); }
	[MenuItem("Build/Windows - 64bit")]
	public static void BuildWindows64() { PerformBuild(BuildTarget.StandaloneWindows64, "Builds/windows/AssetRender.exe", false); }
	
	/// <summary>
	/// Build with command line arguments: -buildTarget <aBuildTarget> -locationPathName <aLocationPathName>
	/// </summary>
	public static void CommandLineBuild()
	{
		string[] theArguments = System.Environment.GetCommandLineArgs();
		string theTargetPath = null;
        string thePlatform = null;
		for (int i = 0; i < theArguments.Length; i++) {
			switch (theArguments[i].ToLower()) {
				case "--to":
					theTargetPath = theArguments[++i];
					break;
				case "--platform":
                    thePlatform = theArguments[++i];
					break;
			}
		}

		if (theTargetPath == null)
			throw new Exception("Argument '--to' was not specified.");
		if (thePlatform == null)
			throw new Exception("Argument '--platform' was not specified.");
            
        BuildTarget theBuildTarget = BuildTarget.StandaloneLinux64;
        switch (thePlatform) {
            case "linux": theBuildTarget = BuildTarget.StandaloneLinux64; break;
            case "osx": theBuildTarget = BuildTarget.StandaloneOSXIntel64; break;
            case "windows32": theBuildTarget = BuildTarget.StandaloneWindows; break;
            case "windows64": theBuildTarget = BuildTarget.StandaloneWindows64; break;
        }
		PerformBuild(theBuildTarget, theTargetPath, false);
	}

	//-----------------------------------------------------------------------------------------
	private static void PerformBuild(BuildTarget aBuildTarget, string aTargetPath, bool aIsDebug)
	{
		string[] theScenes = new string[] { "Assets/Scene-1.unity"};
		BuildOptions theBuildOptions = BuildOptions.None;
		if (aIsDebug)
			theBuildOptions = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;
		BuildPipeline.BuildPlayer(theScenes, aTargetPath, aBuildTarget, theBuildOptions);
	}
}