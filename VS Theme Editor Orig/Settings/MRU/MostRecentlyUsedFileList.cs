// Solution:     VS Theme Editor Orig
// Project:       VS Theme Editor Orig
// File:             MostRecentlyUsedFileList.cs
// Created:      2026-07-16 (20:07)

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using SettingsManager;

namespace VS_Theme_Editor_Orig;

[DataContract(Namespace = "")]
public class MostRecentlyUsedFileList : MostRecentlyUsedList
{
	public string ListType => "File";

	public new void Save()
	{
		UserSettings.Data.MruFileList = Items;
		UserSettings.Data.FileListItemsMax = ItemLimit;

		base.Save();
	}

	public new void Load()
	{
		Read();

		base.Load();
	}

	public new void Read()
	{
		base.Read();

		Items = UserSettings.Data.MruFileList ?? new ();
		ItemLimit = UserSettings.Data.FileListItemsMax;
	}

	public override string ToString()
	{
		return $"ListType >{ListType}< | ItemLimit >{ItemLimit}< | Count >{Count}<";
	}

}