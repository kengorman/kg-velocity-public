using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace Kg.Velocity.Avalonia.Converters;

/// <summary>
/// Converts a progress percentage (0-100) and track width to a position for the spaceship.
/// </summary>
public class ProgressToMarginConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && 
            values[0] is double percentage && 
            values[1] is double trackWidth && 
            trackWidth > 0)
        {
            // Calculate position: percentage of track width
            double position = (percentage / 100.0) * trackWidth;
            return position;
        }
        return 0.0;
    }
}

