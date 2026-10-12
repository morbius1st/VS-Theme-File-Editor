using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UtilityLibrary;

namespace VS_Theme_Editor_Orig;

internal class VsThemeRegistrationParse
{
	private ThemeProcess _ThemeProcess => ThemeProcess.Instance;

	private DataManager _DataManager => DataManager.Instance;

	public Dictionary<string, ThemeFile> Parse(string vsId)
	{
		string regFilePath = _DataManager.GetRegFilePath(vsId);
		
		Dictionary<string, ThemeFile> themeFiles = new ();

		if (!System.IO.File.Exists(regFilePath))
			return themeFiles;

		string[] lines = System.IO.File.ReadAllLines(regFilePath);

		int i = 0;
		string baseFilePath = Path.GetDirectoryName(regFilePath)!;

		for (; i < lines.Length; i++)
		{
			string line = lines[i].Trim();

			if (line.IsVoid() || 
				!line.StartsWith("[$RootKey$\\Themes", StringComparison.OrdinalIgnoreCase)) continue;

			if (!extractVsBuiltinThemeInfo(lines, i,
					//      themeName, guid, fallback, slug
					out Tuple<string, string, string, string> result)) continue;

			string filePath = getThemeFileNameFromSlug(result.Item4, baseFilePath);

			if (filePath.IsVoid() || !System.IO.File.Exists(filePath)) continue;

			ThemeFile tf = new ThemeFile(filePath)
			{
				DefinedStringName = result.Item1,
				Guid = new Guid(result.Item2),
				FallBack = result.Item3,
				ThemeIndex = 0,
				Description = $"VS Builtin Theme {result.Item1}",
			};

			themeFiles.Add(tf.FileName, tf);

			i += 4;
		}

		return themeFiles;
	}

	private bool extractVsBuiltinThemeInfo(string[] lines, 
		int start, out Tuple<string, string, string, string> result)
	{
		int final = start;
		Dictionary<string, string> themeInfo;
		
		string guid;

		result = new (string.Empty, string.Empty, string.Empty, string.Empty);
		
		if (!_ThemeProcess.ParseGuid(lines[final++].Trim(), out guid)) return false;

		if (!matchLine(lines, final, out themeInfo)) return false;

		themeInfo.TryGetValue("@", out string? themeName);
		themeInfo.TryGetValue("Name", out string? slug);
		themeInfo.TryGetValue("FallbackId", out string? fallback);

		fallback = fallback?.Trim('{','}');

		result = new (themeName!, guid, fallback!, slug!);

		return true;
	}

	private string getThemeFileNameFromSlug(string slug, string rootPath)
	{
		if (!_ThemeProcess.ParseThemeFileLine(slug, out string key, out string value)) return string.Empty;

		if (!key.StartsWith(DataManager.BUILDIN_THEME_PREFIX)) return string.Empty;

		int start = key.IndexOf('_');

		string filename = key[(start + 1)..];

		return Path.Combine(rootPath, string.Format(DataManager.BuiltinThemeNameFormatTheme, filename));
	}

	private bool matchLine(string[] lines, int start, out Dictionary<string, string> themeInfo)
	{
		themeInfo = new ();

		bool result = false;
		string line;

		for (int i = 0; i < 3; i++)
		{
			line = lines[start + i].Trim();

			result = _ThemeProcess.ParseThemeFileLine(line, out string key, out string value);

			if (result) themeInfo.Add(key, value);
		}

		return result;
	}

}
