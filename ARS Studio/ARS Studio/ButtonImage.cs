using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ARS_Studio
{
    class ButtonImage : Button
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ButtonImage), new UIPropertyMetadata(null));


        static ButtonImage()
        {
        }

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

    }
}
