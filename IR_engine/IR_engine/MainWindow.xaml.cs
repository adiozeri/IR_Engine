using IR_engine.PartA;
using Microsoft.Win32;
using SearchEngine.PartA;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IR_engine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IRController controller;
        public MainWindow()
        {
            InitializeComponent();
            controller = new IRController();
            this.DataContext = controller;
            controller.IndexDone += ShowIndexDoneMessage;
            controller.SearchDone += ShowSearchDoneMessage;
            controller.LoadDone += ShowLoadDoneMessage;
        }

        #region Index
        public void ShowLoadDoneMessage(string message)
        {
            MessageBox.Show(message, "Load Done!!!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public void ShowIndexDoneMessage(string message)
        {
            MessageBox.Show(message, "Index Done!!!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void BrowseCorpus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = "Choose folder that contain the 'corpus' folder and 'stop_words' text file data";
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    controller.UpdateCorpusPath(dialog.SelectedPath);
                    corpusTextBox.Text = dialog.SelectedPath;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void BrowseResults_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = "Choose folder that contain the 'corpus' folder and 'stop_words' text file data";
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    controller.ResultPath = dialog.SelectedPath;
                    resultTextBox.Text = dialog.SelectedPath;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                controller.Reset();
                MessageBox.Show("Reset is over, now you can start all over again", "Rsest Done", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void DisplayCache_Click(object sender, RoutedEventArgs e)
        {
            DisplayCache display = new DisplayCache(ref controller);
            display.Show();
        }
        private void DisplayDictionary_Click(object sender, RoutedEventArgs e)
        {
            DictionaryDisplay display = new DictionaryDisplay(ref controller);
            display.Show();
        }
        private void LoadOrSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveAndLoadCacheAndDictionary display = new SaveAndLoadCacheAndDictionary(ref controller);
                display.Show();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void Index_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(controller.InputPath) || string.IsNullOrWhiteSpace(controller.ResultPath))
                MessageBox.Show("corpus path or result path missing", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                try
                {
                    controller.Stemming = (bool)stemming.IsChecked;
                    string message = "We Are working on building your index,please be patient";
                    Thread t = new Thread(() => { controller.Start(); });
                    Dispatcher.Invoke(() => t.Start());

                    MessageBox.Show(message, "Index Starting!!!", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                catch (Exception exp)
                {
                    MessageBox.Show($"Index Break Mode: {exp.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        #endregion

        #region Searcher

        public void ShowSearchDoneMessage(string message)
        {
            MessageBox.Show(message, "Search Done!!!", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    QueryResultsListBox.ItemsSource = controller.QueryFilesNameResults;

                }
                catch(Exception exp)
                {
                    MessageBox.Show(exp.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);

                }
            });
        }

        private void BrowseQueriesPath(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                    queriesFilePath.Text = openFileDialog.FileName;

            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveQeryResults_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveQueriesToFile saveQueries = new SaveQueriesToFile(controller);
                saveQueries.Show();
            }
            catch (Exception exp)
            {
                MessageBox.Show("Save faild: " + exp.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ResetForSearcher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                queriesFilePath.Clear();
                quertTextBox.Clear();
                controller.SearchReset();
            }
            catch (Exception exp)
            {
                MessageBox.Show($"Reset failed: {exp.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void RunSearcher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!((string.IsNullOrWhiteSpace(quertTextBox.Text) && string.IsNullOrWhiteSpace(queriesFilePath.Text)) ||
              (!string.IsNullOrWhiteSpace(quertTextBox.Text) && !string.IsNullOrWhiteSpace(queriesFilePath.Text))))
                {

                    string message = "We Are retriving your results,please be patient";

                    bool singleQueryFlag = false;
                    this.Dispatcher.Invoke(() =>
                    {
                        controller.Stemming = (bool)stemming.IsChecked;
                        controller.DocumentSummary = (bool)DocumentsSummaryOption.IsChecked;
                        singleQueryFlag = !string.IsNullOrWhiteSpace(quertTextBox.Text);
                    });

                    if (singleQueryFlag)
                    {
                        if (controller.DocumentSummary)
                        {
                            Dictionary<int, Tuple<string, float>> sentencesToShow = controller.Search5SignificantSentences(quertTextBox.Text);
                            DocumentSummary w1 = new DocumentSummary(sentencesToShow, quertTextBox.Text);
                            w1.Show();
                        }
                        else
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                           
                                controller.QueryToSearch = quertTextBox.Text;
                                Thread t = new Thread(() => { controller.Search(); });
                                this.Dispatcher.Invoke(() =>
                                {
                                    t.Start();
                                });
                                MessageBox.Show(message, "Search Starting!!!", MessageBoxButton.OK, MessageBoxImage.Information);
                            });
                        }
                    }
                    else // path case
                    {
                        controller.QueriesFilePath = queriesFilePath.Text;
                        Thread t = new Thread(() => { controller.SearchQueriesFromFile(); });
                        this.Dispatcher.Invoke(() =>
                        {
                            t.Start();
                        });
                        MessageBox.Show(message, "Search Starting!!!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid query input", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


        }
        #endregion


    }
}
