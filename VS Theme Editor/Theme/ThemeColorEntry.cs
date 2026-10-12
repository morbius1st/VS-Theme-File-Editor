using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.VisualStudio.TextManager.Interop;
using static VS_Theme_Editor.ThemeColorSupport;

namespace VS_Theme_Editor;

public partial class ThemeColorEntry : ObservableObject, IComparable<ThemeColorEntry>
{
	[ObservableProperty]
    private string? name;

	// background

	[ObservableProperty]
	private ThemeColor tcBackground = new ThemeColor(0, 0);


	[ObservableProperty]
    private byte backgroundType;

	[ObservableProperty]
	private uint backgroundColorValue;

	[ObservableProperty]
	private int backgroundIndexValue;

	[ObservableProperty]
	private string background = string.Empty;

	// foreground

	[ObservableProperty]
	private ThemeColor tcForeground = new ThemeColor(0, 0);


	[ObservableProperty]
    private byte foregroundType;

	[ObservableProperty]
	private uint foregroundColorValue;

	[ObservableProperty]
	private int foregroundIndexValue;

    [ObservableProperty]
    private string foreground = string.Empty;

	// source

	[ObservableProperty]
	private ThemeData.ThemeSource themeSource;
	
	[ObservableProperty]
	private bool isEnabled;

	public string SourceDesc => ThemeData.GetThemeSourceDesc(ThemeSource);
	public string SourceFileName { get; init; } = string.Empty;


	// set colors

	public void SetForeground(byte type, uint colorValue, int colorIdx)
	{
		// ForegroundType = type;
		// ForegroundColorValue = colorValue;
		// ForegroundIndexValue = -1;
		TcForeground = colorIdx == -1 ? 
			new ThemeColor(type, colorValue) : new ThemeColor(type, colorIdx);
	}

	public void SetBackground(byte type, uint colorValue, int colorIdx)
	{
		// BackgroundType = type;
		// BackgroundColorValue = colorValue;
		// BackgroundIndexValue = -1;
		TcBackground = colorIdx == -1 ? 
			new ThemeColor(type, colorValue) : new ThemeColor(type, colorIdx);
	}

	// utilities

	public override string ToString()
	{
		return $"{Name} | source {SourceFileName} & {SourceDesc}";
	}

	// ReSharper disable once UnusedParameterInPartialMethod
	partial void OnThemeSourceChanged(ThemeData.ThemeSource value)
	{
		OnPropertyChanged(nameof(SourceDesc));
		OnPropertyChanged(nameof(SourceFileName));
	}

	public int CompareTo(ThemeColorEntry? other)
	{
		return String.Compare(Name, other?.Name, StringComparison.Ordinal);
	}

}


public partial class ThemeColor : ObservableObject
{
	public ThemeColor(byte type, uint colorValue)
	{
		colorType = (ThemeData.VsColorType) type;
		colorEx = new ColorEx(colorValue);
		colorIndex = -1;

		Init();
	}

	public ThemeColor(byte type, int colorIdx)
	{
		colorEx = new ColorEx(0, true);
		colorIndex = colorIdx;
		colorType = (ThemeData.VsColorType) type;

		Init();
	}

	[ObservableProperty]
	private ColorEx colorEx;

	[ObservableProperty]
	private int colorIndex;

	[ObservableProperty]
	private ThemeData.VsColorType colorType;

	[ObservableProperty]
	private bool gotColor;

	[ObservableProperty]
	private string colorValueString = string.Empty;

	[ObservableProperty]
	private string colorValueDesc = string.Empty;

	[ObservableProperty]
	private string colorValueToolTip = string.Empty;

	[ObservableProperty]
	private string colorSwatchToolTip = string.Empty;

	private void Init()
	{
		
		ColorValueToolTip = ThemeData.VsThemeTypeInfo[ColorType].Item2;
		ColorSwatchToolTip = ThemeData.VsThemeTypeInfo[ColorType].Item3;

		if (ColorType == ThemeData.VsColorType.CT_INVALID)
		{
			GotColor = false;
			ColorValueString = "None";
			
			return;
		}

		if (ColorType == ThemeData.VsColorType.CT_RAW)
		{
			GotColor = true;
			ColorValueString = ColorEx.ToHex;
			
			return;
		}

		if (ColorType == ThemeData.VsColorType.CT_COLORINDEX)
		{
			GotColor = false;

			ColorValueString = ColorIdxColors.Name((COLORINDEX) ColorIndex);
			ColorValueToolTip = string.Format(ColorValueToolTip, ColorIndex);
			ColorValueDesc = $"Index {ColorIndex}";
			return;
		}

		if (ColorType == ThemeData.VsColorType.CT_SYSCOLOR)
		{
			GotColor = true;

			string colorName;
			string colorValStr;
			uint color;

			if (!SystemColors.Translate(ColorIndex, out colorName, out color))
			{
				colorName = "Unknown";
				colorValStr = "Invalid";
			}
			else
			{
				ColorEx.SetColor(color, true);
				colorValStr = ColorEx.ToHex;
			}

			ColorValueString = colorName;
			ColorValueToolTip = string.Format(ColorValueToolTip, ColorIndex);
			ColorSwatchToolTip = string.Format(ColorSwatchToolTip, colorValStr);
			ColorValueDesc = $"Index {ColorIndex}";
			return;
		}

		if (ColorType == ThemeData.VsColorType.CT_AUTOMATIC)
		{
			GotColor = true;
			ColorValueString = "Automatic";
			ColorValueDesc  = ColorEx.ToHex;

			return;
		}

		// when 4, 6, or 7 - if ever
		ColorValueString = "None";
		ColorValueDesc = "Not Supported";

	}
}
