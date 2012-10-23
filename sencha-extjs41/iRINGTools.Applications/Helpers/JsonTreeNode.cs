using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iRINGTools.Web.Helpers
{
  public class JsonTreeNode
  {
    public JsonTreeNode()
    {
      hidden = false;
      iconCls = string.Empty;
    }

    public string id { get; set; }
    public string identifier { get; set; }
    public string text { get; set; }
    public string icon { get; set; }
    public bool leaf { get; set; }
    public bool expanded { get; set; }
    public bool hidden { get; set; }
    public string type { get; set; }
    public string nodeType { get; set; }
    public object @checked { get; set; }
    public object record { get; set; }
    public Dictionary<string, string> property { get; set; }    
    public string iconCls { get; set; }    
  }
}