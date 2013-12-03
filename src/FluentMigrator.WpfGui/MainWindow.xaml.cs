using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Win32;

namespace FluentMigrator.WpfGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            SetDefaults();
            InitializeComponent();
        }

        private void SetDefaults()
        {
            ServerName = "(local)\\SQLEXPRESS";
            DbName = "SalusUAT";
            CurrentVersion = "(No Connection)";
            MigrationFilePath = String.Empty;
            _dbConnectionIsGood = false;
            _migrateFileIsGood = false;
        }

        public string ServerName { get; set; }
        public string DbName { get; set; }
        public string CurrentVersion { get; set; }
        public string MigrationFilePath { get; set; }
        public IEnumerable<Version> MigrationVersionList { get; set; }

        private Boolean _dbConnectionIsGood, _migrateFileIsGood;

        private void ConnectButtonClicked(object sender, RoutedEventArgs e)
        {
            string newVersion = "(No Connection)";
            _dbConnectionIsGood = false;
            try
            {
                long versionReturned;
                if (long.TryParse(ExecuteTask("showlatest"), out versionReturned))
                {
                    newVersion = GetVersionFromMigrationCode(versionReturned).ToString();
                    _dbConnectionIsGood = true;
                }
            }
            catch (SqlException) //do nothing if the database connection fails
            {}
            
            CurrentVersion = newVersion;
            NotifyPropertyChanged("CurrentVersion");
            ResetButtonStatus();
        }

        private void SelectFileButtonClicked(object sender, RoutedEventArgs e)
        {
            _migrateFileIsGood = false;
            var openFileDialog = new OpenFileDialog {DefaultExt = ".dll", Filter = "Migration Libraries (.dll)|*.dll"};
            bool? openResult = openFileDialog.ShowDialog();

            if (openResult != true) return;

            MigrationFilePath = openFileDialog.FileName;
            NotifyPropertyChanged("MigrationFilePath");

            MigrationVersionList = GetVersionListWithReflection();
            _migrateFileIsGood = MigrationVersionList.Any();
            NotifyPropertyChanged("MigrationVersionList");
            ResetButtonStatus();
        }

        private void ResetButtonStatus()
        {
            if (_dbConnectionIsGood && _migrateFileIsGood)
            {
                MigrateToSelectedButton.IsEnabled = true;
                MigrateToLatestButton.IsEnabled = MigrationVersionList.First().ToString() != CurrentVersion;
            }
            else
            {
                MigrateToLatestButton.IsEnabled = false;
                MigrateToSelectedButton.IsEnabled = false;
            }
        }

        private string ExecuteTask(string task, long version = 0)
        {
            var catcher = new OutputCatcher();
            new TaskExecutor(GetRunnerContext(catcher, task)).Execute();
            return catcher.Output;
        }

        private RunnerContext GetRunnerContext(IAnnouncer announcer, string task)
        {
            return new RunnerContext(announcer)//expects to get an IAnnouncer to say what the results of the commands are
            {
                Database = "SqlServer2008",
                Connection = GetConnectionString(),
                Target = MigrationFilePath,//path to the assembly holding the migrations
                Task = task,//"migrate:newer",
                Version = 0//version to migrate to as long
            };
        }

        private string GetConnectionString()
        {
            return "Data Source=" + ServerName + ";Initial Catalog=" + DbName + ";Integrated Security=SSPI;MultipleActiveResultSets=True";
        }

        private IEnumerable<Version> GetVersionListWithReflection()
        {
            return Assembly
                .LoadFile(MigrationFilePath)
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Migration)))
                .Select(t => t.GetCustomAttributes(typeof(MigrationAttribute), false)
                    .OfType<MigrationAttribute>()
                    .Single())
                .Select(ma => ma.Version)
                .Select(GetVersionFromMigrationCode)
                .OrderByDescending(v => v)
                .ToArray();
        }

        private Version GetVersionFromMigrationCode(long inputCode)
        {
            var inputBytes = BitConverter.GetBytes(inputCode);
            int rev = BitConverter.ToUInt16(new[] { inputBytes[0], inputBytes[1] }, 0);
            int build = BitConverter.ToUInt16(new[] { inputBytes[2], inputBytes[3] }, 0);
            int minor = BitConverter.ToUInt16(new[] { inputBytes[4], inputBytes[5] }, 0);
            int major = BitConverter.ToUInt16(new[] { inputBytes[6], inputBytes[7] }, 0);
            return new Version(major, minor, build, rev);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MigrateToLatestClicked(object sender, RoutedEventArgs e)
        {
            
        }

        private void MigrateToSelectedClicked(object sender, RoutedEventArgs e)
        {
        }
    }
}
