# TripPosterService - AI Integration Guide

## What Changed

### Added to the Service

1. **JourneyEvent Model**
   ```csharp
   public class JourneyEvent
   {
       public string Text { get; set; } = string.Empty;
       public string? Description { get; set; }
   }
   ```

2. **Mock Event Generator** (temporary)
   - `GenerateMockEvents()` - Returns 3-6 events based on seed
   - Will be replaced with AI service call

3. **Scriban Integration**
   - Events converted to `ScriptArray` with `ScriptObject` items
   - Text properly escaped with `Esc()` for SVG safety
   - Passed to template as `events` variable

## Current Behavior (Mock)

The service generates 3-6 events deterministically based on the nonce seed:
- Seed ending in 0-3 → 3 events
- Seed ending in 4-7 → 4 events  
- Seed ending in 8-9 → 5-6 events

This proves the dynamic rendering works before AI integration.

## Future: AI Integration

### Step 1: Create AI Service Interface

```csharp
public interface IJourneyEventGenerator
{
    Task<List<JourneyEvent>> GenerateEventsAsync(
        string destination, 
        string speed, 
        CancellationToken cancellationToken = default);
}
```

### Step 2: Implement with Your AI Provider

Example with Anthropic Claude API:

```csharp
public class ClaudeJourneyEventGenerator : IJourneyEventGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public async Task<List<JourneyEvent>> GenerateEventsAsync(
        string destination, 
        string speed, 
        CancellationToken cancellationToken)
    {
        var prompt = $@"Generate 4-6 interesting waypoints for a space journey 
from Earth to {destination} traveling at {speed}.

Return ONLY valid JSON with this exact structure (no markdown, no explanation):
{{
  ""events"": [
    {{
      ""text"": ""Brief event name (3-5 words)"",
      ""description"": ""One sentence detail""
    }}
  ]
}}

Make events realistic but imaginative. Mix cosmic phenomena, navigation challenges, 
and interesting encounters. Keep it grounded, not ridiculous.";

        var request = new
        {
            model = "claude-sonnet-4-20250514",
            max_tokens = 1000,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            "https://api.anthropic.com/v1/messages",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ClaudeResponse>();
        
        // Parse the JSON from Claude's response
        var jsonText = result.Content.First(c => c.Type == "text").Text;
        var eventData = JsonSerializer.Deserialize<EventsResponse>(jsonText);
        
        return eventData.Events.Select(e => new JourneyEvent
        {
            Text = e.Text,
            Description = e.Description
        }).ToList();
    }

    private class ClaudeResponse
    {
        public List<ContentBlock> Content { get; set; }
    }

    private class ContentBlock
    {
        public string Type { get; set; }
        public string Text { get; set; }
    }

    private class EventsResponse
    {
        public List<JourneyEvent> Events { get; set; }
    }
}
```

### Step 3: Update TripPosterService

Replace `GenerateMockEvents()` with:

```csharp
public class TripPosterService
{
    private readonly IJourneyEventGenerator _eventGenerator;

    public TripPosterService(IJourneyEventGenerator eventGenerator)
    {
        _eventGenerator = eventGenerator;
    }

    public async Task<byte[]> GeneratePosterAsync(HttpRequest httpRequest)
    {
        // ... existing code ...

        // Replace mock with AI call
        var events = await _eventGenerator.GenerateEventsAsync(
            destination, 
            speed, 
            httpRequest.HttpContext.RequestAborted);

        // ... rest of existing code ...
    }
}
```

### Step 4: Register in DI Container

In your `Program.cs` or `Startup.cs`:

```csharp
// For now (mock)
builder.Services.AddSingleton<IJourneyEventGenerator, MockJourneyEventGenerator>();

// Later (real AI)
builder.Services.AddHttpClient<IJourneyEventGenerator, ClaudeJourneyEventGenerator>()
    .ConfigureHttpClient(client =>
    {
        client.DefaultRequestHeaders.Add("x-api-key", builder.Configuration["Anthropic:ApiKey"]);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    });
```

## Error Handling Strategies

### Strategy 1: Fallback to Mock on AI Failure

```csharp
try
{
    events = await _eventGenerator.GenerateEventsAsync(destination, speed, ct);
}
catch (Exception ex)
{
    _logger.LogError(ex, "AI event generation failed, using fallback");
    events = GenerateMockEvents(destination, seed);
}
```

### Strategy 2: Return Empty Events (No Branches)

```csharp
try
{
    events = await _eventGenerator.GenerateEventsAsync(destination, speed, ct);
}
catch (Exception ex)
{
    _logger.LogError(ex, "AI event generation failed, rendering without branches");
    events = new List<JourneyEvent>(); // Template handles empty gracefully
}
```

### Strategy 3: Cache Previous Results

```csharp
private readonly IMemoryCache _cache;

var cacheKey = $"events_{destination}_{speed}";
if (!_cache.TryGetValue(cacheKey, out List<JourneyEvent> events))
{
    events = await _eventGenerator.GenerateEventsAsync(destination, speed, ct);
    _cache.Set(cacheKey, events, TimeSpan.FromHours(1));
}
```

## Testing the Integration

### Unit Test Example

```csharp
[Fact]
public async Task GeneratePoster_WithAIEvents_RendersCorrectly()
{
    // Arrange
    var mockGenerator = new Mock<IJourneyEventGenerator>();
    mockGenerator.Setup(x => x.GenerateEventsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<JourneyEvent>
        {
            new() { Text = "Test Event 1", Description = "Description 1" },
            new() { Text = "Test Event 2", Description = "Description 2" }
        });

    var service = new TripPosterService(mockGenerator.Object);
    var request = CreateMockHttpRequest("Andromeda", "0.9c", "ABC123");

    // Act
    var result = await service.GeneratePosterAsync(request);

    // Assert
    var svg = Encoding.UTF8.GetString(result);
    Assert.Contains("Test Event 1", svg);
    Assert.Contains("Description 1", svg);
}
```

## Performance Considerations

### Async Generation
Since AI calls can take 1-3 seconds:
- Change method signature to `async Task<byte[]>`
- Update controller to await the call
- Consider timeout policies (e.g., 5 second max)

### Caching Strategy
```csharp
// Cache by destination+speed combination
var cacheKey = $"poster_{destination}_{speed}_{nonce}";

// Option 1: Cache final SVG bytes (fastest)
if (_cache.TryGetValue(cacheKey, out byte[] cachedBytes))
    return cachedBytes;

// Option 2: Cache just the events (allows color variation)
var eventsCacheKey = $"events_{destination}_{speed}";
if (_cache.TryGetValue(eventsCacheKey, out List<JourneyEvent> cachedEvents))
    events = cachedEvents;
```

## Migration Checklist

- [ ] Create `IJourneyEventGenerator` interface
- [ ] Implement mock generator (for testing without AI costs)
- [ ] Implement real AI generator (Claude/GPT/etc.)
- [ ] Add DI registration
- [ ] Update `GeneratePoster` signature to async
- [ ] Update controller to await
- [ ] Add error handling/fallback
- [ ] Add logging for AI calls
- [ ] Add caching (optional but recommended)
- [ ] Write integration tests
- [ ] Monitor AI costs and response times

## Notes

- Current mock approach works perfectly for development
- No changes needed to the SVG template
- Events are properly escaped in C# before passing to Scriban
- Empty events array is handled gracefully (no branches render)
