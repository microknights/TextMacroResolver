using System;
using MicroKnights.Texting.Attributes;

namespace MicroKnights.Texting.Resolvers
{
    internal class MacroValueResolverType
    {
        public MacroValueResolverType(MacroAttribute attribute, Type type)
        {
            _attribute = attribute;
            Type = type;
        }

        private readonly MacroAttribute _attribute;

        public string Name => _attribute.Name;
        public string Description => _attribute.Description;
        public bool Synchronize => _attribute.Synchronize;

        public Type Type { get; }
    }
}