using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BeTong.Models;
using BeTong.Repositories;
using BeTong.Services;
using BeTong.Helpers;

namespace BeTong.Forms
{
    public class CapPhoiHieuChinhEditForm : Form
    {
        private readonly CapPhoiHieuChinhRepository _repository;
        private readonly LookupRepository _lookupRepository;
        private readonly PermissionService _permissionService;
        private readonly ApprovalService _approval_service;
        private readonly NotificationService _notificationService;
        private readonly ApplicationUserInfo _user; // changed to ApplicationUserInfo
        private readonly bool _isEdit;
        private readonly DateTime _baseTime;
        private readonly UiSettings _uiSettings;
        private CapPhoiHieuChinh _entity;

        private readonly Dictionary<string, TextBox> _texts = new Dictionary<string, TextBox>();
        private readonly Dictionary<string, NumericUpDown> _numbers = new Dictionary<string, NumericUpDown>();
        private readonly Dictionary<string, ComboBox> _combos = new Dictionary<string, ComboBox>();
        private readonly Dictionary<string, int> _decimals = new Dictionary<string, int>(); // store requested decimals per field
        private DateTimePicker _ngayPicker;
        private DateTimePicker _ngayTaoPicker;

        public CapPhoiHieuChinhEditForm(
            CapPhoiHieuChinhRepository repository,
            LookupRepository lookupRepository,
            PermissionService permissionService,
            ApprovalService approvalService,
            NotificationService notificationService,
            ApplicationUserInfo user, // accept ApplicationUserInfo instead of CurrentUserContext
            string id)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
            _approval_service = approvalService ?? throw new ArgumentNullException(nameof(approvalService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _isEdit = !(id == null || id.Trim().Length == 0);
            _entity = _isEdit ? _repository.GetById(id) : new CapPhoiHieuChinh();
            _baseTime = DateTime.Now;
            _uiSettings = UiSettings.Load();

            Text = _isEdit ? "Cập nhật cấp phối hiệu chỉnh" : "Thêm cấp phối hiệu chỉnh";
            StartPosition = FormStartPosition.CenterParent;

            // Tăng cỡ chữ toàn cục cho form để dễ đọc (tăng lớn hơn)
            Font = new Font("Segoe UI", _uiSettings.FontSize, FontStyle.Regular);

            // Tăng kích thước form để phù hợp với font lớn hơn
            Size = new Size(Scale(1120), Scale(720));
            MinimumSize = new Size(980, 620);

            BuildUi();
            LoadLookups();
            BindEntityToControls();
        }

        private void BuildUi()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };

            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            // tăng chiều cao footer
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(82)));

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = this.Font
            };

            // "Hiệu chỉnh" tab: 6 trường -> hiển thị 2 dòng x 3 cột
            tabs.TabPages.Add(
                BuildNumbersPage(
                    "Hiệu chỉnh",
                    new[]
                    {
                        "CPSXXM",
                        "CPSXCat",
                        "CPSXDa",
                        "CPSXNuoc",
                        "CPSXPG",
                        "CPSXTroBay"
                    },
                    3));

            // Nếu cần thêm tab "Thông tin chung",
            //tabs.TabPages.Add(BuildGeneralPage());

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8),
                AutoSize = true,
                WrapContents = false
            };

            var save = new Button
            {
                Text = "Lưu",
                Width = Scale(160),
                Height = Scale(52),
                Font = this.Font,
                Margin = new Padding(6)
            };

            save.Click += Save_Click;

            var cancel = new Button
            {
                Text = "Hủy",
                Width = Scale(160),
                Height = Scale(52),
                Font = this.Font,
                Margin = new Padding(6)
            };

            cancel.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            buttons.Controls.Add(save);
            buttons.Controls.Add(cancel);

            root.Controls.Add(tabs, 0, 0);
            root.Controls.Add(buttons, 0, 1);

            Controls.Add(root);
        }

        private TabPage BuildGeneralPage()
        {
            var page = new TabPage("Thông tin chung");
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                Padding = new Padding(12)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(160)));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(160)));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            AddCombo(panel, "CompanyId", "Chi nhánh");
            AddCombo(panel, "IDKhachHang", "Khách hàng");
            AddDate(panel, "Ngay", "Ngày");
            AddText(panel, "LoaiCapPhoi", "Loại cấp phối");
            AddCombo(panel, "IDMac", "Mác");
            AddCombo(panel, "LoaiXiMang", "Xi măng");
            AddCombo(panel, "LoaiDa", "Đá");
            AddCombo(panel, "LoaiCat", "Cát");
            AddCombo(panel, "DoSut", "Độ sụt");
            AddCombo(panel, "YCDB", "YCĐB");
            AddCombo(panel, "LoaiPhuGia", "Phụ gia");
            AddText(panel, "GhiChu", "Ghi chú");
            AddText(panel, "NguoiTao", "Người tạo");
            AddDate(panel, "NgayTao", "Ngày tạo");
            AddNumber(panel, "TrangThai", "Trạng thái", 0);
            AddCombo(panel, "IDCongTrinh", "Công trình");
            AddText(panel, "HangMuc", "Hạng mục");
            AddNumber(panel, "TroBay", "Tro bay", 0);
            AddCombo(panel, "IDMacHD", "Mác HĐ");
            AddCombo(panel, "LoaiXiMangHD", "Xi HĐ");
            AddCombo(panel, "LoaiDaHD", "Đá HĐ");
            AddCombo(panel, "LoaiCatHD", "Cát HĐ");
            AddCombo(panel, "DoSutHD", "Độ sụt HĐ");
            AddCombo(panel, "YCDBHD", "YCDB HĐ");
            AddCombo(panel, "LoaiNuoc", "Loại nước");
            AddCombo(panel, "LoaiTroBay", "Loại tro bay");
            AddText(panel, "IDDinhMuc", "Định mức");

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            scroll.Controls.Add(panel);
            page.Controls.Add(scroll);
            return page;
        }

        private TabPage BuildNumbersPage(string title, string[] fields)
        {
            return BuildNumbersPage(title, fields, 3);
        }

        // Tạo layout 2 hàng x 3 cột, mỗi ô có label ở trên và NumericUpDown bên dưới (font lớn)
        private TabPage BuildNumbersPage(string title, string[] fields, int decimals)
        {
            var page = new TabPage(title);

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(12),
            };

            // 3 cột đều nhau
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

            // Hai hàng, cho mỗi hàng một chiều cao phù hợp
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(112)));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(112)));

            int index = 0;
            for (int r = 0; r < 2; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (index >= fields.Length) break;

                    string fieldName = fields[index];
                    // Tạo panel cho ô (để label + control xếp dọc)
                    var cell = new Panel
                    {
                        Dock = DockStyle.Fill,
                        Padding = new Padding(6)
                    };

                    var lbl = new Label
                    {
                        Text = fieldName,
                        Dock = DockStyle.Top,
                        Height = Scale(34),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Regular)
                    };

                    var number = new NumericUpDown
                    {
                        DecimalPlaces = decimals,
                        Maximum = 1000000000,
                        Minimum = -1000000000,
                        ThousandsSeparator = false,
                        Dock = DockStyle.Top,
                        Height = Scale(52),
                        Width = Scale(260),
                        Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Regular)
                    };

                    // lưu decimals để dùng khi bind/show
                    _decimals[fieldName] = decimals;

                    // đảm bảo value 0 nếu chưa có
                    number.Value = 0;

                    // thêm xử lý tự động ẩn phần thập phân nếu là số nguyên
                    number.Leave += (s, e) => UpdateDecimalPlaces(number, decimals);
                    // gọi một lần để khởi tạo hiển thị đúng
                    UpdateDecimalPlaces(number, decimals);

                    // thêm vào dictionary để bind/save
                    _numbers[fieldName] = number;

                    // thêm controls vào cell: label trên, numeric dưới
                    cell.Controls.Add(number);
                    cell.Controls.Add(lbl);

                    grid.Controls.Add(cell, c, r);

                    index++;
                }
            }

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            scroll.Controls.Add(grid);
            page.Controls.Add(scroll);
            return page;
        }

        private void AddText(TableLayoutPanel panel, string name, string label)
        {
            AddLabel(panel, label);
            var text = new TextBox { Dock = DockStyle.Fill, Width = Scale(320), Font = this.Font };
            _texts[name] = text;
            panel.Controls.Add(text);
        }

        private void AddCombo(TableLayoutPanel panel, string name, string label)
        {
            AddLabel(panel, label);
            var combo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Width = Scale(320), Font = this.Font };
            _combos[name] = combo;
            panel.Controls.Add(combo);
        }

        private void AddDate(TableLayoutPanel panel, string name, string label)
        {
            AddLabel(panel, label);
            var picker = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Font = this.Font };
            if (name == "Ngay") _ngayPicker = picker;
            if (name == "NgayTao") _ngayTaoPicker = picker;
            panel.Controls.Add(picker);
        }
        private void AddNumber(TableLayoutPanel panel, string name, string label, int decimals)
        {
            AddLabel(panel, label);
            var number = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Maximum = 1000000000,
                Minimum = -1000000000,
                DecimalPlaces = decimals,
                ThousandsSeparator = false,
                Width = Scale(320),
                Font = this.Font
            };

            // lưu decimals để dùng khi bind/show
            _decimals[name] = decimals;

            // tự động cập nhật hiển thị phần thập phân
            number.Leave += (s, e) => UpdateDecimalPlaces(number, decimals);
            UpdateDecimalPlaces(number, decimals);

            _numbers[name] = number;
            panel.Controls.Add(number);
        }

        private void UpdateDecimalPlaces(NumericUpDown ctrl, int maxDecimals)
        {
            if (ctrl == null) return;
            try
            {
                decimal val = ctrl.Value;
                decimal trunc = Math.Truncate(val);
                if (val == trunc)
                {
                    // nếu giá trị là số nguyên, hiển thị không có phần thập phân
                    if (ctrl.DecimalPlaces != 0) ctrl.DecimalPlaces = 0;
                }
                else
                {
                    // nếu có phần thập phân, thử tìm số chữ số thập phân tối thiểu cần hiển thị (<= maxDecimals)
                    decimal frac = Math.Abs(val - trunc);
                    int needed = 0;
                    decimal scaled = frac;
                    for (int i = 1; i <= maxDecimals; i++)
                    {
                        scaled = Math.Round(frac * (decimal)Math.Pow(10, i));
                        if (scaled == Math.Truncate(scaled))
                        {
                            needed = i;
                            break;
                        }
                    }
                    if (needed == 0) needed = maxDecimals;
                    if (ctrl.DecimalPlaces != needed) ctrl.DecimalPlaces = needed;
                }
            }
            catch
            {
                // không để lỗi làm crash UI
            }
        }

        private int Scale(int value)
        {
            return Math.Max(value, (int)Math.Ceiling(value * Font.Size / UiSettings.DefaultFontSize));
        }

        private static void AddLabel(TableLayoutPanel panel, string text)
        {
            panel.Controls.Add(new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true,
                Padding = new Padding(0, 5, 4, 5)
                // Font is inherited from parent form, set on form constructor
            });
        }

        private void LoadLookups()
        {
            BindCombo("CompanyId", _lookupRepository.GetChiNhanhs(_user.GroupId, false));
            BindCombo("IDKhachHang", _lookupRepository.GetKhachHangs(_user.GroupId, false));
            BindCombo("IDMac", _lookupRepository.GetLoaiSanPhams(_user.GroupId, false));
            BindCombo("LoaiXiMang", _lookupRepository.GetLoaiXiMangs(_user.GroupId, false));
            BindCombo("LoaiDa", _lookupRepository.GetCotLieuThos(_user.GroupId, false));
            BindCombo("LoaiCat", _lookupRepository.GetCotLieuMins(_user.GroupId, false));
            BindCombo("DoSut", _lookupRepository.GetDoSuts(_user.GroupId, false));
            BindCombo("YCDB", _lookupRepository.GetYeuCauDacBiets(_user.GroupId, false));
            BindCombo("LoaiPhuGia", _lookupRepository.GetPhuGias(_user.GroupId, false));
            BindCombo("IDCongTrinh", _lookupRepository.GetHDBanBeTongs(_user.GroupId, false));
            BindCombo("IDMacHD", _lookupRepository.GetLoaiSanPhams(_user.GroupId, false));
            BindCombo("LoaiXiMangHD", _lookupRepository.GetLoaiXiMangs(_user.GroupId, false));
            BindCombo("LoaiDaHD", _lookupRepository.GetCotLieuThos(_user.GroupId, false));
            BindCombo("LoaiCatHD", _lookupRepository.GetCotLieuMins(_user.GroupId, false));
            BindCombo("DoSutHD", _lookupRepository.GetDoSuts(_user.GroupId, false));
            BindCombo("YCDBHD", _lookupRepository.GetYeuCauDacBiets(_user.GroupId, false));
            BindCombo("LoaiNuoc", _lookupRepository.GetLoaiNuocs(_user.GroupId, false));
            BindCombo("LoaiTroBay", _lookupRepository.GetLoaiTroBays(_user.GroupId, false));
        }
        private void BindCombo(string name, List<LookupItem> items)
        {
            if (!_combos.ContainsKey(name))
            {
                return;
            }

            var combo = _combos[name];
            combo.DisplayMember = "Text";
            combo.ValueMember = "Value";
            combo.DataSource = items;
        }

        private void BindEntityToControls()
        {
            SetCombo("CompanyId", _entity.CompanyId);
            SetCombo("IDKhachHang", _entity.IDKhachHang);
            if (_ngayPicker != null)
            {
                _ngayPicker.Value = _entity.Ngay ?? DateTime.Now;
            }
            SetText("LoaiCapPhoi", _entity.LoaiCapPhoi);
            SetCombo("IDMac", _entity.IDMac);
            SetCombo("LoaiXiMang", _entity.LoaiXiMang.ToString());
            SetCombo("LoaiDa", _entity.LoaiDa.ToString());
            SetCombo("LoaiCat", _entity.LoaiCat.ToString());
            SetCombo("DoSut", _entity.DoSut.ToString());
            SetCombo("YCDB", _entity.YCDB.ToString());
            SetCombo("LoaiPhuGia", _entity.LoaiPhuGia.ToString());
            SetText("GhiChu", _entity.GhiChu);
            SetText("NguoiTao", _entity.NguoiTao);
            if (_ngayTaoPicker != null)
            {
                _ngayTaoPicker.Value = _entity.NgayTao ?? DateTime.Now;
            }
            SetNumber("TrangThai", _entity.TrangThai);
            SetCombo("IDCongTrinh", _entity.IDCongTrinh);
            SetText("HangMuc", _entity.HangMuc);
            SetNumber("TroBay", _entity.TroBay);
            SetCombo("IDMacHD", _entity.IDMacHD);
            SetCombo("LoaiXiMangHD", _entity.LoaiXiMangHD.ToString());
            SetCombo("LoaiDaHD", _entity.LoaiDaHD.ToString());
            SetCombo("LoaiCatHD", _entity.LoaiCatHD.ToString());
            SetCombo("DoSutHD", _entity.DoSutHD.ToString());
            SetCombo("YCDBHD", _entity.YCDBHD.ToString());
            SetCombo("LoaiNuoc", _entity.LoaiNuoc.ToString());
            SetCombo("LoaiTroBay", _entity.LoaiTroBay.ToString());
            SetText("IDDinhMuc", _entity.IDDinhMuc);

            SetNumber("KLRiengXi", _entity.KLRiengXi);
            SetNumber("KLRiengCat", _entity.KLRiengCat);
            SetNumber("KLRiengDa", _entity.KLRiengDa);
            SetNumber("KLRiengPG", _entity.KLRiengPG);
            SetNumber("KLRiengTroBay", _entity.KLRiengTroBay);
            foreach (var pair in _numbers)
            {
                if (pair.Key != "TrangThai" && pair.Key != "TroBay")
                {
                    SetNumber(pair.Key, GetDouble(_entity, pair.Key));
                }
            }
        }
        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyControlsToEntity();

                if (_isEdit)
                {
                    _repository.CheckExclusive(_entity.Id, _baseTime);
                    _repository.UpdateCpsxValues(_entity);
                }
                else
                {
                    ValidateRequired();
                    _permissionService.EnsureUserHasApprovalPermission(_entity.CompanyId, ApprovalService.PermissionAdd);
                    _repository.CheckDuplicate(_entity, false);

                    var first = _approval_service.GetFirstApprovalStep(_entity.CompanyId, ApprovalService.PermissionAdd);
                    var last = _approval_service.GetLastApprovalStep(_entity.CompanyId, ApprovalService.PermissionAdd);
                    _entity.Id = Guid.NewGuid().ToString();
                    _entity.IDCapPhoi = _entity.Id;
                    _entity.IsActive = 0;
                    _entity.Ordinarily = 0;
                    ApplyApproval(_entity, first, last);
                    _repository.Insert(_entity);
                    _notificationService.InsertApprovalNotifications(_entity, first.Id, _user, "Cấp phối hiệu chỉnh - Thêm");
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyControlsToEntity()
        {
            foreach (var pair in _numbers)
            {
                SetDouble(_entity, pair.Key, Convert.ToDouble(pair.Value.Value));
            }

            _entity.GroupId = _user.GroupId;
            _entity.CreateBy = _user.UserId;
            _entity.CreateAt = DateTime.Now;
            if (TextHelper.IsNullOrWhiteSpace(_entity.ApprovalUserId))
            {
                _entity.ApprovalUserId = "";
            }
        }

        private static void ApplyApproval(CapPhoiHieuChinh entity, ApprovalStep first, ApprovalStep last)
        {
            entity.DepartmentId = first.DepartmentId;
            entity.ApprovalId = first.Id;
            entity.LastApprovalId = last.Id;
            entity.ApprovalOrder = 1;
            entity.DepartmentOrder = 1;
            entity.IsStatus = first.Content;
        }

        private void ValidateRequired()
        {
            Require(_entity.CompanyId, "Chi nhánh");
            Require(_entity.IDKhachHang, "Khách hàng");
            Require(_entity.LoaiCapPhoi, "Loại cấp phối");
            Require(_entity.IDMac, "Mác");
            Require(_entity.IDCongTrinh, "Công trình");
            Require(_entity.IDMacHD, "Mác HĐ");
            Require(_entity.IDDinhMuc, "Định mức");
            if (_entity.LoaiXiMang == 0 || _entity.LoaiDa == 0 || _entity.LoaiCat == 0 || _entity.DoSut == 0 || _entity.YCDB == 0 || _entity.LoaiPhuGia == 0)
            {
                throw new InvalidOperationException("Vui lòng chọn đầy đủ vật liệu/thông số cấp phối.");
            }
        }

        private static void Require(string value, string label)
        {
            if (TextHelper.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException("Vui lòng nhập/chọn " + label + ".");
            }
        }

        private string GetCombo(string name)
        {
            return Convert.ToString(_combos[name].SelectedValue ?? "");
        }

        private int GetComboInt(string name)
        {
            int value;
            return int.TryParse(GetCombo(name), out value) ? value : 0;
        }

        private void SetCombo(string name, string value)
        {
            if (_combos.ContainsKey(name))
            {
                _combos[name].SelectedValue = value ?? "";
            }
        }

        private void SetText(string name, string value)
        {
            if (_texts.ContainsKey(name))
            {
                _texts[name].Text = value ?? "";
            }
        }

        private void SetNumber(string name, double value)
        {
            if (_numbers.ContainsKey(name))
            {
                var control = _numbers[name];
                if ((decimal)value > control.Maximum) value = (double)control.Maximum;
                if ((decimal)value < control.Minimum) value = (double)control.Minimum;
                control.Value = (decimal)value;

                // sau khi gán giá trị, cập nhật số chữ số thập phân hiển thị dựa trên giá trị
                int maxDecimals = _decimals.ContainsKey(name) ? _decimals[name] : control.DecimalPlaces;
                UpdateDecimalPlaces(control, maxDecimals);
            }
        }

        private static double GetDouble(CapPhoiHieuChinh entity, string name)
        {
            var prop = typeof(CapPhoiHieuChinh).GetProperty(name);
            if (prop == null) return 0;
            return Convert.ToDouble(prop.GetValue(entity, null));
        }

        private static void SetDouble(CapPhoiHieuChinh entity, string name, double value)
        {
            var prop = typeof(CapPhoiHieuChinh).GetProperty(name);
            if (prop != null && prop.PropertyType == typeof(double))
            {
                prop.SetValue(entity, value, null);
            }
        }
    }
}
