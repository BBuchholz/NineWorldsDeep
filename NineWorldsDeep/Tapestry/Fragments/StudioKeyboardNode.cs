﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NineWorldsDeep.FragmentCloud;

namespace NineWorldsDeep.Tapestry.Fragments
{
    public class StudioKeyboardNode : FragmentCloud.TapestryNode
    {
        public StudioKeyboardNode() 
            : base("Studio/Keyboard")
        {
        }

        public override string GetShortName()
        {
            return "Studio Keyboard";
        }

        public override void PerformSelectionAction()
        {
            var window = new Studio.VisualKeyboardTestHarness();
            window.Show();
        }
    }
}