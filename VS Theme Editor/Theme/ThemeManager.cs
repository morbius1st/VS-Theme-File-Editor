// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             ThemeManager.cs
// Created:      2026-08-03 (19:08)

namespace VS_Theme_Editor;

public class ThemeManager
{
	public static ThemeManager Instance { get; }

	static ThemeManager()
	{
		if (Instance == null)
		{
			Instance = new ThemeManager();
		}
	}

	private ThemeManager()
	{
		VsThemeData vtd1 = VsThemeData.Instance;
		vtd1.Init();
	}

	private ThemeSupport ts = ThemeSupport.Instance;

	private VisualStudioData vsd = VisualStudioData.Instance;

	/// <summary>
	/// process the selected file to get the theme with or without the overlay information
	/// </summary>
	/// <remarks></remarks>
	public string? ProcessSelectedTheme(string filePath, out ThemeFile tf, bool getOverlay)
	{
		tf = ThemeFile.Invalid();

		Theme? themeFinal = null;

		if (!vsd.SetVsInfo(filePath)) return null;

		// get or create a themefile
		bool? result = ts.GetThemeFileForSelected(filePath, true, out tf);
		
		// if the result is null, no themefile returned
		if (result.HasValue)
		{
			// is true or false - i.e. is a VS theme file or a user theme file
			
			if (getOverlay)
			{
				// follow the overlay path and add all of the theme files
				// ts.GetOverlayThemes(ref tf, false);
				ts.GetOverlayThemes(ref tf, false);

				if (!tf.IsMultiTheme &&
					!ts.MergeThemes(tf.GetTheme()!, tf.OverlayThemes, out themeFinal)) result = null;

				if (result.HasValue) Samples.ShowMergeResults();
			}
			else
			{
				themeFinal = tf.GetTheme();
			}
		}
		
		if (!result.HasValue)
		{
			tf.ThemeFinal = null;
			return null;
		}

		tf.ThemeFinal = themeFinal;

		return tf.ThemeName;
	}

	public string? GetThemeNameFromFile(string filePath)
	{
		string? name;

		ts.ReadThemeHeader(filePath, out name, out string? _, out string? _, out string? _);

		return name;
	}

	// public Theme? CompareThemes(string filePath1, string filePath2, int type)
	// {
	// 	if (!File.Exists(filePath1) && !File.Exists(filePath2)) return null;
	//
	// 	if (type == 0) return ts.CompareThemesA(filePath1, filePath2);
	//
	// 	if (type == 1) return ts.CompareThemesB(filePath1, filePath2);
	//
	// 	return null;
	// }
}