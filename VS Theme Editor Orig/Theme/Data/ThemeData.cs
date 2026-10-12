using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using UtilityLibrary;
using static VS_Theme_Editor_Orig.DataManager.ThemeSource;
using static VS_Theme_Editor_Orig.DataManager;

namespace VS_Theme_Editor_Orig;

[ObservableObject]
internal partial class ThemeData
{
	private readonly ThemeProcess _themeProcess = ThemeProcess.Instance;

	// *** ThemeInfoList ***

	// storage format
	// subject is a list of theme objects categorized by
	// VS version, and theme style (light, dark, HC)
	// dictionary key is VsId (e.g. vs2022comm)
	// value is a dictionary of VsThemeInfo objects keyed by a guid
	// the guid is specific to a theme style
	// the vs theme info object contains a list (dictionary [VsThemeList]) of VsThemeFile objects for the specific theme style
	// the VsThemeFile object contains information about the theme file (path, source, style, description) and a Theme object (if it has been loaded)

	public Dictionary<string, Dictionary<Guid, ThemeInfo>> ThemeInfoList { get; set; } = new ();

	/* functions needed
	* add (provide a VsData object)
	* load (read the theme file and create a Theme object)
	* get A [for a user file with a fallback guid string] return the vsthemeinfo object
	* get B [for a user file with no fallback guid] return a created vsthemeinfo object based on "light"
	* get C [for a user file with a fallback guid object] return the vsthemeinfo object
	* get D (get a guid) (provide vsid & theme style string) (return guid)
	* get E (get a source and description) (provide vsid & file path / name) (return source and description
	* get F [for an A root theme file e.g. Theme.Light.pkgdef] return a created vs theme info object with the B root theme data only (and visa versa)
	* description [for a vs theme] (e.g. "Theme Light")
	* merge (create a combined theme object)
	* show
	*
	* add (provide VsData object) (return void)
	* load (only for a vs root theme)(provide VsId, and fallback guid string) (return void)
	*
	* get A (provide vsid & fallback guid string) (return VsThemeInfo object)
	* get B (provide vsid & empty string) (get guid for "light" [data manager, DM_LIGHT]) (return VsThemeInfo object)
	* get C (provide vsid & guid) (return VsThemeInfo object)
	* get D (provide vsid & theme style string) (return guid)
	* get E (provide vsid & file path / name) (return source and description)
	*
	* description (use get E)
	*
	* merge (provide a list of theme objects) (return a combined theme object) (merge order - from left [0] to right [count - 1] - left supersceds right)
	*
	* show (output the data in a readable format)
	*
	*/

	/// <summary>
	/// add basic vs theme data to the theme info list
	/// </summary>
	public void AddThemeData(VsData vsd)
	{
		Dictionary<Guid, ThemeInfo> vsThemes = new ();

		List<ThemeFile> filePaths;
		ThemeInfo vst;
		Guid tempGuid;

		int k0 = vsd.VsThemeFileData.GetLength(0);

		for (int i = 0; i < k0; i++)
		{
			filePaths = new List<ThemeFile>();

			foreach (ThemeFile vtf in vsd.VsThemeFileData[i])
			{
				filePaths.Add(new ThemeFile($"{vsd.VsThemeDir}\\{vtf.FilePath}", vtf.ThemeIndex, vtf.Style));
			}

			vst = new (filePaths, vsd.VsThemeFileData[i][0].Style);


			// return falue is not needed as these all have a guid
			_themeProcess.ReadVsRootThemeGuid(filePaths[0].FilePath, out tempGuid);

			vst.Guid = tempGuid;

			vsThemes.Add(vst.Guid, vst);
		}

		ThemeInfoList.Add(vsd.VsId, vsThemes);
	}

	/// <summary>
	/// read and load themes into a themeinfo object in the
	/// themeinfo list for a specific vsid and fallback guid string
	/// </summary>
	public ThemeInfo LoadThemes(string vsId, string fallBack)
	{
		Guid fb = new Guid(fallBack);

		ThemeInfo vst = ThemeInfoList[vsId][fb];

		DataManager.ThemeSource src = DataManager.ThemeSource.TSRC_VS_THEME;

		if (vst.IsInit == true) return vst;

		Theme temp;

		PkgDefDecompiler decompiler = new ();

		foreach (ThemeFile vtf in vst.ThemeList)
		{
			temp = decompiler.Decompile(vtf.FilePath, src, vtf.Description);

			vst.ThemeList[(int) src] = new ThemeFile(vtf.FilePath, vtf.ThemeIndex, vtf.Style) { Theme = temp };
		}

		if (ThemeInfoList[vsId][fb].ThemeList[(int) src].Theme == null) Debug.WriteLine("**> FAILED TO LOAD THEME DATA ***");

		return vst;
	}

	// get A
	/// <summary>
	/// get the VsThemeInfo object for a specific vsid and fallback guid string
	/// </summary>
	public ThemeInfo GetThemeInfo(string vsId, string fallBack)
	{
		Guid fb = new Guid(fallBack);

		return ThemeInfoList[vsId][fb];
	}

	// get B
	/// <summary>
	/// get a VsThemeInfo object based on "light" when no fallback guid
	/// </summary>
	public ThemeInfo GetThemeInfo(string vsId)
	{
		Guid fb = GetGuidForVsRootTheme(vsId, DataManager.ThemeStyle.TSTY_LIGHT);

		return ThemeInfoList[vsId][fb];
	}

	// get C
	/// <summary>
	/// get a VsThemeInfo object based for a specific vsid and fallback guid object
	/// </summary>
	public ThemeInfo GetThemeInfo(string vsId, Guid guid)
	{
		return ThemeInfoList[vsId][guid];
	}

	// get D
	/// <summary>
	/// get the guid object for a specific vsid and theme style<br/>
	/// return Guid.Empty if not found
	/// </summary>
	public Guid GetGuidForVsRootTheme(string vsId, ThemeStyle themeStyle)
	{
		Guid guid = Guid.Empty;

		foreach ((Guid key, ThemeInfo vti) in ThemeInfoList[vsId])
		{
			if (vti.Style == themeStyle)
			{
				guid = key;
				break;
			}
		}

		return guid;
	}

	// get E
	/// <summary>
	/// get the theme source and description for a specific Vs theme file based on the vsid and file name
	/// </summary>
	public ThemeInfo GetThemeInfoWithFileName(string vsId, string fileName)
	{
		ThemeInfo backup = new ();

		foreach ((Guid key, ThemeInfo vti) in ThemeInfoList[vsId])
		{
			foreach (ThemeFile vtf in vti.ThemeList)
			{
				if (vtf.FilePath.Contains(DM_LIGHT)) backup = vti;

				if (vtf.FilePath.Contains(fileName, StringComparison.OrdinalIgnoreCase))
				{
					return vti;
				}
			}
		}

		return backup;
	}

	// get F
	/// <summary>
	/// create a VsThemeInfo object specifically for a vs base or shell theme file.
	/// this includes the VsThemeFile object for all items in the list except for
	/// its own
	/// </summary>
	public ThemeInfo CreateThemeInfo(string vsId, string fileName)
	{
		ThemeInfo start = GetThemeInfoWithFileName(vsId, fileName);
		ThemeInfo final = start.Clone();

		if (!final.IsInit.HasValue) return final;

		foreach (ThemeFile vtf in start.ThemeList)
		{
			if (!vtf.FilePath.Contains(fileName, StringComparison.OrdinalIgnoreCase))
			{
				final.ThemeList.Add(vtf);
			}
		}

		return final;
	}

	// get G
	/// <summary>
	/// get the source, stype, and description for a specific vsId and file path
	/// </summary>
	public Tuple<ThemeSource, ThemeStyle, string> GetThemeSettings(string vsId, string filePath)
	{
		ThemeSource source = TSRC_UKN;
		ThemeStyle style = ThemeStyle.TSTY_UKN;
		string description = string.Empty;

		ThemeInfo vti = GetThemeInfoWithFileName(vsId, Path.GetFileName(filePath));

		foreach (ThemeFile vtf in vti.ThemeList)
		{
			if (vtf.FilePath.Contains(Path.GetFileName(filePath), StringComparison.OrdinalIgnoreCase))
			{
				style = vtf.Style;
				description = vtf.Description;
				return new Tuple<ThemeSource, ThemeStyle, string>(source, style, description);
			}
		}

		return new Tuple<ThemeSource, ThemeStyle, string>(source, style, description);
	}

	// get H
	// public string GetThemeSourceDesc


	// need to get the theme data based on
	// source == TSRC_UKN => exit / abort
	// source == TSRC_USER & style = TSTY_TBD => A  (user file, need fallback)
	// source == TSRC_VS_* & style = TSTY_* => C (vs root theme file)
	// source == TSRC_BUILTIN & style = TSTY_TBD => D (vs built in theme file)


	public ThemeInfo GetThemeInfo(string filePath, string vsId, ThemeSource source, ThemeStyle style)
	{
		if (source == TSRC_UKN) return new ();

		ThemeInfo result = new ();

		Guid guid;

		switch (source)
		{
		case TSRC_USER_TBD_FALLBACK:
			{
				if (_themeProcess.ReadUserThemeFallbackGuid(filePath, out guid))
				{
					result = GetThemeInfo(vsId, guid);
				}
				else
				{
					result = GetThemeInfo(vsId);
				}

				break;
			}
		case TSRC_VS_BUILTIN:
			{
				result = getThemeInfoForBuiltInTheme(vsId, source, style);
				break;
			}
		default:
			{
				result = getThemeInfoForVsRootTheme(vsId, source, style);

				break;
			}
		}

		return result;
	}

	private ThemeInfo getThemeInfoForVsRootTheme(string vsId, ThemeSource source, ThemeStyle style)
	{
		ThemeInfo result = new ();


		return result;
	}

	private ThemeInfo getThemeInfoForBuiltInTheme(string vsId, ThemeSource source, ThemeStyle style)
	{
		ThemeInfo result = new ();


		return result;
	}


	public void ShowThemeData()
	{
		if (ThemeInfoList.Count == 0)
		{
			Console.WriteLine("No VS Root Themes found.");
			return;
		}

		foreach (KeyValuePair<string, Dictionary<Guid, ThemeInfo>> kvp in ThemeInfoList)
		{
			Debug.WriteLine($"\n*+++> for key (vsid) = {kvp.Key}");

			foreach (KeyValuePair<Guid, ThemeInfo> themeKvp in kvp.Value)
			{
				Debug.WriteLine($"\n\t** for key (guid) = {themeKvp.Key}");

				Debug.WriteLine($"\t\t** Name {themeKvp.Value.Name} | count {themeKvp.Value.ThemeList.Count} | Style {themeKvp.Value.Style} | is init {themeKvp.Value.IsInit}");


				foreach (ThemeFile value in themeKvp.Value.ThemeList)
				{
					Debug.WriteLine($"\n\t\t** VsThemeFile | key (source)  File Path {value.FilePath}");
					Debug.WriteLine($"\t\t\t** index {value.ThemeIndex} [ {value.Description} ] | style {value.Style} [ {value.StyleDesc} ]");
					Debug.WriteLine($"\t\t\t** guid {value.Guid} | fallback {value.FallBack}");
					Debug.WriteLine($"\t\t\t** got theme? {value.Theme != null}");

					if (value.Theme != null)
					{
						Debug.WriteLine($"\t\t\t\t** Theme Name {value.Theme.Name} | categories {value.Theme.Categories.Count}");
					}
				}
			}
		}
	}


	// *** BuiltinThemeList ***

	//                vsid            filename  theme data
	public Dictionary<string, Dictionary<string, ThemeFile>> BuiltinThemeList { get; set; } = new (StringComparer.OrdinalIgnoreCase);

	public void AddBuiltinThemes(string vsId)
	{
		VsThemeRegistrationParse parser = new ();

		Dictionary<string, ThemeFile> themeList = parser.Parse(vsId);

		if (themeList == null) return;

		if (BuiltinThemeList.ContainsKey(vsId))
		{
			BuiltinThemeList[vsId] = themeList;
		}
		else
		{
			BuiltinThemeList.Add(vsId, themeList);
		}
	}

	public bool GotBuiltinTheme(string vsId, string filePath)
	{
		string fileName;

		if (!_themeProcess.SplitFilePath(filePath, out string _, out fileName)) return false;

		if (!BuiltinThemeList.ContainsKey(vsId)) return false;

		if (!BuiltinThemeList[vsId].ContainsKey(fileName)) return false;

		return true;
	}

	public void LoadTheme(string vsId, string filePath)
	{
		string fileName;

		if (!_themeProcess.SplitFilePath(filePath, out string _, out fileName)) return;

		if (!GetBuiltinThemeFile(vsId, fileName, out ThemeFile tf)) return;

		PkgDefDecompiler decomp = new PkgDefDecompiler();

		// Theme? theme = 

	}

	public bool GetBuiltinThemeFile(string vsId, string fileName, out ThemeFile themeFile)
	{
		themeFile = ThemeFile.Invalid();

		if (!BuiltinThemeList.ContainsKey(vsId)) return false;

		if (!BuiltinThemeList[vsId].ContainsKey(fileName)) return false;

		themeFile = BuiltinThemeList[vsId][fileName];

		return true;
	}

	public void ShowBuiltinThemeData()
	{
		if (BuiltinThemeList.Count == 0)
		{
			Console.WriteLine("No VS Builtin Themes found.");
			return;
		}

		foreach ((string vsId, Dictionary<string, ThemeFile> kvp) in BuiltinThemeList)
		{
			Debug.WriteLine($"\n*+++> for key (vsid) = {vsId}");

			foreach ((string key, ThemeFile value) in kvp)
			{
				Debug.WriteLine($"\n\t** ThemeFile | file name (key) = {key}");

				bool exists = Path.Exists(value.FilePath);

				Debug.WriteLine($"\t\t\t** name {value.ThemeName} [ {value.Description} | index {value.ThemeIndex} ] | style {value.Style} [ {value.StyleDesc} ]");
				Debug.WriteLine($"\t\t\t** guid {value.Guid} | fallback {value.FallBack}");
				Debug.WriteLine($"\n\t\t** exists? {exists} | file name {value.FileName} | File Path {value.FilePath}");
				Debug.WriteLine($"\t\t\t** got theme? {value.Theme != null}");

				if (value.Theme != null)
				{
					Debug.WriteLine($"\t\t\t\t** Theme Name {value.Theme.Name} | categories {value.Theme.Categories.Count}");
				}
			}
		}
	}
}