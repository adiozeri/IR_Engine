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
    /// Interaction logic for DocumentSummary.xaml
    /// </summary>
    public partial class DocumentSummary : Window
    {
        public DocumentSummary(Dictionary<int, Tuple<string, float>> sentencesToShow, string docName)
        {         
            InitializeComponent();
            int score = 1;
            Dictionary<Tuple<int, int>, string> sentences = new Dictionary<Tuple<int, int>, string>();
            foreach (var sentence in sentencesToShow)
            {
                sentences.Add(new Tuple<int, int>(sentence.Key, score), sentence.Value.Item1);
                score++;
            }
            PageTitle.Text = "5 Most Significant Sentences Of Document: " + docName;
            sentences = sentences.OrderBy(pair => pair.Key.Item1).ToDictionary(pair => pair.Key, pair => pair.Value);

            int index = 1;
            Dictionary<int, Tuple<string,int>> sentencesOrdered = new Dictionary<int, Tuple<string, int>>();
            foreach (var item in sentences)
            {
                sentencesOrdered.Add(index, new Tuple<string, int>(item.Value, item.Key.Item2));
                index++;
            }

            sentence1score.Text = "1.Score: "+ sentencesOrdered[1].Item2;
            sentence1.Text = sentencesOrdered[1].Item1;

            sentence2score.Text = "2.Score: " + sentencesOrdered[2].Item2;
            sentence2.Text = sentencesOrdered[2].Item1;

            sentence3score.Text = "3.Score: " + sentencesOrdered[3].Item2;
            sentence3.Text = sentencesOrdered[3].Item1;

            sentence4score.Text = "4.Score: " + sentencesOrdered[4].Item2;
            sentence4.Text = sentencesOrdered[4].Item1;

            sentence5score.Text = "5.Score: " + sentencesOrdered[5].Item2;
            sentence5.Text = sentencesOrdered[5].Item1; 
        }
    }
}
