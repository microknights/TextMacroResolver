using System;
using MicroKnights.Texting.Results;

namespace UnitTest.Macos
{
    public class TestMacroValueContext : MacroValueContext, IDisposable
    {
        public int EntityId { get; set; }

        public void Dispose()
        {
            // Only to support the IClassFixture
        }
    }
}