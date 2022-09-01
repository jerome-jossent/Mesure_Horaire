using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

    //affichge de "mark" curseur rond permettant de montrer la sélection actuelle à l'utilisateur

    //étape d'utilisation de l'outil
    //si clik droit on reset, si clik gauche :
    //1er  click 1ère heure
    //2nd  click 2ème heure
    //3ème click sens : horaire/antihoraire ou 1->2/2->1

    // une séquence de 4 états (qui s'incrémente au "click") :
    //- 0 : reset/vierge/première utilisation       | en attente de la sélection du premier point
    //- 1 : premier point sélectionné               | en attente de la sélection du second point
    //- 2 : second point sélectionné                | en attente de la sélection de la zone
    //- 3 : zone sélectionnée                       | sélection terminée

    public partial class LocalisationHoraire : UserControl, INotifyPropertyChanged
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

        public event EventHandler<EventArgs> _SelectedHourChange;
        public EventHandler _Loaded;

        int start, end;
        int step;
        bool loaded;

        bool debug = true;

        Dictionary<int, Secteur> secteurs;
        List<Secteur> concerned_values;
        Secteur previous_sector;

        //mesures par rapport à la position de la souris
        Point mouse_position; //par rapport au du canvas (origine en haut à gauche avec X augmentant en allant sur la droite et Y augmentant en allant vers le bas)
        double X, Y; // position de la souris par rapport au centre du canvas (origine au centre avec X augmentant en allant sur la droite et Y augmentant en allant vers le haut)
        double angle; // exprimé en degrés, 0 quand la souris est au milieu en haut du cadran, augmente dans le sens horaire
        double angle_h; //exprimé en heure (décimal)
        int angle_h0; //exprimé en heure la plus proche

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

        public double _markSize
        {
            get => markSize;
            set
            {
                if (markSize == value) return;
                markSize = value;
                OnPropertyChanged("_markSize");
            }
        }
        double markSize = 8;
        #endregion

        #region Constructeur & Initialisations
        public LocalisationHoraire()
        {
            loaded = false;
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

                s._mark_Size = _markSize;

                s._arc_Horaire_actif = false;
                s._arc_antiHoraire_actif = false;
            }

            _Reset();
            _debug.Visibility = debug ? Visibility.Visible : Visibility.Hidden;
        }
        #endregion

        internal void _Reset()
        {
            Marks_Hidden();
            foreach (Secteur s in secteurs.Values)
            {
                s._arc_Horaire_actif = false;
                s._arc_antiHoraire_actif = false;
            }

            start = 0;
            end = 0;

            step = 0;
        }

        void Marks_Hidden()
        {
            foreach (Secteur s in secteurs.Values)
            {
                s._mark1_Visibility = Visibility.Hidden;
                s._mark2_Visibility = Visibility.Hidden;
            }
        }

        public void _SetValue()
        {
            _SetValue(_code_Emp1, _code_Emp2);
        }

        //par défaut on a au moins une valeur sur "end" / "val2"
        public void _SetValue(int val1, int val2)
        {
            if (!loaded) return;

            concerned_values = new List<Secteur>();
            start = val1;
            end = val2;

            if (val2 > 0 && val2 < 13)
            {
                if (val1 == 0)
                {
                    concerned_values.Add(secteurs[val2]);
                }
                else
                {
                    int val = val1;
                    while (val != val2)
                    {
                        concerned_values.Add(secteurs[val]);
                        val++;
                        if (val == 13) val = 1;
                    }
                    concerned_values.Add(secteurs[val2]);
                }
            }

            //foreach (Secteur s in secteurs.Values)
            //{
            //    s._mark1_Visibility = Visibility.Hidden;
            //    s._mark2_Visibility = Visibility.Hidden;
            //}
            _ActiveArcs();

            Debug();
        }

        void _ActiveArcs()
        {
            //algo : avec un premier élément (=start)
            //-> le 2ème est voisin ?

            if (concerned_values.Count > 1)
            {
                //on active tous les secteurs inclus dans les bornes ET pour les secteurs extrêmes on n'active que les arcs nécessaires
                int val_first = concerned_values[0]._heure;
                int val_last = (end == 0) ? concerned_values[concerned_values.Count - 1]._heure : end;

                foreach (Secteur s in secteurs.Values)
                {
                    if (s._heure == val_first)
                    {
                        s._arc_Horaire_actif = concerned_values.Contains(s);
                        s._arc_antiHoraire_actif = false;
                    }
                    else if (s._heure == val_last)
                    {
                        s._arc_Horaire_actif = false;
                        s._arc_antiHoraire_actif = concerned_values.Contains(s);
                    }
                    else
                    {
                        s._arc_Horaire_actif = concerned_values.Contains(s);
                        s._arc_antiHoraire_actif = concerned_values.Contains(s);
                    }
                }
            }
            else if (concerned_values.Count == 1)
            {
                //on n'active QUE le secteur en activant ses 2 arcs
                foreach (Secteur s in secteurs.Values)
                {
                    if (s == concerned_values[0])
                    {
                        s._arc_Horaire_actif = true;
                        s._arc_antiHoraire_actif = true;
                    }
                    else
                    {
                        s._arc_Horaire_actif = false;
                        s._arc_antiHoraire_actif = false;
                    }
                }
            }
            else
            {
                //on désactive tout les arcs
                foreach (var s in secteurs)
                {
                    s.Value._arc_Horaire_actif = false;
                    s.Value._arc_antiHoraire_actif = false;
                }
            }
        }

        Secteur GetSecteur()
        {
            return secteurs[angle_h0];
        }

        void AngleCompute()
        {
            X = (mouse_position.X - _canvas.Width / 2);
            Y = -(mouse_position.Y - _canvas.Height / 2);

            angle = Math.Atan2(X, Y) / Math.PI * 180;
            if (angle < 0) angle += 360;

            angle_h = angle / 30;

            angle_h0 = (int)Math.Round(angle_h, 0);

            if (angle_h0 == 0) angle_h0 = 12;
        }

        void Debug()
        {
            if (!debug) return;

            string[] msg = new string[]
            {
                "Step " + step,
                mouse_position.X.ToString("F2") + " \t " + mouse_position.Y.ToString("F2"),
                X.ToString("F0") + " \t " + Y.ToString("F0"),
                angle.ToString("F1") + "°",
                angle_h.ToString("F2"),
                angle_h0 + "h",
                start + "h → " + end + "h",
                start * 30 + " \t " + end * 30
            };

            _debug.Text = string.Join("\n", msg);
        }

        void Reset_Asked()
        {
            _Reset();
            _SelectedHourChange?.Invoke(new int[2] { 0, 0 }, null);

        }

        void Canvas_MouseEnter(object sender, MouseEventArgs e)
        {
            switch (step)
            {
                case 0:
                    break;

                case 1:
                    if (secteurs.ContainsKey(start))
                        secteurs[start]._mark1_Visibility = Visibility.Visible;
                    break;

                case 2:
                    if (secteurs.ContainsKey(start))
                        secteurs[start]._mark1_Visibility = Visibility.Visible;
                    if (secteurs.ContainsKey(end))
                        secteurs[end]._mark2_Visibility = Visibility.Visible;
                    break;

                case 3:
                    break;
            }
        }

        void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            mouse_position = e.GetPosition(_canvas);

            AngleCompute();

            Secteur s = GetSecteur();

            if (s != previous_sector)
            {
                switch (step)
                {
                    case 0:
                        if (previous_sector != null)
                            previous_sector._mark1_Visibility = Visibility.Hidden;
                        s._mark1_Visibility = Visibility.Visible;
                        break;

                    case 1:
                        if (previous_sector != null)
                            previous_sector._mark2_Visibility = Visibility.Hidden;
                        s._mark2_Visibility = Visibility.Visible;




                        //int A_tmp, B_tmp;
                        //if (start < end)
                        //{
                        //    A_tmp = start;
                        //    B_tmp = end;
                        //}
                        //else
                        //{
                        //    B_tmp = start;
                        //    A_tmp = end;
                        //}

                        //// A -> B ?
                        //if (A_tmp * 30 < angle && angle < B_tmp * 30)
                        //    _SetValue(A_tmp, B_tmp);
                        //else
                        //    _SetValue(B_tmp, A_tmp);
                        break;

                    case 2:
                        int A, B;
                        if (start < end)
                        {
                            A = start;
                            B = end;
                        }
                        else
                        {
                            B = start;
                            A = end;
                        }

                        // A -> B ?
                        if (A * 30 < angle && angle < B * 30)
                            _SetValue(A, B);
                        else
                            _SetValue(B, A);
                        break;

                    case 3:
                        break;
                }
                previous_sector = s;
            }

            Debug();
        }

        void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Secteur s = GetSecteur();

            //click droit = reset
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Reset_Asked();
                //redessine le marqueur sur ce secteur
                s._mark1_Visibility = Visibility.Visible;
            }
            else
            {
                //click gauche => step by step
                switch (step)
                {
                    case 0:
                        start = s._heure;
                        step = 1;
                        break;

                    case 1:
                        s._mark2_Visibility = Visibility.Visible;
                        end = s._heure;

                        if (start == end) // cas position ponctuelle
                            SaisieTermine();
                        else
                            step = 2;
                        break;

                    case 2:
                        SaisieTermine();
                        break;
                }

                Debug();
            }

            void SaisieTermine()
            {
                //Masquage des marks
                secteurs[start]._mark1_Visibility = Visibility.Hidden;
                secteurs[start]._mark2_Visibility = Visibility.Hidden;

                secteurs[end]._mark1_Visibility = Visibility.Hidden;
                secteurs[end]._mark2_Visibility = Visibility.Hidden;

                step = 3;

                _SelectedHourChange?.Invoke(new int[2] { start, end }, null);
            }
        }
    
        void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            Marks_Hidden();
        }
    }
}