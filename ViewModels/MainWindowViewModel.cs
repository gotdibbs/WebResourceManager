using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using WebResourceManager.Data;
using WebResourceManager.Helpers;
using WebResourceManager.Models;

namespace WebResourceManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private CrmServiceClient _client;
        private FileWatcher _fileWatcher;
        private Settings _settings;
        private Timer _timer;

        private int _countRemoteNew = 0;
        private int _countRemoteDeleted = 0;
        private int _countRemoteUpdated = 0;

        public string StatusText
        {
            get
            {
                if (!IsConnected)
                {
                    return "Not connected";
                }

                if (_countRemoteNew > 0 || _countRemoteDeleted > 0 || _countRemoteUpdated > 0)
                {
                    return $"Updates detected. Please refresh. New: {_countRemoteNew} | Deleted: {_countRemoteDeleted} | Updated: {_countRemoteUpdated}";
                }

                return "Up to date";
            }
        }

        private bool _isSettingsVisible;
        public bool IsSettingsVisible
        {
            get { return _isSettingsVisible; }
            set { Set(ref _isSettingsVisible, value); }
        }

        private bool _isOverrideVisible;
        public bool IsOverrideVisible
        {
            get { return _isOverrideVisible; }
            set { Set(ref _isOverrideVisible, value); }
        }

        public bool IsProjectSelected
        {
            get
            {
                return SelectedProject != null;
            }
        }

        public bool IsConnected
        {
            get
            {
                return _client != null;
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(ref _isBusy, value); }
        }

        public bool IsNotBusy
        {
            get
            {
                return !IsBusy;
            }
        }

        public bool IsConnectEnabled
        {
            get
            {
                return IsProjectSelected && IsNotBusy;
            }
        }

        public int? CountSelected
        {
            get
            {
                var count = FilteredWebResources?.Count(w => w.IsSelected);

                return count > 0 ? count : null;
            }
        }

        public int? CountUploadSelected
        {
            get
            {
                var count = FilteredWebResources?.Count(w => w.IsSelected && !string.IsNullOrEmpty(w.FilePath));

                return count > 0 ? count : null;
            }
        }

        public int? CountDownloadSelected
        {
            get
            {
                var count = FilteredWebResources?.Count(w => w.IsSelected && w.Create == false);

                return count > 0 ? count : null;
            }
        }

        private bool _areAllSelected;
        public bool AreAllSelected
        {
            get { return _areAllSelected; }
            set { Set(ref _areAllSelected, value); }
        }

        private ObservableCollection<WebResource> _webResources;
        public ObservableCollection<WebResource> WebResources
        {
            get { return _webResources; }
            set { Set(ref _webResources, value); }
        }

        private ObservableCollection<WebResource> _filteredWebResources;
        public ObservableCollection<WebResource> FilteredWebResources
        {
            get { return _filteredWebResources; }
            set { Set(ref _filteredWebResources, value); }
        }

        private WebResource _selectedWebResource;
        public WebResource SelectedWebResource
        {
            get { return _selectedWebResource; }
            set { Set(ref _selectedWebResource, value); }
        }

        private ObservableCollection<Filter> _filters;
        public ObservableCollection<Filter> Filters
        {
            get { return _filters; }
            set { Set(ref _filters, value); }
        }

        private Filter _selectedFilter;
        public Filter SelectedFilter
        {
            get { return _selectedFilter; }
            set { Set(ref _selectedFilter, value); }
        }

        private ObservableCollection<Project> _projects;
        public ObservableCollection<Project> Projects
        {
            get { return _projects; }
            set { Set(ref _projects, value); }
        }

        private Project _selectedProject;
        public Project SelectedProject
        {
            get { return _selectedProject; }
            set { Set(ref _selectedProject, value); }
        }

        private ObservableCollection<Solution> _solutions;
        public ObservableCollection<Solution> Solutions
        {
            get { return _solutions; }
            set { Set(ref _solutions, value); }
        }

        private Solution _selectedSolution;
        public Solution SelectedSolution
        {
            get { return _selectedSolution; }
            set { Set(ref _selectedSolution, value); }
        }

        private ObservableCollection<ImposterProfile> _profiles;
        public ObservableCollection<ImposterProfile> Profiles
        {
            get { return _profiles; }
            set { Set(ref _profiles, value); }
        }

        private ImposterProfile _selectedProfile;
        public ImposterProfile SelectedProfile
        {
            get { return _selectedProfile; }
            set { Set(ref _selectedProfile, value); }
        }

        private Override _selectedOverride;
        public Override SelectedOverride
        {
            get { return _selectedOverride; }
            set { Set(ref _selectedOverride, value); }
        }

        public string SelectedProjectName
        {
            get
            {
                return SelectedProject != null ? SelectedProject.Name : string.Empty;
            }
        }

        private bool _isWatching;
        public bool IsWatching
        {
            get { return _isWatching; }
            set { Set(ref _isWatching, value); }
        }
        public string FileWatcherCommandText
        {
            get
            {
                return !IsWatching ? "Start Watching" : "Stop Watching";
            }
        }

        private List<string> _localChanges;
        public int? CountLocalChanges
        {
            get
            {
                return _localChanges?.Count > 0 ? _localChanges?.Count : null;
            }
        }

        public ICommand OpenSettingsCommand { get; private set; }
        public ICommand UpdateConnectionCommand { get; private set; }
        public ICommand ToggleFileWatcherCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand UploadCommand { get; private set; }
        public ICommand DownloadCommand { get; private set; }
        public ICommand ViewCommand { get; private set; }
        public ICommand OpenFolderCommand { get; private set; }
        public ICommand EditInCodeCommand { get; private set; }
        public ICommand DebugWithImposterCommand { get; private set; }
        public ICommand DiffCommand { get; private set; }
        public ICommand AddCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand EditOverrideCommand { get; private set; }
        public ICommand SaveOverrideCommand { get; private set; }

        public MainWindowViewModel()
        {
            OpenSettingsCommand = new RelayCommand(() => OpenSettings());
            UpdateConnectionCommand = new RelayCommand(() => UpdateConnection());
            ToggleFileWatcherCommand = new RelayCommand(() => ToggleFileWatcher());
            RefreshCommand = new RelayCommand(() => Refresh());
            UploadCommand = new RelayCommand(() => Upload());
            DownloadCommand = new RelayCommand(() => Download());
            ViewCommand = new RelayCommand(() => View());
            OpenFolderCommand = new RelayCommand(() => OpenFolder());
            EditInCodeCommand = new RelayCommand(() => EditInCode());
            DebugWithImposterCommand = new RelayCommand(() => DebugWithImposter());
            DiffCommand = new RelayCommand(() => Diff());
            AddCommand = new RelayCommand(() => AddConnection());
            DeleteCommand = new RelayCommand(() => DeleteConnection());
            SaveCommand = new RelayCommand(() => SaveConnection());
            EditOverrideCommand = new RelayCommand(() => EditOverride());
            SaveOverrideCommand = new RelayCommand(() => SaveOverride());

            WebResources = new ObservableCollection<WebResource>();
            WebResources.CollectionChanged += WebResources_CollectionChanged;

            FilteredWebResources = new ObservableCollection<WebResource>();
            Filters = new ObservableCollection<Filter>(Filter.GetAllFilters());
            SelectedFilter = Filters.First();

            Solutions = new ObservableCollection<Solution>();

            PropertyChanged += MainWindowViewModel_PropertyChanged;

            LoadSettings();
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == GetPropertyName(() => SelectedProject) &&
                SelectedProject != null &&
                !string.IsNullOrEmpty(SelectedProject.Path) &&
                !string.IsNullOrEmpty(SelectedProject.ConnectionString))
            {
                _timer?.Stop();

                LoadProject();
                _settings.LastSelectedProjectId = SelectedProject.Id;
                SaveSettings();

                RaisePropertyChanged(() => SelectedProjectName);
            }
            else if (e.PropertyName == GetPropertyName(() => SelectedSolution) && SelectedSolution != null)
            {
                SelectedProject.SolutionId = SelectedSolution.Id;
                SelectedProject.SolutionName = SelectedSolution.Name;
                SelectedProject.CustomizationPrefix = SelectedSolution.CustomizationPrefix;

                LoadWebResources();
            }
            else if (e.PropertyName == GetPropertyName(() => IsWatching))
            {
                RaisePropertyChanged(() => FileWatcherCommandText);
            }
            else if (e.PropertyName == GetPropertyName(() => AreAllSelected))
            {
                foreach (var resource in FilteredWebResources)
                {
                    resource.IsSelected = AreAllSelected;
                }
            }
            else if (e.PropertyName == GetPropertyName(() => SelectedFilter))
            {
                UpdateFilter();
            }
            else if (e.PropertyName == GetPropertyName(() => IsBusy))
            {
                RaisePropertyChanged(() => IsNotBusy);
                RaisePropertyChanged(() => IsConnectEnabled);
            }
            else if (e.PropertyName == GetPropertyName(() => SelectedProfile) && SelectedProject != null)
            {
                SelectedProject.ImposterProfileId = SelectedProfile == null ? Guid.Empty : SelectedProfile.ProfileId;
            }
        }

        private void UpdateFilter()
        {
            UIHelper.Invoke(() =>
            {
                FilteredWebResources.Clear();

                foreach (var resource in WebResources.Where(SelectedFilter.ApplyFilter))
                {
                    FilteredWebResources.Add(resource);
                }

                RaisePropertyChanged(() => CountSelected);
                RaisePropertyChanged(() => CountUploadSelected);
                RaisePropertyChanged(() => CountDownloadSelected);
            });
        }

        public void OpenSettings()
        {
            IsSettingsVisible = !IsSettingsVisible;
        }

        public void UpdateConnection()
        {
            if (string.IsNullOrEmpty(SelectedProject?.ConnectionString))
            {
                Alert.Show("Please populate a connection string before initiating a test.");
                return;
            }

            Connect();
        }

        private async Task Connect()
        {
            IsBusy = true;

            await Task.Run(async () =>
            {
                _client = new CrmServiceClient(SelectedProject.ConnectionString);

                if (!_client.IsReady)
                {
                    UIHelper.Invoke(() =>
                    {
                        Alert.Show("Connection failed. Please recheck your connection string.");
                        IsBusy = false;
                    });

                    _client = null;
                }
                else
                {
                    await LoadSolutions();
                }

                UIHelper.Invoke(() =>
                {
                    RaisePropertyChanged(() => SelectedProjectName);
                    RaisePropertyChanged(() => IsConnected);
                    RaisePropertyChanged(() => StatusText);
                });
            });
        }

        private void LoadSettings()
        {
            _settings = Settings.Load();
            Profiles = new ObservableCollection<ImposterProfile>(ImposterSettings.Load().Profiles);

            Projects = new ObservableCollection<Project>();
            Projects.CollectionChanged += Projects_CollectionChanged;
            foreach (var project in _settings.Projects.OrderBy(p => p.Name))
            {
                Projects.Add(project);
            }

            if (Projects.Count > 0)
            {
                var project = Projects.FirstOrDefault(p => p.Id == _settings.LastSelectedProjectId);

                SelectedProject = project ?? Projects.First();
            }
            else
            {
                Projects.Add(new Project());

                SelectedProject = Projects.First();

                IsSettingsVisible = true;
            }
        }

        private void Projects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                    item.PropertyChanged -= Project_PropertyChanged;
            }
            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                    item.PropertyChanged += Project_PropertyChanged;
            }
        }

        private void Project_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                RaisePropertyChanged(() => SelectedProjectName);
            }
        }

        private async void LoadProject()
        {
            if (SelectedProject == null || string.IsNullOrEmpty(SelectedProject.ConnectionString))
            {
                return;
            }

            if (SelectedProject.ImposterProfileId != Guid.Empty)
            {
                SelectedProfile = Profiles.FirstOrDefault(p => p.ProfileId == SelectedProject.ImposterProfileId);
            }

            await Connect();

            await LoadWebResources();

            StartTimer();
        }

        private async Task LoadWebResources()
        {
            await Task.Run(() =>
            {
                if (_client == null)
                {
                    return;
                }

                UIHelper.Invoke(() => IsBusy = true);

                var local = FileData.GetFiles(SelectedProject);
                var remote = DynamicsData.GetAllBySolutionId(_client, SelectedProject.SolutionId);

                foreach (var file in local)
                {
                    var match = remote?.FirstOrDefault(w => w.Name == file.Name);
                    if (match != null)
                    {
                        file.ModifiedBy = match.ModifiedBy;
                        file.ModifiedOn = match.ModifiedOn;
                        file.Id = match.Id;
                        file.Create = false;
                        file.Status = Constants.WebResourceStatus.Exists;
                        file.RowVersion = match.RowVersion;

                        remote.Remove(match);
                        continue;
                    }

                    var existing = DynamicsData.GetExisting(_client, file.Name);

                    if (existing != null)
                    {
                        file.ModifiedBy = existing.ModifiedBy;
                        file.ModifiedOn = existing.ModifiedOn;
                        file.Id = existing.Id;
                        file.Create = false;
                        file.Status = Constants.WebResourceStatus.NotInSolution;
                    }
                    else
                    {
                        file.Create = true;
                        file.Status = Constants.WebResourceStatus.New;
                    }
                }

                UIHelper.Invoke(() =>
                {
                    WebResources.Clear();
                    foreach (var resource in remote.Concat(local))
                    {
                        WebResources.Add(resource);
                    }

                    UpdateFilter();

                    RaisePropertyChanged(() => CountSelected);
                    RaisePropertyChanged(() => CountUploadSelected);
                    RaisePropertyChanged(() => CountDownloadSelected);
                    IsBusy = false;
                });
            });
        }

        private void WebResources_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                    item.PropertyChanged -= WebResource_PropertyChanged;
            }
            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                    item.PropertyChanged += WebResource_PropertyChanged;
            }

            UpdateFilter();
        }

        private void WebResource_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                RaisePropertyChanged(() => CountSelected);
                RaisePropertyChanged(() => CountUploadSelected);
                RaisePropertyChanged(() => CountDownloadSelected);
                RaisePropertyChanged(() => FilteredWebResources);

                UpdateFilter();
            }
        }

        private async Task LoadSolutions()
        {
            await Task.Run(() =>
            {
                UIHelper.Invoke(() => IsBusy = true);

                var solutions = DynamicsData.GetSolutions(_client);

                Solution selectedSolution = null;

                if (solutions != null &&
                    solutions.Count > 0 && 
                    SelectedProject != null &&
                    SelectedProject.SolutionId != Guid.Empty)
                {
                    selectedSolution = solutions.FirstOrDefault(s => s.Id == SelectedProject.SolutionId);

                    if (selectedSolution == null)
                    {
                        Alert.Show("Selected solution is not found in the target environment. You may need to select a new solution.");
                    }
                }

                UIHelper.Invoke(() =>
                {
                    Solutions.Clear();

                    foreach (var solution in solutions)
                    {
                        Solutions.Add(solution);
                    }

                    SelectedSolution = selectedSolution ?? Solutions.FirstOrDefault();

                    IsBusy = false;
                });
            });
        }

        private void ToggleFileWatcher()
        {
            if (!IsWatching)
            {
                if (SelectedProject == null || SelectedSolution == null)
                {
                    return;
                }

                IsWatching = true;
                _localChanges = new List<string>();
                _fileWatcher = new FileWatcher(SelectedProject.Path, true);
                _fileWatcher.Handler = FileWatchUpdate;
            }
            else
            {
                IsWatching = false;
                _fileWatcher.Dispose();

                _localChanges.Clear();
                RaisePropertyChanged(() => CountLocalChanges);
            }
        }

        private void Refresh()
        {
            LoadWebResources();
        }

        private void FileWatchUpdate(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                LoadWebResources();
                return;
            }

            var resource = WebResources.FirstOrDefault(w => w.FilePath == e.FullPath);

            if (resource == null)
            {
                return;
            }

            resource.Status = resource.Create ? resource.Status : Constants.WebResourceStatus.Changed;
            resource.IsSelected = true;

            if (!_localChanges.Contains(e.FullPath))
            {
                _localChanges.Add(e.FullPath);
                RaisePropertyChanged(() => CountLocalChanges);
            }
        }

        private async void Upload()
        {
            IsBusy = true;

            await Task.Run(() =>
            {
                if (_client == null)
                {
                    return;
                }

                var selected = WebResources.Where(w => w.IsSelected && !string.IsNullOrEmpty(w.FilePath)).ToList();

                foreach (var resource in selected)
                {
                    DynamicsData.Upsert(_client, resource, SelectedSolution);
                }

                DynamicsData.Publish(_client, selected);
            });

            IsBusy = false;

            LoadWebResources();
        }

        private async void Download()
        {
            await Task.Run(() =>
            {
                foreach (var resource in WebResources.Where(w => w.IsSelected && w.Create == false))
                {
                    var existing = DynamicsData.GetExistingContent(_client, resource.Id.Value);

                    if (existing == null)
                    {
                        continue;
                    }

                    FileData.SaveWebResource(SelectedProject, existing);
                }

                LoadWebResources();
            });
        }

        private void AddConnection()
        {
            var project = new Project();
            Projects.Add(project);

            SelectedProject = project;
            SelectedSolution = null;

            WebResources.Clear();
        }

        private void DeleteConnection()
        {
            Projects.Remove(SelectedProject);
            SelectedProject = Projects.FirstOrDefault();

            if (SelectedProject == null)
            {
                Projects.Add(new Project());
                SelectedProject = Projects.First();
            }

            SaveSettings();
        }

        private void SaveConnection()
        {
            if (SelectedProject == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(SelectedProject.Name) || string.IsNullOrEmpty(SelectedProject.Path) || string.IsNullOrEmpty(SelectedProject.ConnectionString) ||
                SelectedSolution == null)
            {
                Alert.Show("Please populate all fields, test your connection string, and select a solution.");
                return;
            }

            if (!Directory.Exists(SelectedProject.Path))
            {
                Alert.Show("The specified directory appears not to exist. Please correct before saving.");
                return;
            }

            _settings.LastSelectedProjectId = SelectedProject.Id;

            SaveSettings();
        }

        private void SaveSettings()
        {
            _settings.Projects = Projects.ToList();
            _settings.Save();

            LoadWebResources();
        }

        private void EditOverride()
        {
            if (SelectedProject == null || SelectedWebResource == null)
            {
                return;
            }

            var o = SelectedProject.Overrides?.FirstOrDefault(wr => wr.LocalRelativePath == SelectedWebResource.LocalRelativePath);

            if (o == null)
            {
                o = new Override
                {
                    LocalRelativePath = SelectedWebResource.LocalRelativePath,
                    RemoteName = SelectedWebResource.RemoteName
                };
            }

            SelectedOverride = o;

            IsOverrideVisible = true;
        }

        private void SaveOverride()
        {
            var o = SelectedProject.Overrides?.FirstOrDefault(wr => wr.LocalRelativePath == SelectedOverride.LocalRelativePath);

            if (o != null)
            {
                o.RemoteName = SelectedOverride.RemoteName;
                o.Description = SelectedOverride.Description;
            }
            else
            {
                SelectedProject.Overrides.Add(SelectedOverride);
            }

            SelectedProject.Save();

            IsOverrideVisible = false;

            LoadWebResources();
        }

        private void View()
        {
            if (!IsConnected || SelectedWebResource == null || SelectedWebResource.Id == null)
            {
                return;
            }

            var url = _client.CrmConnectOrgUriActual.ToString();

            if (url.EndsWith("/"))
            {
                url.TrimEnd('/');
            }

            url = url.Replace(".api", string.Empty).Replace("/XRMServices/2011/Organization.svc", string.Empty);

            Process.Start(string.Format("{0}/main.aspx?etc=9333&id={1}&appSolutionId={2}&pagetype=webresourceedit",
                url,
                SelectedWebResource.Id.ToString(),
                SelectedProject.SolutionId.ToString()));
        }

        private void OpenFolder()
        {
            if (SelectedProject == null || string.IsNullOrEmpty(SelectedProject.Path))
            {
                return;
            }

            string path = SelectedProject.Path;

            if (SelectedWebResource != null && !string.IsNullOrEmpty(SelectedWebResource.FilePath))
            {
                path = Path.GetDirectoryName(SelectedWebResource.FilePath);
            }

            Process.Start(path);
        }

        private void Diff()
        {
            if (SelectedWebResource?.Id == null)
            {
                Alert.Show("The selected web resource does not exist in the target environment.");
                return;
            }

            var existing = DynamicsData.GetExistingContent(_client, SelectedWebResource.Id.Value);

            if (existing == null)
            {
                return;
            }

            string tempFile = FileData.CreateTempFile(existing);

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C code -d {tempFile} {SelectedWebResource.FilePath}"
            };
            process.StartInfo = startInfo;
            process.Start();
        }

        private void EditInCode()
        {
            if (SelectedProject == null)
            {
                return;
            }

            string file = null;

            if (SelectedWebResource != null && !string.IsNullOrEmpty(SelectedWebResource.FilePath))
            {
                file = SelectedWebResource.FilePath;
            }

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C code {SelectedProject.Path} {file}"
            };
            process.StartInfo = startInfo;
            process.Start();
        }

        private void DebugWithImposter()
        {
            if (SelectedProject == null)
            {
                return;
            }

            FiddlerHelper.ExecAction(SelectedProject.ImposterProfileId);
        }

        private void StartTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }

            _timer = new Timer(10000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Enabled = true;
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!IsConnected || IsSettingsVisible)
            {
                return;
            }

            IsBusy = true;

            await Task.Run(() =>
            {
                var remote = DynamicsData.GetAllBySolutionId(_client, SelectedProject.SolutionId);

                _countRemoteNew = 0;
                _countRemoteDeleted = 0;
                _countRemoteUpdated = 0;

                foreach (var remoteResource in remote)
                {
                    if (!WebResources.Any(w => w.Id != null && w.Id == remoteResource.Id))
                    {
                        _countRemoteNew++;
                        continue;
                    }

                    var existing = WebResources.FirstOrDefault(w => w.Id == remoteResource.Id);

                    if (existing == null)
                    {
                        _countRemoteDeleted++;
                        continue;
                    }

                    if (existing.RowVersion != remoteResource.RowVersion)
                    {
                        _countRemoteUpdated++;
                    }
                }
            });

            RaisePropertyChanged(() => StatusText);

            IsBusy = false;
        }
    }
}
