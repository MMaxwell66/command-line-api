﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    internal sealed class CommandNode : NonterminalSyntaxNode
    {
        public CommandNode(
            Token token,
            Command command) : base(token)
        {
            Command = command;
        }

        public Command Command { get; }
    }
}
