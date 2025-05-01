using Avalonia.Controls;
using Avalonia.Threading;

namespace MsBox.Avalonia.Windows;

public partial class MsBoxWindow : Window
{
    public MsBoxWindow()
    {
        InitializeComponent();
        ShowInTaskbar = false;
        CanResize = false;
    }

    public async void CloseSafe()
    {
        await Dispatcher.UIThread.InvokeAsync(Close);
    }

    public void ForceExit()
    {
        Environment.Exit(-1);
    }
}