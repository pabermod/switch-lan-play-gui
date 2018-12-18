using LanPlayGui.Extensions;
using LanPlayGui.Model;
using LanPlayGui.Model.GitHub;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LanPlayGui.Service
{
    public class LanPlayService
    {
        private const string releasesUrl = "https://api.github.com/repos/spacemeowx2/switch-lan-play/releases";

        private static readonly string executableName = Environment.Is64BitOperatingSystem ? "lan-play-win64.exe" : "lan-play-win32.exe";

        private HttpClient httpClient = new HttpClient();
        private Process lanPlayProcess;

        public void Start(ILanPlayServer server, string interfaceName)
        {
            lanPlayProcess = Process.Start(executableName, $"--relay-server-addr {server.Uri.AbsoluteUri} --netif {interfaceName}");
        }

        public void Stop()
        {
            if(lanPlayProcess != null && !lanPlayProcess.HasExited)
            {
                lanPlayProcess.Kill();
            }
        }

        public string GetExecutableName()
        {
            return executableName;
        }

        public bool IsLanPlayPresent()
        {
            return File.Exists(GetExecutableName());
        }

        public async Task<IRelease> GetLatestReleaseAsync()
        {
            IEnumerable<IRelease> releases = new List<IRelease>();

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

        public async Task<bool> DownloadLanPlayExecutable(IRelease release)
        {
            if (release == null)
                return false;
            IAsset executableAsset = release.Assets.FirstOrDefault(a => a.Name == executableName);
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

        public async Task<bool> CheckUpdate(IRelease release)
        {
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

            return !ComputeLanPlayCheckSum(executableName).Equals(checkSum, StringComparison.InvariantCultureIgnoreCase);
        }

        private string ComputeLanPlayCheckSum(string filePath)
        {
            StringBuilder formatted = null;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
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
