﻿using NineWorldsDeep.Tapestry.Fragments;
using System;

namespace NineWorldsDeep.FragmentCloud
{
    public class StudioNode : TapestryNode
    {
        public StudioNode()
            : base("Studio")
        {
            AddChild(new StudioMainNode());
            AddChild(new StudioProjectsNode());
            AddChild(new StudioLyricsNode());
            AddChild(new StudioScalesNode());
            AddChild(new StudioKeyboardNode());
            AddChild(new ArpaBetNode());
        }

        public override string GetShortName()
        {
            return "Studio";
        }

        public override void PerformSelectionAction()
        {
            var window = new Studio.StudioMainWindow();
            window.Show();
        }
    }
}