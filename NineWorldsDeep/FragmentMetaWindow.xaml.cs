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

namespace NineWorldsDeep
{
    /// <summary>
    /// Interaction logic for ListViewDetail.xaml
    /// </summary>
    public partial class FragmentMetaWindow : Window
    {
        public FragmentMetaWindow()
        {
            InitializeComponent();
        }

        private void LoadDemo()
        {
            List<Fragment> lst = new List<Fragment>();

            for (int i = 1; i < 10; i++)
            {
                Fragment f = new Fragment("frg" + i);
                f.SetMeta("DemoKey", "demo value for frg" + i);
                lst.Add(f);
            }

            SetItemsSource(lst);
        }

        public void SetItemsSource(IEnumerable<Fragment> fragments)
        {
            lvItems.ItemsSource = fragments;
        }

        private void MenuItemDemo_Click(object sender, RoutedEventArgs e)
        {
            LoadDemo();
        }

        private void lvItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Fragment frg = (Fragment)lvItems.SelectedItem;
            if(frg != null)
            {
                lvDetail.ItemsSource = frg.Meta;
            }
        }
    }
}