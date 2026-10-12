using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using UtilityLibrary;
using Wpf.Ui;
using Wpf.Ui.Controls;
using static VS_Theme_Editor.ApiUtilities;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;

using Microsoft.VisualStudio.TextManager.Interop;

namespace VS_Theme_Editor;

public partial class MainWindowViewModel : ObservableObject
{
	private readonly MainWindow _parent;
	private readonly ApiUtilities _apiUtilities;

	public MainWindowViewModel(ISnackbarService snackbarService, MainWindow parent)
	{
		_snackbarService = snackbarService;

		MruMgr = new MruManager();

		ThemeMgr = ThemeManager.Instance;

		_parent = parent;

		_apiUtilities = new ApiUtilities();

		WorkingTheme = new Theme("unnamed");
		workingThemeFile = new ThemeFile();

		FoundCatName = string.Empty;

	}

	private ThemeFile workingThemeFile;

	[ObservableProperty]
	private Theme? workingTheme;

	[ObservableProperty]
	private bool gotWorkingTheme;

	[ObservableProperty]
	private KeyValuePair<string, CategoryData?> selectedCategoryKvp;

	[ObservableProperty]
	private CategoryData? currentCategory;

	[ObservableProperty]
	private string foundCatName;

	[ObservableProperty]
	private string? visualStudioDirectory;

	[ObservableProperty]
	private string? visualStudioDesc;

	[ObservableProperty]
	private Brush? visualStudioDescBdr;

	[ObservableProperty]
	private string? themeFilePath;

	[ObservableProperty]
	private string? categorySearch;

	[ObservableProperty]
	private string? colorFilter;

	// [ObservableProperty]
	// private string themeStyleName = "Light";

	// public ICollectionView? FilteredEntries
	// {
	// 	get
	// 	{
	// 		return filteredEntries;
	// 	}
	// 	private set => filteredEntries = value;
	// }

	[ObservableProperty]
	private	MruManager mruMgr;

	[ObservableProperty]
	private ThemeManager themeMgr;

	[ObservableProperty]
	private bool extendThemeInfo;

	private ScrollViewer? scrollViewer;

	private readonly ISnackbarService _snackbarService;

	private MostRecentlyUsed mruFile = MostRecentlyUsed.MruEmpty();

	private MostRecentlyUsed mruFolder = MostRecentlyUsed.MruEmpty();

	private SolidColorBrush bdrBrush1 = null!;
	private SolidColorBrush bdrBrush2 = null!;

	public MostRecentlyUsed MruFile
	{
		get => mruFile;

		set
		{
			if (mruFile != value)
			{
				mruFile = value;
				OnPropertyChanged();

				showLoadFileToast(mruFile.FileName);

				ThemeFilePath = mruFile.FilePath;

				processSelectedTheme(mruFile.FilePath);
			}
		}
	}

	public MostRecentlyUsed MruFolder
	{
		get => mruFolder;

		set
		{
			if (value != null! && mruFolder != value)
			{
				mruFolder = value;
				OnPropertyChanged();

				SelectTheme();
			}
		}
	}

	private void setMruFile(MostRecentlyUsed mrufile)
	{
		mruFile = mrufile;
		OnPropertyChanged();

		showLoadFileToast(mruFile.FileName);

		ThemeFilePath = mruFile.FilePath;
	}

	private void showLoadFileToast(string fileName)
	{
		_snackbarService.Show(
			$"Loading Theme | {fileName}",
			string.Empty,
			ControlAppearance.Primary,
			new SymbolIcon(SymbolRegular.Info24),
			TimeSpan.FromSeconds(1.0)
			);
	}

	partial void OnWorkingThemeChanged(Theme? value)
	{
		// GotWorkingTheme = false;
		GotWorkingTheme = value?.Guid != null;
	}

	public CategoryData? AddCategory(string categoryName)
	{
		if (WorkingTheme is null) return null;
		if (string.IsNullOrWhiteSpace(categoryName))
		{
			Debug.WriteLine("Category name cannot be empty.");
			return null;
		}

		Debug.WriteLine($"selected category {categoryName}");

		CategoryData newCategory = new CategoryData
		{
			Name = categoryName,
			Entries = new ()
		};

		WorkingTheme.Categories.Add(categoryName, newCategory);

		WorkingTheme.CategoriesView!.Refresh();

		SelectedCategoryKvp = new KeyValuePair<string, CategoryData?>(categoryName, newCategory);
		
		return newCategory;
	}


	/* commands */

	// ReSharper disable once InconsistentNaming
	[RelayCommand]
	private void ThemeReport()
	{
		WinThemeReport themeReport = new WinThemeReport(workingThemeFile);
		themeReport.Owner = _parent;

		themeReport.ShowDialog();
	}

	[RelayCommand]
	// ReSharper disable once InconsistentNaming
	private void ColorChooser()
	{
		ColorChooser colorChooser = new ColorChooser(Colors.White, Colors.Black);
		colorChooser.Owner = _parent;

		colorChooser.ShowDialog();
	}

	[RelayCommand]
	// ReSharper disable once InconsistentNaming
	private void SaveTheme()
	{
		if (WorkingTheme is null || ThemeFilePath is null) return;

		PkgDefCompiler compiler = new PkgDefCompiler();
		compiler.Compile(WorkingTheme, ThemeFilePath);
	}

	[RelayCommand]
	// ReSharper disable once InconsistentNaming
	private void SaveAsTheme()
	{
		if (WorkingTheme is null || ThemeFilePath is null) return;

		SaveFileDialog fileDialog = new SaveFileDialog();
		fileDialog.Filter = "PkgDef files (*.pkgdef)|*.pkgdef";
		fileDialog.DefaultExt = ".pkgdef";

		if (fileDialog.ShowDialog() == true)
		{
			PkgDefCompiler compiler = new PkgDefCompiler();
			compiler.Compile(WorkingTheme, fileDialog.FileName);
		}
	}

	// ReSharper disable once InconsistentNaming
	[RelayCommand]
	private void SelectTheme()
	{
		string defDir = mruFolder.FolderPath.IsVoid() ? string.Empty : mruFolder.FolderPath;

		OpenFileDialog fileDialog = new OpenFileDialog
		{
			Filter = "PkgDef files (*.pkgdef)|*.pkgdef",
			Title = "Select a theme file",
			AddToRecent = true,
			InitialDirectory = defDir
		};

		if (fileDialog.ShowDialog() == true)
		{
			string themeName = processSelectedTheme(fileDialog.FileName);

			setMruFile(MruMgr.Add(fileDialog.FileName, themeName));
		}

		mruFolder = MostRecentlyUsed.MruEmpty();
	}

	[RelayCommand]
	// ReSharper disable once InconsistentNaming
	private void DeleteCategory(CategoryData parameter)
	{
		if (WorkingTheme is null) return;

		// ReSharper disable once CanSimplifyDictionaryRemovingWithSingleCall
		if (parameter.Name != null && WorkingTheme.Categories.ContainsKey(parameter.Name))
		{
			WorkingTheme.Categories.Remove(parameter.Name);
			OnPropertyChanged(nameof(WorkingTheme));
		}
		else
		{
			Debug.WriteLine("Category not found in theme.");
		}
	}

	[RelayCommand]
	// ReSharper disable once InconsistentNaming
	private async Task TestTheme()
	{
		if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) == false)
		{
			MessageBoxResult ret = MessageBox.Show("Admin mode is required to test the theme as it needs access to the Visual Studio Directory. Restart in Admin mode? (Make sure you save the theme first!)", "Administrator Mode required", MessageBoxButton.YesNo);

			if (ret == MessageBoxResult.Yes)
			{
				//Restart as admin
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = AppContext.BaseDirectory + AppDomain.CurrentDomain.FriendlyName,
					UseShellExecute = true,
					Verb = "runas"
				};

				Process.Start(startInfo);

				Application.Current.Shutdown();
			}

			return;
		}

		if (Directory.Exists(VisualStudioDirectory) == false)
		{
			MessageBox.Show("Please select the Visual Studio Directory (/Microsoft Visual Studio/2022/Community", "Visual Studio Directory Not Found");
			OpenFolderDialog folderDialog = new OpenFolderDialog()
			{
				Title = "Select Visual Studio Community 2022 Directory (/Microsoft Visual Studio/2022/Community)",
				Multiselect = false
			};
			if (folderDialog.ShowDialog() == true)
			{
				// string? directoryName = folderDialog.FolderName;
				VisualStudioDirectory = folderDialog.FolderName ?? string.Empty;
			}
			else
			{
				return; // User cancelled the dialog
			}
		}

		string vsExe = Path.Combine(VisualStudioDirectory, @"Common7\IDE\devenv.exe");
		string vsThemeDir = @"Common7\IDE\CommonExtensions\Platform";

		if (!File.Exists(vsExe))
		{
			MessageBox.Show("Visual Studio executable not found. Please ensure the path is correct.", "Error");
			return;
		}

		if (!Directory.Exists(Path.Combine(VisualStudioDirectory, vsThemeDir)))
		{
			MessageBox.Show("Visual Studio theme directory not found. Please ensure the path is correct.", "Error");
			return;
		}

		string outputFilePath = Path.Combine(VisualStudioDirectory, vsThemeDir, "ThemeTest.pkgdef");

		PkgDefCompiler compiler = new PkgDefCompiler();

		#pragma warning disable CS8604 // Possible null reference argument.
		compiler.Compile(WorkingTheme, outputFilePath);
		#pragma warning restore CS8604 // Possible null reference argument.

		Process updateConfigProcess = Process.Start(vsExe, "/updateconfiguration");

		_snackbarService.Show("Testing Theme", "Updating Visual Studio Configuration. Please wait...", ControlAppearance.Info, null, TimeSpan.FromSeconds(15));

		await updateConfigProcess.WaitForExitAsync();
		_snackbarService.Show("Testing Theme", "Close Visual Studio to Continue", ControlAppearance.Success, null, TimeSpan.FromSeconds(1000));


		if (updateConfigProcess.ExitCode != 0) throw new Exception("VS Failed to Update Config");
		await Task.Delay(1000); // Wait a bit to ensure the config is updated

		Process runner = Process.Start(vsExe);

		runner.WaitForExit();
		_snackbarService.Show("Testing Theme", "Visual Studio Closed. You can now edit the theme.", ControlAppearance.Success, null, TimeSpan.FromSeconds(10));

		File.Delete(outputFilePath);
	}

	[RelayCommand]
	// ReSharper disable once InconsistentNaming
	private void RegenerateGuid()
	{
		Guid guid = Guid.NewGuid();
		if (WorkingTheme is not null)
		{
			WorkingTheme.Guid = guid;
			OnPropertyChanged(nameof(WorkingTheme));
		}
		else
		{
			Debug.WriteLine("WorkingTheme is null, cannot regenerate GUID.");
		}
	}

	[RelayCommand]
	// ReSharper disable once InconsistentNaming
	private void Test()
	{
		// *** sample tests

		// Samples.TestSamples2();
		// Samples.TestSampleClone();
		// Samples.TestSampleCompare();
		// Samples.TestApxSearch();
		// Debug.WriteLine("\n\n*** ignore case - false");
		// SearchTests.TestApxSearch(false);
		// Debug.WriteLine("\n\n*** ignore case - true");
		// SearchTests.TestApxSearch(true);

		// SearchTests.ShowList();
		//
		// Debug.WriteLine("\n\n*** ******* Dl Search **********\n");

		// SearchTests.TestFuzzySearchDl();
		// SearchTests.TestFuzzySearchDlMultiple();


		// Debug.WriteLine("\n\n*** ******* Cs Search (old) **********\n");
		//
		// SearchTests.TestFuzzySearch();
		// SearchTests.TestFuzzySearchMultiple();

		// Debug.WriteLine("\n\n*** ******* Cs Search (new) **********\n");
		//
		// SearchTests.TestFuzzyStringSearch();


		// *** live tests
		//
		// if (WorkingTheme == null) return;
		//
		// SearchTests.TestFuzzySearchLive(WorkingTheme);


	}

	// [RelayCommand]
	// private void ChooseBaseTheme(ThemeStyle baseTheme)
	// {
	// 	Dictionary<Guid, ThemeInfo> vsRootThemes =
	// 		_ThemeMgr.ThemeData.ThemeInfoList[_DataMgr.VsId];
	//
	// 	foreach ((Guid key, ThemeInfo vst) in vsRootThemes)
	// 	{
	// 		if (vst.Style == baseTheme)
	// 		{
	// 			WorkingTheme!.Fallback = key.ToString();
	// 			ThemeInfo =
	// 				_ThemeMgr.ThemeData.LoadThemes(_DataMgr.VsId, WorkingTheme.Fallback);
	//
	// 			showData();
	// 		}
	// 	}
	// }


	// partial void OnSelectedCategoryChanged(CategoryData? value)


	partial void OnCategorySearchChanged(string? value)
	{
		if (value == null || value.Length < 3) return;

		// int idx = findNearestCategory(value);
		string name = findCategoryFuzzy(value);

		if (name.IsVoid()) return;

		FoundCatName = name;

		CategoryData cd = WorkingTheme!.Categories[name];

		WorkingTheme!.CategoriesView!.MoveCurrentTo(new KeyValuePair<string, CategoryData?>(name, cd));

		int index = WorkingTheme!.CategoriesView!.CurrentPosition;

		ScrollItemToCenter(index);
	}

	// private int findNearestCategory(string findText)
	// {
	// 	if ((WorkingTheme!.Categories.Find(findText, true) == false)) return -1;
	//
	// 	return WorkingTheme.Categories.FoundIdx;
	// }

	private string findCategoryFuzzy(string findText)
	{
		BestMatchDbl found = WorkingTheme!.Categories.FindFuzzy(findText);

		return found[0].Item2;
	}

	partial void OnSelectedCategoryKvpChanged(KeyValuePair<string, CategoryData?> value)
	{
		Debug.WriteLine($"selected category changed {value.Key}");

		if (value.Value != null)
		{
			CurrentCategory = value.Value;

			value.Value.InitView();

			// FilteredEntries = CollectionViewSource.GetDefaultView(kvp.Value.Entries.Values);
			//
			// FilteredEntries.Filter = FilterPredicate;
			//
			// OnPropertyChanged(nameof(FilteredEntries));
		}
	}

	public void InitScrollViewer()
	{
		scrollViewer = getVisualChild<ScrollViewer>(_parent.WorkingThemeCategories);
	}

	partial void OnColorFilterChanged(string? value)
	{
		// FilteredEntries?.Refresh();

		#pragma warning disable CS8602 // Dereference of a possibly null reference.
		CurrentCategory.SetColorFilter(value);
		#pragma warning restore CS8602 // Dereference of a possibly null reference.
	}

	private string processSelectedTheme(string filePath)
	{
		// string? themeName = ThemeMgr.ProcessSelectedTheme(filePath, out tf, true);
		ThemeMgr.ProcessSelectedTheme(filePath, out workingThemeFile, ExtendThemeInfo);

		WorkingTheme = workingThemeFile.ThemeFinal;

		if (WorkingTheme == null) return VisualStudioData.VSD_UDF;

		WorkingTheme.Sort();
		// WorkingTheme.SetCategoryCount();

		ThemeFilePath = filePath;

		CurrentCategory = WorkingTheme.Categories.First().Value;

		reSelectCategory();

		OnPropertyChanged(nameof(WorkingTheme));

		return WorkingTheme.Name!;
	}

	private void reSelectCategory()
	{
		if (CurrentCategory != null && WorkingTheme != null)
		{
			CategoryData cd;

			if (CurrentCategory.Name != null && WorkingTheme.Categories.TryGetValue(CurrentCategory.Name, out cd))
			{
				SelectedCategoryKvp =
					new KeyValuePair<string, CategoryData?>(CurrentCategory.Name, cd);
			}

			// cd = WorkingTheme.Categories[priorCategory.Name];
			//
			//
			// foreach ((string key, CategoryData cat) in WorkingTheme.Categories)
			// {
			// 	if (cat.Name.Equals(priorCategory.Name, StringComparison.OrdinalIgnoreCase))
			// 	{
			// 		SelectedCategory = new KeyValuePair<string, CategoryData?>(key, cat);
			// 		// OnSelectedCategoryChanged(SelectedCategory);
			// 		return;
			// 	}
			// }
		}

		// FilteredEntries = null;
		// OnPropertyChanged(nameof(FilteredEntries));
	}

	// private bool FilterPredicate(object obj)
	// {
	// 	if (obj is not ThemeColorEntry entry)
	// 		return false;
	// 	if (string.IsNullOrWhiteSpace(ColorFilter))
	// 		return true;
	// 	string filter = ColorFilter.Trim().ToLowerInvariant();
	// 	return (entry.Background?.ToLowerInvariant().Contains(filter) == true) ||
	// 		(entry.Foreground?.ToLowerInvariant().Contains(filter) == true) ||
	// 		(entry.Name?.ToLowerInvariant().Contains(filter) == true);
	// }

	public SolidColorBrush BdrBrush1
	{
		get => bdrBrush1;
		set
		{
			if (Equals(value, bdrBrush1)) return;
			bdrBrush1 = value;
			OnPropertyChanged();
		}
	}

	public SolidColorBrush BdrBrush2
	{
		get => bdrBrush2;
		set
		{
			if (Equals(value, bdrBrush2)) return;
			bdrBrush2 = value;
			OnPropertyChanged();
		}
	}

	public void ResetColorSamples()
	{
		BdrBrush1 = Brushes.DarkGray;
		BdrBrush2 = Brushes.DarkGray;
	}

	/// <summary>
	/// scroll the item to the center - for scolling by item in a listview
	/// </summary>
	public void ScrollItemToCenter(int idx)
	{
		if (scrollViewer == null) return;

		double pos = idx - (scrollViewer.ViewportHeight / 2);

		if (pos < 0) pos = 0;

		scrollViewer.ScrollToVerticalOffset(pos);
	}

	private T? getVisualChild<T>(Visual parent) where T : Visual
	{
		T? descendant = null;

		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			Visual child = (Visual)VisualTreeHelper.GetChild(parent, i);
			if (child is T typedChild)
			{
				return typedChild;
			}

			descendant = getVisualChild<T>(child);
			if (descendant != null) break;
		}

		return descendant;
	}


	/* removed
	private void showFolderList()
	{
		if (MruMgr.FoldersView.IsEmpty)
		{
			Debug.WriteLine("** View is empty");
		}
		else
		{
			Debug.WriteLine($"\n*** Folders View is not empty | {MruMgr.FoldersCount}");

			MruMgr.FoldersView.MoveCurrentToFirst();

			Debug.WriteLine($"*** 1 | {MruMgr.FoldersView.CurrentItem}");

			MruMgr.FoldersView.MoveCurrentToNext();
			Debug.WriteLine($"*** 2 | {MruMgr.FoldersView.CurrentItem}");
		}
	}

	private void showFilesList()
	{
		if (MruMgr.FilesView.IsEmpty)
		{
			Debug.WriteLine("** View is empty");
		}
		else
		{
			Debug.WriteLine($"\n*** Files View is not empty | {MruMgr.FilesCount}");

			MruMgr.FilesView.MoveCurrentToFirst();

			Debug.WriteLine($"*** 1 | {MruMgr.FilesView.CurrentItem}");

			MruMgr.FilesView.MoveCurrentToNext();
			Debug.WriteLine($"*** 2 | {MruMgr.FilesView.CurrentItem}");
		}
	}
	*/


	/* global mouse routines */

	[ObservableProperty]

	
	private string? mouseCoord;

	[ObservableProperty]
	private string? mouseMsg;

	[ObservableProperty]
	private SolidColorBrush? rectFill;

	[RelayCommand]
	private void BtnMouseTest_OnClick()
	{
		if (mouseMoving)
		{
			endMouseMove();
		}
		else
		{
			startMouseMove();
		}
	}


	private void OnMouseAction(object? sender, MouseHookEventArgs e)
	{
		// Low-level hooks run on the UI thread message loop, 
		// but explicitly updating elements safely:
		MouseCoord = $"Mouse Coord: X={e.X}, Y={e.Y}";

		switch (e.Message)
		{
		case WM_LBUTTONDOWN: // WM_LBUTTONDOWN
			MouseMsg = "Left Button Pressed";
			break;
		case WM_LBUTTONUP: // WM_LBUTTONUP
			MouseMsg = "Left Button Released";
			endMouseMove();
			break;
		case WM_RBUTTONDOWN: // WM_RBUTTONDOWN
			MouseMsg = "Right Button Pressed";
			break;
		case WM_RBUTTONUP: // WM_LBUTTONUP
			MouseMsg = "Right Button Released";
			break;
		case WM_MOUSEMOVE: // WM_MOUSEMOVE
			// Keep status simple during high volume movement
			if (MouseMsg != "Moving") MouseMsg = "Moving";
			updateRectColor(e.X, e.Y);
			break;
		}
	}

	private bool mouseMoving;

	private void updateRectColor(int x, int y)
	{
		System.Drawing.Color color = GetColorAt(x, y);
		RectFill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
	}

	private void startMouseMove()
	{
		_apiUtilities.MouseMoveHook.MouseAction += OnMouseAction;
		_apiUtilities.MouseMoveHook.Start();
		mouseMoving = true;
		MouseCoord = "mouse coord first";
		MouseMsg = "mouse start moving";
	}

	private void endMouseMove()
	{
		_apiUtilities.MouseMoveHook.Stop();
		_apiUtilities.MouseMoveHook.MouseAction -= OnMouseAction;
		_apiUtilities.MouseMoveHook.Dispose();
		mouseMoving = false;
		MouseCoord = "mouse coord last";
		MouseMsg = "mouse stop moving";
	}

	public void OnClosing()
	{
		// _apiUtilities.MouseClickHook?.Dispose();

		_apiUtilities.MouseMoveHook.Stop();
		_apiUtilities.MouseMoveHook.Dispose();
	}
}