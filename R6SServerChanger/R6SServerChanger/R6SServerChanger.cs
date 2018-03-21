namespace R6SServerChanger
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    using MessageBox = System.Windows.MessageBox;

    /// <summary>
    /// using Win32Api
    /// </summary>
    public static class Win32Controller
    {
        public const int WmChar = 0x0102;
        public const int WmKeyDown = 0x0100;
        public const int WmKeyUp = 0x0101;
        public const int WmLButtonDown = 0x201;
        public const int WmLButtonUp = 0x202;
        public const int MkLBotton = 0x0001;
        public const int GwlStyle = -16;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        [DllImport("user32.dll")]
        public static extern int PostMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hWnd, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static uint MakeLParam(ushort x, ushort y)
        {
            return y * (uint)Math.Pow(16, 4) + x;
        }

        public static uint MakeWParam(ushort x, ushort y)
        {
            return y * (uint)Math.Pow(16, 4) + x;
        }
    }

    public static class R6SServerChangerTextContainer
    {
        /// <summary>
        /// Gets dataのデフォルトの項目
        /// </summary>
        public static string[] SettingItemsName { get; } = new string[]
        {
            "//setting path",
            "GameSettingPath=",
            "//R6S path",
            "R6SPath=",
            "//uplay path",
            "UplayPath=",
            "//user data"
            // "8-4-4-4-12Name="
            // "8-4-4-4-12Email="
            // "8-4-4-4-12Pass="
        };

        /// <summary>
        /// Gets the last selection items name.
        /// </summary>
        public static string[] LastSelectionItemsName { get; } = new string[]
        {
            "//last selected user and server"
        };

        public static Dictionary<string, string> ServerList { get; } = new Dictionary<string, string>()
        {
            {
                "ping based",
                "default"
            },
            {
                "us east",
                "eus"
            },
            {
                "us central",
                "cus"
            },
            {
                "us south central",
                "scus"
            },
            {
                "us west",
                "wus"
            },
            {
                "brazil south",
                "sbr"
            },
            {
                "europe north",
                "neu"
            },
            {
                "europe west",
                "weu"
            },
            {
                "asia east",
                "eas"
            },
            {
                "asia south east",
                "seas"
            },
            {
                "australia east",
                "eau"
            },
            {
                "japan west",
                "wja"
            }
        };

        private static string[] NavigationTexts { get; } = new string[]
        {
            "ユーザーを選択してください", "サーバーを選択してください",
            "実行したい内容をクリックしてください", "正常にサーバーを変更しました"
        };

        public static string GetNavigationText(bool isUserSelected, bool isServerSelected, bool isServerChanged)
        {
            if (isServerChanged)
            {
                return NavigationTexts[3];
            }

            if (isUserSelected && isServerSelected)
            {
                return NavigationTexts[2];
            }

            return isUserSelected ? NavigationTexts[1] : NavigationTexts[0];
        }
    }

    public static class FileController
    {
        private const string FolderPath = @"R6SServerChanger";
        private const string FilePath = @"R6SServerChanger\data";

        /// <summary>
        /// Open app.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <returns>
        /// 正常に終了:true 失敗:false
        /// </returns>
        public static bool OpenApp(string filePath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = filePath
            };

            if (!psi.Verbs.Contains("open"))
            {
                return false;
            }

            var p = Process.Start(psi);
            return true;
        }

        public static string[] GetUserNames(string dir)
        {
            // 設定ファイルが入っているフォルダ名は8-4-4-4-12で構成されている（数字には英数字が入る）
            var userNames = new List<string>();
            var subFolders = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
            foreach (var t in subFolders)
            {
                var temp = Path.GetFileName(t);
                if (temp == null)
                {
                    return null;
                }

                if (System.Text.RegularExpressions.Regex.IsMatch(
                    temp,
                    @"^[0-9a-zA-Z]{8}-[0-9a-zA-Z]{4}-[0-9a-zA-Z]{4}-[0-9a-zA-Z]{4}-[0-9a-zA-Z]{12}$"))
                {
                    userNames.Add(temp);
                }
            }

            return userNames.ToArray();
        }

        /*
        Comment: // (一行コメント)

        //setting path
        GameSettingPath=
        //R6S path
        R6SPath=
        //uplay path
        UplayPath=
        //user name
        8-4-4-4-12=
        */

        /// <summary>
        /// このアプリの設定ファイルを保存する　保存する内容はアプリ下部にて参照しているディレクトリパス
        /// </summary>
        /// <param name="pathData">
        /// 保存するディレクトリパスの文字列(itemPropertyNameをKeyにして中身を取得できるDictionary型)
        /// </param>
        /// <param name="itemPropertyName">
        /// The item Property Name.
        /// </param>
        /// <param name="settingItemsName">
        /// The setting Items Name.
        /// </param>
        public static void SaveApplicationDirectoryInfo(
            IReadOnlyDictionary<string, string> pathData,
            string[] itemPropertyName,
            string[] settingItemsName)
        {
            // ファイルが存在しない場合新規作成
            if (!File.Exists(FilePath))
            {
                try
                {
                    // アプリの設定ファイルを保存するフォルダーが存在しなければ新規作成
                    if (!Directory.Exists(FolderPath))
                    {
                        Directory.CreateDirectory(FolderPath);
                    }

                    // ファイルを新規作成
                    using (var hstream = File.Create(FilePath))
                    {
                        hstream.Close();
                    }

                    // ファイルに項目を記述
                    using (var sr = new StreamWriter(FilePath, false, Encoding.GetEncoding("shift_jis")))
                    {
                        foreach (var item in settingItemsName)
                        {
                            sr.WriteLine(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("エラーが発生しました\n設定ファイルが正常に保存できませんでした");
                    MessageBox.Show(e.ToString());
                }
            }

            // 設定ファイルを開き、対象となる項目を探し、書き換える
            using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var sr = new StreamReader(fs);
                var sw = new StreamWriter(fs);

                var isExitsts = new bool[itemPropertyName.Length];
                var txtData = new List<string>();

                while (sr.Peek() > -1)
                {
                    var s = sr.ReadLine();

                    foreach (var item in pathData)
                    {
                        if (s == null || !System.Text.RegularExpressions.Regex.IsMatch(s, "^" + item.Key + "=.*$"))
                        {
                            continue;
                        }

                        if (Array.IndexOf(itemPropertyName, item.Key) > -1)
                        {
                            isExitsts[Array.IndexOf(itemPropertyName, item.Key)] = true;
                        }

                        s = item.Key + "=" + item.Value;

                        break;
                    }

                    txtData.Add(s);
                }

                // 存在しない場合、項目別に新規作成(順不同)
                for (var i = isExitsts.Length - 1; i >= 0; i--)
                {
                    if (!isExitsts[i])
                    {
                        txtData.Insert(0, itemPropertyName[i] + "=" + pathData[itemPropertyName[i]]);
                    }
                }

                fs.Position = 0;
                fs.SetLength(0);

                foreach (var t in txtData)
                {
                    sw.WriteLine(t);
                }

                sw.Flush();

                sr.Close();
                if (sw.BaseStream.CanWrite)
                {
                    sw.Close();
                }
            }
        }

        /// <summary>
        /// 設定ファイルからパスを読み取る
        /// </summary>
        /// <param name="itemPropertyName">
        /// The item Property Name.
        /// </param>
        /// <returns>
        /// パスをitemPropertyNameをKeyとしたDictionary形式で返す
        /// </returns>
        public static Dictionary<string, string> ReadApplicationDirectoryInfo(string[] itemPropertyName)
        {
            var data = itemPropertyName.ToDictionary(item => item, item => string.Empty);

            // ファイルが存在しない場合、全ての項目をString.Emptyで返す
            if (!File.Exists(FilePath))
            {
                return data;
            }

            // 項目をそれぞれ探し、それに対応したValueを設定する
            using (var sr = new StreamReader(FilePath, Encoding.GetEncoding("shift_jis")))
            {
                while (sr.Peek() > -1)
                {
                    var s = sr.ReadLine();
                    if (s == null)
                    {
                        continue;
                    }

                    if (s.IndexOf("//", StringComparison.Ordinal) >= 0)
                    {
                        s = s.Substring(0, s.IndexOf("//", StringComparison.Ordinal));
                    }

                    foreach (var t in itemPropertyName)
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(s, @"^" + t + @"=.+$"))
                        {
                            continue;
                        }

                        data[t] = s.Substring((t + "=").Length);
                        break;
                    }
                }
            }

            return data;
        }

        public static void SaveUserInfo(
            string userName,
            IReadOnlyDictionary<string, string> userData,
            string[] userDataPropertyName,
            string[] userItemsName)
        {
            // ファイルが存在しない場合新規作成
            if (!File.Exists(FilePath))
            {
                try
                {
                    // アプリの設定ファイルを保存するフォルダーが存在しなければ新規作成
                    if (!Directory.Exists(FolderPath))
                    {
                        Directory.CreateDirectory(FolderPath);
                    }

                    // ファイルを新規作成
                    using (var hstream = File.Create(FilePath))
                    {
                        hstream.Close();
                    }

                    // ファイルに項目を記述
                    using (var sr = new StreamWriter(FilePath, false, Encoding.GetEncoding("shift_jis")))
                    {
                        foreach (var item in userItemsName)
                        {
                            sr.WriteLine(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("エラーが発生しました\n設定ファイルが正常に保存できませんでした");
                    MessageBox.Show(e.ToString());
                }
            }

            // 設定ファイルを開き、対象となる項目を探し、書き換える
            using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var sr = new StreamReader(fs);
                var sw = new StreamWriter(fs);

                var isExitsts = new bool[userDataPropertyName.Length];
                var txtData = new List<string>();

                while (sr.Peek() > -1)
                {
                    var s = sr.ReadLine();

                    foreach (var item in userData)
                    {
                        if (s == null || !System.Text.RegularExpressions.Regex.IsMatch(s, "^" + userName + item.Key + "=.*$"))
                        {
                            continue;
                        }

                        if (Array.IndexOf(userDataPropertyName, item.Key) > -1)
                        {
                            isExitsts[Array.IndexOf(userDataPropertyName, item.Key)] = true;
                        }

                        s = userName + item.Key + "=" + item.Value;

                        break;
                    }

                    txtData.Add(s);
                }

                // 存在しない場合、項目別に新規作成(順不同)
                for (var i = isExitsts.Length - 1; i >= 0; i--)
                {
                    if (!isExitsts[i])
                    {
                        txtData.Add(userName + userDataPropertyName[i] + "=" + userData[userDataPropertyName[i]]);
                    }
                }

                fs.Position = 0;
                fs.SetLength(0);

                foreach (var t in txtData)
                {
                    sw.WriteLine(t);
                }

                sw.Flush();

                sr.Close();
                if (sw.BaseStream.CanWrite)
                {
                    sw.Close();
                }
            }
        }

        public static Dictionary<string, string> ReadUserInfo(string userName, string[] userDataPropertyName)
        {
            var data = userDataPropertyName.ToDictionary(item => item, item => string.Empty);

            // ファイルが存在しない場合、全ての項目をString.Emptyで返す
            if (!File.Exists(FilePath))
            {
                return data;
            }

            // 項目をそれぞれ探し、それに対応したValueを設定する
            using (var sr = new StreamReader(FilePath, Encoding.GetEncoding("shift_jis")))
            {
                while (sr.Peek() > -1)
                {
                    var s = sr.ReadLine();
                    if (s == null)
                    {
                        continue;
                    }

                    if (s.IndexOf("//", StringComparison.Ordinal) >= 0)
                    {
                        s = s.Substring(0, s.IndexOf("//", StringComparison.Ordinal));
                    }

                    foreach (var t in userDataPropertyName)
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(s, @"^" + userName + t + @"=.+$"))
                        {
                            continue;
                        }

                        data[t] = s.Substring((userName + t + "=").Length);
                        break;
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// GameSetting.iniを編集し、サーバーを変更する
        /// </summary>
        /// <param name="folderPath">フォルダ"Rainbow Six - Siege"のパス</param>
        /// <param name="userName">選択したユーザー名(8-4-4-4-12)</param>
        /// <param name="serverName">変更先のサーバー名</param>
        /// <returns>bool型 エラー:false 成功:true</returns>
        public static bool SaveChangedServerData(string folderPath, string userName, string serverName)
        {
            var filePath = folderPath + @"\" + userName + @"\GameSettings.ini";

            if (!File.Exists(filePath))
            {
                MessageBox.Show("エラーが発生しました\nGameSettings.iniが存在しません");

                return false;
            }

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var sr = new StreamReader(fs);
                var sw = new StreamWriter(fs);

                var txtData = new List<string>();
                var isExist = false;

                while (sr.Peek() > -1)
                {
                    var s = sr.ReadLine();

                    if (s == null)
                    {
                        continue;
                    }

                    if (System.Text.RegularExpressions.Regex.IsMatch(s, "^DataCenterHint.*$"))
                    {
                        s = "DataCenterHint=" + serverName;
                        isExist = true;
                    }

                    txtData.Add(s);
                }

                // 存在しない場合、エラーメッセージを出力
                if (!isExist)
                {
                    MessageBox.Show("エラーが発生しました\nGameSettings.iniにDataCenterHintの項目が存在しません");
                }

                fs.Position = 0;
                fs.SetLength(0);

                foreach (var t in txtData)
                {
                    sw.WriteLine(t);
                }

                sw.Flush();

                sr.Close();
                if (sw.BaseStream.CanWrite)
                {
                    sw.Close();
                }
            }

            return true;
        }
    }

    public static class UplayController
    {
        /// <summary>
        /// The uplay's process name.
        /// </summary>
        private const string UplayProcessName = "upc";
        private static string[] UplayState { get; } = new string[]
        {
            "uplay_start",
            "uplay_main"
        };

        private static IntPtr GetUplayProcessHandle()
        {
            /*var uplayProcesses = Process.GetProcessesByName(UplayProcessName);
            if (uplayProcesses.Length <= 0)
            {
                // MessageBox.Show("Failed\nPlease try again");
                return IntPtr.Zero;
            }*/

            for (var i = 0; i < 10000; i++)
            {
                System.Threading.Thread.Sleep(1);
                var uplayProcesses = Process.GetProcessesByName(UplayProcessName);
                if (uplayProcesses.Length <= 0)
                {
                    continue;
                }

                var uplayWindowHandle = uplayProcesses[0].MainWindowHandle;

                if (uplayWindowHandle != IntPtr.Zero)
                {
                    // MessageBox.Show("Accepted " + uplayWindowHandle.ToString() + " " + i);
                    return uplayWindowHandle;
                }
            }

            // MessageBox.Show("Failed\nTime out");
            return IntPtr.Zero;
        }

        public static bool ActivateUplay()
        {
            var psh = GetUplayProcessHandle();
            if (psh == IntPtr.Zero)
            {
                return false;
            }

            return !Win32Controller.IsIconic(psh) || Win32Controller.ShowWindow(psh, 1);
        }

        /// <summary>
        /// Uplayにログアウトを行うための操作を行う
        /// </summary>
        /// <returns>正常にUplayを操作出来たら返り値trueを返す 失敗の場合はfalse</returns>
        public static bool SendUplayLogoutMessage()
        {
            var psh = GetUplayProcessHandle();

            if (psh == IntPtr.Zero)
            {
                return false;
            }

            if (GetWindow(psh).ClassName == UplayState[0])
            {
                return true;
            }

            // 既にメニューを開いているとここでログアウト
            if (!PostMlbClickToProcess(psh, 300, 8, 333))
            {
                return false;
            }

            // 補助でもう一回
            PostMlbClickToProcess(psh, 300, 8, 333);
            
            System.Threading.Thread.Sleep(3000);
            psh = GetUplayProcessHandle();

            if (psh == IntPtr.Zero)
            {
                return false;
            }

            if (GetWindow(psh).ClassName == UplayState[0])
            {
                return true;
            }

            // メニューを開く
            if (!PostMlbClickToProcess(psh, 200, 50, 62))
            {
                return false;
            }

            // ログアウト
            if (!PostMlbClickToProcess(psh, 300, 8, 333))
            {
                return false;
            }

            // 補助でもう一回
            PostMlbClickToProcess(psh, 300, 8, 333);

            System.Threading.Thread.Sleep(3000);
            psh = GetUplayProcessHandle();
            return psh == IntPtr.Zero || GetWindow(psh).ClassName == UplayState[0];
        }

        public static bool SendUserLoginMessage(string email, string pass)
        {
            var psh = GetUplayProcessHandle();

            if (psh == IntPtr.Zero || GetWindow(psh).ClassName != UplayState[0])
            {
                return false;
            }

            System.Threading.Thread.Sleep(3000);

            // email欄をアクティベート
            if (!PostMlbClickToProcess(psh, 200, 330, 155))
            {
                return false;
            }

            // email入力
            if (!PostKeyCharToProcess(psh, email, 5))
            {
                return false;
            }

            // Tab入力で次の項目へ
            if (!PostTabKeyInputToProcess(psh, 5))
            {
                return false;
            }

            // pass入力
            if (!PostKeyCharToProcess(psh, pass, 5))
            {
                return false;
            }

            // Tab入力で次の項目へ
            if (!PostTabKeyInputToProcess(psh, 5))
            {
                return false;
            }

            // Spaceキーで保存にチェック
            if (!PostSpaceKeyInputToProcess(psh, 5))
            {
                return false;
            }

            // Enterキーで入力終了
            if (!PostEnterKeyInputToProcess(psh, 5))
            {
                return false;
            }

            System.Threading.Thread.Sleep(10000);
            psh = GetUplayProcessHandle();
            return psh != IntPtr.Zero && GetWindow(psh).ClassName != UplayState[0];
        }

        /// <summary>
        /// 与えられたプロセスに対し、約waitTime[ms]後にクライアント座標(x,y)の位置にマウスクリックイベントを起こす
        /// </summary>
        /// <param name="processHandle">
        /// The process Handle.
        /// </param>
        /// <param name="waitTime">
        /// The wait Time.
        /// </param>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool PostMlbClickToProcess(IntPtr processHandle, int waitTime, ushort x, ushort y)
        {
            // wait
            System.Threading.Thread.Sleep(waitTime);

            // マウスクリックダウン
            if (Win32Controller.PostMessage(
                    processHandle,
                    Win32Controller.WmLButtonDown,
                    Win32Controller.MkLBotton,
                    Win32Controller.MakeLParam(x, y)) == 0)
            {
                return false;
            }

            System.Threading.Thread.Sleep(waitTime);

            // マウスクリックアップ
            return Win32Controller.PostMessage(
                       processHandle,
                       Win32Controller.WmLButtonUp,
                       0x00000000,
                       Win32Controller.MakeLParam(x, y)) != 0;
        }

        private static bool PostKeyCharToProcess(IntPtr processHandle, string keys, int intervalTime)
        {
            foreach (var item in keys)
            {
                System.Threading.Thread.Sleep(intervalTime);

                // 0x20:Space
                if (Regex.IsMatch(item.ToString(), @"^ $"))
                {
                    if (Win32Controller.PostMessage(processHandle, Win32Controller.WmChar, 0x00000020, 0x001E0001) == 0)
                    {
                        return false;
                    }

                    continue;
                }

                // 0x2E:Dot
                if (Regex.IsMatch(item.ToString(), @"^\.$"))
                {
                    if (Win32Controller.PostMessage(processHandle, Win32Controller.WmChar, 0x0000002E, 0x001E0001) == 0)
                    {
                        return false;
                    }

                    continue;
                }

                // 0x40:@
                if (Regex.IsMatch(item.ToString(), @"^@$"))
                {
                    if (Win32Controller.PostMessage(processHandle, Win32Controller.WmChar, 0x00000040, 0x001E0001) == 0)
                    {
                        return false;
                    }

                    continue;
                }

                // 0x5F:_
                if (Regex.IsMatch(item.ToString(), @"^_$"))
                {
                    if (Win32Controller.PostMessage(processHandle, Win32Controller.WmChar, 0x0000005F, 0x00730001) == 0)
                    {
                        return false;
                    }

                    continue;
                }

                // 0x2D:-
                if (Regex.IsMatch(item.ToString(), @"^-$"))
                {
                    if (Win32Controller.PostMessage(processHandle, Win32Controller.WmChar, 0x0000002D, 0x000C0001) == 0)
                    {
                        return false;
                    }

                    continue;
                }

                // 0x21:!
                if (Regex.IsMatch(item.ToString(), @"^!$"))
                {
                    if (Win32Controller.PostMessage(processHandle, Win32Controller.WmChar, 0x00000021, 0x00020001) == 0)
                    {
                        return false;
                    }

                    continue;
                }

                // 0x3F:?
                if (Regex.IsMatch(item.ToString(), @"^\?$"))
                {
                    if (Win32Controller.PostMessage(processHandle, Win32Controller.WmChar, 0x0000003F, 0x00350001) == 0)
                    {
                        return false;
                    }

                    continue;
                }

                // 0x30:0
                if (Regex.IsMatch(item.ToString(), @"^[0-9]$"))
                {
                    uint wparam = 0x00000030 + (uint)(item - '0');
                    if (Win32Controller.PostMessage(processHandle, Win32Controller.WmChar, wparam, 0x001E0001) == 0)
                    {
                        return false;
                    }

                    continue;
                }

                // 0x41:A
                if (Regex.IsMatch(item.ToString(), @"^[A-Z]$"))
                {
                    uint wparam = 0x00000041 + (uint)(item - 'A');
                    if (Win32Controller.PostMessage(processHandle, Win32Controller.WmChar, wparam, 0x001E0001) == 0)
                    {
                        return false;
                    }

                    continue;
                }

                // 0x61:a
                if (Regex.IsMatch(item.ToString(), @"^[a-z]$"))
                {
                    uint wparam = 0x00000061 + (uint)(item - 'a');
                    if (Win32Controller.PostMessage(processHandle, Win32Controller.WmChar, wparam, 0x001E0001) == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool PostTabKeyCharToProcess(IntPtr processHandle, int waitTime)
        {
            System.Threading.Thread.Sleep(waitTime);

            // 0x9:Tab
            return Win32Controller.PostMessage(processHandle, Win32Controller.WmChar, 0x00000009, 0x000F0001) != 0;
        }

        private static bool PostEnterKeyCharToProcess(IntPtr processHandle, int waitTime)
        {
            System.Threading.Thread.Sleep(waitTime);

            // 0xD:Enter
            return Win32Controller.PostMessage(processHandle, Win32Controller.WmChar, 0x0000000D, 0x001C0001) != 0;
        }

        private static bool PostTabKeyInputToProcess(IntPtr processHandle, int waitTime)
        {
            System.Threading.Thread.Sleep(waitTime);

            // 0x9:Tab
            if (Win32Controller.PostMessage(processHandle, Win32Controller.WmKeyDown, 0x00000009, 0x000F0001) != 0)
            {
                System.Threading.Thread.Sleep(waitTime);
                return Win32Controller.PostMessage(processHandle, Win32Controller.WmKeyUp, 0x00000009, 0xC00F0001) != 0;
            }

            return false;
        }

        private static bool PostSpaceKeyInputToProcess(IntPtr processHandle, int waitTime)
        {
            System.Threading.Thread.Sleep(waitTime);

            // 0x20:Space
            if (Win32Controller.PostMessage(processHandle, Win32Controller.WmKeyDown, 0x00000020, 0x00390001) != 0)
            {
                System.Threading.Thread.Sleep(waitTime);
                return Win32Controller.PostMessage(processHandle, Win32Controller.WmKeyUp, 0x00000020, 0xC0390001) != 0;
            }

            return false;
        }

        private static bool PostEnterKeyInputToProcess(IntPtr processHandle, int waitTime)
        {
            System.Threading.Thread.Sleep(waitTime);

            // 0xD:Enter
            if (Win32Controller.PostMessage(processHandle, Win32Controller.WmKeyDown, 0x0000000D, 0x001C0001) != 0)
            {
                System.Threading.Thread.Sleep(waitTime);
                return Win32Controller.PostMessage(processHandle, Win32Controller.WmKeyUp, 0x0000000D, 0xC01C0001) != 0;
            }

            return false;
        }


        /// <summary>
        /// http://tech.sanwasystem.com/entry/2015/11/25/171004　様より
        /// </summary>
        
        // 指定したウィンドウの全ての子孫ウィンドウを取得し、リストに追加する
        private static List<WindowData> GetAllChildWindows(WindowData parent, List<WindowData> dest)
        {
            dest.Add(parent);
            EnumChildWindows(parent.HWnd).ToList().ForEach(x => GetAllChildWindows(x, dest));
            return dest;
        }

        // 与えた親ウィンドウの直下にある子ウィンドウを列挙する（孫ウィンドウは見つけてくれない）
        private static IEnumerable<WindowData> EnumChildWindows(IntPtr hParentWindow)
        {
            var hWnd = IntPtr.Zero;
            while ((hWnd = Win32Controller.FindWindowEx(hParentWindow, hWnd, null, null)) != IntPtr.Zero) { yield return GetWindow(hWnd); }
        }

        // ウィンドウハンドルを渡すと、ウィンドウテキスト（ラベルなど）、クラス、スタイルを取得してWindowsクラスに格納して返す
        private static WindowData GetWindow(IntPtr hWnd)
        {
            var textLen = Win32Controller.GetWindowTextLength(hWnd);
            string windowText = null;
            if (textLen > 0)
            {
                //ウィンドウのタイトルを取得する
                var windowTextBuffer = new StringBuilder(textLen + 1);
                Win32Controller.GetWindowText(hWnd, windowTextBuffer, windowTextBuffer.Capacity);
                windowText = windowTextBuffer.ToString();
            }

            //ウィンドウのクラス名を取得する
            var classNameBuffer = new StringBuilder(256);
            Win32Controller.GetClassName(hWnd, classNameBuffer, classNameBuffer.Capacity);

            // スタイルを取得する
            var style = Win32Controller.GetWindowLong(hWnd, Win32Controller.GwlStyle);
            return new WindowData() { HWnd = hWnd, Title = windowText, ClassName = classNameBuffer.ToString(), Style = style };
        }
    }

    /// <summary>
    /// http://tech.sanwasystem.com/entry/2015/11/25/171004 様より
    /// </summary>
    public class WindowData
    {
        public string ClassName { get; set; }

        public string Title { get; set; }

        public IntPtr HWnd { get; set; }

        public int Style { get; set; }
    }
}
