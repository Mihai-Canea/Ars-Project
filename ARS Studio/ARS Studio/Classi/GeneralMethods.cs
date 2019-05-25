using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ARS_Studio
{
    class GeneralMethods
    {
        /// <summary>
        /// Per convertire un Bitmap in BitmapImage
        /// </summary>
        /// <param name="img">Immagine bitmap da convertire</param>
        /// <returns></returns>
        public static BitmapImage BitmapToBitmapImage(Bitmap img)
        {
            MemoryStream ms = new MemoryStream();
            img.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();

            return bi;
        }

        public static Image ImageSourceToBitmap(ImageSource src)
        {
            MemoryStream ms = new MemoryStream();
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(src as System.Windows.Media.Imaging.BitmapSource));
            encoder.Save(ms);
            ms.Flush();
            return Image.FromStream(ms);
        }

        public static int Trova(string[] vet, string cerca)
        {
            for (var i = 0; i < vet.Length; i++)
                if (vet[i].Contains(cerca))
                    return i;

            return -1;
        }


        public static string Cerca(string[] vet, string cerca)
        {
            return vet.FirstOrDefault(elem => elem.Contains(cerca));
        }
    }
}
