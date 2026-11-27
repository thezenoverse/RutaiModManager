using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RutaiModManager.Lang;

namespace RutaiModManager.Themes
{
    public class ThemeList : List<ThemeEntry>
    {
    }

    public class ThemeEntry
    {
        public string FileName { get; set; }
        public string Name { get; set; }

        public override string ToString() => Localise("Theme" + Name, Name);
    }
}
