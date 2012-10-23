using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iRINGTools.Web.Helpers
{
  public class Node
  {
    public String text { get; set; }
    public String iconCls { get; set; }
    public String identifier { get; set; }
    public bool hidden { get; set; }
    public String type { get; set; }
    public String nodeType { get; set; }
    public Object record { get; set; }
  }
}