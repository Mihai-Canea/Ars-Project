using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ARS_Studio.Controls
{
    /// <summary>
    /// Logica di interazione per NumericUpDownW.xaml
    /// </summary>
    public partial class NumericUpDownW : UserControl
    {
        public NumericUpDownW()
        {
            InitializeComponent();
        }

        #region Proprietà

        /// <summary>Il valore minimo assegnabile</summary>
        public double MinValue { get; set; } = 0;
        /// <summary>Il valore massimo assegnabile</summary>
        public double MaxValue { get; set; } = 100;
        /// <summary>Il passo di incremento / decremento</summary>
        public double Step { get; set; } = 5;
        /// <summary>Il valore attuale</summary>
        public double Value
        {
            get => Convert.ToDouble(TxtTesto.Text);
            set
            {
                try
                {
                    //Si controlla se il valore è valido nel range MinValue <= Value <= MaxValue
                    if (value < MinValue)
                        TxtTesto.Text = Convert.ToString(MinValue);
                    else if (value > MaxValue)
                        TxtTesto.Text = Convert.ToString(MaxValue);
                    else
                        TxtTesto.Text = Convert.ToString(value);
                }
                catch { TxtTesto.Text = Convert.ToString(MinValue); }                           //Nel caso si verifichi qualsiasi errore, si reimposta il valore al valore minimo

                //Si scatena l'evento di cambiamento del valore
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion


        #region Metodi

        /// <summary>Incrementa il valore di uno step</summary>
        public void IncrementValue() => Value += Step;
        /// <summary>Incrementa il valore del valore inserito come parametro</summary>
        /// <param name="step">Il valore di quanto incrementare</param>
        public void IncrementValue(int step) => Value += step;

        /// <summary>Decrementa il valore di uno step</summary>
        public void DecrementValue() => Value -= Step;
        /// <summary>Decrementa il valore del valore inserito come parametro</summary>
        /// <param name="step">Il valore di quanto decrementare</param>
        public void DecrementValue(int step) => Value -= step;

        #endregion


        #region Eventi pubblici

        /// <summary>
        /// Evento al cambiamento del valore
        /// </summary>
        public event EventHandler ValueChanged;

        #endregion


        #region Eventi privati

        /// <summary>Evento al <see cref="Border.MouseDown"/> del pulsante "+".</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Plus_MouseDown(object sender, MouseButtonEventArgs e) => IncrementValue();
        /// <summary>Evento al <see cref="Border.MouseDown"/> del pulsante "-".</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minus_MouseDown(object sender, MouseButtonEventArgs e) => DecrementValue();

        /// <summary>
        /// Evento al cambiamento del testo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtTesto_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //Se il valore inserito non è un numero, si ritorna allo stato precedente
                if (!int.TryParse(TxtTesto.Text, out var _) && TxtTesto.CanUndo)
                    Dispatcher.BeginInvoke(new Action(() => TxtTesto.Undo()));
            }
            catch { TxtTesto.Text = Convert.ToString(MinValue); }                   //Ma nel caso si verifichi qualche errore, si imposta il testo al valore minimo
        }
        /// <summary>
        /// Evento alla rotazione della rotellina sul controllo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDownW_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta >= 0)
                IncrementValue();               //Se si ruota la rotellina verso l'alto si incrementa il valore
            else
                DecrementValue();               //Altrimenti, se si ruota la rotellina verso il basso, lo si decrementa
        }

        #endregion
    }
}
