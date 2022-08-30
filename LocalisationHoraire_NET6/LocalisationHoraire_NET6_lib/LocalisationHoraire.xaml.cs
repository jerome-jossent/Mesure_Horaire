﻿using System;
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
    //12 secteurs se composants de 3 choses
    //- 1 morceau antihoraire (polygone visible)
    //- 1 morceau horaire (polygone visible)
    //- 1 polygone invisible utilisé comme bouton (mouse enter/leave/click)

    //Ces 12 secteurs sont réparties comme les heures d'une horloge.

    //on définit la localisation avec 2 valeurs horaires :
    //- si elles sont identiques, seul le secteur concerné affiche ses 2 morceaux
    //- si elles sont différentes, on se pose la question du début et de fin de localisation :
    //  c'est le sens horaire qui va définir quelles sont secteurs sont concernés
    //  avec des secteurs extrêmes qui n'afficheront que le morceau compris dans la zone
    //  ex : 6h->9h est différent de 9h->6h (on peut même dire opposé)

    public partial class LocalisationHoraire : UserControl, INotifyPropertyChanged
    {
        public event EventHandler<EventArgs> _SelectedHourChange;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        Secteur previous_sector;

        //étape d'utilisation de l'outil
        //si clik droit on reset, si clik gauche :
        //1er click 1ère heure
        //2nd click 2ème heure
        //3ème click => ?
        int step;

        Dictionary<int, Secteur> secteurs;
        List<Secteur> concerned_values;
        int start, end;
        int temp_end;

        #region VALEURS        
        public int _code_Emp1
        {
            get { return (int)GetValue(__code_Emp1); }
            set { SetValue(__code_Emp1, value); }
        }
        public static readonly DependencyProperty __code_Emp1 = DependencyProperty.Register(
                "_code_Emp1",
                typeof(int),
                typeof(LocalisationHoraire),
                new FrameworkPropertyMetadata(0, OnCodeEmpChanged));

        public int _code_Emp2
        {
            get { return (int)GetValue(__code_Emp2); }
            set { SetValue(__code_Emp2, value); }
        }
        public static readonly DependencyProperty __code_Emp2 = DependencyProperty.Register(
                "_code_Emp2",
                typeof(int),
                typeof(LocalisationHoraire),
                new FrameworkPropertyMetadata(0, OnCodeEmpChanged));

        private static void OnCodeEmpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LocalisationHoraire source = d as LocalisationHoraire;
            source._SetValue(source._code_Emp1, source._code_Emp2);
        }

        #endregion

        #region EDGES
        public double _thickness
        {
            get => thickness;
            set
            {
                if (double.Equals(value, thickness)) return;
                thickness = value;
                OnPropertyChanged("_Thickness");
            }
        }
        double thickness;

        public Brush _color
        {
            get => color;
            set
            {
                if (Brush.Equals(value, color)) return;
                color = value;
                OnPropertyChanged("_color");
            }
        }
        Brush color;
        #endregion

        #region TEXTE
        //public string _Saisie_complete
        //{
        //    get => Saisie_complete; set
        //    {
        //        if (Saisie_complete == value) return;
        //        Saisie_complete = value;
        //        OnPropertyChanged("_Saisie_complete");
        //    }
        //}
        //string Saisie_complete = "1 - 12";
        public Visibility _textShow
        {
            get => textShow;
            set
            {
                if (textShow == value) return;
                textShow = value;
                OnPropertyChanged("_textShow");
            }
        }
        Visibility textShow = Visibility.Visible;

        /// <summary>
        /// Definition d'un dependency property pour effectuer le binding sur la vue
        /// https://stackoverflow.com/questions/1636807/what-exactly-does-wpf-data-bindings-relativesource-findancestor-do
        /// </summary>
        public static readonly DependencyProperty _textProperty =
            DependencyProperty.Register(
                "Horaire",
                typeof(string),
                typeof(LocalisationHoraire),
                new FrameworkPropertyMetadata(null));

        public string Horaire
        {
            get { return (string)GetValue(_textProperty); }
            set { SetValue(_textProperty, value); }
        }

        public bool _LoseTextFocusOnMouseLeave
        {
            get => loseTextFocusOnMouseLeave;
            set
            {
                if (_LoseTextFocusOnMouseLeave == value) return;
                loseTextFocusOnMouseLeave = value;
                OnPropertyChanged("_LoseTextFocusOnMouseLeave");
            }
        }
        bool loseTextFocusOnMouseLeave;

        #endregion

        #region MARKS
        public Brush _markColor
        {
            get => markColor;
            set
            {
                if (Brush.Equals(value, markColor)) return;
                markColor = value;
                OnPropertyChanged("_markColor");
            }
        }
        Brush markColor;

        public Brush _markStrokeColor
        {
            get => markStrokeColor;
            set
            {
                if (Brush.Equals(value, markStrokeColor)) return;
                markStrokeColor = value;
                OnPropertyChanged("_markStrokeColor");
            }
        }
        Brush markStrokeColor;

        public double _markThickness
        {
            get => markThickness;
            set
            {
                if (markThickness == value) return;
                markThickness = value;
                OnPropertyChanged("_markThickness");
            }
        }
        double markThickness = 1;
        #endregion

        bool loaded;
        public EventHandler _Loaded;


        public LocalisationHoraire()
        {
            loaded = false;
            //DataContext = this;
            InitializeComponent();
        }

        void _Composant_Loaded(object sender, RoutedEventArgs e)
        {
            _INIT();
            loaded = true;
            _Loaded?.Invoke(this, EventArgs.Empty);
        }

        void _INIT()
        {
            secteurs = new Dictionary<int, Secteur>();
            secteurs.Add(S1._heure, S1);
            secteurs.Add(S2._heure, S2);
            secteurs.Add(S3._heure, S3);
            secteurs.Add(S4._heure, S4);
            secteurs.Add(S5._heure, S5);
            secteurs.Add(S6._heure, S6);
            secteurs.Add(S7._heure, S7);
            secteurs.Add(S8._heure, S8);
            secteurs.Add(S9._heure, S9);
            secteurs.Add(S10._heure, S10);
            secteurs.Add(S11._heure, S11);
            secteurs.Add(S12._heure, S12);

            //"liens" fait manuellement
            foreach (Secteur s in secteurs.Values)
            {
                s._arc_thickness = _thickness;
                s._arc_color = _color;

                s._mark1_Color = _markColor;
                s._mark1_StrokeColor = _markStrokeColor;
                s._mark1_Thickness = _markThickness;

                s._mark2_Color = _markColor;
                s._mark2_StrokeColor = _markStrokeColor;
                s._mark2_Thickness = _markThickness;

                s._arc_Horaire_actif = false;
                s._arc_antiHoraire_actif = false;

                s._SecteurEnter += SecteurEnter;
                s._SecteurClick += SecteurClick;
                s._SecteurLeave += SecteurLeave;
            }

            _Reset();
        }

        internal void _Reset()
        {
            foreach (Secteur s in secteurs.Values)
            {
                s._arc_Horaire_actif = false;
                s._arc_antiHoraire_actif = false;
                s._mark1_Visibility = Visibility.Hidden;
                s._mark2_Visibility = Visibility.Hidden;
            }
            concerned_values = new List<Secteur>();
            start = 0;
            end = 0;
            step = 0;
        }

        public void _SetValue()
        {
            _SetValue(_code_Emp1, _code_Emp2);
        }

        public void _SetValue(int val1, int val2)
        {
            if (!loaded) return;

            concerned_values = new List<Secteur>();
            int senshoraire = (val1 < val2) ? 1 : -1;
            senshoraire = (val1 - val2 < 0) ? 1 : -1;
            start = val1;
            end = val2;

            if (end > 0 && end < 13)
            {
                if (start == 0)
                {
                    concerned_values.Add(secteurs[end]);
                }
                else
                {
                    int val = start;
                    while (val != end)
                    {
                        concerned_values.Add(secteurs[val]);
                        val++;
                        if (val == 13) val = 1;
                    }
                    concerned_values.Add(secteurs[end]);
                }
            }
            //if (start > 0 && start < 13 && end > 0 && end < 13)
            //{
            //    int val = start;
            //    while (val != end)
            //    {
            //        concerned_values.Add(secteurs[val]);
            //        val++;
            //        if (val == 13) val = 1;
            //    }
            //    concerned_values.Add(secteurs[end]);
            //}

            _ActiveSecteurs(false);

            //_code_Emp1 = start;
            //_code_Emp2 = end;
        }

        void _ActiveSecteurs(bool full = true)
        {
            //algo : avec un premier élément (=start)
            //-> le 2ème est voisin ?
            if (concerned_values.Count > 1)
            {
                int val1 = concerned_values[0]._heure;
                int val2 = concerned_values[1]._heure;
                int diff = val1 - val2;
                int diff_abs = Math.Abs(diff);
                int val_last = (end == 0) ? concerned_values[concerned_values.Count - 1]._heure : end;

                int senshoraire = 0;
                if (diff_abs < 3)
                    senshoraire = (diff < 0) ? 1 : -1;
                else if (diff_abs > 9)
                    senshoraire = (diff > 0) ? 1 : -1;

                if (senshoraire != 0)
                {
                    int val = val1;
                    int index = 1;
                    //rempli les trous ??
                    while (val != val_last)
                    {
                        if (!concerned_values.Contains(secteurs[val]))
                        {
                            try
                            {
                                concerned_values.Insert(index, secteurs[val]);
                            }
                            catch (Exception ex)
                            {
                                val = val;
                            }
                        }
                        val += senshoraire;
                        if (val == 13) val = 1;
                        if (val == 0) val = 12;
                        index++;
                    }
                }

                foreach (Secteur s in secteurs.Values)
                {
                    if (s._heure == val1)
                    {
                        s._arc_Horaire_actif = concerned_values.Contains(s) && (senshoraire == 1);
                        s._arc_antiHoraire_actif = concerned_values.Contains(s) && (senshoraire == -1);
                    }
                    else if (s._heure == val_last)
                    {
                        s._arc_Horaire_actif = concerned_values.Contains(s) && (senshoraire == -1);
                        s._arc_antiHoraire_actif = concerned_values.Contains(s) && (senshoraire == 1);
                    }
                    else
                    {
                        s._arc_Horaire_actif = concerned_values.Contains(s);
                        s._arc_antiHoraire_actif = concerned_values.Contains(s);
                    }
                }

                if (full)
                {
                    if (senshoraire > 0)
                    {
                        _code_Emp1 = start;
                        _code_Emp2 = (end == 0) ? val_last : end;
                    }

                    if (senshoraire < 0)
                    {
                        _code_Emp2 = start;
                        _code_Emp1 = (end == 0) ? val_last : end;
                    }
                }
            }
            else if (concerned_values.Count == 1)
            {
                foreach (var s in secteurs)
                {
                    if (concerned_values[0] == s.Value)
                    {
                        s.Value._arc_Horaire_actif = true;
                        s.Value._arc_antiHoraire_actif = true;
                    }
                    else
                    {
                        s.Value._arc_Horaire_actif = false;
                        s.Value._arc_antiHoraire_actif = false;
                    }
                }
            }
            else
            {
                foreach (var s in secteurs)
                {
                    s.Value._arc_Horaire_actif = false;
                    s.Value._arc_antiHoraire_actif = false;
                }
            }
        }

        Secteur GetSecteur(object sender)
        {
            return (Secteur)sender;
        }


        #region UC Methods
        void SecteurClick(object sender, EventArgs e)
        {
            Secteur s = GetSecteur(sender);

            if (((MouseButtonEventArgs)e).RightButton == MouseButtonState.Pressed)
            {
                _Reset();
                //redessine le marqueur sur ce secteur
                s._mark1_Visibility = Visibility.Visible;
                _SelectedHourChange?.Invoke(new int[2] { 0, 0 }, null);
                return;
            }

            switch (step)
            {
                case 0:
                    //s._mark1_Visibility = Visibility.Visible;
                    s._mark1_Visibility = Visibility.Hidden;
                    s._arc_antiHoraire_actif = true;
                    s._arc_Horaire_actif = true;
                    step = 1;
                    start = s._heure;
                    concerned_values.Add(s);
                    break;
                case 1:
                    //s._mark2_Visibility = Visibility.Visible;
                    //s._arc_actif = true;
                    end = s._heure;
                    step = 2;

                    _SelectedHourChange?.Invoke(new int[2] { start, end }, null);
                    break;
            }
            _ActiveSecteurs();
        }

        void SecteurEnter(object sender, EventArgs e)
        {
            //montrer mark
            Secteur s = GetSecteur(sender);
            switch (step)
            {
                case 0:
                    s._mark1_Visibility = Visibility.Visible;
                    break;

                case 1:
                    //s._mark2_Visibility = Visibility.Visible;
                    if (concerned_values.Contains(s))
                        concerned_values.Remove(previous_sector);
                    else
                        concerned_values.Add(s);

                    temp_end = s._heure;
                    _ActiveSecteurs();
                    break;
            }
            previous_sector = s;
        }

        void SecteurLeave(object sender, EventArgs e)
        {
            //retirer mark
            Secteur s = GetSecteur(sender);
            switch (step)
            {
                case 0:
                    s._mark1_Visibility = Visibility.Hidden;
                    break;

                case 1:
                    s._mark2_Visibility = Visibility.Hidden;
                    break;
            }
            _ActiveSecteurs();
        }

        #endregion
    }
}