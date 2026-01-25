using Kg.Velocity.UI.Services;
using Kg.Velocity.Contracts.Catalogs;
using Kg.Velocity.Contracts.Nasa;
using Kg.Velocity.Contracts.Trips;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Kg.Velocity.UI.ViewModels;

public class MainViewModel
{
    private readonly TripEvaluationService _tripEvaluationService;
    private readonly IPersonaIdStore _personaIdStore;
    private readonly TripCatalogClient _catalogClient;
    private DateTimeOffset _startTime;
    private double _selectedSpeedMph;
    private int _evaluationRequestVersion;
    private int? _currentPersonaId;

    // Event to notify UI of state changes
    public event Action? StateChanged;

    public MainViewModel(
        TripEvaluationService tripEvaluationService,
        IPersonaIdStore personaIdStore,
        TripCatalogClient catalogClient)
    {
        _tripEvaluationService = tripEvaluationService;
        _personaIdStore = personaIdStore;
        _catalogClient = catalogClient;
        _startTime = DateTimeOffset.Now;
        _selectedSpeedMph = 0;

        // Initialize display properties without triggering state change
        UpdatePropertiesWithoutNotification();
    }

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

    public async Task InitializeAsync()
    {
        var destinations = await _catalogClient.GetDestinationsAsync();
        Destinations = new ObservableCollection<DestinationDto>(destinations);

        var presets = await _catalogClient.GetSpeedPresetsAsync();
        SpeedPresets = presets.ToList();

        SelectedDestination = null;
        NotifyStateChanged();
    }

    // Phase messages for the three-beat ritual
    private const string Phase1Message = "Establishing distance...";
    private const string Phase2Message = "Reflecting on your journey...";
    private const string Phase3Message = "Marking the passage...";

    private const int Phase1MinMs = 600;
    private const int Phase2MinMs = 1000;
    private const int Phase3MinMs = 2000;

    // Properties
    public bool HasCalculated { get; set; }
    public bool IsCalculatingTrip { get; set; }
    public bool IsGeneratingContent { get; set; }
    public string PhaseMessage { get; set; } = "";
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

    // NASA Images bottom sheet state
    public bool IsImageSheetOpen { get; set; }
    public bool IsLoadingNasaImages { get; set; }
    public List<NasaImageDto> NasaImages { get; set; } = [];

    private DestinationDto? _selectedDestination;
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

    public bool CanStart => SelectedDestination != null
        && _selectedSpeedMph > 0
        && !IsCalculatingTrip
        && !IsGeneratingContent
        && !IsFetchingPosterBytes
        && !ShowResults;

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

    private void NotifyStateChanged() => StateChanged?.Invoke();

    private void ClearResults()
    {
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
    }

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

        // Phase 1: Establishing distance...
        PhaseMessage = Phase1Message;
        NotifyStateChanged();
        var phase1Start = DateTime.UtcNow;

        try
        {
            _currentPersonaId ??= await _personaIdStore.TryGetAsync();
            _startTime = DateTimeOffset.Now;

            var request = new TripEvaluateRequest(
                Destination: SelectedDestination.Name,
                SpeedName: speedName,
                SpeedMph: _selectedSpeedMph,
                DistanceMiles: SelectedDestination.DistanceMiles,
                StartTime: _startTime,
                PersonaId: _currentPersonaId
            );

            // Start both calls in parallel
            var computeTask = _tripEvaluationService.ComputeTripAsync(request);
            var contentTask = _tripEvaluationService.GenerateContentAsync(request);

            // Wait for calculations
            var trip = await computeTask;
            if (requestVersion != _evaluationRequestVersion) return;

            // Store results but don't show yet
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

            // Ensure minimum Phase 1 display time
            var phase1Elapsed = (DateTime.UtcNow - phase1Start).TotalMilliseconds;
            if (phase1Elapsed < Phase1MinMs)
                await Task.Delay(Phase1MinMs - (int)phase1Elapsed);
            if (requestVersion != _evaluationRequestVersion) return;

            // Transition: Start Phase 2 (don't show results yet - wait for all phases)
            PhaseMessage = Phase2Message;
            NotifyStateChanged();
            var phase2Start = DateTime.UtcNow;

            // Wait for content (summary + poster URL)
            var content = await contentTask;
            if (requestVersion != _evaluationRequestVersion) return;

            IsGeneratingContent = false;
            _currentPersonaId = content.PersonaId;
            await _personaIdStore.TrySetAsync(content.PersonaId);

            // Store summary but don't show yet
            JourneySummary = content.Summary;
            PersonaName = content.PersonaName;

            // Ensure minimum Phase 2 display time
            var phase2Elapsed = (DateTime.UtcNow - phase2Start).TotalMilliseconds;
            if (phase2Elapsed < Phase2MinMs)
                await Task.Delay(Phase2MinMs - (int)phase2Elapsed);
            if (requestVersion != _evaluationRequestVersion) return;

            // Transition: Start Phase 3 (don't show summary yet - wait for all phases)
            DisplayedJourneySummary = JourneySummary;
            PhaseMessage = Phase3Message;
            NotifyStateChanged();
            var phase3Start = DateTime.UtcNow;

            // Fetch poster and travel log bytes in parallel
            var posterTask = FetchPosterBytesAsync(content.PosterUrl, requestVersion);
            var travelLogTask = FetchTravelLogBytesAsync(content.TravelLogUrl, requestVersion);
            await Task.WhenAll(posterTask, travelLogTask);

            // Ensure minimum Phase 3 display time
            var phase3Elapsed = (DateTime.UtcNow - phase3Start).TotalMilliseconds;
            if (phase3Elapsed < Phase3MinMs)
                await Task.Delay(Phase3MinMs - (int)phase3Elapsed);
            if (requestVersion != _evaluationRequestVersion) return;

            // Transition: All phases complete - show everything at once
            IsFetchingPosterBytes = false;
            ShowResults = true;
            ShowSummary = true;
            ShowPoster = true;
            PhaseMessage = "";
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            if (requestVersion != _evaluationRequestVersion) return;

            IsCalculatingTrip = false;
            IsGeneratingContent = false;
            IsFetchingPosterBytes = false;
            PhaseMessage = "";
            ShowResults = true;
            ShowSummary = true;
            JourneySummary = ex.Message;
            DisplayedJourneySummary = JourneySummary;
            PersonaName = "System";
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Opens the destination images bottom sheet.
    /// </summary>
    public void OpenImageSheet()
    {
        if (SelectedDestination == null) return;

        IsImageSheetOpen = true;
        // TODO: Load embedded images for destination
        NotifyStateChanged();
    }

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

    /// <summary>
    /// Closes the destination images bottom sheet.
    /// </summary>
    public void CloseImageSheet()
    {
        IsImageSheetOpen = false;
        NotifyStateChanged();
    }
}
