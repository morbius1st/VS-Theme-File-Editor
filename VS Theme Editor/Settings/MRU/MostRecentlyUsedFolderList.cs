// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             MostRecentlyUsedFolderList.cs
// Created:      2026-07-16 (20:07)

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using SettingsManager;

namespace VS_Theme_Editor;

[DataContract(Namespace = "")]
public class MostRecentlyUsedFolderList : MostRecentlyUsedList
{
	public string ListType => "Folder";

	public new void Save()
	{
		UserSettings.Data.MruFolderList = Items;
		UserSettings.Data.FolderListItemsMax = ItemLimit;
		
		base.Save();
	}

	public new void Load()
	{
		Read();

		base.Load();
	}

	private new void Read()
	{
		base.Read();

		Items = UserSettings.Data.MruFolderList ?? new ();
		ItemLimit = UserSettings.Data.FolderListItemsMax;
	}

	public override string ToString()
	{
		return $"ListType >{ListType}< | ItemLimit >{ItemLimit}< | Count >{Count}<";
	}
}