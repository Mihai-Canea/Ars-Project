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
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace cube3D_WPF
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //--------------------------------------------------------------------------------------------------------------------------------

        Point startMoveCamera;
        bool allowMoveCamera = false, allowMoveLayer = false, gameOver = false;
        int size = 3;                                                           //istanziamo la dimensione di un lato del cubo
        double edge_len = 1;                                                    // fuoco della rotazione
        double space = 0.1;                                                     // spessore bordo del cubo
        double len;
        int delay;                                                              //delay per le rotazioni

        Transform3DGroup rotations = new Transform3DGroup();
        RubikCube c;
        MyModelVisual3D touchFaces;
        Movimenti movement = new Movimenti();
        HashSet<string> touchedFaces = new HashSet<string>();

        List<KeyValuePair<Move, RotationDirection>> doneMoves = new List<KeyValuePair<Move, RotationDirection>>();
        InputOutput IO;


        //--------------------------------------------------------------------------------------------------------------------------------

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double distanceFactor = 2;
            len = edge_len * size + space * (size - 1);

            IO = new InputOutput(size);

            Point3D cameraPos = new Point3D(len * distanceFactor, len * distanceFactor, len * distanceFactor);
            PerspectiveCamera camera = new PerspectiveCamera(
                cameraPos,
                new Vector3D(-cameraPos.X, -cameraPos.Y, -cameraPos.Z),
                new Vector3D(0, 1, 0),
                45
            );

            this.mainViewport.Camera = camera;
        }

        //--------------------------------------------------------------------------------------------------------------------------------

        private void init(string file = null)
        {
            this.mainViewport.Children.Remove(c);
            this.mainViewport.Children.Remove(touchFaces);

            rotations.Children.Clear();
            doneMoves.Clear();

            //risolviMenu.IsEnabled = false;

            if (file != null)
            {
                c = new RubikCube(IO.read(file), size, new Point3D(-len / 2, -len / 2, -len / 2), TimeSpan.FromMilliseconds(370), edge_len, space);
            }
            else
            {
                c = new RubikCube(size, new Point3D(-len / 2, -len / 2, -len / 2), TimeSpan.FromMilliseconds(370), edge_len, space);
            }

            c.Transform = rotations;

            touchFaces = Helpers.createTouchFaces(len, size, rotations,
                    new DiffuseMaterial(new SolidColorBrush(Colors.Transparent)));

            this.mainViewport.Children.Add(c);
            this.mainViewport.Children.Add(touchFaces);

            //if (!attivaAnimazioni.IsChecked)
            //{
            //    c.animationDuration = TimeSpan.FromMilliseconds(1);
            //}

            //if (file == null)
            //{
            //    scramble(25);
            //}

            gameOver = false;
            //salvaMenu.IsEnabled = true;
            //risolviMenu.IsEnabled = true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------

        private void scramble(int n)
        {
            Random r = new Random();
            RotationDirection direction;
            List<Move> moveList = new List<Move> { Move.B, Move.D, Move.E, Move.F, Move.L, Move.M, Move.R, Move.S, Move.U };
            List<KeyValuePair<Move, RotationDirection>> moves = new List<KeyValuePair<Move, RotationDirection>>();

            for (int i = 0; i < n; i++)
            {
                int index = r.Next(0, moveList.Count);

                if (r.Next(0, 101) == 0)
                {
                    direction = RotationDirection.ClockWise;
                }
                else
                {
                    direction = RotationDirection.CounterClockWise;
                }

                Debug.Print("Move: {0} {1}", moveList[index].ToString(), direction.ToString());

                moves.Add(new KeyValuePair<Move, RotationDirection>(moveList[index], direction));
                doneMoves.Add(new KeyValuePair<Move, RotationDirection>(moveList[index], direction));
            }
            c.rotate(moves);
        }

        private void orientaFile(string fileName)
        {
            StreamReader sr = new StreamReader(fileName);
            StreamWriter sw = new StreamWriter("appoggio.cst");
            int[] v = new int[54];
            int[] v1 = new int[54];

            int i = 0;
            while (sr.Peek()!=-1)
            {
                v[i] = Convert.ToInt16(sr.ReadLine());
                i++;
            }
            /*questo commento serve a distanziare le 2 fasi*/
            // faccia 1
            v1[0] = v[38];
            v1[1] = v[37];
            v1[2] = v[36];
            v1[3] = v[41];
            v1[4] = v[40];
            v1[5] = v[39];
            v1[6] = v[44];
            v1[7] = v[43];
            v1[8] = v[42];
            // faccia 2
            v1[9] = v[9];
            v1[10] = v[12];
            v1[11] = v[15];
            v1[12] = v[51];
            v1[13] = v[52];
            v1[14] = v[53];
            v1[15] = v[35];
            v1[16] = v[32];
            v1[17] = v[29];
            // faccia 3
            v1[18] = v[10];
            v1[19] = v[13];
            v1[20] = v[16];
            v1[21] = v[48];
            v1[22] = v[49];
            v1[23] = v[50];
            v1[24] = v[34];
            v1[25] = v[31];
            v1[26] = v[28];
            // faccia 4
            v1[27] = v[11];
            v1[28] = v[14];
            v1[29] = v[17];
            v1[30] = v[45];
            v1[31] = v[46];
            v1[32] = v[47];
            v1[33] = v[33];
            v1[34] = v[30];
            v1[35] = v[27];
            // faccia 5
            v1[36] = v[24];
            v1[37] = v[25];
            v1[38] = v[26];
            v1[39] = v[21];
            v1[40] = v[22];
            v1[41] = v[23];
            v1[42] = v[18];
            v1[43] = v[19];
            v1[44] = v[20];
            // faccia 6
            v1[45] = v[6];
            v1[46] = v[7];
            v1[47] = v[8];
            v1[48] = v[3];
            v1[49] = v[4];
            v1[50] = v[5];
            v1[51] = v[0];
            v1[52] = v[1];
            v1[53] = v[2];

            for (int j = 0; j < 54; j++)
                sw.WriteLine(v1[j].ToString());
            sr.Close();
            sw.Close();
        }

        private void convertiFile(string fileName)
        {
            StreamReader sr = new StreamReader("appoggio.cst");
            //StreamReader sr = new StreamReader(fileName);
            StreamWriter sw = new StreamWriter("Convertito.cst");

            for (int l = 0; l < 3; l++)
            {
                for (int i = 0; i < 3; i++)
                    sw.WriteLine("0");
                for (int i = 0; i < 3; i++)
                    sw.WriteLine(sr.ReadLine());
                for (int i = 0; i < 3; i++)
                    sw.WriteLine("0");
                //sw.WriteLine();
            }
            for (int i = 0; i < 27; i++)
                sw.WriteLine(sr.ReadLine());
            for (int l = 0; l < 6; l++)
            {
                for (int i = 0; i < 3; i++)
                    sw.WriteLine("0");
                for (int i = 0; i < 3; i++)
                    sw.WriteLine(sr.ReadLine());
                for (int i = 0; i < 3; i++)
                    sw.WriteLine("0");
                //sw.WriteLine();
            }
            sr.Close();
            sw.Close();
        }

        //--------------------------------------------------------------------------------------------------------------------------------

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //init();
            string path = "../../../../ARS Studio/ARS Studio/bin/Debug/ImpostazioneCubo.txt";
            orientaFile(path);
            convertiFile("appoggio.cst");
            init("Convertito.cst");

            //StreamReader sr = new StreamReader("../../../../TwoPhaseSolver/SolverTest/bin/Debug/Soluzione.txt");
            //string[] v = null;
            //while (sr.Peek() != -1)
            //    v = sr.ReadLine().Split(' ');
            //sr.Close();

            //MetodoRotazioni(v);
        }

        private async Task MetodoRotazioni(string[] v)
        {
            //await Task.Delay(1600);
            for (int i = 0; i < v.Length; i++)
            {
                await Task.Delay(delay);
                txtMosse.Text += v[i] + " ";
                switch (v[i])
                {
                    case "L":   Ruota(Move.L);  break;
                    case "D":   Ruota(Move.D);  break;
                    case "U":   Ruota(Move.U);  break;
                    case "R":   Ruota(Move.R);  break;
                    case "F":   Ruota(Move.F);  break;
                    case "B":   Ruota(Move.B);  break;

                    case "L2":  Ruota(Move.L);  await Task.Delay(delay);    Ruota(Move.L);  break;
                    case "D2":  Ruota(Move.D);  await Task.Delay(delay);    Ruota(Move.D);  break;
                    case "U2":  Ruota(Move.U);  await Task.Delay(delay);    Ruota(Move.U);  break;
                    case "R2":  Ruota(Move.R);  await Task.Delay(delay);    Ruota(Move.R);  break;
                    case "F2":  Ruota(Move.F);  await Task.Delay(delay);    Ruota(Move.F);  break;
                    case "B2":  Ruota(Move.B);  await Task.Delay(delay);    Ruota(Move.B);  break;

                    case "L'":  RuotaInverso(Move.L);   break;
                    case "D'":  RuotaInverso(Move.D);   break;
                    case "U'":  RuotaInverso(Move.U);   break;
                    case "R'":  RuotaInverso(Move.R);   break;
                    case "F'":  RuotaInverso(Move.F);   break;
                    case "B'":  RuotaInverso(Move.B);   break;
                }
            }
        }

        //Mouse
        //--------------------------------------------------------------------------------------------------------------------------------
        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            startMoveCamera = e.GetPosition(this);
            //txtMosse.Text = e.GetPosition(this).ToString();
            allowMoveCamera = true;
            this.Cursor = Cursors.SizeAll;
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            allowMoveCamera = false;
            this.Cursor = Cursors.Arrow;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (allowMoveCamera)
            {
                moveCamera(e.GetPosition(this));
            }

            if (allowMoveLayer)
            {
                moveLayer(e.GetPosition((UIElement)sender));
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                init();
            }
        }

        private void moveCamera(Point p)
        {
            double distX = p.X - startMoveCamera.X;
            double distY = p.Y - startMoveCamera.Y;

            startMoveCamera = p;

            RotateTransform3D rotationX = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), distY), new Point3D(0, 0, 0));
            RotateTransform3D rotationY = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), distX), new Point3D(0, 0, 0));

            rotations.Children.Add(rotationX);
            rotations.Children.Add(rotationY);
        }

        private void moveLayer(Point p)
        {
            VisualTreeHelper.HitTest(this.mainViewport, null, new HitTestResultCallback(resultCb), new PointHitTestParameters(p));
        }

        private HitTestResultBehavior resultCb(HitTestResult r)
        {
            MyModelVisual3D model = r.VisualHit as MyModelVisual3D;

            if (model != null)
            {
                touchedFaces.Add(model.Tag);
            }

            return HitTestResultBehavior.Continue;
        }

        //--------------------------------------------------------------------------------------------------------------------------------

        private void btnRisoluzione_Click(object sender, RoutedEventArgs e)
        {
            delay = 400;
            txtMosse.Text = "";
            Risolvi();
        }

        private void btnRisolviLento_Click(object sender, RoutedEventArgs e)
        {
            delay = 2000;
            txtMosse.Text = "";
            Risolvi();
        }

        private void Risolvi()
        {
            StreamReader sr = new StreamReader("../../../../TwoPhaseSolver/SolverTest/bin/Debug/Soluzione.txt");
            string[] v = null;
            while (sr.Peek() != -1)
                v = sr.ReadLine().Split(' ');
            sr.Close();

            MetodoRotazioni(v);
        }

        private void btnInizio_Click(object sender, RoutedEventArgs e)
        {
            string path = "../../../../ARS Studio/ARS Studio/bin/Debug/ImpostazioneCubo.txt";
            orientaFile(path);
            convertiFile("appoggio.cst");
            init("Convertito.cst");
        }

        private void btnFaccie_Click(object sender, RoutedEventArgs e)
        {
            Point p = new Point(45, 50);
            moveCamera(p);
            RuotaFaccie();
            btnFaccie.IsEnabled = false;
        }

        private async Task RuotaFaccie()
        {
            int faccia = 1;
            txtMosse.Text = "Faccia " + faccia;
            int temp = 50;
            faccia++;
            c.animationDuration = TimeSpan.FromMilliseconds(1);
            // 1
            await Task.Delay(2000);
            await ruotaAvanti(temp);                            //ruota avanti
            // 2
            await ruotaSx(temp);                                //ruota a sinistra
            txtMosse.Text = "Faccia " + faccia;
            faccia++;

            // 3
            for (int i = 0; i < 3; i++)
            {
                await Task.Delay(2000);
                await ruotaDx(temp);                            //ruota destra
                txtMosse.Text = "Faccia " + faccia;
                faccia++;
            }
            await Task.Delay(2000);

            // 3 reverse -1
            for (int i = 0; i < 2; i++)
                await ruotaSx(temp);                            //ruota sinistra
            txtMosse.Text = "Faccia " + faccia;
            faccia++;

            // 1
            await ruotaAvanti(temp);                            //ruota avanti
            faccia++;
            await Task.Delay(3000);

            // torna a inizio
            for (int i = 0; i < 2; i++)
                await ruotaIndietro(temp);                      //ruota indieto (torniamo alla posizione originale)
            txtMosse.Text = "";
            c.animationDuration = TimeSpan.FromMilliseconds(370);
        }

        private async Task ruotaIndietro(int temp)
        {
            for (int j = 0; j < 3; j++)
            {
                await Task.Delay(temp);
                switch (j)
                {
                    case 0: RuotaInverso(Move.R); break;
                    case 1: Ruota(Move.M); break;
                    case 2: Ruota(Move.L); break;
                }
            }
        }

        private async Task ruotaDx(int temp)
        {
            for (int j = 0; j < 3; j++)
            {
                await Task.Delay(temp);
                switch (j)
                {
                    case 0: Ruota(Move.B); break;
                    case 1: RuotaInverso(Move.S); break;
                    case 2: RuotaInverso(Move.F); break;
                }
            }
        }

        private async Task ruotaSx(int temp)
        {
            for (int j = 0; j < 3; j++)
            {
                await Task.Delay(temp);
                switch (j)
                {
                    case 0: RuotaInverso(Move.B); break;
                    case 1: Ruota(Move.S); break;
                    case 2: Ruota(Move.F); break;
                }
            }
        }

        private async Task ruotaAvanti(int temp)
        {
            for (int j = 0; j < 3; j++)
            {
                await Task.Delay(temp);
                switch (j)
                {
                    case 0: Ruota(Move.R); break;
                    case 1: RuotaInverso(Move.M); break;
                    case 2: RuotaInverso(Move.L); break;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        //          SCORCIATOIE DA TASTIERA
        //--------------------------------------------------------------------------------------------------------------------------------
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.R)
                delay = 400;
            else if (e.Key == Key.L)
                delay = 2000;
            txtMosse.Text = "";
            Risolvi();
        }

        //Rotazioni
        //---------------------------------------------------------------------------------------
        private void Ruota(Move m)
        {
            RotationDirection direction;
            List<Move> moveList = new List<Move> { m };
            List<KeyValuePair<Move, RotationDirection>> moves = new List<KeyValuePair<Move, RotationDirection>>();

            direction = RotationDirection.ClockWise;
            moves.Add(new KeyValuePair<Move, RotationDirection>(moveList[0], direction));

            c.rotate(moves);
        }

        //Rotazioni inverse
        //---------------------------------------------------------------------------------------
        private void RuotaInverso(Move m)
        {
            RotationDirection direction;
            List<Move> moveList = new List<Move> { m };
            List<KeyValuePair<Move, RotationDirection>> moves = new List<KeyValuePair<Move, RotationDirection>>();

            direction = RotationDirection.CounterClockWise;
            moves.Add(new KeyValuePair<Move, RotationDirection>(moveList[0], direction));

            c.rotate(moves);
        }
    }
}
