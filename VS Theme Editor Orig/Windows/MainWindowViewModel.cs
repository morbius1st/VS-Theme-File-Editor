using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Win32;
using UtilityLibrary;
using Wpf.Ui;
using Wpf.Ui.Controls;

using static VS_Theme_Editor_Orig.DataManager;

using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;



namespace VS_Theme_Editor_Orig;

public partial class MainWindowViewModel : ObservableObject
{

	public MainWindowViewModel(ISnackbarService snackbarService)
	{
		_snackbarService = snackbarService;

		MruMgr = new MruManager();

		_DataMgr.InitVsRootThemes();
		_DataMgr.InitVsBuiltinThemes();

		refreshVsData();

		// _DataMgr.TestGetInfo1();

		// showData();
	}

	public Theme? WorkingTheme
	{
		get => selectedThemeFile.Theme;
		set
		{
			selectedThemeFile.Theme = value;

			OnPropertyChanged();
		}
	}

	private ThemeFile selectedThemeFile;

	[ObservableProperty]
	private CategoryData? selectedCategory;

	private CategoryData? priorCategory;

	[ObservableProperty]
	private string visualStudioDirectory;

	[ObservableProperty]
	private string visualStudioDesc;

	[ObservableProperty]
	private Brush visualStudioDescBdr;

	[ObservableProperty]
	private string? themeFilePath;

	[ObservableProperty]
	private string? colorFilter;

	// [ObservableProperty]
	// private string themeStyleName = "Light";

	[ObservableProperty]
	private ThemeInfo themeInfo;

	public ICollectionView? FilteredEntries { get; private set; }

	[ObservableProperty]
	private	MruManager mruMgr;

	
	private DataManager _DataMgr => DataManager.Instance;

	
	private ThemeManager _ThemeMgr => ThemeManager.Instance;

	private readonly ISnackbarService _snackbarService;

	private MostRecentlyUsed mruFile = MostRecentlyUsed.MruEmpty();

	private MostRecentlyUsed mruFolder = MostRecentlyUsed.MruEmpty();
	

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

	// commands

	[RelayCommand]
	private void SaveTheme()
	{
		if (WorkingTheme is null || ThemeFilePath is null) return;

		PkgDefCompiler compiler = new PkgDefCompiler();
		compiler.Compile(WorkingTheme, ThemeFilePath);
	}

	[RelayCommand]
	private void SaveAsTheme()
	{
		if (WorkingTheme is null || ThemeFilePath is null) return;

		SaveFileDialog fileDialog = new Microsoft.Win32.SaveFileDialog();
		fileDialog.Filter = "PkgDef files (*.pkgdef)|*.pkgdef";
		fileDialog.DefaultExt = ".pkgdef";

		if (fileDialog.ShowDialog() == true)
		{
			PkgDefCompiler compiler = new PkgDefCompiler();
			compiler.Compile(WorkingTheme, fileDialog.FileName);
		}
	}		

	[RelayCommand]
	private void SelectTheme()
	{
		string defDir = mruFolder.FolderPath.IsVoid() ? string.Empty : mruFolder.FolderPath;

		OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog
		{
			Filter = "PkgDef files (*.pkgdef)|*.pkgdef",
			Title = "Select a theme file",
			AddToRecent = true,
			InitialDirectory = defDir
		};

		if (fileDialog.ShowDialog() == true)
		{
			string themeName = _ThemeMgr.GetThemeName(fileDialog.FileName);

			processSelectedTheme(fileDialog.FileName);

			setMruFile(MruMgr.Add(fileDialog.FileName, themeName));
		}

		mruFolder = MostRecentlyUsed.MruEmpty();
	}

	private void processSelectedTheme(string filePath)
	{
		selectedThemeFile = new (filePath);

		ThemeSource source = ThemeSource.TSRC_UKN;
		ThemeStyle style;
		// ThemeType type;

		Tuple < ThemeSource, ThemeStyle > themeInfo = _DataMgr.SelectVsData(filePath);

		string desc = _ThemeMgr.GetThemeSourceDesc(source);

		if (source == ThemeSource.TSRC_NONE) return;

		PkgDefDecompiler decompiler = new PkgDefDecompiler();

		WorkingTheme = decompiler.Decompile(filePath, source, desc);

		if (WorkingTheme == null) return;
		
		ThemeFilePath = filePath;

		if (source != ThemeSource.TSRC_USER_TBD_FALLBACK)
		{
			WorkingTheme.Fallback = WorkingTheme.Guid.ToString();
		}

		ThemeInfo =
			_ThemeMgr.ThemeData.LoadThemes(_DataMgr.VsId, WorkingTheme.Fallback!);

		refreshVsData();

		reSelectCategory();

		OnPropertyChanged(nameof(WorkingTheme));

		showData();
	}


	private void reSelectCategory()
	{
		if (priorCategory != null && WorkingTheme != null)
		{
			foreach (CategoryData cat in WorkingTheme.Categories)
			{
				if (cat.Name.Equals(priorCategory.Name, StringComparison.OrdinalIgnoreCase))
				{
					SelectedCategory = cat;
					OnSelectedCategoryChanged(cat);
					return;
				}
			}
		}

		FilteredEntries = null;
		OnPropertyChanged(nameof(FilteredEntries));
	}

	private void refreshVsData()
	{
		VisualStudioDirectory = _DataMgr.VsDirectory;
		VisualStudioDesc = _DataMgr.VsDesc;
		VisualStudioDescBdr = _DataMgr.VsDescBdr;
	}

	[RelayCommand]
	public void DeleteCategory(CategoryData parameter)
	{
		if (WorkingTheme is null) return;

		if (WorkingTheme.Categories.Contains(parameter))
		{
			WorkingTheme.Categories.Remove(parameter);
			OnPropertyChanged(nameof(WorkingTheme));
		}
		else
		{
			Debug.WriteLine("Category not found in theme.");
		}
	}

	[RelayCommand]
	public void AddCategory(string categoryName)
	{
		if (WorkingTheme is null) return;
		if (string.IsNullOrWhiteSpace(categoryName))
		{
			Debug.WriteLine("Category name cannot be empty.");
			return;
		}

		CategoryData newCategory = new CategoryData
		{
			Name = categoryName,
			Entries = new List<ThemeColorEntry>()
		};
		WorkingTheme.Categories.Add(newCategory);
		OnPropertyChanged(nameof(WorkingTheme));

		SelectedCategory = newCategory;
		OnPropertyChanged(nameof(SelectedCategory));
	}

	[RelayCommand]
	private void ChooseBaseTheme(ThemeStyle baseTheme)
	{
		Dictionary<Guid, ThemeInfo> vsRootThemes =
			_ThemeMgr.ThemeData.ThemeInfoList[_DataMgr.VsId];

		foreach ((Guid key, ThemeInfo vst) in vsRootThemes)
		{
			if (vst.Style == baseTheme)
			{
				WorkingTheme!.Fallback = key.ToString();
				ThemeInfo =
					_ThemeMgr.ThemeData.LoadThemes(_DataMgr.VsId, WorkingTheme.Fallback);

				showData();
			}
		}
	}

	[RelayCommand]
	public async Task TestTheme()
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
			OpenFolderDialog folderDialog = new Microsoft.Win32.OpenFolderDialog()
			{
				Title = "Select Visual Studio Community 2022 Directory (/Microsoft Visual Studio/2022/Community)",
				Multiselect = false
			};
			if (folderDialog.ShowDialog() == true)
			{
				string? directoryName = folderDialog.FolderName;
				VisualStudioDirectory = folderDialog.FolderName ?? string.Empty;
			}
			else
			{
				return; // User cancelled the dialog
			}
		}

		string VSExe = Path.Combine(VisualStudioDirectory, @"Common7\IDE\devenv.exe");
		string VSThemeDir = @"Common7\IDE\CommonExtensions\Platform";

		if (!File.Exists(VSExe))
		{
			MessageBox.Show("Visual Studio executable not found. Please ensure the path is correct.", "Error");
			return;
		}

		if (!Directory.Exists(Path.Combine(VisualStudioDirectory, VSThemeDir)))
		{
			MessageBox.Show("Visual Studio theme directory not found. Please ensure the path is correct.", "Error");
			return;
		}

		string outputFilePath = Path.Combine(VisualStudioDirectory, VSThemeDir, "ThemeTest.pkgdef");

		PkgDefCompiler compiler = new PkgDefCompiler();
		compiler.Compile(WorkingTheme, outputFilePath);

		Process updateConfigProcess = Process.Start(VSExe, "/updateconfiguration");

		_snackbarService.Show("Testing Theme", "Updating Visual Studio Configuration. Please wait...", Wpf.Ui.Controls.ControlAppearance.Info, null, TimeSpan.FromSeconds(15));

		await updateConfigProcess.WaitForExitAsync();
		_snackbarService.Show("Testing Theme", "Close Visual Studio to Continue", Wpf.Ui.Controls.ControlAppearance.Success, null, TimeSpan.FromSeconds(1000));


		if (updateConfigProcess.ExitCode != 0) throw new Exception("VS Failed to Update Config");
		await Task.Delay(1000); // Wait a bit to ensure the config is updated

		Process runner = Process.Start(VSExe);

		runner.WaitForExit();
		_snackbarService.Show("Testing Theme", "Visual Studio Closed. You can now edit the theme.", Wpf.Ui.Controls.ControlAppearance.Success, null, TimeSpan.FromSeconds(10));

		File.Delete(outputFilePath);
	}

	partial void OnSelectedCategoryChanged(CategoryData? value)
	{
		if (value != null)
		{
			priorCategory = value;

			FilteredEntries = CollectionViewSource.GetDefaultView(value.Entries);
			FilteredEntries.Filter = FilterPredicate;
			OnPropertyChanged(nameof(FilteredEntries));
		}
	}

	partial void OnColorFilterChanged(string? value)
	{
		FilteredEntries?.Refresh();
	}

	private bool FilterPredicate(object obj)
	{
		if (obj is not ThemeColorEntry entry)
			return false;
		if (string.IsNullOrWhiteSpace(ColorFilter))
			return true;
		string filter = ColorFilter.Trim().ToLowerInvariant();
		return (entry.Background?.ToLowerInvariant().Contains(filter) == true) ||
			(entry.Foreground?.ToLowerInvariant().Contains(filter) == true) ||
			(entry.Name?.ToLowerInvariant().Contains(filter) == true);
	}

	[RelayCommand]
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

	private void showData()
	{
		if (_DataMgr.VsData.VsId.Equals(DataManager.VSID_UKN))
		{
			Debug.WriteLine("invalid VsData (is empty)");
			return;
		}

		Debug.WriteLine($"*** vsid {_DataMgr.VsData.VsId} | vs version {_DataMgr.VsData.Version} / {_DataMgr.VsData.VersionType}");

		if (WorkingTheme != null) Debug.WriteLine($"** working theme {WorkingTheme.Name} | {WorkingTheme.Guid} | {WorkingTheme.Fallback}");

		Debug.WriteLine($"** VsThemeInfo   {ThemeInfo.Name} | {ThemeInfo.Guid} | {ThemeInfo.Style} | {ThemeInfo.IsInit}");
		
		if (ThemeInfo.ThemeList != null )
		{
			Debug.WriteLine($"** VsThemeInfo   {ThemeInfo.ThemeList.Count}");

			foreach (ThemeFile vtf in ThemeInfo.ThemeList)
			{
				Debug.WriteLine($"** Path: {vtf.FilePath} | Theme: {vtf.Theme?.Name} | Count: {vtf.Theme?.Categories.Count} | Guid: {vtf.Theme?.Guid}");
			}
		}

	}

}