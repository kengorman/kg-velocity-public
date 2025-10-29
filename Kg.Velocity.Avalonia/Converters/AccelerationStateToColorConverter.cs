using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Kg.Velocity.Avalonia.Converters;

/// <summary>
/// Converts acceleration state to color: green when accelerating, red when decelerating, default otherwise.
/// </summary>
public class AccelerationStateToColorConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && 
            values[0] is bool isAccelerating && 
            values[1] is bool isDecelerating)
        {
            if (isAccelerating)
                return new SolidColorBrush(Color.Parse("#69f0ae")); // Green
            else if (isDecelerating)
                return new SolidColorBrush(Color.Parse("#ef5350")); // Red
            else
                return new SolidColorBrush(Color.Parse("#e0e6ed")); // Default
        }
        return new SolidColorBrush(Color.Parse("#e0e6ed")); // Default
    }
}

