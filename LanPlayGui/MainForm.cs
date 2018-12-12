using LanPlayGui.Model;
using LanPlayGui.Model.GitHub;
using LanPlayGui.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LanPlayGui
{
    public partial class MainForm : Form
    {
        private ILanPlayServer currentServer;
        private LanPlayService lanPlayService;
        private LanPlayServerService serverService;

        public MainForm()
        {
            InitializeComponent();

            lanPlayService = new LanPlayService();
            serverService = new LanPlayServerService();
        }

        private async void Form1_LoadAsync(object sender, EventArgs e)
        {
            button1.Enabled = false;
            toolStripStatusLabel1.Text = "Checking for LanPlay updates...";

            await serverService.InitializeAsync();

            bindingSource1.DataSource = serverService.Servers.OrderBy(u => u.Uri.Host);
            listBox1.DataSource = bindingSource1;

            IRelease release = await lanPlayService.GetLatestReleaseAsync();
            if (!lanPlayService.IsLanPlayPresent())
            {
                toolStripStatusLabel1.Text = "Downloading LanPlay...";
                if(!await lanPlayService.DownloadLanPlayExecutable(release))
                {
                    toolStripStatusLabel1.Text = lanPlayService.GetExecutableName() + " not found";
                    return;
                }
                toolStripStatusLabel1.Text = "Ready";
            }
            else
            {
                if (release == null)
                {
                    toolStripStatusLabel1.Text = "Error checking for LanPlay updates";
                }
                else if (await lanPlayService.CheckUpdate(release))
                {
                    DialogResult result = MessageBox.Show("An update is available. Do you want to download it?", "Update available", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        toolStripStatusLabel1.Text = "Updating LanPlay...";
                        if (!await lanPlayService.DownloadLanPlayExecutable(release))
                        {
                            toolStripStatusLabel1.Text = "Error downloading LanPlay update";
                            return;
                        }
                    }
                    toolStripStatusLabel1.Text = "Ready";
                }
                else
                {
                    toolStripStatusLabel1.Text = "Ready";
                }
            }

            button1.Enabled = true;
        }

        private void ListBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            currentServer = (ILanPlayServer)listBox1.SelectedItem;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (currentServer == null)
                return;

            lanPlayService.Start(currentServer);
        }

    }
}