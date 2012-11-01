using System;
using System.Collections.Generic;
using org.iringtools.utility;
using org.iringtools.library;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using Ninject;
using log4net;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using org.iringtools.mapping;
using System.Net;

namespace org.iringtools.adapter.semantic
{
  public class dotNetRDFEngine : ISemanticLayer
  {
    private static readonly string DATALAYER_NS = "org.iringtools.adapter.datalayer";

    private static readonly XNamespace RDF_NS = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace OWL_NS = "http://www.w3.org/2002/07/owl#";
    private static readonly XNamespace XSD_NS = "http://www.w3.org/2001/XMLSchema#";
    private static readonly XNamespace XSI_NS = "http://www.w3.org/2001/XMLSchema-instance#";
    private static readonly XNamespace TPL_NS = "http://tpl.rdlfacade.org/data#";
    private static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";

    private static readonly XName OWL_THING = OWL_NS + "Thing";
    private static readonly XName RDF_ABOUT = RDF_NS + "about";
    private static readonly XName RDF_DESCRIPTION = RDF_NS + "Description";
    private static readonly XName RDF_TYPE = RDF_NS + "type";
    private static readonly XName RDF_RESOURCE = RDF_NS + "resource";
    private static readonly XName RDF_DATATYPE = RDF_NS + "datatype";

    private static readonly string RDF_PREFIX = "rdf:";
    private static readonly string RDF_NIL = RDF_PREFIX + "nil";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(dotNetRDFEngine));

    private AdapterSettings _settings = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private Graph _graph = null;  // dotNetRdf graph
    private MicrosoftAdoManager _tripleStore = null;
    private XNamespace _graphNs = String.Empty;
    private string _dataObjectsAssemblyName = String.Empty;
    private string _dataObjectNs = String.Empty;
    private Properties _uriMaps;

    [Inject]
    public dotNetRDFEngine(AdapterSettings settings, Mapping mapping)
    {
      _settings = settings;

      // load uri maps config
      _uriMaps = new Properties();

      string uriMapsFilePath = _settings["AppDataPath"] + "UriMaps.conf";

      if (File.Exists(uriMapsFilePath))
      {
        try
        {
          _uriMaps.Load(uriMapsFilePath);
        }
        catch (Exception e)
        {
          _logger.Info("Error loading [UriMaps.config]: " + e);
        }
      }

      _tripleStore = new MicrosoftAdoManager(
        _settings["dotNetRDFServer"],
        _settings["dotNetRDFCatalog"],
        _settings["dotNetRDFUser"],
        _settings["dotNetRDFPassword"]
        );

      _mapping = mapping;

      _graph = new Graph();

      string baseUri = _settings["GraphBaseUri"];
      string project = _settings["ProjectName"];
      string app = _settings["ApplicationName"];
      _graphNs = Utility.FormEndpointBaseURI(_uriMaps, baseUri, project, app);

      _dataObjectNs = String.Format("{0}.proj_{1}", DATALAYER_NS, _settings["Scope"]);

      _dataObjectsAssemblyName = _settings["ExecutingAssemblyName"];
    }

    public Response Refresh(string graphName, XDocument xDocument)
    {
      Response response = new Response();
      response.StatusList = new List<Status>();

      Status status = new Status();
      status.Messages = new Messages();

      try
      {
        status.Identifier = graphName;

        DateTime startTime = DateTime.Now;
        _graphMap = _mapping.FindGraphMap(graphName);

        // create xdoc from rdf xelement
        Uri graphUri = new Uri(_graphNs.NamespaceName + graphName);
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xDocument.ToString());
        xDocument.Root.RemoveAll();

        // load xdoc to graph
        RdfXmlParser parser = new RdfXmlParser();
        _graph.Clear();
        _graph.BaseUri = graphUri;
        parser.Load(_graph, xmlDocument);
        xmlDocument.RemoveAll();

        // delete old graph and save new one
        DeleteGraph(graphUri);
        _tripleStore.SaveGraph(_graph);

        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime.Subtract(startTime);

        status.Messages.Add("Graph [" + graphName + "] has been refreshed in triple store successfully.");
        status.Messages.Add(
          String.Format("Execution time [{0}:{1}.{2}] minutes.",
            duration.Minutes,
            duration.Seconds,
            duration.Milliseconds
          )
        );
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error refreshing graph [{0}]. {1}", graphName, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(String.Format("Error refreshing graph [{0}]. {1}", graphName, ex));
      }

      response.Append(status);
      return response;
    }

    public Response Delete(string graphName)
    {
      Uri graphUri = new Uri(_graphNs.NamespaceName + graphName);
      return DeleteGraph(graphUri);
    }

    #region helper methods
    private Response DeleteGraph(Uri graphUri)
    {
      Response response = new Response();
      response.StatusList = new List<Status>();

      Status status = new Status();
      status.Messages = new Messages();
      try
      {
        status.Identifier = graphUri.ToString();

        int graphId = _tripleStore.GetGraphID(graphUri);
        Uri uri = _tripleStore.GetGraphUri(graphId);
        _tripleStore.DeleteGraph(uri);

        status.Messages.Add(String.Format("Graph [{0}] has been deleted successfully.", graphUri));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error deleting graph [{0}]: {1}", graphUri, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(String.Format("Error deleting graph [{0}]: {1}", graphUri, ex));
      }

      response.Append(status);
      return response;
    }
    #endregion
  }
}
