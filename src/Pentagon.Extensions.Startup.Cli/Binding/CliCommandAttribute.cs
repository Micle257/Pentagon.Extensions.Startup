namespace Pentagon.Extensions.Startup.Cli {
    using System;

    public class CliCommandAttribute : Attribute
    {
        public string Name { get; set; }

        public Type Parent { get; set; }

        public CliCommandAttribute(string name)
        {
            Name = name;
        }
    }
}