using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;
using YuzuUpdater.Core;
using YuzuUpdater.Pages;
using YuzuUpdater.ViewModels;

namespace YuzuUpdater;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : UiWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        var page = (MainPage)ContentFrame.Content;
        var vm = (MainPageViewModel)page.DataContext;
        vm.SaveSettings(); 
        Networking.AssignWindow(this);
    }
}