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

namespace IR_engine
{
    /// <summary>
    /// Interaction logic for SaveQueriesToFile.xaml
    /// </summary>
    public partial class SaveQueriesToFile : Window
    {
        IRController controller;
        public SaveQueriesToFile(IRController controller)
        {
            this.controller = controller;
            InitializeComponent();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(fileNameTextBox.Text) || string.IsNullOrWhiteSpace(FilePathTextBox.Text))
                MessageBox.Show("File name or File path are missing", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
           else
            {
                controller.SavingFileName = fileNameTextBox.Text;
                try
                {
                    controller.WriteResultsInfo();
                    MessageBox.Show("now you can run trec-eval and check out the retriving results", "Save Done!!!", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BrwoserPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = "Choose folder that you want to save your result to";
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    controller.SavingQueryResultPath = dialog.SelectedPath;
                    FilePathTextBox.Text = dialog.SelectedPath;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}
