﻿using System;

namespace NineWorldsDeep.FragmentCloud
{
    internal class SynergyFragment : Tapestry.TapestryNode
    {
        public SynergyFragment()
            : base("Synergy")
        {
        }

        public override string GetShortName()
        {
            return "Synergy";
        }

        public override void PerformSelectionAction()
        {
            var window = new Synergy.SynergyV4MainWindow();
            window.Show();
        }
    }
}