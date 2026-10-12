using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using UtilityLibrary;

using static VS_Theme_Editor_Orig.DataManager;

namespace VS_Theme_Editor_Orig;

[ObservableObject]
internal partial class ThemeProcess
{
	public static ThemeProcess Instance => themeProcess ??= new ThemeProcess();

	private ThemeProcess() { }

	private static ThemeProcess themeProcess;

	private ThemeManager _ThemeMgr => ThemeManager.Instance;
	private DataManager _DataMgr => DataManager.Instance;

	private PkgDefDecompiler decompiler;

	public Dictionary<string, string> ThemeHeader;

	public Theme? ThemeFinal { get; set; }

	private string fauxGuid = "00000000-0000-0000-0000-00000000{0:D4}";
	public static int FauxGuidIdx = 0;

	public string GetFauxGuidStr() => string.Format(fauxGuid, FauxGuidIdx++);
	
	public Guid GetFauxGuid() => new (string.Format(fauxGuid, FauxGuidIdx++));

	/// <summary>
	/// divides a file path into directory and file name<br/>
	/// return false if dir or filename is empty
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="dir"></param>
	/// <param name="fileName"></param>
	/// <returns></returns>
	public bool SplitFilePath(string filePath, out string dir, out string fileName)
	{
		dir = string.Empty;
		fileName = string.Empty;
		if (filePath.IsVoid()) return false;

		dir = Path.GetDirectoryName(filePath) ?? string.Empty;
		fileName = Path.GetFileName(filePath) ?? string.Empty;

		return !dir!.IsVoid() && !fileName!.IsVoid();
	}

	public ThemeFile ProcessSelectedThemeFile(string filePath)
	{
		ThemeFile themeFile = new (filePath);

		ThemeSource source;
		ThemeStyle style;
		// ThemeType type;

		Tuple < ThemeSource, ThemeStyle > themeInfo = _DataMgr.SelectVsData(filePath);

		source = themeInfo.Item1;
		themeFile.Style = themeInfo.Item2;

		return themeFile;
	}


	public Theme ReadTheme(ThemeFile themeFile)
	{
		return decompiler.Decompile(themeFile.FilePath, ThemeSource.TSRC_VS_THEME, themeFile.Description);
	}

	public bool ReadVsRootThemeGuid(string filePath, out Guid guid)
	{
		guid = new Guid(readThemeGuid(filePath, "[$RootKey$\\Theme"));
		return guid != Guid.Empty;
	}

	public bool ReadUserThemeFallbackGuid(string filePath, out Guid guid)
	{
		guid = new Guid(readThemeGuid(filePath, "\"FallbackId\"="));
		return guid != Guid.Empty;
	}

	private string readThemeGuid(string filePath, string start)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"The theme file '{filePath}' does not exist.");
		}

		string[] fileLines = File.ReadAllLines(filePath);

		string guid = parseThemeGuid(fileLines, start);

		return guid;
	}

	private string parseThemeGuid(string[] lines, string start)
	{
		string line;
		string guid;

		foreach (string l in lines)
		{
			line = l.TrimStart();

			if (line.StartsWith(start, StringComparison.OrdinalIgnoreCase))
			{
				if (ParseGuid(line, out guid)) return guid;
			}
		}

		return Guid.Empty.ToString();
	}

	public bool ParseGuid(string line, out string guid)
	{
		Match match = Regex.Match(line, @"\{([0-9a-fA-F\-]+)\}");
		if (match.Success)
		{
			guid = match.Groups[1].Value;
			return true;
		}

		guid = string.Empty;

		return false;
	}

	public bool ParseThemeFileLine(string line, out string key, out string value)
	{
		bool result = false;

		Match match = Regex.Match(line, @"^""?(?<key>@?[^""]*)""?\s*=\s*""(?<value>.*)""$|^@+(?<key>[^;]+);(?<value>.*)");
		if (match.Success)
		{
			key = match.Groups["key"].Value.Trim('"');
			value = match.Groups["value"].Value;
			result = true;
		}
		else
		{
			key = string.Empty;
			value = string.Empty;
		}

		return result;
	}



	public string ReadThemeName(string filePath)
	{
		ReadThemeHeader(filePath);

		return readThemeName(filePath);

	}

	public void ReadThemeHeader(string filePath)
	{
		ThemeHeader = PkgDefDecompiler.GetThemeHeader(filePath);
	}

	private string readThemeName(string filePath)
	{
		string? name;
		if (!ThemeHeader.TryGetValue("Name", out name))
		{
			name = $"[ {Path.GetFileNameWithoutExtension(filePath)} ]";
		}

		return name;
	}


#region merge themes

	// merge themes
	public bool MergeThemes(List<Theme> themes)
	{
		if (themes.Count < 2) return false;

		for (int i = 1; i < themes.Count; i++)
		{
			mergeThemesA(themes[0], themes[i]);
			mergeThemesB(themes[0], themes[i]);
		}

		return true;
	}

	// merge themes.  the first theme is the starting point and the other
	// themes are merged into this theme except that, the first theme takes precident
	// and duplicates entries are ignored
	// three paths here
	// * second theme does not include the category in the first - just add the first to the final
	// * second theme has the same category.  merge the second into the first with the first taking precident
	// * second theme has a category that the first does not, add the second category to the final
	private void mergeThemesA(Theme themeFirst, Theme themeSecond)
	{
		ThemeFinal = CloneTheme(themeFirst);

		foreach (CategoryData catFirst in themeFirst.Categories)
		{
			// find a matching category in the second theme
			CategoryData? catSecond = findCatData(themeSecond, catFirst.Name)!;

			if (catSecond == null)
			{
				// second does not have the category so nothing to merge, just add
				ThemeFinal.Categories.Add(catFirst);
				continue;
			}

			// has matching category - merge
			CategoryData finalCat = mergeCatData(catFirst, catSecond);

			ThemeFinal.Categories.Add(finalCat);
		}
	}

	private void mergeThemesB(Theme themeFirst, Theme themeSecond)
	{
		foreach (CategoryData catSecond in themeSecond.Categories)
		{
			CategoryData? catFirst = findCatData(themeSecond, catSecond.Name)!;

			if (catFirst == null)
			{
				ThemeFinal.Categories.Add(catFirst);
			}
		}
	}

	private Theme CloneTheme(Theme themeIn)
	{
		Theme themeOut = new ();

		themeOut.Name = themeIn.Name;
		themeOut.Guid = themeIn.Guid;
		themeOut.Fallback = themeIn.Fallback;
		themeOut.Slug = themeIn.Slug;

		themeOut.Categories = new ObservableCollection<CategoryData>();

		return themeOut;
	}

	private CategoryData? findCatData(Theme theme, string name)
	{
		foreach (CategoryData cat in theme.Categories)
		{
			if (cat.Name!.Equals(name) == true) return cat;
		}

		return null;
	}

	private CategoryData mergeCatData(CategoryData catFirst, CategoryData catSecond)
	{
		CategoryData catOut = CloneCatData(catFirst);

		foreach (ThemeColorEntry tceSecond in catSecond.Entries)
		{
			// look for the tceSecond color entry in the first list
			// if found, then skip it, otherwise add it to the output list
			ThemeColorEntry? tceFinal = antiFindColorEntry(catFirst, tceSecond);
			if (tceFinal == null) continue;

			catOut.Entries.Add(tceFinal);
		}

		catOut.EntryCount = catOut.Entries.Count;

		return catOut;
	}

	public CategoryData CloneCatData(CategoryData catIn)
	{
		CategoryData catOut = new CategoryData();

		catOut.Name = catIn.Name;
		catOut.CategoryCount = catIn.CategoryCount; // value is NOT recalculated on compile updated above
		catOut.DataLength = catIn.DataLength;       // value is recalculated on compile
		catOut.EntryCount = catIn.EntryCount;       // value is recalculated on compile
		catOut.Guid = catIn.Guid;
		catOut.HeaderLength = catIn.HeaderLength;		// value replace upon compile

		catOut.Entries = new List<ThemeColorEntry>(catIn.Entries);
		return catOut;
	}

	// search for a color entry in the first category list, if found return null,
	// otherwise return the color entry
	private ThemeColorEntry? antiFindColorEntry(CategoryData catFirst, ThemeColorEntry tce)
	{
		if (catFirst.Entries.Contains(tce)) return null;

		return tce;
	}
	

#endregion
	
}
