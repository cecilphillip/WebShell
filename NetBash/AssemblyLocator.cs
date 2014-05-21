using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;

namespace NetBash
{
    internal static class AssemblyLocator
    {
        private static ReadOnlyCollection<Assembly> AllAssemblies = null;
        private static ReadOnlyCollection<Assembly> BinFolderAssemblies = null;

        public static ReadOnlyCollection<Assembly> GetAssemblies()
        {
            if(AllAssemblies != null) return AllAssemblies;
            //TODO: remove this after OWIN implementaiton new ReadOnlyCollection<Assembly>(BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToList());
            AllAssemblies = new ReadOnlyCollection<Assembly>((from file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"bin\")
                                                  where Path.GetExtension(file) == ".dll"
                                                  select Assembly.LoadFrom(file)).ToList());


            return AllAssemblies;
        }
    }
}
