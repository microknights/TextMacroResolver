using System.Threading.Tasks;
using MicroKnights.Texting.Results;

namespace MicroKnights.Texting.Resolvers
{
    public abstract class MacroValueResolver : IMacroValueResolver
    {
        public virtual string DefaultTextFormat { get; } = null;

        public abstract Task<MacroValueResult> Resolve(string originalMacroName, string textFormat);
    }
}