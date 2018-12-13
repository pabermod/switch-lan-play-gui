using LanPlayGui.Model;
using LanPlayGui.Model.GitHub;
using LanPlayGui.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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
            button1.Enabled = false;
            toolStripStatusLabel1.Text = "Checking for LanPlay updates...";

            await serverService.InitializeAsync();

            bindingSource1.DataSource = new SortableBindingList<ILanPlayServer>(serverService.Servers.ToList());

            dataGridView1.DataSource = bindingSource1;
            dataGridView1.MultiSelect = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.Columns["Uri"].Visible = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

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
            toolStripStatusLabel1.Text = "Updating server list status...";

            await Task.Run(() => serverService.UpdateServersStatus());
            toolStripStatusLabel1.Text = "Ready";
        }

        private void DataGrid_SelectedValueChanged(object sender, EventArgs e)
        { 
            currentServer = (ILanPlayServer)dataGridView1.CurrentRow?.DataBoundItem;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (currentServer == null)
                return;

            lanPlayService.Start(currentServer);
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            foreach (DataGridViewRow Myrow in dataGridView1.Rows)
            {            
                if ((ServerStatus)Myrow.Cells["Status"].Value  == ServerStatus.Offline)// Or your condition 
                {
                    Myrow.DefaultCellStyle.BackColor = Color.Red;
                }
                else
                {
                    Myrow.DefaultCellStyle.BackColor = Color.Green;
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