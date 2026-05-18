using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace FFVI_tileTool
{
    public class AboutForm : Form
    {
        private const int SectionHorizontalInset = 12;

        private bool isDarkMode;
        private Button infoTabButton;
        private Button licenseTabButton;
        private Panel infoPanel;
        private Panel licensePanel;

        public AboutForm(bool darkMode)
        {
            isDarkMode = darkMode;
            InitializeComponent();
            ApplyTheme();
            Form1.ApplyTitleBarThemeToForm(this, isDarkMode);
        }

        private void InitializeComponent()
        {
            this.Text = "About FFVI Old Tile Tool";
            this.Width = 500;
            this.Height = 500;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Icon = Form1.GetApplicationIcon();

            // Main container with vertical layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                ColumnCount = 1,
                RowCount = 5,
                AutoSize = false
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.Controls.Add(mainPanel);

            // Icon centered
            var iconContainer = new Panel { Height = 50, Dock = DockStyle.Top, AutoSize = false };
            var pictureBox = new PictureBox
            {
                Image = Form1.GetApplicationIcon().ToBitmap(),
                SizeMode = PictureBoxSizeMode.AutoSize,
                Anchor = AnchorStyles.None
            };
            iconContainer.Controls.Add(pictureBox);
            iconContainer.Resize += (s, e) =>
            {
                pictureBox.Left = Math.Max(0, (iconContainer.ClientSize.Width - pictureBox.Width) / 2);
                pictureBox.Top = (iconContainer.ClientSize.Height - pictureBox.Height) / 2;
            };
            mainPanel.Controls.Add(iconContainer, 0, 0);

            // Title
            var titleLabel = new Label
            {
                Text = "FFVI Old Tile Tool",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.TopCenter,
                Height = 30,
                Margin = new Padding(0, 5, 0, 5)
            };
            mainPanel.Controls.Add(titleLabel, 0, 1);

            // Description
            var descLabel = new Label
            {
                Text = "FFVI Old Tile Tool is an application for exploring, exporting and importing tiles from bin files Final Fantasy VI Old Ver",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.TopCenter,
                Height = 60,
                AutoSize = false,
                Margin = new Padding(0, 5, 0, 10)
            };
            mainPanel.Controls.Add(descLabel, 0, 2);

            // Section switcher + content host
            var sectionContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 0, 5)
            };
            mainPanel.Controls.Add(sectionContainer, 0, 3);

            var sectionHeader = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 32,
                WrapContents = false,
                AutoSize = false,
                Margin = new Padding(0),
                Padding = new Padding(SectionHorizontalInset, 0, SectionHorizontalInset, 0)
            };
            sectionContainer.Controls.Add(sectionHeader);

            infoTabButton = new Button
            {
                Text = "Info",
                Width = 110,
                Height = 28,
                Margin = new Padding(0),
                FlatStyle = FlatStyle.Flat,
                TabStop = false
            };
            infoTabButton.FlatAppearance.BorderSize = 1;
            infoTabButton.Click += (s, e) => ShowSection(true);
            sectionHeader.Controls.Add(infoTabButton);

            licenseTabButton = new Button
            {
                Text = "License",
                Width = 110,
                Height = 28,
                Margin = new Padding(2, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                TabStop = false
            };
            licenseTabButton.FlatAppearance.BorderSize = 1;
            licenseTabButton.Click += (s, e) => ShowSection(false);
            sectionHeader.Controls.Add(licenseTabButton);

            var sectionBorder = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(SectionHorizontalInset, 1, SectionHorizontalInset, 1)
            };
            sectionContainer.Controls.Add(sectionBorder);

            var sectionContentHost = new Panel
            {
                Dock = DockStyle.Fill
            };
            sectionBorder.Controls.Add(sectionContentHost);

            infoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                AutoScroll = true,
                BorderStyle = BorderStyle.None
            };
            sectionContentHost.Controls.Add(infoPanel);

            int yPos = 10;
            var versionLabel = new Label
            {
                Text = $"Version: {GetDisplayVersion()}",
                AutoSize = true,
                Location = new Point(0, yPos)
            };
            infoPanel.Controls.Add(versionLabel);

            yPos += 40;
            var authorLabelPart1 = new Label
            {
                Text = "Main Author: ",
                AutoSize = true,
                Location = new Point(0, yPos)
            };
            infoPanel.Controls.Add(authorLabelPart1);

            var authorLink = new LinkLabel
            {
                Text = "MaKiPL",
                Location = new Point(75, yPos),
                AutoSize = true,
                Links = { new LinkLabel.Link(0, 7, "https://github.com/MaKiPL") }
            };
            authorLink.LinkClicked += (s, e) =>
            {
                try { System.Diagnostics.Process.Start(e.Link.LinkData.ToString()); }
                catch { }
            };
            infoPanel.Controls.Add(authorLink);

            yPos += 20;
            var forkedLabel = new Label
            {
                Text = "Forked by: ",
                AutoSize = true,
                Location = new Point(0, yPos)
            };
            infoPanel.Controls.Add(forkedLabel);

            var forkedLink = new LinkLabel
            {
                Text = "Faospark",
                Location = new Point(70, yPos),
                AutoSize = true,
                Links = { new LinkLabel.Link(0, 8, "https://github.com/faospark/") }
            };
            forkedLink.LinkClicked += (s, e) =>
            {
                try { System.Diagnostics.Process.Start(e.Link.LinkData.ToString()); }
                catch { }
            };
            infoPanel.Controls.Add(forkedLink);

            yPos += 40;
            var repoLabel = new Label
            {
                Text = "Project Repository:",
                AutoSize = true,
                Location = new Point(0, yPos)
            };
            infoPanel.Controls.Add(repoLabel);

            var appLink = new LinkLabel
            {
                Text = "https://github.com/faospark/ffvi_tileConverter",
                Location = new Point(0, yPos + 25),
                AutoSize = true,
                Links = { new LinkLabel.Link(0, 48, "https://github.com/faospark/ffvi_tileConverter") }
            };
            appLink.LinkClicked += (s, e) =>
            {
                try { System.Diagnostics.Process.Start(e.Link.LinkData.ToString()); }
                catch { }
            };
            infoPanel.Controls.Add(appLink);

            licensePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                BorderStyle = BorderStyle.None
            };
            sectionContentHost.Controls.Add(licensePanel);

            var licenseTextWrap = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 14, 0, 14),
                BorderStyle = BorderStyle.None
            };
            licensePanel.Controls.Add(licenseTextWrap);

            var licenseText = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Text = GetMITLicense(),
                Font = new Font("Courier New", 9),
                BorderStyle = BorderStyle.None
            };
            licenseTextWrap.Controls.Add(licenseText);

            ShowSection(true);
        }

        private void ShowSection(bool showInfo)
        {
            if (infoPanel == null || licensePanel == null)
                return;

            infoPanel.Visible = showInfo;
            licensePanel.Visible = !showInfo;

            if (showInfo)
                infoPanel.BringToFront();
            else
                licensePanel.BringToFront();

            ApplySectionButtonStyles(showInfo);
        }

        private void ApplySectionButtonStyles(bool infoSelected)
        {
            if (infoTabButton == null || licenseTabButton == null)
                return;

            Color activeBack = isDarkMode ? Color.FromArgb(55, 55, 62) : SystemColors.ControlLightLight;
            Color inactiveBack = isDarkMode ? Color.FromArgb(40, 40, 46) : SystemColors.Control;
            Color fore = isDarkMode ? Color.FromArgb(220, 220, 220) : SystemColors.ControlText;
            Color border = isDarkMode ? Color.FromArgb(70, 70, 78) : SystemColors.ControlDark;

            infoTabButton.BackColor = infoSelected ? activeBack : inactiveBack;
            infoTabButton.ForeColor = fore;
            infoTabButton.FlatAppearance.BorderColor = border;

            licenseTabButton.BackColor = infoSelected ? inactiveBack : activeBack;
            licenseTabButton.ForeColor = fore;
            licenseTabButton.FlatAppearance.BorderColor = border;
        }

        private void ApplyTheme()
        {
            if (isDarkMode)
            {
                this.BackColor = Color.FromArgb(30, 30, 30);
                this.ForeColor = Color.FromArgb(200, 200, 200);

                foreach (Control control in GetAllControls(this))
                {
                    if (control is TableLayoutPanel tableLayout)
                    {
                        tableLayout.BackColor = Color.FromArgb(30, 30, 30);
                        tableLayout.ForeColor = Color.FromArgb(200, 200, 200);
                    }
                    if (control is TextBox tb)
                    {
                        tb.BackColor = Color.FromArgb(45, 45, 48);
                        tb.ForeColor = Color.FromArgb(200, 200, 200);
                    }
                    else if (control is TabControl tabCtrl)
                    {
                        tabCtrl.BackColor = Color.FromArgb(35, 35, 40);
                        tabCtrl.ForeColor = Color.FromArgb(200, 200, 200);
                        foreach (TabPage page in tabCtrl.TabPages)
                        {
                            page.BackColor = Color.FromArgb(35, 35, 40);
                            page.ForeColor = Color.FromArgb(200, 200, 200);
                        }
                    }
                    else if (control is TabPage tabPage)
                    {
                        tabPage.BackColor = Color.FromArgb(35, 35, 40);
                        tabPage.ForeColor = Color.FromArgb(200, 200, 200);
                    }
                    else if (control is Label label)
                    {
                        label.BackColor = Color.Transparent;
                        label.ForeColor = Color.FromArgb(200, 200, 200);
                    }
                    else if (control is LinkLabel linkLabel)
                    {
                        linkLabel.BackColor = Color.Transparent;
                        linkLabel.ForeColor = Color.Cyan;
                        linkLabel.LinkColor = Color.Cyan;
                        linkLabel.VisitedLinkColor = Color.FromArgb(0, 200, 200);
                    }
                    else if (control is Panel panel)
                    {
                        panel.BackColor = Color.FromArgb(30, 30, 30);
                        panel.ForeColor = Color.FromArgb(200, 200, 200);
                    }
                    else if (control is Button button)
                    {
                        button.ForeColor = Color.FromArgb(220, 220, 220);
                    }
                }

                ApplySectionButtonStyles(infoPanel != null && infoPanel.Visible);
            }
        }

        private System.Collections.Generic.IEnumerable<Control> GetAllControls(Control container)
        {
            foreach (Control control in container.Controls)
            {
                yield return control;
                foreach (Control child in GetAllControls(control))
                    yield return child;
            }
        }

        private static string GetDisplayVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyInformationalVersionAttribute info = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (info != null && !string.IsNullOrWhiteSpace(info.InformationalVersion))
                return info.InformationalVersion;

            Version version = assembly.GetName().Version;
            return version != null ? version.ToString() : "unknown";
        }

        private string GetMITLicense()
        {
            return @"MIT License

Copyright (c) 2024 Faospark

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.";
        }

    }
}
