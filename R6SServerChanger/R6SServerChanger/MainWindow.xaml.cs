namespace R6SServerChanger
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Microsoft.VisualBasic;

    using MessageBox = System.Windows.MessageBox;
    using RadioButton = System.Windows.Controls.RadioButton;

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.Loaded += this.MainWindowLoaded;
            this.Closed += this.MainWindowClosed;
        }

        /// <summary>
        /// Gets the item property name.
        /// </summary>
        private string[] ItemPropertyName { get; } = new string[]
        {
            "GameSettingPath",
            "R6SPath",
            "UplayPath"
        };

        private string[] UserDataPropertyName { get; } = new string[]
        {
            "Name",
            "Email",
            "Pass"
        };

        private string[] LastSelectionPropertyName { get; } = new string[]
        {
            "LastSelectedUser",
            "LastSelectedServer"
        };

        /// <summary>
        /// ユーザーデータを格納する　
        /// Key:ユーザー名(8-4-4-4-12) 
        /// Key2:UserDataPropertyName 
        /// Value:値
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> UserData { get; set; }
            = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// メインウィンドウのロード完了時に実行されるメソッド
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            // アプリの設定ファイルを読み込み、以前設定したパスがあるなら読み込む
            var pathData = FileController.ReadApplicationDirectoryInfo(this.ItemPropertyName);

            // game setting path
            if (Directory.Exists(pathData[this.ItemPropertyName[0]]))
            {
                this.SettingFileDirectoryTextBox.Text = pathData[this.ItemPropertyName[0]];
            }
            else
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\My Games\Rainbow Six - Siege";
                if (Directory.Exists(dir))
                {
                    this.SettingFileDirectoryTextBox.Text = dir;
                }
            }

            // R6S path
            if (File.Exists(pathData[this.ItemPropertyName[1]]))
            {
                this.AppFileDirectoryTextBox.Text = pathData[this.ItemPropertyName[1]];
            }
            else
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                if (Directory.Exists(dir))
                {
                    this.AppFileDirectoryTextBox.Text = dir;
                }
            }

            // uplay path
            if (File.Exists(pathData[this.ItemPropertyName[2]]))
            {
                this.UplayAppFileDirectoryTextBox.Text = pathData[this.ItemPropertyName[2]];
            }
            else
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Ubisoft\Ubisoft Game Launcher";
                if (Directory.Exists(dir))
                {
                    this.UplayAppFileDirectoryTextBox.Text = dir;
                    if (File.Exists(dir + @"\Uplay.exe"))
                    {
                        this.UplayAppFileDirectoryTextBox.Text += @"\Uplay.exe";
                    }
                }
            }

            // アプリの設定ファイルを読み込み、以前設定したユーザーデータが存在するなら読み込む
            var settingFileDir = this.SettingFileDirectoryTextBox.Text;
            var userNames = FileController.GetUserNames(settingFileDir);
            foreach (var userName in userNames)
            {
                var data = FileController.ReadUserInfo(userName, this.UserDataPropertyName);
                this.UserData.Add(userName, data);
            }

            this.UpdateUserDataField();
            this.InitializeServerList();

            var ls = FileController.ReadApplicationDirectoryInfo(this.LastSelectionPropertyName);

            foreach (var item in this.UserDataStackPanel.Children)
            {
                if (((RadioButton)item).Tag.ToString() != ls[this.LastSelectionPropertyName[0]])
                {
                    continue;
                }

                ((RadioButton)item).IsChecked = true;
                break;
            }

            foreach (var item in this.ServerListBox.Items)
            {
                if (((TextBlock)item).Tag.ToString() != ls[this.LastSelectionPropertyName[1]])
                {
                    continue;
                }

                this.ServerListBox.SelectedItem = item;
                break;
            }

            this.UpdateUserNavigationText(false);
        }

        /// <summary>
        /// The main window closed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MainWindowClosed(object sender, EventArgs e)
        {
            var items = new Dictionary<string, string>();
            if (!this.CheckIsUserDataSelected() && !this.CheckIsServerListSelected())
            {
                return;
            }

            if (this.CheckIsUserDataSelected())
            {
                items.Add(this.LastSelectionPropertyName[0], this.GetSelectedUserName());
            }

            if (this.CheckIsServerListSelected())
            {
                items.Add(this.LastSelectionPropertyName[1], this.GetSelectedServerName());
            }

            FileController.SaveApplicationDirectoryInfo(items, this.LastSelectionPropertyName, R6SServerChangerTextContainer.LastSelectionItemsName);
        }

        /// <summary>
        /// ユーザー名を記載したスタックパネルの項目が選択されているかどうかをbool値で返す
        /// </summary>
        /// <returns>UserDataStackPanelの項目が選択されているか</returns>
        private bool CheckIsUserDataSelected()
        {
            foreach (RadioButton item in this.UserDataStackPanel.Children)
            {
                if (item.IsChecked.HasValue && (bool)item.IsChecked)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// サーバー名を記載したリストボックスの項目が選択されているかをbool値で返す
        /// </summary>
        /// <returns>ServerListBoxの項目が選択されているか</returns>
        private bool CheckIsServerListSelected()
        {
            return this.ServerListBox.SelectedItem != null;
        }

        /// <summary>
        /// ユーザー名、およびサーバー名を記載したそれぞれのコンポーネントに対し、双方項目が選択されているかをbool値で返す
        /// </summary>
        /// <returns>UserDataStackPanel,ServerListBoxの項目が選択されているか</returns>
        private bool CheckIsUserAndServerListSelected()
        {
            return this.CheckIsUserDataSelected() && this.CheckIsServerListSelected();
        }

        /// <summary>
        /// ユーザー名を記載したスタックパネルからどの項目が選択されているかを取得し、その要素のタグをString型にし、返す
        /// </summary>
        /// <returns>ユーザー名（8-4-4-4-12で構成される英数字）　選択されていない場合、String.Emptyを返す</returns>
        private string GetSelectedUserName()
        {
            foreach (RadioButton item in this.UserDataStackPanel.Children)
            {
                if (item.IsChecked.HasValue && (bool)item.IsChecked)
                {
                    return item.Tag.ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// サーバー名を記載したリストボックスからどの項目が選択されているかを取得し、その要素のタグをString型にし、返す
        /// </summary>
        /// <returns>サーバー名(例:wus,wja) 選択されていない場合、String.Emptyを返す</returns>
        private string GetSelectedServerName()
        {
            return !this.CheckIsServerListSelected() ? string.Empty : ((TextBlock)this.ServerListBox.SelectedItem).Tag.ToString();
        }

        private void UpdateUserNavigationText(bool isServerChanged)
        {
            this.UserNavigateTextBlock.Text = R6SServerChangerTextContainer.GetNavigationText(
                this.CheckIsUserDataSelected(),
                this.CheckIsServerListSelected(),
                isServerChanged);
        }

        /// <summary>
        /// ユーザー名が記載されたスタックパネルの要素を削除、与えられたパス(SettingFileDirectoryTextBox.Text)を元にユーザー名を新規追加する
        /// </summary>
        private void UpdateUserDataField()
        {
            if (this.UserDataStackPanel.Children.Count > 0)
            {
                this.UserDataStackPanel.Children.Clear();
            }

            var dir = this.SettingFileDirectoryTextBox.Text;
            var userNames = FileController.GetUserNames(dir);

            foreach (var item in userNames)
            {
                var rb = new RadioButton
                {
                    Style = this.ToggleStyledRadioButton.Style,
                    Template = this.ToggleStyledRadioButton.Template,
                    Tag = item,

                    // 画像サイズ:200*160
                    Width = this.UserDataStackPanel.ActualWidth / 4,
                    Height = this.UserDataStackPanel.ActualWidth / 5,
                    FontSize = this.UserDataStackPanel.ActualWidth / 40
                };
                if (this.UserData[item][this.UserDataPropertyName[0]] != string.Empty)
                {
                    rb.Content = this.UserData[item][this.UserDataPropertyName[0]];
                }
                else
                {
                    rb.Content = item.Substring(0, 12) + "...";
                }

                rb.Checked += this.RbChecked;
                rb.MouseRightButtonDown += this.RbRightButtonDown;
                this.UserDataStackPanel.Children.Add(rb);
            }
        }

        /// <summary>
        /// ユーザー名を記載したスタックパネル内の項目の大きさをアプリ全体の大きさに応じて変更する
        /// </summary>
        private void UpdateUserDataFieldSize()
        {
            if (this.UserDataStackPanel.Children.Count <= 0)
            {
                return;
            }

            foreach (var item in this.UserDataStackPanel.Children)
            {
                var temp = (RadioButton)item;
                temp.Width = this.UserDataStackPanel.ActualWidth / 4;
                temp.Height = this.UserDataStackPanel.ActualWidth / 5;
            }
        }

        /// <summary>
        /// ユーザー名を記載したスタックパネルの要素であるラジオボタンのどれかがクリックされたらその要素の画像を変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RbChecked(object sender, RoutedEventArgs e)
        {
            var rb = (RadioButton)sender;
            rb.Background = new ImageBrush(new BitmapImage(new Uri(@"./icons/folder2.png", UriKind.Relative)));
            this.UpdateUserNavigationText(false);
        }

        private void RbRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var rb = (RadioButton)sender;
            var userName = rb.Tag.ToString();
            var udiw = new UserDataInputWindow
            {
                Title = "データ管理 -" + rb.Tag.ToString() + "-",
                Tag = userName,
                Owner = this
            };

            this.InitializeUserDataInputWindow(userName, udiw);

            udiw.UserDataSaveButton.Click += this.UserDataSaveButtonClick;
            udiw.Show();
        }

        private void InitializeUserDataInputWindow(string userName, UserDataInputWindow udiw)
        {
            udiw.UserDataNameTextBox.Text = userName.Substring(0, 8);

            if (!this.UserData.ContainsKey(userName))
            {
                return;
            }

            if (this.UserData[userName][this.UserDataPropertyName[0]] != string.Empty)
            {
                udiw.UserDataNameTextBox.Text = this.UserData[userName][this.UserDataPropertyName[0]];
            }

            udiw.UserDataEmailTextBox.Text = this.UserData[userName][this.UserDataPropertyName[1]];
            udiw.UserDataPassTextBox.Password = this.UserData[userName][this.UserDataPropertyName[2]];
        }

        private void UserDataSaveButtonClick(object sender, RoutedEventArgs e)
        {
            var parentWindow = GetWindow((System.Windows.Controls.Button)sender);
            if (parentWindow?.Tag == null)
            {
                return;
            }

            var userName = parentWindow.Tag.ToString();
            var udiw = (UserDataInputWindow)parentWindow;

            var data = udiw.GetUserData(this.UserDataPropertyName);

            // アプリ中に保存
            if (this.UserData.ContainsKey(userName))
            {
                this.UserData[userName] = data;
            }
            else
            {
                this.UserData.Add(userName, data);
            }

            var userItemsName = new List<string>();
            userItemsName.AddRange(this.UserDataPropertyName.Select(item => userName + item + "="));

            // ファイル中に保存
            // これはUserDataInputWindowがFileControllerとかかわっていることになる?
            FileController.SaveUserInfo(
                userName,
                data,
                this.UserDataPropertyName,
                userItemsName.ToArray());

            this.UpdateUserDataField();

            udiw.Close();
        }

        /*
         [ONLINE]
        ;DataCenterHint => 
        ;    default 'ping based'
        ;    eus     'us east'
        ;    cus     'us central'
        ;    scus    'us south central'
        ;    wus     'us west'
        ;    sbr     'brazil south'
        ;    neu     'europe north'
        ;    weu     'europe west'
        ;    eas     'asia east'
        ;    seas    'asia south east'
        ;    eau     'australia east'
        ;    wja     'japan west'
        */

        /// <summary>
        /// サーバー名を記載したリストボックスの要素を初期化
        /// </summary>
        private void InitializeServerList()
        {
            this.ServerListBox.SelectionMode = System.Windows.Controls.SelectionMode.Single;
            this.ServerListBox.SelectionChanged += this.ServerListBoxSelectionChanged;

            var serverList = R6SServerChangerTextContainer.ServerList;

            foreach (var item in serverList)
            {
                var serverData = new TextBlock { Text = item.Key, FontSize = 15, Tag = item.Value };

                this.ServerListBox.Items.Add(serverData);
            }
        }

        /// <summary>
        /// ディレクトリパスのデータを取得し、Dictionary形式で返す
        /// </summary>
        /// <returns>Dictionary形式のディレクトリパスのデータ Key:itemPropertyNameなどの項目名、Value:実際の値</returns>
        private Dictionary<string, string> GetPathData()
        {
            var pathData = new Dictionary<string, string>
            {
                {
                    this.ItemPropertyName[0],
                    this.SettingFileDirectoryTextBox.Text
                },
                {
                    this.ItemPropertyName[1],
                    this.AppFileDirectoryTextBox.Text
                },
                {
                    this.ItemPropertyName[2],
                    this.UplayAppFileDirectoryTextBox.Text
                }
            };

            return pathData;
        }

        /// <summary>
        /// パスを取得し、そのままそのデータを保存
        /// </summary>
        private void GetandSavePathData()
        {
            FileController.SaveApplicationDirectoryInfo(this.GetPathData(), this.ItemPropertyName, R6SServerChangerTextContainer.SettingItemsName);
        }

        private void ServerListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateUserNavigationText(false);
        }

        private void ChangeSettingFileDirectoryButtonClick(object sender, RoutedEventArgs e)
        {
            // string dir = SettingFileDirectoryTextBox.Text.Substring(0, SettingFileDirectoryTextBox.Text.LastIndexOf(@"\"));//現在のフォルダの直上のフォルダの指定
            var fbd = new FolderBrowserDialog
            {
                Description =
                    @"""MyGames""フォルダ直下の""Rainbow Six - Siege""フォルダを選択してください",
                RootFolder = Environment.SpecialFolder.Desktop,
                SelectedPath = this.SettingFileDirectoryTextBox.Text,
                ShowNewFolderButton = true
            };

            if (fbd.ShowDialog().ToString() != "OK")
            {
                return;
            }

            this.SettingFileDirectoryTextBox.Text = fbd.SelectedPath;
            this.UpdateUserNavigationText(false);
            this.UpdateUserDataField();
            this.GetandSavePathData();
        }

        private void ChangeAppFileDirectoryButtonClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                FileName = "RainbowSix.exe",
                InitialDirectory = this.AppFileDirectoryTextBox.Text,
                Filter = @"EXEファイル(*.exe)|*.exe|すべてのファイル(*.*)|*.*",
                FilterIndex = 1,
                Title = @"R6Sのアプリケーションファイルを選択してください",
                RestoreDirectory = true,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (ofd.ShowDialog().ToString() != "OK")
            {
                return;
            }

            this.AppFileDirectoryTextBox.Text = ofd.FileName;
            this.GetandSavePathData();
        }

        private void ChangeUplayAppDirectoryButtonClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                FileName = "Uplay.exe",
                InitialDirectory = this.UplayAppFileDirectoryTextBox.Text,
                Filter = @"EXEファイル(*.exe)|*.exe|すべてのファイル(*.*)|*.*",
                FilterIndex = 1,
                Title = @"Uplay.exeを選択してください",
                RestoreDirectory = true,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (ofd.ShowDialog().ToString() != "OK")
            {
                return;
            }

            this.UplayAppFileDirectoryTextBox.Text = ofd.FileName;
            this.GetandSavePathData();
        }

        private void DirectoryUpdataButtonClick(object sender, RoutedEventArgs e)
        {
            this.UpdateUserNavigationText(false);
            this.UpdateUserDataField();
            this.GetandSavePathData();
        }

        /// <summary>
        /// The app main window size changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void AppMainWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateUserDataFieldSize();
        }

        private void ServerAndR6SStartButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.CheckIsUserAndServerListSelected())
            {
                MessageBox.Show("エラーが発生しました\nユーザー、もしくはサーバーを選択してください");
                return;
            }

            if (FileController.SaveChangedServerData(this.GetPathData()[this.ItemPropertyName[0]], this.GetSelectedUserName(), this.GetSelectedServerName()))
            {
                this.UpdateUserNavigationText(true);
            }

            if (!FileController.OpenApp(this.AppFileDirectoryTextBox.Text))
            {
                MessageBox.Show("R6Sの正しいパスが設定されていません");
                return;
            }

            this.WindowState = WindowState.Minimized;
        }

        private void R6SStartButtonClick(object sender, RoutedEventArgs e)
        {
            if (!FileController.OpenApp(this.AppFileDirectoryTextBox.Text))
            {
                MessageBox.Show("R6Sの正しいパスが設定されていません");
                return;
            }

            this.WindowState = WindowState.Minimized;
        }

        private void ChangeServerButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.CheckIsUserAndServerListSelected())
            {
                MessageBox.Show("エラーが発生しました\nユーザー、もしくはサーバーを選択してください");
                return;
            }

            if (FileController.SaveChangedServerData(this.GetPathData()[this.ItemPropertyName[0]], this.GetSelectedUserName(), this.GetSelectedServerName()))
            {
                this.UpdateUserNavigationText(true);
            }
        }

        /// <summary>
        /// 指定されたアプリケーションファイルにopenというverbがある場合、そのままopenする。
        /// アプリケーションが起動していなかったら起動し、バックグラウンドにあっても最前面に押し出すことが出来る。
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void UplayStartButtonClick(object sender, RoutedEventArgs e)
        {
            if (!FileController.OpenApp(this.UplayAppFileDirectoryTextBox.Text))
            {
                MessageBox.Show("Uplayの正しいパスが設定されていません");
                return;
            }

            this.WindowState = WindowState.Minimized;
        }

        private void UplayUserChangeButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.CheckIsUserDataSelected())
            {
                MessageBox.Show("ユーザーを選択してください");
                return;
            }

            if (!FileController.OpenApp(this.UplayAppFileDirectoryTextBox.Text))
            {
                MessageBox.Show("Uplayの正しいパスが設定されていません");
                return;
            }

            System.Threading.Thread.Sleep(3000);
            this.WindowState = WindowState.Minimized;

            // もう少しなんとかしたい
            if (!UplayController.SendUplayLogoutMessage())
            {
                this.WindowState = WindowState.Normal;
                this.Activate();
                MessageBox.Show("Failed to Logout.\nPlease try again.");
                return;
            }

            if (!UplayController.ActivateUplay())
            {
                this.WindowState = WindowState.Normal;
                this.Activate();
                MessageBox.Show("Failed to Activate.\nPlease try again.");
                return;
            }

            var userName = this.GetSelectedUserName();

            if (!UplayController.SendUserLoginMessage(
                    this.UserData[userName][this.UserDataPropertyName[1]], 
                    this.UserData[userName][this.UserDataPropertyName[2]]))
            {
                this.WindowState = WindowState.Normal;
                this.Activate();
                MessageBox.Show("Failed to Login.\nPlease try again.");
            }
        }
    }
}
