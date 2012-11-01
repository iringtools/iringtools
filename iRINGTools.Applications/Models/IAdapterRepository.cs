using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using org.iringtools.library;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;
using System.Collections;
using System.IO;


namespace iRINGTools.Web.Models
{
  public interface IAdapterRepository
  {
    Directories GetScopes();

    DataLayers GetDataLayers(string baseUrl);

    Tree GetDirectoryTree(string user);

    Mapping GetMapping(string contextName, string endpoint, string baseUrl);

    DataDictionary GetDictionary(string contextName, string endpoint, string baseUrl);

    Entity GetClassLabel(string classId);

    string Folder(string newFolderName, string description, string path, string state, string context, string oldContext, string user);

    string DeleteEntry(string path, string type, string context, string baseUrl, string user);

    string Endpoint(string newEndpointName, string path, string description, string states, string context, string oldAssembly, string newAssembly, string baseUrl, string oldBaseUrl, string user);

    string GetNodeIconCls(string type);

    string GetRootSecurityRole();

    string GetUserLdap();

    string TestBaseUrl(string baseUrl);

    Urls GetEndpointBaseUrl(string user);

    ContextNames GetFolderContexts(string user);

    string GetCombinationMsg();

    Response RegenAll(string user);

    Response SaveDataLayer(MemoryStream dataLayerStream);
      
  }
}