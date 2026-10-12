// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             ThemeCompareManager.cs
// Created:      2026-08-13 (20:08)

using System.Diagnostics;
using UtilityLibrary;
using static VS_Theme_Editor.ThemeData.ThemeCompareEventTypes;
using static VS_Theme_Editor.ThemeData.ThemeCompareTypes;
using static VS_Theme_Editor.ThemeData.VsColorType;
using static VS_Theme_Editor.ThemeData;


namespace VS_Theme_Editor;

public class ThemeCompareSupport
{
	private const string TA = "A";
	private const string TB = "B";
	private const int EVT_COL_WIDTH_A = 34;
	private const int EVT_COL_WIDTH_B = 48;

	public static ThemeCompareSupport Instance { get; } = new ThemeCompareSupport();

	static ThemeCompareSupport()
	{
		if (Instance == null!)
		{
			Instance = new ThemeCompareSupport();
		}
	}

	private ThemeCompareSupport()
	{
		themeA = new Theme("test A");
		themeB = new Theme("test B");

		eventTypeCounts = new ();

		currFldName = string.Empty;
	}

	private Dictionary<string, ThemeCompareEvent> compareEvents = new ();

	private Theme themeA;
	private Theme themeB;

	private string? currCdName;
	private string? currCeName;
	private string currFldName;

	private int idx;

	private ThemeCompareTypes compareType;
	private bool verbose;

	// private int _EventTypes => Enum.GetValues(typeof(ThemeCompareEventTypes)).Length;

	private int _EventCount => compareEvents.Count;

	private Dictionary<ThemeCompareEventTypes, int> eventTypeCounts;

	// ReSharper disable once MemberCanBePrivate.Global
	public bool IgnoreGuid { get; set; } = true;

	// ReSharper disable once ParameterHidesMember
	public int CompareEx(Theme? tA, Theme? tB, ThemeCompareTypes compType, bool verbose = false)
	{
		compareInit(tA, tB, verbose, compType);

		recordEvent(TA, TE_TITLE_MARKER, null, $"Start of comparison | verbose? {verbose} | type {ThemeData.ThemeCompareTypeInfo[compType].Item2}");

		if (comparePrep(tA, tB) < 0) return -1;

		compareThemeHeaderData();

		compareCdListsEx();

		compareCeListsEx();

		if (compareType > TCT_BASIC_1)
		{
			countEventsTypes();
			showEventCounts();
		}

		showEventList();

		return _EventCount;
	}

	// ReSharper disable once ParameterHidesMember
	private void compareInit(Theme? tA, Theme? tB, bool verbose, ThemeCompareTypes compType)
	{
		idx = 0;

		this.themeA = tA!;
		this.themeB = tB!;

		currCdName = string.Empty;
		currCeName = string.Empty;
		currFldName = string.Empty;

		compareType = compType;
		this.verbose = verbose;

		compareEvents = new ();
	}

	private int comparePrep(Theme? tA, Theme? tB)
	{
		bool result = true;

		recordEvent(TA, TE_HEADING_MARKER, null, "Validate theme names not null");

		if (tA == null)
		{
			recordEvent(TA, TE_THEME_NULL_A, null);
			result = false;
		}

		if (tB == null)
		{
			recordEvent(TB, TE_THEME_NULL_B, null);
			result = false;
		}

		if (!result)
		{
			recordEvent("!", TE_INVALID_DATA, null);
			return -1;
		}

		if (verbose) recordEvent("!", TE_VALID_DATA, [null]);

		return 1;
	}

	private void compareThemeHeaderData()
	{
		recordEvent(TA, TE_HEADING_MARKER, null, "Compare part 1 | heading data");

		currFldName = nameof(themeA.Name);
		testStrings(TA, themeA.Name, themeB.Name, TE_MISMATCH_STR, TE_MATCH_STR, null, TCT_BASIC_1) ;

		currFldName = nameof(themeA.Guid);
		if (!IgnoreGuid) testStrings(TA, themeA.Guid.ToString(), themeB.Guid.ToString(), TE_THEME_GUID_MISMATCH, TE_MATCH_DATA, null, TCT_EXTENDED_2);

		currFldName = nameof(themeA.Fallback);
		testStrings(TA, themeA.Fallback, themeB.Fallback, TE_FALLBACK_IMSMATCH, TE_MATCH_DATA, null, TCT_EXTENDED_2);

		currFldName ="Category Count A";
		if (themeA.Categories.Count == 0) recordEvent( TA, TE_CD_EMPTY_A, null);

		currFldName = "Category Count B";
		if (themeB.Categories.Count == 0) recordEvent( TB, TE_CD_EMPTY_B, null);
	}
	
	private void compareCdListsEx()
	{
		// comparison of CD's
		// 3 outcomes
		// 1. CD exists in A and is in B
		// 2. CD exists in B but not in A - whole CD + CE gets added to A
		// 3. CD exists in A but not in B - do nothing here - CD / CE in A are ok as is
		// 
		// for 1, compare CEs' for in A versus B, 3 outcomes
		// 1. CE exists in A and is in B - level 1 compare, ignore CE and CE data / Level 2 compare, compare CE data & flag difference
		// 2. CE exists in B but not in A - add CE to A
		// 3. CE exists in A but not in B - do nothing here - CE in A is ok as is

		currCeName = string.Empty;

		currFldName = "A CD Exists In B";

		recordEvent(TA, TE_HEADING_MARKER, null, "Compare part 2A | Compare CD lists - Theme A versus Theme B");

		// step 1: compare the category data list of theme A to theme B
		foreach ((string keyA, CategoryData cdA) in themeA.Categories)
		{
			currCdName = keyA;

			// if match found, get the cd from theme B and compare the CE entries count
			if (themeB.Categories.TryGetValue(keyA, out CategoryData cdB1))
			{
				// found a CD in both
				recordEvent( TB, TE_CD_FOUND_IN_B, [keyA, ""]);

				// compare guids - they should always not match - record when they do match
				if (compareType > TCT_BASIC_1)
				{
					if (cdA.Guid.Equals(cdB1.Guid))
					{
						recordEvent( TB, TE_CD_GUID_MATCH, [cdA.Guid.ToString()[^8..], cdB1.Guid.ToString()[^8..]]);
					}
				}

				if (verbose)
				{
					if (cdA.Entries.Count < cdB1.Entries.Count)
					{
						recordEvent( TB, TE_MISMATCH_COUNT_MORE, [$"A CE count {cdA.Entries.Count}", $"B CE count {cdB1.Entries.Count} (has more)"]);
					}
					else if (cdA.Entries.Count > cdB1.Entries.Count)
					{
						recordEvent( TB, TE_MISMATCH_COUNT_LESS, [$"A CE count {cdA.Entries.Count}", $"B CE count {cdB1.Entries.Count} (has less)"]);
					}
					else
					{
						recordEvent(TB, TE_MATCH_COUNT, [$"CE count {cdA.Entries.Count}", ""]);
					}
				}

				continue;
			}

			// not found 
			if (verbose) recordEvent( TB, TE_CD_NOT_IN_B, [keyA, ""]);
		}

		currFldName = "B CD Exists In A";

		recordEvent(TA, TE_HEADING_MARKER, null, "Compare part 2B | Compare CD lists - Theme B versus Theme A");

		// part 2, look for theme A CDs in theme B
		// attempt to get the category data from theme A using the key from theme B
		foreach ((string keyB, CategoryData _ ) in themeB.Categories)
		{
			currCdName = keyB;

			// if match found, continue to next key
			// no need to record event as this was already done in the previous loop
			if (themeA.Categories.ContainsKey(keyB)) continue;

			// not found, record event
			recordEvent( TA, TE_CD_NOT_IN_A, ["", keyB]);
		}
	}

	// ex version - follows the Cd Ex version and the
	// list of Cd events has been determined
	// ReSharper disable once UnusedParameter.Local
	private void compareCeListsEx()
	{
		if (compareType == TCT_BASIC_1) return;

		recordEvent(TA, TE_HEADING_MARKER, null, $"Compare CE lists - Theme A versus Theme B based on compare events list");

		// string[] keyParts;
		// int et;

		// create a list of those compare events that need to be processed
		Dictionary<string, ThemeCompareEvent> tEvts = compareEvents.Where(kvp => kvp.Value.Type == TE_CD_FOUND_IN_B).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

		foreach ((string _ , ThemeCompareEvent value) in tEvts)
		{
			compareCeListEx(themeA.Categories[value.CdName!], themeB.Categories[value.CdName!]);
		}
	}

	private void compareCeListEx(CategoryData cdA, CategoryData cdB)
	{
		recordEvent(TA, TE_HEADING_MARKER, null, $"Compare part 3 | Compare CE list - Theme A CD / {cdA.Name} versus Theme B CD / {cdB.Name}");

		// this only processes when a CD is found in both themes
		// this is compare type extended 1+
		// two comparisons
		// 1. compare A to B 
		//		extended 1 - note match / verbose, note missing
		//		extended 2 - validate CE data
		// 2. compare B to A
		//		extended 1 & 2 - note missing / verbose, n/a

		// 1. compare A to B - look for CE in A that are also in B
		foreach ((string key, ThemeColorEntry ceA) in cdA.Entries)
		{
			currCeName = key;

			currFldName = "A CE Exists In B";

			// try to get the entry in B - if yes, process, if no record event if verbose
			if (cdB.Entries.TryGetValue(key, out ThemeColorEntry ceB))
			{
				recordEvent( TA, TE_CE_FOUND_IN_B, [key, ""]);

				if (compareType > TCT_EXTENDED_2) compareCeEx(ceA, ceB);
			}
			else if (verbose) 
			{
				// A is not in B is do nothing - record if verbose
				recordEvent( TB, TE_CE_NOT_IN_B, [key, ""]);
			}
		}

		currFldName = "B CE Missing From A";

		// 2. look for CE in B not in A
		foreach ((string key, ThemeColorEntry _) in cdB.Entries)
		{
			currCeName = key;

			// if no match - record event
			// if match, processed above
			if (!cdA.Entries.ContainsKey(key))
			{
				recordEvent(TA, TE_CE_NOT_IN_A, ["", key]);
			}
		}
	}

	private void compareCeEx(ThemeColorEntry ceA, ThemeColorEntry ceB)
	{
		// process when extended 3+
		if (compareType < TCT_EXTENDED_3) return;


		compareCeColorEx(nameof(ceA.Foreground), 
			nameof(ceA.ForegroundType), ceA.Foreground, ceB.Foreground, 
			ceA.ForegroundType, ceB.ForegroundType, 0);

		compareCeColorEx(nameof(ceA.Background), 
			nameof(ceA.BackgroundType), ceA.Background, ceB.Background, 
			ceA.BackgroundType, ceB.BackgroundType, 1);
	}

	private void compareCeColorEx(string colorFldName, string typeFldName, string colorA, string colorB, byte typeA, byte typeB, int fgBg)
	{
		VsColorType colorTypeA = (VsColorType) typeA;
		VsColorType colorTypeB = (VsColorType) typeB;

		if (colorTypeA != CT_TRACK_FOREGROUND && colorTypeA != CT_TRACK_BACKGROUND)
		{
			currFldName = typeFldName;

			if (colorTypeA == colorTypeB)
			{
				if (colorTypeA != CT_INVALID)
				{
					if (verbose) recordEvent(TA, TE_CE_FG_TYPE_MATCH + fgBg, [colorTypeA.ToString(), ""]);

					// got matching types - check that the source matches
					currFldName = colorFldName;

					testStrings(TA, colorA, colorB,
						TE_CE_FG_MISMATCH - fgBg, TE_CE_FG_MATCH + fgBg, null);
				}
				else if (verbose)
				{
					recordEvent(TA, TE_CE_FG_TYPE_IS_INVALID, [colorTypeA.ToString(), ""]);
				}
			}
			else
			{
				recordEvent(TA, TE_CE_FG_TYPE_MISMATCH - fgBg, [colorTypeA.ToString(), colorTypeB.ToString()]);
			}
		}
		else
		{
			recordEvent(TA, TE_CE_FG_TYPE_NOT_VALID - fgBg, [colorTypeA.ToString(), ""]);
		}
	}

	// utility

	private string fmtKey(string themeId, ThemeCompareEventTypes eventType)
	{
		//                 0 through 10 (11 characters)
		//                 A    ± 1 4 1  .  0 0 0 0  1
		//                0,0   1,2,3,4  5 6,7,8,9,10 # name @ name
		// key format : [themeId][eventType] [index]#[cdName]@[ceName]
		string evt = ((int) eventType).ToString("+000;-000");
		string index = (++idx).ToString("D5");
		string cd = currCdName?.PadRight(30, '.') ?? string.Empty.PadRight(30, '.');
		string ce = currCeName?.PadRight(30, '.') ?? string.Empty.PadRight(30, '.');

		return $"{themeId}{evt}.{index}#{cd}@{ce}";
	}

	private void testStrings(string themeId, string? stringA, string? stringB, 
		ThemeCompareEventTypes eTypeFail, ThemeCompareEventTypes eTypeGood, 
		string? comment, ThemeCompareTypes minType = TCT_NONE)
	{
		if (minType > compareType) return;

		if (stringA.IsVoid() && stringB.IsVoid()) return;
		
		if (stringA.IsVoid())
		{
			recordEvent( themeId, TE_STR_VOID_A, [$"{currFldName} is null", "_"]);
		}

		if (stringB.IsVoid())
		{
			recordEvent(themeId, TE_STR_VOID_B, ["_", $"{currFldName} is null"]);
		}

		string dataA;
		string dataB;

		if (eTypeFail == TE_THEME_GUID_MISMATCH)
		{
			dataA = stringA![0..9];
			dataB = stringB![0..9];
		}
		else
		{
			dataA = stringA!;
			dataB = stringB!;
		}

		// record event if they do not match
		if (!(stringA!.Equals(stringB)))
		{
			recordEvent(themeId, eTypeFail, [dataA, dataB], comment);
		}
		else if (verbose)
		{
			// if verbose, record event that the strings do match
			recordEvent(themeId, eTypeGood, [dataA, dataB], comment);
		}
	}

	private void recordEvent(string themeId, ThemeCompareEventTypes eventType, 
		string?[]? evtData, string? comment = null, string? fName = null)
	{
		string evtKey = fmtKey(themeId, eventType);

		string fn = fName.IsVoid() ? currFldName : fName!;

		ThemeCompareEvent evt = 
			new (evtKey, eventType, idx.ToString(),  
				currCdName, currCeName, fn, evtData, comment);

		compareEvents.Add(evtKey, evt);
	}


	// post processing

	private void countEventsTypes()
	{
		eventTypeCounts = new Dictionary<ThemeCompareEventTypes, int>();

		if (_EventCount != 0)
		{
			string[] keyParts;
			int evtType;
			ThemeCompareEventTypes eType;

			foreach ((string key, ThemeCompareEvent _ ) in compareEvents)
			{
				keyParts = splitEventKey(key);

				if (int.TryParse(keyParts[1], out evtType))
				{
					if (evtType < 100 && evtType > 0) continue;

					eType = (ThemeCompareEventTypes) evtType;

					if (!eventTypeCounts.ContainsKey(eType))
					{
						eventTypeCounts.Add(eType, 1);
					}
					else
					{
						eventTypeCounts[eType]++;
					}

				}
			}
		}
	}

	private string fmtEventData(ThemeCompareEventTypes type, string?[]? evtData)
	{
		string fmt = "[ {0} ]";
		string desc = ThemeData.ThemeCompareEventInfo[type].Item2;

		if (evtData == null ||
			evtData.Length < 2||
			(evtData[0] == null && evtData[1] == null)) return string.Empty;

		// string s0 = evtData[0] == null ? "[ is null ]" : string.Format(fmt, evtData[0]);
		// string s1 = evtData[1] == null ? "[ is null ]" : string.Format(fmt, evtData[1]);

		string svs = evtData[0].IsVoid() || evtData[1].IsVoid() ? "    " : " vs ";

		string s0 = evtData[0].IsVoid() ? "" : string.Format(fmt, evtData[0]);
		string s1 = evtData[1].IsVoid() ? "" : string.Format(fmt, evtData[1]);

		return $"{desc,EVT_COL_WIDTH_A} | {s0,EVT_COL_WIDTH_B}{svs}{s1}";
	}

	private string[] splitEventKey(string key)
	{
		string[] parts = new string[3];

		parts[0] = $"{key[0..1]}"; // theme id
		parts[1] = $"{key[1..5]}"; // event type
		parts[2] = $"{key[6..11]}"; // index

		return parts;
	}

	private void showEventCounts()
	{
		ThemeCompareEventTypes[] eTypes = Enum.GetValues<ThemeCompareEventTypes>();

		string temp;
		string eTypeDesc;

		Debug.Write($"\n*** Theme Compare Event type List");

		if (verbose)
		{
			Debug.WriteLine($" | (all listed)\n");
		}
		else
		{
			Debug.WriteLine($" | (if zero, not listed)\n");
		}

		foreach (ThemeCompareEventTypes eType in eTypes)
		{
			eTypeDesc = ThemeData.ThemeCompareEventInfo[eType].Item1;

			if ((int) eType < 100 && (int) eType > 0) continue;

			temp = $"[ {eType} ]";

			if (eventTypeCounts.TryGetValue(eType, out int count))
			{
				Debug.WriteLine($"***\t{count, 5} in {temp, -30} | {eTypeDesc}");
			}
			else if (verbose)
			{
				Debug.WriteLine($"***\t{0, 5} in {temp, -30} | {eTypeDesc}");
			}
		}
	}

	private void showEventList()
	{
		Debug.WriteLine($"\n*** Compare of Themes | {themeA.Name} vs {themeB.Name}");

		Debug.WriteLine($"\n*** Theme Compare Event List | Count: {_EventCount}\n");

		if (_EventCount == 0)
		{
			Debug.WriteLine("*** -- no events -- ***");
			return;
		}

		string[] keyParts;
		string parts;
		string desc;
		string location;
		string comment;
		string? cd;
		string? ce;

		foreach ((string key, ThemeCompareEvent value) in compareEvents)
		{
			keyParts = splitEventKey(key);
			// parts = $"{keyParts[0],-6} {keyParts[1],-6} {keyParts[2],-6}";

			int evtType;

			if (int.TryParse(keyParts[1], out evtType))
			{
				if (evtType < 100 && evtType > 0)
				{
					if (evtType == (int) TE_TITLE_MARKER)
					{
						Debug.WriteLine($"+++> ********* {value.Comment} ********");
						continue;
					}

					if (evtType == (int) TE_HEADING_MARKER)
					{
						Debug.WriteLine($"+++>\t****** {value.Comment} *****");
						continue;
					}
				}
			}

			parts = $"[ {key[0..11]} ]";

			cd = value.CdName.IsVoid() ? "" : value.CdName;
			ce = value.CeName.IsVoid() ? "" : $" @ {value.CeName}";

			location = $"{cd}{ce}";

			desc = fmtEventData(value.Type, value.EventData);

			comment = value.Comment.IsVoid() ? string.Empty : $"| >> {value.Comment}";

			Debug.WriteLine($"**\t{value.Index,6} | {value.Type, -24} | {parts} | {location, -64} | {desc, 88} {comment}");

			// if (value.Index.Equals("10")) return;
		}
	}

}