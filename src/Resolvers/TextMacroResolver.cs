using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MicroKnights.Texting.Attributes;
using MicroKnights.Texting.Results;
using Microsoft.Extensions.DependencyInjection;

namespace MicroKnights.Texting.Resolvers
{
    public class TextMacroResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TextMacroResolverOptions _options;

        public TextMacroResolver(IServiceProvider serviceProvider, TextMacroResolverOptions options)
        {
            _serviceProvider = serviceProvider;
            _options = options;
        }

        public IEnumerable<string> GetResolverMacroNames()
        {
            return _options.GetMacroValueResolverTypes().Keys;
        }

        private Task<MacroValueResult> ResolveAsync(MacroValueResolver resolver, string macroName, string textFormat)
        {
            return resolver.Resolve(macroName, textFormat ?? resolver.DefaultTextFormat);
        }

        private async Task<MacroValueResult> ResolveSync(MacroValueResolver resolver, string macroName, string textFormat)
        {
            return await resolver.Resolve(macroName, textFormat ?? resolver.DefaultTextFormat);
        }

        public Task<MacroValueResult> ResolveMacro(string macroName, string textFormat = null)
        {
            try
            {
                if (_options.GetMacroValueResolverTypes().TryGetValue(macroName.ToLowerInvariant(), out var resolverType))
                {
                    var macroValueResolver = (MacroValueResolver) _serviceProvider.GetRequiredService(resolverType.Type);
                    if (resolverType.Synchronize)
                        return ResolveSync(macroValueResolver,macroName, textFormat ?? macroValueResolver.DefaultTextFormat);
                    return ResolveAsync(macroValueResolver, macroName, textFormat ?? macroValueResolver.DefaultTextFormat);
                }
                return Task.FromResult(new MacroValueResult(macroName, new InvalidOperationException($"Unknown MacroName \"{macroName}\"")));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new MacroValueResult(macroName, ex));
            }
        }

        protected virtual TextMacroResolverMatch GetMatchGroups(Match match)
        {
            var groups = match.Groups;
            return new TextMacroResolverMatch(
                groups[0].Value, // fullmatch
                groups[1].Value, // macroName
                groups[2].Success ? groups[2].Value.Substring(1) : null // textFormat 
            );
        }

        /// <summary>
        /// Any text where "....{{macro.name}}....." is present, it will be resolved. <br />
        /// It is also possible to specify a specific format resolving of the macro value. so a ".... {{macro.name:N0}} USD....." will end as ".... 1,000 USD. ....."
        /// </summary>
        /// <param name="textWithMacros"></param>
        /// <returns>The result of replaceing the macro with resolved values</returns>
        /// <seealso cref="MacroTextResult"/>
        public async Task<MacroTextResult> ResolveText(string textWithMacros)
        {
            var macroValueResolverTypes = _options.GetMacroValueResolverTypes();
            var macroTasks = new Dictionary<string,(string FullMatch, Task<MacroValueResult> Task)>();
            var resolvedText = textWithMacros;
            foreach (var matchGroup in ExtractMacros(textWithMacros))
            {
                var lowerFullMatch = matchGroup.FullMatch;
                if (macroTasks.TryGetValue(lowerFullMatch, out var fullmatchTask) == false)
                {
                    Task<MacroValueResult> macroResultTask;
                    if (macroValueResolverTypes.TryGetValue(matchGroup.MacroName.ToLowerInvariant(), out var resolverType))
                    {
                        var macroValueResolver = (MacroValueResolver)_serviceProvider.GetRequiredService(resolverType.Type);
                        if (resolverType.Synchronize)
                            macroResultTask = ResolveSync(macroValueResolver, matchGroup.MacroName, matchGroup.TextFormat ?? macroValueResolver.DefaultTextFormat);
                        else
                            macroResultTask = ResolveAsync(macroValueResolver, matchGroup.MacroName, matchGroup.TextFormat ?? macroValueResolver.DefaultTextFormat);
                    }
                    else
                        macroResultTask = Task.FromResult(new MacroValueResult(matchGroup.MacroName, new InvalidOperationException($"Unknown MacroName \"{matchGroup.MacroName}\"")));
                    macroTasks.Add(lowerFullMatch, (matchGroup.FullMatch, macroResultTask));
                }
                else if(matchGroup.FullMatch != fullmatchTask.FullMatch)
                {
                    // TODO: hhmmmmm - we have some macro name doublet case sensitive troubles where....
                    throw new InvalidCastException($"Please change macro \"{matchGroup.FullMatch}\" to \"{fullmatchTask.FullMatch}\" due to unhandled case sensitivity");
                }
            }

            await Task.WhenAll(macroTasks.Values.Select(v => v.Task));

            foreach (var fullmatchTask in macroTasks.Values)
            {
                resolvedText = resolvedText.Replace(fullmatchTask.FullMatch, fullmatchTask.Task.Result.FormattedText);
            }

            return new MacroTextResult(textWithMacros, resolvedText, macroTasks.Values.Select(mr => mr.Task.Result));
        }

        /// <summary>
        /// Extract macros from text, one at a time. <br />
        /// If a macro is repeated multiple times, it will likewise be part of the return multiple times.
        /// </summary>
        /// <param name="textWithMacros">Text string, with macros to extract</param>
        /// <returns>List of extracted macros</returns>
        /// <seealso cref="TextMacroResolverMatch"/>
        public IEnumerable<TextMacroResolverMatch> ExtractMacros(string textWithMacros)
        {
            var match = _options.GetRegexMatcher().Match(textWithMacros);
            while (match.Success)
            {
                yield return GetMatchGroups(match);

                match = match.NextMatch();
            }
        }
    }
}