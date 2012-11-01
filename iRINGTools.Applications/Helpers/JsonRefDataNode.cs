using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iRINGTools.Web.Helpers
{
  public class JsonRefDataNode : TreeNode
  {
    public JsonRefDataNode()
    {
      hidden = false;
      iconCls = string.Empty;
    }

    public string Namespace { get; set; }
  }
}