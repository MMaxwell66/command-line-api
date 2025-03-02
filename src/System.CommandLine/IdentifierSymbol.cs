﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Diagnostics;

namespace System.CommandLine
{
    /// <summary>
    /// A symbol, such as an option or command, having one or more fixed names in a command line interface.
    /// </summary>
    public abstract class IdentifierSymbol : Symbol
    {
        private readonly HashSet<string> _aliases = new(StringComparer.Ordinal);

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifierSymbol"/> class.
        /// </summary>
        /// <param name="description">The description of the symbol, which is displayed in command line help.</param>
        protected IdentifierSymbol(string? description = null)
        {
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifierSymbol"/> class.
        /// </summary>
        /// <param name="name">The name of the symbol.</param>
        /// <param name="description">The description of the symbol, which is displayed in command line help.</param>
        protected IdentifierSymbol(string name, string? description = null) 
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
        }

        /// <summary>
        /// Gets the set of strings that can be used on the command line to specify the symbol.
        /// </summary>
        public IReadOnlyCollection<string> Aliases => _aliases;

        /// <inheritdoc/>
        public override string Name
        {
            set
            {
                if (_name is null || !string.Equals(_name, value, StringComparison.Ordinal))
                {
                    AddAlias(value);

                    if (_name != null)
                    {
                        RemoveAlias(_name);
                    }

                    _name = value;
                }
            }
        }

        /// <summary>
        /// Adds an <see href="/dotnet/standard/commandline/syntax#aliases">alias</see>.
        /// </summary>
        /// <param name="alias">The alias to add.</param>
        /// <remarks>
        /// You can add multiple aliases for a symbol.
        /// </remarks>
        public void AddAlias(string alias)
        {
            ThrowIfAliasIsInvalid(alias);

            _aliases.Add(alias);
        }

        private protected virtual void RemoveAlias(string alias) => _aliases.Remove(alias);

        /// <summary>
        /// Determines whether the specified alias has already been defined.
        /// </summary>
        /// <param name="alias">The alias to search for.</param>
        /// <returns><see langword="true" /> if the alias has already been defined; otherwise <see langword="false" />.</returns>
        public bool HasAlias(string alias) => _aliases.Contains(alias);

        internal string GetLongestAlias(bool removePrefix)
        {
            string max = "";
            foreach (string alias in _aliases)
            {
                if (alias.Length > max.Length)
                {
                    max = alias;
                }
            }
            return removePrefix ? max.RemovePrefix() : max;
        }

        [DebuggerStepThrough]
        private void ThrowIfAliasIsInvalid(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("An alias cannot be null, empty, or consist entirely of whitespace.");
            }

            for (var i = 0; i < alias.Length; i++)
            {
                if (char.IsWhiteSpace(alias[i]))
                {
                    throw new ArgumentException($"Alias cannot contain whitespace: \"{alias}\"", nameof(alias));
                }
            }
        }
    }
}