using System;

namespace WebShell
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WebCommandAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public WebCommandAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
    }
}
