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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ElectroTools.TestTools
{
    /// <summary>
    /// Логика взаимодействия для WinToolsxaml.xaml
    /// </summary>
    public partial class WinTools : Window
    {
      private DeBag _deBag;
        public WinTools()
        {
            InitializeComponent();
            _deBag = new DeBag();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _deBag.ExportSelectedToDxf();
        }
    }
}
