namespace MicroKnights.Texting.Resolvers
{
    public class TextMacroResolverMatch
    {
        public TextMacroResolverMatch(string fullMatch, string macroName, string textFormat)
        {
            FullMatch = fullMatch;
            MacroName = macroName;
            TextFormat = textFormat;
        }

        public string FullMatch { get; }
        public string MacroName { get; }
        public string TextFormat { get; }
        public bool HasTextFormat => string.IsNullOrWhiteSpace(TextFormat) == false;
    }
}