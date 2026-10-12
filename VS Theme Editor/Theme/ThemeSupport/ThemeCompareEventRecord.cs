// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             ThemeCompareEvent.cs
// Created:      2026-08-13 (19:08)

namespace VS_Theme_Editor;


public record ThemeCompareEvent(
	// ReSharper disable once NotAccessedPositionalProperty.Global
	string EventKey, 
	ThemeData.ThemeCompareEventTypes Type, 
	string Index,
	string? CdName, 
	string? CeName, 
	// ReSharper disable once NotAccessedPositionalProperty.Global
	string FldName, 
	string?[]? EventData, 
	string? Comment);