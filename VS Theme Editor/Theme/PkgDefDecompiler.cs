using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UtilityLibrary;
using static VS_Theme_Editor.ThemeColorSupport;

namespace VS_Theme_Editor;

internal class PkgDefDecompiler
{
	// ReSharper disable once InconsistentNaming
	private record PkgDefEntry (string SectionHeader, string guid, byte[] DataLine);

	#pragma warning disable IDE0044
	private static Regex sectionHeader = new (@"^\[\$RootKey\$\\Themes\\\{.+?\}\\(?<Category>.+?)\]", RegexOptions.Compiled);
	private static Regex dataLine = new ("^\"Data\"=hex:(?<HexData>.+)$", RegexOptions.Compiled);
	#pragma warning restore IDE0044


	public Dictionary<string, Theme> DecompileMultiTheme(string filePath, 
		ThemeData.ThemeSource themeSource,  bool enableEntries)
	{
		string[] fileLines = readFile(filePath);
		string sourceFileName = Path.GetFileNameWithoutExtension(filePath);

		Dictionary<string, string> themeHeader = getThemeHeader(fileLines);

		List<PkgDefEntry> pkgDefEntries = getPkgDefEntries(fileLines);
		Dictionary<string, List<CategoryData>> categories = new ();

		foreach (PkgDefEntry entry in pkgDefEntries)
		{
			CategoryData categoryData = parseCategoryData(entry, sourceFileName, themeSource, enableEntries);
			
			if (!categories.ContainsKey(categoryData.Guid.ToString()))
				categories[categoryData.Guid.ToString()] = new List<CategoryData>();
			categories[categoryData.Guid.ToString()].Add(categoryData);
		}

		Dictionary<string, Theme> themes = new ();

		foreach ((string guid, List<CategoryData> categoryList) in categories)
		{
			Theme theme = new ("Unnamed");

			foreach (CategoryData categoryData in categoryList)
			{
				theme.Categories.Add(categoryData.Name!, categoryData);
			}

			if (themeHeader.TryGetValue("@", out string? slug))
				theme.Slug = slug;
			if (theme.Slug!.IsVoid())
				theme.Slug = "Undefined";
			if (themeHeader.TryGetValue("Name", out string? name))
				theme.Name = name;
			if (name!.IsVoid())
				theme.Name = $"[ {Path.GetFileNameWithoutExtension(filePath)} ]";
			if (themeHeader.TryGetValue("FallbackId", out string? fallback))
				theme.Fallback = fallback.Trim('{','}');
			// if (themeHeader.TryGetValue("Guid", out string? guid))
			theme.Guid = Guid.Parse(guid);	

			themes[guid] = theme;
		}

		return themes;
	}

	public Theme Decompile(string filePath, 
		ThemeData.ThemeSource themeSource, bool enableEntries)
	{
		string[] fileLines = readFile(filePath);
		string sourceFileName = Path.GetFileNameWithoutExtension(filePath);

		Dictionary<string, string> themeHeader = getThemeHeader(fileLines);

		List<PkgDefEntry> pkgDefEntries = getPkgDefEntries(fileLines);

		List<CategoryData> categories = new ();

		foreach (PkgDefEntry entry in pkgDefEntries)
		{
			CategoryData categoryData = 
				parseCategoryData(entry, sourceFileName, themeSource, enableEntries);
			categories.Add(categoryData);
		}

		Theme theme = new ("Unnamed");

		foreach (CategoryData categoryData in categories)
		{
			theme.Categories.Add(categoryData.Name!, categoryData);
		}

		if (themeHeader.TryGetValue("@", out string? slug))
			theme.Slug = slug;
		if (theme.Slug!.IsVoid())
			theme.Slug = "Undefined";
		if (themeHeader.TryGetValue("Name", out string? name))
			theme.Name = name;
		if (name!.IsVoid())
			theme.Name = $"[ {Path.GetFileNameWithoutExtension(filePath)} ]";
		if (themeHeader.TryGetValue("FallbackId", out string? fallback))
			theme.Fallback = fallback.Trim('{','}');
		if (themeHeader.TryGetValue("Guid", out string? guid))
			theme.Guid = Guid.Parse(guid);	

		return theme;
	}

	private static string[] readFile(string filepath)
	{
		return File.ReadAllLines(filepath);
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

		Utilities.ParseGuidFromHeader(lines[i], out string guid);

		header["Guid"] = guid;

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
		string guid = string.Empty;

		foreach (string line in lines)
		{
			if (sectionHeader.IsMatch(line))
			{
				currentSection = sectionHeader.Match(line).Groups["Category"].Value;

				Utilities.ParseGuidFromHeader(line, out guid);
			}
			else if (dataLine.IsMatch(line))
			{
				string data = dataLine.Match(line).Groups["HexData"].Value;

				// Convert hex string to byte array
				byte[] hexData = data.Split(',')
				.Select(hex => Convert.ToByte(hex.Trim(), 16))
				.ToArray();

				entries.Add(new PkgDefEntry(currentSection, guid, hexData));
			}
		}

		return entries;
	}

	private CategoryData parseCategoryData(PkgDefEntry entry, 
		string sourceFileName, ThemeData.ThemeSource themeSource, bool enableEntries)
	{
		ObservableDictionaryEx<string, ThemeColorEntry> result = new ();

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
		// ReSharper disable once UnusedVariable
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

			string? bg = null;
			uint bgVal = uint.MaxValue;
			int bgIdx = -1;

			if (IsValidColorType(bgType))
			{
				bgVal = reader.ReadUInt32();
				bg = FormatArgb(bgVal);
			}
			else if (IsValidIndexType(bgType))
			{
				bgIdx = reader.ReadInt32();
			}

			byte fgType = reader.ReadByte();

			string? fg = null;
			uint fgVal = uint.MaxValue;
			int fgIdx = -1;

			if (IsValidColorType(fgType))
			{
				fgVal = reader.ReadUInt32();
				fg = FormatArgb(fgVal);
			}
			else if (IsValidIndexType(fgType))
			{
				fgIdx = reader.ReadInt32();
			}

			result.Add(name, new ThemeColorEntry
			{
				Name = name,

				BackgroundType = bgType,
				Background = bg!,
				BackgroundColorValue = bgVal,
				BackgroundIndexValue = bgIdx,
				TcBackground = bgIdx == -1 ?
					new ThemeColor(bgType, bgVal) : new ThemeColor(bgType, bgIdx),

				ForegroundType = fgType,
				Foreground = fg!,
				ForegroundColorValue = fgVal,
				ForegroundIndexValue = fgIdx,
				TcForeground = fgIdx == -1 ?
					new ThemeColor(fgType, fgVal) : new ThemeColor(fgType, fgIdx),

				SourceFileName = sourceFileName,
				ThemeSource = themeSource,
				IsEnabled = enableEntries,


			});
		}

		CategoryData categoryData = new()
		{
			Name = entry.SectionHeader,
			DataLength = dataLength,
			HeaderLength = headerLength,
			CategoryCount = categoryCount,
			Guid = new Guid(entry.guid),
			Entries = result,
			SourceFileName = sourceFileName,
		};

		return categoryData;
	}

	// validate which color types to process
	// types 0, 4, 6, & 7 do not get processed
	// that is, read / write no additional bytes

	// valid color types are CT_RAW & CT_AUTOMATIC only
	// read as a color UInit32
	// static bool isValidColorType(byte colorType)
	// {
	// 	return colorType == 1 || colorType >= 5;
	// 	// return colorType is 1 or 5; 
	// }

	// valid color types are CT_SYSCOLOR,CT_COLORINDEX, CT_VSCOLOR
	// read as an index = Init32
	// static bool isValidIndexType(byte colorType)
	// {
	// 	return colorType == 2 || colorType == 3;
	// 	// return colorType is 2 or 3; 
	// }

	// ReSharper disable once MemberCanBePrivate.Global
	// public static string FormatArgb(uint abgr)
	// {
	// 	string color = $"#{(abgr >> 24) & 0xFF:X2}{abgr & 0xFF:X2}{(abgr >> 8) & 0xFF:X2}{(abgr >> 16) & 0xFF:X2}";
	//
	// 	return color;
	// }

	// public static string FormatArgb(int abgr)
	// {
	// 	string color = $"#{(abgr >> 24) & 0xFF:X2}{abgr & 0xFF:X2}{(abgr >> 8) & 0xFF:X2}{(abgr >> 16) & 0xFF:X2}";
	//
	// 	return color;
	// }


}