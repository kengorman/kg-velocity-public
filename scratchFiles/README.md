# Absurd Journey Template - Enhanced with Dynamic Branches

## Overview

This enhanced Scriban template adds dynamic branching waypoints to your cosmic journey visualization. Branches are generated from AI-returned event data and automatically positioned along the journey bar.

## What's New

### 1. Dynamic Branch System
- **Automatic positioning**: Branches space evenly from bottom to top based on event count
- **Alternating sides**: Left, right, left, right... pattern starting from the first event
- **Visual design**: 
  - Curved branch lines with gradient coloring
  - Terminal circles with soft glow effect
  - Text labels with optional descriptions

### 2. New Template Variables

```scriban
# Branch zone (safe area between smoke and destination eye)
branch_zone_top = bar_top_y + 80
branch_zone_bottom = bar_launch_y - 60
branch_zone_height = branch_zone_bottom - branch_zone_top

# Spacing calculation
event_count = events.size
branch_spacing = event_count > 1 ? branch_zone_height / (event_count - 1) : 0

# Branch geometry
branch_length = 140              # Horizontal extension from spine
branch_end_radius = 18           # Size of terminal circle
branch_stroke_width = 3.5        # Branch line thickness
text_offset_x = 32               # Text distance from circle
```

### 3. Required Data Structure

The template expects an `events` array in your data model:

```json
{
  "events": [
    {
      "text": "Event Name",           // Required: Main label
      "description": "Brief detail"   // Optional: Secondary text
    }
  ]
}
```

## Usage

### Step 1: Prepare Your Data

Create a data object with your journey parameters and events:

```json
{
  "destination_esc": "Andromeda Galaxy",
  "speed_esc": "0.99c",
  "generated_at": "2026-01-03T14:30:00Z",
  "nonce_esc": "7F3A9B2E",
  
  "accent": "#a855f7",
  "accent2": "#ec4899",
  
  "earth_cx": 400,
  "earth_cy": 1140,
  "earth_r": 85,
  
  "bar_height": 820,
  
  "events": [
    { "text": "First Waypoint", "description": "Optional details" },
    { "text": "Second Waypoint", "description": "More info" },
    { "text": "Third Waypoint" }
  ]
}
```

### Step 2: Render with Scriban

Using the Scriban.NET package:

```csharp
using Scriban;

// Load your template
var templateContent = File.ReadAllText("absurd_journey_enhanced.sbn");
var template = Template.Parse(templateContent);

// Load your data
var data = JsonSerializer.Deserialize<Dictionary<string, object>>(
    File.ReadAllText("your_data.json")
);

// Render
var result = template.Render(data);
File.WriteAllText("output.svg", result);
```

### Step 3: AI Integration

When calling your AI to generate journey events, request structured output:

**Example AI Prompt:**
```
Generate 4-6 interesting waypoints for a journey from Earth to [destination] at [speed].
Return as JSON with this structure:
{
  "events": [
    {
      "text": "Brief event name (3-5 words)",
      "description": "One sentence detail (optional)"
    }
  ]
}

Make events realistic but imaginative. Mix cosmic phenomena, navigation challenges, 
and interesting encounters. Keep it grounded, not ridiculous.
```

## Branch Behavior

### Positioning Logic
1. **First event** → Bottom-most position (closest to Earth)
2. **Last event** → Top-most position (closest to destination)
3. **Middle events** → Evenly distributed between

### Side Alternation
- Event index 0 (first) → **Left** side
- Event index 1 (second) → **Right** side  
- Event index 2 (third) → **Left** side
- ...and so on

### Visual Spacing
The template calculates safe zones automatically:
- **Top boundary**: 80px below the destination eye
- **Bottom boundary**: 60px above the smoke effect
- **Spacing**: `(zone_height) / (event_count - 1)`

For 1 event: centered in the zone  
For 2+ events: distributed from bottom to top

## Customization

### Adjust Branch Appearance

In the template's calculation block, modify:

```scriban
# Make branches longer/shorter
branch_length = 140  # Change this value

# Bigger/smaller end circles
branch_end_radius = 18  # Change this value

# Thicker/thinner branch lines
branch_stroke_width = 3.5  # Change this value

# Move text closer/farther from circles
text_offset_x = 32  # Change this value
```

### Change Branch Colors

The branches use gradients that reference your accent colors:
- `branchGradientLeft`: Flows from `accent` to `accent2`
- `branchGradientRight`: Flows from `accent` to `accent2`

These automatically match your journey's color scheme.

### Modify Safe Zones

Adjust where branches can appear:

```scriban
# More/less space below destination eye
branch_zone_top = bar_top_y + 80  # Increase to move branches down

# More/less space above smoke
branch_zone_bottom = bar_launch_y - 60  # Decrease to move branches down
```

## Example Outputs

### With 3 Events
```
    🎯 Destination
    │
    ├─── Event 3 (left)
    │
    ────┤ Event 2 (right)
    │
    ├─── Event 1 (left)
    │
    🔥 Earth Launch
```

### With 6 Events
```
    🎯 Destination
    │
    ├─── Event 6 (left)
    ────┤ Event 5 (right)
    ├─── Event 4 (left)
    ────┤ Event 3 (right)
    ├─── Event 2 (left)
    ────┤ Event 1 (right)
    │
    🔥 Earth Launch
```

## Troubleshooting

### Branches overlap with smoke/destination
- Adjust `branch_zone_top` and `branch_zone_bottom` values
- Reduce `branch_length` if branches extend too far

### Text is cut off
- Increase viewBox width: `viewBox="0 0 800 1200"` → `viewBox="0 0 1000 1200"`
- Reduce `branch_length`
- Reduce `text_offset_x`

### Too many events (crowded)
- Limit AI to 6-8 events maximum
- Increase `bar_height` for more vertical space
- Consider reducing branch elements (remove descriptions)

### Empty events array
The template handles this gracefully - no branches will render if `events` is empty or missing.

## What's Next

Consider adding:
1. **Branch categories**: Different colors/styles for different event types
2. **Icons**: SVG symbols at branch endpoints instead of plain circles
3. **Interactivity**: Hover states or click handlers (if rendering to interactive SVG)
4. **Animation**: Fade-in sequence as branches appear
5. **Timestamps**: Add time/distance markers along branches

## Files in This Package

- `absurd_journey_enhanced.sbn` - Enhanced Scriban template
- `example_data.json` - Sample data structure with 6 events
- `README.md` - This documentation

## Questions?

The template is heavily commented. Check the calculation blocks (marked with `{{~ ... ~}}`) for detailed explanations of each variable and formula.
