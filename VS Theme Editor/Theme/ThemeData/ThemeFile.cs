using System.Diagnostics;
using System.IO;
using UtilityLibrary;
using static VS_Theme_Editor.ThemeData;
using static VS_Theme_Editor.VisualStudioData;


// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             ThemeFile.cs
// Created:      2026-08-02 (21:08)

namespace VS_Theme_Editor;

// ReSharper disable once DefaultStructEqualityIsUsed.Global
public struct ThemeFile
{

	public ThemeFile(string filePath)
	{
		FileName = string.Empty;
		FilePathDir = string.Empty;
		Slug = string.Empty;
		this.filePath = string.Empty;

		VsId = string.Empty;
		FilePath = filePath;
		Style = ThemeStyle.TSTY_UKN;
		Description = VSD_UDF;
		Guid = Guid.Empty;
		FallBack = Guid.Empty.ToString();
		Source = ThemeSource.TSRC_TBD;
		OverlayThemes = new ();
		Themes = new ();
	}

	public ThemeFile(string filePath, ThemeStyle style)
	{
		FileName = string.Empty;
		FilePathDir = string.Empty;
		Slug = string.Empty;
		this.filePath = string.Empty;

		VsId = string.Empty;
		FilePath = filePath;
		Style = style;
		Description = VSD_UDF;
		Guid = Guid.Empty;
		FallBack = Guid.Empty.ToString();
		Source = ThemeSource.TSRC_TBD;
		OverlayThemes = new ();
		Themes = new ();
	}



	/// <summary>
	/// an invalid theme file / object
	/// </summary>
	public static ThemeFile Invalid()
	{
		return new ();
	}

	/// <summary>
	/// flag that this ThemeFile is not valid
	/// </summary>
	public bool IsInvalid => filePath.IsVoid();

	/// <summary>
	/// the theme name forecd into this theme file / object
	/// </summary>
	public string DefinedThemeName
	{
		get => themeName;
		set => themeName = value;
	}

	private string themeName = string.Empty;

	/// <summary>
	/// the name of the theme or a substitute name
	/// </summary>
	public string ThemeName => themeName ?? Path.GetFileNameWithoutExtension(FileName);

	private string filePath;

	/// <summary>
	/// the Visual Studio version id for this theme file / object
	/// </summary>
	public string VsId { get; set; }

	/// <summary>
	/// the full file path of the theme file
	/// </summary>
	public string FilePath
	{
		get => filePath;
		private init
		{
			filePath = value;

			Utilities.DivideFilePath(filePath, 
				out string? fileDirPath, out string? fileName, out string? _);

			FilePathDir = fileDirPath ?? string.Empty;
			FileName = fileName ?? string.Empty;


		}
	}

	/// <summary>
	/// the filename component of the file path (with extension)
	/// </summary>
	public string FileName { get; private set; }
	
	/// <summary>
	/// the filename component of the file path (without extension)
	/// </summary>
	public string FileNameEx => Path.GetFileNameWithoutExtension(FileName);

	/// <summary>
	/// the directory component of the file path
	/// </summary>
	public string FilePathDir { get; private set; }

	/// <summary>
	/// the theme description
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// the theme source for this theme file / object
	/// </summary>
	public ThemeSource Source { get; set; }
	public string SourceDesc => ThemeSourceInfo[Source].Item1;

	/// <summary>
	/// the theme style for this theme file / object
	/// </summary>
	public ThemeStyle Style { get; set; }
	public string StyleDesc => ThemeStyleInfo[Style].Item1;

	public string GuidStr
	{
		get => Guid.ToString();
		set => Guid = new Guid(value);
	}

	/// <summary>
	/// the guid of this theme file / object
	/// </summary>
	public Guid Guid { get; set; }

	/// <summary>
	/// the guid (as string) of the fallback theme
	/// </summary>
	public string FallBack { get; set; }

	/// <summary>
	/// the guid of the fallback theme
	/// </summary>
	public Guid FallBackAsGuid => new (FallBack);

	/// <summary>
	/// the slug of the theme
	/// </summary>
	public string Slug { get; set; }

	/// <summary>
	/// the list of themes which allows this object to store
	/// one or more themes (i.e. when a multi-theme object)
	/// where each theme is stored in the dictionary with a guid key
	/// </summary>
	public Dictionary<string, Theme>? Themes { get; set; }

	// /// <summary>
	// /// the actual theme object (unmodified) but stored in the
	// /// list of themes as 
	// /// </summary>
	// private Theme Theme
	// {
	// 	get => Themes[Guid.Empty.ToString()];
	// 	set
	// 	{
	// 		// if (IsMultiTheme) throw new InvalidOperationException("Cannot Assign Theme When Multi-Theme");
	// 		Themes[Guid.Empty.ToString()] = value;
	// 	}
	// }

	/// <summary>
	/// the actual theme object (unmodified)
	/// </summary>
	public Theme? ThemeFinal { get; set; }

	/// <summary>
	/// true , good and completed<br/>
	/// false, good but incomplete<br/>
	/// null, created but no good / is invalid
	/// </summary>
	public bool? IsInit => IsInitialized();

	/// <summary>
	/// the filename of the parent theme (if any)
	/// </summary>
	public string? Parent { get; set; }

	/// <summary>
	/// the filename of the overlay theme (if any)
	/// </summary>
	public string? Overlay { get; set; }

	/// <summary>
	/// flag that tis theme file data contains multiple theme information
	/// </summary>
	public bool IsMultiTheme { get; set; }

	/// <summary>
	/// the list of themes to overlay onto this theme
	/// </summary>
	public List<Theme> OverlayThemes { get; set; }

	/// <summary>
	/// flag indicating whether the theme file is valid theme object
	/// </summary>
	public bool? IsInitialized()
	{
		if (FilePathDir.IsVoid()) return null;
		if (Style <= ThemeStyle.TSTY_UKN) return false;
		if (Description.IsVoid() || Description.Equals(VSD_UDF)) return false;
		if (!GotGuid) return false;
		if (GotFallBackGuid)
		{
			return false;
		}

		return null;
	}

	/// <summary>
	/// flag indicating whether the theme file has a valid guid
	/// </summary>
	public bool GotGuid => Guid != Guid.Empty;

	/// <summary>
	/// flag indicating whether the theme file has a valid fallback guid
	/// </summary>
	public bool GotFallBackGuid => FallBackAsGuid != Guid.Empty;

	/// <summary>
	/// flag to indicate that themes have been installed
	/// </summary>
	public bool GotThemes => Themes != null && Themes.Count > 0;

	/// <summary>
	/// flag that the this is valid
	/// </summary>
	public readonly bool IsValid() => !filePath.IsVoid();

	private static PkgDefDecompiler? decomp  ;

	public void SetTheme(Theme theme, string? guid = null)
	{
		if (!IsMultiTheme)
		{
			Themes?.TryAdd(Guid.ToString(), theme);
		}
		else if (guid != null)
		{
			Themes?.TryAdd(guid, theme);
		}
	}

	public Theme? GetTheme(string? guid = null)
	{
		Theme? tf = null;

		if (!IsMultiTheme)
		{
			Themes?.TryGetValue(Guid.ToString(), out tf);
		}
		else if (guid != null)
		{
			// is multi-theme
			Themes?.TryGetValue(guid, out tf);
		}
		else
		{
			Debug.Write("*");
		}

		return tf;
	}

	/// <summary>
	/// sets the Theme property based on the file path and fileName
	/// </summary>
	public void InstallTheme(bool enableEntries)
	{
		if (Themes?.Count != 0) return;

		if (decomp == null) decomp = new ();

		if (!IsMultiTheme)
		{
			Themes[GuidStr] = decomp.Decompile(filePath, Source, enableEntries);
		}
		else
		{
			installMultiTheme(enableEntries);
		}
	}

	private void installMultiTheme(bool enableEntries)
	{
		if (!IsMultiTheme) return;

		Themes = decomp?.DecompileMultiTheme(filePath, Source, enableEntries);

		if (Themes?.Values != null)
			foreach (Theme theme in	Themes?.Values!)
			{
				theme.SetCategoryCount();
			}
	}


	// public void LoadTheme()
	// {
	// 	if (!IsMultiTheme)
	// 	{
	// 		if (Theme.Categories.Count == 0)
	// 		{
	// 			if (decomp == null) decomp = new();
	//
	// 			Theme = decomp.Decompile(filePath, FileNameEx, Source);
	// 		}
	// 	}
	// 	else
	// 	{
	// 		LoadMultiTheme();
	// 	}
	// }
	//
	// public void LoadMultiTheme()
	// {
	// 	if (Theme.Categories.Count == 0)
	// 	{
	// 		if (decomp == null) decomp = new ();
	//
	// 		Themes = decomp.DecompileMultiTheme(filePath, FileNameEx, Source);
	// 	}
	// }

}