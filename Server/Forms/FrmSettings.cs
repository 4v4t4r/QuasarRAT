﻿using System;
using System.Globalization;
using System.Windows.Forms;
using xServer.Core.Data;
using xServer.Core.Encryption;
using xServer.Core.Networking;
using xServer.Core.Networking.Utilities;
using xServer.Core.Utilities;

namespace xServer.Forms
{
    public partial class FrmSettings : Form
    {
        private readonly ConnectionHandler _listenServer;

        public FrmSettings(ConnectionHandler listenServer)
        {
            this._listenServer = listenServer;

            InitializeComponent();

            if (listenServer.Listening)
            {
                btnListen.Text = "Stop listening";
                ncPort.Enabled = false;
                txtPassword.Enabled = false;
            }

            ShowPassword(false);
        }

        private void FrmSettings_Load(object sender, EventArgs e)
        {
            ncPort.Value = Settings.ListenPort;
            chkAutoListen.Checked = Settings.AutoListen;
            chkPopup.Checked = Settings.ShowPopup;
            txtPassword.Text = Settings.Password;
            chkUseUpnp.Checked = Settings.UseUPnP;
            chkShowTooltip.Checked = Settings.ShowToolTip;
            chkNoIPIntegration.Checked = Settings.EnableNoIPUpdater;
            txtNoIPHost.Text = Settings.NoIPHost;
            txtNoIPUser.Text = Settings.NoIPUsername;
            txtNoIPPass.Text = Settings.NoIPPassword;
        }

        private ushort GetPortSafe()
        {
            var portValue = ncPort.Value.ToString(CultureInfo.InvariantCulture);
            ushort port;
            return (!ushort.TryParse(portValue, out port)) ? (ushort)0 : port;
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            ushort port = GetPortSafe();

            if (port == 0)
            {
                MessageBox.Show("Please enter a valid port > 0.", "Please enter a valid port", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (btnListen.Text == "Start listening" && !_listenServer.Listening)
            {
                try
                {
                    if (chkUseUpnp.Checked)
                    {
                        if (!UPnP.IsDeviceFound)
                        {
                            MessageBox.Show("No available UPnP device found!", "No UPnP device", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            int outPort;
                            UPnP.CreatePortMap(port, out outPort);
                            if (port != outPort)
                            {
                                MessageBox.Show("Creating a port map with the UPnP device failed!\nPlease check if your device allows to create new port maps.", "Creating port map failed", MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                            }
                        }
                    }
                    if(chkNoIPIntegration.Checked)
                        NoIpUpdater.Start();
                    _listenServer.Listen(port);
                }
                finally
                {
                    btnListen.Text = "Stop listening";
                    ncPort.Enabled = false;
                    txtPassword.Enabled = false;
                }
            }
            else if (btnListen.Text == "Stop listening" && _listenServer.Listening)
            {
                try
                {
                    _listenServer.Disconnect();
                    UPnP.DeletePortMap(port);
                }
                finally
                {
                    btnListen.Text = "Start listening";
                    ncPort.Enabled = true;
                    txtPassword.Enabled = true;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ushort port = GetPortSafe();

            if (port == 0)
            {
                MessageBox.Show("Please enter a valid port > 0.", "Please enter a valid port", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Settings.ListenPort = port;
            Settings.AutoListen = chkAutoListen.Checked;
            Settings.ShowPopup = chkPopup.Checked;
            var newPassword = txtPassword.Text;
            if (newPassword != Settings.Password)
                AES.PreHashKey(newPassword);
            Settings.Password = newPassword;
            Settings.UseUPnP = chkUseUpnp.Checked;
            Settings.ShowToolTip = chkShowTooltip.Checked;
            Settings.EnableNoIPUpdater = chkNoIPIntegration.Checked;
            Settings.NoIPHost = txtNoIPHost.Text;
            Settings.NoIPUsername = txtNoIPUser.Text;
            Settings.NoIPPassword = txtNoIPPass.Text;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Discard your changes?", "Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes)
                this.Close();
        }

        private void chkNoIPIntegration_CheckedChanged(object sender, EventArgs e)
        {
            NoIPControlHandler(chkNoIPIntegration.Checked);
        }

        private void NoIPControlHandler(bool enable)
        {
            lblHost.Enabled = enable;
            lblUser.Enabled = enable;
            lblPass.Enabled = enable;
            txtNoIPHost.Enabled = enable;
            txtNoIPUser.Enabled = enable;
            txtNoIPPass.Enabled = enable;
            chkShowPassword.Enabled = enable;
        }

        private void ShowPassword(bool show = true)
        {
            txtNoIPPass.PasswordChar = (show) ? (char)0 : (char)'●';
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            ShowPassword(chkShowPassword.Checked);
        }
    }
}