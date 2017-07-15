﻿using NineWorldsDeep.Hive;
using NineWorldsDeep.Mnemosyne.V5;
using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NineWorldsDeep.Tapestry.NodeUI
{
    /// <summary>
    /// Interaction logic for HiveMain.xaml
    /// </summary>
    public partial class HiveMain : UserControl
    {
        public HiveMain()
        {
            InitializeComponent();
            Refresh();
        }

        private void ClearTreeViews()
        {
            tvHive.Items.Clear();
            tvHiveDeactivated.Items.Clear();
        }

        private void Refresh()
        {
            ClearTreeViews();
            PopulateActiveRoots(UtilsHive.GetActiveRoots());
            PopulateDeactivatedRoots(UtilsHive.GetDeactivatedRoots());
        }

        private void PopulateDeactivatedRoots(List<HiveRoot> roots)
        {
            foreach (HiveRoot hr in roots)
            {
                tvHiveDeactivated.Items.Add(CreateTreeItem(hr, false));
            }
        }

        private void PopulateActiveRoots(List<HiveRoot> roots)
        {
            foreach (HiveRoot hr in roots)
            {
                tvHive.Items.Add(CreateTreeItem(hr, true));
            }
        }

        private void tvHive_Expanded(object sender, RoutedEventArgs e)
        {
            ProcessExpander(e);
        }

        private void tvHiveDeactivated_Expanded(object sender, RoutedEventArgs e)
        {
            ProcessExpander(e, false);
        }

        private void ProcessExpander(RoutedEventArgs e, bool isActiveTreeView = true)
        {
            TreeViewItem item = e.Source as TreeViewItem;

            if (item.Items.Count == 1 &&
                item.Items[0] is string)
            {
                item.Items.Clear();

                if(item.Tag is HiveRoot)
                {
                    HiveRoot hr = item.Tag as HiveRoot;

                    UtilsHive.RefreshLobes(hr);

                    foreach(HiveLobe hl in hr.Lobes)
                    {
                        item.Items.Add(CreateTreeItem(hl, isActiveTreeView));
                    }
                }

                if(item.Tag is HiveLobe)
                {
                    HiveLobe hl = item.Tag as HiveLobe;

                    UtilsHive.RefreshSpores(hl);

                    foreach(HiveSpore hs in hl.Spores)
                    {
                        item.Items.Add(CreateTreeItem(hs, isActiveTreeView));
                    }
                }

                if(item.Tag is HiveSpore)
                {
                    //do something
                }
            }
        }
        
        private TreeViewItem CreateTreeItem(HiveRoot hr, bool isActiveTreeView)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = hr.HiveRootName;
            item.Tag = hr;
            item.Items.Add("Loading...");

            item.ContextMenu = isActiveTreeView ? 
                (ContextMenu)this.Resources["cmHiveRootMenu"] : 
                (ContextMenu)this.Resources["cmDeactivatedHiveRootMenu"];

            item.MouseRightButtonDown += SelectRightClickedTreeViewItem;
            return item;
        }

        private void SelectRightClickedTreeViewItem(object sender, MouseButtonEventArgs e)
        {            
            var item = (TreeViewItem)e.Source;
            item.IsSelected = true;            
        }

        private TreeViewItem CreateTreeItem(HiveLobe hl, bool isActiveTreeView)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = hl.Name;
            item.Tag = hl;
            item.Items.Add("Loading...");

            item.ContextMenu = isActiveTreeView ? 
                (ContextMenu)this.Resources["cmHiveLobeMenu"] :
                (ContextMenu)this.Resources["cmDeactivatedHiveLobeMenu"]; 

            item.MouseRightButtonDown += SelectRightClickedTreeViewItem;
            return item;
        }

        private TreeViewItem CreateTreeItem(HiveSpore hs, bool isActiveTreeView)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = hs.Name;
            item.Tag = hs;
            item.Items.Add("Loading...");

            item.ContextMenu = isActiveTreeView ? 
                (ContextMenu)this.Resources["cmHiveSporeMenu"] :
                (ContextMenu)this.Resources["cmDeactivatedHiveSporeMenu"];

            item.MouseRightButtonDown += SelectRightClickedTreeViewItem;
            return item;
        }

        private void btnAddHiveRoot_Click(object sender, RoutedEventArgs e)
        {
            string input = UI.Prompt.Input("Enter Hive Root Name");

            if (!string.IsNullOrWhiteSpace(input))
            {
                while (!UtilsHive.HiveRootNameIsValid(input))
                {
                    input = UI.Prompt.Input("Invalid name format, please correct", input);
                }

                UtilsHive.EnsureHiveRootName(input);
                Refresh();
            }
            else
            {
                UI.Display.Message("Cancelled.");
            }
        }
        
        private void MenuItemTest_Click(object sender, RoutedEventArgs e)
        {
            string msg = "null tag";

            MenuItem mnu = sender as MenuItem;
            TreeViewItem item = null;
            if (mnu != null)
            {
                item = ((ContextMenu)mnu.Parent).PlacementTarget as TreeViewItem;
                
                if (item != null && item.Tag != null)
                {
                    msg = item.Tag.ToString();
                }
            }

            UI.Display.Message(msg);
        }

        private void MenuItemDeactivateHiveRoot_Click(object sender, RoutedEventArgs e)
        {
            UI.Display.Message("deactivate hive root here");
        }

        private void MenuItemActivateHiveRoot_Click(object sender, RoutedEventArgs e)
        {
            UI.Display.Message("activate hive root here");
        }
    }
}
