using System.Runtime.CompilerServices;
using static VS_Theme_Editor_Orig.DataManager;

using UtilityLibrary;

namespace VS_Theme_Editor_Orig;

public class ThemeManager
{
	public static ThemeManager Instance => instance ??= new ThemeManager();

	private ThemeManager()
	{
		_themeProcess = ThemeProcess.Instance;

		ThemeData = new ();
	}

	private static ThemeManager instance;

	private PkgDefDecompiler decompiler;

	internal readonly ThemeData ThemeData;

	private readonly ThemeProcess _themeProcess;

	private DataManager _DataMgr =>  DataManager.Instance;



	public string GetThemeSourceDesc(ThemeSource source)
	{
		return DataManager.ThemeSourceInfo[source].Item1;
	}

	public string GetThemeName(string filePath)
	{
		return _themeProcess.ReadThemeName(filePath);
	}


	/// <summary>
	/// selects the VsRootTheme object which contains
	/// the list of associated VsRootTheme files
	/// </summary>
	public ThemeInfo SelectVsRootThemes(string vsId, Guid fallback)
	{
		return ThemeData.ThemeInfoList[vsId][fallback];
	}

	public Dictionary<string, string> GetThemeHeader(string filePath)
	{
		_themeProcess.ReadThemeHeader(filePath);

		return _themeProcess.ThemeHeader;
	}

}