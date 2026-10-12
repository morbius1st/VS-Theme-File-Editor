using System.Diagnostics;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Extensions;
using MColor = System.Windows.Media.Color;
using DColor = System.Drawing.Color;



// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             ColorEx.cs
// Created:      2026-08-23 (22:08)

namespace VS_Theme_Editor;

public partial class ColorEx : ObservableObject
{

	public ColorEx(uint colorValue, bool nonAlpha = false)
	{
		SetColor(colorValue);
		NonAlphaColor = nonAlpha;
	}
	

	public ColorEx(MColor colorValue)
	{
		ColorValue = colorValue;
	}

	public ColorEx(DColor colorValue)
	{
		ColorValue = MColor.FromArgb(colorValue.A, colorValue.R, colorValue.G, colorValue.B);
	}

	private float hue;
	private float lightness;
	private float saturation;

	private byte grayB;

	/// <summary>
	/// Color as ARGB value
	/// </summary>
	[ObservableProperty]
	private MColor colorValue;

	public bool NonAlphaColor { get; set; }

	public float Hue
	{
		get => hue;
		set
		{
			if (value.Equals(hue)) return;
			hue = value;

			OnPropertyChanged();

			UpdatePerHsl();
			UpdateColorValues();
		}
	}				// hsl - 0.0 to 360.0  (red to red)

	public float Saturation
	{
		get => saturation;
		set
		{
			if (value.Equals(saturation)) return;
			saturation = value;
			OnPropertyChanged();
			
			UpdatePerHsl();
			UpdateColorValues();
		}
	}		// hsl - 0.0 to 1.0 (gray to color)

	public float Lightness
	{
		get => lightness;
		set
		{
			if (value.Equals(lightness)) return;
			lightness = value;
			OnPropertyChanged();

			UpdatePerHsl();
			UpdateGray();
			UpdateColorValues();

		}
	}		// hsl - 0.0 (black) to 1.0 (white)

	/// <summary>
	/// just tracks the current grayB setting
	/// </summary>
	// ReSharper disable once MemberCanBePrivate.Global
	public byte GrayB
	{
		get => grayB;
		set
		{
			if (value == grayB) return;
			grayB = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(GrayChangeB));
		}
	} 

	/// <summary>
	/// updates the colorvalue
	/// </summary>
	public byte GrayChangeB
	{
		get => grayB;
		set
		{
			if (value == grayB) return;

			UpdatePerGray(value);
			OnPropertyChanged();
		}
	}

	public byte AlphaB
	{
		get => ColorValue.A;
		set
		{
			if (value == ColorValue.A) return;
			#pragma warning disable MVVMTK0034
			colorValue.A = value;
			#pragma warning restore MVVMTK0034
			OnPropertyChanged();
			UpdateColorValue();
		}
	}

	public byte RedB
	{
		get => ColorValue.R;
		set
		{
			if (value == ColorValue.R) return;
			#pragma warning disable MVVMTK0034
			colorValue.R = value;
			#pragma warning restore MVVMTK0034	
			OnPropertyChanged();
			UpdateColorValue();
		}
	}

	public byte GreenB
	{
		get => ColorValue.G;
		set
		{
			if (value == ColorValue.G) return;
			#pragma warning disable MVVMTK0034
			colorValue.G = value;
			#pragma warning restore MVVMTK0034	
			OnPropertyChanged();
			UpdateColorValue();
		}
	}

	public byte BlueB
	{
		get => ColorValue.B;
		set
		{
			if (value == ColorValue.B) return;
			#pragma warning disable MVVMTK0034
			colorValue.B = value;
			#pragma warning restore MVVMTK0034	
			OnPropertyChanged();
			UpdateColorValue();
		}
	}


	public string GrayH
	{
		get => GrayB.ToString("X2");
		set
		{
			GrayB = byte.Parse(value);
			OnPropertyChanged();
		}
	}

	public string AlphaH
	{
		get => AlphaB.ToString("X2");
		set
		{
			AlphaB = byte.Parse(value);
			OnPropertyChanged();
			OnPropertyChanged(nameof(ToHex));
		}
	}

	public string RedH
	{
		get => RedB.ToString("X2");
		set
		{
			RedB = byte.Parse(value);
			OnPropertyChanged();
			OnPropertyChanged(nameof(ToHex));
		}
	}

	public string GreenH
	{
		get => GreenB.ToString("X2");
		set
		{
			GreenB = byte.Parse(value);
			OnPropertyChanged();
			OnPropertyChanged(nameof(ToHex));
		}
	}

	public string BlueH
	{
		get => BlueB.ToString("X2");
		set
		{
			BlueB = byte.Parse(value);
			OnPropertyChanged();
			OnPropertyChanged(nameof(ToHex));
		}
	}


	public SolidColorBrush ToSolidColorBrush
	{
		get
		{
			Color c = ColorValue;

			if (NonAlphaColor)
			{
				c.A = 255;
			}

			return c.ToBrush();
		}
	}

	/// <summary>
	/// set alpha via a string value
	/// </summary>
	public string Alpha
	{
		set
		{
			if (byte.TryParse(value, out byte aByte))
			{
				ColorValue = MColor.FromArgb(aByte, ColorValue.R, ColorValue.G, ColorValue.B);
			}
		}
	}

	/// <summary>
	/// set red via a string value
	/// </summary>
	public string Red
	{
		set
		{
			if (byte.TryParse(value, out byte rByte))
			{
				ColorValue = MColor.FromArgb(ColorValue.A, rByte, ColorValue.G, ColorValue.B);
			}
		}
	}

	/// <summary>
	/// set green via a string value
	/// </summary>
	public string Green
	{
		set
		{
			if (byte.TryParse(value, out byte gByte))
			{
				ColorValue = MColor.FromArgb(ColorValue.A, ColorValue.R, gByte, ColorValue.B);
			}
		}
	}

	/// <summary>
	/// set blue via a string value
	/// </summary>
	public string Blue	
	{
		set
		{
			if (byte.TryParse(value, out byte bByte))
			{
				ColorValue = MColor.FromArgb(ColorValue.A, ColorValue.R, ColorValue.G, bByte);
			}
		}
	}

	public string GrayD
	{
		get => grayB.ToString("##0");
		set
		{
			ColorValue.UpdateLuminance((float.Parse(value))/255f);
			OnPropertyChanged();
		}
	}

	// ReSharper disable once MemberCanBePrivate.Global
	public string AlphaD => ColorValue.A.ToString("##0");

	// ReSharper disable once MemberCanBePrivate.Global
	public string RedD => ColorValue.R.ToString("##0");

	// ReSharper disable once MemberCanBePrivate.Global
	public string GreenD => ColorValue.G.ToString("##0");

	// ReSharper disable once MemberCanBePrivate.Global
	public string BlueD => ColorValue.B.ToString("##0");

	// convert the color value to a hex string x8, e.g. #FFFFFFFF
	public string ToHex
	{
		get
		{
			if (NonAlphaColor) return $"#{RedH}{GreenH}{BlueH}";

			return $"#{AlphaH}{RedH}{GreenH}{BlueH}";
		}
	}

	// convert the color value to a decimal string, e.g. 255, 255, 255, 255
	public string ToDec => $"{AlphaD}, {RedD}, {GreenD}, {BlueD}";


	// re-set the color

	// ReSharper disable once MemberCanBePrivate.Global
	public void SetColor(short a, short r, short g, short b)
	{
		byte aa = (byte) (a >= 0 ? a : ColorValue.A);
		byte rr = (byte) (r >= 0 ? r : ColorValue.R);
		byte gg = (byte) (g >= 0 ? g : ColorValue.G);
		byte bb = (byte) (b >= 0 ? b : ColorValue.B);

		ColorValue = MColor.FromArgb(aa, rr, gg, bb);

		OnColorSet();
	}

	// ReSharper disable once MemberCanBePrivate.Global
	public void SetColor(uint uColor, bool nonAlpha = false)
	{
		NonAlphaColor = nonAlpha;
		ColorValue = ThemeColorSupport.MConvert(uColor);
	}

	// ReSharper disable once MemberCanBePrivate.Global
	public void SetColor(SolidColorBrush scb)
	{
		SetColor(scb.Color);
	}

	// ReSharper disable once MemberCanBePrivate.Global
	public void SetColor(MColor c)
	{
		ColorValue = c;
		// ColorValue = Color.FromArgb(c.A, c.R, c.G, c.B);

		OnColorSet();
	}

	// ReSharper disable once MemberCanBePrivate.Global
	public void SetColor(DColor c)
	{
		MColor cx = ToColor(c);
		ColorValue = cx;
		OnColorSet();
	}

	// ReSharper disable once MemberCanBePrivate.Global
	public void SetColor(double a, double h, double s, double l)
	{
		ColorValue = FromHsl((byte) a, h, s, l);
		OnColorSet();
	}


	// update color properties

	public void UpdateColorValue()
	{
		OnPropertyChanged(nameof(ColorValue));
		OnPropertyChanged(nameof(ToSolidColorBrush));
	}

	public void UpdateColorValues()
	{
		OnPropertyChanged(nameof(AlphaB));
		OnPropertyChanged(nameof(RedB));
		OnPropertyChanged(nameof(GreenB));
		OnPropertyChanged(nameof(BlueB));

		UpdateColorValue();
	}

	public void UpdateGray()
	{
		GrayB = (byte) (256f * lightness);
	}

	public void UpdateHslValues()
	{
		hue = ColorValue.GetHueHsl();
		saturation = ColorValue.GetSaturationHsl();
		lightness = ColorValue.GetLightnessHsl();
	}

	public void UpdatePerHsl()
	{
		ColorValue =  FromHsl(AlphaB, Hue, Saturation, Lightness);
	}

	public void UpdatePerGray(byte gray)
	{
		grayB = gray;

		ColorValue = MColor.FromArgb(AlphaB, gray, gray, gray);

		UpdateHslValues();
		UpdateColorValues();
	}


	// utilities

	public static MColor ToColor(DColor c)
	{
		// Color cx = Color.FromArgb(c.A, c.R, c.G, c.B);

		return MColor.FromArgb(c.A, c.R, c.G, c.B);
	}

	public static SolidColorBrush ToScb(DColor c)
	{
		return new SolidColorBrush(ToColor(c));
	}

	public void OnColorSet()
	{
		UpdateHslValues();
		UpdateGray();
		UpdateColorValues();
	}

	public static MColor FromHsl(byte a, double hue, double saturation, double lightness)
	{
		double c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
		double x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
		double m = lightness - c / 2;
		double r = 0, g = 0, b = 0;

		if (0 <= hue && hue < 60)
		{
			r = c; g = x; b = 0;
		}
		else if (60 <= hue && hue < 120)
		{
			r = x; g = c; b = 0;
		}
		else if (120 <= hue && hue < 180)
		{
			r = 0; g = c; b = x;
		}
		else if (180 <= hue && hue < 240)
		{
			r = 0; g = x; b = c;
		}
		else if (240 <= hue && hue < 300)
		{
			r = x; g = 0; b = c;
		}
		else if (300 <= hue && hue < 360)
		{
			r = c; g = 0; b = x;
		}

		byte red = (byte)((r + m) * 255);
		byte green = (byte)((g + m) * 255);
		byte blue = (byte)((b + m) * 255);

		return MColor.FromArgb(a, red, green, blue);
	}

	/*
	public static System.Windows.Media.Color FromHsl2(byte a, double hue, double saturation, double lightness)
	{
		(int R, int G, int B) rgb = HslToRgb(hue, saturation, lightness);
		return System.Windows.Media.Color.FromArgb(a, (byte) rgb.R, (byte) rgb.G, (byte) rgb.B);
	}

	/// <summary>
	/// Converts HSL color values to RGB.
	/// </summary>
	/// <param name="h">Hue in degrees [0, 360]</param>
	/// <param name="s">Saturation percentage [0, 1]</param>
	/// <param name="l">Lightness percentage [0, 1]</param>
	/// <returns>A tuple containing (R, G, B) values from 0 to 255</returns>
	public static (int R, int G, int B) HslToRgb(double h, double s, double l)
	{
		// Normalize input values to ranges of 0-1
		h /= 360.0;
		// s /= 100.0;
		// l /= 100.0;

		double r = l;
		double g = l;
		double b = l;

		// If there is saturation, calculate the distinct channels
		if (s != 0)
		{
			double q = l < 0.5 ? l * (1.0 + s) : l + s - (l * s); //
			double p = 2.0 * l - q;

			r = HueToRgb(p, q, h + 1.0 / 3.0);
			g = HueToRgb(p, q, h);
			b = HueToRgb(p, q, h - 1.0 / 3.0);
		}

		// Convert final values back to the 0-255 scale
		return (
			(int)Math.Round(r * 255.0),
			(int)Math.Round(g * 255.0),
			(int)Math.Round(b * 255.0)
			);
	}

	private static double HueToRgb(double p, double q, double t)
	   {
	   	// Correct for color wheel wrap-around
	   	if (t < 0.0) t += 1.0;
	   	if (t > 1.0) t -= 1.0;

	   	// Perform linear interpolation based on color sector
	   	if (t < 1.0 / 6.0) return p + (q - p) * 6.0 * t;
	   	if (t < 1.0 / 2.0) return q;
	   	if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6.0;

	   	return p;
	   }



	   // public void UpdateColorValues2()
	   // {
	   // 	OnPropertyChanged(nameof(GrayB));
	   // 	OnPropertyChanged(nameof(AlphaB));
	   // 	OnPropertyChanged(nameof(RedB));
	   // 	OnPropertyChanged(nameof(GreenB));
	   // 	OnPropertyChanged(nameof(BlueB));
	   //
	   // 	OnPropertyChanged(nameof(GrayD));
	   // 	OnPropertyChanged(nameof(AlphaD));
	   // 	OnPropertyChanged(nameof(RedD));
	   // 	OnPropertyChanged(nameof(GreenD));
	   // 	OnPropertyChanged(nameof(BlueD));
	   //
	   // 	OnPropertyChanged(nameof(GrayH));
	   // 	OnPropertyChanged(nameof(AlphaH));
	   // 	OnPropertyChanged(nameof(RedH));
	   // 	OnPropertyChanged(nameof(GreenH));
	   // 	OnPropertyChanged(nameof(BlueH));
	   //
	   // 	OnPropertyChanged(nameof(ToHex));
	   // 	OnPropertyChanged(nameof(ToDec));
	   // }
	   //
	   // public void UpdateAssociatedValues2()
	   // {
	   // 	UpdateHsl2();
	   //
	   // 	grayB = (byte) (lightness * 255);
	   // }
	   //
	   // public void UpdateHsl2()
	   // {
	   // 	DColor c = DColor.FromArgb(ColorValue.A, ColorValue.R, ColorValue.G, ColorValue.B);
	   //
	   // 	lightness = c.GetBrightness();
	   // 	hue = c.GetHue();
	   // 	saturation = c.GetSaturation();
	   // }
	   //
	   // partial void OnColorValueChanged(Color oldValue, Color newValue)
	   // {
	   // 	UpdateAssociatedValues2();
	   //
	   // 	OnPropertyChanged(nameof(ToSolidColorBrush));
	   //
	   // 	UpdateColorValues2();
	   //
	   // }
	*/
}

