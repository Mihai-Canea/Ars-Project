using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ARS_Studio.Classi;
using Point = System.Windows.Point;
using Microsoft.Win32;

//Aforge
using AForge.Video.DirectShow;
using AForge.Video;
using AForge.Imaging.Filters;
using AForge.Imaging;
using System.Windows.Threading;

namespace ARS_Studio
{

    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variabili

        /// <summary>Variabile che memorizza l'effetto shadow del border principale per rimuoverlo e applicarlo al <see cref="System.Windows.Window.StateChanged"/> della finestra (massimizza = no effetto. Ripristina = effetto ripristinato)</summary>
        private Effect OmbraFinestra;
        /// <summary>Il fattore di moltiplicazione per l'impostazione delle tab; equivale al numero della tab aperta in base 0. È ASSOLUTAMENTE CONSIGLIATO DI NON MODIFICARLO (Viene modificato nell'evento <see cref="AnimTabPages"/>)</summary>
        private int TabMainPageMoltiplicazione = 0;
        /// <summary>Posizione relativa del cursore rispetto al Picker attuale per mantenerlo in linea col cursore</summary>
        private Point PickerRelativePosition;
        /// <summary>Per sapere se è attiva la griglia automatica o no nella schermata scansione</summary>
        private bool AutoGriglia = true;

        /// <summary>La lista di webcam disponibili</summary>
        private FilterInfoCollection AvaibleCameras;
        /// <summary>La webcam attualmente in uso</summary>
        private VideoCaptureDevice SelectedCamera;

        /// <summary>Memorizza il colore selezionato</summary>
        private SolidColorBrush ColoreSelezionato = CuboMetods.Colors.Empty;


        private string SolverPath = "../../../../TwoPhaseSolver/SolverTest/bin/Debug/";
        //private string SolverPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/ARS Studio/";
        private string Visualizzazione1;
        private string Visualizzazione2;

        private Process Proc;

        private SerialPort SerialCom;
        private const int BAUD_RATE = 9600;


        private DispatcherTimer TmrScansiona = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        private int FacciaScan = 0;
        
        #endregion


        #region Finestra

        /// <summary>
        /// Costruttore della MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            //TODO: Far funzionare il maximize in modo che non copra la barra start
            //Costruzione della classe per massimizzare la finestra in modo che non copra la barra Start. Non è necessario memorizzarla
            //new WindowResizer(this);                                    // Non funziona ancora, quindi è disattivato per ora, ma è comunque possibile ingrandire la finestra, che però copre l'intero schermo :(
        }

        /// <summary>
        /// Evento al caricamento della pagina
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //Si memorizza l'effetto shadow del border principale
            OmbraFinestra = MainContainer.Effect;
            AggiornaCamereDispoibili(CmbAvaibleCameras);

            //CuboMetods.GetEdge(Cube, Cubo.Faccia.F1, 0, 0).Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
            //CuboMetods.SvuotaColori(Cube);
            //CuboMetods.AzzeraColori(Cube);

            CuboMetods.AssegnaEventoButtonColor_Click(StkButtonColor, ButtonColorClick);
            CuboMetods.AssegnaEventoButtonCube_Click (Cube, ButtonCubeClick);

            /*FSWSoluzione.Path = SolverPath;
            FSWSoluzione.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite| NotifyFilters.FileName | NotifyFilters.DirectoryName;
            FSWSoluzione.Created += FswSoluzioneOnCreated;
            FSWSoluzione.EnableRaisingEvents = true;*/

            AggiornaSeriali();

            TmrScansiona.Tick += new EventHandler(TmrScansiona_Tick);
        }

        /// <summary>
        /// Evento al Massimizza, Minimizza e Ripristina della finestra. Si assegnano o rimuovono i padding e gli effetti
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            //Si applica o rimuove il padding e gli effetti del border
            switch (WindowState)
            {
                case WindowState.Normal:                                                                                //Ripristina = padding ripristinato
                    MainContainer.Padding = (Thickness)Application.Current.Resources["ThicknessWindowBorder"];
                    MainContainer.Effect = OmbraFinestra;
                    break;
                case WindowState.Maximized:                                                                             //Massimizza = no padding
                    MainContainer.Padding = new Thickness(0);
                    MainContainer.Effect = null;
                    break;
            }
        }

        /// <summary>
        /// Evento al ridimensionamento della finestra
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Si aggiorna il margin delle tab nel MainFrame per l'allineamento a sinistra
            AnimTabPages(TabContainer, TabMainPageMoltiplicazione, MainFrame.ActualWidth, false, 0);
        }


        //TODO: implementare le animazioni di Massimizza / Minimizza, Riduci, Apri/Chiudi
        /// <summary>
        /// Procedura per massimizzare / ripristinare lo stato della finestra
        /// </summary>
        /// <param name="animazione"></param>
        public void MassimizzaFinestra(bool animazione = true)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        /// <summary>
        /// Procedura per ridurre ad icona la finestra
        /// </summary>
        /// <param name="animazione"></param>
        public void RiduciFinestra(bool animazione = true)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Procedura per mettere / togliere dal primo piano la finestra
        /// </summary>
        public void TopMostFinestra()
        {
            Topmost ^= true;
        }

        /// <summary>
        /// Procedura per chiudere la finestra attuale
        /// </summary>
        /// <param name="animazione"></param>
        public void ChiudiFinestra(bool animazione = true)
        {
            try { SelectedCamera.Stop(); } catch { /*Eccezione ignorata*/ }
            Close();
        }


        /// <summary>
        /// Tasti scelta rapida
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D0: ColoreSelezionato = (SolidColorBrush)BtnEmpty.Background; break;
                case Key.D1: ColoreSelezionato = (SolidColorBrush)BtnBianco.Background; break;
                case Key.D2: ColoreSelezionato = (SolidColorBrush)BtnGiallo.Background; break;
                case Key.D3: ColoreSelezionato = (SolidColorBrush)BtnArancione.Background; break;
                case Key.D4: ColoreSelezionato = (SolidColorBrush)BtnRosso.Background; break;
                case Key.D5: ColoreSelezionato = (SolidColorBrush)BtnVerde.Background; break;
                case Key.D6: ColoreSelezionato = (SolidColorBrush)BtnBlu.Background; break;
            }
        }


        #endregion


        #region ControlBox

        /// <summary>
        /// Evento al <see cref="Button.OnClick"/> dei button nella ControlBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnControl_Click(object sender, RoutedEventArgs e)
        {
            var premuto = (Button)sender;
            if (Equals(premuto, BtnControlChiudi))
                ChiudiFinestra();
            else if (Equals(premuto, BtnControlMassimizza))
                MassimizzaFinestra();
            else if (Equals(premuto, BtnControlRiduci))
                RiduciFinestra();
            else if (Equals(premuto, BtnControlTopMost))
                TopMostFinestra();
        }

        #endregion


        #region MenuTab

        /// <summary>
        /// Evento al <see cref="Button.Click"/> dei pulsanti nella Tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTab_OnClick(object sender, RoutedEventArgs e)
        {
            //Si animano le tab, col relativo menu, in base a quale pulsante è stato premuto
            if (Equals(sender, BtnTabSoluzione))                                                    //Tab Soluzione (1) - Menu1
            {
                AnimTabPages(TabContainer, Grid.GetColumn(Page1), MainFrame.ActualWidth);
                AnimTabPages(MenuContainer, Grid.GetColumn(Menu1), MenuRight.ActualWidth, false);
            } 
            else if (Equals(sender, BtnTabScansione))                                               //Tab Scansione (2) - Menu2
            {
                AnimTabPages(TabContainer, Grid.GetColumn(Page2), MainFrame.ActualWidth);
                AnimTabPages(MenuContainer, Grid.GetColumn(Menu2), MenuRight.ActualWidth, false);
            }
        }

        /// <summary>
        /// Procedura per animare il cambio di Tab
        /// </summary>
        /// <param name="tabContainer">Il contenitore delle pagine</param>
        /// <param name="moltiplicazione">Il numero della pagina SU BASE 0</param>
        /// <param name="containerWidth">L'ACTUAL WIDTH del padre del contenitore delle Pagine</param>
        /// <param name="changeTabMainPageMoltiplicazione">Per decidere se si vuole modificare il fattore di moltiplicazione globale</param>
        /// <param name="duration">Durata dell'animazione in SECONDI. Predefinito: 0.5s</param>
        public void AnimTabPages(UIElement tabContainer, int moltiplicazione, double containerWidth, bool changeTabMainPageMoltiplicazione = true, double duration = 0.5)
        {
            //Dichiarazione animazione di Thickness...
            ThicknessAnimation animThickness = new ThicknessAnimation(
                    new Thickness(-containerWidth * moltiplicazione, 0, 0, 0),
                    new Duration(TimeSpan.FromSeconds(duration)))
                    { EasingFunction =  new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 8 } };

            //... e la si fa partire sul contenitore dei tab inserito come parametro
            tabContainer.BeginAnimation(MarginProperty, animThickness);

            //Si memorizza il fattore di moltiplicazione inserito come parametro per poi riutilizzarlo durante il ridimensionamento
            TabMainPageMoltiplicazione = moltiplicazione;
        }

        #endregion


        #region Soluzione

        private void ButtonCubeClick(object sender, RoutedEventArgs e)
        {
            ((Button)sender).Background = ColoreSelezionato;
        }

        private void ButtonColorClick(object sender, RoutedEventArgs e)
        {
            ColoreSelezionato = ((Button)sender)?.Background as SolidColorBrush;
        }

        private void BtnSalvaCubo_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog
            {
                FileName = DateTime.Today.ToShortDateString().Replace('/', '-') + ".cst",
                DefaultExt = ".cst",
                Filter = "Cube State|*.cst"
            };

            if (save.ShowDialog() == true)
            {
                string output = "";
                for (int f = 1; f <= 6; f++)
                    for (int i = 0; i <= 2; i++)
                        for (int j = 0; j <= 2; j++)
                            output += CuboMetods.NumberFromColor((SolidColorBrush)CuboMetods.GetEdge(Cube, f, i, j).Background) + "\n";
                File.WriteAllText(save.FileName, output);
            }
        }

        private void BtnCaricaCubo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog
            {
                FileName = DateTime.Today.ToShortDateString().Replace('/', '-') + ".cst",
                DefaultExt = ".cst",
                Filter = "Cube State|*.cst"
            };

            if (open.ShowDialog() == true)
            {
                StreamReader sr = new StreamReader(open.FileName);
                int s;
                SolidColorBrush output = new SolidColorBrush();

                try
                {
                    for (int f = 1; f <= 6; f++)
                        for (int i = 0; i <= 2; i++)
                            for (int j = 0; j <= 2; j++)
                            {
                                s = Convert.ToInt16(sr.ReadLine());
                                output = CuboMetods.ColorFromNumber(s);
                                CuboMetods.GetEdge(Cube, f, i, j).Background = output;
                            }
                }
                catch { MessageBox.Show("Non è possibile caricare il cubo"); }
            }
        }

        private void CmbSeriale_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApriSeriale(ref SerialCom, CmbSeriale.SelectedItem.ToString());
        }



        #region Risoluzione

        private void BtnStringiPinze_OnClick(object sender, RoutedEventArgs e)
        {
            SerialCom?.Write(",");
        }

        private void BtnInvia_OnClick(object sender, RoutedEventArgs e)
        {
            SerialWrite();
        }

        private void BtnRisolvi_OnClick(object sender, RoutedEventArgs e)
        {
            try { File.Delete(SolverPath + "Oggetti_output.txt"); }
            catch { /* Eccezione ignorata. Il file non esiste */ }
            try { File.Delete(SolverPath + "Orientamenti_output.txt"); }
            catch { /* Eccezione ignorata. Il file non esiste */ }
            try { File.Delete(SolverPath + "Soluzione.txt"); }
            catch { /* Eccezione ignorata. Il file non esiste */ }
            try { File.Delete(SolverPath + "Orient.txt"); }
            catch { /* Eccezione ignorata. Il file non esiste */ }

            Directory.CreateDirectory(SolverPath);

            var angoliRead = File.ReadAllLines("Angoli.txt");
            var spigoliRead = File.ReadAllLines("Spigoli.txt");

            var quadretto = new int[54];
            CreaQuadretto(quadretto);

            int a, b, c,
                ogg = 0;
            const char sep = '/';

            int[] oggetti = new int[20],
                orientam = new int[20];
            //--------------------------------------------------------------------------------
            //salvo in bin/debug il cst del cubo attuale
            StreamWriter sw = new StreamWriter("ImpostazioneCubo.txt",false);
            string output = "";
            int cont = 0;
            string[] v = new string[54];
            for (int f = 1; f <= 6; f++)
                for (int i = 0; i <= 2; i++)
                    for (int j = 0; j <= 2; j++)
                    {
                        output += CuboMetods.NumberFromColor((SolidColorBrush)CuboMetods.GetEdge(Cube, f, i, j).Background) + " ";
                        cont++;
                    }
            v = output.Split(' ');
            for (int i = 0; i < cont; i++)
                sw.WriteLine(v[i]);
            sw.Close();

            try
            {
                File.WriteAllText(SolverPath + "Orient.txt", Orientamento(quadretto));

                /* ANGOLI superiori */
                a = 7;
                b = 18;
                c = 24;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(angoliRead, $"{quadretto[a]}{quadretto[b]}{quadretto[c]}")
                        .Split(sep)[1]
                    );

                if (quadretto[a] == 1 || quadretto[a] == 2)
                    orientam[ogg] = 0;
                else if (quadretto[b] == 1 || quadretto[b] == 2)
                    orientam[ogg] = 2;
                else if (quadretto[c] == 1 || quadretto[c] == 2)
                    orientam[ogg] = 1;


                a = 5;
                b = 16;
                c = 10;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(angoliRead, $"{quadretto[a]}{quadretto[b]}{quadretto[c]}")
                        .Split(sep)[1]
                );

                if (quadretto[a] == 1 || quadretto[a] == 2)
                    orientam[ogg] = 0;
                else if (quadretto[b] == 1 || quadretto[b] == 2)
                    orientam[ogg] = 1;
                else if (quadretto[c] == 1 || quadretto[c] == 2)
                    orientam[ogg] = 2;


                a = 0;
                b = 8;
                c = 34;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(angoliRead, $"{quadretto[a]}{quadretto[b]}{quadretto[c]}")
                        .Split(sep)[1]
                );

                if (quadretto[a] == 1 || quadretto[a] == 2)
                    orientam[ogg] = 0;
                else if (quadretto[b] == 1 || quadretto[b] == 2)
                    orientam[ogg] = 1;
                else if (quadretto[c] == 1 || quadretto[c] == 2)
                    orientam[ogg] = 2;


                a = 2;
                b = 26;
                c = 32;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(angoliRead, $"{quadretto[a]}{quadretto[b]}{quadretto[c]}")
                        .Split(sep)[1]
                );

                if (quadretto[a] == 1 || quadretto[a] == 2)
                    orientam[ogg] = 0;
                else if (quadretto[b] == 1 || quadretto[b] == 2)
                    orientam[ogg] = 2;
                else if (quadretto[c] == 1 || quadretto[c] == 2)
                    orientam[ogg] = 1;


                //Inferiori

                a = 23;
                b = 29;
                c = 42;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(angoliRead, $"{quadretto[a]}{quadretto[b]}{quadretto[c]}")
                        .Split(sep)[1]
                );

                if (quadretto[a] == 1 || quadretto[a] == 2)
                    orientam[ogg] = 1;
                else if (quadretto[b] == 1 || quadretto[b] == 2)
                    orientam[ogg] = 2;
                else if (quadretto[c] == 1 || quadretto[c] == 2)
                    orientam[ogg] = 0;


                a = 15;
                b = 21;
                c = 40;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(angoliRead, $"{quadretto[a]}{quadretto[b]}{quadretto[c]}")
                        .Split(sep)[1]
                );

                if (quadretto[a] == 1 || quadretto[a] == 2)
                    orientam[ogg] = 1;
                else if (quadretto[b] == 1 || quadretto[b] == 2)
                    orientam[ogg] = 2;
                else if (quadretto[c] == 1 || quadretto[c] == 2)
                    orientam[ogg] = 0;


                a = 13;
                b = 39;
                c = 45;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(angoliRead, $"{quadretto[a]}{quadretto[b]}{quadretto[c]}")
                        .Split(sep)[1]
                );

                if (quadretto[a] == 1 || quadretto[a] == 2)
                    orientam[ogg] = 2;
                else if (quadretto[b] == 1 || quadretto[b] == 2)
                    orientam[ogg] = 1;
                else if (quadretto[c] == 1 || quadretto[c] == 2)
                    orientam[ogg] = 0;


                a = 47;
                b = 31;
                c = 37;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(angoliRead, $"{quadretto[a]}{quadretto[b]}{quadretto[c]}")
                        .Split(sep)[1]
                );

                if (quadretto[a] == 1 || quadretto[a] == 2)
                    orientam[ogg] = 0;
                else if (quadretto[b] == 1 || quadretto[b] == 2)
                    orientam[ogg] = 1;
                else if (quadretto[c] == 1 || quadretto[c] == 2)
                    orientam[ogg] = 2;



                /* SPIGOLI fascia sopra */
                a = 4;
                b = 25;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(spigoliRead, $"{quadretto[a]}{quadretto[b]}")
                        .Split(sep)[1]
                );
                ControlloSpigoli(quadretto, a, b, ogg, orientam);


                a = 6;
                b = 17;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(spigoliRead, $"{quadretto[a]}{quadretto[b]}")
                        .Split(sep)[1]
                );
                ControlloSpigoli(quadretto, a, b, ogg, orientam);


                a = 3;
                b = 9;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(spigoliRead, $"{quadretto[a]}{quadretto[b]}")
                        .Split(sep)[1]
                );
                ControlloSpigoli(quadretto, a, b, ogg, orientam);


                a = 1;
                b = 33;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(spigoliRead, $"{quadretto[a]}{quadretto[b]}")
                        .Split(sep)[1]
                );
                ControlloSpigoli(quadretto, a, b, ogg, orientam);


                a = 44;
                b = 30;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(spigoliRead, $"{quadretto[a]}{quadretto[b]}")
                        .Split(sep)[1]
                );
                ControlloSpigoli(quadretto, a, b, ogg, orientam);


                a = 41;
                b = 22;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(spigoliRead, $"{quadretto[a]}{quadretto[b]}")
                        .Split(sep)[1]
                );
                ControlloSpigoli(quadretto, a, b, ogg, orientam);


                a = 43;
                b = 14;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(spigoliRead, $"{quadretto[a]}{quadretto[b]}")
                        .Split(sep)[1]
                );
                ControlloSpigoli(quadretto, a, b, ogg, orientam);


                a = 46;
                b = 38;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(spigoliRead, $"{quadretto[a]}{quadretto[b]}")
                        .Split(sep)[1]
                );
                ControlloSpigoli(quadretto, a, b, ogg, orientam);


                //fascia intermedia
                a = 20;
                b = 27;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(spigoliRead, $"{quadretto[a]}{quadretto[b]}")
                        .Split(sep)[1]
                );
                ControlloSpigoli(quadretto, a, b, ogg, orientam);


                a = 19;
                b = 12;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(spigoliRead, $"{quadretto[a]}{quadretto[b]}")
                        .Split(sep)[1]
                );
                ControlloSpigoli(quadretto, a, b, ogg, orientam);


                a = 36;
                b = 11;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(spigoliRead, $"{quadretto[a]}{quadretto[b]}")
                        .Split(sep)[1]
                );
                ControlloSpigoli(quadretto, a, b, ogg, orientam);


                a = 35;
                b = 28;
                ogg++;

                oggetti[ogg] = Convert.ToInt32(
                    GeneralMethods.Cerca(spigoliRead, $"{quadretto[a]}{quadretto[b]}")
                        .Split(sep)[1]
                );
                ControlloSpigoli(quadretto, a, b, ogg, orientam);
            }
            catch
            {
                MessageBox.Show("Il cubo non è stato colorato correttamente");
                return;
            }

            /* OUTPUT */

            try
            {
                File.WriteAllText(SolverPath + "Oggetti_output.txt", string.Join("/", oggetti));
                File.WriteAllText(SolverPath + "Orientamenti_output.txt", string.Join("/", orientam));
            }
            catch
            { MessageBox.Show("Non è stato possibile avviare il risolutore"); }

            StartSolver();

            CuboMetods.AzzeraColori(Cube);

            var solver = new ProcessStartInfo { FileName = AppDomain.CurrentDomain.BaseDirectory + "../../../../cube3D_WPF/cube3D_WPF/bin/Debug/cube3D_WPF.exe",
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory + "../../../../cube3D_WPF/cube3D_WPF/bin/Debug"
            };
            Proc = Process.Start(solver);

        }


        private static void ControlloSpigoli(int[] quadretto, int a, int b, int ogg, int[] orientam)
        {
            if (quadretto[a] == 1 || quadretto[a] == 2 || quadretto[b] == 3 || quadretto[b] == 4)
                orientam[ogg] = 0;
            else if (quadretto[b] == 1 || quadretto[b] == 2 || quadretto[a] == 3 || quadretto[a] == 4)
                orientam[ogg] = 1;
        }

        private void StartSolver()
        {
            var solver = new ProcessStartInfo { FileName = AppDomain.CurrentDomain.BaseDirectory + "../../../../TwoPhaseSolver/SolverTest/bin/Debug/SolverTest.exe", WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory + "../../../../TwoPhaseSolver/SolverTest/bin/Debug" };
            Proc = Process.Start(solver);

            if (Proc == null) return;
            Proc.WaitForExit();
            if (Proc.ExitCode != 0) return;

            Visualizzazione1 = "";
            Visualizzazione2 = "";
            if (File.Exists(SolverPath + "Soluzione.txt"))
            {
                Visualizzazione1 = File.ReadAllText(SolverPath + "Soluzione.txt");
                Visualizzazione2 = Visualizzazione1To2();
            }
            TxtRisoluzione.Text = Visualizzazione1;
            Proc.Dispose();
            //SerialWrite();
        }

        private void SerialWrite()
        {
            if (Visualizzazione2 == null || SerialCom == null) return;
            SerialCom.Write(Visualizzazione2.Replace(" ", ""));
        }

        private void TxtRisoluzione_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TxtRisoluzione.Text = TxtRisoluzione.Text == Visualizzazione1 ? Visualizzazione2 : Visualizzazione1;
        }

        private string Visualizzazione1To2()
        {
            //Visualizzazione1 =  + Visualizzazione1;
            var soluzione = (File.ReadAllText(SolverPath + "Orient.txt") + Visualizzazione1).Split(' ');
            var sol = "";
            var ret = "";

            for (var i = 0; i < soluzione.Length; i++)
            {
                switch (soluzione[i])
                {
                    case "1":
                        sol += "jnilmk"                                ; break;
                    case "2":                                          
                        sol += "jmilnk"                                ; break;
                    case "3":                                          
                        sol += "lokjpi"                                ; break;
                    case "4":                                          
                        sol += "lpkjoi"                                ; break;
                    case "5":                                          
                        sol += "lokjpmilnpkjoi"                        ; break;
                    case "6":                                          
                        sol += "lokjpnilmpkjoi"                        ; break;
                    case "7":
                        sol += "jmmi"                                  ; break;
                    case "8":
                        sol += "jmmi"                                  ; break;
                    case "9":
                        sol += "lppk"                                  ; break;
                    case "/":
                        sol += "lppk"                                  ; break;
                    case "(":
                        sol += "lokjommpilpk"                          ; break;
                    case ")":
                        sol += "lokjommpilpk"                          ; break;

                    case "L":                                          
                        soluzione[i] = "f"; sol += "flek"              ; break;
                    case "L'":                                         
                        soluzione[i] = "e"; sol += "elfk"              ; break;
                    case "R":                                          
                        soluzione[i] = "a"; sol += "blak"              ; break;
                    case "R'":                                         
                        soluzione[i] = "b"; sol += "albk"              ; break;
                    case "U":                                          
                        soluzione[i] = "q"; sol += "lmkjnihjgilnkjmi"  ; break;
                    case "U'":                                         
                        soluzione[i] = "r"; sol += "lmkjnigjhilnkjmi"  ; break;
                    case "D":                                          
                        soluzione[i] = "n"; sol += "lmkjnidjcmilnk"    ; break;
                    case "D'":                                         
                        soluzione[i] = "m"; sol += "lmkjnicjdmilnk"    ; break;
                    case "F":                                          
                        soluzione[i] = "l"; sol += "djci"              ; break;
                    case "F'":                                         
                        soluzione[i] = "k"; sol += "cjdi"              ; break;
                    case "B":                                          
                        soluzione[i] = "g"; sol += "hjgi"              ; break;
                    case "B'":                                         
                        soluzione[i] = "h"; sol += "gjhi"              ; break;
                    case "M":                                          
                        soluzione[i] = "i"; sol += "lokp"              ; break;
                    case "M'":                                         
                        soluzione[i] = "j"; sol += "lpko"              ; break;
                    case "E":                                          
                        soluzione[i] = "p"; sol += "lmkjnilpkolnkjmi"  ; break;
                    case "E'":                                         
                        soluzione[i] = "o"; sol += "lmkjnilokplnkjmi"  ; break;
                    case "S":                                          
                        soluzione[i] = "d"; sol += "jmin"              ; break;
                    case "S'":                                         
                        soluzione[i] = "c"; sol += "jnim"              ; break;
                                                                       
                    case "L2":                                         
                        soluzione[i] = "F"; sol += "ff"                ; break;
                    case "R2":                                         
                        soluzione[i] = "A"; sol += "bb"                ; break;
                    case "U2":                                         
                        soluzione[i] = "Q"; sol += "jnilmkggjmilnk"    ; break;
                    case "D2":                                         
                        soluzione[i] = "N"; sol += "jnilmkddjmilnk"    ; break;
                    case "F2":                                         
                        soluzione[i] = "L"; sol += "cc"                ; break;
                    case "B2":                                         
                        soluzione[i] = "G"; sol += "gg"                ; break;
                    case "M2":                                         
                        soluzione[i] = "I"; sol += "oolppk"            ; break;
                    case "E2":
                        soluzione[i] = "P"; sol += "jnilmkoolookjmilnk"; break;
                    case "S2":
                        soluzione[i] = "D"; sol += "nnjmmi"            ; break;
                    case "None":
                        soluzione[i] = ""                              ; break;
                    default:
                        soluzione[i] = ""                              ; break;
                }
            }

            var lstInput   = new List<string>() { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r" };
            var lstInversa = new List<string>() { "b", "a", "d", "c", "f", "e", "h", "g", "j", "i", "l", "k", "n", "m", "p", "o", "r", "q" };

            sol += " ";

            Inizio:
            for (var i = 0; i < sol.Length - 1; i++)
            {
                var numInizio  = lstInput  .IndexOf(Convert.ToString(sol[i]    ));
                var numInverso = lstInversa.IndexOf(Convert.ToString(sol[i + 1]));

                if (numInizio == numInverso)
                {
                    i++;
                    //MessageBox.Show("SAS");
                }
                else
                    ret += sol[i];
            }

            if (ret + " " != sol)
            {
                sol = ret + " ";
                ret = "";
                goto Inizio;
            }

            return string.Join(" ", ret);
        }

        #endregion


        #region Rotazioni

        private string Orientamento(int[] quad)
        {
            var ret = "";

            if (quad[49] == 1)
            {
                Rot2(quad);
                ret = "4 ";
            }
            else if (quad[50] == 1)
            {
                Rot1(quad);
                ret = "1 ";
            }
            else if (quad[51] == 1)
            {
                Rot2(quad); Rot2(quad); Rot2(quad);
                ret = "3 ";
            }
            else if (quad[52] == 1)
            {
                Rot1(quad); Rot1(quad); Rot1(quad);
                ret = "2 ";
            }
            else if (quad[53] == 1)
            {
                Rot1(quad); Rot1(quad);
                ret = "7 ";
            }

            if (quad[49] == 5)
            {
                Rot0(quad);
                ret += "6 ";
            }
            else if (quad[51] == 5)
            {
                Rot0(quad); Rot0(quad); Rot0(quad);
                ret += "5";
            }
            else if (quad[52] == 5)
            {
                Rot0(quad); Rot0(quad);
                ret = "( ";
            }

            return ret;
        }

        public void Rot0(int[] quad)
        {
            var aus = new int[quad.Length];

            aus[0]  = quad[2 ];
            aus[1]  = quad[4 ];
            aus[2]  = quad[7 ];
            aus[3]  = quad[1 ];
            aus[4]  = quad[6 ];
            aus[5]  = quad[0 ];
            aus[6]  = quad[3 ];
            aus[7]  = quad[5 ];
            aus[8]  = quad[32];
            aus[9]  = quad[33];
            aus[10] = quad[34];
            aus[11] = quad[35];
            aus[12] = quad[36];
            aus[13] = quad[37];
            aus[14] = quad[38];
            aus[15] = quad[39];
            aus[16] = quad[8 ];
            aus[17] = quad[9 ];
            aus[18] = quad[10];
            aus[19] = quad[11];
            aus[20] = quad[12];
            aus[21] = quad[13];
            aus[22] = quad[14];
            aus[23] = quad[15];
            aus[24] = quad[16];
            aus[25] = quad[17];
            aus[26] = quad[18];
            aus[27] = quad[19];
            aus[28] = quad[20];
            aus[29] = quad[21];
            aus[30] = quad[22];
            aus[31] = quad[23];
            aus[32] = quad[24];
            aus[33] = quad[25];
            aus[34] = quad[26];
            aus[35] = quad[27];
            aus[36] = quad[28];
            aus[37] = quad[29];
            aus[38] = quad[30];
            aus[39] = quad[31];
            aus[40] = quad[45];
            aus[41] = quad[43];
            aus[42] = quad[40];
            aus[43] = quad[46];
            aus[44] = quad[41];
            aus[45] = quad[47];
            aus[46] = quad[44];
            aus[47] = quad[42];
            aus[48] = quad[48];
            aus[49] = quad[52];
            aus[50] = quad[49];
            aus[51] = quad[50];
            aus[52] = quad[51];
            aus[53] = quad[53];

            aus.CopyTo(quad, 0);
        }

        public void Rot1(int[] quad)
        {
            var aus = new int[quad.Length];

            aus[0]  = quad[16];
            aus[1]  = quad[17];
            aus[2]  = quad[18];
            aus[3]  = quad[19];
            aus[4]  = quad[20];
            aus[5]  = quad[21];
            aus[6]  = quad[22];
            aus[7]  = quad[23];
            aus[8]  = quad[10];
            aus[9]  = quad[12];
            aus[10] = quad[15];
            aus[11] = quad[9 ];
            aus[12] = quad[14];
            aus[13] = quad[8 ];
            aus[14] = quad[11];
            aus[15] = quad[13];
            aus[16] = quad[40];
            aus[17] = quad[41];
            aus[18] = quad[42];
            aus[19] = quad[43];
            aus[20] = quad[44];
            aus[21] = quad[45];
            aus[22] = quad[46];
            aus[23] = quad[47];
            aus[24] = quad[29];
            aus[25] = quad[27];
            aus[26] = quad[24];
            aus[27] = quad[30];
            aus[28] = quad[25];
            aus[29] = quad[31];
            aus[30] = quad[28];
            aus[31] = quad[26];
            aus[32] = quad[7 ];
            aus[33] = quad[6 ];
            aus[34] = quad[5 ];
            aus[35] = quad[4 ];
            aus[36] = quad[3 ];
            aus[37] = quad[2 ];
            aus[38] = quad[1 ];
            aus[39] = quad[0 ];
            aus[40] = quad[39];
            aus[41] = quad[38];
            aus[42] = quad[37];
            aus[43] = quad[36];
            aus[44] = quad[35];
            aus[45] = quad[34];
            aus[46] = quad[33];
            aus[47] = quad[32];
            aus[48] = quad[50];
            aus[49] = quad[49];
            aus[50] = quad[53];
            aus[51] = quad[51];
            aus[52] = quad[48];
            aus[53] = quad[52];

            aus.CopyTo(quad, 0);
        }

        public void Rot2(int[] quad)
        {
            var aus = new int[quad.Length];

            aus[0]  = quad[13];
            aus[1]  = quad[11];
            aus[2]  = quad[8 ];
            aus[3]  = quad[14];
            aus[4]  = quad[9 ];
            aus[5]  = quad[15];
            aus[6]  = quad[12];
            aus[7]  = quad[10];
            aus[8]  = quad[45];
            aus[9]  = quad[43];
            aus[10] = quad[40];
            aus[11] = quad[46];
            aus[12] = quad[41];
            aus[13] = quad[47];
            aus[14] = quad[44];
            aus[15] = quad[42];
            aus[16] = quad[21];
            aus[17] = quad[19];
            aus[18] = quad[16];
            aus[19] = quad[22];
            aus[20] = quad[17];
            aus[21] = quad[23];
            aus[22] = quad[20];
            aus[23] = quad[18];
            aus[24] = quad[5 ];
            aus[25] = quad[3 ];
            aus[26] = quad[0 ];
            aus[27] = quad[6 ];
            aus[28] = quad[1 ];
            aus[29] = quad[7 ];
            aus[30] = quad[4 ];
            aus[31] = quad[2 ];
            aus[32] = quad[34];
            aus[33] = quad[36];
            aus[34] = quad[39];
            aus[35] = quad[33];
            aus[36] = quad[38];
            aus[37] = quad[32];
            aus[38] = quad[35];
            aus[39] = quad[37];
            aus[40] = quad[29];
            aus[41] = quad[27];
            aus[42] = quad[24];
            aus[43] = quad[30];
            aus[44] = quad[25];
            aus[45] = quad[31];
            aus[46] = quad[28];
            aus[47] = quad[26];
            aus[48] = quad[49];
            aus[49] = quad[53];
            aus[50] = quad[50];
            aus[51] = quad[48];
            aus[52] = quad[52];
            aus[53] = quad[51];

            aus.CopyTo(quad, 0);
        }

        #endregion

        #endregion


        #region Scansione

        private void BtnIstantanea_Click(object sender, RoutedEventArgs e)
        {
            ImgAcquisitaBig.Source = ImgAcquisitaSmall.Source;
        }

        #region Webcam

        /// <summary>
        /// Evento alla selezione di una nuova webcam dalla combobox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbAvaibleCameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try { SelectedCamera.Stop(); }
            catch { /*Eccezione ignorata*/ }

            SelectedCamera = new VideoCaptureDevice(AvaibleCameras[CmbAvaibleCameras.SelectedIndex].MonikerString);
            SelectedCamera.VideoResolution = SelectedCamera.VideoCapabilities[4];

            SelectedCamera.NewFrame += SelectedCamera_NewFrame;
            SelectedCamera.Start();
        }

        /// <summary>
        /// Evento all'acquisizione di un nuovo fotogramma dalla webcam selezionata
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void SelectedCamera_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                BitmapImage img = GeneralMethods.BitmapToBitmapImage((Bitmap)eventArgs.Frame.Clone());
                Dispatcher.BeginInvoke(new ThreadStart(delegate { ImgAcquisitaSmall.Source = img; }));
            }
            catch { /*Eccezione ignorata*/ }
        }

        /// <summary>
        /// Per aggiornare la lista delle webcam disponbili e caricarle in una combobox
        /// </summary>
        /// <param name="cmb">ComboBox in cui caricare le webcam disponibili</param>
        private void AggiornaCamereDispoibili(ComboBox cmb)
        {
            AvaibleCameras = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            cmb.Items.Clear();
            foreach (FilterInfo cam in AvaibleCameras)
                cmb.Items.Add(cam.Name);
        }

        #endregion

        #region Picker

        /// <summary>
        /// Evento al click del bottone "Auto griglia"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAutoGriglia_Click(object sender, RoutedEventArgs e) => AutoGriglia ^= true;


        /// <summary>
        /// Evento al <see cref="Button.PreviewMouseDown"/> dei Picker per memorizzare la posizione relativa del cursore rispetto all'oggetto attuale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPickerW_Down(object sender, MouseButtonEventArgs e)
        {
            //Si memorizza la posizione relativa del cursore rispetto all'oggetto attuale nella variabile globale
            PickerRelativePosition = e.GetPosition(sender as Button);
        }

        /// <summary>
        /// Evento al movimento dei Picker sopra l'immagine nella schermata scansione. Sposta i Picker al cursore
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPickerW_Move(object sender, MouseEventArgs e)
        {
            //Si esegue l'evento solo nel caso sia premuto il tasto sinistro
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var picker = (Button)sender;                            //Oggetto Picker appena trascinato
            var posAbsolute = e.GetPosition(PickerContainer);       //Posizione assoluta rispetto al contenitore del Picker appena trascinato

            //Due gestioni diverse in caso sia attiva l'autogriglia o meno
            if (AutoGriglia)
            {
                //Sposta tutti i picker interessati in base alla riga e colonna
                PickerSpostaRiga(
                    Convert.ToInt16(picker.Name.Substring(picker.Name.Length - 2, 1)),
                    posAbsolute);
                PickerSpostaColonna(
                    Convert.ToInt16(picker.Name.Substring(picker.Name.Length - 1)),
                    posAbsolute);
            }
            else
            {
                //Si settano le coordinate X e Y del Picker per spostarlo sotto al cursore. Si sottrae alla posizione assoluta
                //quella relativa del cursore rispetto all'oggetto attuale per mantenerlo in linea col cursore
                Canvas.SetLeft(picker, posAbsolute.X - PickerRelativePosition.X);
                Canvas.SetTop(picker, posAbsolute.Y - PickerRelativePosition.Y);
            }

            //TODO: Aggiungere la modificazione del BorderBrush del controllo attuale in base al colore letto alla posizione attuale
        }


        /// <summary>
        /// Per spostare una colonna di Picker
        /// </summary>
        /// <param name="col">Il numero della colonna da spostare</param>
        /// <param name="mousePos">Posizione attuale del mouse relativa al contenitore</param>
        private void PickerSpostaColonna(int col, Point mousePos)
        {
            //Nuova X dei picker dipendenti dal picker attuale
            double xCenter = (Canvas.GetLeft((UIElement)PickerContainer.FindName("Picker10")) + Canvas.GetLeft((UIElement)PickerContainer.FindName("Picker12"))) / 2;
            //Si spostano tutti i picker necessari (X)
            for (var i = 0; i <= 2; i++)
            {
                //Si sposta il picker attuale (X)
                Canvas.SetLeft(
                    (UIElement)PickerContainer.FindName($"Picker{i}{col}"),
                    mousePos.X - PickerRelativePosition.X);

                //Si spostano i picker dipendenti (Colonna, X)
                Canvas.SetLeft(
                    (UIElement)PickerContainer.FindName($"Picker{i}1"),
                    xCenter);
            }
        }

        /// <summary>
        /// Per spostare una riga di Picker
        /// </summary>
        /// <param name="riga">Il numero della riga da spostare</param>
        /// <param name="mousePos">Posizione attuale del mouse relativa al contenitore</param>
        private void PickerSpostaRiga(int riga, Point mousePos)
        {
            //Nuova Y dei picker dipendenti dal picker attuale
            double yCenter = (Canvas.GetTop((UIElement)PickerContainer.FindName($"Picker01")) + Canvas.GetTop((UIElement)PickerContainer.FindName($"Picker21"))) / 2;
            //Si spostano tutti i picker necessari (Y)
            for (var j = 0; j <= 2; j++)
            {
                //Si sposta il picker attuale (Y)
                Canvas.SetTop(
                    (UIElement)PickerContainer.FindName($"Picker{riga}{j}"),
                    mousePos.Y - PickerRelativePosition.Y);

                //Si spostano i picker dipendenti (Riga, Y)
                Canvas.SetTop(
                    (UIElement)PickerContainer.FindName($"Picker1{j}"),
                    yCenter);
            }
        }

        #endregion

        #endregion


        #region Comunicazione

        public void AggiornaSeriali()
        {
            SerialCom?.Close();

            foreach (var porta in SerialPort.GetPortNames())
                CmbSeriale.Items.Add(porta);

            try
            {
                CmbSeriale.SelectedIndex = 0;
                ApriSeriale(ref SerialCom, CmbSeriale.SelectedItem.ToString());
            }
            catch { /* Eccezione ignorata */ }
        }

        
        public void ApriSeriale(ref SerialPort serialPort, string name)
        {
            serialPort?.Close();

            serialPort = new SerialPort
            {
                PortName  = name,
                BaudRate  = BAUD_RATE,
                DataBits  = 8,
                Parity    = Parity.None,
                StopBits  = StopBits.One,
                Handshake = Handshake.None,
                Encoding  = Encoding.Default
            };
            serialPort.Open();
        }

        #endregion


        private void BtnScansiona_Click(object sender, RoutedEventArgs e)
        {
            FacciaScan = 6;
            //SerialCom.Write("jm");
            TmrScansiona.Start();
        }


        private void TmrScansiona_Tick(object sender, EventArgs e)
        {
            /*if (FacciaScan != 0)
            {
                ImgAcquisitaBig.Source = ImgAcquisitaSmall.Source;
                GeneralMethods.ImageSourceToBitmap(ImgAcquisitaBig.Source).Save($"Images/{FacciaScan}.jpg", ImageFormat.Jpeg);
            }*/

            switch (FacciaScan)
            {
                case 0:
                    SerialCom.Write("jm"); break;       //w
                case 1:
                    SerialCom.Write("ilmo"); break;     //x
                case 2:
                    SerialCom.Write("kjom"); break;     //y
                case 3:
                    SerialCom.Write("ilmo"); break;
                case 4:
                    SerialCom.Write("kjom"); break;
                case 5:
                    SerialCom.Write("ilmo"); break;     //z
                case 6:
                    SerialCom.Write("kjoi"); TmrScansiona.Stop(); Analizza(); break;  //z
            }

            FacciaScan++;
        }

        private void Analizza()
        {
            ColoriPrincipali();
            AddCentri();
            AddRossiArancioni();
        }

        private void AddRossiArancioni()
        {
            var leggi = File.ReadAllLines("Angoli_RA.txt");

            var out1 = 0;
            var riga = 0;
            var sep = '/';
            string[] blocco;
            var output = "";
            var bypass = false;
            Button a, b, c;

            //------------------------------------------------------------------------------------------//
            {
                a = CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F1, 2, 2);
                b = CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F3, 0, 2);
                c = CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F4, 0, 0);
                if (a.Background == CuboMetods.Colors.Empty)
                    out1 = CuboMetods.NumberFromColor((SolidColorBrush)b.Background) +
                           CuboMetods.NumberFromColor((SolidColorBrush)c.Background);
                else if (b.Background == CuboMetods.Colors.Empty)
                    out1 = CuboMetods.NumberFromColor((SolidColorBrush)c.Background) +
                           CuboMetods.NumberFromColor((SolidColorBrush)a.Background);
                else if (c.Background == CuboMetods.Colors.Empty)
                    out1 = CuboMetods.NumberFromColor((SolidColorBrush)a.Background) +
                           CuboMetods.NumberFromColor((SolidColorBrush)b.Background);
                else
                    bypass = true;

                if (!bypass)
                {
                    riga = Cerca(leggi, out1.ToString());
                    blocco = leggi[riga].Split(sep);
                    if (a.Background == CuboMetods.Colors.Empty)
                        a.Background = CuboMetods.ColorFromNumber(Convert.ToInt32(blocco[1]));
                    else if (b.Background == CuboMetods.Colors.Empty)
                        b.Background = CuboMetods.ColorFromNumber(Convert.ToInt32(blocco[1]));
                    else if (c.Background == CuboMetods.Colors.Empty)
                        c.Background = CuboMetods.ColorFromNumber(Convert.ToInt32(blocco[1]));
                }
            }

            //------------------------------------------------------------------------------------------//
            {
                a = CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F1, 2, 0);
                b = CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F2, 0, 2);
                c = CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F3, 0, 0);
                if (a.Background == CuboMetods.Colors.Empty)
                    out1 = CuboMetods.NumberFromColor((SolidColorBrush)b.Background) +
                           CuboMetods.NumberFromColor((SolidColorBrush)c.Background);
                else if (b.Background == CuboMetods.Colors.Empty)
                    out1 = CuboMetods.NumberFromColor((SolidColorBrush)c.Background) +
                           CuboMetods.NumberFromColor((SolidColorBrush)a.Background);
                else if (c.Background == CuboMetods.Colors.Empty)
                    out1 = CuboMetods.NumberFromColor((SolidColorBrush)a.Background) +
                           CuboMetods.NumberFromColor((SolidColorBrush)b.Background);
                else
                    bypass = true;

                if (!bypass)
                {
                    riga = Cerca(leggi, out1.ToString());
                    blocco = leggi[riga].Split(sep);
                    if (a.Background == CuboMetods.Colors.Empty)
                        a.Background = CuboMetods.ColorFromNumber(Convert.ToInt32(blocco[1]));
                    else if (b.Background == CuboMetods.Colors.Empty)
                        b.Background = CuboMetods.ColorFromNumber(Convert.ToInt32(blocco[1]));
                    else if (c.Background == CuboMetods.Colors.Empty)
                        c.Background = CuboMetods.ColorFromNumber(Convert.ToInt32(blocco[1]));
                }
            }

            //------------------------------------------------------------------------------------------//
            {
                a = CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F1, 0, 0);
                b = CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F5, 0, 2);
                c = CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F2, 0, 0);
                if (a.Background == CuboMetods.Colors.Empty)
                    out1 = CuboMetods.NumberFromColor((SolidColorBrush)b.Background) +
                           CuboMetods.NumberFromColor((SolidColorBrush)c.Background);
                else if (b.Background == CuboMetods.Colors.Empty)
                    out1 = CuboMetods.NumberFromColor((SolidColorBrush)c.Background) +
                           CuboMetods.NumberFromColor((SolidColorBrush)a.Background);
                else if (c.Background == CuboMetods.Colors.Empty)
                    out1 = CuboMetods.NumberFromColor((SolidColorBrush)a.Background) +
                           CuboMetods.NumberFromColor((SolidColorBrush)b.Background);
                else
                    bypass = true;

                if (!bypass)
                {
                    riga = Cerca(leggi, out1.ToString());
                    blocco = leggi[riga].Split(sep);
                    if (a.Background == CuboMetods.Colors.Empty)
                        a.Background = CuboMetods.ColorFromNumber(Convert.ToInt32(blocco[1]));
                    else if (b.Background == CuboMetods.Colors.Empty)
                        b.Background = CuboMetods.ColorFromNumber(Convert.ToInt32(blocco[1]));
                    else if (c.Background == CuboMetods.Colors.Empty)
                        c.Background = CuboMetods.ColorFromNumber(Convert.ToInt32(blocco[1]));
                }
            }

            //------------------------------------------------------------------------------------------//
            {
                a = CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F1, 0, 0);
                b = CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F5, 0, 2);
                c = CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F2, 0, 0);
                if (a.Background == CuboMetods.Colors.Empty)
                    out1 = CuboMetods.NumberFromColor((SolidColorBrush)b.Background) +
                           CuboMetods.NumberFromColor((SolidColorBrush)c.Background);
                else if (b.Background == CuboMetods.Colors.Empty)
                    out1 = CuboMetods.NumberFromColor((SolidColorBrush)c.Background) +
                           CuboMetods.NumberFromColor((SolidColorBrush)a.Background);
                else if (c.Background == CuboMetods.Colors.Empty)
                    out1 = CuboMetods.NumberFromColor((SolidColorBrush)a.Background) +
                           CuboMetods.NumberFromColor((SolidColorBrush)b.Background);
                else
                    bypass = true;

                if (!bypass)
                {
                    riga = Cerca(leggi, out1.ToString());
                    blocco = leggi[riga].Split(sep);
                    if (a.Background == CuboMetods.Colors.Empty)
                        a.Background = CuboMetods.ColorFromNumber(Convert.ToInt32(blocco[1]));
                    else if (b.Background == CuboMetods.Colors.Empty)
                        b.Background = CuboMetods.ColorFromNumber(Convert.ToInt32(blocco[1]));
                    else if (c.Background == CuboMetods.Colors.Empty)
                        c.Background = CuboMetods.ColorFromNumber(Convert.ToInt32(blocco[1]));
                }
            }
        }

        private void ColoriPrincipali()
        {
            string[] celle = new string[9];
            var filtroL = new BrightnessCorrection((int)NumL.Value);
            var filtroC = new ContrastCorrection  ((int)NumC.Value);
            var filtroS = new SaturationCorrection((int)NumS.Value);
            Bitmap imgBig;


            //Parte 1
            ImgAcquisitaBig.Source = new BitmapImage(new Uri("Images/1.jpg", UriKind.Relative));
            imgBig = (Bitmap)GeneralMethods.ImageSourceToBitmap(new BitmapImage(new Uri("Images/1.jpg", UriKind.Relative)));
            filtroL.ApplyInPlace(imgBig);
            filtroC.ApplyInPlace(imgBig);
            filtroS.ApplyInPlace(imgBig);

            ImgAcquisitaBig.Source = GeneralMethods.BitmapToBitmapImage(imgBig);

            Pass1();
            Pass2();
            Riconosci(celle);
            if (celle[0] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F5, 2, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[0]));
            if (celle[1] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F5, 2, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[1]));
            if (celle[2] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F5, 2, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[2]));
            if (celle[3] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F5, 1, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[3]));
            if (celle[4] == "2" ||
                celle[4] == "5") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F5, 1, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[4]));
            if (celle[5] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F5, 1, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[5]));
            if (celle[6] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F5, 0, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[6]));
            if (celle[7] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F5, 0, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[7]));
            if (celle[8] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F5, 2, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[8]));


            //Parte 2
            ImgAcquisitaBig.Source = new BitmapImage(new Uri("Images/2.jpg", UriKind.Relative));
            imgBig = (Bitmap)GeneralMethods.ImageSourceToBitmap(new BitmapImage(new Uri("Images/2.jpg", UriKind.Relative)));
            filtroL.ApplyInPlace(imgBig);
            filtroC.ApplyInPlace(imgBig);
            filtroS.ApplyInPlace(imgBig);

            ImgAcquisitaBig.Source = GeneralMethods.BitmapToBitmapImage(imgBig);

            Pass1();
            Pass2();
            Riconosci(celle);
            if (celle[0] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F4, 2, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[0]));
            if (celle[1] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F4, 2, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[1]));
            if (celle[2] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F4, 2, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[2]));
            if (celle[3] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F4, 1, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[3]));
            if (celle[4] == "2" ||
                celle[4] == "5") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F4, 1, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[4]));
            if (celle[5] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F4, 1, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[5]));
            if (celle[6] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F4, 0, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[6]));
            if (celle[7] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F4, 0, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[7]));
            if (celle[8] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F4, 0, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[8]));


            //Parte 3
            ImgAcquisitaBig.Source = new BitmapImage(new Uri("Images/3.jpg", UriKind.Relative));
            imgBig = (Bitmap)GeneralMethods.ImageSourceToBitmap(new BitmapImage(new Uri("Images/3.jpg", UriKind.Relative)));
            filtroL.ApplyInPlace(imgBig);
            filtroC.ApplyInPlace(imgBig);
            filtroS.ApplyInPlace(imgBig);

            ImgAcquisitaBig.Source = GeneralMethods.BitmapToBitmapImage(imgBig);

            Pass1();
            Pass2();
            Riconosci(celle);
            if (celle[0] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F6, 2, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[0]));
            if (celle[1] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F6, 1, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[1]));
            if (celle[2] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F6, 0, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[2]));
            if (celle[3] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F6, 2, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[3]));
            if (celle[4] == "2" ||
                celle[4] == "5") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F6, 0, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[4]));
            if (celle[5] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F6, 2, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[5]));
            if (celle[6] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F6, 2, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[6]));
            if (celle[7] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F6, 1, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[7]));
            if (celle[8] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F6, 0, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[8]));


            //Parte 4
            ImgAcquisitaBig.Source = new BitmapImage(new Uri("Images/4.jpg", UriKind.Relative));
            imgBig = (Bitmap)GeneralMethods.ImageSourceToBitmap(new BitmapImage(new Uri("Images/4.jpg", UriKind.Relative)));
            filtroL.ApplyInPlace(imgBig);
            filtroC.ApplyInPlace(imgBig);
            filtroS.ApplyInPlace(imgBig);

            ImgAcquisitaBig.Source = GeneralMethods.BitmapToBitmapImage(imgBig);

            Pass1();
            Pass2();
            Riconosci(celle);
            if (celle[0] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F3, 2, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[0]));
            if (celle[1] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F3, 1, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[1]));
            if (celle[2] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F3, 0, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[2]));
            if (celle[3] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F3, 2, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[3]));
            if (celle[4] == "2" ||
                celle[4] == "5") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F3, 0, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[4]));
            if (celle[5] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F3, 0, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[5]));
            if (celle[6] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F3, 2, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[6]));
            if (celle[7] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F3, 2, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[7]));
            if (celle[8] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F3, 0, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[8]));


            //Parte 5
            ImgAcquisitaBig.Source = new BitmapImage(new Uri("Images/5.jpg", UriKind.Relative));
            imgBig = (Bitmap)GeneralMethods.ImageSourceToBitmap(new BitmapImage(new Uri("Images/5.jpg", UriKind.Relative)));
            filtroL.ApplyInPlace(imgBig);
            filtroC.ApplyInPlace(imgBig);
            filtroS.ApplyInPlace(imgBig);

            ImgAcquisitaBig.Source = GeneralMethods.BitmapToBitmapImage(imgBig);

            Pass1();
            Pass2();
            Riconosci(celle);
            if (celle[0] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F2, 2, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[0]));
            if (celle[1] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F2, 1, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[1]));
            if (celle[2] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F2, 0, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[2]));
            if (celle[3] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F2, 2, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[3]));
            if (celle[4] == "2" ||
                celle[4] == "5") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F2, 1, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[4]));
            if (celle[5] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F2, 0, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[5]));
            if (celle[6] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F2, 2, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[6]));
            if (celle[7] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F2, 2, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[7]));
            if (celle[8] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F2, 0, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[8]));


            //Parte 6
            ImgAcquisitaBig.Source = new BitmapImage(new Uri("Images/6.jpg", UriKind.Relative));
            imgBig = (Bitmap)GeneralMethods.ImageSourceToBitmap(new BitmapImage(new Uri("Images/6.jpg", UriKind.Relative)));
            filtroL.ApplyInPlace(imgBig);
            filtroC.ApplyInPlace(imgBig);
            filtroS.ApplyInPlace(imgBig);

            ImgAcquisitaBig.Source = GeneralMethods.BitmapToBitmapImage(imgBig);

            Pass1();
            Pass2();
            Riconosci(celle);
            if (celle[0] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F1, 0, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[0]));
            if (celle[1] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F1, 0, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[1]));
            if (celle[2] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F1, 0, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[2]));
            if (celle[3] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F1, 1, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[3]));
            if (celle[4] == "2" ||
                celle[4] == "5") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F1, 1, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[4]));
            if (celle[5] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F1, 1, 2).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[5]));
            if (celle[6] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F1, 2, 0).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[6]));
            if (celle[7] != "0") CuboMetods.GetEdge(Cube, CuboMetods.Faccia.F1, 2, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(celle[7]));
        }

        private void AddCentri()
        {
            var output1 = "";

            var riga = 0;
            string[] blocco;

            var centri = File.ReadAllLines("Center_library.txt");

            for (var i = 1; i <=6; i++)
            {
                var back = CuboMetods.GetEdge(Cube, i, 1, 1);
                if (!(back.Background == CuboMetods.Colors.Giallo || back.Background == CuboMetods.Colors.Verde))
                    back.Background = CuboMetods.Colors.Empty;

                output1 += Convert.ToString(CuboMetods.NumberFromColor((SolidColorBrush)back.Background));
            }

            riga = Cerca(centri, output1);
            try
            {
                blocco = centri[riga].Split('/');

                for (var i = 1; i <= 6; i++)
                    CuboMetods.GetEdge(Cube, i, 1, 1).Background = CuboMetods.ColorFromNumber(Convert.ToInt32(blocco[i]));
            }
            catch
            { }
        }


        private void Pass1()
        {
            var bmSource = (Bitmap)GeneralMethods.ImageSourceToBitmap(ImgAcquisitaBig.Source);
            var bmDest   = new Bitmap((int)(bmSource.Width / 0.5), (int)(bmSource.Height / 0.5));
            var grDest   = Graphics.FromImage(bmDest);
            grDest.DrawImage(bmSource, 0, 0, bmDest.Width + 1, bmDest.Height + 1);

            ImgAcquisitaBig.Source = GeneralMethods.BitmapToBitmapImage(bmSource);
        }

        private void Pass2()
        {
            var bmSource = (Bitmap)GeneralMethods.ImageSourceToBitmap(ImgAcquisitaBig.Source);
            var bmDest   = new Bitmap(bmSource.Width, bmSource.Height);
            var grDest   = Graphics.FromImage(bmDest);
            grDest.DrawImage(bmSource, 0, 0, bmDest.Width + 1, bmDest.Height + 1);

            ImgAcquisitaBig.Source = GeneralMethods.BitmapToBitmapImage(bmSource);
        }

        private void Riconosci(string[] celle)
        {
            var                  img       = (Bitmap)GeneralMethods.ImageSourceToBitmap(ImgAcquisitaBig.Source);
            System.Drawing.Color colore;
            var                  tentativi = 0;

            int R     , G     , B     ,
                Rmedio, Gmedio, Bmedio;

            

            Inizio:

            R = G = B = Rmedio = Gmedio = Bmedio = 0;

            Point[] pickerPos =
            {
                new Point(Canvas.GetLeft(Picker00), Canvas.GetTop(Picker00)),
                new Point(Canvas.GetLeft(Picker00), Canvas.GetTop(Picker01)),
                new Point(Canvas.GetLeft(Picker00), Canvas.GetTop(Picker02)),

                new Point(Canvas.GetLeft(Picker00), Canvas.GetTop(Picker10)),
                new Point(Canvas.GetLeft(Picker00), Canvas.GetTop(Picker11)),
                new Point(Canvas.GetLeft(Picker00), Canvas.GetTop(Picker12)),

                new Point(Canvas.GetLeft(Picker00), Canvas.GetTop(Picker20)),
                new Point(Canvas.GetLeft(Picker00), Canvas.GetTop(Picker21)),
                new Point(Canvas.GetLeft(Picker00), Canvas.GetTop(Picker22)),
            };

            for (var i = 0; i < pickerPos.Length; i++)
            {
                colore = img.GetPixel((int)pickerPos[i].X, (int)pickerPos[i].Y);
                R = colore.R;
                G = colore.G;
                B = colore.B;

                colore = img.GetPixel((int)pickerPos[i].X + 4, (int)pickerPos[i].Y);
                R += colore.R;
                G += colore.G;
                B += colore.B;

                colore = img.GetPixel((int)pickerPos[i].X + 8, (int)pickerPos[i].Y);
                R += colore.R;
                G += colore.G;
                B += colore.B;

                colore = img.GetPixel((int)pickerPos[i].X, (int)pickerPos[i].Y + 4);
                R += colore.R;
                G += colore.G;
                B += colore.B;

                colore = img.GetPixel((int)pickerPos[i].X + 4, (int)pickerPos[i].Y + 4);
                R += colore.R;
                G += colore.G;
                B += colore.B;

                colore = img.GetPixel((int)pickerPos[i].X + 8, (int)pickerPos[i].Y + 4);
                R += colore.R;
                G += colore.G;
                B += colore.B;

                colore = img.GetPixel((int)pickerPos[i].X, (int)pickerPos[i].Y + 8);
                R += colore.R;
                G += colore.G;
                B += colore.B;

                colore = img.GetPixel((int)pickerPos[i].X + 4, (int)pickerPos[i].Y + 8);
                R += colore.R;
                G += colore.G;
                B += colore.B;

                colore = img.GetPixel((int)pickerPos[i].X + 8, (int)pickerPos[i].Y + 8);
                R += colore.R;
                G += colore.G;
                B += colore.B;

                Rmedio = R / 9;
                Gmedio = G / 9;
                Bmedio = B / 9;

                string[] transizioni = new string[2];
                int ris;

                ris = Rmedio - Gmedio;

                if (-60 <= ris && ris <= 60)
                    transizioni[0] = "=";
                else if (255 >= ris && ris > 60)
                    transizioni[0] = "-H";
                else if (-60 > ris && ris >= -255)
                    transizioni[0] = "+H";

                ris = Gmedio - Bmedio;

                if (-50 <= ris && ris <= 50)
                    transizioni[1] = "=";
                else if (255 >= ris && ris > 50)
                    transizioni[1] = "-H";
                else if (-50 > ris && ris >= -255)
                    transizioni[1] = "+H";

                var andamento = $"{transizioni[0]}/{transizioni[1]}";

                switch (andamento)
                {
                    case "-H/-H":
                        celle[i] = "0"; break;
                    case "-H/=":
                        celle[i] = "0"; break;
                    case "-H/+H":
                        celle[i] = "0"; break;
                    case "=/-H":
                        celle[i] = "2"; break;
                    case "=/=":
                        if (Rmedio < 20) celle[i] = "6";
                        else celle[i] = "1";
                        break;
                    case "=/+H":
                        if (Rmedio < 60) celle[i] = "6";
                        else celle[i] = "1";
                        break;
                    case "+H/-H":
                        if (Rmedio > 100) celle[i] = "2";
                        else celle[i] = "5";
                        break;
                    case "+H/=":
                        celle[i] = "5"; break;
                    case "+H/+H":
                        celle[i] = "6"; break;
                }
            }

            for (var i = 0; i < pickerPos.Length; i++)
            {
                if (celle[i] == "")
                {
                    var bmp = (Bitmap)GeneralMethods.ImageSourceToBitmap(ImgAcquisitaBig.Source);

                    var filterL = new BrightnessCorrection(20);
                    filterL.ApplyInPlace(bmp);

                    var filterC = new ContrastCorrection(50);
                    filterC.ApplyInPlace(bmp);

                    ImgAcquisitaBig.Source = GeneralMethods.BitmapToBitmapImage(bmp);
                    ImgAcquisitaBig.InvalidateVisual();

                    tentativi++;

                    if(tentativi == 4)
                    {
                        MessageBox.Show("Non è stato possibile riconoscere i colori");
                        break;
                    }
                    goto Inizio;
                }
            }
        }


        private int Cerca(string[] v, string cerca)
        {
            for (var i = 0; i < v.Length; i++)
                if (v[i].Contains(cerca))
                    return i;

            return -1;
        }



        private void CreaQuadretto(int[] quadretto)
        {
            var faccia = 1;
            for (var i = 0; i < quadretto.Length - 6; i += 8)
            {
                quadretto[i] = CuboMetods.NumberFromColor((SolidColorBrush)CuboMetods.GetEdge(Cube, faccia, 0, 0).Background);
                quadretto[i + 1] = CuboMetods.NumberFromColor((SolidColorBrush)CuboMetods.GetEdge(Cube, faccia, 0, 1).Background);
                quadretto[i + 2] = CuboMetods.NumberFromColor((SolidColorBrush)CuboMetods.GetEdge(Cube, faccia, 0, 2).Background);

                quadretto[i + 3] = CuboMetods.NumberFromColor((SolidColorBrush)CuboMetods.GetEdge(Cube, faccia, 1, 0).Background);
                //quadretto[i] = CuboMetods.NumberFromColor((SolidColorBrush)CuboMetods.GetEdge(Cube, faccia, 1, 1).Background);
                quadretto[i + 4] = CuboMetods.NumberFromColor((SolidColorBrush)CuboMetods.GetEdge(Cube, faccia, 1, 2).Background);

                quadretto[i + 5] = CuboMetods.NumberFromColor((SolidColorBrush)CuboMetods.GetEdge(Cube, faccia, 2, 0).Background);
                quadretto[i + 6] = CuboMetods.NumberFromColor((SolidColorBrush)CuboMetods.GetEdge(Cube, faccia, 2, 1).Background);
                quadretto[i + 7] = CuboMetods.NumberFromColor((SolidColorBrush)CuboMetods.GetEdge(Cube, faccia++, 2, 2).Background);
            }

            faccia = 1;
            for (var i = 48; i < quadretto.Length; i++)
                quadretto[i] = CuboMetods.NumberFromColor((SolidColorBrush)CuboMetods.GetEdge(Cube, faccia++, 1, 1).Background);
        }

        private void BtnSimulazione_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}