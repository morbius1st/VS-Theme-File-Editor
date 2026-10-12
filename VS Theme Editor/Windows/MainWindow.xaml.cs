using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;
using SettingsManager;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using ListViewItem = System.Windows.Controls.ListViewItem;
using TextBlock = Wpf.Ui.Controls.TextBlock;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace VS_Theme_Editor;


// note the location of where the theme colors apply
// https://learn.microsoft.com/en-us/visualstudio/extensibility/ux-guidelines/shared-colors-for-visual-studio?view=vs-2022#shell-appearance
// https://learn.microsoft.com/en-us/visualstudio/extensibility/ux-guidelines/theme-color-token-reference?view=visualstudio


public partial class MainWindow : INotifyPropertyChanged
{

    private readonly MainWindowViewModel _viewModel;

	public MainWindow()
    {
        InitializeComponent();

        ISnackbarService snackbarService = new SnackbarService();
        snackbarService.SetSnackbarPresenter(RootSnackbar);

        _viewModel = new MainWindowViewModel(snackbarService, this);
        DataContext = _viewModel;

		ThemeIsDark = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark;

		//if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) == false)
		//{
		//    //Restart as admin
		//    var startInfo = new ProcessStartInfo
		//    {
		//        FileName = System.AppContext.BaseDirectory + AppDomain.CurrentDomain.FriendlyName,
		//        UseShellExecute = true,
		//        Verb = "runas"
		//    };

		//    Process.Start(startInfo);

		//    Application.Current.Shutdown();
		//}
	}		

	private async void btn_AddCategory_clicked(object sender, RoutedEventArgs e)
    {
 
        //var msgError = new Wpf.Ui.Controls.ContentDialog { Title = $"New Settings Version Detected", Content = "Your settings have been reset to their default to accommodate the update", CloseButtonText = "OK" };
        //await msgError.ShowAsync();

		if (_viewModel.WorkingTheme is null) return;

        TextBox textBox = new Wpf.Ui.Controls.TextBox
        {
            PlaceholderText = "Category Name"
        };
 
        TextBlock label = new Wpf.Ui.Controls.TextBlock
        {
            Text = "Enter the name of the new category:"
        };
 
        StackPanel stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { label, textBox }
        };
 
        ContentDialog cDialog = new Wpf.Ui.Controls.ContentDialog
        {
            Title = "Add Category",
            Content = stackPanel,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DialogHostEx = RootContentDialog,
		};
 
        ContentDialogResult retX = await cDialog.ShowAsync();
 
        if (retX != ContentDialogResult.Primary) return;
 
        if (string.IsNullOrWhiteSpace(textBox.Text))
        {
			Wpf.Ui.Controls.MessageBox msgBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "Error",
                Content = "Category name cannot be empty."
            };
            await msgBox.ShowDialogAsync();
        }

		CategoryData? cd = _viewModel.AddCategory(textBox.Text);

		if (cd == null) return;
		

		_viewModel.ScrollItemToCenter(WorkingThemeCategories.SelectedIndex);

		// scrollToCenter();

		// object a = WorkingThemeCategories.SelectedItem; 
		// WorkingThemeCategories.ScrollIntoView(a);


		// WorkingThemeCategories.SelectedIndex = 73;

		// _viewModel.SelectedCategoryKvp = new KeyValuePair<string, CategoryData?>(cd.Name, cd);

		// KeyValuePair<string, CategoryData> c = (KeyValuePair<string, CategoryData>) WorkingThemeCategories.Items.GetItemAt(index);
		//
		// lv.ScrollIntoView(73);

		// lv.ScrollIntoView(_viewModel.SelectedCategory);

		// KeyValuePair<string, CategoryData> kvp = new KeyValuePair<string, CategoryData> (cd.Name, cd);
		//
		// _viewModel.CurrentCategory = cd;

		// int index = WorkingThemeCategories.SelectedIndex;

		// Debug.WriteLine($"selected is {(c.Value).Name} / index {index}");
		// Debug.WriteLine($"selected index {index} ");

		// Thread.Sleep(2000);

		// _viewModel.WorkingTheme.CategoriesView.MoveCurrentTo(a);
		// BtnTestFind_OnClick(null, null);
	}

	// private void scrollToCenter()
	// {
	// 	ScrollViewer? sv = getVisualChild<ScrollViewer>(WorkingThemeCategories);
	// 	// object? a = WorkingThemeCategories.SelectedItem;
	// 	int idx = WorkingThemeCategories.SelectedIndex;
	//
	// 	double pos = idx - (sv.ViewportHeight / 2);
	//
	// 	if (pos < 0) pos = 0;
	//
	// 	sv.ScrollToVerticalOffset(pos);
	// }
	//
	// private T? getVisualChild<T>(Visual parent) where T: Visual
	// {
	// 	T? descendant = null;
	//
	// 	for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
	// 	{
	// 		Visual child = (Visual)VisualTreeHelper.GetChild(parent, i);
	// 		if (child is T typedChild)
	// 		{
	// 			return typedChild;
	// 		}
	//
	// 		descendant = getVisualChild<T>(child);
	// 		if (descendant != null) break;
	// 	}
	//
	// 	return descendant;
	// }


	private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
	{
		// _viewModel.AddTestMru();

		if (!UserSettings.Data.WinThemeEditorTL.X.Equals(0.0) &&
			!UserSettings.Data.WinThemeEditorTL.Y.Equals(0.0))
		{
			Left = UserSettings.Data.WinThemeEditorTL.X;
			Top = UserSettings.Data.WinThemeEditorTL.Y;
		}
		else
		{
			Left = 100.0;
			Top = 100.0;
		}

		_viewModel.InitScrollViewer();
	}

	private void FluentWindow_Closing(object sender, CancelEventArgs e)
	{
		_viewModel.OnClosing();

		Point p = new Point(Left, Top);

		UserSettings.Data.WinThemeEditorTL = p;

		base.OnClosed(e);
	}

	private void BtnThemeSwitch_OnClick(object sender, RoutedEventArgs e)
	{
		themeIsDark = !themeIsDark;
		updateAppTheme();
	}

	private ApplicationTheme appTheme;

	private bool themeIsDark;

	public bool ThemeIsDark
	{
		get => themeIsDark;
		set
		{
			if (themeIsDark == value) return;
			themeIsDark = value;
			OnPropertyChanged();
		}
	}

	private void updateAppTheme()
	{
		OnPropertyChanged(nameof(ThemeIsDark));

		appTheme = themeIsDark ? ApplicationTheme.Dark : ApplicationTheme.Light;

		ApplicationThemeManager.Apply(appTheme);

		switchAppTheme();
	}

	private void switchAppTheme()
	{
		Uri newThemeUri;
		string currentThemeScheme;

		if (themeIsDark)
		{
			newThemeUri = new Uri($"Windows/XamlThemes/DarkCustom.xaml", UriKind.RelativeOrAbsolute);
			currentThemeScheme = "LightCustom";
		}
		else
		{
			newThemeUri = new Uri($"Windows/XamlThemes/LightCustom.xaml", UriKind.RelativeOrAbsolute);
			currentThemeScheme = "DarkCustom";
		}

		try
		{
			ResourceDictionary newThemeDict = new ResourceDictionary { Source = newThemeUri };

			Collection<ResourceDictionary> dictionaries = Application.Current.Resources.MergedDictionaries;

			ResourceDictionary? existingThemeDict = dictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains(currentThemeScheme));

			if (existingThemeDict == null) return;

			int idx = dictionaries.IndexOf(existingThemeDict);
			dictionaries[idx] = newThemeDict;

		}
		// ReSharper disable once EmptyGeneralCatchClause
		catch { }
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[DebuggerStepThrough]
	private void OnPropertyChanged([CallerMemberName] string memberName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
	}

	private void BtnTestFind_OnClick(object sender, RoutedEventArgs e)
	{
		KeyValuePair<string, CategoryData>? a = findItem("test");

		if (a == null) return;

		WorkingThemeCategories.ScrollIntoView(a);
	}

	private KeyValuePair<string, CategoryData>? findItem(string name)
	{
		foreach (KeyValuePair<string, CategoryData> kvp in WorkingThemeCategories.Items)
		{
			if (kvp.Key.Equals(name)) return kvp;
		}

		return null;
	}

}

/*
public static void ChangeTheme(string themeName)
{
// 1. Create the URI pointing to the specific ResourceDictionary
var themeUri = new Uri($"Themes/{themeName}.xaml", UriKind.RelativeOrAbsolute);

try
{
// 2. Load the dictionary object
var newThemeDict = new ResourceDictionary { Source = themeUri };

// 3. Clear existing merged dictionaries and add the new one
var dictionaries = Application.Current.Resources.MergedDictionaries;
dictionaries.Clear();
dictionaries.Add(newThemeDict);
}
catch (Exception ex)
{
MessageBox.Show($"Error loading theme: {ex.Message}");
}
}
*/



