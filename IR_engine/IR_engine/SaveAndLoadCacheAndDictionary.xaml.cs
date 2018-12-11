using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IR_engine
{
    /// <summary>
    /// Interaction logic for SaveAndLoadCacheAndDictionary.xaml
    /// </summary>
    public partial class SaveAndLoadCacheAndDictionary : Window
    {
        IRController controller;
        public SaveAndLoadCacheAndDictionary(ref IRController controller)
        {
            InitializeComponent();
            this.controller = controller;
        }
        public void ShowLoadDoneMessage(string message)
        {
            MessageBox.Show(message, "Load Done!!!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void BrowsePath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = "Choose the folder you want to save the data in";
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    controller.UpdateSavingAndLoadingPath(dialog.SelectedPath);
                    pathTextBox.Text = dialog.SelectedPath;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                controller.SaveDataToFolder();
                MessageBox.Show("Saving data is over, now you can load dictionary and cache any time you choose", "Saving is done", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Thread t = new Thread(() => { controller.LoadDataFromFolder(); });

                Dispatcher.Invoke(() => t.Start());
                MessageBox.Show("The data is loading, please be patient", "Loading is running", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}


