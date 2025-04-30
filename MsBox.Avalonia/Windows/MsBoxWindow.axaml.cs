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
        Closing += MsBoxWindow_Closing;
    }

    private void MsBoxWindow_Closing(object sender, WindowClosingEventArgs e)
    {
        throw new NotImplementedException();
    }

    public async void CloseSafe()
    {
        Environment.Exit(-1);
    }

    public void ForceExit()
    {
        Environment.Exit(-1);
    }
}