﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AudioDevice_Quickswitcher.model;
using AudioDevice_Quickswitcher.utilities;
using AudioDevice_Quickswitcher.views;

namespace AudioDevice_Quickswitcher.controllers.setup
{
    class SetupController : IDeviceDisconnectedListener
    {
        private readonly AudioDeviceManager _audioDeviceManager = new AudioDeviceManager(@"dependencies/EndPointController_forked.exe");
        private IList<AudioDevice> _preConnectAudioDevices;
        private readonly Timer _reconnectTimer = new Timer();

        private Form _currentView;

        public SetupController()
        {
            _reconnectTimer.Tick += ListenForReconnect;
            _reconnectTimer.Interval = 300;

            _currentView = new DisconnectDeviceView(this);
            
            Application.Run(_currentView);

        }

        private void ChangeView(Form newForm)
        {
            _currentView.Hide();
            _currentView = newForm;
            _currentView.Show();
        }

        public void DeviceDisconnected()
        {
            _preConnectAudioDevices = _audioDeviceManager.GetDevices();
            _reconnectTimer.Start();

            ChangeView(new ReconnectDeviceView());
        }

        private void ListenForReconnect(object sender, EventArgs eventArgs)
        {
            IList<AudioDevice> connectedDevices = _audioDeviceManager.GetDevices();
            if (_preConnectAudioDevices.Count != connectedDevices.Count)
            {
                // A new device has been connected, it's the one we're searching for
                AudioDevice foundDevice = connectedDevices.Except(_preConnectAudioDevices).First();
                _reconnectTimer.Stop();

                DeviceFound(foundDevice);
            }
        }

        private void DeviceFound(AudioDevice device)
        {
            // TODO save it
            ChangeView(new DeviceFoundView(device));
        }
    }
}
