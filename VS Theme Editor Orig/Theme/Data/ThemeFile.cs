using System.IO;
using UtilityLibrary;
using static VS_Theme_Editor_Orig.DataManager;

namespace VS_Theme_Editor_Orig;

public struct ThemeFile
{
	private const string UNDETERMINED = "Undetermined";
	private string filePath;

	public ThemeFile(string filePath)
	{
		FilePath = filePath;
		ThemeIndex = ThemeIndex.RT_UNK;
		Style = ThemeStyle.TSTY_UKN;
		Description = UNDETERMINED;

		Guid = Guid.Empty;
		FallBack = Guid.Empty.ToString();

		Theme = null;
	}

	public ThemeFile(string filePath, ThemeStyle style)
	{
		FilePath = filePath;
		Style = style;
		ThemeIndex = ThemeIndex.RT_UNK;
		Description = UNDETERMINED;

		Guid = Guid.Empty;
		FallBack = Guid.Empty.ToString();

		Theme = null;
	}

	public ThemeFile(string filePath, ThemeIndex index, ThemeStyle style)
	{
		FilePath = filePath.Trim();
		ThemeIndex = index;
		Style = style;
		// Type = ThemeType.TT_UKN;
		Description = $"Theme {StyleDesc}";

		Guid = Guid.Empty;
		FallBack = Guid.Empty.ToString();

		Theme = null;
	}

	public static ThemeFile Invalid()
	{
		return new ();
	}

	public string DefinedStringName
	{
		get => themeName;
		set => themeName = value;
	}

	public string ThemeName => themeName ?? Theme?.Name ?? Path.GetFileNameWithoutExtension(FileName);
	private string themeName = null;

	public string FilePath
	{
		readonly get => filePath;
		private init
		{
			filePath = value;
			parseFileInfo();
		}
	}
	public string FileName { get; private set; }
	public string FileDirPath { get; private set; }

	public string Description { get; set; }

	// intended to be the actual index in the List<VsThemeFile>
	public ThemeIndex ThemeIndex { get; set; }

	public ThemeSource Source { get; set; }
	public string SourceDesc => DataManager.ThemeSourceInfo[Source].Item1;

	public ThemeStyle Style { get; set; }
	public string StyleDesc => DataManager.ThemeStyleInfo[Style].Item1;
	
	public Guid Guid { get; set; }
	public string FallBack { get; set; } 
	public Guid FallBackAsGuid => new (FallBack);

	public Theme? Theme { get; set; }

	/// <summary>
	/// true , good and completed<br/>
	/// false, good but incomplete<br/>
	/// null, created but no good / is invalid
	/// </summary>
	public bool? IsInit => IsInitialized();

	public bool GotGuid => Guid != Guid.Empty;
	public bool GotFallBackGuid => FallBackAsGuid != Guid.Empty;

	private void parseFileInfo()
	{
		// FileInfo will allways have a file name but may have a path
		FileName = Path.GetFileName(FilePath);

		FileDirPath = Path.GetDirectoryName(FilePath) ?? string.Empty;
	}

	public bool? IsInitialized()
	{
		if (FileDirPath.IsVoid()) return null;
		if (ThemeIndex <= ThemeIndex.RT_UNK) return false;
		if (Style <= ThemeStyle.TSTY_UKN) return false;
		if (Description.IsVoid() || Description.Equals(UNDETERMINED)) return false;
		if (!GotGuid) return false;
		if (Theme == null) return false;
		if (GotFallBackGuid)
		{
			return false;
		}

		return null;
	}

}
