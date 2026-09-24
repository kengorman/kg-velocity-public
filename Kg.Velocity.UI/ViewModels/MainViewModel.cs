using Kg.Velocity.UI.Services;
using Kg.Velocity.UI.Utilities;
using Kg.Velocity.Contracts.Catalogs;
using Kg.Velocity.Contracts.Trips;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Kg.Velocity.UI.ViewModels;

/// <summary>
/// Holds everything the main page shows: the destination and speed choices,
/// the trip results, and the AI-written summary, poster and mission log.
/// The page redraws whenever StateChanged fires.
/// </summary>
public class MainViewModel
{
    private readonly TripEvaluationService _tripEvaluationService;
    private readonly TripCatalogClient _catalogClient;
    private DateTimeOffset _startTime;
    private double _selectedSpeedMph;
    private int _evaluationRequestVersion;

    // Event to notify UI of state changes
    public event Action? StateChanged;

    /// <summary>
    /// Sets up the view model with the services it needs and blank starting values.
    /// </summary>
    public MainViewModel(
        TripEvaluationService tripEvaluationService,
        TripCatalogClient catalogClient)
    {
        _tripEvaluationService = tripEvaluationService;
        _catalogClient = catalogClient;
        _startTime = DateTimeOffset.Now;
        _selectedSpeedMph = 0;

        // Initialize display properties without triggering state change
        UpdatePropertiesWithoutNotification();
    }

    /// <summary>
    /// Fills in placeholder values for the first draw of the page,
    /// without telling the page anything changed.
    /// </summary>
    private void UpdatePropertiesWithoutNotification()
    {
        // Initialize properties to safe defaults for first render
        SpeedMph = 0;
        PercentageOfLightSpeed = 0;
        DistanceMiles = 0;
        DistanceLightYears = 0;
        EarthTimeElapsed = "00:00:00";
        ShipTimeElapsed = "00:00:00";
        TimeDifference = "0s";
        ArrivalDateString = "N/A";
        ArrivalShipDateString = "N/A";
    }

    /// <summary>
    /// Loads the destinations and speed presets from the API.
    /// Called once when the page first opens.
    /// </summary>
    public async Task InitializeAsync()
    {
        var destinations = await _catalogClient.GetDestinationsAsync();
        Destinations = new ObservableCollection<DestinationDto>(destinations);

        var presets = await _catalogClient.GetSpeedPresetsAsync();
        SpeedPresets = presets.ToList();

        SelectedDestination = null;
        NotifyStateChanged();
    }

    // Properties
    public bool HasCalculated { get; set; }
    public bool IsCalculatingTrip { get; set; }
    public bool IsGeneratingContent { get; set; }
    public bool ShowResults { get; set; }
    public bool ShowTimeChart { get; set; }
    public bool ShowSummary { get; set; }
    public bool ShowPoster { get; set; }
    public double SpeedMph { get; set; }
    public double PercentageOfLightSpeed { get; set; }
    public double DistanceMiles { get; set; }
    public double DistanceLightYears { get; set; }
    public double EarthTimeSeconds { get; set; }
    public double ShipTimeSeconds { get; set; }
    public string EarthTimeElapsed { get; set; } = "00:00:00";
    public string ShipTimeElapsed { get; set; } = "00:00:00";
    public string ArrivalDateString { get; set; } = "N/A";
    public string ArrivalShipDateString { get; set; } = "N/A";
    public string TimeDifference { get; set; } = "0s";
    public string JourneySummary { get; set; } = "";
    public string DisplayedJourneySummary { get; set; } = "";
    public string PersonaName { get; set; } = "";
    public string PosterDataUrl { get; set; } = "";
    public string PosterFileName { get; set; } = "velocity-poster.svg";
    public string PosterGeneratedAtDisplay { get; set; } = "";
    public string TravelLogDataUrl { get; set; } = "";
    public bool IsFetchingPosterBytes { get; set; }
    public ObservableCollection<DestinationDto> Destinations { get; set; } = new();
    public List<SpeedPresetDto> SpeedPresets { get; set; } = [];

    private DestinationDto? _selectedDestination;
    /// <summary>
    /// The destination the user has picked. Changing it clears any results on screen.
    /// </summary>
    public DestinationDto? SelectedDestination
    {
        get => _selectedDestination;
        set
        {
            _selectedDestination = value;
            if (HasCalculated)
                ClearResults();
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// True when the Go button should be enabled: a destination and speed are picked,
    /// nothing is loading, and no results are on screen.
    /// </summary>
    public bool CanStart => SelectedDestination != null
        && _selectedSpeedMph > 0
        && !IsCalculatingTrip
        && !IsGeneratingContent
        && !IsFetchingPosterBytes
        && !ShowResults;

    /// <summary>
    /// The speed the user has picked, in mph. Changing it clears any results on screen.
    /// Reads as null if the stored speed doesn't match any preset.
    /// </summary>
    public double? SelectedPresetSpeed
    {
        get
        {
            var match = SpeedPresets.FirstOrDefault(p => System.Math.Abs(p.SpeedMph - _selectedSpeedMph) < 0.001);
            return match?.SpeedMph;
        }
        set
        {
            if (value.HasValue && value.Value > 0)
                _selectedSpeedMph = value.Value;
            else
                _selectedSpeedMph = 0;

            if (HasCalculated)
                ClearResults();
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Tells the page something changed so it redraws.
    /// </summary>
    private void NotifyStateChanged() => StateChanged?.Invoke();

    /// <summary>
    /// Puts the results area back to its opening state and empties all result panels.
    /// </summary>
    private void ClearResults()
    {
        // Return main panel to opening state so user sees "The universe is vast. Light is slow." again
        HasCalculated = false;

        // Reset result values to placeholder
        DistanceMiles = 0;
        EarthTimeElapsed = "---";
        ShipTimeElapsed = "---";
        TimeDifference = "---";
        ArrivalDateString = "---";
        ArrivalShipDateString = "---";

        // Hide all result panels
        ShowResults = false;
        ShowTimeChart = false;
        ShowSummary = false;
        ShowPoster = false;

        // Clear their content
        DisplayedJourneySummary = "";
        PersonaName = "";
        PosterDataUrl = "";
        TravelLogDataUrl = "";
    }

    /// <summary>
    /// Runs a trip for the chosen destination and speed. Asks the API for the trip
    /// numbers and the AI content at the same time, shows the numbers (and starts the
    /// movie) as soon as they arrive, then the summary, then fetches the images.
    /// If the user starts a new trip before this one finishes, the old results are ignored.
    /// On error, the error message is shown in place of the summary.
    /// </summary>
    public async Task EvaluateTripAsync()
    {
        if (SelectedDestination == null) return;
        if (_selectedSpeedMph <= 0) return;

        var requestVersion = ++_evaluationRequestVersion;

        var speedPreset = SpeedPresets.FirstOrDefault(p => System.Math.Abs(p.SpeedMph - _selectedSpeedMph) < 0.001);
        string speedName = speedPreset?.Name ?? $"{_selectedSpeedMph:N0} mph";

        // Reset all visibility and state
        HasCalculated = true;
        IsCalculatingTrip = true;
        IsGeneratingContent = true;
        IsFetchingPosterBytes = true;
        ShowResults = false;
        ShowTimeChart = false;
        ShowSummary = false;
        ShowPoster = false;

        // Reset display values
        DistanceMiles = 0;
        EarthTimeElapsed = "—";
        ShipTimeElapsed = "—";
        TimeDifference = "—";
        ArrivalDateString = "—";
        ArrivalShipDateString = "—";
        JourneySummary = "";
        DisplayedJourneySummary = "";
        PersonaName = "";
        PosterDataUrl = "";
        TravelLogDataUrl = "";
        PosterGeneratedAtDisplay = "";

        NotifyStateChanged();

        try
        {
            _startTime = DateTimeOffset.Now;

            var request = new TripEvaluateRequest(
                Destination: SelectedDestination.Name,
                SpeedName: speedName,
                SpeedMph: _selectedSpeedMph,
                DistanceMiles: SelectedDestination.DistanceMiles,
                StartTime: _startTime
            );

            // Fire both API calls in parallel
            var computeTask = _tripEvaluationService.ComputeTripAsync(request);
            var contentTask = _tripEvaluationService.GenerateContentAsync(request);

            // Await physics (fast)
            var trip = await computeTask;
            if (requestVersion != _evaluationRequestVersion) return;

            // Store physics results
            SpeedMph = trip.SpeedMph;
            PercentageOfLightSpeed = trip.PercentageOfLightSpeed;
            DistanceMiles = trip.DistanceMiles;
            DistanceLightYears = trip.DistanceLightYears;
            EarthTimeSeconds = trip.EarthTimeSeconds;
            ShipTimeSeconds = trip.ShipTimeSeconds;
            EarthTimeElapsed = trip.EarthTimeFormatted;
            ShipTimeElapsed = trip.ShipTimeFormatted;
            TimeDifference = trip.TimeDifferenceFormatted;
            ArrivalDateString = trip.ArrivedEarthTime;
            ArrivalShipDateString = trip.ArrivedShipTime;
            IsCalculatingTrip = false;

            // Show carousel — movie is slide 1, other slides have placeholders
            ShowResults = true;
            NotifyStateChanged();

            // Await AI content (carousel is already visible with movie playing)
            var content = await contentTask;
            if (requestVersion != _evaluationRequestVersion) return;

            IsGeneratingContent = false;

            // Update summary in-place (Blazor re-renders the slide content)
            JourneySummary = content.Summary;
            DisplayedJourneySummary = content.Summary;
            PersonaName = content.PersonaName;
            ShowSummary = true;
            NotifyStateChanged();

            // Fetch poster and travel log in background (don't block)
            _ = FetchMediaInBackgroundAsync(content.PosterUrl, content.TravelLogUrl, requestVersion);
        }
        catch (Exception ex)
        {
            if (requestVersion != _evaluationRequestVersion) return;

            IsCalculatingTrip = false;
            IsGeneratingContent = false;
            IsFetchingPosterBytes = false;
            ShowResults = true;
            ShowSummary = true;
            JourneySummary = ex.Message;
            DisplayedJourneySummary = JourneySummary;
            PersonaName = "System";
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Downloads the poster and mission log images together, then shows them.
    /// Runs without holding up the rest of the trip.
    /// </summary>
    private async Task FetchMediaInBackgroundAsync(string? posterUrl, string? travelLogUrl, int requestVersion)
    {
        var posterTask = FetchPosterBytesAsync(posterUrl, requestVersion);
        var travelLogTask = FetchTravelLogBytesAsync(travelLogUrl, requestVersion);
        await Task.WhenAll(posterTask, travelLogTask);

        if (requestVersion != _evaluationRequestVersion) return;

        IsFetchingPosterBytes = false;
        ShowPoster = true;
        NotifyStateChanged();
    }

    /// <summary>
    /// Downloads the poster image and stores it in a form the page can show directly.
    /// Also sets a time-stamped file name for it. Leaves the poster empty if the download fails.
    /// </summary>
    private async Task FetchPosterBytesAsync(string? posterUrl, int requestVersion)
    {
        if (string.IsNullOrWhiteSpace(posterUrl)) return;

        try
        {
            var posterBytes = await _tripEvaluationService.GetPosterBytesAsync(posterUrl);
            if (requestVersion != _evaluationRequestVersion) return;

            PosterDataUrl = "data:image/svg+xml;base64," + Convert.ToBase64String(posterBytes);

            var nowLocal = DateTimeOffset.Now.ToLocalTime();
            PosterGeneratedAtDisplay = nowLocal.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            PosterFileName = $"velocity-poster-{nowLocal:yyyyMMdd-HHmmss}.svg";
        }
        catch
        {
            PosterDataUrl = "";
            PosterGeneratedAtDisplay = "";
        }
    }

    /// <summary>
    /// Downloads the mission log image and stores it in a form the page can show directly.
    /// Leaves it empty if the download fails.
    /// </summary>
    private async Task FetchTravelLogBytesAsync(string? travelLogUrl, int requestVersion)
    {
        if (string.IsNullOrWhiteSpace(travelLogUrl)) return;

        try
        {
            var travelLogBytes = await _tripEvaluationService.GetPosterBytesAsync(travelLogUrl);
            if (requestVersion != _evaluationRequestVersion) return;

            TravelLogDataUrl = "data:image/svg+xml;base64," + Convert.ToBase64String(travelLogBytes);
        }
        catch
        {
            TravelLogDataUrl = "";
        }
    }

}
