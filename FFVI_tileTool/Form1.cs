using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFVI_tileTool
{
    public partial class Form1 : Form
    {
        private const string LastOpenedFileStateName = "last-opened-map.txt";
        private const string DarkModeStateName = "dark-mode.txt";
        private const string DefaultWindowTitle = "FFVI tile tool";

        struct Color
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;
        }

        struct MapTile
        {
            public Color[] palette;
            public byte[] imgBuff;
        }

        string[] st;
        public Form1()
        {
            InitializeComponent();
            Text = DefaultWindowTitle;

            bool darkModeEnabled = LoadDarkModeState();
            darkModeToolStripMenuItem.Checked = darkModeEnabled;
            ApplyTheme(darkModeEnabled);

            RestoreLastOpenedFile();
        }

        private void browseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string lastOpenedFile = LoadLastOpenedFile();
            string initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!string.IsNullOrWhiteSpace(lastOpenedFile) && File.Exists(lastOpenedFile))
                initialDirectory = Path.GetDirectoryName(lastOpenedFile);

            using (OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select one map*.bin file",
                Filter = "Map files (map*.bin)|map*.bin|All BIN files (*.bin)|*.bin|All files (*.*)|*.*",
                Multiselect = false,
                CheckFileExists = true,
                InitialDirectory = initialDirectory
            })
            {
                if (openFileDialog.ShowDialog() != DialogResult.OK) return;
                LoadMapFilesFromFolder(Path.GetDirectoryName(openFileDialog.FileName), openFileDialog.FileName);
                SaveLastOpenedFile(openFileDialog.FileName);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox listBox = (sender as ListBox);
            if (listBox.Items.Count == 0) return;
            if (st == null) return;
            if (st.Length == 0) return;

            
            string filePath = st.Where(x => Path.GetFileName(x) == (string)listBox.SelectedValue).First();
            RenderImage(filePath);
            SaveLastOpenedFile(filePath);
            UpdateWindowTitle(filePath);
        }

        private void LoadMapFilesFromFolder(string folderPath, string fileToSelect = null)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                st = new string[0];
                listBox1.DataSource = null;
                return;
            }

            st = Directory.GetFiles(folderPath, "map*.bin", SearchOption.TopDirectoryOnly);
            if (st.Length == 0)
            {
                listBox1.DataSource = null;
                return;
            }

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
        }

        private string GetStateFilePath()
        {
            return Path.Combine(Application.UserAppDataPath, LastOpenedFileStateName);
        }

        private string GetDarkModeStateFilePath()
        {
            return Path.Combine(Application.UserAppDataPath, DarkModeStateName);
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

        private void ApplyTheme(bool darkMode)
        {
            System.Drawing.Color background = darkMode ? System.Drawing.Color.FromArgb(30, 30, 30) : SystemColors.Control;
            System.Drawing.Color surface = darkMode ? System.Drawing.Color.FromArgb(45, 45, 48) : SystemColors.Window;
            System.Drawing.Color foreground = darkMode ? System.Drawing.Color.Gainsboro : SystemColors.ControlText;

            ApplyThemeToControlTree(this, background, surface, foreground, darkMode);
            ApplyThemeToMenu(menuStrip1, surface, foreground);
            Invalidate(true);
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

        private void RenderImage(string file)
        {
            byte[] paletteBuffer = new byte[1024];
            byte[] secPaletteBuffer = new byte[4096];
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            paletteBuffer = br.ReadBytes(1024);
            byte[] firstImageBuffer = br.ReadBytes(512 * 512);
            //secPaletteBuffer = br.ReadBytes(4096);
            byte[] secondImageBuffer = new byte[0];
            if (fs.Length > 0x80400 + 1024)
            {
                fs.Seek(-0x80400, SeekOrigin.End); //Ark's hack
                secPaletteBuffer = br.ReadBytes(1024);
                secondImageBuffer = br.ReadBytes((int)(fs.Length - fs.Position));
            }
            br.Close();
            fs.Close();
            fs.Dispose();


            Bitmap bmpOne = new Bitmap(512, 512, PixelFormat.Format8bppIndexed);

            MapTile mapTile = new MapTile() { palette = new Color[256], imgBuff = firstImageBuffer };
            for (int i = 0; i < mapTile.palette.Length; i++)
                mapTile.palette[i] = new Color() { R = paletteBuffer[i * 4], G = paletteBuffer[i * 4 + 1], B = paletteBuffer[i * 4 + 2], A = paletteBuffer[i * 4 + 3] };
            ColorPalette cp = bmpOne.Palette;
            for (int i = 0; i < 256; i++)
                cp.Entries[i] = System.Drawing.Color.FromArgb(
                    255 - mapTile.palette[i].A,
                    mapTile.palette[i].B,
                    mapTile.palette[i].G,
                    mapTile.palette[i].R);
            bmpOne.Palette = cp;
            BitmapData bmpData = bmpOne.LockBits(new Rectangle(0, 0, bmpOne.Width, bmpOne.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            byte[] bmpDataBuffer = new byte[bmpData.Width * bmpData.Height];
            Marshal.Copy(mapTile.imgBuff, 0, bmpData.Scan0, mapTile.imgBuff.Length);
            bmpOne.UnlockBits(bmpData);

            pictureBox1.Size = bmpOne.Size;
            pictureBox1.Image = bmpOne;

            //two
            if (secondImageBuffer.Length > 512)
            {
                Bitmap bmpTwo = new Bitmap(512, secondImageBuffer.Length / 512, PixelFormat.Format8bppIndexed);
                mapTile = new MapTile() { palette = new Color[256], imgBuff = secondImageBuffer };
                for (int i = 0; i < mapTile.palette.Length; i++)
                    mapTile.palette[i] = new Color() { R = secPaletteBuffer[i * 4], G = secPaletteBuffer[i * 4 + 1], B = secPaletteBuffer[i * 4 + 2], A = secPaletteBuffer[i * 4 + 3] };
                cp = bmpTwo.Palette;
                for (int i = 0; i < 256; i++)
                    cp.Entries[i] = System.Drawing.Color.FromArgb(
                        255 - mapTile.palette[i].A,
                        mapTile.palette[i].B,
                        mapTile.palette[i].G,
                        mapTile.palette[i].R);
                bmpTwo.Palette = cp;
                bmpData = bmpTwo.LockBits(new Rectangle(0, 0, bmpTwo.Width, bmpTwo.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                bmpDataBuffer = new byte[bmpData.Width * bmpData.Height];
                Marshal.Copy(mapTile.imgBuff, 0, bmpData.Scan0, mapTile.imgBuff.Length);
                bmpTwo.UnlockBits(bmpData);

                pictureBox2.Size = bmpTwo.Size;
                panel2.AutoScrollMinSize = bmpTwo.Size;
                pictureBox2.Image = bmpTwo;
            }
            else
            {
                pictureBox2.Size = new Size(512, 512);
                panel2.AutoScrollMinSize = pictureBox2.Size;
                pictureBox2.Image = null;
            }

            pictureBox1.Image = bmpOne;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ExportImage(pictureBox1.Image, $"{listBox1.SelectedValue}_chunk1");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ExportImage(pictureBox2.Image, $"{listBox1.SelectedValue}_chunk2");
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
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string extension = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                    ImageFormat format = extension == ".bmp" ? ImageFormat.Bmp : ImageFormat.Png;
                    image.Save(sfd.FileName, format);
                }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //is 1st chunk
            string path = "";
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Image files (*.png;*.bmp)|*.png;*.bmp|PNG files (*.png)|*.png|BMP files (*.bmp)|*.bmp", Multiselect = false })
                if (ofd.ShowDialog() == DialogResult.OK)
                    path = ofd.FileName;
                else return;

            Bitmap bmp = new Bitmap(path);
            if(bmp.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                MessageBox.Show("Image is not 8BPP indexed.");
                return;
            }
            if(bmp.Height != 512 || bmp.Width != 512)
            {
                MessageBox.Show($"Chunk 1 is always 512x512. You are trying to import {bmp.Width}x{bmp.Height}.");
                return;
            }
            byte[] palBuffer = BuildPalette(bmp);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, 512, 512), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            byte[] b = new byte[512 * 512 + 1024];
            Buffer.BlockCopy(palBuffer, 0, b, 0, 1024);
            Marshal.Copy(bmpData.Scan0, b, 1024, 512 * 512);
            bmp.UnlockBits(bmpData);

            string filePath = st.Where(x => Path.GetFileName(x) == (string)listBox1.SelectedValue).First();
            byte[] bb = File.ReadAllBytes(filePath);
            Buffer.BlockCopy(b, 0, bb, 0, b.Length);
            File.WriteAllBytes(filePath, bb);
            RenderImage(filePath);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //2nd chunk
            string path = "";
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Image files (*.png;*.bmp)|*.png;*.bmp|PNG files (*.png)|*.png|BMP files (*.bmp)|*.bmp", Multiselect = false })
                if (ofd.ShowDialog() == DialogResult.OK)
                    path = ofd.FileName;
                else return;

            Bitmap bmp = new Bitmap(path);
            if (bmp.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                MessageBox.Show("Image is not 8BPP indexed.");
                return;
            }
            if (bmp.Width != 512)
            {
                MessageBox.Show($"Chunk 2 is always 512 pixels wide. You are trying to import {bmp.Width}x{bmp.Height}.");
                return;
            }
            byte[] palBuffer = BuildPalette(bmp);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, 512, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            byte[] b = new byte[512 * bmp.Height + 1024];
            Buffer.BlockCopy(palBuffer, 0, b, 0, 1024);
            Marshal.Copy(bmpData.Scan0, b, 1024, 512 * bmp.Height);
            bmp.UnlockBits(bmpData);

            string filePath = st.Where(x => Path.GetFileName(x) == (string)listBox1.SelectedValue).First();
            byte[] bb = File.ReadAllBytes(filePath);
            //if(bb.Length < 512*512+1024+b.Length + 512*24)
            //{
            //    MessageBox.Show("Second chunk is too big!");
            //    return;
            //}

            Buffer.BlockCopy(b, 0, bb, bb.Length-0x80400, b.Length);
            File.WriteAllBytes(filePath, bb);
            RenderImage(filePath);
        }

        private byte[] PaletteToByte(System.Drawing.Color[] pal)
        {
            throw new Exception("NO");
        }

        private static byte[] BuildPalette(Bitmap bmp)
        {
            byte[] palBuffer = new byte[1024];
            if (bmp.Palette.Entries.Length == 256)
                for (int i = 0; i < 256; i++)
                {
                    palBuffer[i * 4 + 0] = bmp.Palette.Entries[i].B;
                    palBuffer[i * 4 + 1] = bmp.Palette.Entries[i].G;
                    palBuffer[i * 4 + 2] = bmp.Palette.Entries[i].R;
                    palBuffer[i * 4 + 3] = (byte)(255 - bmp.Palette.Entries[i].A);
                }
            else
            {
                for (int i = 0; i < 255; i++)
                {
                    palBuffer[i * 4 + 0] = bmp.Palette.Entries[i].B;
                    palBuffer[i * 4 + 1] = bmp.Palette.Entries[i].G;
                    palBuffer[i * 4 + 2] = bmp.Palette.Entries[i].R;
                    palBuffer[i * 4 + 3] = (byte)(255 - bmp.Palette.Entries[i].A);
                }
                palBuffer[1023] = 255;
            }
            return palBuffer;
        }

        private void browseAndMassExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("In next release! Sorry, forgot about it yet I want to release working version right now");
            return;
        }
    }
}
