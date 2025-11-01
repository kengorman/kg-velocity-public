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
            // Reserve space at the end for the rocket icon (about 15px)
            // This prevents the rocket from overlapping the destination text
            const double rocketReservedSpace = 15.0;
            double usableWidth = System.Math.Max(0, trackWidth - rocketReservedSpace);
            
            // Calculate position: percentage of usable track width
            double position = (percentage / 100.0) * usableWidth;
            return position;
        }
        return 0.0;
    }
}

