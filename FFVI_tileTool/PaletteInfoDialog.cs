using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FFVI_tileTool
{
    public partial class PaletteInfoDialog : Form
    {
        private byte[] paletteBuffer;
        private List<int> offsets;
        private bool darkMode;

        public PaletteInfoDialog(string title, List<int> offsetList, byte[] palette, bool darkMode = false)
        {
            InitializeComponent();
            Form1.ApplyDefaultAppFont(this);
            this.Text = title;
            this.Icon = Form1.GetApplicationIcon();
            this.paletteBuffer = palette ?? new byte[1024];
            this.offsets = offsetList ?? new List<int>();
            this.darkMode = darkMode;

            if (darkMode)
                ApplyDarkMode();

            Form1.ApplyTitleBarThemeToForm(this, darkMode);

            PopulateOffsets();
            DrawPalettePreview();
        }

        private void ApplyDarkMode()
        {
            Color background = Color.FromArgb(30, 30, 30);
            Color surface = Color.FromArgb(45, 45, 48);
            Color foreground = Color.Gainsboro;

            this.BackColor = background;
            this.ForeColor = foreground;

            listBoxOffsets.BackColor = surface;
            listBoxOffsets.ForeColor = foreground;

            panelPalettePreview.BackColor = surface;
        }

        private void PopulateOffsets()
        {
            listBoxOffsets.Items.Clear();
            foreach (int offset in offsets)
            {
                listBoxOffsets.Items.Add($"0x{offset:X6}");
            }
        }

        private void DrawPalettePreview()
        {
            panelPalettePreview.Invalidate();
        }

        private void PanelPalettePreview_Paint(object sender, PaintEventArgs e)
        {
            if (paletteBuffer == null || paletteBuffer.Length < 1024)
                return;

            int cellSize = 16;
            int cellsPerRow = 16;
            int x = 5;
            int y = 5;

            for (int i = 0; i < 256; i++)
            {
                int offset = i * 4;
                if (offset + 3 >= paletteBuffer.Length) continue;

                byte b = paletteBuffer[offset];
                byte g = paletteBuffer[offset + 1];
                byte r = paletteBuffer[offset + 2];
                byte a = (byte)(255 - paletteBuffer[offset + 3]);

                Color color = Color.FromArgb(a, r, g, b);
                using (Brush brush = new SolidBrush(color))
                {
                    e.Graphics.FillRectangle(brush, x, y, cellSize, cellSize);
                }

                e.Graphics.DrawRectangle(Pens.Gray, x, y, cellSize, cellSize);

                x += cellSize;
                if ((i + 1) % cellsPerRow == 0)
                {
                    x = 5;
                    y += cellSize;
                }
            }
        }

        private void InitializeComponent()
        {
            this.listBoxOffsets = new System.Windows.Forms.ListBox();
            this.panelPalettePreview = new System.Windows.Forms.Panel();
            this.SuspendLayout();

            // listBoxOffsets
            this.listBoxOffsets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxOffsets.FormattingEnabled = true;
            this.listBoxOffsets.Location = new System.Drawing.Point(12, 12);
            this.listBoxOffsets.Name = "listBoxOffsets";
            this.listBoxOffsets.Size = new System.Drawing.Size(150, 500);
            this.listBoxOffsets.TabIndex = 0;

            // panelPalettePreview
            this.panelPalettePreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPalettePreview.AutoScroll = true;
            this.panelPalettePreview.BackColor = System.Drawing.Color.White;
            this.panelPalettePreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelPalettePreview.Location = new System.Drawing.Point(170, 12);
            this.panelPalettePreview.Name = "panelPalettePreview";
            this.panelPalettePreview.Size = new System.Drawing.Size(400, 500);
            this.panelPalettePreview.TabIndex = 1;
            this.panelPalettePreview.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelPalettePreview_Paint);

            // PaletteInfoDialog
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 524);
            this.Controls.Add(this.panelPalettePreview);
            this.Controls.Add(this.listBoxOffsets);
            this.Name = "PaletteInfoDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Palette Info";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.ListBox listBoxOffsets;
        private System.Windows.Forms.Panel panelPalettePreview;
    }
}

