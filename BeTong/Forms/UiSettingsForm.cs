using System;
using System.Drawing;
using System.Windows.Forms;
using BeTong.Helpers;

namespace BeTong.Forms
{
    public class UiSettingsForm : Form
    {
        private readonly NumericUpDown _fontSize;
        private readonly NumericUpDown _widthPercent;

        public float SelectedFontSize
        {
            get { return (float)_fontSize.Value; }
        }

        public int SelectedStartupWidthPercent
        {
            get { return (int)_widthPercent.Value; }
        }

        public UiSettingsForm(UiSettings settings)
        {
            Text = "C\u00e0i \u0111\u1eb7t giao di\u1ec7n";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(420, 190);
            Font = new Font("Segoe UI", 11F, FontStyle.Regular);

            var current = settings ?? UiSettings.Load();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(16)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            root.Controls.Add(new Label { Text = "C\u1ee1 ch\u1eef", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            _fontSize = new NumericUpDown
            {
                Minimum = 8,
                Maximum = 24,
                DecimalPlaces = 0,
                Value = (decimal)UiSettings.ClampFontSize(current.FontSize),
                Dock = DockStyle.Fill,
                Font = Font
            };
            root.Controls.Add(_fontSize, 1, 0);

            root.Controls.Add(new Label { Text = "Chi\u1ec1u r\u1ed9ng khi m\u1edf (%)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            _widthPercent = new NumericUpDown
            {
                Minimum = 10,
                Maximum = 100,
                Increment = 5,
                Value = UiSettings.ClampWidthPercent(current.StartupWidthPercent),
                Dock = DockStyle.Fill,
                Font = Font
            };
            root.Controls.Add(_widthPercent, 1, 1);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 12, 0, 0)
            };

            var ok = new Button { Text = "L\u01b0u", Width = 100, Height = 36, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "H\u1ee7y", Width = 100, Height = 36, DialogResult = DialogResult.Cancel };
            buttons.Controls.Add(ok);
            buttons.Controls.Add(cancel);
            root.SetColumnSpan(buttons, 2);
            root.Controls.Add(buttons, 0, 2);

            Controls.Add(root);
            AcceptButton = ok;
            CancelButton = cancel;
        }
    }
}
