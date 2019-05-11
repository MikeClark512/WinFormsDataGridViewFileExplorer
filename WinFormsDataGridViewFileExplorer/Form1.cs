using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using WinFormsDataGridViewFileExplorer.Properties;

namespace WinFormsDataGridViewFileExplorer
{
    /// <summary>
    ///     See README.md
    /// </summary>
    public partial class Form1 : Form
    {
        private const string ValueColumnName = "_Value_";
        private readonly DataTable _dataTable = new DataTable();

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

            _dataTable.Columns.Add("Name", typeof(string));
            _dataTable.Columns.Add("Date", typeof(DateTime));
            _dataTable.Columns.Add(ValueColumnName, typeof(FileSystemInfo));
        }

        private void DataGridView1OnDataSourceChanged(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBoxInput.Text = Settings.Default.LastPath;
            ShowDirectory();
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (sender is DataGridView g)
            {
                Point p = g.PointToClient(MousePosition);
                DataGridView.HitTestInfo hti = g.HitTest(p.X, p.Y);
                if (hti.Type == DataGridViewHitTestType.ColumnHeader || hti.Type == DataGridViewHitTestType.RowHeader)
                {
                    return; // Do not process double-clicks on header fields as a command from the user to navigate
                }
            }
            NavigateSelectedPath();
        }

        private void TextBoxInput_TextChanged(object sender, EventArgs e)
        {
            bool? isDir = IsDirectory(textBoxInput.Text);
            if (isDir.HasValue)
            {
                textBoxInput.ForeColor = SystemColors.ControlText;
                if (isDir.Value)
                {
                    NavigateDir(textBoxInput.Text);
                }
            }
            else
            {
                textBoxInput.ForeColor = Color.Red;
            }
        }

        private void TextBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NavigatePath(textBoxInput.Text);
            }
            else if (e.KeyCode == Keys.Down)
            {
                dataGridView1.Focus();
                TrySelectFirstRow();
            }
            else if (e.KeyCode == Keys.Up)
            {
                e.Handled = true;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Up)
            {
                TryNavigateParentDir();
            }
        }

        private void ButtonUp_Click(object sender, EventArgs e)
        {
            TryNavigateParentDir();
        }

        private void DataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up && !e.Alt
                && dataGridView1.SelectedRows.Count > 0
                && dataGridView1.SelectedRows[0].Index == 0)
            {
                textBoxInput.Focus();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                var fsInfo = GetPrimarySelectedValue<FileSystemInfo>(dataGridView1);
                NavigatePath(fsInfo?.FullName);
                e.Handled = true;
            }
        }

        private void DataGridView1_Leave(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private void DataGridView1_Enter(object sender, EventArgs e)
        {
            //if (GetPrimarySelectedValue() == null)
            //{
            //    TrySelectFirstRow(); // If there is data in the grid and no selection, make a selection
            //}
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.LastPath = textBoxInput.Text;
            Settings.Default.Save();
        }

        private void NavigatePath(string path)
        {
            if (path == null || path.Trim().Length == 0)
            {
                return;
            }
            bool? isDir = IsDirectory(path);
            if (isDir.HasValue)
            {
                if (isDir.Value)
                {
                    NavigateDir(path);
                }
                else
                {
                    NavigateFile(path);
                }
            }
            // else path is not a file or directory -- must be invalid or non-existant
        }

        private void NavigateFile(string path)
        {
            textBoxOutput.Text = path;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var fsi = (FileSystemInfo)row.Cells[ValueColumnName].Value;
                if (PathsEqual(fsi.FullName, path))
                {
                    row.Selected = true;
                }
            }
        }

        [System.Diagnostics.DebuggerNonUserCode]
        public static string NormalizePath(string path)
        {
            try { path = new Uri(path).LocalPath; } catch (Exception) { /*path cleanup*/ } 
            return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static bool PathsEqual(FileSystemInfo fsi, string path2)
        {
            return PathsEqual(fsi?.FullName, path2);
        }

        private static bool PathsEqual(FileSystemInfo fsi1, FileSystemInfo fsi2)
        {
            return PathsEqual(fsi1?.FullName, fsi2?.FullName);
        }

        private static bool PathsEqual(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2))
            {
                return false;
            }
            string np1 = NormalizePath(path1);
            string np2 = NormalizePath(path2);
            return np1.Equals(np2, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool? IsDirectory(string path)
        {
            if (Directory.Exists(path)) return true;
            if (File.Exists(path)) return false;
            return null;
        }

        private void NavigateDir(string path)
        {
            textBoxInput.Text = path;
            FileSystemInfo[] fsInfos;
            try
            {
                var di = new DirectoryInfo(path);
                fsInfos = di.GetFileSystemInfos();
            }
            catch (Exception)
            {
                // Probably the user-entered path just doesn't exist.
                return;
            }

            if (ParentPathEquals(GetPrimarySelectedValue(), path)) return;

            _dataTable.Rows.Clear();
            foreach (FileSystemInfo fsInfo in fsInfos)
            {
                bool isFile = fsInfo is FileInfo;
                string name = isFile ? fsInfo.Name : $@".\{fsInfo}\";
                _dataTable.Rows.Add(name, fsInfo.LastWriteTime, fsInfo);
            }

            dataGridView1.DataSource = _dataTable;
            DataGridViewColumn valueColumn = dataGridView1.Columns[ValueColumnName];
            if (valueColumn != null) valueColumn.Visible = false;
            DataGridViewColumn c0 = dataGridView1.Columns[0];
            DataGridViewColumn c1 = dataGridView1.Columns[1];
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

        private bool ParentPathEquals(FileSystemInfo fsi, string path)
        {
            if (fsi == null) return false;
            return PathsEqual(Directory.GetParent(fsi.FullName), path);
        }

        private void ShowDirectory()
        {
            NavigatePath(textBoxInput.Text);
        }

        private void NavigateSelectedPath()
        {
            FileSystemInfo fsInfo = GetPrimarySelectedValue();
            NavigatePath(fsInfo?.FullName);
        }

        private FileSystemInfo GetPrimarySelectedValue()
        {
            return GetPrimarySelectedValue<FileSystemInfo>(dataGridView1);
        }

        private static T GetPrimarySelectedValue<T>(DataGridView dgv, string valueColumnName = ValueColumnName)
        {
            int? columnIndex = dgv.Columns[valueColumnName]?.Index;
            T value = default(T);
            if (columnIndex.HasValue && dgv.SelectedRows.Count > 0)
            {
                value = (T)dgv.SelectedRows[0].Cells[columnIndex.Value].Value;
            }
            return value;
        }

        private void TrySelectFirstRow()
        {
            if (dataGridView1.Rows.Count > 0)
            {
                dataGridView1.Rows[0].Selected = true;
            }
        }

        private void TryNavigateParentDir()
        {
            try
            {
                DirectoryInfo parent = Directory.GetParent(textBoxInput.Text);
                if (parent != null) textBoxInput.Text = parent.FullName;
                textBoxInput.SelectionStart = textBoxInput.Text.Length;
            }
            catch (Exception)
            {
                // only a best effort to parse user input as path info; failure is OK
            }
        }
    }
}