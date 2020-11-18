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
//using System.Windows.Shapes;
using Disk.SDK;
using Disk.SDK.Provider;
using System.Net;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Microsoft.Win32;

using SdkSample.WPF.Properties;

namespace GameApplication
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private IDiskSdkClient sdk;
        private List<DiskItemInfo> folderItems;
        private string currentPath;
        private readonly string acctok = "AgAAAAAT2O6cAAY8xImvxmtkgkDQjyn7-LcIj-Q";
        private readonly string musicPath = "/GuessTheSong/";
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.CreateSdkClient();
        }

        private void CreateSdkClient() 
        {
            this.sdk = new DiskSdkClient(acctok);
            this.AddCompletedHandlers();
        }

        public List<DiskItemInfo> FolderItems
        {
            get { return this.folderItems; }
            set
            {
                this.folderItems = value;
                this.OnPropertyChanged("FolderItems");
            }
        }

        public string CurrentPath
        {
            get
            {
                return this.currentPath != null
                           ? Uri.UnescapeDataString(this.currentPath)
                           : string.Empty;
            }
            set
            {
                if (this.currentPath != value)
                {
                    this.currentPath = value;
                    this.OnPropertyChanged("CurrentPath");
                    this.OnPropertyChanged("WindowTitle");
                }
            }
        }


        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) //geting song list
        {
            this.CurrentPath = musicPath;
            this.sdk.GetListAsync(musicPath);
        }

        private void ButtonSet_Click(object sender, RoutedEventArgs e) // opening Results window
        {
            Application.Current.Properties.Add("client", this.sdk);
            Settings SettingsWindow = new Settings();
            SettingsWindow.Show();
            this.Close();
        }

        private void AddCompletedHandlers()
        {
           
            this.sdk.GetListCompleted += this.SdkOnGetListCompleted;
           
        }


        private void SdkOnGetListCompleted(object sender, GenericSdkEventArgs<IEnumerable<DiskItemInfo>> e) //event handler
        {
            if (e.Error == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() => { this.FolderItems = new List<DiskItemInfo>(e.Result); }));
            }
            else
            {
                this.ProcessError(e.Error);
            }


        }


      

        private void ProcessError(SdkException ex)
        {
            Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("SDK error: " + ex.Message)));
        }

        public event PropertyChangedEventHandler PropertyChanged;


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e) // opening Game window

        {
            if (tbName.Text.ToString() != "")
                Application.Current.Properties.Add("userName", tbName.Text.ToString()); //pass params
            Application.Current.Properties.Add("songsList", this.folderItems);          //to the new 
            Application.Current.Properties.Add("client", this.sdk);                     //window
            Game GameWindow = new Game();
            GameWindow.Show();
            this.Close();
        }
    }
}
