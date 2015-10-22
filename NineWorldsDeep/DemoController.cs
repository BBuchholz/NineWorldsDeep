﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NineWorldsDeep
{
    class DemoController
    {
        public FragmentMetaWindow fmw;

        public void Configure(FragmentMetaWindow fmw)
        {
            this.fmw = fmw;
            this.fmw.AddMenuItem("Demo", MenuItemDemo_Click);
        }

        private void MenuItemDemo_Click(object sender, RoutedEventArgs e)
        {
            DemoFragments();
            DemoDynamicMenus();
        }

        private void DemoFragments()
        {
            List<Fragment> lst = new List<Fragment>();

            for (int i = 1; i < 10; i++)
            {
                Fragment f = new Fragment("frg" + i);
                f.SetMeta("DemoKey", "demo value for frg" + i);

                if(i % 2 == 0)
                {                    
                    f.SetMeta("ConditionalMetaKey", "example " + i);
                }

                lst.Add(f);
            }

            fmw.SetItemsSource(lst);
        }

        private void DemoDynamicMenus()
        {
            fmw.AddMenuItem("Dynamic Menu Item", Test_Click);
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Testing");
        }
    }

    
}