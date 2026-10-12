using System.IO;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using UtilityLibrary;
using static VS_Theme_Editor.VisualStudioData;
using static VS_Theme_Editor.ThemeData;
using static VS_Theme_Editor.ThemeData.ThemeStyle;
using static VS_Theme_Editor.ThemeData.ThemeSource;

// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             VsThemeData.cs
// Created:      2026-08-02 (20:08)

namespace VS_Theme_Editor;

public class VsThemeData
{
	public static VsThemeData Instance { get; private set; }

	static VsThemeData()
	{
		if (Instance == null)
		{
			Instance = new VsThemeData();
		}
	}

	private VsThemeData()
	{ }

	public void Init()
	{
		configVsThemeDirs();

		configVsThemeCollections();

		initVsRootThemeData();
		initVsBuiltinThemes();

		showVsThemeIndex();
		showVsRootThemeInfo();
	}

	public const string BUILDIN_THEME_PREFIX = "ThemeName";
	public const string BUILTIN_FILENAME_PREFIX_THEME = "Theme";
	public const string BUILTIN_FILENAME_PREFIX_EDITOR = "Editor";

	public static readonly string BuiltinNameFormatTheme = $"{BUILTIN_FILENAME_PREFIX_THEME}.{{0}}.{THEME_EXTENSION}";
	public static readonly string BuiltinNameFormatEditor = $"{BUILTIN_FILENAME_PREFIX_EDITOR}.{{0}}.{THEME_EXTENSION}";

	public Dictionary<string, Dictionary<Guid, string>> VsThemeIndex;

	public Dictionary<string, Dictionary<string, ThemeFile>> VsThemeFileList;

	private string[] vsIds = { VSID_2022COMM, VSID_2026COMM };

	public Dictionary<string, string> VsThemeDirs;

	public string[] RootNames;

	public void ThemeFileListAdd(string vsId, ThemeFile tf)
	{
		string key = tf.FileNameEx;

		if (VsThemeFileList[vsId].ContainsKey(key)) return;

		VsThemeFileList[vsId].Add(key, tf);
	}

	public void ThemeIndexAdd(string vsId, Guid key, string fileName)
	{
		if (VsThemeIndex[vsId].ContainsKey(key)) return;

		VsThemeIndex[vsId].Add(key, fileName);
	}

	public ThemeFile? GetFallbackThemeFile(string? vsId, string? fallback)
	{
		// use the fallback guid to get the ThemeFile

		if (vsId == null || fallback == null) return null;

		if (VsThemeIndex[vsId].TryGetValue(new Guid(fallback), out string? fileName))
		{
			if (VsThemeFileList[vsId].TryGetValue(fileName, out ThemeFile tf))
			{
				return tf;
			}
		}

		return null;
	}

	public bool GetOverlayThemeFile(string vsId, 
		string overlayFileName, bool enableEntries, out ThemeFile tf)
	{
		LoadThemeFile(vsId, overlayFileName, enableEntries);

		return VsThemeFileList[vsId].TryGetValue(overlayFileName, out tf);
	}

	public void LoadThemeFile(string vsId, string vsThemeFileName, bool enableEntries)
	{
		ThemeFile temp;

		if (VsThemeFileList[vsId].ContainsKey(vsThemeFileName))
		{
			if (VsThemeFileList[vsId][vsThemeFileName].GotThemes) return;

			temp = VsThemeFileList[vsId][vsThemeFileName];
			temp.InstallTheme(enableEntries);
			VsThemeFileList[vsId][vsThemeFileName] = temp;
		}
	}

	/// <summary>
	/// get an <c>existing</c> vs theme file<br/>
	/// return true if found, false if not & ThemeFile.Invalid
	/// </summary>
	public bool LookupVsThemeFile(string vsId, string filePath, out ThemeFile tf)
	{
		tf = ThemeFile.Invalid();

		if (!Utilities.DivideFilePath(filePath, out string? directory, out string? fileName, out string? fileNameEx))
		{
			if (fileNameEx!.IsVoid()) return false;
		}

		if (VsThemeFileList[vsId].TryGetValue(fileNameEx!, out tf))
		{
			// here if found
			if (!tf.FileNameEx.IsVoid() && tf.FilePathDir.Equals(directory) ) return true;
		}

		return  false;
	}

	public string GetGuidForThemeLight(string vsId)
	{
		ThemeFile tf = VsThemeFileList[vsId][RootNames[ThemeLightIdx]];

		return tf.Guid.ToString();
	}

	private void configVsThemeCollections()
	{
		VsThemeIndex = new ();
		VsThemeFileList = new ();

		foreach (string vsId in vsIds)
		{
			VsThemeIndex.Add(vsId, new Dictionary<Guid, string>());
			VsThemeFileList.Add(vsId, new Dictionary<string, ThemeFile>());
		}
	}

	private void configVsThemeDirs()
	{
		VsThemeDirs = new ();

		for (int i = 0; i < vsIds.Length; i++)
		{
			VsThemeDirs.Add(vsIds[i], VisualStudioData.Instance.GetVsThemePath(vsIds[i]));
		}
	}

	private void initVsRootThemeData()
	{
		VsThemeFileConfigure.AssignRootNames();
		VsThemeFileConfigure.ProcessPrimeVsRootThemeFiles();
		VsThemeFileConfigure.ProcessOverlayVsRootThemeFiles();
	}

	private void initVsBuiltinThemes()
	{
		VsBuiltinThemeConfigure.ProcessVsBuiltinThemeInfo(vsIds);
	}

	private void testCloneTheme()
	{

	}

	private void showVsThemeIndex()
	{
		ThemeFile tf;

		Debug.WriteLine("\n *** VsThemeIndex ***");

		foreach ((string vsId, Dictionary<Guid, string> value) in VsThemeIndex)
		{
			Debug.WriteLine($"\n** theme info for {vsId}\n");

			foreach ((Guid guid, string fileName) in value)
			{
				Debug.Write($"**\tentry | {guid} | {fileName}");

				if (VsThemeFileList.ContainsKey(vsId))
				{
					if (VsThemeFileList[vsId].TryGetValue(fileName, out tf))
					{
						Debug.WriteLine($" | Found ThemeFile | {tf.ThemeName}");
					}
					else
					{
						Debug.WriteLine($"\n++> ThemeFile for {fileName} was not found");
					}
				}
				else
				{
					Debug.WriteLine($"\n++> VSID {vsId} was not found");
				}


			}
		}
	}
	
	private void showVsRootThemeInfo()
	{
		bool found;

		Debug.WriteLine("\n *** VsThemeFileList ***");

		foreach (KeyValuePair<string, Dictionary<string, ThemeFile>> kvp in VsThemeFileList)
		{
			Debug.WriteLine($"\n** theme info for {kvp.Key}\n");

			foreach ((string fileName, ThemeFile value) in kvp.Value)
			{
				found = File.Exists(value.FilePath);

				Debug.WriteLine($"**\t exists? {found,-6}| FileName: {fileName, -28}, Style: {value.Style, -14}, Source: {value.Source, -18}, IsMultiTheme: {value.IsMultiTheme, -7}, Parent: {value.Parent ?? "null", -20}, Overlay: {value.Overlay ?? "null", -20} | guid {value.GuidStr, -24} | fallback {value.FallBack}");
			}
		}
	}


	internal static int vsThemeIdx;
	internal static int ThemeLightIdx = vsThemeIdx++;
	internal static int ShellLightIdx = vsThemeIdx++;
	internal static int ThemeDarkIdx = vsThemeIdx++;
	internal static int ShellDarkIdx = vsThemeIdx++;
	internal static int ThemeHcIdx = vsThemeIdx++;
	internal static int ShellHcIdx = vsThemeIdx++;
	internal static int BlueIdx = vsThemeIdx++;
	internal static int BlueEcIdx = vsThemeIdx++;
	internal static int EditorColorsIdx = vsThemeIdx++;
	internal static int EditorLightEcIdx = vsThemeIdx++;
	internal static int EditorDarkEcIdx = vsThemeIdx++;
}