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
    /// Interaction logic for DisplayCache.xaml
    /// </summary>
    public partial class DisplayCache : Window
    {
        public Dictionary<string, string> cacheForDisplay { get; set; }
        public DisplayCache(ref IRController controller)
        {
            InitializeComponent();
            this.DataContext = this;
            cacheForDisplay = new Dictionary<string, string>();
            foreach (var item in controller.CacheDictionary)
            {
                cacheForDisplay.Add(item.Key, item.Value.DisplayForCache());
            }
        }              
    }
}

