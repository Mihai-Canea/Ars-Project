using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ARS_Studio
{
    /// <summary>
    /// Classe relativa alla gestione del cubo
    /// </summary>
    public class CuboMetods
    {
        /// <summary>
        /// Il numero della faccia
        /// </summary>
        public enum Faccia
        {
            F1,
            F2,
            F3,
            F4,
            F5,
            F6
        }

        /// <summary>
        /// Tipo di parte del cubo
        /// </summary>
        public enum Tipo
        {
            Angolo,
            Spigolo,
            Centro
        }

        /// <summary>
        /// Colori delle facce del cubo
        /// </summary>
        public enum Colori
        {
            Empty,
            Bianco,
            Giallo,
            Blu,
            Verde,
            Rosso,
            Arancione
        }

        public class Colors
        {
            public static readonly SolidColorBrush
                Empty     = Application.Current.FindResource("ColorCubeEmpty")     as SolidColorBrush,
                Bianco    = Application.Current.FindResource("ColorCubeBianco")    as SolidColorBrush,
                Giallo    = Application.Current.FindResource("ColorCubeGiallo")    as SolidColorBrush,
                Arancione = Application.Current.FindResource("ColorCubeArancione") as SolidColorBrush,
                Rosso     = Application.Current.FindResource("ColorCubeRosso")     as SolidColorBrush,
                Verde     = Application.Current.FindResource("ColorCubeVerde")     as SolidColorBrush,
                Blu       = Application.Current.FindResource("ColorCubeBlu")       as SolidColorBrush;
        }


        public static Button GetEdge(Grid grd, Faccia face, int r, int c)
        {
            if (r >= 0 && c >= 0 && r <= 2 && c <= 2)
            {
                int r1 = 0,
                    c1 = 0;

                switch (face)
                {
                    case Faccia.F1: r1 = 0; c1 = 1; break;
                    case Faccia.F2: r1 = 1; c1 = 0; break;
                    case Faccia.F3: r1 = 1; c1 = 1; break;
                    case Faccia.F4: r1 = 1; c1 = 2; break;
                    case Faccia.F5: r1 = 1; c1 = 3; break;
                    case Faccia.F6: r1 = 2; c1 = 1; break;
                    default: throw new Exception("L'elemento non è stato trovato");
                }

                return (Button)GetGridElement(
                    (Grid)GetGridElement(grd, r1, c1),
                    r, c);
            }

            throw new Exception("L'elemento non è stato trovato");
        }

        public static Button GetEdge(Grid grd, int face, int r, int c)
        {
            if (r >= 0 && c >= 0 && r <= 2 && c <= 2)
            {
                int r1 = 0,
                    c1 = 0;

                switch (face)
                {
                    case 1: r1 = 0; c1 = 1; break;
                    case 2: r1 = 1; c1 = 0; break;
                    case 3: r1 = 1; c1 = 1; break;
                    case 4: r1 = 1; c1 = 2; break;
                    case 5: r1 = 1; c1 = 3; break;
                    case 6: r1 = 2; c1 = 1; break;
                    default: throw new Exception("L'elemento non è stato trovato");
                }

                return (Button)GetGridElement(
                    (Grid)GetGridElement(grd, r1, c1),
                    r, c);
            }

            throw new Exception("L'elemento non è stato trovato");
        }


        public static UIElement GetGridElement(Grid g, int r, int c)
        {
            for (int i = 0; i < g.Children.Count; i++)
            {
                UIElement e = g.Children[i];
                if (Grid.GetRow(e) == r && Grid.GetColumn(e) == c)
                    return e;
            }
            return null;
        }

        public static Grid Face(Grid grd, Faccia face)
        {
            int r1 = 0,
                c1 = 0;
            switch (face)
            {
                case Faccia.F1: r1 = 0; c1 = 1; break;
                case Faccia.F2: r1 = 1; c1 = 0; break;
                case Faccia.F3: r1 = 1; c1 = 1; break;
                case Faccia.F4: r1 = 1; c1 = 2; break;
                case Faccia.F5: r1 = 1; c1 = 3; break;
                case Faccia.F6: r1 = 2; c1 = 1; break;
            }

            return (Grid)GetGridElement(grd, r1, c1);
        }

        public static Grid Face(Grid grd, int face)
        {
            int r1 = 0,
                c1 = 0;
            switch (face)
            {
                case 1: r1 = 0; c1 = 1; break;
                case 2: r1 = 1; c1 = 0; break;
                case 3: r1 = 1; c1 = 1; break;
                case 4: r1 = 1; c1 = 2; break;
                case 5: r1 = 1; c1 = 3; break;
                case 6: r1 = 2; c1 = 1; break;
                default: throw new Exception("La faccia non esiste");
            }

            return (Grid)GetGridElement(grd, r1, c1);
        }

        public static void AzzeraColori(Grid grd)
        {
            foreach (Button but in Face(grd, Faccia.F1).Children)
                but.Background = Colors.Bianco;

            foreach (Button but in Face(grd, Faccia.F2).Children)
                but.Background = Colors.Arancione;

            foreach (Button but in Face(grd, Faccia.F3).Children)
                but.Background = Colors.Verde;

            foreach (Button but in Face(grd, Faccia.F4).Children)
                but.Background = Colors.Rosso;

            foreach (Button but in Face(grd, Faccia.F5).Children)
                but.Background = Colors.Blu;

            foreach (Button but in Face(grd, Faccia.F6).Children)
                but.Background = Colors.Giallo;
        }

        public static void SvuotaColori(Grid grd)
        {
            foreach (Button but in Face(grd, Faccia.F1).Children)
                but.Background = Colors.Empty;

            foreach (Button but in Face(grd, Faccia.F2).Children)
                but.Background = Colors.Empty;

            foreach (Button but in Face(grd, Faccia.F3).Children)
                but.Background = Colors.Empty;

            foreach (Button but in Face(grd, Faccia.F4).Children)
                but.Background = Colors.Empty;

            foreach (Button but in Face(grd, Faccia.F5).Children)
                but.Background = Colors.Empty;

            foreach (Button but in Face(grd, Faccia.F6).Children)
                but.Background = Colors.Empty;
        }

        public static void AssegnaEventoButtonCube_Click(Grid grd, RoutedEventHandler evento)
        {
            foreach (Button but in Face(grd, Faccia.F1).Children)
                but.Click += evento;

            foreach (Button but in Face(grd, Faccia.F2).Children)
                but.Click += evento;

            foreach (Button but in Face(grd, Faccia.F3).Children)
                but.Click += evento;

            foreach (Button but in Face(grd, Faccia.F4).Children)
                but.Click += evento;

            foreach (Button but in Face(grd, Faccia.F5).Children)
                but.Click += evento;

            foreach (Button but in Face(grd, Faccia.F6).Children)
                but.Click += evento;
        }

        public static void AssegnaEventoButtonColor_Click(StackPanel stk, RoutedEventHandler evento)
        {
            foreach (Button but in stk.Children)
                but.Click += evento;
        }


        public static int NumberFromColor(SolidColorBrush color)
        {
            if (color.Equals(Colors.Bianco))
                return 1;
            if (color.Equals(Colors.Giallo))
                return 2;
            if (color.Equals(Colors.Arancione))
                return 3;
            if (color.Equals(Colors.Rosso))
                return 4;
            if (color.Equals(Colors.Verde))
                return 5;
            if (color.Equals(Colors.Blu))
                return 6;

            return 0;
        }

        public static SolidColorBrush ColorFromNumber(int n)
        {
            if (n == 1)
                return Colors.Bianco;
            if (n == 2)
                return Colors.Giallo;
            if (n == 3)
                return Colors.Arancione;
            if (n == 4)
                return Colors.Rosso;
            if (n == 5)
                return Colors.Verde;
            if (n == 6)
                return Colors.Blu;

            return Colors.Empty;
        }
    }
}
