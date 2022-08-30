using System;
using System.Collections.Generic;
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

namespace MesureHoraire
{
    #region TODO
    // lors de la pose des aiguilles 1 et 2, mettre 1 flèche perpendiculaire pour dire le côté de l'observation
    // changement de couleurs des aiguilles
    // changement de masque
    // proposer mode heure ou mode degrés

    //autre fonctions
    //- rapport de surface (nbr pixel)
    //- mesure de distance (gabarit ?)
    #endregion

    public partial class MainWindow : Window
    {
        #region Parameters
        //int clickNumero;
        enum etape { pt_Centre, pt_Aiguille_1, pt_Aiguille_2, pt_Midi_SixH, pt_Secteur }
        etape selection;
        int selectionMax;

        Line Aiguille1, Aiguille2, MidiSixH;
        Point Centre, PtAiguille1, PtAiguille2, PtMidiSixH, PtSecteur, PtSecteur_inf_B, PtSecteur_inf_C;
        double heure1, heure2;
        double angle, angle1, angle2;

        bool visualisationmode = true;
        bool changeByCode;

        enum angle_mode { clock, degrees }
        #endregion

        public MainWindow()
        {
            changeByCode = true;
            UI_PreInit();
            InitializeComponent();
            UI_PostInit();
            changeByCode = false;

            UpdateAngleMode();
        }

        #region Window Managment
        void UI_PreInit()
        {
            visualisationmode = Properties.Settings.Default.visualisationmode;

            moveAndResize();

            selectionMax = Enum.GetNames(typeof(etape)).Length;
        }

        void moveAndResize(object sender, RoutedEventArgs e)
        {
            visualisationmode = !visualisationmode;
            restart();
        }
        void moveAndResize()
        {
            if (visualisationmode)
                SetWindowTransparent();
            else
                SetWindowOpaque();
        }

        void SetWindowTransparent()
        {
            this.AllowsTransparency = true;
            this.WindowStyle = WindowStyle.None;
            ChangeOpacityWindow();
        }

        void ChangeOpacityWindow()
        {
            SolidColorBrush scb = new SolidColorBrush(Colors.White);
            scb.Opacity = Properties.Settings.Default.Opacity;
            this.Background = scb;
        }

        void SetWindowOpaque()
        {
            this.AllowsTransparency = false;
            this.WindowStyle = WindowStyle.SingleBorderWindow;

            SolidColorBrush scb = new SolidColorBrush(Colors.White);
            this.Background = scb;
        }

        void UI_PostInit()
        {
            SetWindowSizeAndPosition();
            GetPrettyIcons();
            ColorsUpdate();
            ResetMesure();

            ClrPcker_Observation.SelectedColor = Properties.Settings.Default.ColorAiguilles;
            ClrPcker_12H.SelectedColor = Properties.Settings.Default.ColorAiguille_12H;
            _ckb_instrucions.IsChecked = !Properties.Settings.Default.instructions_cachees;
            UpdateInstructions();

            AngleModeInit();
        }

        void SetWindowSizeAndPosition()
        {
            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;
            this.Top = Properties.Settings.Default.Bottom - this.Height;
            this.Left = Properties.Settings.Default.Left;
            if (Properties.Settings.Default.Maximized)
                WindowState = WindowState.Maximized;
        }

        void GetPrettyIcons()
        {
            SetPrettyIcon("pack://application:,,,/Images/002-move.png", _icon_move);
            SetPrettyIcon("pack://application:,,,/Images/ButtonClose.png", _icon_close);
            SetPrettyIcon("pack://application:,,,/Images/Veolia.png", _icon_veolia);
            Image IS = new Image();
            SetPrettyIcon("pack://application:,,,/Images/Veolia.png", IS);
            this.Icon = IS.Source;
        }

        void SetPrettyIcon(string iconPath, Image img)
        {
            Uri iconUri = new Uri(iconPath, UriKind.RelativeOrAbsolute);

            BitmapSource bmp16 = HL.CSharp.Wpf.Icons.IconBitmapEncoder.GetResized(new BitmapImage(iconUri), 16);
            HL.CSharp.Wpf.Icons.IconBitmapEncoder encoder = new HL.CSharp.Wpf.Icons.IconBitmapEncoder();

            img.Source = BitmapFrame.Create(HL.CSharp.Wpf.Icons.IconBitmapEncoder.Get24plus8BitImage(bmp16));
        }

        void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void restart()
        {
            SaveParameters();

            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            System.Windows.Application.Current.Shutdown();
        }

        void SaveParameters()
        {
            Properties.Settings.Default.visualisationmode = visualisationmode;

            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Properties.Settings.Default.Bottom = RestoreBounds.Top + this.Height;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Bottom = this.Top + this.Height;
                Properties.Settings.Default.Left = this.Left;
                Properties.Settings.Default.Height = this.Height;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Maximized = false;
            }

            Properties.Settings.Default.Save();
        }
        #endregion

        #region Line Managment

        Line AddLine(Point p1, Point p2, Brush color, double epaisseur)
        {
            Line line = new Line();
            UpdateLine(ref line, p1, p2, color, epaisseur);
            return line;
        }

        void UpdateLine(ref Line line, Point p1, Point p2, Brush color, double epaisseur)
        {
            line.Visibility = System.Windows.Visibility.Visible;
            line.StrokeThickness = epaisseur;
            line.Stroke = color;
            LineSetBegin(line, p1);
            LineSetEnd(line, p2);
        }
        
        void LineSetBegin(Line ligne, Point pt)
        {
            ligne.X1 = pt.X;
            ligne.Y1 = pt.Y;
        }

        void LineSetEnd(Line ligne, Point pt)
        {
            ligne.X2 = pt.X;
            ligne.Y2 = pt.Y;
        }
        #endregion

        #region Interaction Managment
        void ResetMesure()
        {
            _txb_infos.Text = "";
            selection = 0;

            cadran.Visibility = Visibility.Hidden;

            if (Aiguille1 != null) Aiguille1.Visibility = Visibility.Hidden;
            if (Aiguille2 != null) Aiguille2.Visibility = Visibility.Hidden;
            if (MidiSixH != null) MidiSixH.Visibility = Visibility.Hidden;
        }

        void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(this._cnv);
            switch (selection)
            {
                case etape.pt_Centre:
                    Centre = mousePosition;
                    break;

                case etape.pt_Aiguille_1:
                    PtAiguille1 = mousePosition;
                    LineSetEnd(Aiguille1, mousePosition);
                    break;

                case etape.pt_Aiguille_2:
                    PtAiguille2 = mousePosition;
                    LineSetEnd(Aiguille2, mousePosition);

                    angle = AngleDeg(Centre, PtAiguille1, mousePosition);
                    //angle = Math.Abs(angle);
                    //if (angle > 180)
                    //    angle -= angle;


                    //PROBLEME DE CALCUL ICI !!!!!!!!

                    UpdateInfos();
                    break;

                case etape.pt_Midi_SixH:
                    PtMidiSixH = mousePosition;
                    LineSetEnd(MidiSixH, mousePosition);

                    Point haut = new Point(mousePosition.X, -50000);
                    double anglecadran = AngleDeg(Centre, mousePosition, haut);
                    double d = Distance(mousePosition, Centre);

                    //affichage cadran
                    cadran.Visibility = Visibility.Visible;
                    cadran.Width = d * 2;
                    cadran.Height = d * 2;
                    Canvas.SetLeft(cadran, Centre.X - d);
                    Canvas.SetTop(cadran, Centre.Y - d);
                    cadran_rotation.Angle = anglecadran;

                    //calcul des angles puis des heures
                    angle1 = AngleDeg(Centre, PtAiguille1, mousePosition);
                    angle2 = AngleDeg(Centre, PtAiguille2, mousePosition);
                    UpdateInfos();
                    break;

                case etape.pt_Secteur:
                    PtSecteur = mousePosition;
                    bool val = PointInTriangle(Centre, PtSecteur_inf_B, PtSecteur_inf_C, mousePosition);
                    UpdateInfos(val);
                    break;
            }
        }

        void ChangeOpacity(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (changeByCode) return;
            Properties.Settings.Default.Opacity = e.NewValue;
            Properties.Settings.Default.Save();
            ChangeOpacityWindow();
        }

        void _cnv_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            angle_mode am = UpdateAngleMode();
            Properties.Settings.Default.AngleMode = (int)am;
            SaveParameters();
        }

        angle_mode UpdateAngleMode()
        {
            angle_mode angle_Mode = 0;
            //vérifie si le mode d'angle
            if (rb_angle_mode_clock.IsChecked == true)
                angle_Mode = angle_mode.clock;
            if (rb_angle_mode_degrees.IsChecked == true)
                angle_Mode = angle_mode.degrees;

            Uri uri = null;
            if (angle_Mode == angle_mode.clock)
                uri = new Uri("pack://application:,,,/Images/cadran.png", UriKind.RelativeOrAbsolute);
            if (angle_Mode == angle_mode.degrees)
                uri = new Uri("pack://application:,,,/Images/cadran_degres.png", UriKind.RelativeOrAbsolute);

            cadran.Source = new BitmapImage(uri);
            return angle_Mode;
        }

        void ClrPcker_Observation_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (changeByCode) return;
            Properties.Settings.Default.ColorAiguilles = (Color)ClrPcker_Observation.SelectedColor;
            ColorsUpdate();
        }

        void ClrPcker_12H_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (changeByCode) return;
            Properties.Settings.Default.ColorAiguille_12H = (Color)ClrPcker_12H.SelectedColor;
            ColorsUpdate();
        }

        void ColorsUpdate()
        {
            if (Aiguille1 != null) Aiguille1.Stroke = new SolidColorBrush(Properties.Settings.Default.ColorAiguilles);
            if (Aiguille2 != null) Aiguille2.Stroke = new SolidColorBrush(Properties.Settings.Default.ColorAiguilles);
            if (MidiSixH != null) MidiSixH.Stroke = new SolidColorBrush(Properties.Settings.Default.ColorAiguille_12H);
        }

        void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                selection++;

                //en mode degrés on saute l'étape de positionnement de 12H
                if ((angle_mode)Properties.Settings.Default.AngleMode == angle_mode.degrees)
                    if (selection == etape.pt_Midi_SixH)
                        selection++;

                Point mousePosition = e.GetPosition(this._cnv);
                OnLeftClic(mousePosition);

                if ((int)selection > selectionMax)
                    ResetMesure();

                e.Handled = true;
            }
            if (e.ChangedButton == MouseButton.Right)
            {
                ResetMesure();
                // on ne set pas le handle à true sinon l'affichage du menu contextutuel ne se fait pas
            }
            UpdateInstructions();
        }

        void OnLeftClic(Point mousePosition)
        {
            switch (selection)
            {
                case etape.pt_Centre:
                    break;

                case etape.pt_Aiguille_1:
                    //position infos + futur cadran
                    Canvas.SetLeft(_vbx_infos, Centre.X - _vbx_infos.Width / 2);
                    Canvas.SetTop(_vbx_infos, Centre.Y - _vbx_infos.Height / 2);
                    //position Aiguille 1
                    Line_Init(ref Aiguille1, Properties.Settings.Default.ColorAiguilles, Centre, Centre);
                    break;

                case etape.pt_Aiguille_2:
                    //position Aiguille 2
                    Line_Init(ref Aiguille2, Properties.Settings.Default.ColorAiguilles, Centre, Centre);
                    break;

                case etape.pt_Midi_SixH:
                    //position Aiguille Midi
                    Line_Init(ref MidiSixH, Properties.Settings.Default.ColorAiguille_12H, Centre, Centre);

                    //affichage cadran
                    _vbx_infos.Visibility = Visibility.Visible;

                    //calcul points triangle pour détection secteur
                    PtSecteur_inf_B = (PtAiguille1 - Centre) * 500 + Centre;
                    PtSecteur_inf_C = (PtAiguille2 - Centre) * 500 + Centre;
                    break;

                case etape.pt_Secteur:
                    break;
            }
        }

        void Line_Init(ref Line aiguille, Color col, Point A, Point B)
        {
            if (aiguille == null)
            {
                //init aiguille
                aiguille = AddLine(A, B, new SolidColorBrush(col), 3);
                _cnv.Children.Add(aiguille);
            }
            else
                UpdateLine(ref aiguille, A, B, new SolidColorBrush(col), 3);
        }

        void UpdateInstructions()
        {
            if (Properties.Settings.Default.instructions_cachees)
            {
                _vbx_instructions.Visibility = Visibility.Hidden;
                return;
            }

            string msg;
            switch (selection)
            {
                case etape.pt_Centre:
                    msg = "Sélectionnez le centre de la canalisation";
                    break;
                case etape.pt_Aiguille_1:
                    msg = "Sélectionnez bord 1";
                    break;
                case etape.pt_Aiguille_2:
                    msg = "Sélectionnez bord 2";
                    break;
                case etape.pt_Midi_SixH:
                    msg = "Sélectionnez 12H00";
                    break;
                case etape.pt_Secteur:
                    msg = "Sélectionnez la zone concernée";
                    break;
                default:
                    msg = "";
                    break;
            }
            _vbx_instructions.Visibility = Visibility.Visible;
            _txb_instructions.Text = msg;
        }

        void UpdateInfos(bool? val = null)
        {
            string s = "";
            switch (selection)
            {
                case etape.pt_Centre:
                    s = "";
                    break;

                case etape.pt_Aiguille_1:
                    s = "";
                    break;

                case etape.pt_Aiguille_2:
                    s = "Angle " + angle.ToString("0");
                    break;

                case etape.pt_Midi_SixH:
                    if ((angle_mode)Properties.Settings.Default.AngleMode == angle_mode.clock)
                    {
                        heure1 = AngleToHeure(angle1);
                        heure2 = AngleToHeure(angle2);
                        s = HeureToString(heure1) + "   " + HeureToString(heure2);
                    }
                    break;

                case etape.pt_Secteur:
                    if ((angle_mode)Properties.Settings.Default.AngleMode == angle_mode.clock)
                    {
                        if (val == true)
                            s = HeureToString(heure1) + " →  " + HeureToString(heure2);
                        if (val == false)
                            s = HeureToString(heure2) + " →  " + HeureToString(heure1);
                    }

                    if ((angle_mode)Properties.Settings.Default.AngleMode == angle_mode.degrees)
                    {
                        if (val == true)
                            s = "Angle " + angle.ToString("0");
                        if (val == false)
                            s = "Angle " + (360 - angle).ToString("0");
                    }

                    break;

                default:
                    break;
            }
            _txb_infos.Text = s;
        }

        void AngleModeInit()
        {
            angle_mode am = (angle_mode)Properties.Settings.Default.AngleMode;
            switch (am)
            {
                case angle_mode.clock:
                    rb_angle_mode_clock.IsChecked = true;
                    break;
                case angle_mode.degrees:
                    rb_angle_mode_degrees.IsChecked = true;
                    break;
                default:
                    rb_angle_mode_degrees.IsChecked = true;
                    break;
            }
        }

        void _ckb_instructions_Change(object sender, RoutedEventArgs e)
        {
            if (changeByCode) return;
            Properties.Settings.Default.instructions_cachees = _ckb_instrucions.IsChecked == false;
            UpdateInstructions();
        }
        #endregion

        #region Tools
        /// <summary>
        /// angle entre AB et AC
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <returns></returns>
        double AngleDeg(Point A, Point B, Point C)
        {
            return AngleRad(A, B, C) * 180 / Math.PI;
        }
        double AngleRad(Point A, Point B, Point C)
        {
            double val = Math.Atan2(B.Y - A.Y, B.X - A.X) - Math.Atan2(C.Y - A.Y, C.X - A.X);
            //val = Math.Atan2(C.Y - A.Y, C.X - A.X) - Math.Atan2(B.Y - A.Y, B.X - A.X);
            return val;
        }

        double Distance(Point mousePosition, Point centre)
        {
            return Math.Sqrt(Math.Pow((mousePosition.X - centre.X), 2) + Math.Pow((mousePosition.Y - centre.Y), 2));
        }

        string HeureToString(double heure)
        {
            int h = Convert.ToInt32(Math.Floor(heure));
            double minute = (heure - h) * 60;
            int m = Convert.ToInt32(minute);
            if (m == 60)
            {
                h++;
                m = 0;
            }

            string s = h + ((m >= 10) ? ":" : ":0") + m;
            return s;
        }

        double AngleToHeure(double angle)
        {
            if (angle >= 0)
                return angle / 30;
            else
                return (180 + angle) / 30 + 6;
        }

        bool PointInTriangle(Point a, Point b, Point c, Point p)
        {
            Point d, e;
            float w1, w2;
            d = (Point)(b - a);
            e = (Point)(c - a);
            w1 = (float)((e.X * (a.Y - p.Y) + e.Y * (p.X - a.X)) / (d.X * e.Y - d.Y * e.X));
            w2 = (float)((p.Y - a.Y - w1 * d.Y) / e.Y);
            return (w1 >= 0.0) && (w2 >= 0.0) && ((w1 + w2) <= 1.0);
        }
        #endregion
    }
}