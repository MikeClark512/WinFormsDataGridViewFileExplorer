using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsDataGridViewFileExplorer
{
    /// <summary>
    /// See README.md
    /// </summary>
    public partial class Form1 : Form
    {
        private const string ValueColumnName = "_Value_";

        public Form1()
        {
            InitializeComponent();

            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Control;
            dataGridView1.RowHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Control;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.Empty;
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AllowUserToResizeColumns = true;
            dataGridView1.MultiSelect = false;
            dataGridView1.DataSourceChanged += DataGridView1OnDataSourceChanged;
            dataGridView1.BackgroundColor = SystemColors.ControlLightLight;
        }

        private void DataGridView1OnDataSourceChanged(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBoxInput.Text = Properties.Settings.Default.LastPath;
            ShowDirectory();
        }

        private void ShowDirectory(string path)
        {
            textBoxInput.ForeColor = SystemColors.ControlText;
            if (path == null || path.Trim().Length == 0) return;
            FileSystemInfo[] fsInfos;
            (dataGridView1.DataSource as DataTable)?.Rows.Clear();
            try
            {
                var di = new DirectoryInfo(path);
                fsInfos = di.GetFileSystemInfos();
            }
            catch (Exception)
            {
                // Probably the user-entered path just doesn't exist.
                // Color it red until it does exist.
                textBoxInput.ForeColor = Color.Red;
                return;
            }
            DataTable dt = new DataTable();
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add(ValueColumnName, typeof(FileSystemInfo));
            foreach (var fsInfo in fsInfos)
            {
                bool isFile = fsInfo is FileInfo;
                var name = isFile ? fsInfo.Name : $@".\{fsInfo}\";
                dt.Rows.Add(name, fsInfo.LastWriteTime, fsInfo);
            }
            dataGridView1.DataSource = dt;
            // ReSharper disable once PossibleNullReferenceException
            dataGridView1.Columns[ValueColumnName].Visible = false;
            var c0 = dataGridView1.Columns[0];
            var c1 = dataGridView1.Columns[1];
            c0.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            c1.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            float cw0 = c0.Width;
            float cw1 = c1.Width;
            float sumWidth = cw0 + cw1;
            dataGridView1.Columns[0].FillWeight = cw0 / sumWidth;
            dataGridView1.Columns[1].FillWeight = cw1 / sumWidth;
            c1.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            c1.MinimumWidth = c1.Width;
        }

        private void ShowDirectory()
        {
            ShowDirectory(textBoxInput.Text);
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (sender is DataGridView g)
            {
                var p = g.PointToClient(MousePosition);
                var hti = g.HitTest(p.X, p.Y);
                //var columnIndex = hti.ColumnIndex;
                if (hti.Type == DataGridViewHitTestType.ColumnHeader) return;
                //var rowIndex = hti.RowIndex;
                if (hti.Type == DataGridViewHitTestType.RowHeader) return;
            }
            
            var fsInfo = DgvGetSingleValue<FileSystemInfo>(dataGridView1);
            textBoxOutput.Text = fsInfo.FullName;
            if (fsInfo is DirectoryInfo di)
            {
                textBoxInput.Text = di.FullName;
            }
        }

        private static T DgvGetSingleValue<T>(DataGridView dgv, string valueColumnName = ValueColumnName)
        {
            // ReSharper disable once PossibleNullReferenceException
            int valueColumnIndex = dgv.Columns[ValueColumnName].Index;
            T value = default;
            if (dgv.SelectedRows.Count > 0)
            {
                value = (T)dgv.SelectedRows[0].Cells[valueColumnIndex].Value;
            }
            return value;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.LastPath = textBoxInput.Text;
            Properties.Settings.Default.Save();
        }

        private void TextBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ShowDirectory(textBoxInput.Text);
            }
            else if (e.KeyCode == Keys.Down)
            {
                dataGridView1.Focus();
                if (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.Rows[0].Selected = true;
                }

            }
            else if (e.KeyCode == Keys.Up)
            {
                e.Handled = true;
            }
        }

        private void TextBoxInput_TextChanged(object sender, EventArgs e)
        {
            ShowDirectory(textBoxInput.Text);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Alt) && e.KeyCode == Keys.Up)
            {
                TryNavigateParentDir();
            }
        }

        private void ButtonUp_Click(object sender, EventArgs e)
        {
            TryNavigateParentDir();
        }

        private void TryNavigateParentDir()
        {
            try
            {
                var parent = Directory.GetParent(textBoxInput.Text);
                textBoxInput.Text = parent.FullName;
                textBoxInput.SelectionStart = textBoxInput.Text.Length;
            }
            catch (Exception)
            {
                // only a best effort to parse user input as path info; failure is OK
            }
        }

        private void DataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up 
                && dataGridView1.SelectedRows.Count > 0 
                && dataGridView1.SelectedRows[0].Index == 0)
            {
                textBoxInput.Focus();
            }
        }

        private void DataGridView1_Leave(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }
    }
}
