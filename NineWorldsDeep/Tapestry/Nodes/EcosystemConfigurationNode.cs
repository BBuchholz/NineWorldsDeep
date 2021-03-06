﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NineWorldsDeep.Tapestry.Nodes
{
    class EcosystemConfigurationNode : TapestryNode
    {
        public EcosystemConfigurationNode()
            : base("Configuration/Ecosystem")
        {

        }

        public override string GetShortName()
        {
            return "Ecosystem Config";
        }

        public override bool Parallels(TapestryNode nd)
        {
            throw new NotImplementedException();
        }

        public override void PerformSelectionAction()
        {
            //do nothing
        }

        public override IEnumerable<TapestryNode> Children(TapestryNodeType nodeType)
        {
            //always empty
            return new List<TapestryNode>();
        }

        public override TapestryNodeType NodeType => 
            TapestryNodeType.EcosystemConfiguration;
    }
}
