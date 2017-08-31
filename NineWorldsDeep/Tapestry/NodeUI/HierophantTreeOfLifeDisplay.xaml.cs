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
using System.Windows.Navigation;
using System.Windows.Shapes;
using NineWorldsDeep.Hierophant;
using NineWorldsDeep.Tapestry.Nodes;

namespace NineWorldsDeep.Tapestry.NodeUI
{
    /// <summary>
    /// Interaction logic for HierophantTreeOfLifeDisplay.xaml
    /// </summary>
    public partial class HierophantTreeOfLifeDisplay : UserControl
    {
        public HierophantTreeOfLifeDisplay()
        {
            InitializeComponent();
            hierophantTreeOfLifeInstance.VertexClicked += TreeOfLife_VertexClicked;
            lurianicTreeOfLifeInstance.VertexClicked += TreeOfLife_VertexClicked;
        }

        public event EventHandler<HierophantVertexClickedEventArgs> VertexClicked;

        private void TreeOfLife_VertexClicked(object sender, HierophantVertexClickedEventArgs args)
        {
            args.VertexNode.LoadLocal = chkLoadLocal.IsChecked.Value;
            OnVertexClicked(args);
        }

        protected virtual void OnVertexClicked(HierophantVertexClickedEventArgs args)
        {
            VertexClicked?.Invoke(this, args);
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            ProcessExpanderState((Expander)sender);
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            ProcessExpanderState((Expander)sender);
        }

        /// <summary>
        /// manages grid rows to share space between multiple expanded expanders
        /// </summary>
        /// <param name="expander"></param>
        private void ProcessExpanderState(Expander expander)
        {
            Grid parent = FindAncestor<Grid>(expander);
            int rowIndex = Grid.GetRow(expander);

            if (parent.RowDefinitions.Count > rowIndex && rowIndex >= 0)
                parent.RowDefinitions[rowIndex].Height =
                    (expander.IsExpanded ? new GridLength(1, GridUnitType.Star) : GridLength.Auto);
        }

        public static T FindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            // Need this call to avoid returning current object if it is the 
            // same type as parent we are looking for
            current = VisualTreeHelper.GetParent(current);

            while (current != null)
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            };

            return null;
        }

        public void Display(HierophantVertexNode vNode)
        {
            string details = "";

            if (vNode.Coupling != null)
            {
                HierophantVertex vertex = vNode.Coupling.Vertex;
                details += vertex.Details();
            }

            tbTestText.Text = details;
        }

        private void btnExample_Click(object sender, RoutedEventArgs e)
        {
            TreeOfLifeMap map = new TreeOfLifeMap();

            map.Select(new SemanticKey("Binah-Chokmah")); //should display on both
            map.Select(new SemanticKey("Chesed")); //should display on both
            map.Select(new SemanticKey("Hod-Malkuth")); //should only display on GD tree
            map.Select(new SemanticKey("Chokmah-Geburah")); //should only display on Lurianic tree

            hierophantTreeOfLifeInstance.Display(map);
            lurianicTreeOfLifeInstance.Display(map);
        }
    }
}
