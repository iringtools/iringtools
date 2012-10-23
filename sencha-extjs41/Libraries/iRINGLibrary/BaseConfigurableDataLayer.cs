using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.adapter;

namespace org.iringtools.library
{
  public abstract class BaseConfigurableDataLayer : BaseDataLayer  
  {
    public BaseConfigurableDataLayer(AdapterSettings settings) : base(settings) { }
  }
}
