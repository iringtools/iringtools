using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;

namespace org.iringtools.web.Models
{
    public interface IFacadeRepository
    {
        Response RefreshGraph(string scope, string app, string graph, string baseUrl);
    }
}