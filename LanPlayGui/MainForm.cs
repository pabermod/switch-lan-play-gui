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
using LanPlayGui.Extensions;
using FontAwesome.Sharp;
using System.IO;

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
            IList<LivePacketDevice> allDevices = new List<LivePacketDevice>();
            try
            {
                allDevices  = LivePacketDevice.AllLocalMachine;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "WinPcap not found", MessageBoxButtons.OK);
                button1.Enabled = false;
                Close();
            }


            if (allDevices.Count == 0)
            {
                MessageBox.Show("No interfaces found! Make sure WinPcap is installed.", "WinPcap not found", MessageBoxButtons.OK);
                button1.Enabled = false;
                return;
            }
            else
            {
                InitializeComboBox(allDevices);
            }

            toolStripStatusLabel1.Text = "Checking for LanPlay updates...";

            await serverService.InitializeAsync();

            InitializeDataGrid();

            serverService.UpdateServersStatus();
            DownloadOrUpdateLanPlay();
        }

        private async void DownloadOrUpdateLanPlay()
        {
            IRelease release = await lanPlayService.GetLatestReleaseAsync();
            if (!lanPlayService.IsLanPlayPresent())
            {
                toolStripStatusLabel1.Text = "Downloading LanPlay...";
                if (!await lanPlayService.DownloadLanPlayExecutable(release))
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

        private void InitializeComboBox(IList<LivePacketDevice> allDevices)
        {
            interfaceSource.DataSource = new BindingList<LivePacketDevice>(allDevices
                .Where(i => i.Addresses.Any(a => a.Netmask.Family != SocketAddressFamily.Unspecified)).ToList());
            comboBox1.DisplayMember = "Description";
            comboBox1.DataSource = interfaceSource;
            comboBox1.DropDownWidth = DropDownWidth(comboBox1);
        }

        private void InitializeDataGrid()
        {
            serverSource.DataSource = new SortableBindingList<ILanPlayServer>(serverService.Servers.ToList());

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = serverSource;
            dataGridView1.Columns["Uri"].Visible = false;
            dataGridView1.BackgroundColor = SystemColors.Control;
        }

        private int DropDownWidth(ComboBox myCombo)
        {
            int maxWidth = 0, temp = 0;
            foreach (var obj in myCombo.Items)
            {
                temp = TextRenderer.MeasureText((obj as LivePacketDevice).Description, myCombo.Font).Width;
                if (temp > maxWidth)
                {
                    maxWidth = temp;
                }
            }
            return maxWidth;
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

            string value = (comboBox1.SelectedValue as LivePacketDevice).Name;
            value = value.Substring(value.IndexOf('\\'));
            lanPlayService.Start(currentServer, value);
            lanPlayService.Exited += LanPlayService_Exited;
            button1.Enabled = false;
        }

        private void LanPlayService_Exited(object sender, EventArgs e)
        {
            button1.InvokeIfRequired(b => b.Enabled = true);
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            lanPlayService.Stop();
        }
    }
}