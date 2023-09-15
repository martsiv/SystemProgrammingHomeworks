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

namespace _02._1_Tasks_Text_analizer
{
    /// <summary>
    /// Interaction logic for WindowResults.xaml
    /// </summary>
    public partial class WindowResults : Window
    {
        public Stata MyStatistic { get; set; }
        public WindowResults(Stata stata)
        {
            InitializeComponent();
            MyStatistic = stata;
            this.DataContext = MyStatistic;
        }
    }
}
