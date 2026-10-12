using System.Diagnostics;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using UtilityLibrary;
using static VS_Theme_Editor.ApiUtilities;
using Color = System.Windows.Media.Color;



// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             ColorChooserViewModel.cs
// Created:      2026-08-22 (11:08)

namespace VS_Theme_Editor;



public partial class ColorChooserViewModel : ObservableObject
{
	public ColorChooserViewModel(Color fg, Color bg)
	{
		CurrFg = new SolidColorBrush(fg);
		CurrBg = new SolidColorBrush(bg);

		WkgFg = new SolidColorBrush(fg);
		WkgBg = new SolidColorBrush(bg);

		SelFg = new ColorEx(fg);
		SelBg = new ColorEx(bg);

		updateSelFgColorNumbers();
		updateSelBgColorNumbers();

		SatOne = new ColorEx(fg);
		SatZero = new ColorEx(fg);

		LightOne = new ColorEx(fg);
		LightHalf = new ColorEx(fg);
		LighZero = new ColorEx(fg);	

		Debug.WriteLine("initialize");

		isForeground = false;
		isBgSelected = true;
		TmpCx = new ColorEx(bg);
		activateBgCx();

		isForeground = true;
		isBgSelected = false;
		TmpCx = new ColorEx(fg);
		activateFgCx();

		_apiUtilities = new ApiUtilities();
	}

	private readonly ApiUtilities _apiUtilities;

	[ObservableProperty]
	private ColorEx selFg;

	[ObservableProperty]
	private ColorEx selBg;

	[ObservableProperty]
	private SolidColorBrush currFg;

	[ObservableProperty]
	private SolidColorBrush currBg;

	[ObservableProperty]
	private SolidColorBrush wkgFg;

	[ObservableProperty]
	private SolidColorBrush wkgBg;

	// public ColorEx WkgFgCx { get; set; }
	// public ColorEx WkgBgCx { get; set; }

	public ColorEx TmpCx { get; set; }

	public ColorEx SatOne { get; set; }

	public ColorEx SatZero { get; set; }

	public ColorEx LightOne { get; set; }
	public ColorEx LightHalf { get; set; }
	public ColorEx LighZero { get; set; }

	[ObservableProperty]
	private SolidColorBrush? eyeDropFill;

	[ObservableProperty]
	private Cursor? eyeDropCursor;

	[ObservableProperty]
	private bool isMouseOverEyeDropBtn;

	[ObservableProperty]
	private bool isFgEyeDropActive;

	[ObservableProperty]
	private bool isBgEyeDropActive;

	[ObservableProperty]
	// ReSharper disable once MemberInitializerValueIgnored
	private bool isForeground = true;

	private bool isFgSelected;
	
	[ObservableProperty]
	private bool isBackground;

	private bool isBgSelected;

	[ObservableProperty]
	private float hueValue;

	[ObservableProperty]
	private float saturationValue;

	[ObservableProperty]
	private float lightnessValue;

	[ObservableProperty]
	private byte grayValue;

	[ObservableProperty]
	private string? tmpFgHex;
	
	[ObservableProperty]
	private string? tmpFgDec;
	
	[ObservableProperty]
	private string? tmpBgHex;
	
	[ObservableProperty]
	private string? tmpBgDec;


	[ObservableProperty]
	private string? selFgToolTip;

	[ObservableProperty]
	private string? selBgToolTip;


	[RelayCommand]
	public void SetForground()
	{
		IsForeground = true;
		IsBackground = false;
		activateFgCx();
	}

	[RelayCommand]
	public void SetBackground()
	{
		IsForeground = false;
		IsBackground = true;
		activateBgCx();
	}

	[RelayCommand]
	public void ApplyFg()
	{
		SelFg.SetColor(WkgFg.Color);
		updateSelFgColorNumbers();
		OnPropertyChanged(nameof(SelFg));
	}

	[RelayCommand]
	public void ApplyBg()
	{
		SelBg.SetColor(WkgBg.Color);
		updateSelBgColorNumbers();
		OnPropertyChanged(nameof(SelBg));
	}

	/// <summary>
	/// update the color of the working rectangles.  this is called for the
	/// mouse move and mouse down events on the color chooser box - it gets updated
	/// many times
	/// </summary>
	public System.Drawing.Color SetWkgColor(int x, int y)
	{
		System.Drawing.Color color = ApiUtilities.GetColorAt(x, y);

		if (IsForeground)
		{
			setWkgFgColor(Color.FromArgb(color.A, color.R, color.G, color.B));
			OnPropertyChanged(nameof(WkgFg));
		}
		else
		if (IsBackground)
		{
			setWkgBgColor(Color.FromArgb(color.A, color.R, color.G, color.B));
			OnPropertyChanged(nameof(WkgBg));
		}

		return color;
	}

	private void setWkgFgColor(Color color)
	{
		WkgFg.Color = Color.FromArgb(color.A, color.R, color.G, color.B);
	}

	private void setWkgBgColor(Color color)
	{
		WkgBg.Color = Color.FromArgb(color.A, color.R, color.G, color.B);
	}
	
	#pragma warning disable IDE0001
	ApiUtilities.POINT point;
	#pragma warning restore IDE0001

	public void OnColorChooserBox_MouseMove(object sender, MouseEventArgs e)
	{
		if (IsBgEyeDropActive) return;

		setWkgColorOnMouseMove();
	}

	private void setWkgColorOnMouseMove()
	{
		if (IsForeground && isFgSelected) return;
		if (IsBackground && isBgSelected) return;

		if (ApiUtilities.GetCursorPos(out point))
		{
			SetWkgColor(point.X, point.Y);
		}
	}

	public void OnColorGray_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (IsBgEyeDropActive) return;

		if (ApiUtilities.GetCursorPos(out point))
		{
			SetWkgColor(point.X, point.Y);

			if (IsForeground) isFgSelected = true;
			if (IsBackground) isBgSelected = true;
		}
	}

	public void OnColorChooserBox_MouseDown(object? sender, MouseButtonEventArgs? e)
	{
		if (IsBgEyeDropActive) return;

		setWkgColorOnMouseDown();
	}

	private void setWkgColorOnMouseDown()
	{
		bool setSelColor = false;

		if (IsForeground)
		{
			if (isFgSelected)
			{
				isFgSelected = false;
			}
			else
			{
				isFgSelected = true;
				setSelColor = true;
			}
		}
		else if (IsBackground)
		{
			if (isBgSelected)
			{
				isBgSelected = false;
			}
			else
			{
				isBgSelected = true;
				setSelColor = true;
			}
		}

		if (setSelColor)
		{
			if (ApiUtilities.GetCursorPos(out point))
			{
				SetWkgColor(point.X, point.Y);
			}
		}

		if (IsForeground)
		{
			activateFgCx();
		}
		else
		{
			activateBgCx();
		}
	}

	public void SetFgWkgToSel()
	{
		setWkgFgColor(SelFg.ColorValue);
		activateFgCx();
	}

	public void SetBgWkgToSel()
	{
		setWkgBgColor(SelBg.ColorValue);
		activateBgCx();
	}

	private void activateFgCx()
	{
		TmpCx.SetColor(WkgFg.Color);
		activateCx();

	}

	private void activateBgCx()
	{
		TmpCx.SetColor(WkgBg.Color);
		
		activateCx();
	}

	/// <summary>
	/// this happens only when fg or be is updated or when a
	/// slider has stopped sliding
	/// </summary>
	private void activateCx()
	{
		HueValue = TmpCx.Hue;
		SaturationValue = TmpCx.Saturation;
		LightnessValue = TmpCx.Lightness;

		OnPropertyChanged(nameof(TmpCx));
		updateHslLimits();
		updateColorNumbers();
		updateProperties();
	}

	private void updateColorNumbers()
	{
		if (IsForeground)
		{
			TmpFgHex = TmpCx.ToHex;
			TmpFgDec = TmpCx.ToDec;
		}
		else
		{
			TmpBgHex = TmpCx.ToHex;
			TmpBgDec = TmpCx.ToDec;
		}
	}

	private void updateSelFgColorNumbers()
	{
		SelFgToolTip = $"{SelFg.ToHex}\n{SelFg.ToDec}";
	}

	private void updateSelBgColorNumbers()
	{
		SelBgToolTip = $"{SelBg.ToHex}\n{SelBg.ToDec}";
	}

	private void updateProperties()
	{
		OnPropertyChanged(nameof(TmpCx.AlphaB));
		OnPropertyChanged(nameof(TmpCx.RedB));
		OnPropertyChanged(nameof(TmpCx.GreenB));
		OnPropertyChanged(nameof(TmpCx.BlueB));
	}


	/// <summary>
	/// here when a slider has finished sliding therefore it happens less often
	/// </summary>
	/// <param name="slider"></param>
	public void UpdateSliderValue(string slider)
	{
		// Debug.WriteLine($"*** slider = {slider}");

		if (slider.IsVoid()) return;

		// Debug.WriteLine($"**     r {TmpCx.RedB,-5} g {TmpCx.GreenB,-5} b {TmpCx.BlueB,-5} | h {TmpCx.Hue,-5} s {TmpCx.Saturation,-5} l {TmpCx.Lightness,-5}");

		switch (slider)
		{
		case "GraySlider":
			{
				break;
			}
				
		case "HueSlider":
			{
				TmpCx.Hue = HueValue;
				UpdateColorSample();
				break;
			}
		
		case "SaturationSlider":
			{
				TmpCx.Saturation = SaturationValue;
				UpdateColorSample();
				break;
			}
		
		case "LightnessSlider":
			{
				TmpCx.Lightness = LightnessValue;
				UpdateColorSample();
				break;
			}
		default:
			{
				TmpCx.UpdateHslValues();
				TmpCx.UpdateGray();
				// TmpCx.UpdateColorValues();
				break;
			}
		}

		// Debug.WriteLine($"**     r {TmpCx.RedB,-5} g {TmpCx.GreenB,-5} b {TmpCx.BlueB,-5} | h {TmpCx.Hue,-5} s {TmpCx.Saturation,-5} l {TmpCx.Lightness,-5}\n");

		activateCx();
		updateProperties();
	}

	private void updateHslLimits()
	{
		SatOne.SetColor(TmpCx.AlphaB, TmpCx.Hue, 1.0, TmpCx.Lightness);
		SatZero.SetColor(TmpCx.AlphaB, TmpCx.Hue, 0.0, TmpCx.Lightness);

		LightOne.SetColor(TmpCx.AlphaB, TmpCx.Hue, TmpCx.Saturation, 1.0);
		LightHalf.SetColor(TmpCx.AlphaB, TmpCx.Hue, TmpCx.Saturation, 0.5);
		LighZero.SetColor(TmpCx.AlphaB, TmpCx.Hue, TmpCx.Saturation, 0.0);	

		OnPropertyChanged(nameof(SatOne));
		OnPropertyChanged(nameof(SatZero));
		OnPropertyChanged(nameof(LightOne));
		OnPropertyChanged(nameof(LightHalf));
		OnPropertyChanged(nameof(LighZero));
	}

	public void UpdateColorSample()
	{
		if (IsForeground) WkgFg = TmpCx.ToSolidColorBrush;
		else WkgBg = TmpCx.ToSolidColorBrush;
	}

	// eye drop routines

	private void loadGlobalCursor()
	{
		// ApiUtilities.SetGlobalCursorFromResourceB("/resources/EyeDropperPointer.cur");
		ApiUtilities.SetGlobalCursorTemp("/resources/EyeDropperPointer.cur");
	}


	
	[RelayCommand]
	private void BtnFgEyeDrop_OnClick()
	{
		Debug.WriteLine("*** Eye drop click");

		if (!IsFgEyeDropActive && !IsBgEyeDropActive)
		{
			Debug.WriteLine("*** Eye drop click / start");

			SetForground();
			isFgSelected = false;
			startGlobalMouseMove();
		}
		else // if (isEyeDropActive == true)
		{
			Debug.WriteLine("*** Eye drop click / end");
			endGlobalMouseMove();
		}
	}


	[RelayCommand]
	private void BtnBgEyeDrop_OnClick()
	{
		Debug.WriteLine("*** Eye drop click");

		if (!IsBgEyeDropActive && !IsFgEyeDropActive)
		{
			Debug.WriteLine("*** Eye drop click / start");

			SetBackground();
			isBgSelected = false;
			startGlobalMouseMove();
		}
		else // if (isEyeDropActive == true)
		{
			Debug.WriteLine("*** Eye drop click / end");
			endGlobalMouseMove();
		}
	}

	private void startGlobalMouseMove()
	{
		if (IsBgEyeDropActive) return;

		Debug.WriteLine("\n** start global mouse");

		_apiUtilities.MouseMoveHook.MouseAction += OnMouseAction;
		_apiUtilities.MouseMoveHook.Start();
		loadGlobalCursor();

		IsBgEyeDropActive = true;
	}

	private void endGlobalMouseMove()
	{
		_apiUtilities.MouseMoveHook.Stop();
		_apiUtilities.MouseMoveHook.MouseAction -= OnMouseAction;
		_apiUtilities.MouseMoveHook.Dispose();
		// ApiUtilities.RestoreSystemCursor();
		ApiUtilities.ResetSystemCursor();

		IsBgEyeDropActive = false;

		Debug.WriteLine("** end global mouse\n");

	}

	private void OnMouseAction(object? sender, MouseHookEventArgs e)
	{
		switch (e.Message)
		{
		case WM_LBUTTONDOWN: // WM_LBUTTONDOWN
			// MouseMsg = "Left Button Pressed";
			break;
		case WM_LBUTTONUP: // WM_LBUTTONUP
			// MouseMsg = "Left Button Released";
			Debug.WriteLine("*** action / btn up");
			if (!IsMouseOverEyeDropBtn)
			{
				setWkgColorOnMouseDown();
				endGlobalMouseMove();
			}
			break;
		case WM_RBUTTONDOWN: // WM_RBUTTONDOWN
			// MouseMsg = "Right Button Pressed";
			break;
		case WM_RBUTTONUP: // WM_LBUTTONUP
			// MouseMsg = "Right Button Released";
			break;
		case WM_MOUSEMOVE: // WM_MOUSEMOVE
			// Keep status simple during high volume movement
			// updateRectColor(e.X, e.Y);
			setWkgColorOnMouseMove();
			break;
		}
	}

	// private void updateRectColor(int x, int y)
	// {
	// 	System.Drawing.Color color = GetColorAt(x, y);
	// 	EyeDropFill = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
	// }
}