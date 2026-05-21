using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BeTong.Data;
using BeTong.Models;
using BeTong.Repositories;
using BeTong.Services;
using BeTong.Helpers;

namespace BeTong.Forms
{
    public class CapPhoiHieuChinhForm : Form
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly CapPhoiHieuChinhRepository _repository;
        private readonly LookupRepository _lookupRepository;
        private readonly PermissionService _permissionService;
        private readonly ApprovalService _approvalService;
        private readonly NotificationService _notificationService;
        private readonly ApplicationUserInfo _user;
        private UiSettings _uiSettings;

        private DataGridView _grid;
        private ComboBox _companyFilter;
        private ComboBox _customerFilter;
        private ComboBox _macFilter;
        private ComboBox _congTrinhFilter;
        private ComboBox _macHdFilter;
        private Label _pageLabel;
        private NumericUpDown _pageSize;
        private int _page = 1;
        private int _totalRows;
        private string _sortColumn = "CreateAt";
        private bool _sortDescending = true;

        private static readonly GridColumnDefinition[] GridColumns =
        {
            new GridColumnDefinition("RowNumber", "No."),
            new GridColumnDefinition("IDCapPhoi", "IDCapPhoi"),
            new GridColumnDefinition("CompanyId", "Chi nhánh"),
            new GridColumnDefinition("IDKhachHang", "Khách hàng"),
            new GridColumnDefinition("Ngay", "Ngày"),
            new GridColumnDefinition("LoaiCapPhoi", "Loại cấp phối"),
            new GridColumnDefinition("IDMac", "Mác"),
            new GridColumnDefinition("LoaiXiMang", "Xi măng"),
            new GridColumnDefinition("LoaiDa", "Đá"),
            new GridColumnDefinition("LoaiCat", "Cát"),
            new GridColumnDefinition("DoSut", "Độ sụt"),
            new GridColumnDefinition("YCDB", "YCĐB"),
            new GridColumnDefinition("LoaiPhuGia", "Phụ gia"),
            new GridColumnDefinition("KLRiengXi", "KL riêng xi"),
            new GridColumnDefinition("KLRiengCat", "KL riêng cát"),
            new GridColumnDefinition("KLRiengDa", "KL riêng đá"),
            new GridColumnDefinition("KLRiengPG", "KL riêng phụ gia"),
            new GridColumnDefinition("KLRiengTroBay", "KL riêng tro bay"),
            new GridColumnDefinition("GhiChu", "Ghi chú"),
            new GridColumnDefinition("DMXM", "ĐM xi"),
            new GridColumnDefinition("DMCat", "ĐM cát"),
            new GridColumnDefinition("DMDa", "ĐM đá"),
            new GridColumnDefinition("DMNuoc", "ĐM nước"),
            new GridColumnDefinition("DMPG", "ĐM phụ gia"),
            new GridColumnDefinition("DMTroBay", "ĐM tro bay"),
            new GridColumnDefinition("TLQDXM", "TLQDXM"),
            new GridColumnDefinition("TLQDCat", "TLQDC"),
            new GridColumnDefinition("TLQDDa", "TLQDD"),
            new GridColumnDefinition("TLQDNuoc", "TLQDN"),
            new GridColumnDefinition("TLQDPG", "TLQDPG"),
            new GridColumnDefinition("TLQDTroBay", "TLQDTroB"),
            new GridColumnDefinition("LuongSoi", "Lượng sỏi"),
            new GridColumnDefinition("DoAmCat", "Độ ẩm cát"),
            new GridColumnDefinition("SHCXM", "SHCXM"),
            new GridColumnDefinition("SHCCat", "SHCC"),
            new GridColumnDefinition("SHCDa", "SHCD"),
            new GridColumnDefinition("SHCNuoc", "SHCN"),
            new GridColumnDefinition("SHCPG", "SHCPG"),
            new GridColumnDefinition("SHCTroBay", "SHCTroB"),
            new GridColumnDefinition("TTXM", "TTXM"),
            new GridColumnDefinition("TTCat", "TTCat"),
            new GridColumnDefinition("TTDa", "TTDa"),
            new GridColumnDefinition("TTNuoc", "TTNuoc"),
            new GridColumnDefinition("TTPG", "TTPG"),
            new GridColumnDefinition("TTTroBay", "TTTroBay"),
            new GridColumnDefinition("CPSXXM", "CPSXXM"),
            new GridColumnDefinition("CPSXCat", "CPSXCat"),
            new GridColumnDefinition("CPSXDa", "CPSXDa"),
            new GridColumnDefinition("CPSXNuoc", "CPSXNuoc"),
            new GridColumnDefinition("CPSXPG", "CPSXPG"),
            new GridColumnDefinition("CPSXTroBay", "CPSXTroBay"),
            new GridColumnDefinition("NguoiTao", "Người tạo"),
            new GridColumnDefinition("NgayTao", "Ngày tạo"),
            new GridColumnDefinition("TrangThai", "Trạng thái"),
            new GridColumnDefinition("IDCongTrinh", "Công trình"),
            new GridColumnDefinition("HangMuc", "Hạng mục"),
            new GridColumnDefinition("TroBay", "Tro bay"),
            new GridColumnDefinition("IDMacHD", "Mác HĐ"),
            new GridColumnDefinition("LoaiXiMangHD", "Xi HĐ"),
            new GridColumnDefinition("LoaiDaHD", "Đá HĐ"),
            new GridColumnDefinition("LoaiCatHD", "Cát HĐ"),
            new GridColumnDefinition("DoSutHD", "Độ sụt HĐ"),
            new GridColumnDefinition("YCDBHD", "YCDB HĐ"),
            new GridColumnDefinition("LoaiNuoc", "Loại nước"),
            new GridColumnDefinition("LoaiTroBay", "Loại tro bay"),
            new GridColumnDefinition("IDDinhMuc", "Định mức"),
            new GridColumnDefinition("IsStatus", "Trạng thái"),
        };

        public CapPhoiHieuChinhForm()
        {
            _connectionFactory = new SqlConnectionFactory();
            _repository = new CapPhoiHieuChinhRepository(_connectionFactory);
            _lookupRepository = new LookupRepository(_connectionFactory);
            _permissionService = new PermissionService(_connectionFactory);
            _approvalService = new ApprovalService(_connectionFactory);
            _notificationService = new NotificationService(_connectionFactory);
            _user = CurrentUserContext.CurrentUser;
            _uiSettings = UiSettings.Load();

            Text = "Danh s\u00e1ch c\u1ea5p ph\u1ed1i hi\u1ec7u ch\u1ec9nh";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(200, 520);
            Font = new Font("Segoe UI", _uiSettings.FontSize, FontStyle.Regular);
            ApplyStartupBounds();
            ApplyApplicationIcon();

            BuildUi();
            LoadLookups();
            LoadData();
        }

        private void ApplyApplicationIcon()
        {
            try
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                // Keep the default form icon if the executable icon cannot be loaded.
            }
        }
        private void BuildUi()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(42)));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(146)));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(78)));

            var menu = BuildMenu();

            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 10,
                RowCount = 2,
                Padding = new Padding(8)
            };
            top.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(54)));
            top.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 10; i++)
            {
                top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
            }

            _companyFilter = AddFilter(top, "Chi nhánh", 0);
            _customerFilter = AddFilter(top, "Khách hàng", 2);
            _macFilter = AddFilter(top, "Mac", 4);
            _congTrinhFilter = AddFilter(top, "Công trình", 6);
            _macHdFilter = AddFilter(top, "Mac HD", 8);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(8, 6, 8, 6)
            };
            AddButton(actions, "Tìm kiếm", Search_Click);
            AddButton(actions, "Làm mới", Refresh_Click);
            AddButton(actions, "Sửa", Edit_Click);
            AddButton(actions, "Lịch sử", History_Click);
            top.SetColumnSpan(actions, 10);
            top.Controls.Add(actions, 0, 1);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
                ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText
            };
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _grid.ColumnHeadersHeight = Scale(48);
            _grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            _grid.RowTemplate.Height = Scale(38);
            _grid.DefaultCellStyle.Font = Font;
            _grid.DefaultCellStyle.Padding = new Padding(2, Scale(5), 2, Scale(5));
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(2, Scale(7), 2, Scale(7));
            _grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ConfigureGridColumns();
            _grid.ColumnHeaderMouseClick += Grid_ColumnHeaderMouseClick;
            _grid.CellDoubleClick += delegate { EditSelected(); };

            var bottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8, 10, 8, 8)
            };
            AddButton(bottom, "Sau", Next_Click);
            AddButton(bottom, "Trước", Prev_Click);
            _pageLabel = new Label { Width = Scale(260), Height = Scale(44), TextAlign = ContentAlignment.MiddleCenter, Font = Font, AutoSize = false };
            bottom.Controls.Add(_pageLabel);
            _pageSize = new NumericUpDown { Minimum = 10, Maximum = 500, Value = 50, Width = Scale(100), Height = Scale(44), Font = Font };
            _pageSize.ValueChanged += delegate { _page = 1; LoadData(); };
            bottom.Controls.Add(_pageSize);
            bottom.Controls.Add(new Label { Text = "S\u1ed1 d\u00f2ng", Width = Scale(100), Height = Scale(44), TextAlign = ContentAlignment.MiddleLeft, Font = Font, AutoSize = false });

            root.Controls.Add(menu, 0, 0);
            root.Controls.Add(top, 0, 1);
            root.Controls.Add(_grid, 0, 2);
            root.Controls.Add(bottom, 0, 3);
            Controls.Add(root);
        }

        private MenuStrip BuildMenu()
        {
            var menu = new MenuStrip
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", Font.Size, FontStyle.Bold),
                AutoSize = false,
                Height = Scale(42),
                BackColor = Color.FromArgb(35, 48, 68),
                ForeColor = Color.White,
                Padding = new Padding(10, 4, 10, 4),
                Renderer = new ToolStripProfessionalRenderer(new TopNavColorTable())
            };

            var settings = CreateTopMenuItem("C\u00e0i \u0111\u1eb7t");
            var uiSettings = CreateDropDownMenuItem("Giao di\u1ec7n...");
            uiSettings.Click += Settings_Click;
            settings.DropDownItems.Add(uiSettings);

            var logout = CreateTopMenuItem("\u0110\u0103ng xu\u1ea5t");
            logout.Alignment = ToolStripItemAlignment.Right;
            logout.Click += Logout_Click;

            var userText = string.IsNullOrEmpty(_user == null ? "" : _user.UserName)
                ? (_user == null ? "" : _user.Email)
                : _user.UserName;
            if (!string.IsNullOrEmpty(userText))
            {
                var userInfo = new ToolStripLabel(userText)
                {
                    Alignment = ToolStripItemAlignment.Right,
                    ForeColor = Color.FromArgb(214, 222, 235),
                    Font = new Font("Segoe UI", Font.Size, FontStyle.Regular),
                    Margin = new Padding(0, 4, 10, 4),
                    Padding = new Padding(8, 0, 8, 0)
                };
                menu.Items.Add(userInfo);
            }

            menu.Items.Add(settings);
            menu.Items.Add(logout);
            MainMenuStrip = menu;
            return menu;
        }

        private ToolStripMenuItem CreateTopMenuItem(string text)
        {
            return new ToolStripMenuItem(text)
            {
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Margin = new Padding(2, 2, 2, 2),
                Padding = new Padding(12, 0, 12, 0),
                AutoSize = true
            };
        }

        private ToolStripMenuItem CreateDropDownMenuItem(string text)
        {
            return new ToolStripMenuItem(text)
            {
                ForeColor = Color.FromArgb(35, 48, 68),
                BackColor = Color.White,
                Padding = new Padding(10, 6, 22, 6)
            };
        }

        private sealed class TopNavColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected
            {
                get { return Color.FromArgb(54, 76, 108); }
            }

            public override Color MenuItemBorder
            {
                get { return Color.FromArgb(79, 108, 148); }
            }

            public override Color MenuItemSelectedGradientBegin
            {
                get { return Color.FromArgb(54, 76, 108); }
            }

            public override Color MenuItemSelectedGradientEnd
            {
                get { return Color.FromArgb(54, 76, 108); }
            }

            public override Color MenuItemPressedGradientBegin
            {
                get { return Color.FromArgb(54, 76, 108); }
            }

            public override Color MenuItemPressedGradientMiddle
            {
                get { return Color.FromArgb(54, 76, 108); }
            }

            public override Color MenuItemPressedGradientEnd
            {
                get { return Color.FromArgb(54, 76, 108); }
            }

            public override Color ToolStripDropDownBackground
            {
                get { return Color.White; }
            }

            public override Color ImageMarginGradientBegin
            {
                get { return Color.White; }
            }

            public override Color ImageMarginGradientMiddle
            {
                get { return Color.White; }
            }

            public override Color ImageMarginGradientEnd
            {
                get { return Color.White; }
            }
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            using (var form = new UiSettingsForm(_uiSettings))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                _uiSettings = new UiSettings
                {
                    FontSize = UiSettings.ClampFontSize(form.SelectedFontSize),
                    StartupWidthPercent = UiSettings.ClampWidthPercent(form.SelectedStartupWidthPercent)
                };
                UiSettings.Save(_uiSettings);

                Font = new Font("Segoe UI", _uiSettings.FontSize, FontStyle.Regular);
                Controls.Clear();
                ApplyStartupBounds();
                BuildUi();
                LoadLookups();
                LoadData();
            }
        }

        private void ApplyStartupBounds()
        {
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            var widthPercent = UiSettings.ClampWidthPercent(_uiSettings == null ? UiSettings.DefaultStartupWidthPercent : _uiSettings.StartupWidthPercent);
            if (widthPercent >= 100)
            {
                WindowState = FormWindowState.Maximized;
                return;
            }

            var width = Math.Max(200, workingArea.Width * widthPercent / 100);
            var height = Math.Max(520, Math.Min(workingArea.Height, workingArea.Height * 90 / 100));
            WindowState = FormWindowState.Normal;
            Size = new Size(width, height);
            Location = new Point(
                workingArea.Left + Math.Max(0, (workingArea.Width - width) / 2),
                workingArea.Top + Math.Max(0, (workingArea.Height - height) / 2));
        }

        private int Scale(int value)
        {
            return Math.Max(value, (int)Math.Ceiling(value * Font.Size / UiSettings.DefaultFontSize));
        }
        private ComboBox AddFilter(TableLayoutPanel top, string label, int column)
        {
            top.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = Font, AutoSize = false, Padding = new Padding(0, 0, 0, 4) }, column, 0);
            var combo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = Font, Height = Scale(42), IntegralHeight = false };
            top.Controls.Add(combo, column + 1, 0);
            return combo;
        }

        private void AddButton(FlowLayoutPanel panel, string text, EventHandler handler)
        {
            var button = new Button { Text = text, Width = Scale(126), Height = Scale(44), Font = Font, Margin = new Padding(4), TextAlign = ContentAlignment.MiddleCenter };
            button.Click += handler;
            panel.Controls.Add(button);
        }

        private void LoadLookups()
        {
            BindFilter(_companyFilter, _lookupRepository.GetChiNhanhs(_user.GroupId, true));
            BindFilter(_customerFilter, _lookupRepository.GetKhachHangs(_user.GroupId, true));
            BindFilter(_macFilter, _lookupRepository.GetLoaiSanPhams(_user.GroupId, true));
            BindFilter(_congTrinhFilter, _lookupRepository.GetHDBanBeTongs(_user.GroupId, true));
            BindFilter(_macHdFilter, _lookupRepository.GetLoaiSanPhams(_user.GroupId, true));
        }

        private static void BindFilter(ComboBox combo, object dataSource)
        {
            combo.DisplayMember = "Text";
            combo.ValueMember = "Value";
            combo.DataSource = dataSource;
        }

        private void ConfigureGridColumns()
        {
            _grid.Columns.Clear();
            foreach (var definition in GridColumns)
            {
                var column = new DataGridViewTextBoxColumn
                {
                    DataPropertyName = definition.DataPropertyName,
                    HeaderText = definition.HeaderText,
                    Name = definition.DataPropertyName,
                    SortMode = DataGridViewColumnSortMode.Programmatic
                };

                _grid.Columns.Add(column);
            }
        }

        private void LoadData()
        {
            try
            {
                var data = _repository.GetPage(
                    _user.GroupId,
                    Value(_companyFilter),
                    Value(_customerFilter),
                    Value(_macFilter),
                    Value(_congTrinhFilter),
                    Value(_macHdFilter),
                    _page,
                    (int)_pageSize.Value,
                    _sortColumn,
                    _sortDescending,
                    out _totalRows);
                _grid.DataSource = data;

                int pageCount = Math.Max(1, (int)Math.Ceiling(_totalRows / (double)_pageSize.Value));
                _pageLabel.Text = "Trang " + _page + "/" + pageCount + " - " + _totalRows + " dòng";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string Value(ComboBox combo)
        {
            return Convert.ToString(combo.SelectedValue ?? "");
        }

        private string SelectedId()
        {
            if (_grid.CurrentRow == null)
            {
                throw new InvalidOperationException("Vui lòng chọn một dòng.");
            }

            var view = _grid.CurrentRow.DataBoundItem as DataRowView;
            if (view == null)
            {
                throw new InvalidOperationException("Dòng dữ liệu không hợp lệ.");
            }

            return Convert.ToString(view["Id"]);
        }

        private void Search_Click(object sender, EventArgs e)
        {
            _page = 1;
            LoadData();
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            _page = 1;
            LoadLookups();
            LoadData();
        }

        private void Logout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            CurrentUserContext.Logout();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            using (var form = NewEditForm(""))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void Edit_Click(object sender, EventArgs e)
        {
            EditSelected();
        }

        private void EditSelected()
        {
            try
            {
                using (var form = NewEditForm(SelectedId()))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private CapPhoiHieuChinhEditForm NewEditForm(string id)
        {
            return new CapPhoiHieuChinhEditForm(_repository, _lookupRepository, _permissionService, _approvalService, _notificationService, _user, id);
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            try
            {
                var id = SelectedId();
                var entity = _repository.GetById(id);
                if (entity.IsActive == 2)
                {
                    MessageBox.Show("Bản ghi đang chờ duyệt xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show("Bạn có chắc muốn yêu cầu xóa bản ghi này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                _permissionService.CheckPermission(_user, ApprovalService.PermissionDelete);
                _repository.CheckExclusive(entity.Id, entity.CreateAt ?? DateTime.Now);
                var first = _approvalService.GetFirstApprovalStep(entity.CompanyId, ApprovalService.PermissionDelete);
                var last = _approvalService.GetLastApprovalStep(entity.CompanyId, ApprovalService.PermissionDelete);
                entity.IsActive = 2;
                entity.CreateAt = DateTime.Now;
                entity.CreateBy = _user.UserId;
                entity.DepartmentId = first.DepartmentId;
                entity.ApprovalId = first.Id;
                entity.LastApprovalId = last.Id;
                entity.ApprovalOrder = 1;
                entity.DepartmentOrder = 1;
                entity.IsStatus = first.Content;
                _repository.RequestDelete(entity);
                _notificationService.InsertApprovalNotifications(entity, first.Id, _user, "Cấp phối hiệu chỉnh - Xóa");
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void History_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new CapPhoiHieuChinhHistoryForm(_repository, SelectedId()))
                {
                    form.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Approve_Click(object sender, EventArgs e)
        {
            try
            {
                var entity = _repository.GetById(SelectedId());
                if (entity.IsActive == 3)
                {
                    MessageBox.Show("Bản ghi đã duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var permissionId = ApprovalService.PermissionForState(entity.IsActive);
                if (permissionId == null || permissionId.Trim().Length == 0)
                {
                    throw new InvalidOperationException("Trạng thái hiện tại không hỗ trợ duyệt.");
                }

                _permissionService.CheckApproval(_user, entity.CompanyId, entity.ApprovalId);
                var oldStatus = entity.IsStatus;
                if (entity.ApprovalId == entity.LastApprovalId)
                {
                    if (entity.IsActive == 0 || entity.IsActive == 1)
                    {
                        entity.IsActive = 3;
                        entity.IsStatus = "Đã duyệt";
                    }
                    else if (entity.IsActive == 2)
                    {
                        entity.IsActive = 100;
                        entity.IsStatus = "Đã duyệt xóa";
                    }
                }
                else
                {
                    var next = _approvalService.GetNextApprovalStep(entity.CompanyId, permissionId, entity.DepartmentId, entity.DepartmentOrder, entity.ApprovalOrder);
                    entity.DepartmentId = next.DepartmentId;
                    entity.ApprovalId = next.Id;
                    entity.ApprovalOrder = next.ApprovalStepNo;
                    entity.DepartmentOrder = next.DepartmentOrder;
                    entity.IsStatus = next.Content;
                    entity.ApprovalUserId = _user.UserId;
                }

                entity.DateApproval = DateTime.Now;
                entity.CreateAt = DateTime.Now;
                _repository.Update(entity);
                MessageBox.Show("Đã duyệt " + oldStatus, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Prev_Click(object sender, EventArgs e)
        {
            if (_page > 1)
            {
                _page--;
                LoadData();
            }
        }

        private void Next_Click(object sender, EventArgs e)
        {
            int pageCount = Math.Max(1, (int)Math.Ceiling(_totalRows / (double)_pageSize.Value));
            if (_page < pageCount)
            {
                _page++;
                LoadData();
            }
        }

        private void Grid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var column = _grid.Columns[e.ColumnIndex];
            if (column == null || string.IsNullOrEmpty(column.DataPropertyName) || column.DataPropertyName.Trim().Length == 0)
            {
                return;
            }

            if (column.DataPropertyName == "RowNumber")
            {
                return;
            }

            if (_sortColumn == column.DataPropertyName)
            {
                _sortDescending = !_sortDescending;
            }
            else
            {
                _sortColumn = column.DataPropertyName;
                _sortDescending = false;
            }

            LoadData();
        }

        private sealed class GridColumnDefinition
        {
            public GridColumnDefinition(string dataPropertyName, string headerText)
            {
                DataPropertyName = dataPropertyName;
                HeaderText = headerText;
            }

            public string DataPropertyName { get; private set; }

            public string HeaderText { get; private set; }
        }
    }
}
