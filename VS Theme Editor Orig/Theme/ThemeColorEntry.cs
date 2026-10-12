using CommunityToolkit.Mvvm.ComponentModel;

namespace VS_Theme_Editor_Orig;

public partial class ThemeColorEntry : ObservableObject
{
    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private byte backgroundType;

    [ObservableProperty]
    private string background;

    [ObservableProperty]
    private byte foregroundType;

    [ObservableProperty]
    private string foreground;

	[ObservableProperty]
	private DataManager.ThemeSource source;

	[ObservableProperty]
	private string sourceDesc;

	[ObservableProperty]
	private bool isEnabled;
}
