using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public int _val1
        {
            get => val1; set
            {
                if (value == val1) return;
                val1 = value;
                OnPropertyChanged("_val1");
            }
        }
        int val1;

        public int _val2
        {
            get => val2; set
            {
                if (value == val2) return;
                val2 = value;
                OnPropertyChanged("_val2");
            }
        }
        int val2;

        public MainWindow()
        {
            InitializeComponent();

            for (int i = 1; i < 13; i++)
            {
                _cbx_1.Items.Add(i);
                _cbx_2.Items.Add(i);
            }

            DataContext = this;
        }

        private void Sel__SelectedHourChange(object sender, EventArgs e)
        {
            int[] _codes_Emp = (int[])sender;
            Title = _codes_Emp[0].ToString() + " - " + _codes_Emp[1].ToString();
        }

    }
}
