using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UtilityLibrary;

// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             Utilities.cs
// Created:      2026-08-02 (19:08)

namespace VS_Theme_Editor;

public class Utilities
{
	// return true if the file path have values, else false
	/// <summary>
	/// return true if filename or directory has a values, else false
	/// </summary>
	public static bool DivideFilePath(string filePath, out string? directory, out string? fileName, out string? fileNameEx)
	{
		fileName = Path.GetFileName(filePath);
		fileNameEx = Path.GetFileNameWithoutExtension(fileName);
		directory = Path.GetDirectoryName(filePath);

		if (File.Exists(filePath) || Directory.Exists(filePath))
		{
			// Fetch attributes from the file system
			FileAttributes attr = File.GetAttributes(filePath);

			// Check if the Directory flag is set
			if (attr.HasFlag(FileAttributes.Directory))
			{
				directory = filePath;
				fileName = null;
			}
		}

		return !(fileName == null && directory == null);
	}

	public static bool ParseGuidFromHeader(string input, out Guid result)
	{
		if (!ParseGuidFromHeader(input, out string guid))
		{
			result = Guid.Empty;
			return false;
		}

		result = Guid.Parse(guid);

		return true;
	}

	public static bool ParseGuidFromHeader(string input, out string result)
	{
		result = Guid.Empty.ToString();

		input = input.Trim();

		if (input.IsVoid()) return false;

		Match guidMatch = Regex.Match(input, @"\{([0-9a-fA-F\-]+)\}");
		if (!guidMatch.Success) return false;

		result = guidMatch.Groups[1].Value;
		return true;
	}

}

