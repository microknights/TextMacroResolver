using System.Threading.Tasks;
using MicroKnights.Texting.Results;

namespace MicroKnights.Texting.Resolvers
{
    public interface IMacroValueResolver
    {
        string DefaultTextFormat { get; }

        Task<MacroValueResult> Resolve(string originalMacroName, string textFormat);
    }
}