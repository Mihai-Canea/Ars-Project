﻿#pragma checksum "..\..\..\Controls\NumericUpDownW.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "D82EC150B927FABCAFC82575C5185AD2A3F05638173EBF463726EA418A398D2B"
//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:4.0.30319.42000
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

using ARS_Studio.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace ARS_Studio.Controls {
    
    
    /// <summary>
    /// NumericUpDownW
    /// </summary>
    public partial class NumericUpDownW : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 16 "..\..\..\Controls\NumericUpDownW.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TxtTesto;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Controls\NumericUpDownW.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border Plus;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\..\Controls\NumericUpDownW.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border Minus;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ARS Studio;component/controls/numericupdownw.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Controls\NumericUpDownW.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 8 "..\..\..\Controls\NumericUpDownW.xaml"
            ((ARS_Studio.Controls.NumericUpDownW)(target)).MouseWheel += new System.Windows.Input.MouseWheelEventHandler(this.NumericUpDownW_MouseWheel);
            
            #line default
            #line hidden
            return;
            case 2:
            this.TxtTesto = ((System.Windows.Controls.TextBox)(target));
            
            #line 16 "..\..\..\Controls\NumericUpDownW.xaml"
            this.TxtTesto.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.TxtTesto_TextChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Plus = ((System.Windows.Controls.Border)(target));
            
            #line 24 "..\..\..\Controls\NumericUpDownW.xaml"
            this.Plus.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.Plus_MouseDown);
            
            #line default
            #line hidden
            return;
            case 4:
            this.Minus = ((System.Windows.Controls.Border)(target));
            
            #line 35 "..\..\..\Controls\NumericUpDownW.xaml"
            this.Minus.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.Minus_MouseDown);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

