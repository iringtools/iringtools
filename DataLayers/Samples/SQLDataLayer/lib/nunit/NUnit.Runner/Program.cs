using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Gui;

namespace org.iringtools.nunit
{
    class Program
    {
        static void Main()
        {
          string commandLine = @"/run ../../../../../SQLDataLayer.NUnit/SQLDataLayer.NUnit.csproj";

            string[] args = commandLine.Split(' ');
            AppEntry.Main(args);
        }
    }
}
