using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iRINGTools.Web.Helpers
{

  public class LeafNode : JsonTreeNode
  {
    public bool isLeaf()
    {
      return leaf;
    }

    public void setLeaf(bool value)
    {
      this.leaf = value;
    }


  }
}