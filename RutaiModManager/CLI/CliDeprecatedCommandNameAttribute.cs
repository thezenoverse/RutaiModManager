using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RutaiModManager.CLI
{
    public class CliDeprecatedCommandNameAttribute : Attribute
    {
        public string Name;

        public CliDeprecatedCommandNameAttribute(string name)
        {
            Name = name;
        }
    }
}
