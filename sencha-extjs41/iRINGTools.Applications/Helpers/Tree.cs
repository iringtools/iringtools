using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iRINGTools.Web.Helpers
{
  public class Tree
  {
    public List<JsonTreeNode> nodes;
    public string securityRole; 

    public List<JsonTreeNode> getNodes()
    {
      if (nodes == null)
      {
        nodes = new List<JsonTreeNode>();
      }
      return this.nodes;
    }

    public void setNodes(List<JsonTreeNode> nodes)
    {
      this.nodes = nodes;
    }


  }
}