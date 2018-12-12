using LanPlayGui.Extensions;
using LanPlayGui.Model;
using LanPlayGui.Service;
using Newtonsoft.Json;
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
        private const string serverListFileName = "serverlist.txt";

        private IEnumerable<Uri> serverList;
        private ILanPlayServer currentServer;
        private LanPlayService lanPlayService;

        public MainForm()
        {
            InitializeComponent();

            serverList = new List<Uri>(); ;
            lanPlayService = new LanPlayService();
        }

        private async void Form1_LoadAsync(object sender, EventArgs e)
        {
            button1.Enabled = false;
            toolStripStatusLabel1.Text = "Checking for LanPlay updates...";

            IList<Uri> servers = new List<Uri>();
            using (TextReader reader = File.OpenText(serverListFileName))
            {
                string line;
                while((line = await reader.ReadLineAsync()) != null)
                {
                    if (!line.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                        line = "http://" + line;
                    servers.Add(new Uri(line));
                }
                    
            }
            serverList = servers.OrderBy(u => u.Host);
            bindingSource1.DataSource = serverList;

            listBox1.DataSource = bindingSource1;
            listBox1.DisplayMember = "Host";

            IRelease release = await lanPlayService.GetLatestReleaseAsync();
            if (!File.Exists(lanPlayService.GetExecutableName()))
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
            currentServer = new LanPlayServer((Uri)listBox1.SelectedItem);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (currentServer == null)
                return;

            lanPlayService.Start(currentServer);
        }

    }
}