﻿using System;

namespace NineWorldsDeep.FragmentCloud
{
    public class GardenFragment : Tapestry.TapestryNode
    {
        public GardenFragment()
            : base("")
        {
            AddChild(new FileSystemNode("NWD", true));
            AddChild(new FileSystemNode("NWD-AUX", true));
            AddChild(new FileSystemNode("NWD-MEDIA", true));
            AddChild(new FileSystemNode("NWD-SNDBX", true));
            AddChild(new FileSystemNode("NWD-SYNC", true));
        }

        public override string GetShortName()
        {
            return "Garden";
        }

        public override void PerformSelectionAction()
        {
            //do nothing
        }
    }
}