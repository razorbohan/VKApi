using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using VkApi.Logic;
using VkApi.Logic.UserAction;
using VkApi.Utility;
using ComboBox = System.Windows.Controls.ComboBox;

namespace VkApi.Forms
{
    /// <summary>
    /// Interaction logic for MainFormWpf.xaml
    /// </summary>
    public partial class MainFormWpf
    {
        #region Data

        private VKApiEntities Context { get; }
        public Profile CurrentProfile { get; set; }
        public MainLogic MainLogic { get; set; }

        public CancellationTokenSource CancellationSource { get; set; }
        public XmlDocument XmlSettingsDocument { get; set; }

        public NotifyIcon NotifyIcon { get; set; }

        public MainFormWpf()
        {
            InitializeComponent();

            NotifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = new Icon("VK.ico")
            };

            Logger.ControlLog = RichTextBoxLog;

            XmlSettingsDocument = new XmlDocument();
            Context = new VKApiEntities();
        }

        #endregion

        #region Handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => ReadProfiles());
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentProfile.ProfileName))
            {
                Logger.Log("Выберите пользователя!");
                return;
            }

            if (MainLogic.Pause)
            {
                MainLogic.Pause = false;

                Logger.Log("Продолжено");
                return;
            }

            Logger.FilePath = $"Logs_{CurrentProfile.ProfileName}.txt";
            Logger.Log("Старт");
            NotifyIcon.Text = $"VK_{CurrentProfile.ProfileName}";
            Title = $"VK_{CurrentProfile.ProfileName}";
            
            //TODO deserialize from XML to object
            CurrentProfile.IsSortByRegDate = CheckBoxIsSortByRegdate.IsChecked == true;
            CurrentProfile.IsUpdate = CheckBoxIsUpdateData.IsChecked == true;
            CurrentProfile.IsCheckFriends = CheckBoxIsCheckFriends.IsChecked == true;
            CurrentProfile.IsUseAntiCaptcha = CheckBoxIsAntiCaptcha.IsChecked == true;
            CurrentProfile.IsOnlyAction = CheckBoxIsOnlyAction.IsChecked == true;
            CurrentProfile.IsFilter = CheckBoxIsFilter.IsChecked == true;

            BaseAction.Profile = CurrentProfile;

            //var groupAction = new GroupAction( /*CurrentProfile*/);
            //if (checkBox_IsLetPost.Checked && CurrentProfile.ProfileName == "Fake")
            //groupAction[GroupActionType.Post] = new List<object[]> { new object[] { "Fake_posting_test", "Fake_post" } };
            CancellationSource = new CancellationTokenSource();

            MainLogic = new MainLogic(CurrentProfile, CancellationSource.Token, GetUserActionList() /*, groupAction*/);
            MainLogic.OnNotification += ChangeNotification;
            MainLogic.OnTaskCompleted += () =>
                Dispatcher.Invoke(() =>
                {
                    GridSettings.IsEnabled = true;
                    ButtonStart.IsEnabled = true;
                    ButtonStop.IsEnabled = false;
                });

            this.DataContext = MainLogic;

            Task.Run(() => MainLogic.DoMainWork());

            GridSettings.IsEnabled = false;
            ButtonStart.IsEnabled = false;
            ButtonStop.IsEnabled = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                CancellationSource?.Cancel();

                if (CurrentProfile.ProfileName != null)
                    XmlSettingsWrite(CurrentProfile.ProfileName);
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CancellationSource?.Cancel();
                Logger.Log("Завершение операций...");
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainLogic.Pause = true;
                ButtonStart.IsEnabled = true;
                Logger.Log("Пауза");
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }
        }

        private void ButtonAddProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var form = new AddProfileWpf();
                form.ShowDialog();
                if (!form.DialogResult.HasValue || !form.DialogResult.Value) return;

                Context.Profile.Add(form.NewProfile);
                Context.SaveChanges();

                ComboBoxProfiles.Items.Add(form.NewProfile);

                var newProfileNode = (XmlElement)XmlSettingsDocument.DocumentElement?.SelectNodes("Profile")?.Item(0)?.Clone();
                if (newProfileNode == null) return;

                newProfileNode.SetAttribute("Name", form.NewProfile.ProfileName);
                XmlSettingsDocument.DocumentElement.InsertAfter(newProfileNode, XmlSettingsDocument.DocumentElement.LastChild);
                XmlSettingsDocument.Save("settings.xml");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка во время созданя профиля!\r\n{ex.Message}", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Profiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                CurrentProfile = (Profile)((ComboBox)sender).SelectedItem;
                XmlSettingsRead(CurrentProfile.ProfileName);
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RichTextBoxLog.ScrollToEnd();
        }

        #endregion

        #region Methods

        public void ReadProfiles()
        {
            try
            {
                var profiles = Context.Profile.ToList();

                Dispatcher.Invoke(() =>
                {
                    ComboBoxProfiles.ItemsSource = profiles;
                });

                if (!File.Exists("settings.xml"))
                    XmlSettingsCreate();
                XmlSettingsDocument.Load("settings.xml");
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
                Dispatcher.Invoke(() =>
                {
                    ButtonStart.IsEnabled = false;
                });
            }
        }

        public void XmlSettingsCreate()
        {
            try
            {
                var settings =
                    new XElement("Profile",
                        new XAttribute("Name", "Default"),
                        new XElement("Add",
                            new XElement("IsAct", true),
                            new XElement("Count", 50)
                        ),
                        new XElement("Like",
                            new XElement("IsAct", true),
                            new XElement("Count", 500)
                        ),
                        new XElement("Write",
                            new XElement("IsAct", false),
                            new XElement("Count", 20)
                        ),
                        new XElement("IsLetPost", false),
                        new XElement("IsSortByRegdate", true),
                        new XElement("IsOnlyAction", true),
                        new XElement("IsFilter", false),
                        new XElement("IsAntiCaptcha", true),
                        new XElement("IsCheckFriends", false),
                        new XElement("IsUpdateData", false)
                    );

                using (var xmlWriter = new XmlTextWriter(File.Create("settings.xml"), null)
                {
                    Formatting = Formatting.Indented,
                    IndentChar = '\t',
                    Indentation = 1,
                    QuoteChar = '\''
                })
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("Settings");

                    settings.WriteTo(xmlWriter);

                    xmlWriter.WriteEndElement();
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }
        }

        public void XmlSettingsRead(string profileName)
        {
            try
            {
                var root = XmlSettingsDocument.DocumentElement;
                var profileNode = root?.SelectSingleNode($"Profile[@Name='{profileName}']");

                CheckBoxIsLetAdd.IsChecked = Convert.ToBoolean(profileNode?.SelectSingleNode("Add/IsAct")?.InnerText);
                TextBoxAddLimitCount.Text = profileNode?.SelectSingleNode("Add/Count")?.InnerText ?? "0";
                CheckBoxIsLetLike.IsChecked = Convert.ToBoolean(profileNode?.SelectSingleNode("Like/IsAct")?.InnerText);
                TextBoxLikeLimitCount.Text = profileNode?.SelectSingleNode("Like/Count")?.InnerText ?? "0";
                CheckBoxIsLetWrite.IsChecked = Convert.ToBoolean(profileNode?.SelectSingleNode("Write/IsAct")?.InnerText);
                TextBoxWriteLimitCount.Text = profileNode?.SelectSingleNode("Write/Count")?.InnerText ?? "0";

                CheckBoxIsLetPost.IsChecked = Convert.ToBoolean(profileNode?.SelectSingleNode("IsLetPost")?.InnerText);
                CheckBoxIsSortByRegdate.IsChecked = Convert.ToBoolean(profileNode?.SelectSingleNode("IsSortByRegdate")?.InnerText);
                CheckBoxIsOnlyAction.IsChecked = Convert.ToBoolean(profileNode?.SelectSingleNode("IsOnlyAction")?.InnerText);
                CheckBoxIsFilter.IsChecked = Convert.ToBoolean(profileNode?.SelectSingleNode("IsFilter")?.InnerText);
                CheckBoxIsAntiCaptcha.IsChecked = Convert.ToBoolean(profileNode?.SelectSingleNode("IsAntiCaptcha")?.InnerText);
                CheckBoxIsCheckFriends.IsChecked = Convert.ToBoolean(profileNode?.SelectSingleNode("IsCheckFriends")?.InnerText);
                CheckBoxIsUpdateData.IsChecked = Convert.ToBoolean(profileNode?.SelectSingleNode("IsUpdateData")?.InnerText);
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }
        }

        public void XmlSettingsWrite(string profileName)
        {
            try
            {
                var root = XmlSettingsDocument.DocumentElement;
                var profileNode = root?.SelectSingleNode($"Profile[@Name='{profileName}']");
                if (profileNode == null) return;

                var isAddNode = profileNode.SelectSingleNode("Add/IsAct");
                if (isAddNode != null)
                    isAddNode.InnerText = CheckBoxIsLetAdd.IsChecked.ToString();

                var addCountNode = profileNode.SelectSingleNode("Add/Count");
                if (addCountNode != null)
                    addCountNode.InnerText = TextBoxAddLimitCount.Text;

                var isLikeNode = profileNode.SelectSingleNode("Like/IsAct");
                if (isLikeNode != null)
                    isLikeNode.InnerText = CheckBoxIsLetLike.IsChecked.ToString();

                var likeCountNode = profileNode.SelectSingleNode("Like/Count");
                if (likeCountNode != null)
                    likeCountNode.InnerText = TextBoxLikeLimitCount.Text;

                var isWriteNode = profileNode.SelectSingleNode("Write/IsAct");
                if (isWriteNode != null)
                    isWriteNode.InnerText = CheckBoxIsLetWrite.IsChecked.ToString();

                var writeCountNode = profileNode.SelectSingleNode("Write/Count");
                if (writeCountNode != null)
                    writeCountNode.InnerText = TextBoxWriteLimitCount.Text;

                var isLetPostNode = profileNode.SelectSingleNode("IsLetPost");
                if (isLetPostNode != null)
                    isLetPostNode.InnerText = CheckBoxIsLetPost.IsChecked.ToString();

                var isSortByRegDateNode = profileNode.SelectSingleNode("IsSortByRegdate");
                if (isSortByRegDateNode != null)
                    isSortByRegDateNode.InnerText = CheckBoxIsSortByRegdate.IsChecked.ToString();

                var isOnlyActionNode = profileNode.SelectSingleNode("IsOnlyAction");
                if (isOnlyActionNode != null)
                    isOnlyActionNode.InnerText = CheckBoxIsOnlyAction.IsChecked.ToString();

                var isFilterNode = profileNode.SelectSingleNode("IsFilter");
                if (isFilterNode != null)
                    isFilterNode.InnerText = CheckBoxIsFilter.IsChecked.ToString();

                var isAntiCaptchaNode = profileNode.SelectSingleNode("IsAntiCaptcha");
                if (isAntiCaptchaNode != null)
                    isAntiCaptchaNode.InnerText = CheckBoxIsAntiCaptcha.IsChecked.ToString();

                var isCheckFriendsNode = profileNode.SelectSingleNode("IsCheckFriends");
                if (isCheckFriendsNode != null)
                    isCheckFriendsNode.InnerText = CheckBoxIsCheckFriends.IsChecked.ToString();

                var isUpdateDataNode = profileNode.SelectSingleNode("IsUpdateData");
                if (isUpdateDataNode != null)
                    isUpdateDataNode.InnerText = CheckBoxIsUpdateData.IsChecked.ToString();

                XmlSettingsDocument.Save("settings.xml");
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }
        }

        private List<BaseUserAction> GetUserActionList()
        {
            var userActionList = new List<BaseUserAction>();

            if (CheckBoxIsLetAdd.IsChecked == true)
                userActionList.Add(new UserActionAdd { ActionLimitCount = Convert.ToInt32(TextBoxAddLimitCount.Text)});
            if (CheckBoxIsLetLike.IsChecked == true)
                userActionList.Add(new UserActionLike { ActionLimitCount = Convert.ToInt32(TextBoxLikeLimitCount.Text)});
            if (CheckBoxIsLetWrite.IsChecked == true)
                userActionList.Add(new UserActionWrite { ActionLimitCount = Convert.ToInt32(TextBoxWriteLimitCount.Text)});

            return userActionList;
        }

        private void ChangeNotification(string message)
        {
            try
            {
                Dispatcher.Invoke(() => { NotifyIcon.Text = message; });
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }
        }

        #endregion
    }
}