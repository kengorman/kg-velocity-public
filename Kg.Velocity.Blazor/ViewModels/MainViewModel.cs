using Kg.Velocity.Blazor.Services;
using Kg.Velocity.Contracts.Catalogs;
using Kg.Velocity.Contracts.Trips;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Kg.Velocity.Blazor.ViewModels;

public class MainViewModel
{
    private readonly TripEvaluationService _tripEvaluationService;
    private readonly PersonaIdStore _personaIdStore;
    private readonly TripCatalogClient _catalogClient;
    private DateTimeOffset _startTime;
    private double _selectedSpeedMph;
    private int _evaluationRequestVersion;
    private int _summaryAnimationVersion;
    private int? _currentPersonaId;

    // Event to notify UI of state changes
    public event Action? StateChanged;

    public MainViewModel(
        TripEvaluationService tripEvaluationService,
        PersonaIdStore personaIdStore,
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

    // Properties
    public bool HasCalculated { get; set; }
    public bool IsCalculatingTrip { get; set; }
    public double SpeedMph { get; set; }
    public double PercentageOfLightSpeed { get; set; }
    public double DistanceMiles { get; set; }
    public double DistanceLightYears { get; set; }
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
    public bool IsDownloadingPoster { get; set; }
    public ObservableCollection<DestinationDto> Destinations { get; set; } = new();
    public List<SpeedPresetDto> SpeedPresets { get; set; } = [];
    
    private DestinationDto? _selectedDestination;
    public DestinationDto? SelectedDestination
    {
        get => _selectedDestination;
        set
        {
            _selectedDestination = value;
            if (value != null)
            {
                // Re-evaluate if speed is already set
                if (_selectedSpeedMph > 0)
                    _ = EvaluateTripAsync();
                else
                    ClearCalculations();
            }
            else
            {
                // Blank destination selected - clear calculations
                ClearCalculations();
            }
            NotifyStateChanged();
        }
    }

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
            {
                _selectedSpeedMph = value.Value;

                // Evaluate journey when speed is selected and destination exists
                if (SelectedDestination != null)
                    _ = EvaluateTripAsync();
                NotifyStateChanged();
            }
            else
            {
                // Blank speed selected - clear speed and calculations
                _selectedSpeedMph = 0;
                ClearCalculations();
            }
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    public Task RegenerateSummaryAsync()
    {
        // Regenerate by re-evaluating with the current inputs.
        // The API advances persona deterministically when PersonaId is provided.
        if (SelectedDestination == null) return Task.CompletedTask;
        if (_selectedSpeedMph <= 0) return Task.CompletedTask;

        return EvaluateTripAsync();
    }

    private async Task EvaluateTripAsync()
    {
        if (SelectedDestination == null) return;
        if (_selectedSpeedMph <= 0) return;

        var requestVersion = ++_evaluationRequestVersion;
        _summaryAnimationVersion++;

        var speedPreset = SpeedPresets.FirstOrDefault(p => System.Math.Abs(p.SpeedMph - _selectedSpeedMph) < 0.001);
        string speedName = speedPreset?.Name ?? $"{_selectedSpeedMph:N0} mph";

        HasCalculated = true;
        IsCalculatingTrip = true;
        IsDownloadingPoster = false;

        // Reset display values while calculating
        EarthTimeElapsed = "—";
        ShipTimeElapsed = "—";
        TimeDifference = "—";
        ArrivalDateString = "—";
        ArrivalShipDateString = "—";

        JourneySummary = "Calculating trip summary...";
        DisplayedJourneySummary = JourneySummary;
        PersonaName = "";
        PosterDataUrl = "";
        PosterGeneratedAtDisplay = "";
        NotifyStateChanged();

        try
        {
            // Prefer in-memory persona id for consistent rotation even if localStorage is unavailable.
            // Fall back to localStorage on first run (or after reload) to preserve continuity across sessions.
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

            var response = await _tripEvaluationService.EvaluateTripAsync(request);
            if (requestVersion != _evaluationRequestVersion) return;

            _currentPersonaId = response.PersonaId;
            await _personaIdStore.TrySetAsync(response.PersonaId);

            // Render authoritative server-computed values
            SpeedMph = response.Trip.SpeedMph;
            PercentageOfLightSpeed = response.Trip.PercentageOfLightSpeed;
            DistanceMiles = response.Trip.DistanceMiles;
            DistanceLightYears = response.Trip.DistanceLightYears;
            EarthTimeElapsed = response.Trip.EarthTimeFormatted;
            ShipTimeElapsed = response.Trip.ShipTimeFormatted;
            TimeDifference = response.Trip.TimeDifferenceFormatted;
            ArrivalDateString = response.Trip.ArrivedEarthTime;
            ArrivalShipDateString = response.Trip.ArrivedShipTime;

            JourneySummary = response.Summary;
            PersonaName = response.PersonaName;
            HasCalculated = true;
            IsCalculatingTrip = false;

            // Option A: API returns a PosterUrl. Download bytes and display them.
            if (!string.IsNullOrWhiteSpace(response.PosterUrl))
            {
                IsDownloadingPoster = true;
                NotifyStateChanged();

                try
                {
                    var posterBytes = await _tripEvaluationService.GetPosterBytesAsync(response.PosterUrl);
                    if (requestVersion != _evaluationRequestVersion) return;

                    // For now the server returns SVG bytes. Encode as a data URL for display + download.
                    PosterDataUrl = "data:image/svg+xml;base64," + Convert.ToBase64String(posterBytes);

                    var nowLocal = DateTimeOffset.Now.ToLocalTime();
                    PosterGeneratedAtDisplay = nowLocal.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    PosterFileName = $"velocity-poster-{nowLocal:yyyyMMdd-HHmmss}.svg";
                }
                catch
                {
                    // Trip results should still render even if the poster can't be fetched.
                    PosterDataUrl = "";
                    PosterGeneratedAtDisplay = "";
                }
                finally
                {
                    IsDownloadingPoster = false;
                }
            }

            NotifyStateChanged();

            _ = AnimateSummaryAsync(
                summary: JourneySummary,
                requestVersion: requestVersion,
                animationVersion: _summaryAnimationVersion);
        }
        catch (Exception ex)
        {
            if (requestVersion != _evaluationRequestVersion) return;

            IsCalculatingTrip = false;
            JourneySummary = ex.Message;
            DisplayedJourneySummary = JourneySummary;
            PersonaName = "System";
            HasCalculated = true;
            NotifyStateChanged();
        }
    }

    private async Task AnimateSummaryAsync(string summary, int requestVersion, int animationVersion)
    {
        // If summary is empty, just mirror it.
        if (string.IsNullOrEmpty(summary))
        {
            DisplayedJourneySummary = "";
            NotifyStateChanged();
            return;
        }

        // Reveal summary word-by-word while preserving whitespace/punctuation.
        // Tokenization returns alternating word/punctuation and whitespace tokens.
        var tokens = Regex.Matches(summary, @"(\s+|\S+)")
            .Select(m => m.Value)
            .ToArray();

        DisplayedJourneySummary = "";
        NotifyStateChanged();

        var sb = new StringBuilder(summary.Length);

        foreach (var token in tokens)
        {
            // Cancel if a new trip evaluation started, or a newer animation began.
            if (requestVersion != _evaluationRequestVersion) return;
            if (animationVersion != _summaryAnimationVersion) return;

            sb.Append(token);
            DisplayedJourneySummary = sb.ToString();
            NotifyStateChanged();

            // Delay only after non-whitespace tokens to approximate "100ms per word".
            if (!string.IsNullOrWhiteSpace(token))
            {
                await Task.Delay(15);
            }
        }
    }

    /// <summary>
    /// Clears all calculation results without changing dropdown selections.
    /// </summary>
    private void ClearCalculations()
    {
        HasCalculated = false;
        IsCalculatingTrip = false;
        IsDownloadingPoster = false;
        _startTime = DateTimeOffset.Now;
        _summaryAnimationVersion++;
        _currentPersonaId = null;
        
        // Update display properties to reflect cleared state
        UpdatePropertiesWithoutNotification();
        JourneySummary = "";
        DisplayedJourneySummary = "";
        PosterDataUrl = "";
        PosterFileName = "velocity-poster.svg";
        PosterGeneratedAtDisplay = "";
        
        NotifyStateChanged();
    }
}
