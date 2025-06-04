using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using D4TTS.Entities;

namespace D4TTS.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private string _configFilePath = @"C:\Users\Uthar\Documents\Diablo IV\LocalPrefs.txt";
        private string _installFolderPath = @"D:\Battle.net\Diablo IV";
        private bool _isApplicationClosing = false;
        private bool _isAutoScrollEnabled = true;
        private bool _isTtsConnected = false;
        private bool _isTtsConnecting = false;
        private ObservableCollection<TTSMessage> _messages = new ObservableCollection<TTSMessage>();
        private int _selectedMessageIndex = 0;
        private string _status = "Not connected";

        #region Constructors

        public MainWindowViewModel()
        {
            // Init View commands
            ApplicationClosingCommand = new RelayCommand(ApplicationClosingExecute);
            RefreshStatusCommand = new RelayCommand(RefreshStatusExecute, CanRefreshStatusExecute);
        }

        #endregion

        #region Events

        #endregion

        #region Properties

        public ICommand ApplicationClosingCommand { get; }
        public ICommand RefreshStatusCommand { get; }

        public ObservableCollection<TTSMessage> Messages { get => _messages; set => _messages = value; }

        public string ConfigFilePath
        {
            get => _configFilePath;
            set => SetProperty(ref _configFilePath, value);
        }

        public string InstallFolderPath 
        {
            get => _installFolderPath;
            set => SetProperty(ref _installFolderPath, value);
        }

        public bool IsAutoScrollEnabled 
        {
            get => _isAutoScrollEnabled;
            set => SetProperty(ref _isAutoScrollEnabled, value);
        }

        public bool IsTtsConnected 
        {
            get => _isTtsConnected;
            set
            {
                _isTtsConnected = value;

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ((RelayCommand)RefreshStatusCommand).NotifyCanExecuteChanged();
                });
            }
        }

        public bool IsTtsConnecting
        {
            get => _isTtsConnecting;
            set
            {
                _isTtsConnecting = value;

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ((RelayCommand)RefreshStatusCommand).NotifyCanExecuteChanged();
                });
            }
        }

        public int SelectedMessageIndex
        {
            get => _selectedMessageIndex;
            set => SetProperty(ref _selectedMessageIndex, value);
        }

        public string Status 
        { 
            get => _status;
            set => SetProperty(ref _status, value);
        }

        #endregion

        #region Event handlers

        private void ApplicationClosingExecute()
        {
            _isApplicationClosing = true;
        }

        private bool CanRefreshStatusExecute()
        {
            return !IsTtsConnected && !IsTtsConnecting;
        }

        private void RefreshStatusExecute()
        {
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}");

            IsTtsConnecting = true;

            // Check if saapi64.dll exists.
            if (File.Exists($"{InstallFolderPath}\\saapi64.dll"))
            {
                // Check if config is correct.
                if (File.Exists(ConfigFilePath))
                {
                    string localPrefs = File.ReadAllText(ConfigFilePath);
                    if (localPrefs.Contains("UseScreenReader \"1\"") && localPrefs.Contains("UseThirdPartyReader \"1\""))
                    {
                        Status = "Connecting";
                        Thread ClientThread = new Thread(() => NewThreadStartClient("d4tts"));
                        ClientThread.Start();
                    }
                    else
                    {
                        Status = "TextToSpeech or ThirdPartyReader is not enabled";
                    }
                }
                else
                {
                    Status = $"{ConfigFilePath} not found.";
                }
            }
            else
            {
                Status = $"{InstallFolderPath}\\saapi64.dll not found.";
            }

            IsTtsConnecting = Status.Equals("Connecting");

            Debug.WriteLine($"~{MethodBase.GetCurrentMethod()?.Name}");
        }

        #endregion

        #region Methods

        public void NewThreadStartClient(string name)
        {
            NamedPipeServerStream namedPipeServerStream = new NamedPipeServerStream(name);
            StreamReader streamReader = new StreamReader(namedPipeServerStream);
            namedPipeServerStream.WaitForConnectionAsync().ContinueWith(_ =>
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: tts connection ready.");

                Status = "Connected";
                IsTtsConnected = true;
                while (namedPipeServerStream.IsConnected && !_isApplicationClosing)
                {
                    string? line = streamReader.ReadLine();
                    if (line == null) continue;
                    if (line.Equals("DISCONNECTED")) break;

                    // Add incoming messages.
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        Messages.Add(new TTSMessage
                        {
                            Message = line
                        });
                        SelectedMessageIndex = Messages.Count - 1;
                    });
                }

                namedPipeServerStream.Disconnect();
                IsTtsConnected = false;
            });
        }

        #endregion
    }
}
