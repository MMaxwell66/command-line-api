﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.CommandLine.Parsing
{
    internal sealed class OptionArgumentNode : SyntaxNode
    {
        public OptionArgumentNode(
            Token token,
            Argument argument,
            OptionNode parent) : base(token)
        {
            Debug.Assert(token.Type == TokenType.Argument, $"Incorrect token type: {token}");

            Argument = argument;
            ParentOptionNode = parent;
        }

        public Argument Argument { get; }

        public OptionNode ParentOptionNode { get; }
    }
}
