using System;
using System.Windows;
using VkApi.Utility;

namespace VkApi.Forms
{
    /// <summary>
    /// Логика взаимодействия для AddProfileWpf.xaml
    /// </summary>
    public partial class AddProfileWpf
    {
        private string _appId;
        private string _login;
        private string _password;
        private string _profileName;

        private string Login
        {
            get => _login;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _login = value;
                else throw new WrongDataException("Login");
            }
        }
        private string Password
        {
            get => _password;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _password = value;
                else throw new WrongDataException("Password");
            }
        }
        private string AppId
        {
            get => _appId;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _appId = value;
                else throw new WrongDataException("AppId");
            }
        }
        private string ProfileName
        {
            get => _profileName;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _profileName = value;
                else throw new WrongDataException("ProfileName");
            }
        }

        public Profile NewProfile { get; set; }

        public AddProfileWpf()
        {
            InitializeComponent();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Login = TextBoxLogin.Text;
                Password = TextBoxPassword.Text;
                AppId = TextBoxAppID.Text;
                ProfileName = TextBoxNickname.Text;

                NewProfile = new Profile
                {
                    Login = this.Login,
                    Password = this.Password,
                    AppId = Convert.ToInt64(this.AppId),
                    ProfileName = this.ProfileName,

                    TelegramToken = TextBoxTelegramToken.Text != "" ? TextBoxTelegramToken.Text : null,
                    TelegramChatId = TextBoxTelegramChatId.Text != "" ? Convert.ToInt64(TextBoxTelegramChatId.Text) : 0,
                    AntiCaptchaApiKey = TextBoxAntiCaptchaApiKey.Text != "" ? TextBoxAntiCaptchaApiKey.Text : null
                };

                DialogResult = true;
            }
            catch (WrongDataException ex)
            {
                MessageBox.Show($"Некорректный {ex.Message}!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
