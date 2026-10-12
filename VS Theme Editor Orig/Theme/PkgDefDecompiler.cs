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

internal class PkgDefDecompiler
{
	private static Regex sectionHeader = new (@"^\[\$RootKey\$\\Themes\\\{.+?\}\\(?<Category>.+?)\]", RegexOptions.Compiled);
	private static Regex dataLine = new ("^\"Data\"=hex:(?<HexData>.+)$", RegexOptions.Compiled);

	private DataManager.ThemeSource themeSource;
	private string themeDesc;

	private bool isEnabled;

	public Theme Decompile(string FilePath, DataManager.ThemeSource source, string desc)
	{
		themeSource = source;
		themeDesc = desc;

		string[] fileLines = readFile(FilePath);
		Dictionary<string, string> themeHeader = getThemeHeader(fileLines);

		List<PkgDefEntry> pkgDefEntries = getPkgDefEntries(fileLines);

		List<CategoryData> categories = new ();
		foreach (PkgDefEntry entry in pkgDefEntries)
		{
			CategoryData categoryData = ParseCategoryData(entry);
			categories.Add(categoryData);
		}

		Theme theme = new ();

		foreach (CategoryData categoryData in categories)
		{
			theme.Categories.Add(categoryData);
		}

		if (themeHeader.TryGetValue("@", out string? slug))
			theme.Slug = slug;
		if (theme.Slug!.IsVoid())
			theme.Slug = "Undefined";
		if (themeHeader.TryGetValue("Name", out string? name))
			theme.Name = name;
		if (theme.Name!.IsVoid())
			theme.Name = $"[ {Path.GetFileNameWithoutExtension(FilePath)} ]";
		if (themeHeader.TryGetValue("FallbackId", out string? fallback))
			theme.Fallback = fallback?.Trim('{','}');
		if (fileLines.Length > 0)
		{
			// Extract Guid from the section header line
			string? headerLine = fileLines.FirstOrDefault(l => l.StartsWith("[$RootKey$\\Themes", StringComparison.OrdinalIgnoreCase));
			if (headerLine != null)
			{
				Match guidMatch = Regex.Match(headerLine, @"\{([0-9a-fA-F\-]+)\}");
				if (guidMatch.Success)
					theme.Guid = Guid.Parse(guidMatch.Groups[1].Value);
			}
		}

		return theme;
	}

	private static string[] readFile(string filepath)
	{
		return System.IO.File.ReadAllLines(filepath);
	}

	public static Dictionary<string, string> GetThemeHeader(string filePath)
	{
		string[] fileLines = readFile(filePath);

		return getThemeHeader(fileLines);
	}

	private static Dictionary<string, string> getThemeHeader(string[] lines)
	{
		Dictionary<string, string> header = new (StringComparer.OrdinalIgnoreCase);

		// Find the section header (first line)
		int i = 0;
		while (i < lines.Length && !lines[i].StartsWith("[$RootKey$\\Themes", StringComparison.OrdinalIgnoreCase))
			i++;

		if (i >= lines.Length)
			return header; // No header found

		// Parse lines after the section header until a blank line or next section
		i++;
		for (; i < lines.Length; i++)
		{
			string line = lines[i].Trim();
			if (string.IsNullOrWhiteSpace(line) || line.StartsWith("["))
				break;

			// Match key-value pairs: @="...", "Name"="...", "FallbackId"="..."
			Match match = Regex.Match(line, @"^(?<key>@|""[^""]+"")\s*=\s*""(?<value>.*)""$");
			if (match.Success)
			{
				string key = match.Groups["key"].Value.Trim('"');
				string value = match.Groups["value"].Value;
				header[key] = value;
			}
		}

		return header;
	}

	// read the pkgdef file and return a list of PkgDefEntry objects
	private List<PkgDefEntry> getPkgDefEntries(string[] lines)
	{
		List<PkgDefEntry> entries = new ();
		string currentSection = string.Empty;
		foreach (string line in lines)
		{
			if (sectionHeader.IsMatch(line))
			{
				currentSection = sectionHeader.Match(line).Groups["Category"].Value;
			}
			else if (dataLine.IsMatch(line))
			{
				string data = dataLine.Match(line).Groups["HexData"].Value;

				// Convert hex string to byte array
				byte[] hexData = data.Split(',')
				.Select(hex => Convert.ToByte(hex.Trim(), 16))
				.ToArray();

				entries.Add(new PkgDefEntry(currentSection, hexData));
			}
		}

		return entries;
	}

	private CategoryData ParseCategoryData(PkgDefEntry entry)
	{
		List<ThemeColorEntry> result = new ();

		//start reading bytes
		using MemoryStream ms = new (entry.DataLine);
		using BinaryReader reader = new (ms);

		// Read the first 4 bytes to get the length of the data
		int dataLength = reader.ReadInt32();

		// Next 4 bytes are the header length. Always 11?
		int headerLength = reader.ReadInt32();

		// Next 4 bytes are the number of categories. Always 1?
		int categoryCount = reader.ReadInt32();

		// Next 16 bytes are the Guid
		Guid guid = new (reader.ReadBytes(16));

		// Next 4 bytes are the number of entries
		int entryCount = reader.ReadInt32();


		for (int i = 0; i < entryCount; i++)
		{
			//if (i == 80)
			//{
			//    Debug.WriteLine("Trouble");
			//    // Get the remaining hex data in hex as a string for debugging
			//    var current = ms.Position;
			//    string remainingHex = BitConverter.ToString(reader.ReadBytes((int)(ms.Length - ms.Position))).Replace("-", ", ");
			//    Debug.WriteLine($"Remaining Hex Data: {remainingHex}");

			//    ms.Seek(current, SeekOrigin.Begin);

			//}

			int nameLength = reader.ReadInt32();
			string name = Encoding.UTF8.GetString(reader.ReadBytes(nameLength));

			byte bgType = reader.ReadByte();
			string bg = null;
			if (IsValidColorType(bgType))
			{
				uint argb = reader.ReadUInt32();
				bg = FormatArgb(argb);
			}

			byte fgType = reader.ReadByte();
			string fg = null;
			if (IsValidColorType(fgType))
			{
				uint argb = reader.ReadUInt32();
				fg = FormatArgb(argb);
			}

			result.Add(new ThemeColorEntry
			{
				Name = name,
				BackgroundType = bgType,
				Background = bg,
				ForegroundType = fgType,
				Foreground = fg,
				Source = themeSource,
				SourceDesc = themeDesc
			});
		}

		CategoryData categoryData = new()
		{
			Name = entry.SectionHeader,
			DataLength = dataLength,
			HeaderLength = headerLength,
			CategoryCount = categoryCount,
			Guid = guid,
			EntryCount = entryCount,
			Entries = result
		};

		return categoryData;
	}

	static bool IsValidColorType(byte colorType)
	{
		return colorType is 1 or 2 or 3 or 4 or 5 or 6 or 7; // CT_RAW, CT_SYSCOLOR, CT_AUTOMATIC, CT_COLORINDEX, CT_AUTOMATIC, CT_TRACK_FOREGROUND, CT_TRACK_BACKGROUND
	}

	static string FormatArgb(uint abgr)
	{
		string color = $"#{(abgr >> 24) & 0xFF:X2}{abgr & 0xFF:X2}{(abgr >> 8) & 0xFF:X2}{(abgr >> 16) & 0xFF:X2}";

		return color;
	}

	private record PkgDefEntry (string SectionHeader, byte[] DataLine);
}