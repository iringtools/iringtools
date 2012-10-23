using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iRINGTools.Web.Helpers
{
  public class TreeNode : JsonTreeNode
  {
    public List<JsonTreeNode> children;

    public List<JsonTreeNode> getChildren()
    {
      if (children == null)
      {
        children = new List<JsonTreeNode>();
      }
      return this.children;
    }

    public void setChildren(List<JsonTreeNode> children)
    {
      this.children = children;
    }


  }
}