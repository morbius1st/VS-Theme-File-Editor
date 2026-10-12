using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Windows.Data;
using UtilityLibrary;

namespace VS_Theme_Editor;

public partial class Theme: ObservableObject
{

	public Theme(string name)
	{
		this.name = name;
	}

	[ObservableProperty]
	private Guid? guid;

	[ObservableProperty]
	private string? name;

	[ObservableProperty]
	private string? slug;

	[ObservableProperty]
	private string? fallback;

	[ObservableProperty]
	private ObservableDictionaryEx<string, CategoryData> categories = new ();

	[ObservableProperty]
	private ICollectionView? categoriesView;

	// some statistical data

	[ObservableProperty]
	private int originalCategoryCount;

	[ObservableProperty]
	private int originalTotalColorEntryCount;

	[ObservableProperty]
	private int addedCategoryCountTotal;

	[ObservableProperty]
	private int addedColorEntryCountTotal;


	public void SetCategoryCount()
	{
		OriginalCategoryCount = Categories.Count;
		AddedCategoryCountTotal = 0;
		OriginalTotalColorEntryCount = 0;

		foreach ((string _ , CategoryData cd) in Categories)
		{
			cd.SetColorEntryCount();

			OriginalTotalColorEntryCount += cd.OriginalColorThemeEntryCount;
		}
	}

	public void UpdateAddedCount()
	{
		AddedCategoryCountTotal = Categories.Count - OriginalCategoryCount;

		AddedColorEntryCountTotal = 0;

		foreach ((string _ , CategoryData cd) in Categories)
		{
			cd.UpdateAddedColorEntryCount();

			AddedColorEntryCountTotal += cd.AddedColorEntryCount;
		}
	}

	public void Sort()
	{
		// Categories = new ObservableDictionaryEx<string, CategoryData>(Categories.OrderBy(kvp => kvp.Key));

		CategoriesView = CollectionViewSource.GetDefaultView(Categories);

		if (CategoriesView != null)
		{
			CategoriesView.SortDescriptions.Clear();
			CategoriesView.SortDescriptions.Add(new SortDescription("Value.Name", ListSortDirection.Ascending));
		}

		OnPropertyChanged(nameof(CategoriesView));
	}

	public override string ToString()
	{
		return $"{Name} | Categories [ {Categories.Count} ]";
	}
}