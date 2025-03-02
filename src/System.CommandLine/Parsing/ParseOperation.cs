﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal sealed class ParseOperation
    {
        private readonly List<Token> _tokens;
        private readonly CommandLineConfiguration _configuration;
        private int _index;

        public ParseOperation(
            List<Token> tokens,
            CommandLineConfiguration configuration)
        {
            _tokens = tokens;
            _configuration = configuration;
        }

        private Token CurrentToken => _tokens[_index];

        public CommandNode? RootCommandNode { get; private set; }

        public List<Token>? UnmatchedTokens { get; private set; }

        private void Advance() => _index++;

        private bool More(out TokenType currentTokenType)
        {
            bool result = _index < _tokens.Count;
            currentTokenType = result ? _tokens[_index].Type : (TokenType)(-1);
            return result;
        }

        public void Parse()
        {
            RootCommandNode = ParseRootCommand();
        }

        private CommandNode ParseRootCommand()
        {
            var rootCommandNode = new CommandNode(
                CurrentToken,
                _configuration.RootCommand);

            Advance();

            ParseDirectives(rootCommandNode);

            ParseCommandChildren(rootCommandNode);

            return rootCommandNode;
        }

        private void ParseSubcommand(CommandNode parentNode)
        {
            var commandNode = new CommandNode(CurrentToken, (Command)CurrentToken.Symbol!);

            Advance();

            ParseCommandChildren(commandNode);

            parentNode.AddChildNode(commandNode);
        }

        private void ParseCommandChildren(CommandNode parent)
        {
            int currentArgumentCount = 0;
            int currentArgumentIndex = 0;

            while (More(out TokenType currentTokenType))
            {
                if (currentTokenType == TokenType.Command)
                {
                    ParseSubcommand(parent);
                }
                else if (currentTokenType == TokenType.Option)
                {
                    ParseOption(parent);
                }
                else if (currentTokenType == TokenType.Argument)
                {
                    ParseCommandArguments(parent, ref currentArgumentCount, ref currentArgumentIndex);
                }
                else
                {
                    AddCurrentTokenToUnmatched();
                    Advance();
                }
            }
        }

        private void ParseCommandArguments(CommandNode commandNode, ref int currentArgumentCount, ref int currentArgumentIndex)
        {
            while (More(out TokenType currentTokenType) && currentTokenType == TokenType.Argument)
            {
                while (commandNode.Command.HasArguments && currentArgumentIndex < commandNode.Command.Arguments.Count)
                {
                    Argument argument = commandNode.Command.Arguments[currentArgumentIndex];

                    if (currentArgumentCount < argument.Arity.MaximumNumberOfValues)
                    {
                        var argumentNode = new CommandArgumentNode(
                            CurrentToken,
                            argument,
                            commandNode);

                        commandNode.AddChildNode(argumentNode);

                        currentArgumentCount++;

                        Advance();

                        break;
                    }
                    else
                    {
                        currentArgumentCount = 0;
                        currentArgumentIndex++;
                    }
                }

                if (currentArgumentCount == 0) // no matching arguments found
                {
                    AddCurrentTokenToUnmatched();
                    Advance();
                }
            }
        }

        private void ParseOption(CommandNode parent)
        {
            OptionNode optionNode = new(
                CurrentToken,
                (Option)CurrentToken.Symbol!);

            Advance();

            ParseOptionArguments(optionNode);

            parent.AddChildNode(optionNode);
        }

        private void ParseOptionArguments(OptionNode optionNode)
        {
            var argument = optionNode.Option.Argument;

            var contiguousTokens = 0;
            int argumentCount = 0;

            while (More(out TokenType currentTokenType) && currentTokenType == TokenType.Argument)
            {
                if (argumentCount >= argument.Arity.MaximumNumberOfValues)
                {
                    if (contiguousTokens > 0)
                    {
                        return;
                    }

                    if (argument.Arity.MaximumNumberOfValues == 0)
                    {
                        return;
                    }
                }
                else if (argument.ValueType == typeof(bool) && !bool.TryParse(CurrentToken.Value, out _))
                {
                    return;
                }

                optionNode.AddChildNode(
                    new OptionArgumentNode(
                        CurrentToken,
                        argument,
                        optionNode));

                argumentCount++;

                contiguousTokens++;

                Advance();

                if (!optionNode.Option.AllowMultipleArgumentsPerToken)
                {
                    return;
                }
            }
        }

        private void ParseDirectives(CommandNode rootCommandNode)
        {
            while (More(out TokenType currentTokenType) && currentTokenType == TokenType.Directive)
            {
                ParseDirective(rootCommandNode); // kept in separate method to avoid JIT
            }

            void ParseDirective(CommandNode parent)
            {
                var token = CurrentToken;
                ReadOnlySpan<char> withoutBrackets = token.Value.AsSpan(1, token.Value.Length - 2);
                int indexOfColon = withoutBrackets.IndexOf(':');
                string key = indexOfColon >= 0 
                    ? withoutBrackets.Slice(0, indexOfColon).ToString()
                    : withoutBrackets.ToString();
                string? value = indexOfColon > 0
                    ? withoutBrackets.Slice(indexOfColon + 1).ToString()
                    : null;

                var directiveNode = new DirectiveNode(token, key, value);

                parent.AddChildNode(directiveNode);

                Advance();
            }
        }

        private void AddCurrentTokenToUnmatched()
        {
            if (CurrentToken.Type == TokenType.DoubleDash)
            {
                return;
            }

            (UnmatchedTokens ??= new()).Add(CurrentToken);
        }
    }
}