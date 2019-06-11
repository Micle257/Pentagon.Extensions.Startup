namespace Pentagon.Extensions.Startup.Cli {
    using System;

    public class CommandAttribute : Attribute
    {
        public string Name { get; set; }

        public Type Parent { get; set; }

        public CommandAttribute(string name)
        {
            Name = name;
        }
    }
}