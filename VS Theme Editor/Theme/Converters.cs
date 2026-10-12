using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace VS_Theme_Editor;

[ValueConversion(typeof(string), typeof(Color))]
public class StringToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s && !string.IsNullOrWhiteSpace(s))
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(s);
            }
            catch
            {
                return Colors.Transparent;
            }
        }
        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DoubleToIntegerConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is double d)
		{
			return (int)d;
		}
		return 0;
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is int i)
		{
			return (double)i;
		}
		return 0.0;
	}
}

[ValueConversion(typeof(int[]), typeof(Color))]
public class ColorComponentsToColor : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length != 4 || !values.All(v => v is double)) return Color.FromRgb(255, 255, 255);

		byte[] components = values.Select(v => (byte)(double)v).ToArray();
		
		int idx;

		if (parameter is string)
		{
			if (int.TryParse((string)parameter, out idx))
			{
				if (idx > 9)
				{
					components[idx-10] = 0;
				}
				else
				{
					components[idx] = 255;
				}

			}
		}

		return Color.FromArgb(components[0], components[1], components[2], components[3]);
	}
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		return new object[] { };
	}
}
