﻿using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace NineWorldsDeep.Tapestry
{
    /// <summary>
    /// Interaction logic for TapestrySplitViewWindow.xaml
    /// </summary>
    public partial class TapestrySplitViewWindow : Window
    {
        public TapestrySplitViewWindow()
        {
            InitializeComponent();
            UI.Utils.MainWindow = this;
        }

        private void MenuItemNavigateRoot_Click(object sender, RoutedEventArgs e)
        {
            ctrlNodeSplitView.NavigateRoot();
        }
        
        private void MenuItem_GlobalLoadLocalCheckChanged(object sender, RoutedEventArgs e)
        {
            TapestryRegistry.GlobalLoadLocal = chkGlobalLoadLocal.IsChecked;
        }
    }
}
