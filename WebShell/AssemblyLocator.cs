using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WebShell
{
    internal static class AssemblyLocator
    {
        private static ReadOnlyCollection<Assembly> allAssemblies;

        public static ReadOnlyCollection<Assembly> GetAssemblies()
        {
            if(allAssemblies != null) return allAssemblies;
            //TODO: remove this after OWIN implementaiton new ReadOnlyCollection<Assembly>(BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToList());
            allAssemblies = new ReadOnlyCollection<Assembly>((from file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"bin\")
                                                  where Path.GetExtension(file) == ".dll"
                                                  select Assembly.LoadFrom(file)).ToList());


            return allAssemblies;
        }
    }
}