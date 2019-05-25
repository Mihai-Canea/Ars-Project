using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARS_Studio
{
    class Colori
    {
        public static Color GetNearestWebColor(Color input_color)
        {
            // initialize the RGB-Values of input_color
            double dbl_input_red   = Convert.ToDouble(input_color.R);
            double dbl_input_green = Convert.ToDouble(input_color.G);
            double dbl_input_blue  = Convert.ToDouble(input_color.B);

            // the Euclidean distance to be computed
            // set this to an arbitrary number
            // must be greater than the largest possible distance (appr. 441.7)
            double distance = 500.0;
            double temp;

            // RGB-Values of test colors
            double dbl_test_red;
            double dbl_test_green;
            double dbl_test_blue;

            Color nearest_color = Color.Empty;

            Color[] col =
            {
                /*Color.White,
                Color.Orange, Color.OrangeRed,
                Color.Yellow, Color.LightYellow,
                Color.Red,Color.IndianRed,
                Color.Blue, Color.LightBlue, Color.BlueViolet,
                Color.Green, Color.LightGreen*/

                Color.FromArgb(255, 219, 255, 255),
                Color.FromArgb(255, 173, 140 , 90),
                Color.FromArgb(255, 229, 254, 124),
                Color.FromArgb(255, 161, 36 , 40 ),
                Color.FromArgb(255, 3  , 1  , 108),
                Color.FromArgb(255, 108, 214, 78 )
            };

            foreach (object o in col)
            {
                dbl_test_red = Math.Pow(Convert.ToDouble(((Color)o).R) - dbl_input_red, 2.0);
                dbl_test_green = Math.Pow(Convert.ToDouble(((Color)o).G) - dbl_input_green, 2.0);
                dbl_test_blue = Math.Pow(Convert.ToDouble(((Color)o).B) - dbl_input_blue, 2.0);
                temp = Math.Sqrt(dbl_test_blue + dbl_test_green + dbl_test_red);

                if (temp < distance)
                {
                    distance = temp;
                    nearest_color = (Color)o;
                }
            }

            return nearest_color;
        }
    }
}
