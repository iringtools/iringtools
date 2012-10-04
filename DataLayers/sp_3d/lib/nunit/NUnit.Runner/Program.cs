using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Gui;

namespace NUnit
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            string commandLine = @"/run ../../../../../SPP3DDataLayer.NUnit/SPP3DDataLayer.NUnit.csproj";

            string[] args = commandLine.Split(' ');
            AppEntry.Main(args);
        }
    }
}
