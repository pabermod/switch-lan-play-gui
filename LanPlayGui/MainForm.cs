using LanPlayGui.Extensions;
using LanPlayGui.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LanPlayGui
{
    public partial class MainForm : Form
    {
        private const string serverListFileName = "serverlist.txt";
        private const string releasesUrl = "https://api.github.com/repos/spacemeowx2/switch-lan-play/releases";

        private static string executableName = Environment.Is64BitOperatingSystem ? "lan-play-win64.exe" : "lan-play-win32.exe";
        private static HttpClient httpClient = new HttpClient();

        private IEnumerable<Uri> serverList = new List<Uri>();
        private Uri currentServer = null;

        public MainForm()
        {
            InitializeComponent();
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

            Release release = await GetLatestReleaseAsync();
            if (!File.Exists(executableName))
            {
                toolStripStatusLabel1.Text = "Downloading LanPlay...";
                if(!await DownloadLanPlayExecutable(release))
                {
                    toolStripStatusLabel1.Text = executableName + " not found";
                    return;
                }
            }
            else
            {
                if (release == null)
                {
                    toolStripStatusLabel1.Text = "Error checking for LanPlay updates";
                }
                else if (await CheckUpdate(release))
                {
                    DialogResult result = MessageBox.Show("An update is available. Do you want to download it?", "Update available", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        toolStripStatusLabel1.Text = "Updating LanPlay...";
                        if (!await DownloadLanPlayExecutable(release))
                        {
                            toolStripStatusLabel1.Text = "Error downloading LanPlay update";
                            return;
                        }
                    }
                    toolStripStatusLabel1.Text = "Ready";
                }               
            }

            button1.Enabled = true;
        }

        private void ListBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            currentServer = (Uri)listBox1.SelectedItem;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (currentServer == null)
                return;

            Process.Start(executableName, "--relay-server-addr " + currentServer.AbsoluteUri);
        }

        private async Task<Release> GetLatestReleaseAsync()
        {
            IEnumerable<Release> releases = new List<Release>();

            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("LanPlayGui");
            try
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(releasesUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string resultString = await response.Content.ReadAsStringAsync();
                        releases = JsonConvert.DeserializeObject<IEnumerable<Release>>(resultString);
                    }
                }
                return releases.FirstOrDefault();
            }
            catch (HttpRequestException)
            {
                return null;
            }

        }

        private async Task<bool> DownloadLanPlayExecutable(Release release)
        {
            if (release == null)
                return false;
            Asset executableAsset = release.Assets.FirstOrDefault(a => a.Name == executableName);
            if (executableAsset == null)
                return false;

            try
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(executableAsset.BrowserDownloadUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        await response.Content.ReadAsFileAsync(executableName, true);
                    }
                }
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        private async Task<bool> CheckUpdate(Release release)
        {
            if (release == null)
                return false;
            var checkSumAsset = release.Assets.FirstOrDefault(a => a.Name == "sha1sum.txt");

            if (checkSumAsset == null)
                return false;

            string responseText = null;
            try
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(checkSumAsset.BrowserDownloadUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        responseText = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (HttpRequestException)
            {
                return false;
            }

            if (string.IsNullOrEmpty(responseText))
                return false;

            IEnumerable<string> checkSumList = responseText.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            string checkSum = checkSumList.Where(s => s.Contains(executableName))
                .Select(s => s.Split(' ').FirstOrDefault()).FirstOrDefault();

            if (string.IsNullOrEmpty(checkSum))
                return false;

            return !ComputeFileCheckSum(executableName).Equals(checkSum, StringComparison.InvariantCultureIgnoreCase);
        }

        private string ComputeFileCheckSum(string filePath)
        {
            StringBuilder formatted = null;
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            using (BufferedStream bs = new BufferedStream(fs))
            {
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    byte[] hash = sha1.ComputeHash(bs);
                    formatted = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }
                }
            }

            return formatted?.ToString();
        }
    }
}