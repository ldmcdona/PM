// Updated by XamlIntelliSenseFileGenerator 2024-01-26 12:20:17 PM
#pragma checksum "..\..\MainWindow - Copy.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "CBB0902439162A5B5D45AF6C2CF55507C6C6C834D422350F01E48080C43C181A"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using PartsManager;
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


namespace PartsManager
{


    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector
    {

#line default
#line hidden

        private bool _contentLoaded;

        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent()
        {
            if (_contentLoaded)
            {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/PartsManager;component/mainwindow%20-%20copy.xaml", System.UriKind.Relative);

#line 1 "..\..\MainWindow - Copy.xaml"
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
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.AvailableList = ((System.Windows.Controls.ListBox)(target));

#line 10 "..\..\MainWindow - Copy.xaml"
                    this.AvailableList.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.AvailableList_SelectionChanged);

#line default
#line hidden
                    return;
                case 2:
                    this.UnavailableList = ((System.Windows.Controls.ListBox)(target));

#line 11 "..\..\MainWindow - Copy.xaml"
                    this.UnavailableList.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.UnavailableList_SelectionChanged);

#line default
#line hidden
                    return;
                case 3:
                    this.l1 = ((System.Windows.Controls.Label)(target));
                    return;
                case 4:
                    this.l2 = ((System.Windows.Controls.Label)(target));
                    return;
                case 5:
                    this.Comps = ((System.Windows.Controls.ListView)(target));
                    return;
                case 6:
                    this.Notes = ((System.Windows.Controls.TextBlock)(target));
                    return;
                case 7:
                    this.Procedure = ((System.Windows.Controls.TextBlock)(target));
                    return;
                case 8:
                    this.l3 = ((System.Windows.Controls.Label)(target));
                    return;
                case 9:
                    this.l4 = ((System.Windows.Controls.Label)(target));
                    return;
                case 10:
                    this.Stock = ((System.Windows.Controls.ListView)(target));
                    return;
                case 11:
                    this.l5 = ((System.Windows.Controls.Label)(target));
                    return;
            }
            this._contentLoaded = true;
        }

        internal System.Windows.Controls.ListBox AvailableList;
        internal System.Windows.Controls.ListBox UnavailableList;
        internal System.Windows.Controls.Label l1;
        internal System.Windows.Controls.Label l2;
        internal System.Windows.Controls.ListView Comps;
        internal System.Windows.Controls.TextBlock Notes;
        internal System.Windows.Controls.TextBlock Procedure;
        internal System.Windows.Controls.Label l3;
        internal System.Windows.Controls.Label l4;
        internal System.Windows.Controls.ListView Stock;
        internal System.Windows.Controls.Label l5;
    }
}
