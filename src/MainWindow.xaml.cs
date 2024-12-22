using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace BlackOps2Explorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private FastFile _fastFile;
        private bool _isPlaying;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FastFileLoadCompleted()
        {
            statusBarItem.Content = "";
            loadingIconBarItem.Visibility = Visibility.Hidden;
        }

        private void openFastFileItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "Black Ops II Fast File|*.ff" };

            if (!openFileDialog.ShowDialog().Value) return;

            if (_isPlaying)
            {
                mainMediaElement.Stop();
                _isPlaying = false;
            }

            fileListView.Items.Clear();

            _fastFile = new FastFile(openFileDialog.FileName) { IncludeHLSL = includeHLSLMenuItem.IsChecked };
            _fastFile.AssetFound += fastFile_AssetFound;

            Title = string.Format("Black Ops II Fast File Explorer - {0}", openFileDialog.SafeFileName);
            statusBarItem.Content = string.Format("Decompressing and decrypting {0}, please wait...", openFileDialog.SafeFileName);
            loadingIconBarItem.Visibility = Visibility.Visible;

            ThreadPool.QueueUserWorkItem(x =>
            {
                try
                {
                    _fastFile.Load();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Black Ops II Fast File Explorer", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                Dispatcher.Invoke(new Action(() => statusBarItem.Content = "Reading assets, please wait..."));
                
                _fastFile.Find();

                Dispatcher.Invoke(new Action(FastFileLoadCompleted));
            });
        }

        void fastFile_AssetFound(object sender, FastFile.AssetFoundEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => fileListView.Items.Add(e.Asset)));
        }

        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void fileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 0)
                return;

            var currentAsset = (XAsset) e.AddedItems[0];

            if (_isPlaying)
            {
                mainMediaElement.Stop();
                _isPlaying = false;
            }

            if(currentAsset.IsRawText)
            {
                mainRichTextBox.Visibility = Visibility.Visible;
                mainMediaElement.Visibility = Visibility.Hidden;
                mainRichTextBox.Text = Encoding.ASCII.GetString(currentAsset.Data);
            }
            else if (currentAsset.Path.EndsWith(".bik"))
            {
                mainRichTextBox.Visibility = Visibility.Hidden;
                mainMediaElement.Visibility = Visibility.Visible;
                
                // WPF does not support media from streams (only available in Silverlight)
                // so write the file somewhere temporary.
                string path = Path.Combine(Path.GetTempPath(), Path.GetFileName(currentAsset.Path));
                File.WriteAllBytes(path, currentAsset.Data);
                mainMediaElement.Source = new Uri("file://" + path);
                mainMediaElement.Play();
                _isPlaying = true;
            }
            else
            {
                mainRichTextBox.Text = "// The selected file cannot be displayed as raw text,\n// right-click and use the Export option to view its contents.";
            }
        }

        private void exportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            var currentAsset = (XAsset) fileListView.SelectedItem;
            saveFileDialog.Filter = "All Files|*.*";
            saveFileDialog.FileName = Path.GetFileName(currentAsset.Path);
            if(saveFileDialog.ShowDialog().Value)
                File.WriteAllBytes(saveFileDialog.FileName, currentAsset.Data);
        }

        private void exportAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using(var folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Browse the directory to export all the files.";
                if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                foreach (XAsset asset in fileListView.Items)
                {
                    string targetPath = Path.Combine(folderBrowserDialog.SelectedPath, asset.Path);
                    string targetDirectory = Path.GetDirectoryName(targetPath);
                    if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);
                    File.WriteAllBytes(targetPath, asset.Data);
                }

                MessageBox.Show("All files have been successfully exported!", "Black Ops II Fast File Explorer", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
