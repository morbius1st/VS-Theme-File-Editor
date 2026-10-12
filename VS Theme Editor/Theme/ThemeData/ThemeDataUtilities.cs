// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             ThemeDataUtilities.cs
// Created:      2026-08-02 (20:08)

namespace VS_Theme_Editor;

public class ThemeDataUtilities
{
	public static ThemeDataUtilities Instance { get; private set; }

	static ThemeDataUtilities()
	{
		if (Instance == null)
		{
			Instance = new ThemeDataUtilities();
		}
	}

	private ThemeDataUtilities() { }


	// theme name

}