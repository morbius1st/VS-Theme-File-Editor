using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Appearance;

namespace VS_Theme_Editor
{
	/// <summary>
	/// Interaction logic for WinThemeReport.xaml
	/// </summary>
	public partial class WinThemeReport
	{
		public WinThemeReport(ThemeFile tf)
		{
			InitializeComponent();

			_viewModel = new WinThemeReportViewModel(this, tf);

			DataContext = _viewModel;
		}

		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly WinThemeReportViewModel _viewModel;

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
