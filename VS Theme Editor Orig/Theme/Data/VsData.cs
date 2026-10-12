using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VS_Theme_Editor_Orig;

/// <summary>
/// Data set specific to a version of <b>VS</b>
/// </summary>
public partial class VsData : ObservableObject
{
	public VsData()
	{
		VsId = DataManager.VSID_UKN;
		Version = DataManager.VSID_UKN;
		VersionType = DataManager.VSID_UKN;
	}

	public VsData(string vsId, string version, string versionType, string[] versionIds,
		string presentationName, Brush bdrHighlightBrush, string vsPath, string vsThemeDir, 
		string vsThemeRegFileName, List<ThemeFile>[] vsThemeFileData, 
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
		VsThemeFileData = vsThemeFileData;
		UserThemeDirs = userThemeDirs;
	}

	// used as a common key
	public string VsId;

	public string Version;

	public string VersionType;

	public string[] VersionIds;

	[ObservableProperty]
	public string presentationName;

	[ObservableProperty]
	public Brush bdrHighlightBrush;

	public string VsPath;

	/// <summary>
	/// the path for Vs' theme information
	/// </summary>
	public string VsThemeDir;

	/// <summary>
	/// the name of the registration file for Vs' builtin theme information
	/// </summary>
	public string VsThemeRegFileName;

	// [0,x] = Light Theme, [1, x] = Dark Theme, [2, x] = High Contrast Theme
	public readonly List<ThemeFile>[] VsThemeFileData;

	public string[] UserThemeDirs;
}

/* data to save
* * vs data - version, type, theme dir, theme reg file name,
*		root themes file names, presentation name,
*		border highlight color
* folders / cross references
* program folder, roof theme folder
e.g.
version: 2022
type: Community
version id's: 17, 17.0
presentation name: Visual Studio 2022 Community
highlight brush: #FFFF0000
program dir: C:\Program Files\Microsoft Visual Studio\2022\Community
root theme dir: C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\Microsoft\VisualStudio\Themes
root theme reg file name: Theme.Registration.pkgdef
root theme light file names: "Theme.Light.pkgdef", "Shell.Light.pkgdef"
root theme dark file names: "Theme.Dark.pkgdef", "Shell.Dark.pkgdef"
root theme high contrast file names: "Theme.HighContrast.pkgdef", "Shell.HighContrast.pkgdef"
user theme root dir's (partial): Microsoft Visual Studio\2022, Microsoft\VisualStudio\17.0

*/
