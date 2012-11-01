using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.mapping;

namespace iRINGTools.Web.Models
{
  public interface IMappingRepository
  {
    Mapping GetMapping(string context, string endpoint, string baseUrl);
    void UpdateMapping(Mapping mapping, string context, string endpoint, string baseUrl);
  }
}