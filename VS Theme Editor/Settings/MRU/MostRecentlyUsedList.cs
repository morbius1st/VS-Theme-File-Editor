using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using SettingsManager;

namespace VS_Theme_Editor;

[DataContract(Namespace = "")]
public abstract class MostRecentlyUsedList : INotifyPropertyChanged
{
	protected MostRecentlyUsedList()
	{
		Items = new ObservableCollection<MostRecentlyUsed>();
	}

	public ICollectionView View { get; private set; }

	[DataMember(Order = 1)]
	protected ObservableCollection<MostRecentlyUsed> Items;

	[IgnoreDataMember]
	public int ItemLimit { get; set; } = 10;

	[IgnoreDataMember]
	public int Count => Items.Count;

	public void Add(MostRecentlyUsed item)
	{
		PrepList(item);

		Items.Insert(0, item);
	}

	public void Remove(MostRecentlyUsed item)
	{
		Items.Remove(item);
	}

	public void PrepList(MostRecentlyUsed item)
	{
		removeDuplicate(item);

		int extra = Items.Count - ItemLimit + 1;

		if (extra <= 0) return;

		for (int i = 0; i < extra; i++)
		{
			RemoveLast();
		}
	}

	private void removeDuplicate(MostRecentlyUsed item)
	{
		int idx = Items.IndexOf(item);

		if (idx >= 0)
		{
			Items.RemoveAt(idx);
		}
	}

	public void RemoveLast()
	{
		Items.RemoveAt(Items.Count - 1);
	}

	public void CreateView()
	{
		View = CollectionViewSource.GetDefaultView(Items);

		OnPropertyChanged(nameof(View));
	}

	public void Save()
	{
		
		UserSettings.Admin.Write();
	}

	public void Load()
	{
		CreateView();
	}

	protected void Read()
	{
		Items = new ObservableCollection<MostRecentlyUsed>();

		UserSettings.Admin.Read();

	}

	public bool Contains(string name)
	{
		return Items.Any(item => item.FilePath.Equals(name, StringComparison.OrdinalIgnoreCase));
	}

	public event PropertyChangedEventHandler PropertyChanged;
	
	[DebuggerStepThrough]
	private void OnPropertyChanged([CallerMemberName] string memberName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
	}

}