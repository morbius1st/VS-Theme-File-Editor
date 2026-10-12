// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             ThemeList.cs
// Created:      2026-09-10 (22:09)

using System.Collections.Immutable;
using static VS_Theme_Editor.ThemeColorSupport;
using Microsoft.VisualStudio.TextManager.Interop;
using UtilityLibrary;

namespace VS_Theme_Editor;

public class ThemeReportSupport
{
	public ThemeReportSupport(WinThemeReportViewModel vm, ThemeFile tf)
	{
		this._vm = vm;
		this.tf = tf;
		// t = tf.GetTheme()!;

		_theme = tf.ThemeFinal;

		ceList = new ();
		}

	private readonly WinThemeReportViewModel _vm;

	private ThemeFile tf;

	private readonly Theme? _theme;

	private List<string> ceList;
	
	private int which;
	private bool verbose;

	private int ceCount;
	private int cdTotalCount;
	private int ceTotalCount;

	private int longestCe;

	private string priorFgTypeName;
	private string priorFgColorDesc;
	private string priorBgTypeName;
	private string priorBgColorDesc;


	private const int TITLE_WIDTH1 = 16;
	private const int TITLE_WIDTH2 = 74;

	private const int VALUE_WIDTH1 = 46;

	private const int THEME_SOURCE_WIDTH = 16;
	private const int THEME_SOURCE_TYPE_WIDTH = 30;
	private const int THEME_SOURCE_TYPE_DESC_WIDTH = 20;

	private const int COLOR_VALUE_WIDTH = 12;
	private const int COLOR_TYPE_WIDTH = 16;

	private const int COUNT_WIDTH = 4;

	private const int CAT_NAME_WIDTH = 36;

	private const string TAB_ALT = "    ";

	public void GenerateReport(int w, bool v)
	{
		string temp;

		which = w;
		verbose = v;

		if (!tf.IsValid())
		{
			_vm.AppendLine("Invalid Theme File");
			return;
		}

		_vm.AppendLine($"****  THEME REPORT  ****");

		if (_theme == null)
		{
			_vm.AppendLine($"\t**  THEME is NULL  **");
			return;
		}

		_vm.ProgressValue = 0;
		longestCe = 0;

		reportCount();

		temp = $"Which is {which,-2}";
		_vm.AppendLine($"{temp,-TITLE_WIDTH1} | Verbose is {verbose}");
		_vm.Display();
		_vm.AppendLine("\n\n**** HEADER ****\n");

		_vm.AppendLine("\n{ header information }n");


		// reportHeader();
		// _vm.Display();

		if (which == 0)
		{
			_vm.ProgressMaximum = cdTotalCount;

			_vm.AppendLine("\n\n**** CATEGORIES ****\n");

			cdHeader();
			spacer(0, "-".Repeat(70));
			reportCdData();

			if (verbose) reportEnd();
		}
		else
		{
			// which > 0
			_vm.ProgressMaximum = cdTotalCount + ceTotalCount;

			_vm.AppendLine("\n**** CATEGORIES  AND COLOR ENTRIES ****\n");

			cdHeader();
			spacer(0, "-".Repeat(70));
			reportCdData();

			if (verbose)
			{
				reportEnd();

				_vm.LineBreak();
				_vm.AppendLine($"\nLongest Ce = {longestCe}");
				_vm.Display();
			}


		}

		_vm.ProgressValue = 0;
	}

	private void reportCount()
	{
		cdTotalCount = 0;
		ceTotalCount = 0;

		if (_theme == null) return;

		foreach ((string _, CategoryData cd) in _theme.Categories)
		{
			cdTotalCount++;
			ceTotalCount += cd.Entries.Count;
		}
	}

	private void reportHeader()
	{

		_vm.AppendLine($"{"Name", -TITLE_WIDTH1} | {tf.ThemeName}");
		_vm.AppendLine($"{"Defined Name", -TITLE_WIDTH1} | {tf.DefinedThemeName}");
		_vm.AppendLine($"{"Description", -TITLE_WIDTH1} | {tf.Description}");
		_vm.AppendLine($"{"File Path", -TITLE_WIDTH1} | {tf.FilePath}");
		_vm.AppendLine($"{"Source Desc", -TITLE_WIDTH1} | {tf.SourceDesc}");
		_vm.AppendLine($"{"Style Desc", -TITLE_WIDTH1} | {tf.StyleDesc}");
		_vm.AppendLine($"{"Guid", -TITLE_WIDTH1} | {tf.GuidStr}");
		_vm.AppendLine($"{"FallBack", -TITLE_WIDTH1} | {tf.FallBack}");
		_vm.AppendLine($"{"Counts", -TITLE_WIDTH1} | CD {cdTotalCount} | CE {ceTotalCount}");

		_vm.Display();

		if (!verbose) return;
		
		_vm.AppendLine($"{"VsId", -TITLE_WIDTH1} | {tf.VsId}");
		_vm.AppendLine($"{"Parent", -TITLE_WIDTH1} | {tf.Parent ?? "Empty"}");
		_vm.AppendLine($"{"Is Multi Theme", -TITLE_WIDTH1} | {tf.IsMultiTheme}");
		_vm.AppendLine($"{"Number of Themes", -TITLE_WIDTH1} | {tf.Themes?.Count ?? 0}");
		_vm.AppendLine($"{"Overlay Themes", -TITLE_WIDTH1} | {string.Join(",", (tf.OverlayThemes))}");
		_vm.AppendLine($"{"Slug", -TITLE_WIDTH1} | {tf.Slug}");

		_vm.Display();
	}

	private void spacer(int qty = 1, string preface = "")
	{
		if (!preface.IsVoid()) _vm.AppendLine(preface);

		for (int i = 0; i < qty; i++)
		{
			_vm.LineBreak();
		}

		_vm.Display();
	}

	private void cdHeader()
	{
		// a
		if (verbose && which > 0) _vm.Append($"{" ", -THEME_SOURCE_WIDTH}  ");
		
		// b + c
		_vm.Append($" {"ce's", -COUNT_WIDTH} | {"Category Name", -CAT_NAME_WIDTH} ");
		
		// d
		if (verbose) _vm.Append($"[ = {"ce's", -COUNT_WIDTH} + {"ce's", -COUNT_WIDTH} ]");

		_vm.LineBreak();

		if (which >= 1)
		{
			// j
			if (verbose) _vm.Append($"{" ", -THEME_SOURCE_WIDTH}  ");
			//k + l
			_vm.AppendLine($" {" ", -COUNT_WIDTH} |     Color Entry Name");
		}
	}

	private void reportCdData()
	{
		ceCount = 0;

		if (_theme == null) return;

		int idx = 0;

		foreach ((string _ , CategoryData cd) in _theme.Categories)
		{
			_vm.ProgressValue++;	

			ceCount += cd.Entries.Count;

			cdInfo(cd);

			if (which > 0)
			{
				reportCeList(cd);

				_vm.Display();
			}

			// if (idx++ > 4) break;
		}

	}

	private void cdInfo(CategoryData cd)
	{
		string temp;

		// A
		if (verbose && which >= 1)
		{
			_vm.Append($"{cd.SourceFileName, -THEME_SOURCE_WIDTH} |");
		}

		// B + C
		_vm.Append($" {cd.Entries.Count, -COUNT_WIDTH} | {cd.Name, -CAT_NAME_WIDTH} ");

		// C
		if (verbose && cd.OriginalColorThemeEntryCount > 0)
		{
			temp = $" + {cd.AddedColorEntryCount}";
			_vm.Append($"[ = {cd.OriginalColorThemeEntryCount}{temp} ]");
		}

		_vm.LineBreak();
		_vm.Display();
	}

	private void reportCeList(CategoryData cd)
	{
		string temp;
		string sourceFileNamePrior = string.Empty;
		priorFgTypeName = string.Empty;
		priorFgColorDesc = string.Empty;
		priorBgTypeName = string.Empty;
		priorBgColorDesc = string.Empty;

		ThemeData.ThemeSource themeSourcePrior = ThemeData.ThemeSource.TSRC_NONE;

		createCeList(cd);

		int idx = 0;

		foreach (string ceName in ceList)
		{
			_vm.ProgressValue++;

			if (ceName.Length > longestCe) longestCe = ceName.Length;

			ThemeColorEntry ce = cd.Entries[ceName];

			bool b1 = ce.SourceFileName.Equals(sourceFileNamePrior);
			bool b2 = ce.ThemeSource == themeSourcePrior;
			
			ceInfo(ce, b1, b2);
			sourceFileNamePrior = ce.SourceFileName;
			themeSourcePrior = ce.ThemeSource;

			// if (idx++ > 20) break;
		}
	}

	private void ceInfo(ThemeColorEntry ce, bool srcNameChgd, bool themeSourceChgd)
	{
		// J
		if (verbose && which >= 1) ceSourceInfo(ce, srcNameChgd);

		// K + L
		_vm.Append($" {" ", -COUNT_WIDTH} | {$"    {ce.Name}", -TITLE_WIDTH2}");

		// M
		if (verbose && which >= 9) cdThemeSourceInfo(ce, themeSourceChgd);

		// Q+
		if (which > 1) ceData(ce);

		_vm.LineBreak();
		_vm.Display();
	}

	private void ceSourceInfo(ThemeColorEntry ce, bool srcNameChgd)
	{
		string temp;

		if (srcNameChgd)
		{
			temp = $"{TAB_ALT} ▾";
		}
		else
		{
			temp = $"{TAB_ALT}{ce.SourceFileName}";
		}

		_vm.Append($"{temp, -THEME_SOURCE_WIDTH}  ");
	}

	private void cdThemeSourceInfo(ThemeColorEntry ce, bool srcNameChgd)
	{
		string temp1;
		string temp2;

		if (srcNameChgd)
		{
			temp1 = $"{TAB_ALT} ▾";
			temp2 = $"{TAB_ALT} ▾";
		}
		else
		{
			temp1 = ce.ThemeSource.ToString();
			temp2 = ce.SourceDesc;

		}

		_vm.Append($"[ {temp1,-THEME_SOURCE_TYPE_WIDTH} | {temp2,-THEME_SOURCE_TYPE_DESC_WIDTH} ");
	}

	private void ceData(ThemeColorEntry ce)
	{
		string temp;
		ThemeData.VsColorType fgType = (ThemeData.VsColorType) ce.ForegroundType;
		ThemeData.VsColorType bgType = (ThemeData.VsColorType) ce.BackgroundType;

		// Tuple<string, string, string> fgInfo2 = getColorInfo("fg", fgType, ce.ForegroundColorValue, ce.ForegroundIndexValue);
		Tuple<string, string, string> fgInfo = getColorInfo(ce.TcForeground);
		

		// Q
		if ((verbose && which >= 1) || (!verbose && which >= 2))
		{
			_vm.Append($" | fg| {fgInfo.Item1,-COLOR_VALUE_WIDTH} ");
		}

		// R & X
		if ((verbose && which >= 2) || (!verbose && which >= 9))
		{
			//
			temp = getValueOrPrior(fgInfo.Item2, ref priorFgTypeName);
			_vm.Append($"{temp, -COLOR_TYPE_WIDTH}");

			temp = getValueOrPrior(fgInfo.Item3, ref priorFgColorDesc);
			_vm.Append($"{temp, -VALUE_WIDTH1}");

		}

		// Tuple<string, string, string> bgInfo2 = getColorInfo("bg", bgType, ce.BackgroundColorValue, ce.BackgroundIndexValue);
		Tuple<string, string, string> bgInfo = getColorInfo(ce.TcBackground);

		// S
		if ((verbose && which >= 1) || (!verbose && which >= 2))
		{
			_vm.Append($" | bg| {bgInfo.Item1, -COLOR_VALUE_WIDTH} ");
		}

		// T & Y
		if ((verbose && which >= 2) || (!verbose && which >= 9))
		{
			temp = getValueOrPrior(bgInfo.Item2, ref priorBgTypeName);
			_vm.Append($"{temp, -COLOR_TYPE_WIDTH}");

			temp = getValueOrPrior(bgInfo.Item3, ref priorBgColorDesc);
			_vm.Append($"{temp, -VALUE_WIDTH1}");
		}
	}

	private string getValueOrPrior(string value, ref string prior)
	{
		if (!value.IsVoid() && value.Equals(prior)) return $"{TAB_ALT} ▾";

		prior = value;
		return value;
	}

	//           color   color    color
	//           val str type str desc str
	//             ci1     ci2     ci3
	private Tuple<string, string, string> getColorInfo(ThemeColor tc)
	{
		string typeName = tc.ColorType.ToString();

		string ci1;
		string ci2 = $"[ {typeName} ]";
		string ci3 = string.Empty;

		if (tc.ColorType == ThemeData.VsColorType.CT_RAW)
		{
			ci1 = tc.ColorEx.ToHex;
			ci3 = $"( ARGB )";
			return new (ci1, ci2, ci3);
		}

		if (tc.ColorType == ThemeData.VsColorType.CT_COLORINDEX)
		{
			ci1 = "None";
			ci3 = $"( Color Index [ {tc.ColorIndex:N0} ] | {ColorIdxColors.Name((COLORINDEX) tc.ColorIndex)} )";
			return new (ci1, ci2, ci3);
		}

		if (tc.ColorType == ThemeData.VsColorType.CT_SYSCOLOR)
		{
			string colorName;
			ci1 = tc.ColorEx.ToHex;
			ci3 = "System Color";

			if (tc.ColorIndex != -1)
			{
				if (!SystemColors.Translate(tc.ColorIndex, out colorName, out _))
				{
					colorName = "Unknown";
					ci1 = "Invalid";
				}

				ci3 = $"( {ci3} [ {tc.ColorIndex:N0} ] | {colorName} )";
				return new(ci1, ci2, ci3);
			}

			ci3 = $"( ARGB ) ( Custom {ci3} )";
			return new (ci1, ci2, ci3);
		}

		// all else
		return new ("---", ci2, "( None | n/a )");
	}

	/* removed
	//           color   color    color
	//           val str type str desc str
	//             ci1     ci2     ci3
	private Tuple<string, string, string> getColorInfo(string Layer, ThemeData.VsColorType type, uint color, int idx)
	{
		ThemeData.VsColorType ct = (ThemeData.VsColorType) type;

		string? colorName;
		string colorValStr;
		string colorIdxStr;
		string typeName = ct.ToString();
		// string typeDesc = ThemeData.VsThemeTypeInfo[t].Item1;

		string ci1;
		string ci2 = $"[ {typeName} ]";
		string ci3 = string.Empty;


		if (ct == ThemeData.VsColorType.CT_SYSCOLOR)
		{
			ci3 = "System Color";
			colorValStr = "";

			if (idx != -1)
			{
				colorIdxStr = idx.ToString();

				if (!SystemColors.Translate(idx, out colorName, out _))
				{
					colorName = "Unknown";
					colorValStr = "Invalid";
				}

				ci1 = colorValStr;
				ci3 = $"( {ci3}  [ {colorIdxStr} ] | {colorName} )";

				return new (ci1, ci2, ci3);
			}

			ci3 = $"( Custom {ci3} )";

			ct = ThemeData.VsColorType.CT_RAW;
		}

		if (ct == ThemeData.VsColorType.CT_RAW)
		{
			colorValStr = FormatArgb(color);

			ci1 = colorValStr;
			ci3 = $"( ARGB ) {ci3}";

			return new (ci1, ci2, ci3);
		}

		if (ct == ThemeData.VsColorType.CT_COLORINDEX)
		{
			colorIdxStr = idx.ToString();

			colorName = ColorIdxColors.Name((COLORINDEX) idx);

			if (colorName.IsVoid())
			{
				colorName = "Unknown";
			}

			ci1 = "None";
			ci3 = $"( Color Index  [ {colorIdxStr} ] | {colorName} )";

			return new (ci1, ci2, ci3);
		}


		if (ct == ThemeData.VsColorType.CT_AUTOMATIC)
		{
			ci1 = "n/a";
			ci3 = "( None )";
			return new (ci1, ci2, ci3);
		}

		return new ("---", ci2, string.Empty);
	}
	*/


	private void reportEnd()
	{
		int a = _theme.OriginalCategoryCount;
		int b = _theme.AddedCategoryCountTotal;
		int c = a + b;

		_vm.AppendLine($"\nTotal cd's: {c}  [ = {a} + {b} ]");


		a = _theme.OriginalTotalColorEntryCount;
		b = ceCount - _theme.OriginalTotalColorEntryCount;

		_vm.AppendLine($"\nTotal ce's: {ceCount}  [ = {a} + {b} ]");
		_vm.Display();
	}

	private void createCeList(CategoryData cd)
	{
		ceList = cd.Entries.Keys.ToImmutableSortedSet().ToList();
	}


	//          color #    color   color  color
	//                     type    type   name
	//          as str     to str  desc   maybe

	// generically, return
	// byte color system type (to allow for switching)
	// color as int, color as hex string, color type as string, color type description as string
	// when sys color: add index string, color function



}