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

namespace LocalisationHoraire_NET6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            sel._SelectedHourChange += Sel__SelectedHourChange;

            DataContext = this;
            //sel_manu._LINK(sel);
        }

        private void Sel__SelectedHourChange(object sender, EventArgs e)
        {
            int[] _codes_Emp = (int[])sender;
            Title = _codes_Emp[0].ToString() + " - " + _codes_Emp[1].ToString();
        }

    }
}
