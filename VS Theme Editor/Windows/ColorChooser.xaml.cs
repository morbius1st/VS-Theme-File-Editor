using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using Color = System.Windows.Media.Color;

using SettingsManager;

namespace VS_Theme_Editor;


public partial class ColorChooser : FluentWindow
{
	private readonly ColorChooserViewModel _viewModel;

	public ColorChooserViewModel ViewModel => _viewModel;

	public ColorChooser(Color fg, Color bg)
	{
		_viewModel = new ColorChooserViewModel(fg, bg);
		DataContext = _viewModel;

		InitializeComponent();
	}

	private void ColorChooser_OnContentRendered(object? sender, EventArgs e)
	{
		Point p = GrdColorChooserBox.TranslatePoint(new Point(0, 0), this);
	}

	private void ColorChooser_Closed(object sender, EventArgs e)
	{
		// ApiUtilities.RestoreSystemCursor();
		// ApiUtilities.ResetSystemCursor();
	}

	private void SpColorSelectBox_OnMouseMove(object sender, MouseEventArgs e)
	{
		_viewModel.OnColorChooserBox_MouseMove(sender, e);
	}

	private void SpColorSelectBox_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		_viewModel.OnColorChooserBox_MouseDown(sender, e);
	}

	private void OnColorGray_MouseDown(object sender, MouseButtonEventArgs e)
	{
		_viewModel.OnColorGray_MouseDown(sender, e);
	}

	private void Done_OnClick(object sender, RoutedEventArgs e)
	{
		this.Close();
	}

	private string whoIsSliding = string.Empty;

	private void Thumb_OnDragStarted(object sender, DragStartedEventArgs e)
	{
		whoIsSliding = ((string) ((Thumb) sender).Tag) ?? "unnamed";
	}

	private void Thumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
	{
		_viewModel.UpdateSliderValue(whoIsSliding);

		whoIsSliding = string.Empty;
	}
	
	private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		_viewModel.UpdateColorSample();
	}

	public void ColorTest()
	{
		byte r1;
		byte g1;
		byte b1;

		byte r2;
		byte g2;
		byte b2;

		byte r3;
		byte g3;
		byte b3;

		byte r4;
		byte g4;
		byte b4;

		double l0;

		double l2;
		double s2;

		double h1;
		double s1;
		double v1;

		float h4;
		float s4;
		float l4;

		float h6;
		float s6;
		float l6;



		System.Drawing.Color c6 = System.Drawing.Color.Cyan;
		Color c1 = Colors.Cyan;

		r1 = c1.R;
		g1 = c1.G;
		b1 = c1.B;

		l0 = c1.GetLuminance()/100;

		h1 = c1.GetHue();
		v1 = c1.GetBrightness()/100.0;
		s1 = c1.GetSaturation()/100.0;
		
		l2 = v1 * (1.0 - s1 / 2.0);
		
		if (l2 > 0 && l2 < 1)
		{
			s2 = (v1 - l2) / Math.Min(l2, 1.0 - l2);
		}
		else
		{
			s2 = 0;
		}

		h4 = c1.GetHueHsl();
		s4 = c1.GetSaturationHsl();
		l4 = c1.GetLightnessHsl();

		h6 = c6.GetHue();
		s6 = c6.GetSaturation();
		l6 = c6.GetBrightness();

		Color c2 = ColorEx.FromHsl(255, h1, s2, l2);

		r2 = c2.R;
		g2 = c2.G;
		b2 = c2.B;

		// Color c3 = ColorEx.FromHsl2(255, h1, s2, l2);
		//
		// r3 = c3.R;
		// g3 = c3.G;
		// b3 = c3.B;

		Color c4 = ColorEx.FromHsl(255, h4, s4, l4);

		r4 = c4.R;
		g4 = c4.G;
		b4 = c4.B;
		//
		// Color c5 = ColorEx.FromHsl2(255, h4, s4, l4);


		Color c7 = ColorEx.FromHsl(255, h6, s6, l6);

	}

	private void SelFgRect_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		_viewModel.SetFgWkgToSel();
	}

	private void SelBgRect_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		_viewModel.SetBgWkgToSel();
	}


	private void ColorSelect_OnMouseEnter(object sender, MouseEventArgs e)
	{
		_viewModel.IsMouseOverEyeDropBtn = true;
	}
	private void ColorSelect_OnMouseLeave(object sender, MouseEventArgs e)
	{
		_viewModel.IsMouseOverEyeDropBtn = false;
	}

	private void ColorChooser_OnLoaded(object sender, RoutedEventArgs e)
	{
		if (!UserSettings.Data.WinColorChooserTL.X.Equals(0.0) && !UserSettings.Data.WinColorChooserTL.Y.Equals(0.0))
		{
			this.Left = UserSettings.Data.WinColorChooserTL.X;
			this.Top = UserSettings.Data.WinColorChooserTL.Y;
		}
		else
		{
			this.Left = 100.0;
			this.Top = 100.0;
		}
	}
	private void ColorChooser_OnClosing(object? sender, CancelEventArgs e)
	{
		// ApiUtilities.RestoreSystemCursor();
		ApiUtilities.ResetSystemCursor();

		Point p = new Point(this.Left, this.Top);

		UserSettings.Data.WinColorChooserTL = p;
		UserSettings.Admin.Write();
	}
}
