// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             WinThemeReportModel.cs
// Created:      2026-09-12 (06:09)

using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Controls;

namespace VS_Theme_Editor;


public partial class WinThemeReportViewModel : ObservableObject
{
	// public static string Test = "this is a test\tthis is a long line\tmore line parts\tlonger still\ttest\nthis is another test\nthis\nthis\nthis\nthis\nthis\nthis\nthis\nthis\nthis\nthis\nthis\nthis";

	public WinThemeReportViewModel(WinThemeReport parent, ThemeFile tf)
	{
		this._parent = parent;

		this.tf = tf;
		this._reportSupport = new(this, tf);

		ReportMnuItem = parent.MnuBasicType;
		setDefaultReportType();
		setDefaultVerboseType();
		Reset();
		ProgressMaximum = 100;

	}

	private readonly WinThemeReport _parent;

	private readonly ThemeReportSupport _reportSupport;

	[ObservableProperty]
	private ThemeFile tf;

	[ObservableProperty]
	private string? reportText;

	[ObservableProperty]
	private string? reportType;

	[ObservableProperty]
	private MenuItem? reportMnuItem;

	[ObservableProperty]
	private string? verboseType;

	[ObservableProperty]
	private double progressValue;

	[ObservableProperty]
	private double progressMaximum;

	public bool CanReset => ReportText != null && ReportText.Length > 0;

	private bool verbose;

	private int which;

	private StringBuilder reportBuilder = new StringBuilder();

	public void Display()
	{
		ReportText = reportBuilder.ToString();

		OnPropertyChanged(nameof(CanReset));
	}

	public void Append(string text)
	{
		reportBuilder.Append(text);
	}

	public void AppendLine(string text)
	{
		reportBuilder.AppendLine(text);
	}

	public void LineBreak()
	{
		Append("\n");
	}

	[RelayCommand]
	public void Reset()
	{
		ReportText = string.Empty;
		reportBuilder = new ();
	}

	[RelayCommand]
	public void SetWhichReport(object w)
	{
		ReportMnuItem = (MenuItem) w;

		setDefaultReportType();
	}

	private void setDefaultReportType()
	{
#pragma warning disable CS8601 // Possible null reference assignment.
		ReportType = ReportMnuItem?.Header as string;
		which = (int) (ReportMnuItem?.Tag ?? 0);
#pragma warning restore CS8601 // Possible null reference assignment.
	}

	[RelayCommand]
	public void SetVerboseReport(object v)
	{
		MenuItem? mi = v as MenuItem;

		setDefaultVerboseType(mi);
	}

	private void setDefaultVerboseType(MenuItem? mi = null)
	{
		mi ??= _parent.MnuBasicVerbose;

#pragma warning disable CS8601 // Possible null reference assignment.
		VerboseType = mi.Header as string;
		verbose = (bool) (mi.Tag ?? false);
#pragma warning restore CS8601 // Possible null reference assignment.
	}

	[RelayCommand]
	public void Proceed()
	{
		// _reportSupport.GenerateReport(which, verbose);
		generatReport();

		// testReports();
	}

	private async void generatReport()
	{
		await Task.Run(() => _reportSupport.GenerateReport(which, verbose));
	}

	private async void testReports()
	{
		AppendLine($"\n\n\n************************************************************");
		AppendLine($"Report - type 0 / false");
		AppendLine("");
		await Task.Run(() => _reportSupport.GenerateReport(0, false));

		AppendLine($"\n\n\n************************************************************");
		AppendLine($"Report - type 0 / true");
		AppendLine("");
		await Task.Run(() => _reportSupport.GenerateReport(0, true));




		AppendLine($"\n\n\n************************************************************");
		AppendLine($"Report - type 1 / false");
		AppendLine("");
		await Task.Run(() => _reportSupport.GenerateReport(1, false));

		AppendLine($"\n\n\n************************************************************");
		AppendLine($"Report - type 2 / false");
		AppendLine("");
		await Task.Run(() => _reportSupport.GenerateReport(2, false));

		AppendLine($"\n\n\n************************************************************");
		AppendLine($"Report - type 9 / false");
		AppendLine("");
		await Task.Run(() => _reportSupport.GenerateReport(9, false));



		
		AppendLine($"\n\n\n************************************************************");
		AppendLine($"Report - type 1 / true");
		AppendLine("");
		await Task.Run(() => _reportSupport.GenerateReport(1, true));
		
		AppendLine($"\n\n\n************************************************************");
		AppendLine($"Report - type 2 / true");
		AppendLine("");
		await Task.Run(() => _reportSupport.GenerateReport(2, true));
		
		AppendLine($"\n\n\n************************************************************");
		AppendLine($"Report - type 9 / true");
		AppendLine("");
		await Task.Run(() => _reportSupport.GenerateReport(9, true));

	}


}