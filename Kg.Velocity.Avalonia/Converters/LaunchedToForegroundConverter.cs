using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Kg.Velocity.Avalonia.Converters;

/// <summary>
/// Converts IsLaunched boolean to foreground color.
/// When launched (true), returns gray. When not launched (false), returns normal light color.
/// </summary>
public class LaunchedToForegroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isLaunched)
        {
            return isLaunched 
                ? new SolidColorBrush(Color.Parse("#9fa8b8")) // Gray when launched
                : new SolidColorBrush(Color.Parse("#e0e6ed")); // Normal light when not launched
        }
        return new SolidColorBrush(Color.Parse("#e0e6ed")); // Default
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

