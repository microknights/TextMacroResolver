using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MicroKnights.Texting.Attributes;

namespace MicroKnights.Texting.Resolvers
{
    public class TextMacroResolverOptions
    {
        private readonly Lazy<Regex> _regexMatcher;
        private readonly Lazy<Dictionary<string, MacroValueResolverType>> _resolverTypes;

        public TextMacroResolverOptions()
        {
            _regexMatcher = new Lazy<Regex>(CompileRegexMatcher);
            _resolverTypes = new Lazy<Dictionary<string, MacroValueResolverType>>(InitializeMacroValueResolverTypes);
        }

        public string MacroNamePrefix { get; set; } = @"{{";
        public string MacroNamePostfix { get; set; } = @"}}";
        public char MacroNameTextFormatSeperator { get; set; } = ':';

        public IEnumerable<Type> MacroValueResolverTypes { get; set; }
        public IEnumerable<Assembly> AssembliesWithMacroValueResolverTypes { get; set; }

        internal Regex GetRegexMatcher() => _regexMatcher.Value;
        internal Dictionary<string, MacroValueResolverType> GetMacroValueResolverTypes() => _resolverTypes.Value;

        private Regex CompileRegexMatcher()
        {
            var prefix = Regex.Escape(MacroNamePrefix);
            var seperator = Regex.Escape(MacroNameTextFormatSeperator.ToString());
            var postfix = Regex.Escape(MacroNamePostfix);
            // {{([^:^}]+)(:[^:^}]+)?}}
            return new Regex($@"{prefix}([^{seperator}^{postfix}]+)({seperator}[^{postfix}]+)?{postfix}", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        }

        private IEnumerable<Type> GetMacroValueResolversFromAssemblies()
        {
            if(AssembliesWithMacroValueResolverTypes?.Any() ?? false )
                return AssembliesWithMacroValueResolverTypes.SelectMany(a => a.GetTypes()).Where(t => typeof(MacroValueResolver).IsAssignableFrom(t) && t.IsAbstract == false && t.IsInterface == false).ToArray();
            return Enumerable.Empty<Type>();
        }

        private Dictionary<string, MacroValueResolverType> InitializeMacroValueResolverTypes()
        {
            var result = new Dictionary<string, MacroValueResolverType>();
            var abstractType = typeof(IMacroValueResolver);
            var allMacroValueResolverTypes = (MacroValueResolverTypes ?? new List<Type>()).Concat(GetMacroValueResolversFromAssemblies());

            foreach (var customType in allMacroValueResolverTypes)
            {
                if (abstractType.IsAssignableFrom(customType) == false || customType.IsAbstract || customType.IsInterface)
                    throw new InvalidOperationException($"MacroValueResolver {customType.Name} must inherit \"{abstractType.Name}\" and not be abstract nor interface");
                var attribute = customType.GetCustomAttribute<MacroAttribute>();
                if (attribute == null)
                    throw new InvalidOperationException($"MacroValueResolver {customType.Name} must have the \"[Macro(....)\"] attribute");
                if (string.IsNullOrWhiteSpace(attribute.Name))
                    throw new InvalidOperationException($"MacroValueResolver {customType.Name} must have a name in \"[Macro(Name: \"?\")\"] attribute");

                var resolverType = new MacroValueResolverType(attribute, customType);
                if (result.TryGetValue(resolverType.Name, out var alreadyResolverType))
                    throw new InvalidOperationException($"MacroValueResolver {resolverType.Type.Name} conflicts with macro name \"{resolverType.Name}\" on already registered MacroValueResolver \"{alreadyResolverType.Type.Name}\"");

                result.Add(resolverType.Name, resolverType);
            }

            if (result.Any() == false)
                throw new InvalidOperationException($"No MacroValueResolver types found");

            return result;
        }

    }
}