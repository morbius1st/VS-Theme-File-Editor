using System.Diagnostics;
using System.IO;
using UtilityLibrary;
using static VS_Theme_Editor.ThemeData.ThemeCompareTypes;

// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             Samples.cs
// Created:      2026-08-08 (14:08)

namespace VS_Theme_Editor;

public class Samples
{
	private static ThemeManager tm = ThemeManager.Instance;


	public static void TestSamples2()
	{
		ThemeFile tf;

		// foreach (string sample in VsThemesSamples2)
		foreach (string sample in VsThemesSamples3)
		{
			Debug.WriteLine($"\n** Processing sample: {sample}");
			string? result = tm.ProcessSelectedTheme(sample, out tf, true);

			if (result.IsVoid())
			{
				Debug.WriteLine($"+++> returned value is void | {sample}");
				continue;
			}

			showSampleResults(tf);

			Debug.WriteLine(" ");
		}
	}

	public static void TestSampleClone()
	{
		ThemeFile tf;

		string sample = VsThemesSamples2[0];

		if (!VisualStudioData.Instance.SetVsInfo(sample)) return;

		if (!ThemeSupport.Instance.GetThemeFileForSelected(sample, true, out tf) .HasValue) return;

		tf.InstallTheme(true);

		Debug.WriteLine($"\n*** got theme data? for {tf.FileName}");

		Theme? t1 = tf.GetTheme();

		if (t1 == null)
		{
			Debug.WriteLine("\t*** theme is null ***\n");
		}

		Theme t2 = ThemeSupport.Instance.CloneTheme(t1!);

		// match?

		bool m = t1 != null && t1.Categories.Count == t2.Categories.Count;

		bool m0 = t1 != null && t1.Categories[0].Value.Entries.Count == t2.Categories[0].Value.Entries.Count;
		bool m1 = t1 != null && t1.Categories[1].Value.Entries.Count == t2.Categories[1].Value.Entries.Count;

		bool me1 = t1 != null && t1.Categories[1].Value.Entries[0].Value.Name!.Equals(t2.Categories[1].Value.Entries[0].Value.Name);

		Debug.WriteLine($"** test results - match? | cat count {m} | entry count 0 {m0} | entry count 1 {m1} | entry match 1 {me1}");
	}

	public static void TestSampleCompare()
	{
		PkgDefDecompiler deComp = new ();
		ThemeCompareSupport tcm = ThemeCompareSupport.Instance;

		Theme t1 = deComp.Decompile(VsThemesSamplesCompare[0], ThemeData.ThemeSource.TSRC_VS_BUILTIN, true);
		Theme t2 = deComp.Decompile(VsThemesSamplesCompare[1], ThemeData.ThemeSource.TSRC_VS_OVERLAY, true);

		tcm.CompareEx(t1, t2, CompareTypes[tctIdx], verbose);

		tctIdx++;

		if (tctIdx >= CompareTypes.Length)
		{
			tctIdx = 0;
			verbose = !verbose;
		}
	}

	private static void showSampleResults(ThemeFile tf)
	{
		if (!tf.IsValid())
		{
			Debug.WriteLine($"+++> ThemeFile is not valid | {tf.FilePath}");
			return;
		}

		bool found = File.Exists(tf.FilePath);

		Debug.WriteLine($"**\t exists? {found,-6}| FileName: {tf.FileName, -36}, Style: {tf.Style, -14}, Source: {tf.Source, -26}, IsMultiTheme: {tf.IsMultiTheme, -7}, Parent: {tf.Parent ?? "null", -20}, Overlay: {tf.Overlay ?? "null", -16}"); // | {tf.FilePath}");


		if (tf.IsMultiTheme)
		{
			Debug.Write($"** multi-theme list");

			foreach ((string _ , Theme theme) in tf.Themes!)
			{
				Debug.Write($" > {theme.Name} ( {theme.Guid} )[ {theme.Categories.Count} ]");
			}

			Debug.WriteLine(" ");
		}
		else
		{
			Theme? t = tf.GetTheme();

			Debug.Write($"** single-theme list");

			Debug.WriteLine($" > {t?.Name} ( {t?.Guid} )[ {t?.Categories.Count} ]");
		}

		string s;

		if (tf.OverlayThemes.Count > 0)
		{
			Debug.Write("***\toverlay themes | ");

			foreach (Theme t in tf.OverlayThemes)
			{
				s = $"{t.Name}";

				Debug.Write($"> {s} [ {t.Categories.Count} ] ");
			}

			Debug.WriteLine(" ");
		}

		if (tf.ThemeFinal == null) return;

		Debug.WriteLine($"***\tfinal theme | {tf.ThemeFinal.Name} | categories {tf.ThemeFinal.Categories.Count} | added categories {tf.ThemeFinal.AddedCategoryCountTotal} | >> ");

		int count = 0;

		foreach ((string catName, CategoryData catData) in tf.ThemeFinal.Categories)
		{
			if (catData.AddedColorEntryCount == 0) continue;

			Debug.Write($" > ( {catName} [ +{catData.AddedColorEntryCount} ] ) ");

			if (count++ > 10)
			{
				count = 0;
				Debug.Write(" < |\n** >> ");
			}
		}

		Debug.WriteLine(" ");

		ShowMergeResults();
	}

	public static void ShowMergeResults()
	{
		Debug.WriteLine("\n*** merge results \n");

		foreach (var (mergeId, mergeDepth, message) in ThemeSupport.Instance.MergeResults)
		{
			Debug.WriteLine($"{"\t".Repeat(mergeDepth)}** {mergeId} | {message}");
		}

		Debug.WriteLine("\n*** done \n");
	}

	private static bool verbose = true;
	private static int tctIdx  = 2;

	private static readonly ThemeData.ThemeCompareTypes[] CompareTypes = new []
	{
		TCT_BASIC_1,
		TCT_EXTENDED_2,
		TCT_EXTENDED_3,
	};

	public readonly static string[] VsThemesSamplesCompare =
	[
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\18.0_86373a45Jeff\Extensions\CyberDark\CyberDark.pkgdef",          // has fallback

		@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform\Theme.Dark.pkgdef",
	];

	public readonly static string[] VsThemesSamples2 =
	[
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\18.0_86373a45Jeff\Extensions\CyberDark\CyberDark.pkgdef",				// has fallback

		// user theme
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\18.0_86373a45\Extensions\eksixheh.0al\Colors.pkgdef",              // no fallback
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\18.0_86373a45Jeff\Extensions\CyberLight\CyberLight.pkgdef",        // has fallback
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\18.0_86373a45Jeff\Extensions\CyberDark\CyberDark.pkgdef",          // has fallback
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\17.0_cb502d6aExp\Extensions\jeffs\CyberDark\1.0\CyberDark.pkgdef", // no fallback
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\17.0_cb502d6aExp\Extensions\TestOnly\Theme.Bubblegum.pkgdef",      // no fallback

		// system theme
		@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform\Theme.Blue.pkgdef",
		@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform\Theme.Dark.pkgdef",
		@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform\Theme.Light.pkgdef",
		@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform\Shell.Light.pkgdef",
		@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform\EditorColors.pkgdef",


		@"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Platform\Theme.Bubblegum.pkgdef",
		@"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Platform\Theme.Light.pkgdef",
		@"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Platform\Shell.Light.pkgdef",
		@"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Platform\EditorColors.pkgdef",
		@"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Platform\Editor.Dark.ExtraContrast.pkgdef",

		// fails
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\15.0_0c65df91Exp\Extensions\ahwlcvlt.tul\Colors.pkgdef",
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\18.0_86373a45\Extensions\notexist\Colors.pkgdef",
		@"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Platform\Theme.bogus.pkgdef",
	];

	// the 5 basic theme types x 3 theme styles x 2 theme sources
	public readonly static string[] VsThemesSamples3 =
	[
		// user with fallback A
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\18.0_86373a45Jeff\Extensions\CyberDark\CyberDark.pkgdef",			 // has fallback
		// merge sequence: 1. CyberDark (user) + 2. Theme.Dark (root)  + 3. Shell.Dark (root) + 4. EditorColors (root)

		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\18.0_86373a45Jeff\Extensions\CyberLight\CyberLight.pkgdef",        // has fallback
		// merge sequence: 1. CyberLight (user) + 2. Theme.Light (root)  + 3. Shell.Light (root) + 4. EditorColors (root)

		// user no fallback - B
		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\17.0_cb502d6aExp\Extensions\jeffs\CyberDark\1.0\CyberDark.pkgdef", // no fallback
		// assumes light / merge sequence: 1. CyberLight (user) + 2. Theme.Light (root)  + 3. Shell.Light (root) + 4. EditorColors (root)

		@"C:\Users\jeffs\AppData\Local\Microsoft\VisualStudio\17.0_cb502d6aExp\Extensions\TestOnly\Theme.Bubblegum.pkgdef",      // no fallback - but sounds like a system theme
		// assumes light / merge sequence: 1. CyberLight (user) + 2. Theme.Light (root)  + 3. Shell.Light (root) + 4. EditorColors (root)

		// root theme A - C
		@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform\Theme.Dark.pkgdef",
		// no fallback but configured to use shell dark + editor colors

		// root theme B - D
		@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform\Shell.Light.pkgdef",
		// no fallback but configured to use theme light + editor colors

		// built-in theme - E
		@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform\Theme.Blue.pkgdef",
		// no fallback but configured to use theme dark + shell dark + editor colors
	];


	public readonly static string VsThemeReportSample1 =
		"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur ut ex et tortor\n" +
		"tristique accumsan a sed est. Sed non arcu sit amet enim ornare tempor. Morbi eget\n" +
		"quam quam. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere\n" +
		"cubilia curae; Sed non tortor odio. Vivamus tempus vulputate arcu in ullamcorper.\n" +
		"Phasellus sagittis mi nec nisl eleifend, ut lacinia tortor varius. Maecenas at\n" +
		"vehicula risus, ut hendrerit ante. In suscipit, tellus nec pretium efficitur, felis\n" +
		"ipsum sollicitudin est, sit amet commodo sapien ipsum sed turpis. Ut fringilla enim\n" +
		"suscipit, vulputate velit quis, vehicula tellus.  "+ "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur ut ex et tortor\n" +
		"tristique accumsan a sed est. Sed non arcu sit amet enim ornare tempor. Morbi eget\n" +
		"quam quam. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere\n" +
		"cubilia curae; Sed non tortor odio. Vivamus tempus vulputate arcu in ullamcorper.\n" +
		"Phasellus sagittis mi nec nisl eleifend, ut lacinia tortor varius. Maecenas at\n" +
		"vehicula risus, ut hendrerit ante. In suscipit, tellus nec pretium efficitur, felis\n" +
		"ipsum sollicitudin est, sit amet commodo sapien ipsum sed turpis. Ut fringilla enim\n" +
		"suscipit, vulputate velit quis, vehicula tellus.  ";
}