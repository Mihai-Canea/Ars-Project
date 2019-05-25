using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

namespace cube3D_WPF
{
    class InputOutput
    {
        private int size;

        public InputOutput(int s)
        {
            this.size = s;
        }


        // percorso di salvataggio del file
        public void save(string fileName, Faccie[,] posFaccia)
        {
            int n = 0;

            using (StreamWriter sw = new StreamWriter(fileName))
            {
                for (int i = 0; i < size * 4; i++)
                {
                    for (int j = 0; j < size * 3; j++)
                    {
                        sw.Write(posFaccia[i, j].ToString() + " ");
                    }
                    sw.WriteLine();
                }
            }
        }

        public void saveCst(string fileName, Faccie[,] posFaccia)
        {
            int n = 0, cont = 0;

            using (StreamWriter sw = new StreamWriter(fileName))
            {
                for (int i = 0; i < size * 4; i++)
                {
                    for (int j = 0; j < size * 3; j++)
                    {
                        switch (posFaccia[i, j])
                        {
                            case Faccie.F: n = 5; break;
                            case Faccie.R: n = 4; break;
                            case Faccie.B: n = 6; break;
                            case Faccie.L: n = 3; break;
                            case Faccie.U: n = 1; break;
                            case Faccie.D: n = 2; break;
                            case Faccie.None: n = 0; break;
                        }
                        if (n != 0)
                            sw.WriteLine(Convert.ToString(n));
                    }
                    //sw.WriteLine();
                    // giorno 78 , ho imparato una nuova parola , Cacciavite ...
                }
            }
        }

        public Faccie[,] read(string fileName)
        {
            Faccie[,] posFaccia = new Faccie[size * 4, size * 3];

            using (StreamReader r = new StreamReader(fileName))
            {
                for (int i = 0; i < size * 4; i++)
                {
                    string line = "";
                    string faccia = "";

                    for (int j = 0; j < size * 3; j++)
                    {
                        try
                        {
                            line = r.ReadLine();
                            // guardare bene !!!
                            switch (Convert.ToInt16( line))
                            {
                                case 5: faccia = "F"; break;
                                case 4: faccia = "R"; break;
                                case 6: faccia = "B"; break;
                                case 3: faccia = "L"; break;
                                case 1: faccia = "U"; break;
                                case 2: faccia = "D"; break;
                                case 0: faccia = "None"; break;
                            }
                            // poi mi hanno detto brugola... 
                            posFaccia[i, j] = (Faccie)Enum.Parse(typeof(Faccie), faccia);
                        }
                        catch (ArgumentException)
                        {
                            //genera un errore di tipo InvalidDataException();
                            throw new InvalidDataException();
                        }
                        catch (IndexOutOfRangeException)
                        {
                            //genera un errore di tipo InvalidDataException();
                            throw new InvalidDataException();
                        }
                    }
                }

                return posFaccia;
            }
        }
    }
}
