using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RutaiModManager
{
    public class DummyLogger : ILogger
    {
        public static DummyLogger Instance { get; } = new DummyLogger();
        
        public void Write(string str)
        {
            
        }

        public void WriteLine(string str)
        {

        }
    }
}
