using D4TTS.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace D4TTS.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Only set DataContext when not in Design-mode
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = App.Current.Services.GetRequiredService<MainWindowViewModel>();
            }

            InitializeComponent();
        }

        private void ButtonBaseInstall_OnClick(object sender, RoutedEventArgs eventArgs)
        {
            // https://devblogs.microsoft.com/dotnet/wpf-file-dialog-improvements-in-dotnet-8/
            var folderDialog = new OpenFolderDialog
            {
                InitialDirectory = Path.GetDirectoryName(InstallFolderPathTextBox.Text)
            };

            if (folderDialog.ShowDialog() == true)
            {
                InstallFolderPathTextBox.Text = folderDialog.FolderName;
            }
        }

        private void ButtonBaseConfig_OnClick(object sender, RoutedEventArgs eventArgs)
        {
            // https://devblogs.microsoft.com/dotnet/wpf-file-dialog-improvements-in-dotnet-8/
            var folderDialog = new OpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(ConfigFilePathTextBox.Text)
            };

            if (folderDialog.ShowDialog() == true)
            {
                ConfigFilePathTextBox.Text = folderDialog.FileName;
            }
        }

        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(((MainWindowViewModel)DataContext).IsAutoScrollEnabled)
            {
                var listBox = sender as ListBox;
                int count = listBox?.Items.Count ?? 0;
                if (count == 0) return;
                listBox?.ScrollIntoView(listBox.Items[count - 1]);
            }
        }
    }
}