using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iRINGTools.Web.Helpers
{
  public class JsonPropertyNode : TreeNode
  {
    public JsonPropertyNode()
    {
      hidden = false;
      iconCls = string.Empty;
    }
    public Dictionary<string, TreeNode> hiddenNodes { get; set; }
  }
}