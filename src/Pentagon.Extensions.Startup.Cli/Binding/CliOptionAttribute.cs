namespace Pentagon.Extensions.Startup.Cli
{
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

    public class CliOptionAttribute : Attribute
    {
        public string Name { get; set; }

        public CliOptionAttribute(params string[] aliases)
        {
            Aliases = aliases ?? Array.Empty<string>();
        }

        public string[] Aliases { get; set; }

        public string Description { get; set; }

        public bool IsRequired { get; set; }
    }
}