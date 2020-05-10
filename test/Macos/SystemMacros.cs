using System;
using System.Threading.Tasks;
using MicroKnights.Texting.Attributes;
using MicroKnights.Texting.Resolvers;
using MicroKnights.Texting.Results;

namespace UnitTest.Macos
{
    [Macro("system.datetime.now")]
    public class SystemDateTimeNowMacroValueResolver : MacroValueResolver
    {
        private readonly MacroValueContext _context;

        public SystemDateTimeNowMacroValueResolver(TestMacroValueContext context)
        {
            _context = context;
        }

        public override string DefaultTextFormat => "dddd d 'of' MMMM 'in year' yyyy";

        public override Task<MacroValueResult> Resolve(string originalMacroName, string textFormat)
        {
            return Task.FromResult(new MacroValueResult(originalMacroName, DateTime.Now, textFormat, _context.CultureInfo));
        }
    }

    [Macro("system.datetime.year", description: "", synchronize: true)]
    public class SystemDateTimeYearMacroValueResolver : MacroValueResolver
    {
        public override Task<MacroValueResult> Resolve(string originalMacroName, string textFormat)
        {
            return Task.FromResult(new MacroValueResult(originalMacroName, DateTime.Now.Year, textFormat));
        }
    }


    [Macro("system.datetime.ticks")]
    public class SystemTicksMacroValueResolver : MacroValueResolver
    {
        private readonly TestMacroValueContext _context;

        public SystemTicksMacroValueResolver(TestMacroValueContext context)
        {
            _context = context;
        }

        public override string DefaultTextFormat => "N0";

        public override Task<MacroValueResult> Resolve(string originalMacroName, string textFormat)
        {
            return Task.FromResult(new MacroValueResult(originalMacroName, DateTime.Now.Ticks, textFormat, _context.CultureInfo));
        }
    }

    [Macro("system.machine.name")]
    public class SystemMachineNameMacroValueResolver : MacroValueResolver
    {
        public override Task<MacroValueResult> Resolve(string originalMacroName, string textFormat)
        {
            return Task.FromResult(new MacroValueResult(originalMacroName, Environment.MachineName, textFormat));
        }
    }

    [Macro("context.entityid.check")]
    public class ContextEntityIdCheckMacroValueResolver : MacroValueResolver
    {
        private readonly TestMacroValueContext _context;

        public ContextEntityIdCheckMacroValueResolver(TestMacroValueContext context)
        {
            _context = context;
        }

        public override Task<MacroValueResult> Resolve(string originalMacroName, string textFormat)
        {
            if( _context.EntityId != 42 ) throw new SystemException($"Expected 42 in context entity id, got {_context.EntityId}");
            return Task.FromResult(new MacroValueResult(originalMacroName, _context.EntityId, textFormat));
        }
    }

}