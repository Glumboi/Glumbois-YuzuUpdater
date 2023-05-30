using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using YuzuUpdater.Core;

namespace YuzuUpdater.ViewModels;

internal class MainPageViewModel : ViewModelBase
{
    private IniParser _iniParser;

    private UpdaterSettings _settings;

    private Snackbar _notificationBar;

    public Snackbar NotificationBar
    {
        get => _notificationBar;
        set
        {
            SetProperty(ref _notificationBar, value);
        }
    }

    private bool _adminMode;

    public bool AdminMode
    {
        get => _adminMode;
        set
        {
            SetProperty(ref _adminMode, value);
            _settings.AdminMode = value;
        }
    }

    private bool _autoClose;

    public bool AutoClose
    {
        get => _autoClose;
        set
        {
            SetProperty(ref _autoClose, value);
            _settings.AutoClose = value;
        }
    }

    private List<YuzuVersion> _yuzuVersions = new List<YuzuVersion>();

    public List<YuzuVersion> YuzuVersions
    {
        get => _yuzuVersions;
        set => SetProperty(ref _yuzuVersions, value);
    }

    private bool _launchAfterUpdate;

    public bool LaunchAfterUpdate
    {
        get => _launchAfterUpdate;
        set
        {
            SetProperty(ref _launchAfterUpdate, value);
            _settings.LaunchAfterUpdate = value;
        }
    }

    private string _yuzuPath;

    public string YuzuPath
    {
        get => _yuzuPath;
        set
        {
            SetProperty(ref _yuzuPath, value);
            _settings.YuzuPath = value;
        }
    }

    private Visibility _progressBarVisibility = Visibility.Collapsed;

    public Visibility ProgressBarVisibility
    {
        get => _progressBarVisibility;
        set => SetProperty(ref _progressBarVisibility, value);
    }

    private int _progress;

    public int Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    private string _progressText;

    public string ProgressText
    {
        get => _progressText;
        set => SetProperty(ref _progressText, value);
    }

    private int _shownReleasesCount;

    public int ShownReleasesCount
    {
        get => _shownReleasesCount;
        set
        {
            SetProperty(ref _shownReleasesCount, value);
            _settings.ShownReleasesCount = value;
        }
    }

    private int _selectedVersion;

    public int SelectedVersion
    {
        get => _selectedVersion;
        set => SetProperty(ref _selectedVersion, value);
    }

    public ICommand BrowseFolderCommand
    {
        get;
        private set;
    }

    private void CreateBrowseFolderCommand()
    {
        BrowseFolderCommand = new RelayCommand(BrowseFolder);
    }

    public void BrowseFolder()
    {
        using var openDialog = new CommonOpenFileDialog();
        openDialog.IsFolderPicker = true;
        if (openDialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            YuzuPath = openDialog.FileName;
        }
    }

    public ICommand CheckForUpdatesCommand
    {
        get;
        private set;
    }

    private void CreateCheckForUpdatesCommand()
    {
        CheckForUpdatesCommand = new RelayCommand(() =>
        {
            CheckForYuzuUpdate(true);
        }, () =>
        {
            return Directory.Exists(YuzuPath) && File.Exists(_settings.YuzuExecutable);
        });
    }

    public ICommand InstallCommand
    {
        get;
        private set;
    }

    private void CreateInstallCommand()
    {
        InstallCommand = new RelayCommand(() =>
        {
            Install();
        }, () =>
        {
            return YuzuVersions.Count > 0 && Directory.Exists(YuzuPath);
        });
    }

    public void Install(bool useLatest = false)
    {
        if (!Directory.Exists(YuzuPath) || string.IsNullOrWhiteSpace(YuzuPath))
        {
            ShowNotification("Path Error\nPath may need Admin Rights, please restart the APP as Admin.", SymbolRegular.ErrorCircle24);
            return;
        }

        YuzuVersion downloadVersion = useLatest ? YuzuVersions[0] : YuzuVersions[SelectedVersion];
        
        Networking.DownloadFile(downloadVersion.DirectDownloadUrl, System.IO.Path.Combine(YuzuPath, "TempDownload.zip"),
            GetType().GetProperty(nameof(ProgressText)),
            GetType().GetProperty(nameof(Progress)),
            GetType().GetProperty(nameof(ProgressBarVisibility)),
            this,
            _settings);

        Task.Run(() =>
        {
            while (Networking._downloading)
            {
                continue;
            }

            ShowNotification($"Info\nInstalled Yuzu {downloadVersion.Name} with Success!", SymbolRegular.Checkmark24);
        });
    }

    public ICommand ReloadReleasesCommand
    {
        get;
        private set;
    }

    private void CreateReloadReleasesCommand()
    {
        ReloadReleasesCommand = new RelayCommand(ReloadReleases, () =>
        {
            return _settings.YuzuExecutable != "";
        });
    }

    public void ReloadReleases()
    {
        Task.Run(() =>
        {
            YuzuVersions = new List<YuzuVersion>();
            YuzuVersions = Github.GetYuzuVersionInstances(ShownReleasesCount);
            SelectedVersion = 0;
        });
    }
    
    public ICommand LaunchYuzuCommand
    {
        get;
        private set;
    }

    private void CreateLaunchYuzuCommand()
    {
        LaunchYuzuCommand = new RelayCommand(LaunchYuzu, () =>
        {
            return _settings.YuzuExecutable != "";
        });
    }

    public void LaunchYuzu()
    {
        ProcessHelpers.ExecuteProcess(_settings.YuzuExecutable, AdminMode);
    }

    public void Initialize()
    {
        Task.Run(() =>
        {
            CreateBrowseFolderCommand();
            CreateInstallCommand();
            CreateLaunchYuzuCommand();
            CreateCheckForUpdatesCommand();
            CreateReloadReleasesCommand();
            _iniParser = new IniParser("./UpdaterConfig.ini");
            _settings = new UpdaterSettings
            {
                AdminMode = bool.Parse(_iniParser.GetSetting("Settings", nameof(_settings.AdminMode))),
                LaunchAfterUpdate = bool.Parse(_iniParser.GetSetting("Settings", nameof(_settings.LaunchAfterUpdate))),
                AutoClose = bool.Parse(_iniParser.GetSetting("Settings", nameof(_settings.AutoClose))),
                YuzuPath = _iniParser.GetSetting("Settings", nameof(_settings.YuzuPath)),
                ShownReleasesCount =
                    Int32.Parse(_iniParser.GetSetting("Settings", nameof(_settings.ShownReleasesCount)))
            };

            AdminMode = _settings.AdminMode;
            AutoClose = _settings.AutoClose;
            LaunchAfterUpdate = _settings.LaunchAfterUpdate;
            YuzuPath = _settings.YuzuPath;
            ShownReleasesCount = _settings.ShownReleasesCount;
            YuzuVersions = Github.GetYuzuVersionInstances(ShownReleasesCount);

            CheckForYuzuUpdate();
        });
    }

    private async void CheckForYuzuUpdate(bool notifyIfNotOutdated = false)
    {
        string localVer = null; // Used to store the return value
        await Task.Run(() =>
        {
            localVer = Yuzu.GetYuzuBinaryVersion(_settings.YuzuExecutable);
        });

        if (localVer == string.Empty) return;

        string newestVersion = YuzuVersions[0].VersionNumber;

        if (Yuzu.IsLocalYuzuOutdated(localVer, newestVersion))
        {
            MessageBoxResult dialogResult = System.Windows.MessageBox.Show(
                $"There is a newer version of yuzu-ea!" +
                $"\nCurrent version: {localVer}" +
                $"\nNewest version: {newestVersion}" +
                $"\nDo you want to install it now?",
                "Yuzu update avialable!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (dialogResult == MessageBoxResult.Yes)
            {
                Install(true);
            }

            return;
        }
        if (notifyIfNotOutdated) ShowNotification("Info\nYuzu is up to Date", SymbolRegular.Checkmark24);
    }

    public void SaveSettings()
    {
        foreach (var property in _settings.GetProperties())
        {
            var value = property.GetValue(_settings);

            if (value is null) continue;
            _iniParser.AddSetting("Settings", property.Name, value.ToString());
        }

        _iniParser.SaveSettings();
    }

    public void AssignNotificationBar(Snackbar bar)
    {
        NotificationBar = bar;
    }

    public void ShowNotification(string text, SymbolRegular symbol = SymbolRegular.Info24)
    {
        NotificationBar.Dispatcher.Invoke(new Action(() =>
        {
            NotificationBar.Icon = symbol;
            NotificationBar.Content = text;

            NotificationBar.Show();
        }));
    }

    public MainPageViewModel(Snackbar notificationBar)
    {
        Initialize();
        AssignNotificationBar(notificationBar);
    }
}