using System;

namespace MicroKnights.Texting.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MacroAttribute : Attribute
    {
        public MacroAttribute(string name, string description = null, bool synchronize = false)
        {
            Name = name.ToLowerInvariant();
            Description = description;
            Synchronize = synchronize;
        }

        public string Name { get; }
        public string Description { get; }
        

        /// <summary>
        /// If using resources, as DbContext, set this to True - so they dont run i parallel.
        /// </summary>
        public bool Synchronize { get; }
    }
}