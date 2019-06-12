namespace Pentagon.Extensions.Startup.Cli {
    using System;

    public class CliArgumentAttribute : Attribute
    {
        public int Index { get; }

        public bool IsRequired { get; set; }

        public CliArgumentAttribute(int index = -1)
        {
            Index = index;
        }
    }

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