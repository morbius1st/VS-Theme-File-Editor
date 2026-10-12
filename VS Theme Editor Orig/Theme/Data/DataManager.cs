using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using UtilityLibrary;
using static VS_Theme_Editor_Orig.DataManager.ThemeSource;

using static VS_Theme_Editor_Orig.DataManager.ThemeStyle;
using static VS_Theme_Editor_Orig.DataManager.ThemeIndex;

namespace VS_Theme_Editor_Orig;

public partial class DataManager : ObservableObject
{
	public static DataManager Instance { get; private set; }

	static DataManager()
	{
		if (Instance == null)
		{
			Instance = new DataManager();
		}
	}

	private DataManager()
	{
		// vsThemeMgr = VsThemeManager.Instance;

		assignVsData(VsDataList[0]);
	}


	private ThemeManager _ThemeMgr => ThemeManager.Instance;

	// Vs data

	[ObservableProperty]
	private VsData vsData;

	[ObservableProperty]
	private string vsDirectory;

	[ObservableProperty]
	private string vsDesc;

	[ObservableProperty]
	private Brush vsDescBdr;

	[ObservableProperty]
	public string vsThemeDir;

	public string VsId => vsData.VsId;

	public static readonly Brush Vs2022CommBdrBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x1E, 0x90, 0xFF));
	public static readonly Brush Vs2026CommBdrBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xFF, 0x00));

	// general data

	public string UsrName => Environment.UserName;

	public const string	VSID_2022COMM = "VS2022Comm";
	public const string	VSID_2026COMM = "VS2026Comm";
	public const string VSID_UKN = "VS UnKnown";

	public const string BUILDIN_THEME_PREFIX = "ThemeName";
	public const string BUILTIN_FILENAME_PREFIX_THEME = "Theme";
	public const string BUILTIN_FILENAME_PREFIX_EDITOR = "Editor";
	public const string THEME_EXTENSION = "pkgdef";


	public enum ThemeIndex
	{
		RT_UNK,
		RT_THEME,	// z
		RT_SHELL	// z + 1
	}

	public const string DM_LIGHT = "Light";
	public const string DM_DARK = "Dark";
	public const string DM_HC = "High Contrast";

	/// <summary>
	/// light, dark, and high contrast
	/// </summary>
	public enum ThemeStyle
	{
		TSTY_UKN,
		TSTY_TBD,
		TSTY_LIGHT,	// y
		TSTY_DARK,	// y + 1
		TSTY_HC,	// y + 2
	}

	public static Dictionary<ThemeStyle, Tuple<string>> ThemeStyleInfo = new ()
	{
		{ TSTY_UKN, new ("Unknown") },
		{ TSTY_LIGHT, new (DM_LIGHT) },
		{ TSTY_DARK, new (DM_DARK) },
		{ TSTY_HC, new (DM_HC) },
	};

	public static string GetThemeStyleDesc(ThemeStyle style) => ThemeStyleInfo[style].Item1;

	public enum ThemeSource
	{
		TSRC_NONE,
		TSRC_UKN,
		TSRC_TBD,
		TSRC_USER_TBD_FALLBACK,
		TSRC_USER_NO_FALLBACK,
		TSRC_USER_WITH_FALLBACK,
		TSRC_VS_BUILTIN,
		TSRC_VS_THEME,	// x
		TSRC_VS_SHELL,
	}

	// for users                              one line  two lines
	public static Dictionary<ThemeSource, Tuple<string, string>> ThemeSourceInfo = new ()
	{
		{ TSRC_NONE                , new ("Undetermined"      , "Undetermined"       ) },
		{ TSRC_UKN                 , new ("Unknown"           , "Unknown"            ) },
		{ TSRC_TBD                 , new ("To be determined"  , "To be\nDetermined"  ) },
		{ TSRC_USER_TBD_FALLBACK   , new ("User Theme"        , "User Theme"         ) },
		{ TSRC_USER_NO_FALLBACK    , new ("User Theme"        , "User Theme"         ) },
		{ TSRC_USER_WITH_FALLBACK  , new ("User Theme"        , "User Theme"         ) },
		{ TSRC_VS_BUILTIN          , new ("Vs Built-In Theme" , "Vs Built-In\nTheme" ) },
		{ TSRC_VS_THEME            , new ("Vs Root Theme"     , "Vs Root\nTheme"     ) },
		{ TSRC_VS_SHELL            , new ("Vs Root Shell"     , "Vs Root\nShell"     ) },
	};

	public static string GetTyemeSourceDesc(ThemeSource source) => ThemeSourceInfo[source].Item1;

	public static readonly string BuiltinThemeNameFormatTheme = $"{BUILTIN_FILENAME_PREFIX_THEME}.{{0}}.{THEME_EXTENSION}";
	public static readonly string BuiltinThemeNameFormatEditor = $"{BUILTIN_FILENAME_PREFIX_EDITOR}.{{0}}.{THEME_EXTENSION}";


	public string GetRegFilePath(string vsId)
	{
		VsData? vsd = GetVsData(vsId);

		string a = vsd.VsThemeDir;
		string b = vsd.VsThemeRegFileName;

		return Path.Combine(a, b);
	}

	/// <summary>
	/// get the <c>theme source</c> and <c>theme type</c> for a file.
	/// <c>filepath</c> must include the path and file name.
	/// </summary>
	public Tuple<ThemeSource, ThemeStyle> SelectVsData(string filePath)
	{
		Tuple<ThemeSource, ThemeStyle> result;

		string fileName = Path.GetFileName(filePath) ?? "";
		string dir = Path.GetDirectoryName(filePath) ?? "";

		// Debug.WriteLine($"\n\n\t** for path | name {fileName} | path {dir}\n");

		VsData = new();

		if (dir.IsVoid()) return new(TSRC_NONE, TSTY_UKN);

		foreach (VsData vsd in VsDataList)
		{
			if (selectVsDataForUser(dir, vsd, out result)) return result!;

			// for a vsd, did not match for a user theme file
			// check for a vs root theme and vs base theme

			if (selectVsDataForVs(fileName, dir, vsd, out result)) return result!;

		}

		// Debug.WriteLine("\t** all failed - unknown path");

		// not found a vs theme
		return new(TSRC_UKN, TSTY_UKN);
	}

	private Tuple<ThemeSource, ThemeStyle> matchToVsRootTheme(VsData vsd, string fileName)
	{
		int k0 = vsd.VsThemeFileData.GetLength(0);

		// array - select theme source
		for (int i = 0; i < k0; i++)
		{
			foreach (ThemeFile vtf in vsd.VsThemeFileData[i])
			{
				if (vtf.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
				{
					return new (GetVsThemeSource(vtf.Style), vtf.Style);
				}
			}
		}

		return new (TSRC_UKN, TSTY_UKN);
	}

	private bool selectVsDataForUser(string dir, VsData vsd ,
		out Tuple<ThemeSource, ThemeStyle> result)
	{
		// compare file path to the user theme dirs. these strings are
		// specific to a Vs version (currently only a single user theme dir)

		for (int i = 0; i < vsd.UserThemeDirs.Length; i++)
		{
			// Debug.WriteLine($"\t\t** GOT user? | {vsd.VsId} | validate {dir} | contains | {vsd.UserThemeDirs[i]}? ");

			if (dir.Contains(vsd.UserThemeDirs[i]))
			{
				// Debug.WriteLine("\t** got it");
				VsData = vsd;
				assignVsData(vsd);
				// found a user theme
				result = new (TSRC_USER_TBD_FALLBACK, TSTY_TBD);
				return true;
			}
			// else
			// {
			// 	Debug.WriteLine("\t** nope");
			// }
		}

		result = new (TSRC_UKN, TSTY_UKN);

		return false;
	}

	private bool selectVsDataForVs(string fileName,string dir, VsData vsd ,
		out Tuple<ThemeSource, ThemeStyle> result)
	{
		// check against the Vs theme folder path

		// Debug.WriteLine($"\t\t** NOT user, try");
		// Debug.WriteLine($"\t\t** {vsd.VsId} | validate {vsd.VsThemeDir} | == | {dir}");

		if (vsd.VsThemeDir.Equals(dir, StringComparison.OrdinalIgnoreCase))
		{
			// got the matching dir - get specific
			result = matchToVsRootTheme(vsd, fileName);

			// Debug.WriteLine($"\t*** got result | {result}");

			if (result.Item1 != TSRC_UKN)
			{
				VsData = vsd;
				assignVsData(vsd);
				return true;
			}

			// file exists is in the vs theme folder so must be a vs builtin theme
			result = new (TSRC_VS_BUILTIN, TSTY_TBD);
			VsData = vsd;
			return true;
		}
		// else
		// {
		// 	Debug.WriteLine("\t** nope");
		// }

		result  = new (TSRC_UKN, TSTY_UKN);

		return false;
	}

	public ThemeSource GetVsThemeSource(ThemeStyle style)
	{
		if (style < TSTY_LIGHT) return TSRC_UKN;

		int src = (int) TSRC_VS_THEME;
		int sty = (int) (style - TSTY_LIGHT);

		int ts = (src + sty);

		if (ts < (int) ThemeSource.TSRC_VS_THEME ||
			ts > (int) ThemeSource.TSRC_VS_SHELL) return TSRC_UKN;

		return (ThemeSource) ts;
	}

	private void assignVsData(VsData vsd)
	{
		VsDirectory = vsd.VsPath;
		VsDesc = vsd.PresentationName;
		VsDescBdr = vsd.BdrHighlightBrush;
		VsThemeDir = vsd.VsThemeDir;
	}

	public void InitVsBuiltinThemes()
	{
		foreach (VsData vsd in VsDataList)
		{
			_ThemeMgr.ThemeData.AddBuiltinThemes(vsd.VsId);
		}

		_ThemeMgr.ThemeData.ShowBuiltinThemeData();
	}

	public void InitVsRootThemes()
	{
		foreach (VsData vsd in VsDataList)
		{
			ThemeProcess.FauxGuidIdx = 0;

			_ThemeMgr.ThemeData.AddThemeData(vsd);
		}

		// vsThemeMgr.VsThemeData.ShowThemeData();
	}

	public VsData? GetVsData(string vsId)
	{
		foreach (VsData vsd in VsDataList)
		{
			if (vsd.VsId.Equals(vsId, StringComparison.OrdinalIgnoreCase))
			{
				return vsd;
			}
		}

		return null;
	}

	public readonly string[] InitUserThemeFolders =
	[
		@"C:\Users\{0}\AppData\Local\Microsoft\VisualStudio",
	];

	// divide this data into a data object here (Vs info) and
	// theme info in VsThemeManager
	public VsData[] VsDataList =>
	[
		// 2022 community
		new VsData(VSID_2022COMM, "2022", "Community", ["17", "17.0"], "Visual Studio 2022 Community", Vs2022CommBdrBrush,
			@"C:\Program Files\Microsoft Visual Studio\2022\Community",
			@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform",
			"Theme.Registration.pkgdef",
			[
				// [ new ("NA.user", TS_USER, "User", "User") ] ,
				// [ new ("NA.void", TS_UNKNOWN, "Not Specified", "Not Set") ] ,
				[ new ("Theme.Light.pkgdef", RT_THEME, TSTY_LIGHT), new ("Shell.Light.pkgdef", RT_SHELL, TSTY_LIGHT)],
				[ new ("Theme.Dark.pkgdef", RT_THEME, TSTY_DARK), new ("Shell.Dark.pkgdef", RT_SHELL, TSTY_DARK)],
				[ new ("Theme.HighContrast.pkgdef", RT_THEME, TSTY_HC), new ("Shell.HighContrast.pkgdef", RT_SHELL, TSTY_HC)],
			],
			[
				// // this id's the root dir
				// @"Microsoft Visual Studio\2022", 
				// this id's the root user theme dir
				@"Microsoft\VisualStudio\17.0"
			]
			),
		// 2026 community
		new VsData(VSID_2026COMM, "2026", "Community", ["18", "18.0"], "Visual Studio 2026 Community", Vs2026CommBdrBrush,
			@"C:\Program Files\Microsoft Visual Studio\18\Community",
			@"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Platform",
			"Themes.Registration.pkgdef",
			[
				// [ new ("NA.user", TS_USER, "User", "User") ] ,
				// [ new ("NA.void", TS_UNKNOWN, "Not Specified", "Not Set") ],
				[ new ("Theme.Light.pkgdef", RT_THEME, TSTY_LIGHT), new ("Shell.Light.pkgdef", RT_SHELL, TSTY_LIGHT) ],
				[ new ("Theme.Dark.pkgdef", RT_THEME, TSTY_DARK), new ("Shell.Dark.pkgdef", RT_SHELL, TSTY_DARK) ],
				[ new ("Theme.HighContrast.pkgdef", RT_THEME, TSTY_HC), new ("Shell.HighContrast.pkgdef", RT_SHELL, TSTY_HC) ],
			],
			[
				// @"Microsoft Visual Studio\18", 
				@"Microsoft\VisualStudio\18.0"
			]
			),
	];


	// simple data

	// sample file paths
	/* user theme	vs is 18 	C:\Users\name\AppData\Local\Microsoft\VisualStudio\18.0_86373a45\Extensions\eksixheh.0al\Colors.pkgdef
	* user theme	vs is 17 	C:\Users\name\AppData\Local\Microsoft\VisualStudio\17.0_cb502d6aExp\Extensions\jeffs\CyberDark\1.0\CyberDark.pkgdef"
	* user theme	vs is 15 	C:\Users\name\AppData\Local\Microsoft\VisualStudio\15.0_0c65df91Exp\Extensions\ahwlcvlt.tul\Colors.pkgdef"
	* vs root theme	vs is 17	C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform\Theme.Light.pkgdef
	* vs root theme	vs is 18	C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Platform\Theme.Light.pkgdef
	* vs builtin theme	vs is 18 	C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Platform\Theme.Bubblegum.pkgdef
	*
	* the exact match is made only when the file path contains the stored value
	* this determinew the theme source and type (basic only)
	* and sets VsData (which contains the vsId
	*
	*/

	public void TestGetInfo1()
	{
		Dictionary<string, string> themeHeader;

		// Debug.WriteLine("\n+++> test get theme info (source and type from file name)\n");

		foreach (string s in VsThemesSamples2)
		{
			if (!Path.Exists(s))
			{
				Debug.Write("\n *** File Does NOT exist | ");
				Debug.WriteLine($" for file {Path.GetFileName(s)} | {s}");
				continue;
			}

			Debug.Write("\n *** File DOES exist | ");
			Debug.WriteLine($" for file {Path.GetFileName(s)} | {s}");


			Tuple < ThemeSource, ThemeStyle> info = SelectVsData(s);
			
			Debug.WriteLine($"\t*** results| vsId is {VsId} | source {info.Item1} | style {info.Item2}");
		}
	}

	private ThemeData vtd = new ThemeData();

	private void testThemeInfo(string vsId)
	{
		ThemeInfo vti = vtd.GetThemeInfo(vsId);

		ThemeIndex idx = RT_THEME;

		Debug.WriteLine($"vs theme info, theme status | {vti.ThemeList[(int) idx]}");

	}

	private void showThemeHeader(Dictionary<string, string> themeHeader)
	{
		Debug.WriteLine($"\t*** ThemeHeader");

		foreach ((string key, string value) in themeHeader)
		{
			Debug.WriteLine($"\t\t** {key} = {value}");
		}
	}

	public readonly string[] VsThemesSamples2 =
	[
		//C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\18.0_86373a45\Extensions\eksixheh.0al
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\18.0_86373a45\Extensions\eksixheh.0al\Colors.pkgdef",
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\18.0_86373a45Jeff\Extensions\CyberLight\CyberLight.pkgdef",
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\17.0_cb502d6aExp\Extensions\jeffs\CyberDark\1.0\CyberDark.pkgdef",
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\15.0_0c65df91Exp\Extensions\ahwlcvlt.tul\Colors.pkgdef",

		@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform\Theme.Light.pkgdef",
		@"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Platform\Theme.Light.pkgdef",
		@"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Platform\Theme.Bubblegum.pkgdef",

		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\18.0_86373a45\Extensions\notexist\Colors.pkgdef",
		@"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Platform\Theme.bogus.pkgdef",
	];

	/*

	// public void AddTestMru()
	// {
	// 	foreach (string filePath in	VsThemesSamples1)
	// 	{
	// 		string path = string.Format(filePath, UsrName);
	// 		string themeName = _ThemeMgr.GetThemeName(path);
	//
	// 		MruMgr.Add(path, themeName);
	// 	}
	// }


	public readonly string[] VsThemesSamples1 =
	[
		@"C:\Users\{0}\AppData\Local\Microsoft\VisualStudio\18.0_86373a45\Extensions\eksixheh.0al\Colors.pkgdef",
		@"C:\Users\{0}\AppData\Local\Microsoft\VisualStudio\17.0_cb502d6aExp\Extensions\jeffs\CyberDark\1.0\CyberDark.pkgdef",
		@"C:\Users\{0}\AppData\Local\Microsoft\VisualStudio\17.0_cb502d6aExp\Extensions\jeffs\VSThemeJeffDark\1.0\CustomTheme.pkgdef",
		@"C:\Users\{0}\AppData\Local\Microsoft\VisualStudio\15.0_0c65df91Exp\Extensions\ahwlcvlt.tul\Colors.pkgdef",
		@"C:\Users\{0}\AppData\Local\Microsoft\VisualStudio\15.0_0c65df91Exp\Extensions\ncciwsdg.rbi\Colors.pkgdef",
		@"C:\Users\{0}\AppData\Local\Microsoft\VisualStudio\14.0\Extensions\gq11jklk.lcv\Colors.pkgdef",
		@"C:\Users\{0}\AppData\Local\Microsoft\VisualStudio\14.0\Extensions\l324avdj.4ok\Colors.pkgdef",
		@"C:\Users\{0}\AppData\Local\Microsoft\VisualStudio\18.0_86373a45\Extensions\eksixheh.0al\Colors.pkgdef",
	];

	*/
}