using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;


// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             VsInfo.cs
// Created:      2026-08-02 (19:08)

namespace VS_Theme_Editor;

[ObservableObject]
public partial class VsInfo
{
	public VsInfo()
	{
		VsId = VisualStudioData.VSID_UKN;
		Version = VisualStudioData.VSID_UKN;
		VersionType = VisualStudioData.VSID_UKN;
		IsInit = false;
	}

	public VsInfo(string vsId, string version, string versionType, string[] versionIds, 
		string presentationName, Brush bdrHighlightBrush, string vsPath, 
		string vsThemeDir, string vsThemeRegFileName, string vsEditorColorFileName,
		string[] userThemeDirs)
	{
		VsId = vsId;
		Version = version;
		VersionType = versionType;
		VersionIds = versionIds;
		PresentationName = presentationName;
		BdrHighlightBrush = bdrHighlightBrush;
		VsPath = vsPath;
		VsThemeDir = vsThemeDir;
		VsThemeRegFileName = vsThemeRegFileName;
		VsEditorColorFileName = vsEditorColorFileName;
		UserThemeDirs = userThemeDirs;
		IsInit = true;
	}


	/// <summary>
	/// the unique identifier for the Visual Studio instance
	/// </summary>
	public string VsId;

	/// <summary>
	/// the version of the Visual Studio instance
	/// </summary>
	public string Version;

	/// <summary>
	/// the type of the Visual Studio instance (e.g., Community, Professional, Enterprise)
	/// </summary>
	public string VersionType;

	/// <summary>
	/// alternate version identifiers for the Visual Studio instance (e.g., 17.0, 17.1, etc.)
	/// </summary>
	public string[] VersionIds;

	/// <summary>
	/// a user-friendly name for the Visual Studio instance (e.g., "Visual Studio 2022 Community")
	/// </summary>
	[ObservableProperty]
	public string presentationName;

	/// <summary>
	/// a brush used for highlighting UI elements related to the Visual Studio instance
	/// </summary>
	[ObservableProperty]
	public Brush bdrHighlightBrush;

	/// <summary>
	/// the path to the Visual Studio installation directory
	/// </summary>
	public string VsPath;

	/// <summary>
	/// the path for Vs' theme information
	/// </summary>
	public string VsThemeDir;

	/// <summary>
	/// the name of the registration file for Vs' builtin theme information
	/// </summary>
	public string VsThemeRegFileName;

	/// <summary>
	/// the name of the registration file for Vs' builtin theme information
	/// </summary>
	public string VsEditorColorFileName;

	/// <summary>
	/// the path(s) to the user-specific theme information directory for the Visual Studio instance
	/// </summary>
	public string[] UserThemeDirs;

	/// <summary>
	/// flag indicating whether the Visual Studio instance information has been initialized
	/// </summary>
	public bool IsInit { get; private set; }
}