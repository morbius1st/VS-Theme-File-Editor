using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using UtilityLibrary;

namespace VS_Theme_Editor;

public partial class CategoryData : ObservableObject
{

	[ObservableProperty]
    private string? name;

	/// <summary>
	/// value parsed from the theme file
	/// </summary>
    [ObservableProperty]
    private Int32 dataLength;

	/// <summary>
	/// value parsed from the theme file
	/// </summary>
    [ObservableProperty]
    private Int32 headerLength;

	/// <summary>
	/// value parsed from the theme file
	/// </summary>
    [ObservableProperty]
    private Int32 categoryCount;

    [ObservableProperty]
    private Guid guid;

	public string SourceFileName { get; set; }

	[ObservableProperty]
	private ObservableDictionaryEx<string, ThemeColorEntry> entries = new ();

	[ObservableProperty]
	private ICollectionView? filteredEntriesView;

	private string? colorFilter;
	
	// some statistical data

	/// <summary>
	/// flag - one or more color entries were added
	/// </summary>
	[ObservableProperty]
	private bool colorEntryAdded;

	/// <summary>
	/// the original number of color entries in this category before merging
	/// </summary>
	[ObservableProperty]
	private int originalColorThemeEntryCount;

	/// <summary>
	/// the number of added entries due to being merged
	/// </summary>
	[ObservableProperty]
	private int addedColorEntryCount;

	/// <summary>
	/// set the original color entry count
	/// </summary>
	public void SetColorEntryCount()
	{
		OriginalColorThemeEntryCount = Entries.Count;
		AddedColorEntryCount = 0;
		ColorEntryAdded = false;
	}
	
	/// <summary>
	/// update the color entry count
	/// </summary>
	public void UpdateAddedColorEntryCount()
	{
		AddedColorEntryCount = Entries.Count - OriginalColorThemeEntryCount;
	}

	public void InitView()
	{
		if (FilteredEntriesView != null) return;

		StringComparer sc = StringComparer.OrdinalIgnoreCase;

		#pragma warning disable MVVMTK0034
		filteredEntriesView = CollectionViewSource.GetDefaultView(Entries.Values.OrderBy(e => e.Name, sc));
		filteredEntriesView.Filter = filterPredicate;
		#pragma warning restore MVVMTK0034

		OnPropertyChanged(nameof(FilteredEntriesView));
	}

	public void SetColorFilter(string? colorFilterString)
	{
		colorFilter = colorFilterString;
		FilteredEntriesView!.Refresh();
	}

	private bool filterPredicate(object obj)
	{
		if (obj is not ThemeColorEntry entry)
			return false;
		if (string.IsNullOrWhiteSpace(colorFilter))
			return true;
		string filter = colorFilter.Trim().ToLowerInvariant();

		return entry.Background?.ToLowerInvariant().Contains(filter) == true ||
			(entry.Foreground?.ToLowerInvariant().Contains(filter)) == true ||
			(entry.Name?.ToLowerInvariant().Contains(filter) == true);
	}

	public override string ToString()
	{
		return $"{Name} | entry count [ {Entries.Count} ]";
	}
}