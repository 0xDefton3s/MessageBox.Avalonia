using System.Resources;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;

using DialogHostAvalonia;

using MsBox.Avalonia.Base;
using MsBox.Avalonia.ViewModels;
using MsBox.Avalonia.Windows;

namespace MsBox.Avalonia;

public class MsBox<V, VM, T> : IMsBox<T> where V : UserControl, IFullApi<T>, ISetCloseAction where VM : ISetFullApi<T>, IInput
{
    private readonly V _view;
    private readonly VM _viewModel;

    public string InputValue { get { return _viewModel.InputValue; } }

    public MsBox(V view, VM viewModel)
    {
        _view = view;
        _viewModel = viewModel;
    }

    /// <summary>
    /// Show messagebox depending on the type of application
    /// If application is SingleViewApplicationLifetime (Mobile or Browses) then show messagebox as popup
    /// If application is ClassicDesktopStyleApplicationLifetime (Desktop) then show messagebox as window
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public Task<T> ShowAsync()
    {
        if (Application.Current != null &&
            Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return ShowWindowAsync();
        }

        if (Application.Current != null &&
            Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime lifetime)
        {
            return ShowAsPopupAsync(lifetime.MainView as ContentControl);
        }

        throw new NotSupportedException("ApplicationLifetime is not supported");
    }

    /// <summary>
    ///  Show messagebox as window
    /// </summary>
    /// <returns></returns>
    public Task<T> ShowWindowAsync()
    {
        _viewModel.SetFullApi(_view);
        var window = new MsBoxWindow
        {
            Content = _view,
            DataContext = _viewModel
        };
        window.Closed += _view.CloseWindow;

        var tcs = new TaskCompletionSource<T>();

        _view.SetCloseAction(() =>
        {
            tcs.TrySetResult(_view.GetButtonResult());
            window.Close();
        });

        window.Show();
        return tcs.Task;
    }

    /// <summary>
    ///  Show messagebox as window with owner
    /// </summary>
    /// <param name="owner">Window owner </param>
    /// <returns></returns>
    public Task<T> ShowWindowDialogAsync(Window owner)
    {
        _viewModel.SetFullApi(_view);
        var window = new MsBoxWindow
        {
            Content = _view,
            DataContext = _viewModel
        };
        window.Closed += _view.CloseWindow;
        var tcs = new TaskCompletionSource<T>();

        _view.SetCloseAction(() =>
        {
            tcs.TrySetResult(_view.GetButtonResult());
            window.Close();
        });

        window.ShowDialog(owner);
        return tcs.Task;
    }

    private readonly string ClickAwayParam = "MsBoxIdentifier_Cancel";
    /// <summary>
    ///  Show messagebox as popup
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public Task<T> ShowAsPopupAsync(ContentControl owner)
    {
        DialogHostStyles style = null;
        if (!owner.Styles.OfType<DialogHostStyles>().Any())
        {
            style = [];
            owner.Styles.Add(style);
        }


        var parentContent = owner.Content;
        if(!owner.Resources.ContainsKey("DialogHostOverlayBackgroundMixinBrush")) owner.Resources.Add("DialogHostOverlayBackgroundMixinBrush", new SolidColorBrush { Color = Color.Parse("#000000"), Opacity = 0.3 });

        var dh = new DialogHost
        {
            Identifier = "MsBoxIdentifier" + Guid.NewGuid(),
            CornerRadius = new CornerRadius(20),
            Background=new SolidColorBrush(Colors.Transparent),
            BorderBrush = new SolidColorBrush(Colors.Transparent),
            BorderThickness=new Thickness(0),
            Padding=new Thickness(0),
            Margin=new Thickness(0),
            Effect=null,
            DialogMargin=new Thickness(0),
            BlurBackground=true,
            BlurBackgroundRadius = 50,
        };
        _viewModel.SetFullApi(_view);
        owner.Content = null;
        dh.Content = parentContent;

        dh.CloseOnClickAway = false;
        if (_viewModel is AbstractMsBoxViewModel abv) dh.CloseOnClickAway = abv.CloseOnClickAway;
        dh.CloseOnClickAwayParameter = ClickAwayParam;
        dh.DialogClosing += (ss, ee) =>
        {
            if (ee.Parameter?.ToString() == ClickAwayParam)
            {
                _view.Close();
            }
        };

        owner.Content = dh;
        var tcs = new TaskCompletionSource<T>();
        _view.SetCloseAction(() =>
        {
            var r = _view.GetButtonResult();

            if (dh.CurrentSession != null && dh.CurrentSession.IsEnded == false)
            {
                DialogHost.Close(dh.Identifier);
            }

            owner.Content = null;
            dh.Content = null;
            owner.Content = parentContent;
            if (style != null)
            {
                owner.Styles.Remove(style);
            }
            tcs.TrySetResult(r);
        });
        DialogHost.Show(_view, dh.Identifier);
        return tcs.Task;
    }

    /// <summary>
    /// Show messagebox as popup with owner
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public Task<T> ShowAsPopupAsync(Window owner)
    {
        return ShowAsPopupAsync(owner as ContentControl);
    }
}