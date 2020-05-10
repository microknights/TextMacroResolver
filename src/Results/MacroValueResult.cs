using System;
using System.Globalization;

namespace MicroKnights.Texting.Results
{
    public class MacroValueResult
    {
        public MacroValueResult(string originalMacroName, Exception exception)
        {
            OriginalMacroName = originalMacroName;
            Exception = exception;
        }

        public MacroValueResult(string originalMacroName, object objectValue)
            : this(originalMacroName, objectValue, null, CultureInfo.CurrentCulture)
        {
        }

        public MacroValueResult(string originalMacroName, object objectValue, string textFormat)
            : this(originalMacroName, objectValue, textFormat, CultureInfo.CurrentCulture)
        {
        }

        public MacroValueResult(string originalMacroName, object objectValue, string textFormat, CultureInfo cultureInfo )
        {
            OriginalMacroName = originalMacroName;
            ObjectValue = objectValue;
            CultureInfo = cultureInfo;
            StringValue = ObjectValue as string ?? Convert.ToString(ObjectValue, cultureInfo);
            FormattedText = string.IsNullOrWhiteSpace(textFormat) ? StringValue : string.Format(cultureInfo, $"{{0:{textFormat}}}", ObjectValue);
        }

        public bool IsResolved => Exception == null;

        public string OriginalMacroName { get; protected set; }
        public object ObjectValue { get; protected set; }

        public CultureInfo CultureInfo { get; }

        public string StringValue { get; protected set; }
        public string FormattedText { get; protected set; }

        public Exception Exception { get; protected set; }
    }
}