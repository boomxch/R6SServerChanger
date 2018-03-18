using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace R6SServerChanger
{
    /// <summary>
    /// UserDataInputWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class UserDataInputWindow : Window
    {
        public UserDataInputWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// ユーザーデータを取得し、Dictionary形式で返す
        /// </summary>
        /// <param name="userDataPropertyName">
        /// The user Data Property Name.
        /// </param>
        /// <returns>
        /// Dictionary形式のディレクトリパスのデータ Key:userDataPropertyNameなどの項目名、Value:実際の値
        /// </returns>
        public Dictionary<string, string> GetUserData(string[] userDataPropertyName)
        {
            var userData = new Dictionary<string, string>
            {
                {
                    userDataPropertyName[0],
                    this.UserDataNameTextBox.Text
                },
                {
                    userDataPropertyName[1],
                    this.UserDataEmailTextBox.Text
                },
                {
                    userDataPropertyName[2],
                    this.UserDataPassTextBox.Password
                }
            };

            return userData;
        }

        private void UserDataNameInitializeButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.Tag != null)
            {
                this.UserDataNameTextBox.Text = this.Tag.ToString().Substring(0, 8);
            }
        }

        private void UserDataAllInitializeButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.Tag != null)
            {
                this.UserDataNameTextBox.Text = this.Tag.ToString().Substring(0, 8);
            }

            this.UserDataEmailTextBox.Text = string.Empty;
            this.UserDataPassTextBox.Password = string.Empty;
        }

        private void UserDataCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
