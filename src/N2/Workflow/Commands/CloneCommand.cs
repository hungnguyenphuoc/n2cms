﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N2.Workflow.Commands
{
    public class CloneCommand : CommandBase<CommandContext>
    {
        public override void Process(CommandContext state)
        {
            var clone = state.Content.Clone(false);
            clone.VersionOf = state.Content.VersionOf;
            state.Content = clone;
        }
    }
}
