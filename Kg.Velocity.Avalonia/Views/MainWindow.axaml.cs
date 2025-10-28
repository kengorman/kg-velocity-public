using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Kg.Velocity.Avalonia.ViewModels;

namespace Kg.Velocity.Avalonia.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;
    private bool _isDragging;
    private Control? _trackContainer;

    public MainWindow()
    {
        InitializeComponent();

        // Set up update timer (50 FPS = 20ms interval)
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(20)
        };
        _timer.Tick += OnTimerTick;
        _timer.Start();

        // Handle keyboard events
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        // Skip updates while dragging
        if (!_isDragging)
        {
            ViewModel?.Update();
        }
    }

    public void SetTrackContainer(Control trackContainer)
    {
        _trackContainer = trackContainer;
    }

    private void OnTrackContainerInitialized(object? sender, EventArgs e)
    {
        if (sender is Control control)
        {
            _trackContainer = control;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (ViewModel == null) return;

        switch (e.Key)
        {
            case Key.X:
                ViewModel.SetIncreaseHeld(true);
                e.Handled = true;
                break;
            case Key.W:
                ViewModel.SetDecreaseHeld(true);
                e.Handled = true;
                break;
            case Key.S:
                ViewModel.SetSlowHeld(true);
                e.Handled = true;
                break;
            case Key.Q:
                Close();
                e.Handled = true;
                break;
        }
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (ViewModel == null) return;

        switch (e.Key)
        {
            case Key.X:
                ViewModel.SetIncreaseHeld(false);
                e.Handled = true;
                break;
            case Key.W:
                ViewModel.SetDecreaseHeld(false);
                e.Handled = true;
                break;
            case Key.S:
                ViewModel.SetSlowHeld(false);
                e.Handled = true;
                break;
        }
    }

    public void OnSpaceshipPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control && ViewModel != null && _trackContainer != null)
        {
            _isDragging = true;
            control.Cursor = new Cursor(StandardCursorType.Hand);
            e.Pointer.Capture(control);
            e.Handled = true;
        }
    }

    public void OnSpaceshipPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging && sender is Control control && ViewModel != null && _trackContainer != null)
        {
            // Get pointer position relative to track container
            var position = e.GetPosition(_trackContainer);
            double trackWidth = _trackContainer.Bounds.Width;

            if (trackWidth > 0)
            {
                // Calculate percentage (0-100)
                double percentage = (position.X / trackWidth) * 100.0;
                percentage = System.Math.Clamp(percentage, 0.0, 100.0);

                // Update ViewModel with new position
                ViewModel.UpdateFromDragPosition(percentage);
                
                // Manually trigger an update to refresh the UI
                ViewModel.Update();
            }

            e.Handled = true;
        }
    }

    public void OnSpaceshipPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging && sender is Control control)
        {
            _isDragging = false;
            control.Cursor = Cursor.Default;
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    public void OnSpaceshipPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (sender is Control control)
        {
            _isDragging = false;
            control.Cursor = Cursor.Default;
        }
    }
}
