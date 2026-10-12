// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             Behaviors.cs
// Created:      2026-09-03 (20:09)

using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using Wpf.Ui.Controls;

namespace VS_Theme_Editor;

public class HexInputBehavior : Behavior<TextBox>
{
	private char[] hexChars = new []{ 
		'0', '1', '2', '3', 
		'4', '5', '6', '7', '8', '9', 
		'A', 'B', 'C', 'D', 'E', 'F',
		'a', 'b', 'c', 'd', 'e', 'f' };

	protected override void OnAttached()
	{
		base.OnAttached();

		AssociatedObject.PreviewTextInput += OnPreviewTextInput;
		AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
		DataObject.AddPastingHandler(AssociatedObject, OnPaste);
	}

	private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		int l = ((TextBox)sender).Text.Length;

		// set to true if skip this input
		e.Handled = l == 2 || 
			!e.Text.All(c => hexChars.Contains(c));
		
	}

	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		// Block the space bar since spaces aren't caught by PreviewTextInput
		if (e.Key == Key.Space)
		{
			e.Handled = true;
		}
	}

	private void OnPaste(object sender, DataObjectPastingEventArgs e)
	{
		if (e.DataObject.GetDataPresent(DataFormats.Text))
		{
			string text = (string)e.DataObject.GetData(DataFormats.Text);
			
			if (text.Length > 2 || !text.All(c => hexChars.Contains(c)))
			{
				e.CancelCommand(); // Block the paste action entirely
			}
		}
		else
		{
			e.CancelCommand();
		}
	}
}