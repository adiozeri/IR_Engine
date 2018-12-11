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
    /// Interaction logic for DictionaryDisplay.xaml
    /// </summary>
    public partial class DictionaryDisplay : Window
    {
        public DictionaryDisplay(ref IRController controller)
        {
            InitializeComponent();
            this.DataContext = controller;
            
        }
    }
}
