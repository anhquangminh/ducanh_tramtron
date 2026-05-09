using System;
using System.Drawing;
using System.Windows.Forms;
using BeTong.Models;
using BeTong.Repositories;
using BeTong.Helpers;

namespace BeTong.Forms
{
    public class LoginForm : Form
    {
        private readonly UserRepository _userRepository;
        private TextBox _loginNameTextBox;
        private TextBox _passwordTextBox;
        private CheckBox _rememberMeCheckBox;

        public ApplicationUserInfo LoggedInUser { get; private set; }

        public LoginForm(UserRepository userRepository)
        {
            _userRepository = userRepository;
            LoggedInUser = null;

            Text = string.Empty;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 320);
            BackColor = Color.FromArgb(245, 247, 250);

            BuildUi();
            LoadSavedLogin(); // Gọi hàm tải thông tin đăng nhập đã lưu
        }

        private void BuildUi()
        {
            // Main container
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(0),
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // Title
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Input fields
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));   // Spacer
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // Buttons
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));   // Remember Me

            // Title
            var titleLabel = new Label
            {
                Text = "ĐĂNG NHẬP HỆ THỐNG",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 78, 121),
                Padding = new Padding(0, 10, 0, 0)
            };
            mainPanel.Controls.Add(titleLabel, 0, 0);

            // Input fields panel
            var inputPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(40, 30, 40, 30),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            inputPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            inputPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            // Username
            var userLabel = new Label
            {
                Text = "Tên đăng nhập/Email",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 80, 95)
            };
            _loginNameTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 5, 0, 5)
            };
            _loginNameTextBox.KeyDown += LoginNameTextBox_KeyDown;
            inputPanel.Controls.Add(userLabel, 0, 0);
            inputPanel.Controls.Add(_loginNameTextBox, 1, 0);

            // Password
            var passLabel = new Label
            {
                Text = "Mật khẩu",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 80, 95)
            };
            _passwordTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true,
                Margin = new Padding(0, 5, 0, 5)
            };
            _passwordTextBox.KeyDown += LoginNameTextBox_KeyDown;
            inputPanel.Controls.Add(passLabel, 0, 1);
            inputPanel.Controls.Add(_passwordTextBox, 1, 1);

            mainPanel.Controls.Add(inputPanel, 0, 1);

            // Spacer
            mainPanel.Controls.Add(new Panel { Height = 20, Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 247, 250) }, 0, 2);

            // Buttons panel
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 40, 0),
                BackColor = Color.White,
                Height = 50,
                AutoSize = true
            };
            var loginButton = new Button
            {
                Text = "Đăng nhập",
                Width = 120,
                Height = 36,
                BackColor = Color.FromArgb(31, 78, 121),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(10, 0, 0, 0)
            };
            loginButton.FlatAppearance.BorderSize = 0;
            loginButton.Click += LoginButton_Click;

            var cancelButton = new Button
            {
                Text = "Hủy",
                Width = 100,
                Height = 36,
                BackColor = Color.FromArgb(235, 238, 242),
                ForeColor = Color.FromArgb(40, 45, 52),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(10, 0, 0, 0)
            };
            cancelButton.FlatAppearance.BorderColor = Color.FromArgb(210, 215, 222);
            cancelButton.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };

            buttonPanel.Controls.Add(loginButton);
            buttonPanel.Controls.Add(cancelButton);

            mainPanel.Controls.Add(buttonPanel, 0, 3);

            // Remember Me panel
            var rememberMePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(40, 0, 40, 0)
            };
            _rememberMeCheckBox = new CheckBox
            {
                Text = "Nhớ mật khẩu",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(40, 45, 52),
                AutoSize = true,
                BackColor = Color.White
            };
            rememberMePanel.Controls.Add(_rememberMeCheckBox);

            mainPanel.Controls.Add(rememberMePanel, 0, 4);

            Controls.Clear();
            Controls.Add(mainPanel);

            AcceptButton = loginButton;
            CancelButton = cancelButton;
        }

        private void LoginNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Login();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            Login();
        }

        private void Login()
        {
            try
            {
                // Lấy thông tin người dùng từ UserRepository
                LoggedInUser = _userRepository.GetByLoginName(_loginNameTextBox.Text, _passwordTextBox.Text);

                // Kiểm tra nếu GroupId không tồn tại
                if (string.IsNullOrWhiteSpace(LoggedInUser.GroupId))
                {
                    throw new InvalidOperationException("Người dùng này chưa được gán GroupId.");
                }

                // Lưu thông tin người dùng vào CurrentUserContext
                CurrentUserContext.Save(LoggedInUser);

                // Lưu thông tin đăng nhập nếu "Nhớ mật khẩu" được chọn
                if (_rememberMeCheckBox.Checked)
                {
                    var s = new LoginSettings
                    {
                        LoginName = _loginNameTextBox.Text ?? string.Empty,
                        Password = _passwordTextBox.Text ?? string.Empty,
                        RememberMe = true
                    };
                    LoginSettings.Save(s);
                }
                else
                {
                    LoginSettings.Clear();
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Đăng nhập không thành công", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _passwordTextBox.Focus();
                _passwordTextBox.SelectAll();
            }
        }

        private void LoadSavedLogin()
        {
            var s = LoginSettings.Load();
            if (s != null && s.RememberMe)
            {
                _loginNameTextBox.Text = s.LoginName;
                _passwordTextBox.Text = s.Password;
                _rememberMeCheckBox.Checked = true;
            }
            else
            {
                _rememberMeCheckBox.Checked = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadSavedLogin();
        }
    }
}
