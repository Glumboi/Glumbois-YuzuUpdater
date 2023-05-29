using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Newtonsoft.Json.Serialization;
using Wpf.Ui.Controls;
using YuzuUpdater.ViewModels;

namespace YuzuUpdater.Pages;
/// <summary>
/// Interaction logic for MainPage.xaml
/// </summary>
public partial class MainPage : UiPage
{
    private readonly MainPageViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        _viewModel = new MainPageViewModel(Snackbar);
        this.DataContext = _viewModel;
    }

    private void ProgressBar_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        Storyboard storyboard = (Storyboard)TryFindResource("FadeIn");
        storyboard.Begin();
    }

    private void ProgressBar_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (e.NewValue >= 100)
        {
            Storyboard storyboard = (Storyboard)TryFindResource("FadeOut");
            storyboard.Begin();
        }
    }
}