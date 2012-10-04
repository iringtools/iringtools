using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.sdk.objects.widgets
{
  public class Filter
  {
    public string AttributeName { get; set; }

    public string Logical { get; set; }

    public string RelationalOperator { get; set; }

    public string Value { get; set; }
  }

  //These should not be enums
  //public enum LogicalOperators
  //{
  //  none,
  //  not,
  //  and,
  //  andNot,
  //  or,
  //  orNot,
  //}

  //public enum RelationalOperators
  //{
  //  contains, 
  //  equalTo, 
  //  fullText, 
  //  notEqualTo, 
  //  like, 
  //  notLike, 
  //  @in, 
  //  notIn, 
  //  greaterThan, 
  //  greaterThanOrEqual, 
  //  lesserThan, 
  //  lesserThanOrEqual,
  //}
}
