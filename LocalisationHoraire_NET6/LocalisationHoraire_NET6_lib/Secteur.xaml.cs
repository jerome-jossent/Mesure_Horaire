using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LocalisationHoraire_NET6_lib
{
    public partial class Secteur : UserControl, INotifyPropertyChanged
    {
        public event EventHandler<EventArgs> _SecteurClick;
        public event EventHandler<EventArgs> _SecteurEnter;
        public event EventHandler<EventArgs> _SecteurLeave;

        #region BINDING
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public double _angle
        {
            get => angle; set
            {
                if (double.Equals(value, angle)) return;
                angle = value;
                OnPropertyChanged("_angle");
            }
        }
        double angle = 0;

        public int _heure
        {
            get => heure; set
            {
                heure = value;
                if (heure == 0)
                {
                    _arc_Horaire_actif = false;
                    _arc_antiHoraire_actif = false;
                    return;
                }

                _angle = heure * 360 / 12;
                OnPropertyChanged("_heure");
            }
        }
        int heure = 0;

        #region ARC        
        //public bool _arc_actif
        //{
        //    get => arc_ctif; set
        //    {
        //        arc_ctif = value;
        //        _arc_visibility = (arc_ctif) ? Visibility.Visible : Visibility.Hidden;
        //        OnPropertyChanged("_arc_actif");
        //    }
        //}
        //bool arc_ctif = false;

        public bool _arc_Horaire_actif
        {
            get => arc_Horaire_actif; set
            {
                arc_Horaire_actif = value;
                _arc_Horaire_visibility = (arc_Horaire_actif) ? Visibility.Visible : Visibility.Hidden;
                OnPropertyChanged("_arc_Horaire_actif");
            }
        }
        bool arc_Horaire_actif = false;

        public bool _arc_antiHoraire_actif
        {
            get => arc_antiHoraire_actif; set
            {
                arc_antiHoraire_actif = value;
                _arc_antiHoraire_visibility = (arc_antiHoraire_actif) ? Visibility.Visible : Visibility.Hidden;
                OnPropertyChanged("_arc_antiHoraire_actif");
            }
        }
        bool arc_antiHoraire_actif = false;






        public Visibility _arc_Horaire_visibility
        {
            get => arc_Horaire_visibility; set
            {
                arc_Horaire_visibility = value;
                OnPropertyChanged("_arc_Horaire_visibility");
            }
        }
        Visibility arc_Horaire_visibility;

        public Visibility _arc_antiHoraire_visibility
        {
            get => arc_antiHoraire_visibility; set
            {
                arc_antiHoraire_visibility = value;
                OnPropertyChanged("_arc_antiHoraire_visibility");
            }
        }
        Visibility arc_antiHoraire_visibility;


        public double _arc_thickness
        {
            get => arc_thickness; set
            {
                if (arc_thickness == value) return;
                arc_thickness = value;
                OnPropertyChanged("_arc_thickness");
            }
        }
        double arc_thickness;

        public Brush _arc_color
        {
            get => arc_color; set
            {
                if (Brush.Equals(value, arc_color)) return;
                arc_color = value;
                OnPropertyChanged("_arc_color");
            }
        }
        Brush arc_color;


        #endregion

        #region MARK 1
        public Visibility _mark1_Visibility
        {
            get => mark1_Visibility; set
            {
                if (mark1_Visibility == value) return;
                mark1_Visibility = value;
                OnPropertyChanged("_mark1_Visibility");
            }
        }
        Visibility mark1_Visibility = Visibility.Hidden;

        public Brush _mark1_Color
        {
            get => mark1_Color; set
            {
                if (Brush.Equals(value, mark1_Color)) return;
                mark1_Color = value;
                OnPropertyChanged("_mark1_Color");
            }
        }
        Brush mark1_Color;

        public Brush _mark1_StrokeColor
        {
            get => mark1_StrokeColor; set
            {
                if (Brush.Equals(value, mark1_StrokeColor)) return;
                mark1_StrokeColor = value;
                OnPropertyChanged("_mark1_StrokeColor");
            }
        }
        Brush mark1_StrokeColor;

        public double _mark1_Thickness
        {
            get => mark1_Thickness; set
            {
                if (mark1_Thickness == value) return;
                mark1_Thickness = value;
                OnPropertyChanged("_mark1_Thickness");
            }
        }
        double mark1_Thickness;
        #endregion

        #region MARK 2
        public Visibility _mark2_Visibility
        {
            get => mark2_Visibility; set
            {
                if (mark2_Visibility == value) return;
                mark2_Visibility = value;
                OnPropertyChanged("_mark2_Visibility");
            }
        }
        Visibility mark2_Visibility = Visibility.Hidden;

        public Brush _mark2_Color
        {
            get => mark2_Color; set
            {
                if (Brush.Equals(value, mark2_Color)) return;
                mark2_Color = value;
                OnPropertyChanged("_mark2_Color");
            }
        }
        Brush mark2_Color;

        public Brush _mark2_StrokeColor
        {
            get => mark2_StrokeColor; set
            {
                if (Brush.Equals(value, mark2_StrokeColor)) return;
                mark2_StrokeColor = value;
                OnPropertyChanged("_mark2_StrokeColor");
            }
        }
        Brush mark2_StrokeColor;

        public double _mark2_Thickness
        {
            get => mark2_Thickness; set
            {
                if (mark2_Thickness == value) return;
                mark2_Thickness = value;
                OnPropertyChanged("_mark2_Thickness");
            }
        }
        double mark2_Thickness;
        #endregion
        #endregion

        public Secteur()
        {
            DataContext = this;
            InitializeComponent();
        }

        #region INTERACTIONS
        void _MouseDown(object sender, MouseButtonEventArgs e)
        {
            _SecteurClick?.Invoke(this, e);
        }

        void _MouseEnter(object sender, MouseEventArgs e)
        {
            _SecteurEnter?.Invoke(this, e);
        }

        void _MouseLeave(object sender, MouseEventArgs e)
        {
            _SecteurLeave?.Invoke(this, e);
        }
        #endregion
    }
}