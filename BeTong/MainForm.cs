using System.Drawing;
using System.Windows.Forms;
using BeTong.Forms;

namespace BeTong
{
    public class MainForm : Form
    {
        public MainForm()
        {
            Text = "BeTong";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 650);
            Size = new Size(1200, 750);

            var title = new Label
            {
                Dock = DockStyle.Top,
                Height = 56,
                Text = "BeTong - Quản lý cấp phối hiệu chỉnh",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Padding = new Padding(16, 0, 0, 0)
            };

            var button = new Button
            {
                Text = "Mở danh sách cấp phối hiệu chỉnh",
                Width = 260,
                Height = 38,
                Anchor = AnchorStyles.None
            };
            button.Click += delegate
            {
                using (var form = new CapPhoiHieuChinhForm())
                {
                    form.ShowDialog(this);
                }
            };

            var host = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1 };
            host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            host.Controls.Add(button, 0, 0);

            Controls.Add(host);
            Controls.Add(title);
        }
    }
}
