using System.Linq;
using System.Threading.Tasks;
using MicroKnights.Texting.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Macos;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.Tests
{
    public class TestMacros : IClassFixture<TestFixture<TestMacroValueContext>>
    {
        private readonly TestFixture<TestMacroValueContext> _fixture;
        private readonly ITestOutputHelper _outputHelper;
        private readonly TextMacroResolver _textMacroResolver;

        private readonly string TextWithMacros = @"{{system.datetime.now:yyyy mm DD}} 
Donec pharetra bibendum augue, congue dignissim mauris sodales pulvinar. 
Sed vitae est at sem volutpat placerat. 
Donec in sem ut massa accumsan rhoncus sed nec quam. 
Nam dictum luctus tortor et faucibus. 
{{system.datetime.now:o}}
Etiam sit amet lacus lacus. 
Morbi lobortis scelerisque interdum {{system.datetime.year}}. 
Aenean mollis, elit at pharetra ullamcorper, ligula mi maximus ligula, eget elementum est quam non tortor. 
Suspendisse iaculis {{system.datetime.ticks:N0}} pharetra diam sit amet auctor. 
Sed commodo pharetra dolor sit amet tincidunt. 
Quisque imperdiet tellus arcu, eu varius nulla tempus eget. 
Sed vel consequat ex {{system.machine.name}}. 
Vivamus lobortis tincidunt mattis {{system.machine.name}}. 
Praesent aliquam tincidunt arcu, vel facilisis ex dictum eu. 
{{system.datetime.now}}
Vivamus purus urna, finibus eu lacus eget, condimentum ullamcorper ipsum.
Level {{context.entityid.check}}";

        private readonly int MacroCount = 8;


        public TestMacros(TestFixture<TestMacroValueContext> fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _outputHelper = outputHelper;
            _textMacroResolver = fixture.ServiceProvider.GetRequiredService<TextMacroResolver>();
        }

        [Fact]
        public async Task TestMacroResolver()
        {
            var textMacroResolver = _fixture.ServiceProvider.GetRequiredService<TextMacroResolver>();
            var context = _fixture.ServiceProvider.GetRequiredService<TestMacroValueContext>();
            // Set scope context state
            context.EntityId = 42;

            foreach (var macroName in textMacroResolver.GetResolverMacroNames())
            {
                var result = await _textMacroResolver.ResolveMacro(macroName);
#if DEBUG
                _outputHelper.WriteLine($"{result.IsResolved} | {macroName} = {result.FormattedText}");
#endif       
                Assert.True(result.IsResolved);
            }
        }

        [Fact]
        public void TestExtractMacros()
        {
            var context = _fixture.ServiceProvider.GetRequiredService<TestMacroValueContext>();
            // Set scope context state
            context.EntityId = 42;

            var result = _textMacroResolver.ExtractMacros(TextWithMacros).ToArray();

            Assert.NotEmpty(result);
#if DEBUG
            foreach (var macro in result)
            {
                _outputHelper.WriteLine($"{macro.FullMatch} | {macro.MacroName} | {macro.TextFormat}");
            }
#endif
            Assert.True(result.Length == MacroCount, $"Expected {MacroCount} got {result.Length}");
        }

        [Fact]
        public async Task TestMacroText()
        {
            var context = _fixture.ServiceProvider.GetRequiredService<TestMacroValueContext>();
            // Set scope context state
            context.EntityId = 42;

            var result = await _textMacroResolver.ResolveText(TextWithMacros);

#if DEBUG
            foreach (var macro in result.ResolvedMacros)
            {
                _outputHelper.WriteLine($"{macro.OriginalMacroName} = \"{macro.FormattedText}\"");
            }
            _outputHelper.WriteLine(result.ResolvedText);
#endif
            var expectMacroCount = MacroCount - 1; // one is used twice...
            Assert.True(result.ResolvedMacros.Count() == expectMacroCount, $"Expected {expectMacroCount} got {result.ResolvedMacros.Count()}");
            Assert.True(result.IsResolved);
            Assert.NotStrictEqual(TextWithMacros, result.ResolvedText);
        }
    }
}
