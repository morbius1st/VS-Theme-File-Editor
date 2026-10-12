using static VS_Theme_Editor.ThemeData.ThemeSource;
using static VS_Theme_Editor.ThemeData.ThemeStyle;
using static VS_Theme_Editor.ThemeData.ThemeCompareEventTypes;
using static VS_Theme_Editor.ThemeData.ThemeCompareTypes;
using static VS_Theme_Editor.ThemeData.VsColorType;

// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             ThemeData.cs
// Created:      2026-08-02 (21:08)

namespace VS_Theme_Editor;

public class ThemeData
{
	/*
		info found here: https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.interop.__vscolortype?view=visualstudiosdk-2022
		
		CT_INVALID	0
		Color value is invalid.

		CT_RAW	1
		The color value is a standard 32 bit RGB color value.

		CT_COLORINDEX	2
		The color value is a member of the enumeration.

		CT_SYSCOLOR	= 3	,
		The color value is one used with GetSyscolor.

		CT_VSCOLOR	= 4	, 
		this is not used / do not use
		The color value is a member of the __VSSYSCOLOREX enumeration and can be used with GetVSSysColorEx(Int32, UInt32).

		CT_AUTOMATIC	= 5	,
		The color tracks the Visual Studio session's automatic color.

		CT_TRACK_FOREGROUND	= 6,
		The color tracks the Windows session's foreground color.

		CT_TRACK_BACKGROUND	= 7	,
		The color tracks the Windows session's background color.
	*/

	public enum VsColorType : byte
	{
		CT_INVALID = 0,
		CT_RAW = 1,
		CT_COLORINDEX	= 2,
		CT_SYSCOLOR	= 3	,
		CT_VSCOLOR	= 4	,
		CT_AUTOMATIC	= 5	,
		CT_TRACK_FOREGROUND	= 6,
		CT_TRACK_BACKGROUND	= 7	,
	}

	//                                                         swatch tool tip
	//                                                  value tool tip
	//                                          gen desc
	public static Dictionary<VsColorType, Tuple<string, string, string>> VsThemeTypeInfo = new ()
	{
		{ CT_INVALID         , new ("Value is not used"                                       , $"{CT_INVALID} {$"({((int) CT_INVALID):N0})", 5} - Is Not Used"							                 , $"{CT_INVALID} {$"({((int) CT_INVALID):N0})", 5} - Is Not Used") },
		{ CT_RAW             , new ("Value is a standard 32 bit ARGB color value"             , $"{CT_RAW} {$"({((int) CT_RAW):N0})", 5} - Color is {{ ARGB}}"							                 , $"{CT_RAW} {$"({((int) CT_RAW):N0})", 5} - Standard Color Value Type") },
		{ CT_COLORINDEX      , new ("DO NOT USE"                                              , $"{CT_COLORINDEX} {$"({((int) CT_COLORINDEX):N0})", 5} - Index {{0:N0}} - DO NOT USE"				     , $"{CT_COLORINDEX} {$"({((int) CT_COLORINDEX):N0})", 5} - Member of the Enumeration / n/a outside of VS") },
		{ CT_SYSCOLOR        , new ("Value is one used with GetSyscolor"                      , $"{CT_SYSCOLOR} {$"({((int) CT_SYSCOLOR):N0})", 5} - Index {{0:N0}} - Suffix of N/S notes 'Not Supported'", $"{CT_SYSCOLOR} {$"({((int) CT_SYSCOLOR):N0})", 5} - [ {{0}} ] - use GetSysColor Api") },
		{ CT_VSCOLOR         , new ("Value is a Visual Studio color from __VSSYSCOLOREX"      , $"{CT_VSCOLOR} {$"({((int) CT_VSCOLOR):N0})", 5} - member of the __VSSYSCOLOREX enumeration"             , $"{CT_VSCOLOR} {$"({((int) CT_VSCOLOR):N0})", 5} - used with GetVSSysColorEx") },
		{ CT_AUTOMATIC       , new ("Value tracks the Visual Studio session's automatic color", $"{CT_AUTOMATIC} {$"({((int) CT_AUTOMATIC):N0})", 5} - tracks the Visual Studio session's automatic color", $"{CT_AUTOMATIC} {$"({((int) CT_AUTOMATIC):N0})", 5} - Typically Transparent") },
		{ CT_TRACK_FOREGROUND, new ("DO NOT USE"                                              , $"{CT_TRACK_FOREGROUND} {$"({((int) CT_TRACK_FOREGROUND):N0})", 5} -DO NOT USE - Not Supported"	         , $"{CT_TRACK_FOREGROUND} {$"({((int) CT_TRACK_FOREGROUND):N0})", 5} - DO NOT USE") },
		{ CT_TRACK_BACKGROUND, new ("DO NOT USE"                                              , $"{CT_TRACK_BACKGROUND} {$"({((int) CT_TRACK_BACKGROUND):N0})", 5} - DO NOT USE - Not Supported"         , $"{CT_TRACK_BACKGROUND} {$"({((int) CT_TRACK_BACKGROUND):N0})", 5} - DO NOT USE") },
	};

	public const string THEME_EXTENSION = "pkgdef";

	public enum ThemeCompareTypes
	{
		TCT_NONE,
		TCT_BASIC_1,    // basic compare - just check for mismatched data & missing / extra CD's
		TCT_EXTENDED_2, // extended compare - also check for missing / extra CE's
		TCT_EXTENDED_3, // extended compare - also check for mismatched CE data
	}

	public static Dictionary<ThemeCompareTypes, Tuple<string, string>> ThemeCompareTypeInfo = new ()
	{
		{ TCT_NONE       , new ("N/A - Missing"      , "[N/A - Error-Do Not Use]" ) },
		{ TCT_BASIC_1    , new ("Basic Compare"      , "[Compare mismatch data and missing or extra CD's]" ) },
		{ TCT_EXTENDED_2 , new ("Extended Compare 1" , "[Extended 1 = Basic compare + missing for extra CE's]" ) },
		{ TCT_EXTENDED_3 , new ("Extended Compare 2" , "[Extended 2 = Extended 1 + mismatched CE data]" ) },
	};

	public enum ThemeCompareEventTypes
	{
		// positive events < 100 - markers
		TE_TITLE_MARKER	       = 1,
		TE_HEADING_MARKER	   = 2,

		// negative events = fails
		TE_THEME_NULL_A        = -101,
		TE_THEME_NULL_B        = -102 ,

		TE_STR_VOID_A          = -111,
		TE_STR_VOID_B          = -112,

		TE_CD_EMPTY_A          = -121,
		TE_CD_EMPTY_B          = -122,

		TE_MISMATCH_STR        = -131,
		TE_MISMATCH_DATA       = -132,
		TE_MISMATCH_COUNT_MORE = -133,
		TE_MISMATCH_COUNT_LESS = -134,

		TE_INVALID_DATA        = -139,

		TE_CD_NOT_IN_A         = -141,
		TE_CD_NOT_IN_B         = -142,

		TE_CE_NOT_IN_A         = -151,
		TE_CE_NOT_IN_B         = -152,

		TE_FALLBACK_IMSMATCH   = -181,
		TE_THEME_GUID_MISMATCH = -182,

		// level 2 tests
		TE_CD_GUID_MATCH       = -201,

		TE_CE_FG_MISMATCH      = -202,
		TE_CE_BG_MISMATCH      = -203,
		TE_CE_FG_TYPE_MISMATCH = -204,
		TE_CE_BG_TYPE_MISMATCH = -205,

		TE_CE_FG_TYPE_NOT_VALID  = -214,
		TE_CE_BG_TYPE_NOT_VALID  = -215,

		// positive events = passes

		TE_STR_GOOD_A		   = 111,
		TE_STR_GOOD_B		   = 112,

		TE_MATCH_STR           = 131,
		TE_MATCH_DATA		   = 132,
		TE_MATCH_COUNT		   = 133,

		TE_VALID_DATA		   = 139,

		TE_CD_FOUND_IN_B       = 142,

		TE_CE_FOUND_IN_A       = 151,
		TE_CE_FOUND_IN_B       = 152,

		TE_CE_FG_MATCH         = 202,
		TE_CE_BG_MATCH         = 203,
		TE_CE_FG_TYPE_MATCH    = 204,
		TE_CE_BG_TYPE_MATCH    = 205,

		TE_CE_FG_TYPE_IS_INVALID  = 216,
		TE_CE_BG_TYPE_IS_INVALID  = 217,
	}

	public static Dictionary<ThemeCompareEventTypes, Tuple<string, string>> ThemeCompareEventInfo = new ()
	{
		{ TE_TITLE_MARKER           , new ("Title Marker"                , "[Title Placeholder]" ) },
		{ TE_HEADING_MARKER         , new ("Heading Marker"              , "[Heading Placeholder]" ) },

		{ TE_THEME_NULL_A           , new ("Theme A is null"             , "[Theme A NUll]" ) },
		{ TE_THEME_NULL_B           , new ("Theme B is null"             , "[Theme B NUll]" ) },
		{ TE_STR_VOID_A             , new ("String for A is void"        , "[String A Void]" ) },
		{ TE_STR_VOID_B             , new ("String for B is void"        , "[String B Void]" ) },
		{ TE_CD_EMPTY_A             , new ("CD for A is empty"           , "[CD A Empty]" ) },
		{ TE_CD_EMPTY_B             , new ("CD for B is empty"           , "[CD B Empty]" ) },
		{ TE_MISMATCH_STR           , new ("Mismatch String"             , "[MisMatch]" ) },
		{ TE_MISMATCH_DATA          , new ("Mismatch Data"               , "[MisMatch]"   ) },
		{ TE_MISMATCH_COUNT_MORE    , new ("Mismatch Count More"         , "[Counts do not match (has more)]") },
		{ TE_MISMATCH_COUNT_LESS    , new ("Mismatch Count Less"         , "[Counts do not match (has less)]") },
		{ TE_INVALID_DATA           , new ("Invalid Data"                , "[Invalid]"   ) },
		{ TE_CD_NOT_IN_A            , new ("CD Not in Theme A"           , "[CD Not in Theme A]"  ) },
		{ TE_CD_NOT_IN_B            , new ("CD Not in Theme B"           , "[CD Not in Theme B]"  ) },
		{ TE_CE_NOT_IN_A            , new ("CE Not in Theme A"           , "[CE Not in Theme A]"  ) },
		{ TE_CE_NOT_IN_B            , new ("CE Not in Theme B"           , "[CE Not in Theme B]"  ) },
		{ TE_FALLBACK_IMSMATCH      , new ("Fallback Mismatch"           , "[Fallback Mismatch]"  ) },
		{ TE_THEME_GUID_MISMATCH    , new ("Theme GUID Mismatch"         , "[Theme GUID Mismatch]"  ) },
		{ TE_CD_GUID_MATCH          , new ("CD GUID mismatch"            , "[CD GUID mismatch]"  ) },
		{ TE_CE_BG_MISMATCH         , new ("CE Background Mismatch"      , "[CE Background Mismatch]"  ) },
		{ TE_CE_FG_MISMATCH         , new ("CE Foreground Mismatch"      , "[CE Foreground Mismatch]"  ) },
		{ TE_CE_FG_TYPE_MISMATCH    , new ("CE Foreground Type Mismatch" , "[CE Foreground Type Mismatch]"  ) },
		{ TE_CE_BG_TYPE_MISMATCH    , new ("CE Background Type Mismatch" , "[CE Background Type Mismatch]"  ) },

		{ TE_CE_FG_TYPE_NOT_VALID   , new ("CE Foreground Type Not Valid", "[CE Foreground Type Not Valid]"  ) },
		{ TE_CE_BG_TYPE_NOT_VALID   , new ("CE Background Type Not Valid", "[CE Background Type Not Valid]"  ) },


		{ TE_STR_GOOD_A             , new ("String A good"               , "[String for A is good]"  ) },
		{ TE_STR_GOOD_B             , new ("String B good"               , "[String for B is good]"  ) },
		{ TE_MATCH_STR              , new ("Match String"                , "[Match String]"  ) },
		{ TE_MATCH_DATA             , new ("Match Data"                  , "[Match Data]"  ) },
		{ TE_MATCH_COUNT            , new ("Match Count"                 , "[Counts match]"  ) },
		{ TE_VALID_DATA             , new ("Valid Data"                  , "[Valid Data]"  ) },
		{ TE_CD_FOUND_IN_B          , new ("CD Found in Theme B"         , "[The CD found in Theme B]"  ) },
		{ TE_CE_FOUND_IN_A          , new ("CE Found in Theme A"         , "[The CE found in Theme A]"  ) },
		{ TE_CE_FOUND_IN_B          , new ("CE Found in Theme B"         , "[The CE found in Theme B]"  ) },

		{ TE_CE_BG_MATCH            , new ("CE Background Match"         , "[CE Background Match]"  ) },
		{ TE_CE_FG_MATCH            , new ("CE Foreground Match"         , "[CE Foreground Match]"  ) },
		{ TE_CE_FG_TYPE_MATCH       , new ("CE Foreground Type Match"    , "[CE Foreground Type Match]"  ) },
		{ TE_CE_BG_TYPE_MATCH       , new ("CE Background Type Match"    , "[CE Background Type Match]"  ) },

		{ TE_CE_FG_TYPE_IS_INVALID     , new ("CE Fg Type Is Invalid"    , "[CE Fg Type Is Invalid]"  ) },
		{ TE_CE_BG_TYPE_IS_INVALID     , new ("CE Bg Type Is Invalid"    , "[CE Bg Type Is Invalid]"  ) },
	};

	/// <summary>
	/// light, dark, and high contrast, etc.
	/// </summary>
	public enum ThemeStyle
	{
		TSTY_UKN,
		TSTY_NA,
		TSTY_TBD,
		TSTY_LIGHT, // y
		TSTY_DARK,  // y + 1
		TSTY_HC,    // y + 2
		// TSTY_BLUE,		// y + 3
		// TSTY_BLUE_EC,   // y + 4
		// TSTY_LIGHT_EC,  // y + 5
		// TSTY_DARK_EC,   // y + 6
	}

	public const string TSD_LIGHT = "Light";
	public const string TSD_DARK = "Dark";

	public const string TSD_HC = "High Contrast";
	// public const string TSD_BLUE = "Blue";
	// public const string TSD_BLUE_EC = "Blue Extra Contrast";
	// public const string TSD_LIGHT_EC = "Light Extra Contrast";
	// public const string TSD_DARK_EC = "Dark Extra Contrast";

	public static Dictionary<ThemeStyle, Tuple<string>> ThemeStyleInfo = new ()
	{
		{ TSTY_UKN       , new ("Unknown") },
		{ TSTY_TBD       , new ("To be determined") },
		{ TSTY_NA        , new ("Does not matter (NA)") },
		{ TSTY_LIGHT     , new (TSD_LIGHT) },
		{ TSTY_DARK      , new (TSD_DARK) },
		{ TSTY_HC        , new (TSD_HC) },
		// { TSTY_BLUE      , new (TSD_BLUE) },
		// { TSTY_BLUE_EC   , new (TSD_BLUE_EC) },
		// { TSTY_LIGHT_EC  , new (TSD_LIGHT_EC) },
		// { TSTY_DARK_EC   , new (TSD_DARK_EC) },
	};

	public static string GetThemeStyleDesc(ThemeStyle style) => ThemeStyleInfo[style].Item1;

	public enum ThemeSource
	{
		TSRC_NONE,
		TSRC_UKN,
		TSRC_TBD,
		TSRC_USER_TBD_FALLBACK,
		TSRC_USER_NO_FALLBACK,
		TSRC_USER_WITH_FALLBACK,
		TSRC_VS_BUILTIN,
		TSRC_VS_OVERLAY,
		TSRC_VS_THEME,	// x
		// TSRC_VS_SHELL,
		// TSRC_EDC,
		// TSRC_EDC_EC,
	}

	// for users                              one line  two lines
	public static Dictionary<ThemeSource, Tuple<string, string>> ThemeSourceInfo = new ()
	{
		{ TSRC_NONE                , new ("Undetermined"                     , "Undetermined"       ) },
		{ TSRC_UKN                 , new ("Unknown"                          , "Unknown"            ) },
		{ TSRC_TBD                 , new ("To be determined"                 , "To be\nDetermined"  ) },
		{ TSRC_USER_TBD_FALLBACK   , new ("User Theme"                       , "User Theme"         ) },
		{ TSRC_USER_NO_FALLBACK    , new ("User Theme"                       , "User Theme"         ) },
		{ TSRC_USER_WITH_FALLBACK  , new ("User Theme"                       , "User Theme"         ) },
		{ TSRC_VS_BUILTIN          , new ("Vs Built-In Theme"                , "Vs Built-In\nTheme" ) },
		{ TSRC_VS_OVERLAY          , new ("Vs Overlay Theme"                 , "Vs Overlay\nTheme"  ) },
		{ TSRC_VS_THEME            , new ("Vs Root Theme"                    , "Vs Root\nTheme"     ) },
		// { TSRC_VS_SHELL            , new ("Vs Root Shell"                    , "Vs Root\nShell"     ) },
		// { TSRC_EDC                 , new ("Vs Editor Colors"                 , "Vs Editor\nColors"  ) },
		// { TSRC_EDC_EC              , new ("Vs Editor Colors, Extra Contrast" , "Vs Editor Colors\nExtra Construct"   ) },
	};

	public static string GetThemeSourceDesc(ThemeSource source) => ThemeSourceInfo[source].Item1;
}