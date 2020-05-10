using System;
using System.Reflection;
using MicroKnights.Texting.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTest.Tests
{
    public class TestFixture<TMacroValueContext> where TMacroValueContext : class, IDisposable
    {
        protected IServiceScope ServiceScope { get; }
        public IServiceProvider ServiceProvider => ServiceScope.ServiceProvider;

        public TestFixture()
        {
            ServiceScope = new ServiceCollection()
                                .AddTextMacroResolver<TMacroValueContext>( options =>
                                {
                                    options.AssembliesWithMacroValueResolverTypes = new[] {Assembly.GetExecutingAssembly()};
                                })
                                .BuildServiceProvider()
                                .CreateScope();
        }

        public void Dispose()
        {
            ServiceScope.Dispose();
        }
    }
}