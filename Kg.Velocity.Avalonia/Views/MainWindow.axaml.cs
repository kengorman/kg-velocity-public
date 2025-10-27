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
        ViewModel?.Update();
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
}
