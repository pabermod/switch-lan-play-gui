using LanPlayGui.Model;
using LanPlayGui.Model.GitHub;
using LanPlayGui.Service;
using PcapDotNet.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LanPlayGui
{
    public partial class MainForm : Form
    {
        private ILanPlayServer currentServer;
        private LanPlayService lanPlayService;
        private LanPlayServerService serverService;
        private bool isSortAscending;
        private DataGridViewColumn sortColumn;

        public MainForm()
        {
            InitializeComponent();

            lanPlayService = new LanPlayService();
            serverService = new LanPlayServerService();
        }

        private async void Form1_LoadAsync(object sender, EventArgs e)
        {
            // Retrieve the device list from the local machine
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            if (allDevices.Count == 0)
            {
                Console.WriteLine("No interfaces found! Make sure WinPcap is installed.");
            }
            else
            {
                // Print the list
                for (int i = 0; i != allDevices.Count; ++i)
                {
                    LivePacketDevice device = allDevices[i];
                    Console.Write((i + 1) + ". " + device.Name);
                    if (device.Description != null)
                        Console.WriteLine(" (" + device.Description + ")");
                    else
                        Console.WriteLine(" (No description available)");
                }
            }

            toolStripStatusLabel1.Text = "Checking for LanPlay updates...";

            await serverService.InitializeAsync();

            bindingSource1.DataSource = new SortableBindingList<ILanPlayServer>(serverService.Servers.ToList());

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = bindingSource1;
            dataGridView1.Columns["Uri"].Visible = false;
            dataGridView1.BackgroundColor = SystemColors.Control;

            serverService.UpdateServersStatus();

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
        }

        private void DataGrid_SelectedValueChangedAsync(object sender, EventArgs e)
        { 
            currentServer = (ILanPlayServer)dataGridView1.CurrentRow?.DataBoundItem;
            var task = Task.Run(() => serverService.UpdateServerStatus(currentServer));
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (currentServer == null)
                return;

            lanPlayService.Start(currentServer);
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Status")
            {
                if ((ServerStatus)e.Value == ServerStatus.Offline)
                {
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
                }
                else
                {
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Green;
                }
            }
        }

        private void DataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewColumn column = dataGridView1.Columns[e.ColumnIndex];

            isSortAscending = (sortColumn == null || isSortAscending == false);

            ListSortDirection direction = isSortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            dataGridView1.Sort(column, direction);

            if (sortColumn != null) sortColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
            column.HeaderCell.SortGlyphDirection = isSortAscending ? SortOrder.Ascending : SortOrder.Descending;
            sortColumn = column;
        }
    }
}