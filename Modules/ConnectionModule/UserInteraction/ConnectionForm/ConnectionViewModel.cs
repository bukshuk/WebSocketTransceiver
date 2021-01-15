﻿namespace ConnectionModule.UserInteraction.ConnectionForm
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;
    using System.Windows.Input;

    using BusinessLogic;

    using Prism.Commands;
    using Prism.Mvvm;

    using Settings;

    public class ConnectionViewModel : BindableBase
    {
        private readonly IConnectionMaker _connectionMaker;
        private ConnectionSettings _connectionSettings;
        private string _ip;
        private string _port;
        private bool _isConnected;

        public string Ip
        {
            get => _ip;
            set => SetProperty(ref _ip, value);
        }

        public string Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public ICommand ConnectCommand => new DelegateCommand(ExecuteConnectCommand, CanExecuteConnectCommand)
            .ObservesProperty(() => Ip)
            .ObservesProperty(() => Port);

        public ConnectionViewModel(IConnectionMaker connectionMaker)
        {
            _connectionMaker = connectionMaker;
            _connectionMaker.OnConnected += HandleConnectionMakerOnConnected;
            _connectionMaker.OnConnectionBroken += HandleConnectionMakerOnConnectionBroken;
            Init();
        }

        private void Init()
        {
            _connectionSettings = _connectionMaker.GetSettings();
            _ip = _connectionSettings.Ip ?? Dns.GetHostAddresses(Dns.GetHostName()).First(address => address.AddressFamily == AddressFamily.InterNetwork).ToString();
            _port = _connectionSettings.Port;
        }

        private void HandleConnectionMakerOnConnected(object sender, EventArgs eventArgs)
        {
            IsConnected = true;
        }

        private void HandleConnectionMakerOnConnectionBroken(object sender, EventArgs eventArgs)
        {
            IsConnected = false;
        }

        private void ExecuteConnectCommand()
        {
            if (_isConnected)
            {
                _connectionMaker.Disconnect();
            }
            else
            {
                _connectionSettings.Ip = _ip;
                _connectionSettings.Port = _port;
                _connectionMaker.Connect(_connectionSettings);
            }
        }

        private bool CanExecuteConnectCommand()
        {
            bool isCorrectIp = (Ip != null) && Regex.IsMatch(Ip, @"^\d{1,3}(\.\d{1,3}){3}$");
            bool isCorrectPort = string.IsNullOrWhiteSpace(Port) || Regex.IsMatch(Port, @"^\d{1,4}$");

            return isCorrectIp && isCorrectPort;
        }
    }
}
