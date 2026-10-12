// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             ThemeSupport.cs
// Created:      2026-08-04 (23:08)

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UtilityLibrary;

namespace VS_Theme_Editor;

public class ThemeSupport
{
	public static ThemeSupport Instance { get; private set; }

	static ThemeSupport()
	{
		if (Instance == null)
		{
			Instance = new ThemeSupport();
		}
	}

	private ThemeSupport()
	{ }

	private readonly  VsThemeData Vtd = VsThemeData.Instance;

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


	// vs root theme file

	public void ReadVsRootThemeGuid(string filePath, out string guid)
	{
		guid = readThemeGuid(filePath, "[$RootKey$\\Theme");
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

	// theme file

	public void GetFallbackInfo(string vsId, string? fallback,
		out string overlayName, out ThemeData.ThemeStyle style)
	{
		overlayName = string.Empty;
		style = ThemeData.ThemeStyle.TSTY_UKN;

		if (fallback.IsVoid()) return;


		if (!Vtd.VsThemeIndex[vsId].TryGetValue(new Guid(fallback), out string? value)) return;
		if (value.IsVoid()) return;

		if (!Vtd.VsThemeFileList[vsId].TryGetValue(value, out ThemeFile tf)) return;

		// return FileNameEx, style
		overlayName = tf.FileNameEx;
		style = tf.Style;
	}

	public string GetThemeName(string filePath)
	{
		string vsId;

		if (!VisualStudioData.Instance.GetVsIdForFilePath(filePath, out vsId)) return string.Empty;


		ReadThemeHeader(filePath, out string? themeName, 
			out string? _, out string? _, out string? _);

		if (themeName.IsVoid())
		{

		}

		return themeName ?? string.Empty;
	}

	// theme header

	public void ReadThemeHeader(string filePath, out string? themeName, out string? guid, 
		out string? fallback, out string? slug)
	{
		Dictionary<string, string> themeInfo = readThemeHeader(filePath);

		if (themeInfo ==null || themeInfo.Count < 1)
		{
			themeName = null;
			guid = null;
			fallback = null;
			slug = null;
			return;
		}

		themeInfo.TryGetValue("@", out themeName);
		themeInfo.TryGetValue("Guid", out guid);
		themeInfo.TryGetValue("Name", out slug);
		themeInfo.TryGetValue("FallbackId", out fallback);

		fallback = fallback?.Trim('{','}');
	}
	
	private Dictionary<string, string> readThemeHeader(string filePath)
	{
		return PkgDefDecompiler.GetThemeHeader(filePath);
	}

	/// <summary>
	/// for the selected file, get or create a ThemeFile<br/>
	/// return null if cannot find or create<br/>
	/// return true if an existing themefile was found (VsThemeFile)<br/>
	/// return false if a user theme
	/// </summary>
	public bool? GetThemeFileForSelected(string filePath, bool enableEntries, out ThemeFile tf)
	{
		bool result;

		tf = ThemeFile.Invalid();

		string vsId = VisualStudioData.Instance.VsId;

		if (Vtd.LookupVsThemeFile(vsId, filePath, out tf))
		{
			result = true;
		}
		else
		{
			// if not found, create a new ThemeFile for the user theme
			tf = createThemeFileForUserTheme(vsId, filePath);
			result = false;
		}

		tf.InstallTheme(enableEntries);

		return result;
	}

	public void GetOverlayThemes(ref ThemeFile tf, bool enableEntries)
	{
		// set the overlay starting point
		string? overlay = tf.Overlay;

		Debug.Write($"** get overlay path > {overlay}");

		do
		{
			// follow the overlay path and add all of the theme files
			if (!overlay!.IsVoid() && 
				Vtd.GetOverlayThemeFile(tf.VsId, overlay!, enableEntries, out ThemeFile temp))
			{
				// add the theme file based on the fallback theme value
				tf.OverlayThemes.Add(temp.GetTheme(tf.FallBack)!);
				overlay = temp.Overlay;
			}
			else
			{
				overlay = null;
			}

			Debug.Write($" > {overlay}");

		}
		while (!overlay!.IsVoid());

		Debug.WriteLine(" ");
	}

	private ThemeFile createThemeFileForUserTheme(string vsId, string filePath)
	{
		ThemeFile tf = ThemeFile.Invalid();

		try
		{
			ReadThemeHeader(filePath, out string? themeName, out string? guid, out string? fallback, out string? slug);



			if (fallback.IsVoid())
			{
				fallback = Vtd.GetGuidForThemeLight(vsId);
			}

			GetFallbackInfo(vsId, fallback, out string overlayName, out ThemeData.ThemeStyle style);

			tf = new ThemeFile(filePath)
			{
				VsId = vsId,
				DefinedThemeName = themeName,
				Guid = new Guid(guid),
				FallBack = fallback,
				Description = $"VS Builtin Theme {themeName}",
				Parent = null,
				Overlay = overlayName,
				Source = ThemeData.ThemeSource.TSRC_USER_WITH_FALLBACK,
				Style = style,
				Slug = slug,
			};
		}
		catch (Exception e)
		{
			Debug.WriteLine($"*** FAILED *** | {filePath} | {e}");
			Debug.WriteLine($"*** FAILED *** | {e}\n");
		}


		return tf;
	}


	// theme, etc. objects

	public Theme CloneTheme(Theme themeIn)
	{
		Theme themeOut = new (themeIn.Name!);

		themeOut.Guid = themeIn.Guid;
		themeOut.Fallback = themeIn.Fallback;
		themeOut.Slug = themeIn.Slug;

		themeOut.Categories = new ObservableDictionaryEx<string, CategoryData>(themeIn.Categories);

		themeOut.SetCategoryCount();

		return themeOut;
	}

	public CategoryData CloneCatData(CategoryData catIn)
	{
		CategoryData catOut = new CategoryData();

		catOut.Name = catIn.Name;
		catOut.DataLength = catIn.DataLength;       // value is recalculated on compile
		catOut.HeaderLength = catIn.HeaderLength;		// value replace upon compile
		catOut.CategoryCount = catIn.CategoryCount; // value is NOT recalculated on compile updated above
		catOut.Guid = catIn.Guid;
		catOut.SourceFileName = catIn.SourceFileName;

		catOut.Entries = new ObservableDictionaryEx<string, ThemeColorEntry>(catIn.Entries);

		catOut.SetColorEntryCount();

		return catOut;
	}

	public ThemeColorEntry CloneColorEntry(ThemeColorEntry ceIn)
	{
		return new ()
		{
			Name = ceIn.Name,
			BackgroundType = ceIn.BackgroundType,
			Background = ceIn.Background,
			ForegroundType = ceIn.ForegroundType,
			Foreground = ceIn.Foreground,
			ThemeSource = ceIn.ThemeSource,
			SourceFileName = ceIn.SourceFileName,
			IsEnabled = ceIn.IsEnabled,
		};
	}

	// merge themes

	//             merge id, depth, message
	public List<Tuple<string, int, string>> MergeResults { get; set; }

	private string mergeId;
	private int mergeDepth;

	public bool MergeThemes(Theme theme, List<Theme> themes, out Theme final)
	{
		mergeDepth = 0;
		MergeResults = new ();

		final = CloneTheme(theme);

		// final.SetCategoryCount();

		if (themes.Count < 1 ) return true;

		Debug.WriteLine($"*** counts before merge | cd's {final.OriginalCategoryCount} / ce's {final.OriginalTotalColorEntryCount} / + cd's {final.AddedCategoryCountTotal} / + ce's {final.AddedColorEntryCountTotal}");

		// process each theme in the list and merge into the final theme
		for (int i = 0; i < themes.Count; i++)
		{
			mergeId = $"{(char) ('A' + i)}";

			mergeThemes(final, themes[i]);
		}

		final.UpdateAddedCount();

		Debug.WriteLine($"*** counts before merge | cd's {final.OriginalCategoryCount} / ce's {final.OriginalTotalColorEntryCount} / + cd's {final.AddedCategoryCountTotal} / + ce's {final.AddedColorEntryCountTotal}");

		return true;
	}

	// merge themes.  the first theme is the starting point and the other
	// themes are merged into this theme except that, the first theme takes precident
	// and duplicates entries are ignored
	// three paths here
	// * second theme does not include the category in the first - just add the first to the final
	// * second theme has the same category.  merge the second into the first with the first taking precident
	// * second theme has a category that the first does not, add the second category to the final
	private void mergeThemes(Theme t1st, Theme t2nd)
	{
		CategoryData cd1;

		// Debug.Write($"\n** merging t2nd ( {t2nd.Name} [ {t2nd.Categories.Count} ] ) into t1st ( {t1st.Name} [ {t1st.Categories.Count} ]) ");

		MergeResults.Add(new (mergeId, mergeDepth++, $"merge {t2nd.Name} into {t1st.Name}"));

		foreach ((string cdName2, CategoryData cd2) in t2nd.Categories)
		{
			// set the counts - if cd2 is already in the final theme, this does nothing
			cd2.SetColorEntryCount();
			cd2.ColorEntryAdded = true;

			// Debug.Write($"\n\t*** {cdName2, -34} >>> ");

			// check if the cd already exists in the final theme (t1st)
			if (t1st.Categories.ContainsKey(cdName2))
			{
				// it did exist so merge the cd into the final theme

				// Debug.Write($"*** KEY EXISTS     *** ");
				
				cd1 = t1st.Categories[cdName2];

				// cd1.SetThemeEntryCount();

				string s = $"[[ {cd1.Name} ]]";

				// Debug.Write($" {s,-24} test Ce's");

				foreach ((string ceName2, ThemeColorEntry ce2) in cd2.Entries)
				{
					// Debug.Write($" {ceName2}");

					if (cd1.Entries.TryAdd(ceName2, ce2))
					{
						// Debug.Write($" (add) {ceName2} >");

						MergeResults.Add(new (mergeId, mergeDepth + 2, $"add Ce {ceName2} to Cd [ {cd1.Name} ]"));
					}
				}

				// now replace the category in T1 with the updated category

				cd1.UpdateAddedColorEntryCount();

				t1st.Categories[cdName2] = cd1;

				// Debug.Write(" | ");
			}
			else
			{
				// the cd does not exist.  incorporate the cd into the final theme (t1st)
				// that is, cd2 needs to be added to t1st

				// Debug.Write($"*** KEY NOT EXISTS *** ");
				//
				// Debug.Write($" [[ ** add {cdName2} ]]");
				
				t1st.Categories.Add(cdName2, cd2);

				MergeResults.Add(new (mergeId, mergeDepth +1, $"add Cd {cdName2} | entries added {cd2.Entries.Count}"));
			}


			// Debug.Write(" << << [ cd end ]");
		}

		// Debug.WriteLine(" << << [ theme complete ]");

		mergeDepth--;
	}


	// utilities



}