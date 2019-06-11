namespace Pentagon.Extensions.Startup.Cli {
    using System;

    public class OptionsAttribute : Attribute
    {
        public string Name { get; set; }

        public OptionsAttribute(string name)
        {
            Name = name;
        }

        public string[] Aliases { get; set; }
    }
}