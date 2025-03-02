﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.CommandLine.Parsing
{
    internal sealed class CommandArgumentNode : SyntaxNode
    {
        public CommandArgumentNode(
            Token token, 
            Argument argument,
            CommandNode parent) : base(token)
        {
            Debug.Assert(token.Type == TokenType.Argument, $"Incorrect token type: {token}");

            Argument = argument;
            ParentCommandNode = parent;
        }

        public Argument Argument { get; }

        public CommandNode ParentCommandNode { get; }
    }
}
