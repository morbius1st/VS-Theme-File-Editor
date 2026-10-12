// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             SystemColors.cs
// Created:      2026-09-30 (22:09)

using System.Diagnostics;
using DColor = System.Drawing.Color;
using MColor = System.Windows.Media.Color;
using Microsoft.VisualStudio.TextManager.Interop;

namespace VS_Theme_Editor;


// extensions

public static class ColorExtensions
{
	public static float GetHueHsl(this MColor color)
	{
		return (DColor.FromArgb(color.A, color.R, color.G, color.B)).GetHue();
	}

	public static float GetSaturationHsl(this MColor color)
	{
		return (DColor.FromArgb(color.A, color.R, color.G, color.B)).GetSaturation();
	}

	public static float GetLightnessHsl(this MColor color)
	{
		return (DColor.FromArgb(color.A, color.R, color.G, color.B)).GetBrightness();
	}

	public static DColor ToAltColor(this MColor mediaColor)
	{
		return DColor.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
	}

	public static MColor ToAltColor(this DColor dwgColor)
	{
		return MColor.FromArgb(dwgColor.A, dwgColor.R, dwgColor.G, dwgColor.B);
	}
}

public static class ThemeColorSupport
{
	public static DColor DConvert(uint color)
	{
		byte a = (byte)((color >> 24) & 0xFF);
		byte r = (byte)(color & 0xFF);
		byte g = (byte)((color >> 8) & 0xFF);
		byte b = (byte)((color >> 16) & 0xFF);

		return DColor.FromArgb(a, r, g, b);
	}

	public static MColor MConvert(uint color)
	{
		byte a = (byte)((color >> 24) & 0xFF);
		byte r = (byte)(color & 0xFF);
		byte g = (byte)((color >> 8) & 0xFF);
		byte b = (byte)((color >> 16) & 0xFF);

		return MColor.FromArgb(a, r, g, b);
	}

	// validate which color types to process
	// types 0, 4, 6, & 7 do not get processed
	// that is, read / write no additional bytes

	// valid color types are CT_RAW & CT_AUTOMATIC only
	// read as a color UInit32
	public static bool IsValidColorType(byte colorType)
	{
		return colorType == 1 || colorType >= 5;
		// return colorType is 1 or 5; 
	}

	// valid color types are CT_SYSCOLOR,CT_COLORINDEX, CT_VSCOLOR
	// read as an index = Init32
	public static bool IsValidIndexType(byte colorType)
	{
		return colorType == 2 || colorType == 3;
		// return colorType is 2 or 3; 
	}

	// ReSharper disable once MemberCanBePrivate.Global
	public static string FormatArgb(uint abgr)
	{
		string color = $"#{(abgr >> 24) & 0xFF:X2}{abgr & 0xFF:X2}{(abgr >> 8) & 0xFF:X2}{(abgr >> 16) & 0xFF:X2}";

		return color;
	}
}

public static class ColorIdxColors
{
	public static string Name(COLORINDEX ci)
	{
		string? name = Enum.GetName(typeof(COLORINDEX), ci).Substring(3).ToLower();
		name = char.ToUpper(name[0]) + name.Substring(1);
		return name;
	}

	public static readonly Dictionary<int, string> ColorIdxNames = new ()
	{
		{ 0, "The foreground color for user text" },
		{ 1, "Obsolete. Use USERTEXT_BK instead" },
		{ 1, "The background color for user text" },
		{ 2, "Black" },
		{ 2, "The first fixed color. Beginning of fixed color range" },
		{ 3, "White" },
		{ 4, "Maroon" },
		{ 5, "Dark green" },
		{ 6, "Brown" },
		{ 7, "Dark blue" },
		{ 8, "Purple" },
		{ 9, "Aquamarine" },
		{ 10, "Light gray" },
		{ 11, "Dark gray" },
		{ 12, "Red" },
		{ 13, "Green" },
		{ 14, "Yellow" },
		{ 15, "Blue" },
		{ 16, "Magenta" },
		{ 17, "Cyan" },
		{ 17, "The last fixed color. End of fixed color range" },
		{ 18, "The foreground for selected text" },
		{ 19, "The background for selected text" },
		{ 20, "The foreground for inactive selection" },
		{ 21, "The background for inactive selection" },
	};
}

public static class SystemColors
{
	public static bool Translate(int idx, out string colorName, out uint color)
	{
		color = uint.MaxValue;

		if (idx < 0 || idx > 30)
		{
			colorName = "Invalid";
			
			return false;
		}

		colorName = SysColorNames[idx];

		color = ApiUtilities.GetSysColor(idx);

		return true;
	}


	public static readonly Dictionary<int, string> SysColorNames = new ()
	{
		{ 0,  "ScrollBar-N/S" },					// matches  // win 10+ - not supported
		{ 1,  "Background-N/S" },					// matches  // win 10+ - not supported
		{ 2,  "ActiveCaption-N/S" },				// matches  // win 10+ - not supported
		{ 3,  "InactiveCaption-N/S" },				// matches  // win 10+ - not supported
		{ 4,  "Menu-N/S" },							// matches  // win 10+ - not supported
		{ 5,  "WindowBackground" },					// matches
		{ 6,  "WindowFrame-N/S" },					// matches  // win 10+ - not supported
		{ 7,  "MenuText-N/S" },						// matches  // win 10+ - not supported
		{ 8,  "WindowText" },						// matches
		{ 9,  "ActiveCaptionText-N/S" },			// matches  // win 10+ - not supported
		{ 10, "ActiveBorder-N/S" },					// matches  // win 10+ - not supported
		{ 11, "InactiveBorder-N/S" },				// matches  // win 10+ - not supported
		{ 12, "AppWorkspace-N/S" },					// matches  // win 10+ - not supported
		{ 13, "Highlight" },						// matches
		{ 14, "HighlightText" },					// matches
		{ 15, "ButtonFace" },						// matches
		{ 16, "ButtonShadow-N/S" },					// matches  // win 10+ - not supported
		{ 17, "GrayText" },							// matches
		{ 18, "ButtonText" },						// matches
		{ 19, "InactiveCaptionText-N/S" },			// matches  // win 10+ - not supported
		{ 20, "ControlLightLight-N/S" },			// matches  // win 10+ - not supported
		{ 21, "ControlDarkDark-N/S" },				// matches  // win 10+ - not supported
		{ 22, "ControlLight-N/S" },					// matches  // win 10+ - not supported
		{ 23, "InfoText-N/S" },						// matches  // win 10+ - not supported
		{ 24, "InfoBackground-N/S" },				// matches  // win 10+ - not supported
		{ 25, "25-N/A" },							// matches  // win 10+ - not supported
		{ 26, "HotTrack" },							// matches
		{ 27, "GradientActiveCaption-N/S" },		// matches  // win 10+ - not supported
		{ 28, "GradientInactiveCaption-N/S" },		// matches  // win 10+ - not supported
		{ 29, "MenuHighligh-N/St" },				// matches  // win 10+ - not supported
		{ 30, "MenuBar-N/S" },						// matches  // win 10+ - not supported
	};
}