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
      string[] args = {
                "/run",
                "../../../NUnit.Tests/NUnit.Tests.csproj"
              };

      AppEntry.Main(args);
    }
  }
}
