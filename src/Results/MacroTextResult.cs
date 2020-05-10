using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroKnights.Texting.Results
{
    public class MacroTextResult
    {
        public MacroTextResult(string originalText, string resolvedText, IEnumerable<MacroValueResult> resolvedMacros)
        {
            OriginalText = originalText;
            ResolvedText = resolvedText;
            ResolvedMacros = resolvedMacros;
        }

        public bool IsResolved => ResolvedMacros.All(rm => rm.IsResolved);

        public string OriginalText { get; protected set; }
        public string ResolvedText { get; protected set; }

        public IEnumerable<MacroValueResult> ResolvedMacros { get; protected set; }

        public IEnumerable<Exception> Exceptions => ResolvedMacros.Where(rm => rm.IsResolved == false).Select(rm => rm.Exception);
    }
}