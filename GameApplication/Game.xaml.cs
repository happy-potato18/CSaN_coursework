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
using System.Windows.Navigation;
using Disk.SDK;
using Disk.SDK.Provider;
using System.Net;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Microsoft.Win32;

using SdkSample.WPF.Properties;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Threading;

namespace GameApplication
{
    /// <summary>
    /// Логика взаимодействия для Game.xaml
    /// </summary>
    public partial class Game : Window
    {

        private IDiskSdkClient sdk;
        private List<DiskItemInfo> songList;
        private string downloadFileName;
        private bool isLaunch;
        private MediaPlayer mp;
        private List<string> txtSongList = new List<string>();
        private List<Button> buttons = new List<Button>();
        private FileStream fs, fs2;
        private string currentSongName = "";
        private string downloadUsersFileName = "";
        private string userName = "user";
        private string score = "";
        private bool stop = false;

        public Game()
        {
            InitializeComponent();
            this.DataContext = this;
            this.sdk =(DiskSdkClient)Application.Current.Properties["client"];
            this.songList = (List<DiskItemInfo>)Application.Current.Properties["songsList"];
            if(Application.Current.Properties.Contains("userName"))
                this.userName = Application.Current.Properties["userName"].ToString();

        }


       
        private void PlayingRandomSong()
        {
            if (txtSongList.Count > 4)
            {
                
                Random rnd = new Random();
                int index = rnd.Next(0, songList.Count - 1);
                var currentSong = songList[index];
                songList.Remove(currentSong);
                this.downloadFileName = "music\\song.mp3";
                fs = new FileStream(this.downloadFileName, FileMode.OpenOrCreate, FileAccess.Write);
                this.sdk.DownloadFileAsync(currentSong.OriginalFullPath, fs, new AsyncProgress(this.UpdateProgress), this.SdkOnDownloadCompleted);
                SuffleClass.Shuffle<Button>(buttons);
                SuffleClass.Shuffle<string>(txtSongList);
                mp = new MediaPlayer();
                mp.MediaEnded += Mp_MediaEnded;
            }
            else
            {
                MessageBox.Show("You guessed last song! See you.");
                this.Close();
            }
        }

        private void Mp_MediaEnded(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                foreach (var button in buttons)
                {
                    button.Tag = "0";
                    button.Content = "";

                }

                mp.Stop();
                mp.Close();
                System.IO.File.Delete(this.downloadFileName);
                this.PlayingRandomSong();



            }));
        }

        private  void UpdateProgress(ulong current, ulong total)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.progressBar.Value = current;
                this.progressBar.Maximum = total;
            }));

           
                    
        }

        
        private void SdkOnDownloadCompleted(object sender, SdkEventArgs e)
        {

            if (e.Error == null)
            {
                if (this.isLaunch)
                {
                    var fileName = Path.GetFileName(this.downloadFileName);
                    var filePath = this.GetFilePath(fileName);

                    this.LaunchFile(filePath);
                    this.isLaunch = false;
                   
                }
            }
            else
            {
                this.ProcessError(e.Error);
            }

            fs.Close();

            TagLib.File f = TagLib.File.Create(downloadFileName);
            currentSongName = f.Tag.FirstPerformer + " - " + f.Tag.Title;
            
            Dispatcher.BeginInvoke((Action)(() =>
            {
                foreach (int i in new List<int>() { 0, 1, 2 })
                {
                    if (txtSongList[i] != currentSongName)
                        this.buttons[i].Content = txtSongList[i];
                    else
                        this.buttons[i].Content = txtSongList[3];
                }

                this.buttons[3].Content = currentSongName;
                this.buttons[3].Tag = "1";
                mp.Open(new Uri(this.downloadFileName, UriKind.RelativeOrAbsolute));
                mp.Play();
            }));
           
            txtSongList.Remove(currentSongName);
            f.Dispose();
                              
          
        }

        private void GameWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
            buttons.AddRange(new List<Button>() { Button1, Button2, Button3, Button4 });

            using (StreamReader sr = new StreamReader("music//list.txt"))
            {
                while(!sr.EndOfStream)
                {
                    
                    var base64EncodedBytes = Convert.FromBase64String(sr.ReadLine());
                    txtSongList.Add(Encoding.UTF8.GetString(base64EncodedBytes));
                }
            }

            this.PlayingRandomSong();



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

        private void LaunchFile(string filePath)
        {
            Process.Start(filePath);
        }

        private void ProcessError(SdkException ex)
        {
            Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("SDK error: " + ex.Message)));
        }

        

        private void GameWindow_Unloaded(object sender, RoutedEventArgs e)
        {
          

        }

    
        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
          
            if (((sender as Button).Tag).ToString() == "1")
            {
                (sender as Button).Background = new SolidColorBrush()
                {
                    Color = Colors.GreenYellow
                };

                LblScore.Content = (Int32.Parse(LblScore.Content as string) + 1).ToString();

            }
            else
            {
                (sender as Button).Background = new SolidColorBrush()
                {
                    Color = Colors.Salmon
                };
            }

            await Task.Delay(1000);

            foreach (var button in buttons)
            {
                button.Background = new SolidColorBrush()
                {
                    Color = Colors.DodgerBlue
                };
                button.Tag = "0";
                button.Content = "";

            }
                     

            mp.Stop();
            mp.Close();
           // System.IO.File.Delete(this.downloadFileName);
            this.PlayingRandomSong();

        }

        private  void Window_Closing(object sender, CancelEventArgs e)
        {
            mp.Stop();
            mp.Close();
            Random rnd = new Random();
            score = LblScore.Content.ToString();
            this.downloadUsersFileName = "music\\users.txt";
            fs2 = new FileStream(this.downloadUsersFileName, FileMode.OpenOrCreate, FileAccess.Write);
            this.sdk.DownloadFileAsync("/users/users.txt", fs2, new AsyncProgress(this.UpdateProgress), this.SdkOnDownloadTxtCompleted);
           
            System.IO.File.Delete(this.downloadFileName);
        }

        private void SdkOnDownloadTxtCompleted(object sender, SdkEventArgs e)
        {
            if (e.Error == null)
            {
                if (this.isLaunch)
                {
                    var fileName = Path.GetFileName(this.downloadUsersFileName);
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

                    
            using (StreamWriter sr = new StreamWriter(this.downloadUsersFileName, true, Encoding.UTF8))
            {

                 sr.WriteLine(userName + '-' + score);

            }

           


        }

        private void Window_Closed(object sender, EventArgs e)
        {
            fs2.Close();

            fs2 = new FileStream(this.downloadUsersFileName, FileMode.Open, FileAccess.Read);

            this.sdk.UploadFileAsync("/users/users.txt", fs2, new AsyncProgress(this.UpdateProgress), this.SdkOnUploadCompleted);

            while (!stop) { }  
                  

        }

       

        private void SdkOnUploadCompleted(object sender, SdkEventArgs e)
        {
           
            if (e.Error == null)
            {
                
            }
            else
            {
                this.ProcessError(e.Error);
            }

            fs2.Close();
            System.IO.File.Delete(this.downloadUsersFileName);
            stop = true;
           

        }

    }
}
