using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Kg.Velocity.Math;

namespace Velocity;

internal partial class App
{
    private const int VK_W = 0x57;
    private const int VK_X = 0x58;
    private const int VK_S = 0x53;
    private const int VK_Q = 0x51;

    [LibraryImport("user32.dll")]
    private static partial short GetAsyncKeyState(int vKey);

    public void Run()
    {
        Console.CursorVisible = false;
        Console.Clear();

        var state = new SimulationState();
        var engine = new SimulationEngine(state);

        var stopwatch = Stopwatch.StartNew();
        double lastElapsedSeconds = 0.0;
        DateTime startDateTime = DateTime.Now;

        while (true)
        {
            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            double deltaSeconds = elapsedSeconds - lastElapsedSeconds;
            lastElapsedSeconds = elapsedSeconds;
            
            if (deltaSeconds <= 0)
            {
                Thread.Sleep(1);
                continue;
            }

            bool increaseHeld = IsKeyDown(VK_X);
            bool decreaseHeld = IsKeyDown(VK_W);
            bool slowHeld = IsKeyDown(VK_S);
            bool quitHeld = IsKeyDown(VK_Q);

            if (quitHeld)
            {
                Console.SetCursorPosition(0, 0);
                Console.CursorVisible = true;
                return;
            }

            // Update simulation
            engine.Update(deltaSeconds, increaseHeld, decreaseHeld, slowHeld);

            // Calculate relativistic effects for display
            double lorentzFactor = RelativisticPhysics.CalculateLorentzFactor(state.SpeedMph);

            Console.SetCursorPosition(0, 0);
            WriteStatus(
                state.SpeedMph,
                lorentzFactor,
                state.EarthTimeSeconds,
                state.ShipTimeSeconds,
                state.DistanceMiles,
                state.TargetDistanceMiles,
                startDateTime.AddSeconds(state.EarthTimeSeconds),
                startDateTime.AddSeconds(state.ShipTimeSeconds)
            );

            if (state.DestinationReached)
            {
                Console.WriteLine();
                Console.WriteLine("Destination reached! Press Q to quit.");
            }

            Thread.Sleep(20);
        }
    }

    private static bool IsKeyDown(int virtualKey)
    {
        return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
    }

    private static void WriteStatus(
        double currentSpeedMph,
        double gamma,
        double earthTimeSeconds,
        double shipTimeSeconds,
        double distanceMiles,
        double targetDistanceMiles,
        DateTime earthDateTime,
        DateTime shipDateTime)
    {
        Console.WriteLine("Velocity — Relativistic Travel Simulation");
        Console.WriteLine("Controls: X=Accelerate (exp), W=Decelerate (exp), hold S=Reduce rate, Q=Quit");
        Console.WriteLine();

        double percentC = RelativisticPhysics.CalculatePercentageOfLightSpeed(currentSpeedMph);
        double shipClockRate = RelativisticPhysics.CalculateShipClockRate(gamma);
        double distanceLightYears = RelativisticPhysics.MilesToLightYears(distanceMiles);
        double remainingMiles = targetDistanceMiles - distanceMiles > 0 ? targetDistanceMiles - distanceMiles : 0;

        Console.WriteLine($"Speed: {currentSpeedMph:N0} mph  ({percentC:F6}% of c)");
        Console.WriteLine($"Lorentz factor γ: {gamma:F9}  |  Ship clock rate: {shipClockRate:F9}× Earth");

        Console.WriteLine($"Distance traveled: {distanceMiles:N0} miles  ({distanceLightYears:N6} ly)");
        Console.WriteLine($"Remaining distance: {remainingMiles:N0} miles  ({RelativisticPhysics.MilesToLightYears(remainingMiles):N6} ly of {PhysicsConstants.TargetDistanceLightYears:N0} ly target)");

        Console.WriteLine($"Elapsed time — Earth: {FormatDuration(earthTimeSeconds)}   Ship: {FormatDuration(shipTimeSeconds)}");
        Console.WriteLine($"Date/Time — Earth: {earthDateTime:MM/dd/yyyy HH:mm:ss.fff}   Ship: {shipDateTime:MM/dd/yyyy HH:mm:ss.fff}");
    }

    private static string FormatDuration(double totalSeconds)
    {
        if (double.IsInfinity(totalSeconds) || double.IsNaN(totalSeconds)) return "N/A";
        var ts = TimeSpan.FromSeconds(totalSeconds);
        int days = ts.Days;
        return days > 0
            ? $"{days}d {ts:hh\\:mm\\:ss}"
            : ts.ToString("hh\\:mm\\:ss");
    }
}
