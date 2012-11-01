using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iRINGTools.Web.Helpers
{
  public class JsonRelationNode : TreeNode
  {
    public object relatedObjMap { get; set; }
    public string objectName { get; set; }
    public string relatedObjectName { get; set; }
    public string relationshipType { get; set; }
    public string relatedTableName { get; set; }
    public string relationshipTypeIndex { get; set; }
    public object propertyMap { get; set; }
  }
}