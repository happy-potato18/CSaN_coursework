using Disk.SDK;
using Disk.SDK.Provider;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
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

namespace GameApplication
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private IDiskSdkClient sdk;
        private string downloadUsersFileName = "";
        private FileStream fs2;
        private bool isLaunch;
        private class User
        {
           
            public string name;
            public int score;

        }
        List<User> users = new List<User>();

        public Settings()
        {
            InitializeComponent();
            this.sdk = (DiskSdkClient)Application.Current.Properties["client"];
        }

        private void GridResults_Loaded(object sender, RoutedEventArgs e)
        {

            this.downloadUsersFileName = "music\\users.txt";
            fs2 = new FileStream(this.downloadUsersFileName, FileMode.OpenOrCreate, FileAccess.Write);
            this.sdk.DownloadFileAsync("/users/users.txt", fs2, new AsyncProgress(this.UpdateProgress), this.SdkOnDownloadTxt2Completed);

           

        }

        private void SdkOnDownloadTxt2Completed(object sender, SdkEventArgs e)
        {
            if (e.Error == null)
            {
                if (this.isLaunch)
                {
                    var fileName = System.IO.Path.GetFileName(this.downloadUsersFileName);
                    var filePath = this.GetFilePath(fileName);

                    this.LaunchFile(filePath);
                    this.isLaunch = false;

                }
            }
            else
            {
                this.ProcessError(e.Error);
            }

            fs2.Close();

            using (StreamReader sr = new StreamReader(this.downloadUsersFileName))
            {
                 while (!sr.EndOfStream)
                 {

                    var txt = sr.ReadLine();
                    User user = new User();
                    string[] values = txt.Split(new char[] { '-' });
                    user.name = values[0];
                    user.score =Int32.Parse(values[1]);
                    users.Add(user);
                                          
                 }

                var sortedUsers = from user in users
                                   orderby user.score
                                   select new
                                   {
                                       user.name,
                                       user.score
                                   };
                

                sortedUsers = sortedUsers.Reverse();
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    gridResults.ItemsSource = sortedUsers.ToList();
                    System.IO.File.Delete(this.downloadUsersFileName);

                }));


               
            }




        }

        private void LaunchFile(string filePath)
        {
            Process.Start(filePath);
        }

        private void ProcessError(SdkException ex)
        {
            Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("SDK error: " + ex.Message)));
        }

        private string GetFilePath(string fileName)
        {
            using (var isolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                using (var oStream = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, isolatedStorageFile))
                {
                    return oStream.GetType()
                               .GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic)
                               .GetValue(oStream).ToString();
                }
            }
        }


        private void UpdateProgress(ulong current, ulong total)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.progressBar.Value = current;
                this.progressBar.Maximum = total;
            }));



        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }



    }
}
