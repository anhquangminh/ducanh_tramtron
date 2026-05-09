using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BeTong.Repositories;

namespace BeTong.Forms
{
    public class CapPhoiHieuChinhHistoryForm : Form
    {
        public CapPhoiHieuChinhHistoryForm(CapPhoiHieuChinhRepository repository, string id)
        {
            Text = "Lịch sử cấp phối hiệu chỉnh";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1200, 700);

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                DataSource = repository.GetHistory(id)
            };

            Controls.Add(grid);
        }
    }
}
