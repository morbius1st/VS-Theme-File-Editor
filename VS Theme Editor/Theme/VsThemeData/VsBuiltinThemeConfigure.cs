using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

using UtilityLibrary;

// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             VsThemeRegFileParse.cs
// Created:      2026-08-04 (23:08)

namespace VS_Theme_Editor;

internal class VsBuiltinThemeConfigure
{
	private static readonly ThemeSupport Ts = ThemeSupport.Instance;

	private static readonly  VsThemeData Vtd = VsThemeData.Instance;

	private static readonly VisualStudioData vsd = VisualStudioData.Instance;


	public static void ProcessVsBuiltinThemeInfo(string[] vsIds)
	{
		string vsRgeFilePath;

		VsBuiltinThemeConfigure vsBuiltinConfigure = new ();

		foreach (string vsId in vsIds)
		{
			vsRgeFilePath = VisualStudioData.Instance.GetVsThemeRegFilePath(vsId);

			if (!File.Exists(vsRgeFilePath)) continue;

			vsBuiltinConfigure.Parse(vsId, vsRgeFilePath);
		}
	}

	private void Parse(string vsId, string regFilePath)
	{
		if (!System.IO.File.Exists(regFilePath)) return;

		string[] lines = System.IO.File.ReadAllLines(regFilePath);

		int i = 0;
		string baseFilePath = Path.GetDirectoryName(regFilePath)!;

		for (; i < lines.Length; i++)
		{
			string line = lines[i].Trim();

			if (line.IsVoid() || 
				!line.StartsWith("[$RootKey$\\Themes", StringComparison.OrdinalIgnoreCase)) continue;

			if (!extractVsBuiltinThemeInfo(vsId, lines, i, baseFilePath, out ThemeFile tf)) continue;

			Vtd.ThemeFileListAdd(vsId, tf);

			Vtd.ThemeIndexAdd(vsId, tf.Guid, tf.FileNameEx);

			i += 4;
		}
	}

	private bool extractVsBuiltinThemeInfo(string vsId, string[] lines, int start, string baseFilePath, out ThemeFile tf)
	{
		int final = start;
		Dictionary<string, string> themeInfo;
		
		string guid;

		tf = ThemeFile.Invalid();
		
		if (!Ts.ParseGuid(lines[final++].Trim(), out guid)) return false;

		if (!extractThemeInfo(vsId, lines, final, baseFilePath, out string? filePath, out string? themeName, 
				out string? fallback, out string? slug)) return false;

		Ts.GetFallbackInfo(vsId, fallback, out string overlayName, out ThemeData.ThemeStyle style);

		tf = new ThemeFile(filePath)
		{
			VsId = vsId,
			DefinedThemeName = themeName,
			Guid = new Guid(guid),
			FallBack = fallback,
			Description = $"VS Builtin Theme {themeName}",
			Parent = null,
			Overlay = overlayName,
			Source = ThemeData.ThemeSource.TSRC_VS_BUILTIN,
			Style = style,
			Slug = slug,
		};

		return true;
	}

	private string? getThemeFileNameFromSlug(string vsId, string slug, string basePath, out string baseFileName)
	{
		baseFileName = string.Empty;

		if (!Ts.ParseThemeFileLine(slug, out string key, out string _)) return string.Empty;

		if (!key.StartsWith(VsThemeData.BUILDIN_THEME_PREFIX)) return string.Empty;

		int start = key.IndexOf('_');

		baseFileName = key[(start + 1)..];

		string? filePath = validateFile(vsId, basePath, baseFileName);

		return filePath;
	}

	private string? validateFile(string vsId, string basePath, string baseFileName)
	{
		string baseName;

		string fileName = string.Format(VsThemeData.BuiltinNameFormatTheme, baseFileName);
		string filePath = Path.Combine(basePath, fileName);

		if (!Path.Exists(filePath))
		{
			fileName = vsd.XlateVsThemeFileName(vsId, fileName);
			filePath = Path.Combine(basePath, fileName);
		}

		return filePath;
	}

	private bool extractThemeInfo(string vsId, string[] lines, int start, string baseFilePath, 
		out string? filePath, out string? themeName, out string? fallback, out string? slug)
	{
		filePath = null;
		themeName = null;
		fallback = null;
		slug = null;

		if (!matchLines(lines, start, out Dictionary<string, string> themeInfo)) return false;

		themeInfo.TryGetValue("@", out themeName);
		themeInfo.TryGetValue("Name", out slug);
		themeInfo.TryGetValue("FallbackId", out fallback);

		fallback = fallback?.Trim('{', '}');

		filePath = getThemeFileNameFromSlug(vsId, slug, baseFilePath, out string _);

		return true;
	}

	private bool matchLines(string[] lines, int start, out Dictionary<string, string> themeInfo)
	{
		themeInfo = new ();

		bool result = false;
		string line;

		for (int i = 0; i < 3; i++)
		{
			line = lines[start + i].Trim();

			result = Ts.ParseThemeFileLine(line, out string key, out string value);

			if (result) themeInfo.Add(key, value);
		}

		return result;
	}

}