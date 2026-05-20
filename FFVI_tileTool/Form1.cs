using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFVI_tileTool
{
    public partial class Form1 : Form
    {
        private enum MapCategoryFilter
        {
            Off,
            SnowTiles,
            GrassTiles,
            MagitekTiles,
            ShipDeckTiles
        }

        private enum IsolateDestinationChoice
        {
            Cancel,
            NewFolder,
            ExistingFolder
        }

        private const int DwmwaUseImmersiveDarkMode = 20;
        private const int DwmwaUseImmersiveDarkModeBefore20H1 = 19;
        private const int WhCbt = 5;
        private const int HcbtActivate = 5;
        private const int WmSetIcon = 0x0080;
        private static readonly IntPtr IconSmall = new IntPtr(0);
        private static readonly IntPtr IconBig = new IntPtr(1);
        private const string LastOpenedFileStateName = "last-opened-map.txt";
        private const string RecentDirectoriesStateName = "recent-map-directories.txt";
        private const string DarkModeStateName = "dark-mode.txt";
        private const string BackupReminderStateName = "backup-reminder-shown.txt";
        private const string Preview050505SettingName = "preview-050505.txt";
        private const string DefaultWindowTitle = "FFVI Old Tile Tool";
        private const int MaxRecentDirectories = 8;

        private static readonly HashSet<string> SnowTileMaps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "map211.bin", "map018.bin", "map019.bin", "map020.bin", "map021.bin", "map022.bin", "map023.bin",
            "map032.bin", "map033.bin", "map034.bin", "map035.bin", "map039.bin"
        };

        private static readonly HashSet<string> GrassTileMaps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "map395.bin", "map014.bin", "map047.bin", "map075.bin", "map093.bin", "map115.bin", "map148.bin",
            "map157.bin", "map159.bin", "map169.bin", "map170.bin", "map182.bin", "map185.bin", "map188.bin",
            "map198.bin", "map302.bin", "map340.bin", "map341.bin", "map342.bin", "map343.bin", "map389.bin", "map392.bin"
        };

        private static readonly HashSet<string> MagitekTileMaps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "map091.bin", "map117.bin", "map119.bin", "map187.bin", "map220.bin", "map228.bin", "map332.bin"
        };

        private static readonly HashSet<string> ShipDeckTileMaps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "map011.bin", "map013.bin", "map017.bin", "map215.bin", "map227.bin", "map006.bin", "map010.bin"
        };

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        string[] st;
        private string[] allMapFiles = new string[0];
        private List<string> recentDirectories = new List<string>();
        private List<int> currentSection1PaletteOffsets = new List<int>();
        private List<int> currentSection2PaletteOffsets = new List<int>();
        private byte[] currentSection1Palette = new byte[1024];
        private byte[] currentSection2Palette = new byte[1024];
        private bool currentHasSection2Palette;
        private MapCategoryFilter activeMapFilter = MapCategoryFilter.Off;
        private bool backupReminderHandledSession;
        private ToolStripMenuItem previewTreat050505AsTransparentToolStripMenuItem;
        private bool previewTreat050505AsTransparent;
        private Bitmap currentSection1SourceBitmap;
        private Bitmap currentSection2SourceBitmap;
        private Bitmap previewCheckerBackgroundBitmap;
        private bool isSyncingFilterDropdown;
        private static Icon cachedApplicationIcon;
        private Panel aboutOverlayBackdrop;
        private Panel aboutOverlayCard;
        private Panel aboutOverlayContentHost;
        private Button aboutOverlayCloseButton;
        private AboutForm aboutOverlayContent;

        public Form1()
        {
            InitializeComponent();
            Icon = GetApplicationIcon();
            Text = DefaultWindowTitle;

            comboBoxFileFilter.Items.AddRange(new object[]
            {
                "All Maps",
                "Snow Tiles",
                "Grass Tiles",
                "Magitek Tiles",
                "Ship Deck Tiles"
            });
            comboBoxFileFilter.DrawMode = DrawMode.OwnerDrawFixed;
            comboBoxFileFilter.DrawItem += comboBoxFileFilter_DrawItem;
            comboBoxFileFilter.SelectedIndex = 0;

            bool darkModeEnabled = LoadDarkModeState();
            darkModeToolStripMenuItem.Checked = darkModeEnabled;
            ApplyTheme(darkModeEnabled);
            UpdatePreviewTransparencyBackground();

            bool preview050505Enabled = LoadPreview050505State();
            previewTreat050505AsTransparentToolStripMenuItem.Checked = preview050505Enabled;

            recentDirectories = LoadRecentDirectories();
            RefreshRecentDirectoriesMenu();

            RestoreLastOpenedFile();
            this.SizeChanged += (s, e) => menuStrip1.Invalidate();
            this.SizeChanged += (s, e) => CenterAboutOverlayCard();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyTitleBarTheme(darkModeToolStripMenuItem.Checked);
        }

        private void browseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string lastOpenedFile = LoadLastOpenedFile();
            string initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!string.IsNullOrWhiteSpace(lastOpenedFile) && File.Exists(lastOpenedFile))
                initialDirectory = Path.GetDirectoryName(lastOpenedFile);

            using (OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select folder containing map files",
                InitialDirectory = initialDirectory,
                FileName = "Select Folder",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true
            })
            {
                if (ShowThemedCommonDialog(dialog, this) != DialogResult.OK) return;

                string selectedFolder = Path.GetDirectoryName(dialog.FileName);
                if (string.IsNullOrWhiteSpace(selectedFolder) || !Directory.Exists(selectedFolder)) return;

                LoadMapFilesFromFolder(selectedFolder, null);

                if (!EnsureMapBinFilesOrOfferDecompression(selectedFolder))
                {
                    ShowAppMessage("No map*.bin files found in the selected folder.", "Browse", MessageBoxIcon.Warning);
                    return;
                }

                string firstMapFile = Directory.EnumerateFiles(selectedFolder, "map*.bin").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(firstMapFile))
                    SaveLastOpenedFile(firstMapFile);
                AddRecentDirectory(selectedFolder);
            }
        }

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lbpi);

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern bool SHGetPathFromIDList(IntPtr pidl, System.Text.StringBuilder pszPath);

        [StructLayout(LayoutKind.Sequential)]
        private struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public IntPtr pszDisplayName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszTitle;
            public uint ulFlags;
            public IntPtr lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        private string PickModernFolder()
        {
            try
            {
                var bi = new BROWSEINFO();
                bi.hwndOwner = Handle;
                bi.lpszTitle = "Select folder containing map files";
                bi.ulFlags = 0x0040 | 0x0001; // BIF_RETURNONLYFSDIRS | BIF_USENEWUI (modern style)

                IntPtr pidl = SHBrowseForFolder(ref bi);

                if (pidl != IntPtr.Zero)
                {
                    var sb = new System.Text.StringBuilder(260);
                    SHGetPathFromIDList(pidl, sb);
                    Marshal.FreeCoTaskMem(pidl);
                    return sb.ToString();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private void browseToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            RefreshRecentDirectoriesMenu();
        }

        private void previewTreat050505AsTransparentToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            previewTreat050505AsTransparent = previewTreat050505AsTransparentToolStripMenuItem.Checked;
            SavePreview050505State(previewTreat050505AsTransparent);
            UpdatePreviewTransparencyBackground();
            if (listBox1.SelectedValue is string selectedName && !string.IsNullOrWhiteSpace(selectedName) && st != null)
            {
                string selectedPath = st.FirstOrDefault(x => string.Equals(Path.GetFileName(x), selectedName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(selectedPath) && File.Exists(selectedPath))
                    RenderImage(selectedPath);
            }
        }

        private void recentDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem menuItem)) return;

            string folderPath = menuItem.Tag as string;
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                ShowAppMessage("Directory is no longer available.", "Recent directories", MessageBoxIcon.Warning);
                recentDirectories.RemoveAll(x => string.Equals(x, folderPath, StringComparison.OrdinalIgnoreCase));
                SaveRecentDirectories(recentDirectories);
                RefreshRecentDirectoriesMenu();
                return;
            }

            string fileToSelect = null;
            string lastOpenedFile = LoadLastOpenedFile();
            if (!string.IsNullOrWhiteSpace(lastOpenedFile) && File.Exists(lastOpenedFile) &&
                string.Equals(Path.GetDirectoryName(lastOpenedFile), folderPath, StringComparison.OrdinalIgnoreCase))
                fileToSelect = lastOpenedFile;

            LoadMapFilesFromFolder(folderPath, fileToSelect);

            if (!EnsureMapBinFilesOrOfferDecompression(folderPath))
            {
                ShowAppMessage("No map*.bin files found in the selected folder.", "Recent directories", MessageBoxIcon.Warning);
                return;
            }

            string selectedMapPath = GetSelectedMapFilePath();
            if (!string.IsNullOrWhiteSpace(selectedMapPath))
                SaveLastOpenedFile(selectedMapPath);

            AddRecentDirectory(folderPath);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox listBox = (sender as ListBox);
            if (listBox.Items.Count == 0) return;
            if (st == null) return;
            if (st.Length == 0) return;

            string filePath = GetSelectedMapFilePath();
            if (string.IsNullOrWhiteSpace(filePath)) return;
            RenderImage(filePath);
            SaveLastOpenedFile(filePath);
            UpdateWindowTitle(filePath);
        }

        private string GetSelectedMapFilePath()
        {
            if (st == null || st.Length == 0 || listBox1.SelectedItem == null) return null;

            string selectedName = listBox1.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(selectedName)) return null;

            return st.FirstOrDefault(x => string.Equals(Path.GetFileName(x), selectedName, StringComparison.OrdinalIgnoreCase));
        }

        private List<string> GetSelectedMapFilePaths()
        {
            List<string> selectedPaths = new List<string>();
            if (st == null || st.Length == 0) return selectedPaths;

            foreach (object selectedItem in listBox1.SelectedItems)
            {
                string selectedName = selectedItem as string;
                if (string.IsNullOrWhiteSpace(selectedName)) continue;

                string filePath = st.FirstOrDefault(x => string.Equals(Path.GetFileName(x), selectedName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(filePath) && !selectedPaths.Contains(filePath, StringComparer.OrdinalIgnoreCase))
                    selectedPaths.Add(filePath);
            }

            return selectedPaths;
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            int index = listBox1.IndexFromPoint(e.Location);
            if (index == ListBox.NoMatches) return;

            bool isAlreadySelected = listBox1.SelectedIndices.Contains(index);
            if (!isAlreadySelected)
            {
                listBox1.ClearSelected();
                listBox1.SelectedIndex = index;
            }
        }

        private void revealInFileExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filePath = GetSelectedMapFilePath();
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                ShowAppMessage("Unable to locate the selected file.", "Reveal in File Explorer", MessageBoxIcon.Warning);
                return;
            }

            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{filePath}\"") { UseShellExecute = true });
        }

        private void gzipThisFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> selectedFiles = GetSelectedMapFilePaths();
            if (selectedFiles.Count == 0)
            {
                string fallbackFile = GetSelectedMapFilePath();
                if (!string.IsNullOrWhiteSpace(fallbackFile))
                    selectedFiles.Add(fallbackFile);
            }

            if (selectedFiles.Count == 0)
            {
                ShowAppMessage("Unable to locate the selected file(s).", "Gzip file", MessageBoxIcon.Warning);
                return;
            }

            int gzippedCount = 0;
            int failedCount = 0;
            string singleOutputPath = null;
            List<string> failedFiles = new List<string>();

            foreach (string filePath in selectedFiles)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    {
                        failedCount++;
                        failedFiles.Add($"{Path.GetFileName(filePath ?? "(unknown)")}: file not found");
                        continue;
                    }

                    string gzipPath = filePath + ".gz";
                    if (File.Exists(gzipPath))
                        File.Delete(gzipPath);

                    using (FileStream inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (FileStream outputStream = new FileStream(gzipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (GZipStream gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                        inputStream.CopyTo(gzipStream);

                    gzippedCount++;
                    singleOutputPath = gzipPath;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    failedFiles.Add($"{Path.GetFileName(filePath)}: {ex.Message}");
                }
            }

            if (selectedFiles.Count == 1 && gzippedCount == 1 && failedCount == 0)
            {
                ShowAppMessage($"Gzip created successfully.\n\nOutput: {singleOutputPath}", "Gzip file", MessageBoxIcon.Information);
                return;
            }

            StringBuilder summary = new StringBuilder();
            summary.AppendLine("Gzip operation finished.");
            summary.AppendLine();
            summary.AppendLine($"Selected: {selectedFiles.Count}");
            summary.AppendLine($"Gzipped: {gzippedCount}");
            summary.AppendLine($"Failed: {failedCount}");

            if (failedFiles.Count > 0)
            {
                summary.AppendLine();
                summary.AppendLine("Failures:");
                foreach (string failed in failedFiles.Take(10))
                    summary.AppendLine(failed);

                if (failedFiles.Count > 10)
                    summary.AppendLine($"...and {failedFiles.Count - 10} more.");
            }

            ShowAppMessage(summary.ToString(), "Gzip files", failedCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void isolateSelectedFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> selectedFiles = GetSelectedMapFilePaths();
            if (selectedFiles.Count == 0)
            {
                ShowAppMessage("Select one or more files first. You can hold Ctrl to select multiple files.", "Isolate selected files", MessageBoxIcon.Warning);
                return;
            }

            string sourceFolder = Path.GetDirectoryName(selectedFiles[0]);
            if (string.IsNullOrWhiteSpace(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                ShowAppMessage("Unable to locate current source folder.", "Isolate selected files", MessageBoxIcon.Warning);
                return;
            }

            string isolationRoot = Path.Combine(sourceFolder, "isolation");
            Directory.CreateDirectory(isolationRoot);

            IsolateDestinationChoice choice = ShowIsolateDestinationChoiceDialog();
            if (choice == IsolateDestinationChoice.Cancel)
                return;

            string destinationFolder;
            if (choice == IsolateDestinationChoice.NewFolder)
            {
                destinationFolder = Path.Combine(isolationRoot, $"isolate_{DateTime.Now:yyyyMMdd_HHmmss}");
                Directory.CreateDirectory(destinationFolder);
            }
            else
            {
                destinationFolder = ShowIsolateExistingFolderDialog(isolationRoot);
                if (string.IsNullOrWhiteSpace(destinationFolder))
                    return;

                string normalizedRoot = Path.GetFullPath(isolationRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                string normalizedDestination = Path.GetFullPath(destinationFolder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (!normalizedDestination.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                {
                    ShowAppMessage("Please select a folder inside the isolation directory.", "Isolate selected files", MessageBoxIcon.Warning);
                    return;
                }

                Directory.CreateDirectory(destinationFolder);
            }

            int copiedCount = 0;
            int failedCount = 0;
            foreach (string filePath in selectedFiles)
            {
                try
                {
                    string destinationPath = Path.Combine(destinationFolder, Path.GetFileName(filePath));
                    File.Copy(filePath, destinationPath, true);
                    copiedCount++;
                }
                catch
                {
                    failedCount++;
                }
            }

            DialogResult openDecision = ShowAppMessageWithActions(
                $"Selected files isolated.\n\nCopied: {copiedCount}\nFailed: {failedCount}\nFolder: {destinationFolder}\n\nOpen this folder now?",
                "Isolate selected files",
                "Open Folder",
                "Done",
                failedCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (openDecision == DialogResult.OK)
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{destinationFolder}\"") { UseShellExecute = true });
        }

        private IsolateDestinationChoice ShowIsolateDestinationChoiceDialog()
        {
            bool darkMode = darkModeToolStripMenuItem.Checked;
            using (Form dialog = new Form())
            using (Label messageLabel = new Label())
            using (Panel buttonPanel = new Panel())
            using (Button newFolderButton = new Button())
            using (Button existingFolderButton = new Button())
            using (Button cancelButton = new Button())
            {
                dialog.Text = "Isolate selected files";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(620, 210);

                messageLabel.AutoSize = false;
                messageLabel.Dock = DockStyle.Fill;
                messageLabel.Padding = new Padding(14, 12, 14, 8);
                messageLabel.TextAlign = ContentAlignment.TopLeft;
                messageLabel.Text = "Choose where to isolate the selected files:\n\n- New isolation folder\n- Existing folder inside isolation";

                buttonPanel.Dock = DockStyle.Bottom;
                buttonPanel.Height = 54;
                buttonPanel.Padding = new Padding(0, 12, 12, 12);

                cancelButton.Text = "Cancel";
                cancelButton.Size = new Size(92, 28);
                cancelButton.Dock = DockStyle.Right;
                cancelButton.DialogResult = DialogResult.Cancel;

                existingFolderButton.Text = "Existing Folder";
                existingFolderButton.Size = new Size(122, 28);
                existingFolderButton.Dock = DockStyle.Right;
                existingFolderButton.DialogResult = DialogResult.No;

                newFolderButton.Text = "New Folder";
                newFolderButton.Size = new Size(104, 28);
                newFolderButton.Dock = DockStyle.Right;
                newFolderButton.DialogResult = DialogResult.Yes;

                dialog.AcceptButton = newFolderButton;
                dialog.CancelButton = cancelButton;

                dialog.Controls.Add(messageLabel);
                dialog.Controls.Add(buttonPanel);
                buttonPanel.Controls.Add(cancelButton);
                buttonPanel.Controls.Add(existingFolderButton);
                buttonPanel.Controls.Add(newFolderButton);

                GetThemeColors(darkMode, out System.Drawing.Color background, out System.Drawing.Color surface, out System.Drawing.Color foreground);
                ApplyThemeToControlTree(dialog, background, surface, foreground, darkMode);
                ApplyTitleBarThemeToForm(dialog, darkMode);

                DialogResult result = dialog.ShowDialog(this);
                if (result == DialogResult.Yes) return IsolateDestinationChoice.NewFolder;
                if (result == DialogResult.No) return IsolateDestinationChoice.ExistingFolder;
                return IsolateDestinationChoice.Cancel;
            }
        }

        private string ShowIsolateExistingFolderDialog(string isolationRoot)
        {
            string[] existingFolders = Directory.GetDirectories(isolationRoot)
                .OrderBy(Path.GetFileName)
                .ToArray();

            if (existingFolders.Length == 0)
            {
                ShowAppMessage("No existing folders found under isolation. Choose New Folder instead.", "Isolate selected files", MessageBoxIcon.Information);
                return null;
            }

            bool darkMode = darkModeToolStripMenuItem.Checked;
            using (Form dialog = new Form())
            using (Label messageLabel = new Label())
            using (ListBox folderList = new ListBox())
            using (Panel buttonPanel = new Panel())
            using (Button selectButton = new Button())
            using (Button cancelButton = new Button())
                {
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(680, 340);

                messageLabel.AutoSize = false;
                messageLabel.Dock = DockStyle.Top;
                messageLabel.Height = 52;
                messageLabel.Padding = new Padding(14, 12, 14, 8);
                messageLabel.TextAlign = ContentAlignment.TopLeft;
                messageLabel.Text = "Select an existing folder under isolation:";

                folderList.Dock = DockStyle.Fill;
                folderList.IntegralHeight = false;
                folderList.DisplayMember = "Name";

                foreach (string folder in existingFolders)
                    folderList.Items.Add(folder);

                if (folderList.Items.Count > 0)
                    folderList.SelectedIndex = 0;

                buttonPanel.Dock = DockStyle.Bottom;
                buttonPanel.Height = 54;
                buttonPanel.Padding = new Padding(0, 12, 12, 12);

                cancelButton.Text = "Cancel";
                cancelButton.Size = new Size(92, 28);
                cancelButton.Dock = DockStyle.Right;
                cancelButton.DialogResult = DialogResult.Cancel;

                selectButton.Text = "Select";
                selectButton.Size = new Size(96, 28);
                selectButton.Dock = DockStyle.Right;
                selectButton.DialogResult = DialogResult.OK;

                dialog.AcceptButton = selectButton;
                dialog.CancelButton = cancelButton;

                dialog.Controls.Add(folderList);
                dialog.Controls.Add(messageLabel);
                dialog.Controls.Add(buttonPanel);
                buttonPanel.Controls.Add(cancelButton);
                buttonPanel.Controls.Add(selectButton);

                GetThemeColors(darkMode, out System.Drawing.Color background, out System.Drawing.Color surface, out System.Drawing.Color foreground);
                ApplyThemeToControlTree(dialog, background, surface, foreground, darkMode);
                ApplyTitleBarThemeToForm(dialog, darkMode);

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return null;

                return folderList.SelectedItem as string;
            }
        }

        private void LoadMapFilesFromFolder(string folderPath, string fileToSelect = null)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                allMapFiles = new string[0];
                st = new string[0];
                listBox1.DataSource = null;
                return;
            }

            allMapFiles = Directory.GetFiles(folderPath, "map*.bin", SearchOption.TopDirectoryOnly);
            if (allMapFiles.Length == 0)
            {
                st = new string[0];
                listBox1.DataSource = null;
                return;
            }

            ApplyMapCategoryFilter(fileToSelect);

            MaybeShowFirstRunBackupWarning(folderPath);
        }

        private void ApplyMapCategoryFilter(string fileToSelect = null)
        {
            IEnumerable<string> query = allMapFiles;
            if (activeMapFilter == MapCategoryFilter.SnowTiles)
                query = query.Where(x => SnowTileMaps.Contains(Path.GetFileName(x)));
            else if (activeMapFilter == MapCategoryFilter.GrassTiles)
                query = query.Where(x => GrassTileMaps.Contains(Path.GetFileName(x)));
            else if (activeMapFilter == MapCategoryFilter.MagitekTiles)
                query = query.Where(x => MagitekTileMaps.Contains(Path.GetFileName(x)));
            else if (activeMapFilter == MapCategoryFilter.ShipDeckTiles)
                query = query.Where(x => ShipDeckTileMaps.Contains(Path.GetFileName(x)));

            st = query.OrderBy(Path.GetFileName).ToArray();
            string[] fileNames = st.Select(Path.GetFileName).ToArray();
            listBox1.DataSource = fileNames;

            if (!string.IsNullOrWhiteSpace(fileToSelect))
            {
                string selectedName = Path.GetFileName(fileToSelect);
                if (fileNames.Any(x => string.Equals(x, selectedName, StringComparison.OrdinalIgnoreCase)))
                    listBox1.SelectedItem = selectedName;
            }

            if (listBox1.Items.Count > 0 && listBox1.SelectedIndex < 0)
                listBox1.SelectedIndex = 0;

            if (listBox1.Items.Count == 0)
            {
                pictureBox1.Image = null;
                pictureBox2.Image = null;
                UpdateWindowTitle(null);
            }

            UpdateFilterMenuChecks();
        }

        private void UpdateFilterMenuChecks()
        {
            filterOffToolStripMenuItem.Checked = activeMapFilter == MapCategoryFilter.Off;
            filterSnowTilesToolStripMenuItem.Checked = activeMapFilter == MapCategoryFilter.SnowTiles;
            filterGrassTilesToolStripMenuItem.Checked = activeMapFilter == MapCategoryFilter.GrassTiles;
            filterMagitekTilesToolStripMenuItem.Checked = activeMapFilter == MapCategoryFilter.MagitekTiles;
            filterShipDeckTilesToolStripMenuItem.Checked = activeMapFilter == MapCategoryFilter.ShipDeckTiles;

            if (comboBoxFileFilter == null) return;

            int targetIndex = 0;
            if (activeMapFilter == MapCategoryFilter.SnowTiles) targetIndex = 1;
            else if (activeMapFilter == MapCategoryFilter.GrassTiles) targetIndex = 2;
            else if (activeMapFilter == MapCategoryFilter.MagitekTiles) targetIndex = 3;
            else if (activeMapFilter == MapCategoryFilter.ShipDeckTiles) targetIndex = 4;

            if (comboBoxFileFilter.SelectedIndex != targetIndex)
            {
                isSyncingFilterDropdown = true;
                comboBoxFileFilter.SelectedIndex = targetIndex;
                isSyncingFilterDropdown = false;
            }
        }

        private void SetActiveMapFilter(MapCategoryFilter filter)
        {
            activeMapFilter = filter;
            ApplyMapCategoryFilter();
        }

        private string GetFilterLabel()
        {
            if (activeMapFilter == MapCategoryFilter.SnowTiles) return "SnowTiles";
            if (activeMapFilter == MapCategoryFilter.GrassTiles) return "GrassTiles";
            if (activeMapFilter == MapCategoryFilter.MagitekTiles) return "MagitekTiles";
            if (activeMapFilter == MapCategoryFilter.ShipDeckTiles) return "ShipDeckTiles";
            return "AllMaps";
        }

        private static string[] GetMapGzipFiles(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                return new string[0];

            return Directory.GetFiles(folderPath, "map*.bin.gz", SearchOption.TopDirectoryOnly)
                .OrderBy(Path.GetFileName)
                .ToArray();
        }

        private bool EnsureMapBinFilesOrOfferDecompression(string folderPath)
        {
            if (st != null && st.Length > 0) return true;

            string[] gzipFiles = GetMapGzipFiles(folderPath);
            if (gzipFiles.Length == 0) return false;

            DialogResult decision = ShowAppMessageWithActions(
                "No map*.bin files were found, but map*.bin.gz files were detected.\n\nDo you want to decompress them now?",
                "Compressed map files detected",
                "Decompress",
                "Cancel",
                MessageBoxIcon.Warning);

            if (decision != DialogResult.OK) return false;

            bool cancelled = DecompressMapGzipFilesWithProgress(gzipFiles, "Decompressing map files", out int decompressedCount, out int skippedCount, out int failedCount);
            LoadMapFilesFromFolder(folderPath);

            ShowAppMessage(
                $"Decompression {(cancelled ? "cancelled" : "finished")}.\n\nDecompressed: {decompressedCount}\nSkipped existing: {skippedCount}\nFailed: {failedCount}",
                "Decompression result",
                failedCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            return st != null && st.Length > 0;
        }

        private bool DecompressMapGzipFilesWithProgress(string[] gzipFiles, string title, out int decompressedCount, out int skippedCount, out int failedCount)
        {
            decompressedCount = 0;
            skippedCount = 0;
            failedCount = 0;
            bool cancelRequested = false;

            using (Form progressForm = new Form())
            using (Label statusLabel = new Label())
            using (ProgressBar progressBar = new ProgressBar())
            using (Button cancelButton = new Button())
            {
                progressForm.Text = title;
                progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                progressForm.StartPosition = FormStartPosition.CenterParent;
                progressForm.MinimizeBox = false;
                progressForm.MaximizeBox = false;
                progressForm.ControlBox = false;
                progressForm.ClientSize = new Size(520, 120);

                statusLabel.AutoSize = false;
                statusLabel.TextAlign = ContentAlignment.MiddleLeft;
                statusLabel.Dock = DockStyle.Top;
                statusLabel.Height = 56;
                statusLabel.Text = "Preparing decompression...";

                progressBar.Dock = DockStyle.Bottom;
                progressBar.Height = 24;
                progressBar.Minimum = 0;
                progressBar.Maximum = gzipFiles.Length;
                progressBar.Value = 0;

                cancelButton.Text = "Cancel";
                cancelButton.Size = new Size(90, 26);
                cancelButton.Location = new Point(progressForm.ClientSize.Width - cancelButton.Width - 12, 62);
                cancelButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                cancelButton.Click += (s, evt) =>
                {
                    cancelRequested = true;
                    cancelButton.Enabled = false;
                    statusLabel.Text = "Cancelling after current file...";
                };

                progressForm.Controls.Add(statusLabel);
                progressForm.Controls.Add(cancelButton);
                progressForm.Controls.Add(progressBar);

                GetThemeColors(darkModeToolStripMenuItem.Checked, out System.Drawing.Color background, out System.Drawing.Color surface, out System.Drawing.Color foreground);
                ApplyThemeToControlTree(progressForm, background, surface, foreground, darkModeToolStripMenuItem.Checked);
                ApplyTitleBarThemeToForm(progressForm, darkModeToolStripMenuItem.Checked);

                progressForm.Show(this);
                progressForm.Refresh();

                for (int i = 0; i < gzipFiles.Length; i++)
                {
                    if (cancelRequested) break;

                    string gzipFile = gzipFiles[i];
                    statusLabel.Text = $"Decompressing {i + 1}/{gzipFiles.Length}: {Path.GetFileName(gzipFile)}";
                    progressBar.Value = i + 1;
                    progressForm.Refresh();
                    Application.DoEvents();

                    if (cancelRequested) break;

                    try
                    {
                        if (!gzipFile.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
                        {
                            failedCount++;
                            continue;
                        }

                        string outputFile = gzipFile.Substring(0, gzipFile.Length - 3);
                        if (File.Exists(outputFile))
                        {
                            skippedCount++;
                            continue;
                        }

                        using (FileStream inputStream = new FileStream(gzipFile, FileMode.Open, FileAccess.Read))
                        using (GZipStream gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                        using (FileStream outputStream = new FileStream(outputFile, FileMode.CreateNew, FileAccess.Write))
                        {
                            gzipStream.CopyTo(outputStream);
                        }

                        decompressedCount++;
                    }
                    catch
                    {
                        failedCount++;
                    }
                }

                progressForm.Close();
            }

            return cancelRequested;
        }
        private string GetStateFilePath()
        {
            return Path.Combine(Application.UserAppDataPath, LastOpenedFileStateName);
        }

        private string GetRecentDirectoriesStateFilePath()
        {
            return Path.Combine(Application.UserAppDataPath, RecentDirectoriesStateName);
        }

        private string GetDarkModeStateFilePath()
        {
            return Path.Combine(Application.UserAppDataPath, DarkModeStateName);
        }

        private string GetPreview050505StateFilePath()
        {
            return Path.Combine(Application.UserAppDataPath, Preview050505SettingName);
        }

        private string GetBackupReminderStateFilePath()
        {
            return Path.Combine(Application.UserAppDataPath, BackupReminderStateName);
        }

        private List<string> LoadRecentDirectories()
        {
            try
            {
                string stateFilePath = GetRecentDirectoriesStateFilePath();
                if (!File.Exists(stateFilePath)) return new List<string>();

                return File.ReadAllLines(stateFilePath)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Where(Directory.Exists)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(MaxRecentDirectories)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        private void SaveRecentDirectories(List<string> directories)
        {
            try
            {
                Directory.CreateDirectory(Application.UserAppDataPath);
                File.WriteAllLines(GetRecentDirectoriesStateFilePath(), directories.Take(MaxRecentDirectories));
            }
            catch
            {
                // Non-fatal: app should still work if state can't be persisted.
            }
        }

        private void AddRecentDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) return;

            recentDirectories.RemoveAll(x => string.Equals(x, directory, StringComparison.OrdinalIgnoreCase));
            recentDirectories.Insert(0, directory);
            if (recentDirectories.Count > MaxRecentDirectories)
                recentDirectories = recentDirectories.Take(MaxRecentDirectories).ToList();

            SaveRecentDirectories(recentDirectories);
            RefreshRecentDirectoriesMenu();
        }

        private void RefreshRecentDirectoriesMenu()
        {
            if (browseToolStripMenuItem == null || browseOpenToolStripMenuItem == null ||
                browseRecentSeparatorToolStripMenuItem == null || browseRecentNoneToolStripMenuItem == null)
                return;

            List<ToolStripItem> dynamicRecentItems = browseToolStripMenuItem.DropDownItems
                .Cast<ToolStripItem>()
                .Where(item => item.Tag is string)
                .ToList();

            foreach (ToolStripItem item in dynamicRecentItems)
                browseToolStripMenuItem.DropDownItems.Remove(item);

            recentDirectories = recentDirectories
                .Where(Directory.Exists)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxRecentDirectories)
                .ToList();

            if (recentDirectories.Count == 0)
            {
                browseRecentSeparatorToolStripMenuItem.Visible = true;
                browseRecentNoneToolStripMenuItem.Visible = true;
                browseRecentNoneToolStripMenuItem.Enabled = false;
                return;
            }

            browseRecentSeparatorToolStripMenuItem.Visible = true;
            browseRecentNoneToolStripMenuItem.Visible = false;

            int insertIndex = browseToolStripMenuItem.DropDownItems.IndexOf(browseRecentNoneToolStripMenuItem) + 1;
            for (int i = 0; i < recentDirectories.Count; i++)
            {
                string directory = recentDirectories[i];
                ToolStripMenuItem recentItem = new ToolStripMenuItem();
                recentItem.Text = $"{i + 1}. {directory}";
                recentItem.Tag = directory;
                recentItem.Click += recentDirectoryToolStripMenuItem_Click;
                browseToolStripMenuItem.DropDownItems.Insert(insertIndex + i, recentItem);
            }

            GetThemeColors(darkModeToolStripMenuItem.Checked, out System.Drawing.Color background, out System.Drawing.Color surface, out System.Drawing.Color foreground);
            ApplyThemeToMenu(menuStrip1, surface, foreground);
        }

        private bool HasShownBackupReminder()
        {
            return File.Exists(GetBackupReminderStateFilePath());
        }

        private void MarkBackupReminderShown()
        {
            try
            {
                Directory.CreateDirectory(Application.UserAppDataPath);
                File.WriteAllText(GetBackupReminderStateFilePath(), "1");
            }
            catch
            {
                // Non-fatal: app should still work if state can't be persisted.
            }
        }

        private string GetCurrentMapFolder()
        {
            if (st != null && st.Length > 0)
                return Path.GetDirectoryName(st[0]);

            string lastOpenedFile = LoadLastOpenedFile();
            if (!string.IsNullOrWhiteSpace(lastOpenedFile) && File.Exists(lastOpenedFile))
                return Path.GetDirectoryName(lastOpenedFile);

            return null;
        }

        private void MaybeShowFirstRunBackupWarning(string folderPath)
        {
            if (backupReminderHandledSession) return;
            if (HasShownBackupReminder())
            {
                backupReminderHandledSession = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath)) return;

            if (HasExistingMapBackup(folderPath))
            {
                backupReminderHandledSession = true;
                MarkBackupReminderShown();
                return;
            }

            string[] mapFiles = Directory.GetFiles(folderPath, "map*.bin", SearchOption.TopDirectoryOnly);
            if (mapFiles.Length == 0) return;

            backupReminderHandledSession = true;

            DialogResult decision = ShowAppMessageWithActions(
                "First-time warning: It is strongly recommended to create a backup before editing tiles.\n\nDo you want to create a backup now?",
                "Backup recommended",
                "Create Backup",
                "Later",
                MessageBoxIcon.Warning);

            if (decision == DialogResult.OK)
            {
                TryCreateMapBackup(folderPath, out int backedUpCount, out int skippedCount, out int failedCount, out string outputFolder);
                ShowAppMessage(
                    $"Backup finished.\n\nBacked up: {backedUpCount}\nSkipped existing: {skippedCount}\nFailed: {failedCount}\nOutput folder: {outputFolder}",
                    "Map backup",
                    failedCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }

            MarkBackupReminderShown();
        }

        private static bool HasExistingMapBackup(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                return false;

            string backupRoot = Path.Combine(folderPath, "map_backup");
            if (!Directory.Exists(backupRoot))
                return false;

            return Directory.EnumerateFiles(backupRoot, "map*.bin", SearchOption.AllDirectories).Any();
        }

        private static bool TryCreateMapBackup(string folderPath, out int backedUpCount, out int skippedCount, out int failedCount, out string outputFolder)
        {
            backedUpCount = 0;
            skippedCount = 0;
            failedCount = 0;

            outputFolder = string.Empty;
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath)) return false;

            string[] mapFiles = Directory.GetFiles(folderPath, "map*.bin", SearchOption.TopDirectoryOnly);
            if (mapFiles.Length == 0) return false;

            string backupRoot = Path.Combine(folderPath, "map_backup");
            Directory.CreateDirectory(backupRoot);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            outputFolder = Path.Combine(backupRoot, timestamp);
            Directory.CreateDirectory(outputFolder);

            foreach (string sourceFile in mapFiles)
            {
                try
                {
                    string destinationFile = Path.Combine(outputFolder, Path.GetFileName(sourceFile));
                    if (File.Exists(destinationFile))
                    {
                        skippedCount++;
                        continue;
                    }

                    File.Copy(sourceFile, destinationFile, false);
                    backedUpCount++;
                }
                catch
                {
                    failedCount++;
                }
            }

            return true;
        }

        private void SaveLastOpenedFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;

            try
            {
                Directory.CreateDirectory(Application.UserAppDataPath);
                File.WriteAllText(GetStateFilePath(), filePath);
            }
            catch
            {
                // Non-fatal: app should still work if state can't be persisted.
            }
        }

        private string LoadLastOpenedFile()
        {
            try
            {
                string stateFilePath = GetStateFilePath();
                if (!File.Exists(stateFilePath)) return null;

                string filePath = File.ReadAllText(stateFilePath).Trim();
                if (string.IsNullOrWhiteSpace(filePath)) return null;

                return filePath;
            }
            catch
            {
                return null;
            }
        }

        private void RestoreLastOpenedFile()
        {
            string lastOpenedFile = LoadLastOpenedFile();
            if (string.IsNullOrWhiteSpace(lastOpenedFile) || !File.Exists(lastOpenedFile)) return;

            LoadMapFilesFromFolder(Path.GetDirectoryName(lastOpenedFile), lastOpenedFile);
            UpdateWindowTitle(lastOpenedFile);
        }

        private void SaveDarkModeState(bool enabled)
        {
            try
            {
                Directory.CreateDirectory(Application.UserAppDataPath);
                File.WriteAllText(GetDarkModeStateFilePath(), enabled ? "1" : "0");
            }
            catch
            {
                // Non-fatal: app should still work if state can't be persisted.
            }
        }

        private bool LoadDarkModeState()
        {
            try
            {
                string stateFilePath = GetDarkModeStateFilePath();
                if (!File.Exists(stateFilePath)) return false;

                string value = File.ReadAllText(stateFilePath).Trim();
                return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void SavePreview050505State(bool enabled)
        {
            try
            {
                Directory.CreateDirectory(Application.UserAppDataPath);
                File.WriteAllText(GetPreview050505StateFilePath(), enabled ? "1" : "0");
            }
            catch
            {
                // Non-fatal: app should still work if state can't be persisted.
            }
        }

        private bool LoadPreview050505State()
        {
            try
            {
                string stateFilePath = GetPreview050505StateFilePath();
                if (!File.Exists(stateFilePath)) return false;

                string value = File.ReadAllText(stateFilePath).Trim();
                return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
        private void ApplyTheme(bool darkMode)
        {
            GetThemeColors(darkMode, out System.Drawing.Color background, out System.Drawing.Color surface, out System.Drawing.Color foreground);

            ApplyThemeToControlTree(this, background, surface, foreground, darkMode);
            ApplyThemeToMenu(menuStrip1, surface, foreground);
            ApplyThemeToStatusStrip(statusStrip1, surface, foreground);
            ApplyTitleBarTheme(darkMode);
            UpdateAboutOverlayTheme(darkMode, surface, foreground);
            UpdatePreviewTransparencyBackground();
            Invalidate(true);
        }

        private void EnsureAboutOverlayCreated()
        {
            if (aboutOverlayBackdrop != null)
                return;

            aboutOverlayBackdrop = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false
            };
            aboutOverlayBackdrop.Click += (s, e) => HideAboutOverlay();

            aboutOverlayCard = new Panel
            {
                Size = new Size(560, 560),
                BorderStyle = BorderStyle.FixedSingle
            };
            aboutOverlayCard.Click += (s, e) => { };
            aboutOverlayBackdrop.Controls.Add(aboutOverlayCard);

            aboutOverlayContentHost = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 34, 0, 0)
            };
            aboutOverlayCard.Controls.Add(aboutOverlayContentHost);

            aboutOverlayCloseButton = new Button
            {
                Text = "X",
                Size = new Size(30, 26),
                FlatStyle = FlatStyle.Flat,
                TabStop = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            aboutOverlayCloseButton.FlatAppearance.BorderSize = 1;
            aboutOverlayCloseButton.Click += (s, e) => HideAboutOverlay();
            aboutOverlayCard.Controls.Add(aboutOverlayCloseButton);
            aboutOverlayCard.Resize += (s, e) =>
            {
                aboutOverlayCloseButton.Location = new Point(aboutOverlayCard.ClientSize.Width - aboutOverlayCloseButton.Width - 6, 5);
            };

            Controls.Add(aboutOverlayBackdrop);
            aboutOverlayBackdrop.BringToFront();
            CenterAboutOverlayCard();
        }

        private void CenterAboutOverlayCard()
        {
            if (aboutOverlayBackdrop == null || aboutOverlayCard == null)
                return;

            int x = Math.Max(8, (aboutOverlayBackdrop.ClientSize.Width - aboutOverlayCard.Width) / 2);
            int y = Math.Max(8, (aboutOverlayBackdrop.ClientSize.Height - aboutOverlayCard.Height) / 2);
            aboutOverlayCard.Location = new Point(x, y);
        }

        private void ShowAboutOverlay()
        {
            EnsureAboutOverlayCreated();

            if (aboutOverlayContent != null)
            {
                aboutOverlayContentHost.Controls.Remove(aboutOverlayContent);
                aboutOverlayContent.Dispose();
                aboutOverlayContent = null;
            }

            aboutOverlayContent = new AboutForm(darkModeToolStripMenuItem.Checked)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };

            aboutOverlayContentHost.Controls.Add(aboutOverlayContent);
            aboutOverlayContent.Show();

            UpdateAboutOverlayTheme(darkModeToolStripMenuItem.Checked,
                darkModeToolStripMenuItem.Checked ? System.Drawing.Color.FromArgb(45, 45, 48) : SystemColors.Window,
                darkModeToolStripMenuItem.Checked ? System.Drawing.Color.Gainsboro : SystemColors.ControlText);

            aboutOverlayBackdrop.Visible = true;
            aboutOverlayBackdrop.BringToFront();
            aboutOverlayCard.BringToFront();
            CenterAboutOverlayCard();
        }

        private void HideAboutOverlay()
        {
            if (aboutOverlayBackdrop == null)
                return;

            aboutOverlayBackdrop.Visible = false;
            if (aboutOverlayContent != null)
            {
                aboutOverlayContentHost.Controls.Remove(aboutOverlayContent);
                aboutOverlayContent.Dispose();
                aboutOverlayContent = null;
            }
        }

        private void UpdateAboutOverlayTheme(bool darkMode, System.Drawing.Color surface, System.Drawing.Color foreground)
        {
            if (aboutOverlayBackdrop == null)
                return;

            aboutOverlayBackdrop.BackColor = darkMode
                ? System.Drawing.Color.FromArgb(24, 24, 28)
                : System.Drawing.Color.FromArgb(230, 230, 230);

            aboutOverlayCard.BackColor = darkMode
                ? System.Drawing.Color.FromArgb(30, 30, 30)
                : SystemColors.Control;

            if (aboutOverlayCloseButton != null)
            {
                aboutOverlayCloseButton.BackColor = surface;
                aboutOverlayCloseButton.ForeColor = foreground;
                if (darkMode)
                {
                    aboutOverlayCloseButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 88);
                    aboutOverlayCloseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(72, 72, 78);
                    aboutOverlayCloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(90, 90, 98);
                }
            }
        }

        private static Bitmap CreateCheckerboardTile(System.Drawing.Color first, System.Drawing.Color second)
        {
            const int cellSize = 8;
            Bitmap tile = new Bitmap(cellSize * 2, cellSize * 2, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(tile))
            using (SolidBrush firstBrush = new SolidBrush(first))
            using (SolidBrush secondBrush = new SolidBrush(second))
            {
                g.FillRectangle(firstBrush, 0, 0, tile.Width, tile.Height);
                g.FillRectangle(secondBrush, 0, 0, cellSize, cellSize);
                g.FillRectangle(secondBrush, cellSize, cellSize, cellSize, cellSize);
            }

            return tile;
        }

        private void UpdatePreviewTransparencyBackground()
        {
            if (!previewTreat050505AsTransparent)
            {
                pictureBox1.BackgroundImage = null;
                pictureBox2.BackgroundImage = null;
                previewCheckerBackgroundBitmap?.Dispose();
                previewCheckerBackgroundBitmap = null;
                return;
            }

            bool darkMode = darkModeToolStripMenuItem.Checked;
            System.Drawing.Color a = darkMode ? System.Drawing.Color.FromArgb(62, 62, 66) : System.Drawing.Color.FromArgb(236, 236, 236);
            System.Drawing.Color b = darkMode ? System.Drawing.Color.FromArgb(78, 78, 84) : System.Drawing.Color.FromArgb(216, 216, 216);

            Bitmap oldTile = previewCheckerBackgroundBitmap;
            previewCheckerBackgroundBitmap = CreateCheckerboardTile(a, b);

            pictureBox1.BackgroundImage = previewCheckerBackgroundBitmap;
            pictureBox1.BackgroundImageLayout = ImageLayout.Tile;
            pictureBox2.BackgroundImage = previewCheckerBackgroundBitmap;
            pictureBox2.BackgroundImageLayout = ImageLayout.Tile;

            oldTile?.Dispose();
        }

        private void ApplyTitleBarTheme(bool darkMode)
        {
            if (!IsHandleCreated) return;

            ApplyWindowChrome(Handle, darkMode);
        }

        public static void ApplyTitleBarThemeToForm(Form form, bool darkMode)
        {
            if (form == null) return;

            form.Icon = GetApplicationIcon();

            if (form.IsHandleCreated)
            {
                ApplyWindowChrome(form.Handle, darkMode);
                return;
            }

            EventHandler handleCreated = null;
            handleCreated = (sender, args) =>
            {
                form.HandleCreated -= handleCreated;
                ApplyWindowChrome(form.Handle, darkMode);
            };

            form.HandleCreated += handleCreated;
        }

        public static Icon GetApplicationIcon()
        {
            if (cachedApplicationIcon != null)
                return cachedApplicationIcon;

            try
            {
                Icon executableIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (executableIcon != null)
                    cachedApplicationIcon = (Icon)executableIcon.Clone();
            }
            catch
            {
            }

            if (cachedApplicationIcon == null)
                cachedApplicationIcon = SystemIcons.Application;

            return cachedApplicationIcon;
        }

        private static void ApplyWindowChrome(IntPtr windowHandle, bool darkMode)
        {
            if (windowHandle == IntPtr.Zero) return;

            int useDarkMode = darkMode ? 1 : 0;
            int result = DwmSetWindowAttribute(windowHandle, DwmwaUseImmersiveDarkMode, ref useDarkMode, sizeof(int));
            if (result != 0)
                DwmSetWindowAttribute(windowHandle, DwmwaUseImmersiveDarkModeBefore20H1, ref useDarkMode, sizeof(int));

            Icon appIcon = GetApplicationIcon();
            if (appIcon == null) return;

            SendMessage(windowHandle, WmSetIcon, IconSmall, appIcon.Handle);
            SendMessage(windowHandle, WmSetIcon, IconBig, appIcon.Handle);
        }

        private DialogResult ShowThemedCommonDialog(CommonDialog dialog, IWin32Window owner = null)
        {
            bool darkMode = darkModeToolStripMenuItem.Checked;
            using (new NativeDialogThemeScope(darkMode))
            {
                return owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
            }
        }

        private sealed class NativeDialogThemeScope : IDisposable
        {
            private readonly bool darkMode;
            private readonly HookProc hookCallback;
            private readonly IntPtr hookHandle;
            private bool themedDialog;

            public NativeDialogThemeScope(bool darkMode)
            {
                this.darkMode = darkMode;
                hookCallback = DialogHook;
                hookHandle = SetWindowsHookEx(WhCbt, hookCallback, IntPtr.Zero, GetCurrentThreadId());
            }

            private IntPtr DialogHook(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode == HcbtActivate && !themedDialog)
                {
                    themedDialog = true;
                    ApplyWindowChrome(wParam, darkMode);
                }

                return CallNextHookEx(hookHandle, nCode, wParam, lParam);
            }

            public void Dispose()
            {
                if (hookHandle != IntPtr.Zero)
                    UnhookWindowsHookEx(hookHandle);
            }
        }

        private void GetThemeColors(bool darkMode, out System.Drawing.Color background, out System.Drawing.Color surface, out System.Drawing.Color foreground)
        {
            background = darkMode ? System.Drawing.Color.FromArgb(30, 30, 30) : SystemColors.Control;
            surface = darkMode ? System.Drawing.Color.FromArgb(45, 45, 48) : SystemColors.Window;
            foreground = darkMode ? System.Drawing.Color.Gainsboro : SystemColors.ControlText;
        }

        private DialogResult ShowAppMessage(string message, string title, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            bool darkMode = darkModeToolStripMenuItem.Checked;
            using (Form dialog = new Form())
            using (Label messageLabel = new Label())
            using (Button okButton = new Button())
            {
                dialog.Text = title;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(560, 190);
                dialog.AcceptButton = okButton;

                string iconText = string.Empty;
                if (icon == MessageBoxIcon.Warning) iconText = "Warning\n\n";
                else if (icon == MessageBoxIcon.Information) iconText = "Information\n\n";
                else if (icon == MessageBoxIcon.Error) iconText = "Error\n\n";

                messageLabel.AutoSize = false;
                messageLabel.Dock = DockStyle.Fill;
                messageLabel.Padding = new Padding(14, 12, 14, 8);
                messageLabel.TextAlign = ContentAlignment.TopLeft;
                messageLabel.Text = iconText + message;

                okButton.Text = "OK";
                okButton.Size = new Size(90, 28);
                okButton.Location = new Point(dialog.ClientSize.Width - okButton.Width - 12, dialog.ClientSize.Height - okButton.Height - 12);
                okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                okButton.DialogResult = DialogResult.OK;

                dialog.Controls.Add(messageLabel);
                dialog.Controls.Add(okButton);

                GetThemeColors(darkMode, out System.Drawing.Color background, out System.Drawing.Color surface, out System.Drawing.Color foreground);
                ApplyThemeToControlTree(dialog, background, surface, foreground, darkMode);
                ApplyTitleBarThemeToForm(dialog, darkMode);

                return dialog.ShowDialog(this);
            }
        }

        private DialogResult ShowAppMessageWithActions(string message, string title, string primaryActionText, string secondaryActionText, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            bool darkMode = darkModeToolStripMenuItem.Checked;
            using (Form dialog = new Form())
            using (Label messageLabel = new Label())
            using (Panel buttonPanel = new Panel())
            using (Button primaryButton = new Button())
            using (Button secondaryButton = new Button())
            {
                dialog.Text = title;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(600, 230);

                string iconText = string.Empty;
                if (icon == MessageBoxIcon.Warning) iconText = "Warning\n\n";
                else if (icon == MessageBoxIcon.Information) iconText = "Information\n\n";
                else if (icon == MessageBoxIcon.Error) iconText = "Error\n\n";

                messageLabel.AutoSize = false;
                messageLabel.Dock = DockStyle.Fill;
                messageLabel.Padding = new Padding(14, 12, 14, 8);
                messageLabel.TextAlign = ContentAlignment.TopLeft;
                messageLabel.Text = iconText + message;

                buttonPanel.Dock = DockStyle.Bottom;
                buttonPanel.Height = 54;
                buttonPanel.Padding = new Padding(0, 12, 12, 12);

                secondaryButton.Text = secondaryActionText;
                secondaryButton.Size = new Size(100, 28);
                secondaryButton.Dock = DockStyle.Right;
                secondaryButton.DialogResult = DialogResult.Cancel;

                primaryButton.Text = primaryActionText;
                primaryButton.Size = new Size(110, 28);
                primaryButton.Dock = DockStyle.Right;
                primaryButton.DialogResult = DialogResult.OK;

                dialog.AcceptButton = primaryButton;
                dialog.CancelButton = secondaryButton;

                dialog.Controls.Add(messageLabel);
                dialog.Controls.Add(buttonPanel);
                buttonPanel.Controls.Add(secondaryButton);
                buttonPanel.Controls.Add(primaryButton);

                GetThemeColors(darkMode, out System.Drawing.Color background, out System.Drawing.Color surface, out System.Drawing.Color foreground);
                ApplyThemeToControlTree(dialog, background, surface, foreground, darkMode);
                ApplyTitleBarThemeToForm(dialog, darkMode);

                return dialog.ShowDialog(this);
            }
        }

        private bool ShowMassExportCautionDialog()
        {
            bool darkMode = darkModeToolStripMenuItem.Checked;
            using (Form dialog = new Form())
            using (Label messageLabel = new Label())
            using (Panel buttonPanel = new Panel())
            using (Button proceedButton = new Button())
            using (Button cancelButton = new Button())
            {
                dialog.Text = "Mass export caution";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(620, 220);

                messageLabel.AutoSize = false;
                messageLabel.Dock = DockStyle.Fill;
                messageLabel.Padding = new Padding(14, 12, 14, 8);
                messageLabel.TextAlign = ContentAlignment.TopLeft;
                messageLabel.Text = "Warning\n\nThis method is not necessarily recommended for large number of files or if you are not familiar with the tiles.\nProceed with caution.";

                buttonPanel.Dock = DockStyle.Bottom;
                buttonPanel.Height = 50;
                buttonPanel.Padding = new Padding(0, 11, 12, 11);

                proceedButton.Text = "Proceed";
                proceedButton.Size = new Size(100, 28);
                proceedButton.Dock = DockStyle.Right;
                proceedButton.DialogResult = DialogResult.OK;

                cancelButton.Text = "Cancel";
                cancelButton.Size = new Size(100, 28);
                cancelButton.Dock = DockStyle.Right;
                cancelButton.DialogResult = DialogResult.Cancel;

                dialog.AcceptButton = proceedButton;
                dialog.CancelButton = cancelButton;
                dialog.Controls.Add(messageLabel);
                dialog.Controls.Add(buttonPanel);
                buttonPanel.Controls.Add(cancelButton);
                buttonPanel.Controls.Add(proceedButton);

                GetThemeColors(darkMode, out System.Drawing.Color background, out System.Drawing.Color surface, out System.Drawing.Color foreground);
                ApplyThemeToControlTree(dialog, background, surface, foreground, darkMode);
                ApplyTitleBarThemeToForm(dialog, darkMode);

                return dialog.ShowDialog(this) == DialogResult.OK;
            }
        }

        private string ShowMassExportFormatDialog()
        {
            bool darkMode = darkModeToolStripMenuItem.Checked;
            using (Form dialog = new Form())
            using (Label messageLabel = new Label())
            using (Button pngButton = new Button())
            using (Button bmpButton = new Button())
            using (Button cancelButton = new Button())
            {
                dialog.Text = "Choose export format";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(520, 190);

                messageLabel.AutoSize = false;
                messageLabel.Dock = DockStyle.Top;
                messageLabel.Height = 90;
                messageLabel.Padding = new Padding(14, 12, 14, 8);
                messageLabel.TextAlign = ContentAlignment.TopLeft;
                messageLabel.Text = "Select export format for mass export:";

                string selectedFormat = null;

                pngButton.Text = "Export .PNG";
                pngButton.Size = new Size(110, 30);
                pngButton.Location = new Point(14, 112);
                pngButton.Click += (s, e) => { selectedFormat = "png"; dialog.DialogResult = DialogResult.OK; dialog.Close(); };

                bmpButton.Text = "Export .BMP";
                bmpButton.Size = new Size(110, 30);
                bmpButton.Location = new Point(130, 112);
                bmpButton.Click += (s, e) => { selectedFormat = "bmp"; dialog.DialogResult = DialogResult.OK; dialog.Close(); };

                cancelButton.Text = "Cancel";
                cancelButton.Size = new Size(90, 30);
                cancelButton.Location = new Point(dialog.ClientSize.Width - 102, 112);
                cancelButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                cancelButton.DialogResult = DialogResult.Cancel;

                dialog.CancelButton = cancelButton;
                dialog.Controls.Add(messageLabel);
                dialog.Controls.Add(pngButton);
                dialog.Controls.Add(bmpButton);
                dialog.Controls.Add(cancelButton);

                GetThemeColors(darkMode, out System.Drawing.Color background, out System.Drawing.Color surface, out System.Drawing.Color foreground);
                ApplyThemeToControlTree(dialog, background, surface, foreground, darkMode);
                ApplyTitleBarThemeToForm(dialog, darkMode);

                DialogResult result = dialog.ShowDialog(this);
                if (result != DialogResult.OK) return null;
                return selectedFormat;
            }
        }

        private void ApplyThemeToControlTree(Control control, System.Drawing.Color background, System.Drawing.Color surface, System.Drawing.Color foreground, bool darkMode)
        {
            if (control is MenuStrip)
            {
                // Menu strip colors are handled by ApplyThemeToMenu.
            }
            else if (control is ListBox listBox)
            {
                listBox.BackColor = surface;
                listBox.ForeColor = foreground;
                listBox.BorderStyle = darkMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.BackColor = surface;
                comboBox.ForeColor = foreground;
                comboBox.FlatStyle = darkMode ? FlatStyle.Flat : FlatStyle.Standard;
            }
            else if (control is DataGridView dataGridView)
            {
                dataGridView.BackgroundColor = surface;
                dataGridView.GridColor = darkMode ? System.Drawing.Color.FromArgb(70, 70, 74) : System.Drawing.Color.FromArgb(210, 210, 210);
                dataGridView.BorderStyle = darkMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                dataGridView.EnableHeadersVisualStyles = false;

                dataGridView.ColumnHeadersDefaultCellStyle.BackColor = darkMode ? System.Drawing.Color.FromArgb(55, 55, 60) : SystemColors.Control;
                dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = foreground;
                dataGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = darkMode ? System.Drawing.Color.FromArgb(55, 55, 60) : SystemColors.Control;
                dataGridView.ColumnHeadersDefaultCellStyle.SelectionForeColor = foreground;

                dataGridView.DefaultCellStyle.BackColor = surface;
                dataGridView.DefaultCellStyle.ForeColor = foreground;
                dataGridView.DefaultCellStyle.SelectionBackColor = darkMode ? System.Drawing.Color.FromArgb(63, 63, 70) : SystemColors.Highlight;
                dataGridView.DefaultCellStyle.SelectionForeColor = darkMode ? foreground : SystemColors.HighlightText;

                dataGridView.RowsDefaultCellStyle.BackColor = surface;
                dataGridView.RowsDefaultCellStyle.ForeColor = foreground;
                dataGridView.RowsDefaultCellStyle.SelectionBackColor = darkMode ? System.Drawing.Color.FromArgb(63, 63, 70) : SystemColors.Highlight;
                dataGridView.RowsDefaultCellStyle.SelectionForeColor = darkMode ? foreground : SystemColors.HighlightText;

                dataGridView.Invalidate();
            }
            else if (control is PictureBox pictureBox)
            {
                pictureBox.BackColor = surface;
                pictureBox.ForeColor = foreground;
                pictureBox.BorderStyle = darkMode ? BorderStyle.FixedSingle : BorderStyle.None;
            }
            else if (control is Button button)
            {
                button.BackColor = surface;
                button.ForeColor = foreground;
                button.FlatStyle = darkMode ? FlatStyle.Flat : FlatStyle.Standard;
                if (darkMode)
                {
                    button.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(70, 70, 74);
                    button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(63, 63, 70);
                    button.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(80, 80, 88);
                }
            }
            else if (control is GroupBox groupBox)
            {
                groupBox.BackColor = background;
                groupBox.ForeColor = foreground;
            }
            else if (control is Panel panel)
            {
                panel.BackColor = background;
                panel.ForeColor = foreground;
                panel.BorderStyle = darkMode ? BorderStyle.FixedSingle : BorderStyle.None;
            }
            else
            {
                control.BackColor = background;
                control.ForeColor = foreground;
            }

            foreach (Control child in control.Controls)
                ApplyThemeToControlTree(child, background, surface, foreground, darkMode);
        }

        private void ApplyThemeToMenu(MenuStrip menu, System.Drawing.Color surface, System.Drawing.Color foreground)
        {
            menu.BackColor = surface;
            menu.ForeColor = foreground;
            menu.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable(darkModeToolStripMenuItem.Checked));
            foreach (ToolStripItem item in menu.Items)
                ApplyThemeToMenuItem(item, surface, foreground);

            ApplyThemeToContextMenu(fileListContextMenuStrip, surface, foreground);
            ApplyThemeToContextMenu(previewSection1ContextMenuStrip, surface, foreground);
            ApplyThemeToContextMenu(previewSection2ContextMenuStrip, surface, foreground);
        }

        private void ApplyThemeToContextMenu(ContextMenuStrip contextMenu, System.Drawing.Color surface, System.Drawing.Color foreground)
        {
            if (contextMenu == null) return;

            contextMenu.BackColor = surface;
            contextMenu.ForeColor = foreground;
            contextMenu.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable(darkModeToolStripMenuItem.Checked));

            foreach (ToolStripItem item in contextMenu.Items)
                ApplyThemeToMenuItem(item, surface, foreground);
        }

        private void ApplyThemeToStatusStrip(StatusStrip statusStrip, System.Drawing.Color surface, System.Drawing.Color foreground)
        {
            if (statusStrip == null) return;

            statusStrip.BackColor = surface;
            statusStrip.ForeColor = foreground;
            statusStrip.Renderer = new BorderlessToolStripRenderer(new ThemeColorTable(darkModeToolStripMenuItem.Checked));

            foreach (ToolStripItem item in statusStrip.Items)
            {
                item.BackColor = surface;
                item.ForeColor = foreground;
            }
        }

        private void ApplyThemeToMenuItem(ToolStripItem item, System.Drawing.Color surface, System.Drawing.Color foreground)
        {
            item.BackColor = surface;
            item.ForeColor = foreground;

            if (item is ToolStripDropDownItem dropDown)
                foreach (ToolStripItem child in dropDown.DropDownItems)
                    ApplyThemeToMenuItem(child, surface, foreground);
        }

        private void darkModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyTheme(darkModeToolStripMenuItem.Checked);
            SaveDarkModeState(darkModeToolStripMenuItem.Checked);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAboutOverlay();
        }

        private sealed class ThemeColorTable : ProfessionalColorTable
        {
            private readonly bool darkMode;

            public ThemeColorTable(bool darkMode)
            {
                this.darkMode = darkMode;
                UseSystemColors = !darkMode;
            }

            public override System.Drawing.Color MenuItemSelected => darkMode ? System.Drawing.Color.FromArgb(63, 63, 70) : base.MenuItemSelected;
            public override System.Drawing.Color MenuItemBorder => darkMode ? System.Drawing.Color.FromArgb(80, 80, 88) : base.MenuItemBorder;
            public override System.Drawing.Color MenuItemSelectedGradientBegin => darkMode ? System.Drawing.Color.FromArgb(63, 63, 70) : base.MenuItemSelectedGradientBegin;
            public override System.Drawing.Color MenuItemSelectedGradientEnd => darkMode ? System.Drawing.Color.FromArgb(63, 63, 70) : base.MenuItemSelectedGradientEnd;
            public override System.Drawing.Color MenuItemPressedGradientBegin => darkMode ? System.Drawing.Color.FromArgb(80, 80, 88) : base.MenuItemPressedGradientBegin;
            public override System.Drawing.Color MenuItemPressedGradientMiddle => darkMode ? System.Drawing.Color.FromArgb(80, 80, 88) : base.MenuItemPressedGradientMiddle;
            public override System.Drawing.Color MenuItemPressedGradientEnd => darkMode ? System.Drawing.Color.FromArgb(80, 80, 88) : base.MenuItemPressedGradientEnd;
            public override System.Drawing.Color ToolStripDropDownBackground => darkMode ? System.Drawing.Color.FromArgb(45, 45, 48) : base.ToolStripDropDownBackground;
            public override System.Drawing.Color ImageMarginGradientBegin => darkMode ? System.Drawing.Color.FromArgb(45, 45, 48) : base.ImageMarginGradientBegin;
            public override System.Drawing.Color ImageMarginGradientMiddle => darkMode ? System.Drawing.Color.FromArgb(45, 45, 48) : base.ImageMarginGradientMiddle;
            public override System.Drawing.Color ImageMarginGradientEnd => darkMode ? System.Drawing.Color.FromArgb(45, 45, 48) : base.ImageMarginGradientEnd;
        }

        private sealed class BorderlessToolStripRenderer : ToolStripProfessionalRenderer
        {
            public BorderlessToolStripRenderer(ProfessionalColorTable colorTable) : base(colorTable) { }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                // Don't draw the border
            }
        }

        private void UpdateWindowTitle(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Text = DefaultWindowTitle;
                return;
            }

            string relativePath = filePath;
            string currentDirectory = Directory.GetCurrentDirectory();
            if (!string.IsNullOrWhiteSpace(currentDirectory) && filePath.StartsWith(currentDirectory, StringComparison.OrdinalIgnoreCase))
                relativePath = filePath.Substring(currentDirectory.Length).TrimStart(Path.DirectorySeparatorChar);

            Text = $"{DefaultWindowTitle} - {relativePath}";
        }

        private const bool SectionImageRowsAreBottomUp = false;

        private static Bitmap BuildIndexedBitmap(byte[] imageBuffer, byte[] paletteBuffer, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            ColorPalette palette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = System.Drawing.Color.FromArgb(
                    255 - paletteBuffer[i * 4 + 3],
                    paletteBuffer[i * 4 + 2],
                    paletteBuffer[i * 4 + 1],
                    paletteBuffer[i * 4 + 0]);
            }
            bitmap.Palette = palette;

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            try
            {
                IntPtr topRowPointer = GetTopRowPointer(bitmapData, height);
                int rowStep = -bitmapData.Stride;
                for (int y = 0; y < height; y++)
                {
                    int sourceY = SectionImageRowsAreBottomUp ? (height - 1 - y) : y;
                    IntPtr destinationRow = IntPtr.Add(topRowPointer, y * rowStep);
                    Marshal.Copy(imageBuffer, sourceY * width, destinationRow, width);
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }

            return bitmap;
        }

        private static IntPtr GetTopRowPointer(BitmapData bitmapData, int height)
        {
            if (bitmapData.Stride < 0)
                return bitmapData.Scan0;

            return IntPtr.Add(bitmapData.Scan0, bitmapData.Stride * (height - 1));
        }

        private Bitmap BuildPreviewBitmap(Bitmap source)
        {
            if (source == null)
                return null;

            if (!previewTreat050505AsTransparent)
            {
                return (Bitmap)source.Clone();
            }

            bool[] transparentIndex = new bool[256];
            bool hasTransparentIndices = false;
            Color[] entries = source.Palette.Entries;
            for (int i = 0; i < entries.Length && i < 256; i++)
            {
                Color c = entries[i];
                if (c.R == 5 && c.G == 5 && c.B == 5)
                {
                    transparentIndex[i] = true;
                    hasTransparentIndices = true;
                }
            }

            if (!hasTransparentIndices)
            {
                return (Bitmap)source.Clone();
            }

            Bitmap preview = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            Rectangle rect = new Rectangle(0, 0, source.Width, source.Height);
            BitmapData sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            BitmapData previewData = preview.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                IntPtr sourceTopRow = GetTopRowPointer(sourceData, source.Height);
                IntPtr previewTopRow = GetTopRowPointer(previewData, source.Height);
                int sourceRowStep = -sourceData.Stride;
                int previewRowStep = -previewData.Stride;

                byte[] sourceRow = new byte[source.Width];
                byte[] previewRow = new byte[source.Width * 4];

                for (int y = 0; y < source.Height; y++)
                {
                    IntPtr sourceRowPtr = IntPtr.Add(sourceTopRow, y * sourceRowStep);
                    IntPtr previewRowPtr = IntPtr.Add(previewTopRow, y * previewRowStep);

                    Marshal.Copy(sourceRowPtr, sourceRow, 0, source.Width);
                    for (int x = 0; x < source.Width; x++)
                    {
                        int index = sourceRow[x];
                        Color color = index < entries.Length ? entries[index] : Color.Black;
                        int outOffset = x * 4;
                        previewRow[outOffset + 0] = color.B;
                        previewRow[outOffset + 1] = color.G;
                        previewRow[outOffset + 2] = color.R;
                        previewRow[outOffset + 3] = transparentIndex[index] ? (byte)0 : (byte)255;
                    }

                    Marshal.Copy(previewRow, 0, previewRowPtr, previewRow.Length);
                }
            }
            finally
            {
                source.UnlockBits(sourceData);
                preview.UnlockBits(previewData);
            }

            return preview;
        }

        private static void LoadSectionBitmaps(string filePath, out Bitmap firstSection, out Bitmap secondSection)
        {
            byte[] firstPaletteBuffer;
            byte[] firstImageBuffer;
            byte[] secondPaletteBuffer = new byte[1024];
            byte[] secondImageBuffer = new byte[0];

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                firstPaletteBuffer = br.ReadBytes(1024);
                firstImageBuffer = br.ReadBytes(512 * 512);

                if (fs.Length > 0x80400 + 1024)
                {
                    fs.Seek(-0x80400, SeekOrigin.End);
                    secondPaletteBuffer = br.ReadBytes(1024);
                    secondImageBuffer = br.ReadBytes((int)(fs.Length - fs.Position));
                }
            }

            firstSection = BuildIndexedBitmap(firstImageBuffer, firstPaletteBuffer, 512, 512);

            if (secondImageBuffer.Length > 512)
                secondSection = BuildIndexedBitmap(secondImageBuffer, secondPaletteBuffer, 512, secondImageBuffer.Length / 512);
            else
                secondSection = null;
        }

        private void RenderImage(string file)
        {
            Bitmap oldFirstPreview = pictureBox1.Image as Bitmap;
            Bitmap oldSecondPreview = pictureBox2.Image as Bitmap;
            Bitmap oldFirstSource = currentSection1SourceBitmap;
            Bitmap oldSecondSource = currentSection2SourceBitmap;

            LoadSectionBitmaps(file, out Bitmap firstSection, out Bitmap secondSection);
            UpdatePaletteInfo(file);

            currentSection1SourceBitmap = firstSection;
            currentSection2SourceBitmap = secondSection;

            Bitmap firstPreview = BuildPreviewBitmap(firstSection);

            pictureBox1.Size = firstSection.Size;
            pictureBox1.Image = firstPreview;
            panel1.AutoScrollMinSize = firstSection.Size;

            if (secondSection != null)
            {
                Bitmap secondPreview = BuildPreviewBitmap(secondSection);
                pictureBox2.Size = secondSection.Size;
                panel2.AutoScrollMinSize = secondSection.Size;
                pictureBox2.Image = secondPreview;
            }
            else
            {
                pictureBox2.Size = new Size(512, 512);
                panel2.AutoScrollMinSize = pictureBox2.Size;
                pictureBox2.Image = null;
            }

            oldFirstPreview?.Dispose();
            oldSecondPreview?.Dispose();
            oldFirstSource?.Dispose();
            oldSecondSource?.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string selectedFile = listBox1.SelectedValue as string;
            string fileBase = Path.GetFileNameWithoutExtension(selectedFile);
            ExportImage(currentSection1SourceBitmap, $"{fileBase}_Section1");
        }

        private void previewSection1ExportImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string selectedFile = listBox1.SelectedValue as string;
            string fileBase = Path.GetFileNameWithoutExtension(selectedFile);
            ExportImage(currentSection2SourceBitmap, $"{fileBase}_Section2");
        }

        private void previewSection2ExportImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button4_Click(sender, e);
        }

        private void ExportImage(Image image, string defaultBaseFileName)
        {
            if (listBox1.Items.Count == 0 || image == null) return;

            using (SaveFileDialog sfd = new SaveFileDialog()
            {
                Filter = "PNG files (*.png)|*.png|BMP files (*.bmp)|*.bmp",
                FilterIndex = 1,
                AddExtension = true,
                DefaultExt = "png",
                FileName = $"{defaultBaseFileName}.png"
            })
                if (ShowThemedCommonDialog(sfd, this) == DialogResult.OK)
                {
                    string extension = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                    ImageFormat format = extension == ".bmp" ? ImageFormat.Bmp : ImageFormat.Png;
                    image.Save(sfd.FileName, format);
                }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = "";
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Image files (*.png;*.bmp)|*.png;*.bmp|PNG files (*.png)|*.png|BMP files (*.bmp)|*.bmp", Multiselect = false })
                if (ShowThemedCommonDialog(ofd, this) == DialogResult.OK)
                    path = ofd.FileName;
                else return;

            string filePath = st.Where(x => Path.GetFileName(x) == (string)listBox1.SelectedValue).First();
            if (!TryImportSection1Image(filePath, path, out string error))
            {
                ShowAppMessage(error ?? "Import failed.", "Import warning", MessageBoxIcon.Warning);
                return;
            }

            RenderImage(filePath);
        }

        private void previewSection1ImportImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button2_Click(sender, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string path = "";
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Image files (*.png;*.bmp)|*.png;*.bmp|PNG files (*.png)|*.png|BMP files (*.bmp)|*.bmp", Multiselect = false })
                if (ShowThemedCommonDialog(ofd, this) == DialogResult.OK)
                    path = ofd.FileName;
                else return;

            string filePath = st.Where(x => Path.GetFileName(x) == (string)listBox1.SelectedValue).First();
            if (!TryImportSection2Image(filePath, path, out string error))
            {
                ShowAppMessage(error ?? "Import failed.", "Import warning", MessageBoxIcon.Warning);
                return;
            }

            RenderImage(filePath);
        }

        private void previewSection2ImportImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button3_Click(sender, e);
        }

        private static string FindExistingSectionImagePath(string folderPath, string fileBase, string sectionSuffix)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(fileBase) || string.IsNullOrWhiteSpace(sectionSuffix))
                return null;

            string png = Path.Combine(folderPath, $"{fileBase}_{sectionSuffix}.png");
            if (File.Exists(png)) return png;

            string bmp = Path.Combine(folderPath, $"{fileBase}_{sectionSuffix}.bmp");
            if (File.Exists(bmp)) return bmp;

            return null;
        }

        private static bool TryConvertToIndexed8bpp(Bitmap source, out Bitmap converted, out string error)
        {
            converted = null;
            error = null;

            int width = source.Width;
            int height = source.Height;
            Rectangle rect = new Rectangle(0, 0, width, height);

            Bitmap argbSource = null;
            try
            {
                argbSource = source.Clone(rect, PixelFormat.Format32bppArgb);
                Dictionary<int, byte> colorToIndex = new Dictionary<int, byte>();
                List<Color> paletteEntries = new List<Color>(256);
                byte[] indices = new byte[width * height];

                BitmapData sourceData = argbSource.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    IntPtr topRow = GetTopRowPointer(sourceData, height);
                    int rowStep = -sourceData.Stride;
                    byte[] row = new byte[width * 4];

                    for (int y = 0; y < height; y++)
                    {
                        IntPtr rowPtr = IntPtr.Add(topRow, y * rowStep);
                        Marshal.Copy(rowPtr, row, 0, row.Length);
                        int targetOffset = y * width;

                        for (int x = 0; x < width; x++)
                        {
                            int pixelOffset = x * 4;
                            int b = row[pixelOffset + 0];
                            int g = row[pixelOffset + 1];
                            int r = row[pixelOffset + 2];
                            int a = row[pixelOffset + 3];
                            int argb = (a << 24) | (r << 16) | (g << 8) | b;

                            if (!colorToIndex.TryGetValue(argb, out byte paletteIndex))
                            {
                                if (paletteEntries.Count >= 256)
                                {
                                    error = "Image uses more than 256 unique colors and cannot be converted to 8BPP indexed.";
                                    return false;
                                }

                                paletteIndex = (byte)paletteEntries.Count;
                                colorToIndex[argb] = paletteIndex;
                                paletteEntries.Add(Color.FromArgb(a, r, g, b));
                            }

                            indices[targetOffset + x] = paletteIndex;
                        }
                    }
                }
                finally
                {
                    argbSource.UnlockBits(sourceData);
                }

                converted = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
                ColorPalette palette = converted.Palette;
                for (int i = 0; i < 256; i++)
                {
                    palette.Entries[i] = i < paletteEntries.Count
                        ? paletteEntries[i]
                        : Color.FromArgb(255, 0, 0, 0);
                }
                converted.Palette = palette;

                BitmapData convertedData = converted.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                try
                {
                    IntPtr topRow = GetTopRowPointer(convertedData, height);
                    int rowStep = -convertedData.Stride;
                    for (int y = 0; y < height; y++)
                    {
                        IntPtr rowPtr = IntPtr.Add(topRow, y * rowStep);
                        Marshal.Copy(indices, y * width, rowPtr, width);
                    }
                }
                finally
                {
                    converted.UnlockBits(convertedData);
                }

                return true;
            }
            catch (Exception ex)
            {
                converted?.Dispose();
                converted = null;
                error = ex.Message;
                return false;
            }
            finally
            {
                argbSource?.Dispose();
            }
        }

        private static bool TryLoadImportBitmap8bpp(string imagePath, out Bitmap importBitmap, out string error)
        {
            importBitmap = null;
            error = null;

            Bitmap loaded = null;
            try
            {
                loaded = new Bitmap(imagePath);
                if (loaded.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    importBitmap = loaded.Clone(new Rectangle(0, 0, loaded.Width, loaded.Height), PixelFormat.Format8bppIndexed);
                    return true;
                }

                return TryConvertToIndexed8bpp(loaded, out importBitmap, out error);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
            finally
            {
                loaded?.Dispose();
            }
        }

        private bool TryImportSection1Image(string filePath, string imagePath, out string error)
        {
            error = null;
            Bitmap bmp = null;
            try
            {
                if (!TryLoadImportBitmap8bpp(imagePath, out bmp, out error))
                    return false;

                if (bmp.Width != 512 || bmp.Height != 512)
                {
                    error = $"Section 1 must be 512x512 (got {bmp.Width}x{bmp.Height}).";
                    return false;
                }

                BuildGameImportData(bmp, out byte[] palBuffer, out byte[] pixelBuffer);
                byte[] sectionBuffer = new byte[512 * 512 + 1024];
                Buffer.BlockCopy(palBuffer, 0, sectionBuffer, 0, 1024);
                Buffer.BlockCopy(pixelBuffer, 0, sectionBuffer, 1024, 512 * 512);

                byte[] fileBuffer = File.ReadAllBytes(filePath);
                byte[] originalPalette = ReadPaletteBlock(fileBuffer, 0);
                int section2PaletteOffset = GetSection2PaletteOffset(fileBuffer.Length);
                int section1SearchEnd = section2PaletteOffset >= 0 ? section2PaletteOffset : fileBuffer.Length;
                List<int> section1PaletteOffsets = FindPaletteOffsetsInRange(fileBuffer, originalPalette, 0, section1SearchEnd);

                Buffer.BlockCopy(sectionBuffer, 0, fileBuffer, 0, sectionBuffer.Length);
                ApplyPaletteAtOffsets(fileBuffer, section1PaletteOffsets, palBuffer);
                File.WriteAllBytes(filePath, fileBuffer);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
            finally
            {
                bmp?.Dispose();
            }
        }

        private bool TryImportSection2Image(string filePath, string imagePath, out string error)
        {
            error = null;
            Bitmap bmp = null;
            try
            {
                if (!TryLoadImportBitmap8bpp(imagePath, out bmp, out error))
                    return false;

                if (bmp.Width != 512)
                {
                    error = $"Section 2 must be 512 pixels wide (got {bmp.Width}x{bmp.Height}).";
                    return false;
                }

                BuildGameImportData(bmp, out byte[] palBuffer, out byte[] pixelBuffer);
                byte[] sectionBuffer = new byte[512 * bmp.Height + 1024];
                Buffer.BlockCopy(palBuffer, 0, sectionBuffer, 0, 1024);
                Buffer.BlockCopy(pixelBuffer, 0, sectionBuffer, 1024, 512 * bmp.Height);

                byte[] fileBuffer = File.ReadAllBytes(filePath);
                Buffer.BlockCopy(sectionBuffer, 0, fileBuffer, fileBuffer.Length - 0x80400, sectionBuffer.Length);
                File.WriteAllBytes(filePath, fileBuffer);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
            finally
            {
                bmp?.Dispose();
            }
        }

        private byte[] PaletteToByte(System.Drawing.Color[] pal)
        {
            throw new Exception("NO");
        }

        private static void BuildGameImportData(Bitmap bmp, out byte[] paletteBuffer, out byte[] pixelBuffer)
        {
            paletteBuffer = new byte[1024];
            System.Drawing.Color[] entries = bmp.Palette.Entries;

            for (int i = 0; i < 256; i++)
            {
                System.Drawing.Color color = i < entries.Length ? entries[i] : System.Drawing.Color.FromArgb(255, 0, 0, 0);

                paletteBuffer[i * 4 + 0] = color.B;
                paletteBuffer[i * 4 + 1] = color.G;
                paletteBuffer[i * 4 + 2] = color.R;
                paletteBuffer[i * 4 + 3] = (byte)(255 - color.A);
            }

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            try
            {
                pixelBuffer = new byte[bmp.Width * bmp.Height];
                IntPtr topRowPointer = GetTopRowPointer(bmpData, bmp.Height);
                int rowStep = -bmpData.Stride;
                for (int y = 0; y < bmp.Height; y++)
                {
                    int targetY = SectionImageRowsAreBottomUp ? (bmp.Height - 1 - y) : y;
                    int targetOffset = targetY * bmp.Width;
                    IntPtr sourceRow = IntPtr.Add(topRowPointer, y * rowStep);
                    Marshal.Copy(sourceRow, pixelBuffer, targetOffset, bmp.Width);
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
        }

        private static byte[] ReadPaletteBlock(byte[] fileBuffer, int offset)
        {
            byte[] paletteBuffer = new byte[1024];
            if (fileBuffer != null && offset >= 0 && offset + 1024 <= fileBuffer.Length)
                Buffer.BlockCopy(fileBuffer, offset, paletteBuffer, 0, 1024);

            return paletteBuffer;
        }

        private static int GetSection2PaletteOffset(int fileLength)
        {
            int offset = fileLength - 0x80400;
            return offset >= 0 ? offset : -1;
        }

        private static List<int> FindPaletteOffsets(byte[] fileBuffer, byte[] paletteBuffer)
        {
            return FindPaletteOffsetsInRange(fileBuffer, paletteBuffer, 0, fileBuffer != null ? fileBuffer.Length : 0);
        }

        private static List<int> FindPaletteOffsetsInRange(byte[] fileBuffer, byte[] paletteBuffer, int startOffset, int endOffsetExclusive)
        {
            List<int> offsets = new List<int>();
            if (fileBuffer == null || paletteBuffer == null || paletteBuffer.Length < 1024 || fileBuffer.Length < 1024)
                return offsets;

            int start = Math.Max(0, startOffset);
            int endExclusive = Math.Min(fileBuffer.Length, endOffsetExclusive);
            int maxOffset = endExclusive - 1024;
            if (start > maxOffset)
                return offsets;

            for (int offset = start; offset <= maxOffset; offset++)
            {
                bool same = true;
                for (int i = 0; i < 1024; i++)
                {
                    if (fileBuffer[offset + i] != paletteBuffer[i])
                    {
                        same = false;
                        break;
                    }
                }

                if (same)
                    offsets.Add(offset);
            }

            return offsets;
        }

        private static void ApplyPaletteAtOffsets(byte[] fileBuffer, IEnumerable<int> offsets, byte[] newPalette)
        {
            if (fileBuffer == null || offsets == null || newPalette == null || newPalette.Length < 1024)
                return;

            foreach (int offset in offsets)
            {
                if (offset < 0 || offset + 1024 > fileBuffer.Length)
                    continue;

                Buffer.BlockCopy(newPalette, 0, fileBuffer, offset, 1024);
            }
        }

        private void UpdatePaletteInfo(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                buttonSection1PaletteInfo.Text = "Image Info: n/a";
                buttonSection2PaletteInfo.Text = "Image Info: n/a";
                currentSection1PaletteOffsets = new List<int>();
                currentSection2PaletteOffsets = new List<int>();
                currentSection1Palette = new byte[1024];
                currentSection2Palette = new byte[1024];
                currentHasSection2Palette = false;
                UpdateSharedPaletteInfoDisplay();
                return;
            }

            byte[] fileBuffer = File.ReadAllBytes(filePath);
            if (fileBuffer.Length < 1024)
            {
                buttonSection1PaletteInfo.Text = "Image Info: n/a";
                buttonSection2PaletteInfo.Text = "Image Info: n/a";
                currentSection1PaletteOffsets = new List<int>();
                currentSection2PaletteOffsets = new List<int>();
                currentSection1Palette = new byte[1024];
                currentSection2Palette = new byte[1024];
                currentHasSection2Palette = false;
                UpdateSharedPaletteInfoDisplay();
                return;
            }

            byte[] Section1Palette = ReadPaletteBlock(fileBuffer, 0);
            int Section2PaletteOffset = GetSection2PaletteOffset(fileBuffer.Length);
            byte[] Section2Palette = Section2PaletteOffset >= 0 ? ReadPaletteBlock(fileBuffer, Section2PaletteOffset) : new byte[1024];

            currentSection1Palette = Section1Palette;
            currentSection2Palette = Section2Palette;
            currentHasSection2Palette = Section2PaletteOffset >= 0;

            int Section1SearchEnd = Section2PaletteOffset >= 0 ? Section2PaletteOffset : fileBuffer.Length;
            currentSection1PaletteOffsets = FindPaletteOffsetsInRange(fileBuffer, Section1Palette, 0, Section1SearchEnd);
            currentSection2PaletteOffsets = Section2PaletteOffset >= 0 ? new List<int> { Section2PaletteOffset } : new List<int>();

            buttonSection1PaletteInfo.Text = $"Image Info ({currentSection1PaletteOffsets.Count})";
            buttonSection2PaletteInfo.Text = Section2PaletteOffset >= 0
                ? $"Image Info 0x{Section2PaletteOffset:X6}"
                : "Image Info: n/a";

            UpdateSharedPaletteInfoDisplay();
        }

        private void UpdateSharedPaletteInfoDisplay()
        {
            if (dataGridViewSharedPaletteInfo == null)
                return;

            dataGridViewSharedPaletteInfo.SuspendLayout();
            dataGridViewSharedPaletteInfo.Rows.Clear();

            if (!currentHasSection2Palette)
            {
                dataGridViewSharedPaletteInfo.Rows.Add("n/a", "Section 2 palette is not available", string.Empty);
                dataGridViewSharedPaletteInfo.ResumeLayout();
                return;
            }

            Dictionary<string, List<int>> section1ByColor = BuildPaletteIndexMapByRgb(currentSection1Palette);
            Dictionary<string, List<int>> section2ByColor = BuildPaletteIndexMapByRgb(currentSection2Palette);

            int sharedCount = 0;
            foreach (string color in section1ByColor.Keys.OrderBy(k => k, StringComparer.Ordinal))
            {
                if (!section2ByColor.TryGetValue(color, out List<int> section2Indexes))
                    continue;

                List<int> section1Indexes = section1ByColor[color];
                dataGridViewSharedPaletteInfo.Rows.Add(color, string.Join(",", section1Indexes), string.Join(",", section2Indexes));
                sharedCount++;
            }

            if (sharedCount == 0)
            {
                dataGridViewSharedPaletteInfo.Rows.Add("n/a", "No shared colors found", string.Empty);
                dataGridViewSharedPaletteInfo.ResumeLayout();
                return;
            }

            dataGridViewSharedPaletteInfo.ResumeLayout();
        }

        private static Dictionary<string, List<int>> BuildPaletteIndexMapByRgb(byte[] palette)
        {
            Dictionary<string, List<int>> byColor = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            if (palette == null || palette.Length < 1024)
                return byColor;

            for (int i = 0; i < 256; i++)
            {
                int baseOffset = i * 4;
                byte blue = palette[baseOffset + 0];
                byte green = palette[baseOffset + 1];
                byte red = palette[baseOffset + 2];
                string rgb = $"{red:X2}{green:X2}{blue:X2}";

                if (!byColor.TryGetValue(rgb, out List<int> indexes))
                {
                    indexes = new List<int>();
                    byColor[rgb] = indexes;
                }

                indexes.Add(i);
            }

            return byColor;
        }

        private void ShowPaletteOffsets(string title, List<int> offsets, byte[] palette)
        {
            if (offsets == null || offsets.Count == 0)
            {
                ShowAppMessage("No matching palette offsets were found.", title, MessageBoxIcon.Information);
                return;
            }

            bool darkMode = darkModeToolStripMenuItem.Checked;
            using (PaletteInfoDialog dialog = new PaletteInfoDialog(title, offsets, palette, darkMode))
            {
                dialog.ShowDialog(this);
            }
        }

        private void buttonSection1PaletteInfo_Click(object sender, EventArgs e)
        {
            ShowPaletteOffsets("Section 1 palette offsets", currentSection1PaletteOffsets, currentSection1Palette);
        }

        private void buttonSection2PaletteInfo_Click(object sender, EventArgs e)
        {
            ShowPaletteOffsets("Section 2 palette offsets", currentSection2PaletteOffsets, currentSection2Palette);
        }

        private void browseAndMassExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string lastOpenedFile = LoadLastOpenedFile();
            string initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!string.IsNullOrWhiteSpace(lastOpenedFile) && File.Exists(lastOpenedFile))
                initialDirectory = Path.GetDirectoryName(lastOpenedFile);

            using (OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select folder for mass export",
                InitialDirectory = initialDirectory,
                FileName = "Select Folder",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true
            })
            {
                if (ShowThemedCommonDialog(dialog, this) != DialogResult.OK) return;

                string sourceFolder = Path.GetDirectoryName(dialog.FileName);
                if (string.IsNullOrWhiteSpace(sourceFolder) || !Directory.Exists(sourceFolder)) return;

                string[] mapFiles = Directory.GetFiles(sourceFolder, "map*.bin", SearchOption.TopDirectoryOnly)
                    .OrderBy(Path.GetFileName)
                    .ToArray();

                if (mapFiles.Length == 0)
                {
                    string[] gzipFiles = GetMapGzipFiles(sourceFolder);
                    if (gzipFiles.Length == 0)
                    {
                        ShowAppMessage("No map*.bin files found in the selected folder.", "Mass export", MessageBoxIcon.Warning);
                        return;
                    }

                DialogResult decision = ShowAppMessageWithActions(
                    "No map*.bin files were found, but map*.bin.gz files were detected.\n\nDo you want to decompress them now and continue mass export?",
                    "Compressed map files detected",
                    "Decompress",
                    "Cancel",
                    MessageBoxIcon.Warning);

                if (decision != DialogResult.OK) return;

                bool cancelled = DecompressMapGzipFilesWithProgress(gzipFiles, "Decompressing map files", out int decompressedCount, out int skippedCount, out int failedCount);

                ShowAppMessage(
                    $"Decompression {(cancelled ? "cancelled" : "finished")}.\n\nDecompressed: {decompressedCount}\nSkipped existing: {skippedCount}\nFailed: {failedCount}",
                    "Decompression result",
                    failedCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                mapFiles = Directory.GetFiles(sourceFolder, "map*.bin", SearchOption.TopDirectoryOnly)
                    .OrderBy(Path.GetFileName)
                    .ToArray();

                if (mapFiles.Length == 0)
                {
                    ShowAppMessage("No map*.bin files are available after decompression.", "Mass export", MessageBoxIcon.Warning);
                    return;
                }
            }

            if (!ShowMassExportCautionDialog()) return;

            string exportExtension = ShowMassExportFormatDialog();
            if (string.IsNullOrWhiteSpace(exportExtension)) return;

            ImageFormat exportImageFormat = exportExtension == "bmp" ? ImageFormat.Bmp : ImageFormat.Png;

            string exportFolder = Path.Combine(sourceFolder, "mass_export");
            Directory.CreateDirectory(exportFolder);

            using (Form progressForm = new Form())
            using (Label statusLabel = new Label())
            using (ProgressBar progressBar = new ProgressBar())
            using (Button cancelButton = new Button())
            {
                    progressForm.Text = "Mass export in progress";
                    progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    progressForm.StartPosition = FormStartPosition.CenterParent;
                    progressForm.MinimizeBox = false;
                    progressForm.MaximizeBox = false;
                    progressForm.ControlBox = false;
                    progressForm.ClientSize = new Size(520, 120);

                    statusLabel.AutoSize = false;
                    statusLabel.TextAlign = ContentAlignment.MiddleLeft;
                    statusLabel.Dock = DockStyle.Top;
                    statusLabel.Height = 56;
                    statusLabel.Text = "Preparing export...";

                    progressBar.Dock = DockStyle.Bottom;
                    progressBar.Height = 24;
                    progressBar.Minimum = 0;
                    progressBar.Maximum = mapFiles.Length;
                    progressBar.Value = 0;

                    bool cancelRequested = false;
                    cancelButton.Text = "Cancel";
                    cancelButton.Size = new Size(90, 26);
                    cancelButton.Location = new Point(progressForm.ClientSize.Width - cancelButton.Width - 12, 62);
                    cancelButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                    cancelButton.Click += (s, evt) =>
                    {
                        cancelRequested = true;
                        cancelButton.Enabled = false;
                        statusLabel.Text = "Cancelling after current file...";
                    };

                    progressForm.Controls.Add(statusLabel);
                    progressForm.Controls.Add(cancelButton);
                    progressForm.Controls.Add(progressBar);
                    GetThemeColors(darkModeToolStripMenuItem.Checked, out System.Drawing.Color background, out System.Drawing.Color surface, out System.Drawing.Color foreground);
                    ApplyThemeToControlTree(progressForm, background, surface, foreground, darkModeToolStripMenuItem.Checked);
                    ApplyTitleBarThemeToForm(progressForm, darkModeToolStripMenuItem.Checked);
                    progressForm.Show(this);
                    progressForm.Refresh();

                    int exportedCount = 0;
                    int failedCount = 0;
                    bool cancelled = false;

                    for (int i = 0; i < mapFiles.Length; i++)
                    {
                        if (cancelRequested)
                        {
                            cancelled = true;
                            break;
                        }

                        string filePath = mapFiles[i];
                        statusLabel.Text = $"Exporting {i + 1}/{mapFiles.Length}: {Path.GetFileName(filePath)}";
                        progressBar.Value = i + 1;
                        progressForm.Refresh();
                        Application.DoEvents();

                        if (cancelRequested)
                        {
                            cancelled = true;
                            break;
                        }

                        try
                        {
                            LoadSectionBitmaps(filePath, out Bitmap firstSection, out Bitmap secondSection);
                            string fileBase = Path.GetFileNameWithoutExtension(filePath);

                            using (firstSection)
                                firstSection.Save(Path.Combine(exportFolder, $"{fileBase}_Section1.{exportExtension}"), exportImageFormat);

                            if (secondSection != null)
                                using (secondSection)
                                    secondSection.Save(Path.Combine(exportFolder, $"{fileBase}_Section2.{exportExtension}"), exportImageFormat);

                            exportedCount++;
                        }
                        catch
                        {
                            failedCount++;
                        }
                    }

                    progressForm.Close();
                    if (cancelled)
                    {
                        ShowAppMessage(
                            $"Mass export cancelled.\n\nExported: {exportedCount}\nFailed: {failedCount}\nOutput folder: {exportFolder}",
                            "Mass export",
                            MessageBoxIcon.Warning);
                    }
                    else
                    {
                        ShowAppMessage(
                            $"Mass export completed.\n\nExported: {exportedCount}\nFailed: {failedCount}\nOutput folder: {exportFolder}",
                            "Mass export",
                            failedCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    }
            }
            }
        }

        private void massImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string targetFolder = GetCurrentMapFolder();
            if (string.IsNullOrWhiteSpace(targetFolder) || !Directory.Exists(targetFolder))
            {
                ShowAppMessage("No working map folder is loaded yet. Please use Browse first.", "Mass import", MessageBoxIcon.Warning);
                return;
            }

            string[] mapFiles = Directory.GetFiles(targetFolder, "map*.bin", SearchOption.TopDirectoryOnly)
                .OrderBy(Path.GetFileName)
                .ToArray();

            if (mapFiles.Length == 0)
            {
                ShowAppMessage("No map*.bin files found in the selected folder.", "Mass import", MessageBoxIcon.Warning);
                return;
            }

            string defaultImportFolder = Path.Combine(targetFolder, "mass_export");
            string importFolder;
            using (OpenFileDialog importFolderDialog = new OpenFileDialog()
            {
                Title = "Select folder containing *_Section1 / *_Section2 PNG or BMP files",
                InitialDirectory = Directory.Exists(defaultImportFolder) ? defaultImportFolder : targetFolder,
                FileName = "Select Folder",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Multiselect = false
            })
            {
                if (ShowThemedCommonDialog(importFolderDialog, this) != DialogResult.OK) return;
                importFolder = Path.GetDirectoryName(importFolderDialog.FileName);
            }

            if (string.IsNullOrWhiteSpace(importFolder) || !Directory.Exists(importFolder))
            {
                ShowAppMessage("Selected import folder is not valid.", "Mass import", MessageBoxIcon.Warning);
                return;
            }

            using (Form progressForm = new Form())
            using (Label statusLabel = new Label())
            using (ProgressBar progressBar = new ProgressBar())
            using (Button cancelButton = new Button())
            {
                progressForm.Text = "Mass import in progress";
                progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                progressForm.StartPosition = FormStartPosition.CenterParent;
                progressForm.MinimizeBox = false;
                progressForm.MaximizeBox = false;
                progressForm.ControlBox = false;
                progressForm.ClientSize = new Size(560, 120);

                statusLabel.AutoSize = false;
                statusLabel.TextAlign = ContentAlignment.MiddleLeft;
                statusLabel.Dock = DockStyle.Top;
                statusLabel.Height = 56;
                statusLabel.Text = "Preparing import...";

                progressBar.Dock = DockStyle.Bottom;
                progressBar.Height = 24;
                progressBar.Minimum = 0;
                progressBar.Maximum = mapFiles.Length;
                progressBar.Value = 0;

                bool cancelRequested = false;
                cancelButton.Text = "Cancel";
                cancelButton.Size = new Size(90, 26);
                cancelButton.Location = new Point(progressForm.ClientSize.Width - cancelButton.Width - 12, 62);
                cancelButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                cancelButton.Click += (s, evt) =>
                {
                    cancelRequested = true;
                    cancelButton.Enabled = false;
                    statusLabel.Text = "Cancelling after current file...";
                };

                progressForm.Controls.Add(statusLabel);
                progressForm.Controls.Add(cancelButton);
                progressForm.Controls.Add(progressBar);
                GetThemeColors(darkModeToolStripMenuItem.Checked, out System.Drawing.Color background, out System.Drawing.Color surface, out System.Drawing.Color foreground);
                ApplyThemeToControlTree(progressForm, background, surface, foreground, darkModeToolStripMenuItem.Checked);
                ApplyTitleBarThemeToForm(progressForm, darkModeToolStripMenuItem.Checked);
                progressForm.Show(this);
                progressForm.Refresh();

                int importedSection1 = 0;
                int importedSection2 = 0;
                int updatedFiles = 0;
                int skippedFiles = 0;
                int failedOperations = 0;
                List<string> failureEntries = new List<string>();
                bool cancelled = false;

                for (int i = 0; i < mapFiles.Length; i++)
                {
                    if (cancelRequested)
                    {
                        cancelled = true;
                        break;
                    }

                    string filePath = mapFiles[i];
                    string fileBase = Path.GetFileNameWithoutExtension(filePath);
                    statusLabel.Text = $"Importing {i + 1}/{mapFiles.Length}: {Path.GetFileName(filePath)}";
                    progressBar.Value = i + 1;
                    progressForm.Refresh();
                    Application.DoEvents();

                    string section1Image = FindExistingSectionImagePath(importFolder, fileBase, "Section1");
                    string section2Image = FindExistingSectionImagePath(importFolder, fileBase, "Section2");

                    if (section1Image == null && section2Image == null)
                    {
                        skippedFiles++;
                        continue;
                    }

                    bool fileUpdated = false;

                    if (section1Image != null)
                    {
                        if (TryImportSection1Image(filePath, section1Image, out string section1Error))
                        {
                            importedSection1++;
                            fileUpdated = true;
                        }
                        else
                        {
                            failedOperations++;
                            failureEntries.Add($"{Path.GetFileName(filePath)} | Section1 | {section1Error ?? "Unknown error"}");
                        }
                    }

                    if (section2Image != null)
                    {
                        if (TryImportSection2Image(filePath, section2Image, out string section2Error))
                        {
                            importedSection2++;
                            fileUpdated = true;
                        }
                        else
                        {
                            failedOperations++;
                            failureEntries.Add($"{Path.GetFileName(filePath)} | Section2 | {section2Error ?? "Unknown error"}");
                        }
                    }

                    if (fileUpdated)
                        updatedFiles++;
                }

                progressForm.Close();

                string selectedMap = GetSelectedMapFilePath();
                if (!string.IsNullOrWhiteSpace(selectedMap) && File.Exists(selectedMap))
                    RenderImage(selectedMap);

                string logPath = null;
                if (failureEntries.Count > 0)
                {
                    try
                    {
                        string logName = $"mass_import_failures_{DateTime.Now:yyyyMMdd_HHmmss}.log";
                        logPath = Path.Combine(targetFolder, logName);
                        string logBody =
                            "Mass import failure log" + Environment.NewLine +
                            $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}" + Environment.NewLine +
                            $"Working folder: {targetFolder}" + Environment.NewLine +
                            $"Import folder: {importFolder}" + Environment.NewLine +
                            Environment.NewLine +
                            string.Join(Environment.NewLine, failureEntries);
                        File.WriteAllText(logPath, logBody, Encoding.UTF8);
                    }
                    catch
                    {
                        logPath = null;
                    }
                }

                string summary =
                    $"Mass import {(cancelled ? "cancelled" : "completed")}.\n\n" +
                    $"Updated files: {updatedFiles}\n" +
                    $"Imported Section 1: {importedSection1}\n" +
                    $"Imported Section 2: {importedSection2}\n" +
                    $"Skipped (no images): {skippedFiles}\n" +
                    $"Failed operations: {failedOperations}\n" +
                    $"Import folder: {importFolder}" +
                    (string.IsNullOrWhiteSpace(logPath) ? string.Empty : $"\nFailure log: {logPath}");

                ShowAppMessage(
                    summary,
                    "Mass import",
                    failedOperations == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
        }

        private void createBackupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string folderPath = GetCurrentMapFolder();
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                ShowAppMessage("No working map folder is loaded yet. Please use Browse first.", "Map backup", MessageBoxIcon.Warning);
                return;
            }

            string backupRoot = Path.Combine(folderPath, "map_backup");
            bool backupRootHasContent = false;
            int existingBackupSetCount = 0;
            if (Directory.Exists(backupRoot))
            {
                try
                {
                    backupRootHasContent = Directory.EnumerateFileSystemEntries(backupRoot).Any();
                    existingBackupSetCount = Directory.EnumerateDirectories(backupRoot).Count();
                }
                catch
                {
                    backupRootHasContent = true;
                    existingBackupSetCount = 0;
                }
            }

            if (backupRootHasContent)
            {
                DialogResult decision = ShowAppMessageWithActions(
                    $"A backup folder already exists for this map set. Existing backup sets found: {existingBackupSetCount}. Creating another backup may produce redundant backup copies.\n\nDo you want to proceed and create a new backup anyway?",
                    "Map backup",
                    "Create Backup",
                    "No",
                    MessageBoxIcon.Warning);

                if (decision != DialogResult.OK)
                    return;
            }

            if (!TryCreateMapBackup(folderPath, out int backedUpCount, out int skippedCount, out int failedCount, out string outputFolder))
            {
                ShowAppMessage("No map*.bin files found in the current folder.", "Map backup", MessageBoxIcon.Warning);
                return;
            }

            ShowAppMessage(
                $"Backup finished.\n\nBacked up: {backedUpCount}\nSkipped existing: {skippedCount}\nFailed: {failedCount}\nOutput folder: {outputFolder}",
                "Map backup",
                failedCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void filterOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetActiveMapFilter(MapCategoryFilter.Off);
        }

        private void filterSnowTilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetActiveMapFilter(MapCategoryFilter.SnowTiles);
        }

        private void filterGrassTilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetActiveMapFilter(MapCategoryFilter.GrassTiles);
        }

        private void filterMagitekTilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetActiveMapFilter(MapCategoryFilter.MagitekTiles);
        }

        private void filterShipDeckTilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetActiveMapFilter(MapCategoryFilter.ShipDeckTiles);
        }

        private void comboBoxFileFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isSyncingFilterDropdown) return;

            MapCategoryFilter selectedFilter = MapCategoryFilter.Off;
            if (comboBoxFileFilter.SelectedIndex == 1)
                selectedFilter = MapCategoryFilter.SnowTiles;
            else if (comboBoxFileFilter.SelectedIndex == 2)
                selectedFilter = MapCategoryFilter.GrassTiles;
            else if (comboBoxFileFilter.SelectedIndex == 3)
                selectedFilter = MapCategoryFilter.MagitekTiles;
            else if (comboBoxFileFilter.SelectedIndex == 4)
                selectedFilter = MapCategoryFilter.ShipDeckTiles;

            if (selectedFilter != activeMapFilter)
                SetActiveMapFilter(selectedFilter);
        }

        private void comboBoxFileFilter_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (!(sender is ComboBox comboBox)) return;

            bool darkMode = darkModeToolStripMenuItem != null && darkModeToolStripMenuItem.Checked;
            GetThemeColors(darkMode, out System.Drawing.Color background, out System.Drawing.Color surface, out System.Drawing.Color foreground);

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            System.Drawing.Color itemBackColor = surface;
            System.Drawing.Color itemForeColor = foreground;

            if (isSelected)
            {
                itemBackColor = darkMode ? System.Drawing.Color.FromArgb(63, 63, 70) : SystemColors.Highlight;
                itemForeColor = darkMode ? foreground : SystemColors.HighlightText;
            }

            using (SolidBrush backBrush = new SolidBrush(itemBackColor))
            using (SolidBrush textBrush = new SolidBrush(itemForeColor))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);

                if (e.Index >= 0 && e.Index < comboBox.Items.Count)
                {
                    string itemText = comboBox.GetItemText(comboBox.Items[e.Index]);
                    Rectangle textBounds = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, e.Bounds.Width - 4, e.Bounds.Height - 4);
                    e.Graphics.DrawString(itemText, e.Font, textBrush, textBounds);
                }
                else
                {
                    Rectangle textBounds = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, e.Bounds.Width - 4, e.Bounds.Height - 4);
                    e.Graphics.DrawString(comboBox.Text, e.Font, textBrush, textBounds);
                }
            }

            e.DrawFocusRectangle();
        }

        private void isolateFilteredFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (st == null || st.Length == 0)
            {
                ShowAppMessage("No files are currently available in the selected filter.", "Isolate filtered files", MessageBoxIcon.Warning);
                return;
            }

            string sourceFolder = Path.GetDirectoryName(st[0]);
            if (string.IsNullOrWhiteSpace(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                ShowAppMessage("Unable to locate current source folder.", "Isolate filtered files", MessageBoxIcon.Warning);
                return;
            }

            string destinationRoot = Path.Combine(sourceFolder, "filtered_sets");
            Directory.CreateDirectory(destinationRoot);

            string destinationFolder = Path.Combine(destinationRoot, $"{GetFilterLabel()}_{DateTime.Now:yyyyMMdd_HHmmss}");
            Directory.CreateDirectory(destinationFolder);

            int copiedCount = 0;
            int failedCount = 0;
            foreach (string filePath in st)
            {
                try
                {
                    string destinationPath = Path.Combine(destinationFolder, Path.GetFileName(filePath));
                    File.Copy(filePath, destinationPath, false);
                    copiedCount++;
                }
                catch
                {
                    failedCount++;
                }
            }

            DialogResult openDecision = ShowAppMessageWithActions(
                $"Filtered set created.\n\nCopied: {copiedCount}\nFailed: {failedCount}\nFolder: {destinationFolder}\n\nOpen this isolated folder now?",
                "Isolate filtered files",
                "Open Folder",
                "Keep Current",
                failedCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (openDecision == DialogResult.OK)
            {
                activeMapFilter = MapCategoryFilter.Off;
                LoadMapFilesFromFolder(destinationFolder);
                if (st != null && st.Length > 0)
                    SaveLastOpenedFile(st[0]);
            }
        }
    }
}

