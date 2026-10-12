using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using static VS_Theme_Editor.VisualStudioData;
using static VS_Theme_Editor.ThemeData;
using static VS_Theme_Editor.ThemeData.ThemeStyle;
using static VS_Theme_Editor.ThemeData.ThemeSource;
using static VS_Theme_Editor.VsThemeData;

// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             VsThemeFileConfigure.cs
// Created:      2026-08-05 (19:08)

namespace VS_Theme_Editor;

public class VsThemeFileConfigure
{
	private static readonly VsThemeData Vtd = VsThemeData.Instance;

	private static readonly ThemeSupport Ts = ThemeSupport.Instance;

	public static void AssignRootNames()
	{
		Vtd.RootNames = new string[vsThemeIdx];
		// applies to both VS 2022 and VS 2026
		Vtd.RootNames[ThemeLightIdx]    = "Theme.Light";
		Vtd.RootNames[ShellLightIdx]    = "Shell.Light";
		Vtd.RootNames[ThemeDarkIdx]     = "Theme.Dark";
		Vtd.RootNames[ShellDarkIdx]     = "Shell.Dark";
		Vtd.RootNames[ThemeHcIdx]	    = "Theme.HighContrast";
		Vtd.RootNames[ShellHcIdx]       = "Shell.HighContrast";

		// applies to VS 2022 only
		Vtd.RootNames[BlueIdx]          = "Theme.Blue";
		Vtd.RootNames[BlueEcIdx]        = "Theme.Blue.ExtraContrast";
		Vtd.RootNames[EditorColorsIdx]  = "EditorColors";

		// applies to VS 2026 only
		Vtd.RootNames[EditorLightEcIdx] = "Editor.Light.ExtraContrast";
		Vtd.RootNames[EditorDarkEcIdx]  = "Editor.Dark.ExtraContrast";
	}

	public static void ProcessPrimeVsRootThemeFiles()
	{
		loadVsRootThemeData([VSID_2022COMM, VSID_2026COMM], true  , ThemeLightIdx, "Vs Root Theme Light"                   , TSTY_LIGHT  , TSRC_VS_THEME   , false  , null      , ShellLightIdx);
		loadVsRootThemeData([VSID_2022COMM, VSID_2026COMM], true  , ThemeDarkIdx, "Vs Root Theme Dark"                     , TSTY_DARK   , TSRC_VS_THEME   , false  , null      , ShellDarkIdx);
		loadVsRootThemeData([VSID_2022COMM, VSID_2026COMM], true  , ThemeHcIdx, "Vs Root Theme High Contrast"              , TSTY_HC     , TSRC_VS_THEME   , false  , null      , ShellHcIdx);
		loadVsRootThemeData([VSID_2022COMM],                true  , BlueIdx, "Vs Root Theme Blue"                        , TSTY_LIGHT  , TSRC_VS_THEME   , false  , null      , ShellLightIdx);
		loadVsRootThemeData([VSID_2022COMM],                true  , BlueEcIdx, "Vs Root Theme Blue Extra Contrast"       , TSTY_LIGHT  , TSRC_VS_THEME   , false  , null      , ShellLightIdx);
	}

	public static void ProcessOverlayVsRootThemeFiles()
	{
		loadVsRootThemeData([VSID_2022COMM, VSID_2026COMM], false, ShellLightIdx, "Vs Root Shell Light"                   , TSTY_LIGHT  , TSRC_VS_OVERLAY , false  , ThemeLightIdx, EditorColorsIdx);
		loadVsRootThemeData([VSID_2022COMM, VSID_2026COMM], false, ShellDarkIdx, "Vs Root Shell Dark"                     , TSTY_DARK   , TSRC_VS_OVERLAY , false  , ThemeDarkIdx , EditorColorsIdx);
		loadVsRootThemeData([VSID_2022COMM, VSID_2026COMM], false, ShellHcIdx, "Vs Root Shell High Contrast"              , TSTY_HC     , TSRC_VS_OVERLAY , false  , ThemeHcIdx   , EditorColorsIdx);
		loadVsRootThemeData([VSID_2022COMM, VSID_2026COMM], false, EditorColorsIdx, "Vs Root Editor Colors"               , TSTY_NA     , TSRC_VS_OVERLAY , true   , null         , null);
		// loadVsRootThemeData([VSID_2026COMM],                false, EditorLightEcIdx, "Vs Root Editor Light Extra Contrast", TSTY_NA     , TSRC_VS_OVERLAY , false  , null         , EditorDarkEcIdx);
		// loadVsRootThemeData([VSID_2026COMM],                false, EditorDarkEcIdx, "Vs Root Editor Dark Extra Contrast"  , TSTY_NA     , TSRC_VS_OVERLAY , false  , null         , null);
	}

	private static void loadVsRootThemeData(string[] vsIds, bool isPrime,
		int a, string b, ThemeStyle c, ThemeSource d, bool e, int? f, int? g)
	{
		foreach (string vsId in vsIds)
		{
			string filePath = Path.Combine(Vtd.VsThemeDirs[vsId], $"{Vtd.RootNames[a]}.pkgdef");
			
			Ts.ReadVsRootThemeGuid(filePath, out string guid);

			ThemeFile tf = new (filePath)
			{
				VsId = vsId,
				DefinedThemeName = Vtd.RootNames[a],
				Description  = b,
				Style        = c,
				Source       = d,
				IsMultiTheme = e,
				Parent       = f.HasValue ? Vtd.RootNames[f.Value] : null,
				Overlay      = g.HasValue ? Vtd.RootNames[g.Value] : null,

				Guid = new Guid(guid),
				FallBack = guid,
				Slug = string.Empty,
			};

			Vtd.ThemeFileListAdd(vsId, tf);

			if (isPrime) Vtd.ThemeIndexAdd(vsId, tf.Guid, tf.FileNameEx);
		}
	}
}