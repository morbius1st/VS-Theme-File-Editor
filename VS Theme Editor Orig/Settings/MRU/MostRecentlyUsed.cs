using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UtilityLibrary;

namespace VS_Theme_Editor_Orig;

[DataContract(Namespace = "")]
public class MostRecentlyUsed : IEquatable<MostRecentlyUsed>
{
	private const string COMM_EXTN = @"CommonExtensions";
	private const string COMM_PLATFORM = @"Platform";
	private const string EXTN = @"Extensions";

	[DataMember(Order = 2)]
	public string FileName { get; set; }

	[DataMember(Order = 3)]
	public string FolderPath { get; set; }

	[IgnoreDataMember]
	public string ThemeName { get; set; }

	[IgnoreDataMember]
	public string FilePath { get; private set; }

	private MostRecentlyUsed() { }

	/// <summary>
	/// ctor for an MRU entry, either a file path or a folder path<br/>
	/// isFolderPath = false, path is a file path<br/>
	/// isFolderPath = true, path is a simple folder path<br/>
	/// isFolderPath = null, path is a file path but only use the folder path portion<br/>
	/// </summary>
	public MostRecentlyUsed(  string path, bool? isFolderPath)
	{
		parsePaths(path, isFolderPath);

		setFilePath();
	}

	// usage
	// if isfolderpath is false, path is a file path
	// if isfolderpath is true, path is only a folder path
	// if isfolderpath is null, path is a file path but only use the folder path
	private void parsePaths(string path, bool? isFolderPath)
	{
		FileName = string.Empty;

		if (!isFolderPath.HasValue)
		{
			// got null, path is a file path but only use the folder path portion
			FolderPath = getFolderPath( Path.GetDirectoryName(path)!);
		}
		else
		if (isFolderPath == false)
		{
			// got false, a file path
			FileName = Path.GetFileName(path)!;
			FolderPath = Path.GetDirectoryName(path)!;
		}
		else
		{
			// got true, a simple folder path
			FolderPath = path;
		}
	}

	[OnDeserialized]
	internal void OnDeSerialized(StreamingContext ctx)
	{
		setFilePath();
	}

	private void setFilePath()
	{
		FilePath = Path.Combine(FolderPath, FileName);
	}

	public static MostRecentlyUsed MruEmpty()
	{
		return new ();
	}

	private string getFolderPath(string folderPath)
	{
		string result;

		int idx = folderPath.IndexOf(COMM_EXTN, StringComparison.OrdinalIgnoreCase);

		if (idx >= 0)
		{
			result = folderPath.Substring(0, idx + COMM_EXTN.Length + 1 + COMM_PLATFORM.Length);
		}
		else
		{
			idx = folderPath.IndexOf(EXTN, StringComparison.OrdinalIgnoreCase);

			if (idx >= 0)
			{
				result = folderPath.Substring(0, idx + EXTN.Length);
			}
			else
			{
				result = folderPath;
			}
		}

		return result;
	}

	public bool Equals(MostRecentlyUsed? other)
	{
		if (other == null) return false;

		bool test = FilePath.Equals(other!.FilePath, StringComparison.OrdinalIgnoreCase);

		return test;
	}

	public override string ToString()
	{
		return $"FileName >{FileName}< | Folder Path >{FolderPath}< | ThemeName >{ThemeName}<";
	}
}