//// Copyright (c) 2010, iringtools.org //////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using log4net;
using Ninject;
using org.ids_adi.qmxf;
using org.iringtools.library;
using org.iringtools.utility;
using org.w3.sparql_results;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;
using VDS.RDF.Writing.Formatting;
using System.Net;
using org.iringtools.refdata.federation;


namespace org.iringtools.refdata
{
  public class ReferenceDataProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ReferenceDataProvider));

    private Response _response = null;

    private const string FEDERATION_FILE_NAME = "Federation.xml";
    private const string QUERIES_FILE_NAME = "Queries.xml";

    private NamespaceMapper nsMap = new NamespaceMapper();

    NTriplesFormatter formatter = new NTriplesFormatter();
    INode subj;
    INode pred;
    INode obj;

    //private bool qn = false;
    //private string qName = string.Empty;
    private const string insertData = "INSERT DATA {";
    private const string deleteData = "DELETE DATA {";
    private const string deleteWhere = "DELETE WHERE {";
    private const string rdfssubClassOf = "rdfs:subClassOf";
    private const string rdfType = "rdf:type";
    private int _pageSize = 0;

    private bool _useExampleRegistryBase = false;

    private List<Repository> _repositories = null;
    private Federation _federation = null;
    private List<Namespace> _namespaces = null;

    private Queries _queries = null;
    private static Dictionary<string, RefDataEntities> _searchHistory = new Dictionary<string, RefDataEntities>();
    private IKernel _kernel = null;
    private ReferenceDataSettings _settings = null;

    private StringBuilder prefix = new StringBuilder();
    private StringBuilder sparqlBuilder = new StringBuilder();
    private StringBuilder sparqlStr = new StringBuilder();
    private string defaultLanguage = "en";

    public ReferenceDataProvider(NameValueCollection settings)
    {
      try
      {
        this._kernel = new StandardKernel(new ReferenceDataModule());
        this._settings = this._kernel.Get<ReferenceDataSettings>();
        this._settings.AppendSettings(settings);
        Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);
        this._pageSize = Convert.ToInt32(_settings["PageSize"]);
        _useExampleRegistryBase = Convert.ToBoolean(_settings["UseExampleRegistryBase"]);
        string queriesPath = _settings["AppDataPath"] + QUERIES_FILE_NAME;
        _queries = Utility.Read<Queries>(queriesPath);
        string federationPath = _settings["AppDataPath"] + FEDERATION_FILE_NAME;
        if (File.Exists(federationPath))
        {
          this._federation = Utility.Read<Federation>(federationPath);
          this._repositories = this._federation.Repositories;
          this._namespaces = this._federation.Namespaces;
          foreach (var ns in this._namespaces)
          {
            nsMap.AddNamespace(ns.Prefix, new Uri(ns.Uri));
          }
        }
        _response = new Response();
        this._kernel.Bind<Response>().ToConstant(_response);

      }
      catch (Exception ex)
      {
        _logger.Error("Error in initializing ReferenceDataServiceProvider: " + ex);
      }
    }

    public Federation GetFederation()
    {
      return this._federation;
    }

    public List<Repository> GetRepositories()
    {
      try
      {
        List<Repository> repositories;
        repositories = this._repositories;
        //Don't Expose Tokens
        foreach (Repository repository in repositories)
        {
          repository.EncryptedCredentials = null;
        }
        return repositories;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetRepositories: " + ex);
        return null;
      }
    }

    private Repository GetRepository(string name)
    {
      return _repositories.Find(c => c.Name == name);
    }

    public RefDataEntities Search(string query)
    {
      try
      {
        return SearchPage(query, 0, 0);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Search: " + ex);
        throw new Exception("Error while Searching " + query + ".\n" + ex.ToString(), ex);
      }
    }

    public RefDataEntities SearchPage(string query, int start, int limit)
    {
        _logger.Debug("SearchPage");

      RefDataEntities entities = null;
      int counter = 0;
      Entity resultEntity = null;
      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;

        if (_searchHistory.ContainsKey(query))
        {
            _logger.Debug("SearchPage: Using History");

          entities = _searchHistory[query];
        }
        else
        {
          RefDataEntities resultEntities = new RefDataEntities();

          _logger.Debug("SearchPage: Preparing Queries");

          Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "ContainsSearch").Query;

          _logger.Debug("SearchPage: Got Contains Search");

          QueryBindings queryBindings = queryContainsSearch.Bindings;

          _logger.Debug("SearchPage: Got Bindings");

          foreach (QueryItem q in _queries)
          {
              _logger.DebugFormat("SearchPage: Looging for ContainsSearchJORD: {0}", q.Key);
          }

          QueryItem queryItem = _queries.FirstOrDefault(c => c.Key == "ContainsSearchJORD");

          if (queryItem != null)
          {
              _logger.Debug("SearchPage: Got QueryItem");
          }

            Query queryContainsSearchJORD = queryItem.Query;

          //Query queryContainsSearchJORD = (Query)_queries.FirstOrDefault(c => c.Key == "ContainsSearchJORD").Query;

          _logger.Debug("SearchPage: Got Contains Search JORD");

          QueryBindings queryBindingsJORD = queryContainsSearchJORD.Bindings;

          _logger.Debug("SearchPage: Got JORD Bindings");
            
          foreach (Repository repository in _repositories)
          {
            if (repository.RepositoryType == RepositoryType.JORD)
            {
                _logger.Debug("SearchPage: JORD!");

              sparql = ReadSPARQL(queryContainsSearchJORD.FileName);
              sparql = sparql.Replace("param1", query);
            }
            else
            {
                _logger.Debug("SearchPage: Other!");

              sparql = ReadSPARQL(queryContainsSearch.FileName);
              sparql = sparql.Replace("param1", query);
            }

            _logger.Debug("SearchPage: Query Repo");

            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

            foreach (SparqlResult result in sparqlResults)
            {
              resultEntity = new Entity();
              foreach (var v in result.Variables)
              {
                INode node = result[v];
                if (node is LiteralNode && v.Equals("label"))
                {
                  resultEntity.Label = ((LiteralNode)node).Value;
                  resultEntity.Lang = ((LiteralNode)node).Language;
                  if (string.IsNullOrEmpty(resultEntity.Lang))
                    resultEntity.Lang = defaultLanguage;
                }
                else if (node is UriNode && v.Equals("uri"))
                {
                  resultEntity.Uri = ((UriNode)node).Uri.ToString();
                }
                else if (node is UriNode && v.Equals("rds"))
                {
                  resultEntity.RDSUri = ((UriNode)node).Uri.ToString();
                }
              }
              resultEntity.Repository = repository.Name;
              string key = resultEntity.Label;
              if (resultEntity.Label.StartsWith("has") || resultEntity.Label.StartsWith("val"))
              {
                resultEntity = null;
                continue;
              }
              if (resultEntities.Entities.ContainsKey(key))
              {
                key += ++counter;
              }
              resultEntities.Entities.Add(key, resultEntity);
            }
            //results.Clear();
          }
          _searchHistory.Add(query, resultEntities);
          entities = resultEntities;
          entities.Total = resultEntities.Entities.Count;
        }

        if (limit > 0)
        {
          entities = GetRequestedPage(entities, start, limit);
        }

        _logger.Info(string.Format("SearchPage is returning {0} records", entities.Entities.Count));
        return entities;
      }
      catch (Exception e)
      {
        _logger.Error("Error in SearchPage: " + e);
        throw new Exception("Error while Finding " + query + ".\n" + e.ToString(), e);
      }
    }

    public RefDataEntities SearchReset(string query)
    {
      Reset(query);

      return Search(query);
    }

    public RefDataEntities SearchPageReset(string query, int start, int limit)
    {
      Reset(query);

      return SearchPage(query, start, limit);
    }

    private Entity GetLabel(string uri)
    {
      Entity labelEntity = new Entity();
      try
      {
        string label = String.Empty;
        string sparql = String.Empty;
        string relativeUri = String.Empty;

        Query query = (Query)_queries.FirstOrDefault(c => c.Key == "GetLabel").Query;
        Query queryEquivalent = (Query)_queries.FirstOrDefault(c => c.Key == "GetLabelRdlEquivalent").Query;


        foreach (Repository repository in _repositories)
        {
          if (repository.RepositoryType == RepositoryType.JORD && uri.Contains("#"))
          {
            sparql = ReadSPARQL(queryEquivalent.FileName).Replace("param1", uri);
          }
          else
          {
            sparql = ReadSPARQL(query.FileName).Replace("param1", uri);
          }
          SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

          foreach (SparqlResult result in sparqlResults)
          {
            foreach (var v in result.Variables)
            {
              if ((INode)result[v] is LiteralNode && v.Equals("label"))
              {
                labelEntity.Label = ((LiteralNode)result[v]).Value;
                labelEntity.Lang = ((LiteralNode)result[v]).Language;
                if (string.IsNullOrEmpty(labelEntity.Lang))
                  labelEntity.Lang = defaultLanguage;
              }
            }
            labelEntity.Repository = repository.Name;
            labelEntity.Uri = repository.Uri;
            break;
          }
        }

        return labelEntity;
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetLabel: " + e);
        throw new Exception("Error while Getting Label for " + uri + ".\n" + e.ToString(), e);
      }
    }

    private List<Classification> GetClassifications(string id, Repository rep)
    {
      QMXF qmxf = new QMXF();

      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;

        List<Classification> classifications = new List<Classification>();
        Query getClassification;

        foreach (Repository repository in _repositories)
        {
          if (rep != null)
            if (rep.Name != repository.Name) continue;

          switch (rep.RepositoryType)
          {
            case RepositoryType.Camelot:
            case RepositoryType.RDSWIP:
              getClassification = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassification").Query;
              sparql = ReadSPARQL(getClassification.FileName).Replace("param1", id);
              classifications = ProcessClassifications(rep, sparql);
              break;
            case RepositoryType.JORD:
              getClassification = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassificationJORD").Query;
              sparql = ReadSPARQL(getClassification.FileName).Replace("param1", id);
              classifications = ProcessClassifications(rep, sparql);
              break;
            case RepositoryType.Part8:
              getClassification = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Classification").Query;
              sparql = ReadSPARQL(getClassification.FileName).Replace("param1", id);
              classifications = ProcessClassifications(rep, sparql);
              break;
          }
        }

        return classifications;
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetClassifications: " + e);
        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
      }
    }

    private List<Classification> ProcessClassifications(Repository repository, string sparql)
    {

      SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

      List<Classification> classifications = new List<Classification>();
      List<string> names = new List<string>();
      string resultValue = string.Empty;

      foreach (SparqlResult result in sparqlResults)
      {
        Classification classification = new Classification();
        string uri = String.Empty;
        if (result.HasValue("uri"))
        {
          string pref = nsMap.GetPrefix(new Uri(result["uri"].ToString().Substring(0, result["uri"].ToString().IndexOf("#") + 1)));
          if (pref.Equals("owl") || pref.Contains("dm")) continue;
          uri = result["uri"].ToString();
          classification.reference = uri;
        }
        foreach (var v in result.Variables)
        {
          INode node = result[v];
          if (node is LiteralNode && v.Equals("label"))
          {
            classification.label = ((LiteralNode)node).Value;
            classification.lang = ((LiteralNode)node).Language;
          }
        }
        if (string.IsNullOrEmpty(classification.label))
        {
          Entity entity = GetLabel(uri);
          classification.label = entity.Label;
          classification.lang = entity.Lang;
        }
        if (string.IsNullOrEmpty(classification.lang))
          classification.lang = defaultLanguage;

        Utility.SearchAndInsert(classifications, classification, Classification.sortAscending());
      }

      return classifications;
    }

    private List<Specialization> GetSpecializations(string id, Repository rep)
    {
      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;
        SparqlResultSet sparqlResults = null;

        List<Specialization> specializations = new List<Specialization>();

        Query queryRdsWip = (Query)_queries.FirstOrDefault(c => c.Key == "GetSuperclass").Query;
        Query queryJord = (Query)_queries.FirstOrDefault(c => c.Key == "GetSuperclassJORD").Query;
        Query queryPart8 = (Query)_queries.FirstOrDefault(c => c.Key == "GetSuperClassOf").Query;

        foreach (Repository repository in _repositories)
        {
          if (rep != null)
            if (rep.Name != repository.Name) continue;

          switch (repository.RepositoryType)
          {
            case RepositoryType.Camelot:
            case RepositoryType.RDSWIP:

              sparql = ReadSPARQL(queryRdsWip.FileName).Replace("param1", id);
              sparqlResults = QueryFromRepository(repository, sparql);
              foreach (SparqlResult result in sparqlResults)
              {
                Specialization specialization = new Specialization();
                string uri = string.Empty;

                foreach (var v in result.Variables)
                {
                  INode node = result[v];

                  if (node is LiteralNode && v.Equals("label"))
                  {
                    specialization.label = ((LiteralNode)node).Value;
                    specialization.lang = ((LiteralNode)node).Language;
                  }
                  else if (node is UriNode && v.Equals("uri"))
                  {
                    specialization.reference = ((UriNode)node).Uri.ToString();
                    uri = specialization.reference;
                  }
                }
                if (string.IsNullOrEmpty(specialization.label))
                {
                  Entity entity = GetLabel(uri);
                  specialization.label = entity.Label;
                  specialization.lang = entity.Lang;
                }
                if (string.IsNullOrEmpty(specialization.lang))
                  specialization.lang = defaultLanguage;

                Utility.SearchAndInsert(specializations, specialization, Specialization.sortAscending());
              }
              break;

            case RepositoryType.Part8:
              sparql = ReadSPARQL(queryPart8.FileName).Replace("param1", id);
              sparqlResults = QueryFromRepository(repository, sparql);
              foreach (SparqlResult result in sparqlResults)
              {
                Specialization specialization = new Specialization();
                string uri = string.Empty;

                foreach (var v in result.Variables)
                {
                  INode node = result[v];
                  if (node is LiteralNode && v.Equals("label"))
                  {
                    specialization.label = ((LiteralNode)node).Value;
                    specialization.lang = ((LiteralNode)node).Language;
                  }
                  else if (node is UriNode && v.Equals("uri"))
                  {
                    specialization.reference = ((UriNode)node).Uri.ToString();
                    uri = specialization.reference;
                  }
                }
                if (string.IsNullOrEmpty(specialization.label))
                {
                  Entity entity = GetLabel(uri);
                  specialization.label = entity.Label;
                  specialization.lang = entity.Lang;
                }
                if (string.IsNullOrEmpty(specialization.lang))
                  specialization.lang = defaultLanguage;

                Utility.SearchAndInsert(specializations, specialization, Specialization.sortAscending());
              }
              break;
            case RepositoryType.JORD:
              sparql = ReadSPARQL(queryJord.FileName).Replace("param1", id);
              sparqlResults = QueryFromRepository(repository, sparql);
              foreach (SparqlResult result in sparqlResults)
              {
                Specialization specialization = new Specialization();
                string uri = string.Empty;

                foreach (var v in result.Variables)
                {
                  INode node = result[v];
                  if (node is LiteralNode && v.Equals("label"))
                  {
                    specialization.label = ((LiteralNode)node).Value;
                    specialization.lang = ((LiteralNode)node).Language;
                  }
                  else if (node is UriNode && v.Equals("uri"))
                  {
                    specialization.reference = ((UriNode)node).Uri.ToString();
                    uri = specialization.reference;
                  }
                  else if (node is UriNode && v.Equals("rdsuri"))
                  {
                    specialization.rdsuri = ((UriNode)node).Uri.ToString();
                    uri = specialization.reference;
                  }
                }
                if (string.IsNullOrEmpty(specialization.label))
                {
                  Entity entity = GetLabel(uri);
                  specialization.label = entity.Label;
                  specialization.lang = entity.Lang;
                }
                if (string.IsNullOrEmpty(specialization.lang))
                  specialization.lang = defaultLanguage;

                Utility.SearchAndInsert(specializations, specialization, Specialization.sortAscending());
              }
              break;
          }


        }

        return specializations;
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetSpecializations: " + e);
        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
      }
    }

    public Entity GetClassLabel(string id)
    {
      int number;
      bool isNumber = int.TryParse(id.Substring(1, 1), out number);
      if (isNumber)
        return GetLabel(this._namespaces.Find(ns => ns.Prefix == "rdl").Uri + id);
      else
        return GetLabel(this._namespaces.Find(ns => ns.Prefix == "jordrdl").Uri + id);
    }

    public QMXF GetClass(string id, Repository repository)
    {
      return GetClass(id, String.Empty, repository);
    }

    public QMXF GetClass(string id)
    {
      return GetClass(id, String.Empty, null);
    }

    public QMXF GetClass(string id, string namespaceUrl, Repository rep)
    {
      QMXF qmxf = new QMXF();
      if (!id.StartsWith("R")) return qmxf;
      try
      {
        QMXFName name;
        Description description;
        QMXFStatus status;

        List<Classification> classifications = new List<Classification>();
        List<Specialization> specializations = new List<Specialization>();

        RefDataEntities resultEntities = new RefDataEntities();
        List<Entity> resultEnt = new List<Entity>();
        string sparql = String.Empty;
        string relativeUri = String.Empty;
        string resultValue = string.Empty;
        string dataType = string.Empty;
        string uri = string.Empty;

        Query classQuery = (Query)_queries.FirstOrDefault(c => c.Key == "GetClass").Query;
        Query classQueryJord = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassJORD").Query;

        /// Always use rdl namespace
        namespaceUrl = this._namespaces.Find(n => n.Prefix == "rdl").Uri;
        uri = namespaceUrl + id;

        foreach (Repository repository in this._repositories)
        {

          if (rep != null)
            if (rep.Name != repository.Name) continue;

          if (repository.RepositoryType == RepositoryType.JORD)
          {
            sparql = ReadSPARQL(classQueryJord.FileName).Replace("param1", uri);
          }
          else
          {
            sparql = ReadSPARQL(classQuery.FileName).Replace("param1", uri);
          }
          ClassDefinition classDefinition = null;

          SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);
          classifications = new List<Classification>();
          specializations = new List<Specialization>();

          foreach (SparqlResult result in sparqlResults)
          {
            classDefinition = new ClassDefinition();

            classDefinition.identifier = uri;
            classDefinition.repositoryName = repository.Name;
            name = new QMXFName();
            description = new Description();
            status = new QMXFStatus();
            foreach (var v in result.Variables)
            {
              INode node = result[v];
              if (node is LiteralNode && v.Equals("label"))
              {
                name.value = ((ILiteralNode)node).Value;
                name.lang = ((ILiteralNode)node).Language;
                if (string.IsNullOrEmpty(name.lang))
                  name.lang = defaultLanguage;
              }
              else if (node is LiteralNode && v.Equals("definition"))
              {
                description.value = ((ILiteralNode)node).Value;
                description.lang = ((ILiteralNode)node).Language;
                if (string.IsNullOrEmpty(description.lang))
                  description.lang = defaultLanguage;
              }
              else if (node is LiteralNode && v.Equals("creator"))
              {
                status.authority = ((ILiteralNode)node).Value;
              }
              else if (node is LiteralNode && v.Equals("creationDate"))
              {
                status.from = ((ILiteralNode)node).Value;
              }
              else if (node is LiteralNode && v.Equals("class"))
              {
                status.Class = ((ILiteralNode)node).Value;
              }
              else if (node is LiteralNode && v.Equals("comment"))
              {
                description.value = ((ILiteralNode)node).Value;
                description.lang = ((ILiteralNode)node).Language;
                if (string.IsNullOrEmpty(description.lang))
                  description.lang = defaultLanguage;
              }
              else if (node is UriNode && v.Equals("type"))
              {
                string typeName = ((UriNode)node).Uri.ToString();
                string pref = nsMap.GetPrefix(new Uri(typeName.Substring(0, typeName.IndexOf("#") + 1)));
                if (pref.Contains("dm"))
                  classDefinition.entityType = new EntityType { reference = typeName };
              }
              else if (node is UriNode && v.Equals("authority"))
              {
                status.authority = ((UriNode)node).Uri.ToString();
              }
              else if (node is UriNode && v.Equals("recorded"))
              {
                status.Class = ((UriNode)node).Uri.ToString();
              }
              else if (node is LiteralNode && v.Equals("from"))
              {
                status.from = ((LiteralNode)node).Value;
              }
            }
            classDefinition.name.Add(name);

            classDefinition.description.Add(description);
            classDefinition.status.Add(status);

            classifications = GetClassifications(id, repository);

            specializations = GetSpecializations(id, repository);
            if (classifications.Count > 0)
              classDefinition.classification = classifications;
            if (specializations.Count > 0)
              classDefinition.specialization = specializations;
          }
          if (classDefinition != null)
            qmxf.classDefinitions.Add(classDefinition);
        }
        return qmxf;
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetClass: " + e);
        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
      }
    }

    public Entities GetSuperClasses(string id, Repository repo)
    {
      Entities queryResult = new Entities();
      string language = string.Empty;
      List<string> names = new List<string>();
      try
      {
        List<Specialization> specializations = GetSpecializations(id, repo);

        foreach (Specialization specialization in specializations)
        {
          string uri = specialization.reference;

          string label = specialization.label;

          if (label == null)
            label = GetLabel(uri).Label;
          names = label.Split('@').ToList();

          if (names.Count == 1)
            language = defaultLanguage;
          else
            language = names[names.Count - 1];


          Entity resultEntity = new Entity
          {
            Uri = uri,
            Label = names[0],
            Lang = language
          };
          Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
          //queryResult.Add(resultEntity);
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetSuperClasses: " + e);
        throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
      }
      return queryResult;
    }

    public Entities GetAllSuperClasses(string id)
    {
      Entities list = new Entities();
      return GetAllSuperClasses(id, list);
    }

    public Entities GetAllSuperClasses(string id, Entities list)
    {
      //List<Entity> queryResult = new List<Entity>();
      List<string> names = new List<string>();
      try
      {

        List<Specialization> specializations = GetSpecializations(id, null);
        //base case
        if (specializations.Count == 0)
        {
          return list;
        }

        foreach (Specialization specialization in specializations)
        {
          string uri = specialization.reference;
          string label = specialization.label;
          string language = string.Empty;

          if (label == null)
          {
            names = GetLabel(uri).Label.Split('@').ToList();
            label = names[0];
            if (names.Count == 1)
              language = defaultLanguage;
            else
              language = names[names.Count - 1];
          }
          Entity resultEntity = new Entity
          {
            Uri = uri,
            Label = label,
            Lang = language
          };

          string trimmedUri = string.Empty;
          bool found = false;
          foreach (Entity entt in list)
          {
            if (resultEntity.Uri.Equals(entt.Uri))
            {
              found = true;
            }
          }

          if (!found)
          {
            trimmedUri = uri.Remove(0, uri.LastIndexOf('#') + 1);
            Utility.SearchAndInsert(list, resultEntity, Entity.sortAscending());
            GetAllSuperClasses(trimmedUri, list);
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetAllSuperClasses: " + e);
        throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
      }

      return list;
    }

    public Entities GetClassMembers(string Id, Repository repo)
    {
      Entities membersResult = new Entities();
      try
      {
        string sparql = string.Empty;
        Entity resultEntity = null;
        SparqlResultSet sparqlResults;
        Query getMembers = (Query)_queries.FirstOrDefault(c => c.Key == "GetMembers").Query;
        sparql = ReadSPARQL(getMembers.FileName);
        Query getMembersP8 = (Query)_queries.FirstOrDefault(c => c.Key == "GetMembersPart8").Query;
        Query getMembersJORD = (Query)_queries.FirstOrDefault(c => c.Key == "GetMembersJORD").Query;


        foreach (Repository repository in _repositories)
        {
          if (repo != null)
            if (repository.Name != repo.Name) continue;

          if (repository.RepositoryType == RepositoryType.Part8)
          {
            sparql = ReadSPARQL(getMembersP8.FileName).Replace("param1", Id);
          }
          else if (repository.RepositoryType == RepositoryType.JORD)
          {
            sparql = ReadSPARQL(getMembersJORD.FileName).Replace("param1", Id);
          }
          else
          {
            sparql = ReadSPARQL(getMembers.FileName).Replace("param1", Id);
          }
          sparqlResults = QueryFromRepository(repository, sparql);
          foreach (SparqlResult result in sparqlResults)
          {
            resultEntity = new Entity();
            foreach (var v in result.Variables)
            {
              INode node = result[v];

              if (node is LiteralNode)
              {
                resultEntity.Label = ((LiteralNode)node).Value;
                if (string.IsNullOrEmpty(((LiteralNode)node).Language))
                {
                  resultEntity.Lang = defaultLanguage;
                }
                else
                {
                  resultEntity.Lang = ((LiteralNode)node).Language;
                }
              }
              else if (node is UriNode && v.Equals("uri"))
              {
                resultEntity.Uri = ((UriNode)node).Uri.ToString();
              }
              else if (node is UriNode && v.Equals("rds"))
              {
                resultEntity.RDSUri = ((UriNode)node).Uri.ToString();
              }
            }
            Utility.SearchAndInsert(membersResult, resultEntity, Entity.sortAscending());
            //queryResult.Add(resultEntity);
          }
        }

      }
      catch (Exception ex)
      {
        _logger.Error("Error in Getmembers: " + ex);
        throw new Exception("Error while Finding " + Id + ".\n" + ex.ToString(), ex);
      }
      return membersResult;
    }

    public Entities GetSubClasses(string id, Repository repo)
    {
      Entities queryResult = new Entities();

      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;
        Entity resultEntity = null;

        Query queryGetSubClasses = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClasses").Query;
        Query queryGetSubClassesJORD = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassesJORD").Query;
        Query queryGetSubClassesP8 = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassOf").Query;

        foreach (Repository repository in _repositories)
        {
          if (repo != null)
            if (repository.Name != repo.Name) continue;

          if (repository.RepositoryType == RepositoryType.Part8)
          {
             sparql = ReadSPARQL(queryGetSubClassesP8.FileName).Replace("param1", id); 
             SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);
            foreach (SparqlResult result in sparqlResults)
            {
              resultEntity = new Entity();
              foreach (var v in result.Variables)
              {
                INode node = result[v];
                if (node is LiteralNode)
                {
                  resultEntity.Label = ((LiteralNode)node).Value;
                  if (string.IsNullOrEmpty(((LiteralNode)node).Language))
                  {
                    resultEntity.Lang = defaultLanguage;
                  }
                  else
                  {
                    resultEntity.Lang = ((LiteralNode)node).Language;
                  }
                }
                else if (node is UriNode)
                {
                  resultEntity.Uri = ((UriNode)node).Uri.ToString();
                }
              }
              resultEntity.Repository = repository.Name;
              Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
              //queryResult.Add(resultEntity);
            }
          }
          else if (repository.RepositoryType == RepositoryType.JORD)
          {
            sparql = ReadSPARQL(queryGetSubClassesJORD.FileName).Replace("param1", id);
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

            foreach (SparqlResult result in sparqlResults)
            {
              resultEntity = new Entity();
              foreach (var v in result.Variables)
              {
                INode node = result[v];
                if (node is LiteralNode)
                {
                  resultEntity.Label = ((LiteralNode)result[v]).Value;
                  if (string.IsNullOrEmpty(((LiteralNode)result[v]).Language))
                  {
                    resultEntity.Lang = defaultLanguage;
                  }
                  else
                  {
                    resultEntity.Lang = ((LiteralNode)result[v]).Language;
                  }
                }
                else if (node is UriNode && v.Equals("rdsuri"))
                {
                  resultEntity.RDSUri = ((UriNode)result[v]).Uri.ToString();
                }
                else if (node is UriNode && v.Equals("uri"))
                {
                  resultEntity.Uri = ((UriNode)result[v]).Uri.ToString();
                }
              }
              resultEntity.Repository = repository.Name;

              Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
              //queryResult.Add(resultEntity);
            }
          }
          else
          {
            sparql = ReadSPARQL(queryGetSubClasses.FileName).Replace("param1", id);
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

            foreach (SparqlResult result in sparqlResults)
            {
              resultEntity = new Entity();
              foreach (var v in result.Variables)
              {
                INode node = result[v];
                if (node is LiteralNode)
                {
                  resultEntity.Label = ((LiteralNode)node).Value;
                  if (string.IsNullOrEmpty(((LiteralNode)node).Language))
                  {
                    resultEntity.Lang = defaultLanguage;
                  }
                  else
                  {
                    resultEntity.Lang = ((LiteralNode)node).Language;
                  }
                }
                else if (node is UriNode)
                {
                  resultEntity.Uri = ((UriNode)node).Uri.ToString();
                }
              }
              resultEntity.Repository = repository.Name;

              Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
              //queryResult.Add(resultEntity);
            }
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetSubClasses: " + e);
        throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
      }
      return queryResult;
    }

    public Entities GetSubClassesCount(string id)
    {
      Entities queryResult = new Entities();

      try
      {
        string sparql = String.Empty;
        string sparqlPart8 = String.Empty;
        string relativeUri = String.Empty;

        Query queryGetSubClasses = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassesCount").Query;

        sparql = ReadSPARQL(queryGetSubClasses.FileName);
        sparql = sparql.Replace("param1", id);

        Query queryGetSubClassOfInverse = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassOfCount").Query;

        sparqlPart8 = ReadSPARQL(queryGetSubClassOfInverse.FileName);
        sparqlPart8 = sparqlPart8.Replace("param1", id);

        int count = 0;
        foreach (Repository repository in _repositories)
        {
          if (repository.RepositoryType == RepositoryType.Part8)
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparqlPart8);

            foreach (SparqlResult result in sparqlResults)
            {
              foreach (var v in result.Variables)
              {
                count += Convert.ToInt32(((LiteralNode)result[v]).Value);
              }
            }
          }
          else
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

            foreach (SparqlResult result in sparqlResults)
            {
              foreach (var v in result.Variables)
              {
                count += Convert.ToInt32(((LiteralNode)result[v]).Value);
              }
            }
          }
        }
        Entity resultEntity = new Entity
        {
          Uri = string.Empty,
          Label = Convert.ToString(count),
          Lang = string.Empty,
        };

        Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetSubClasses: " + e);
        throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
      }
      return queryResult;
    }
    public Entities GetEntityTypes()
    {
      Entities queryResult = new Entities();
      string sparql = string.Empty;
      try
      {
        Query getEntities = (Query)_queries.FirstOrDefault(c => c.Key.Equals("GetEntityTypes")).Query;
        sparql = ReadSPARQL(getEntities.FileName);
        foreach (Repository rep in _repositories)
        {
          if (rep.Name.Equals("EntityTypes"))
          {
            SparqlResultSet sparqlResults = QueryFromRepository(rep, sparql);
            foreach (SparqlResult result in sparqlResults)
            {
              Entity entity = new Entity();
              foreach (String v in result.Variables)
              {
                if (((UriNode)result[v]).Uri != null)
                {
                  Uri e = ((UriNode)result[v]).Uri;
                  entity.Uri = e.ToString();
                  entity.Label = e.Fragment.Substring(1);
                }
              }
              queryResult.Add(entity);
            }
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetSubClasses: " + e);
        throw new Exception("Error getting EntityTypes " + e.ToString(), e);
      }

      return queryResult;
    }

    public Entities GetClassTemplates(string id)
    {
      Entities queryResult = new Entities();
      Entity resultEntity = null;

      try
      {
        string sparqlGetClassTemplates = String.Empty;
        string sparqlGetRelatedTemplates = String.Empty;
        string relativeUri = String.Empty;

        Query queryGetClassTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassTemplates").Query;

        sparqlGetClassTemplates = ReadSPARQL(queryGetClassTemplates.FileName);
        sparqlGetClassTemplates = sparqlGetClassTemplates.Replace("param1", id);

        Query queryGetRelatedTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetRelatedTemplates").Query;

        sparqlGetRelatedTemplates = ReadSPARQL(queryGetRelatedTemplates.FileName);
        sparqlGetRelatedTemplates = sparqlGetRelatedTemplates.Replace("param1", id);

        foreach (Repository repository in _repositories)
        {
          if (repository.RepositoryType == RepositoryType.Part8)
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparqlGetRelatedTemplates);

            foreach (SparqlResult result in sparqlResults.Results)
            {
              resultEntity = new Entity();
              foreach (var v in result.Variables)
              {
                if ((INode)result[v] is LiteralNode && v.Equals("label"))
                {
                  resultEntity.Label = ((LiteralNode)result[v]).Value;
                  if (string.IsNullOrEmpty(((LiteralNode)result[v]).Language))
                  {
                    resultEntity.Lang = defaultLanguage;
                  }
                  else
                  {
                    resultEntity.Lang = ((LiteralNode)result[v]).Language;
                  }
                }
                else if ((INode)result[v] is UriNode)
                {
                  resultEntity.Uri = ((UriNode)result[v]).Uri.ToString();
                }
              }
              resultEntity.Repository = repository.Name;
              Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
            }
          }
          else
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparqlGetClassTemplates);

            foreach (SparqlResult result in sparqlResults)
            {
              resultEntity = new Entity();
              foreach (var v in result.Variables)
              {
                if ((INode)result[v] is LiteralNode && v.Equals("label"))
                {
                  resultEntity.Label = ((LiteralNode)result[v]).Value;
                  if (string.IsNullOrEmpty(((LiteralNode)result[v]).Language))
                  {
                    resultEntity.Lang = defaultLanguage;
                  }
                  else
                  {
                    resultEntity.Lang = ((LiteralNode)result[v]).Language;
                  }
                }
                else if ((INode)result[v] is UriNode)
                {
                  resultEntity.Uri = ((UriNode)result[v]).Uri.ToString();
                }
              }
              resultEntity.Repository = repository.Name;
              Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
            }
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetClassTemplates: " + e);
        throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
      }
      return queryResult;
    }

    public Entities GetClassTemplatesCount(string id)
    {
      Entities queryResult = new Entities();

      try
      {
        string sparqlGetClassTemplates = String.Empty;
        string sparqlGetRelatedTemplates = String.Empty;
        string relativeUri = String.Empty;

        Query queryGetClassTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassTemplatesCount").Query;

        sparqlGetClassTemplates = ReadSPARQL(queryGetClassTemplates.FileName);
        sparqlGetClassTemplates = sparqlGetClassTemplates.Replace("param1", id);

        Query queryGetRelatedTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetRelatedTemplatesCount").Query;

        sparqlGetRelatedTemplates = ReadSPARQL(queryGetRelatedTemplates.FileName);
        sparqlGetRelatedTemplates = sparqlGetRelatedTemplates.Replace("param1", id);

        int count = 0;
        foreach (Repository repository in _repositories)
        {
          if (repository.RepositoryType == RepositoryType.Part8)
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparqlGetRelatedTemplates);

            foreach (SparqlResult result in sparqlResults)
            {
              foreach (var v in result.Variables)
              {
                count += Convert.ToInt32(((LiteralNode)result[v]).Value);
              }
            }
          }
          else
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparqlGetClassTemplates);

            foreach (SparqlResult result in sparqlResults)
            {
              foreach (var v in result.Variables)
              {
                count += Convert.ToInt32(((LiteralNode)result[v]).Value);
              }
            }
          }
        }
        Entity resultEntity = new Entity
        {
          Uri = string.Empty,
          Label = Convert.ToString(count),
          Lang = string.Empty,
        };

        Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetClassTemplates: " + e);
        throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
      }
      return queryResult;
    }

    private List<RoleDefinition> GetRoleDefinition(string id, Repository repository)
    {
      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;
        string sparqlQuery = string.Empty;

        Description description = new Description();
        QMXFStatus status = new QMXFStatus();

        List<RoleDefinition> roleDefinitions = new List<RoleDefinition>();
        RefDataEntities resultEntities = new RefDataEntities();

        switch (repository.RepositoryType)
        {
          case RepositoryType.Part8:
            sparqlQuery = "GetPart8Roles";
            break;
          case RepositoryType.Camelot:
          case RepositoryType.RDSWIP:
            sparqlQuery = "GetRoles";
            break;
        }


        Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == sparqlQuery).Query;

        sparql = ReadSPARQL(queryContainsSearch.FileName);
        sparql = sparql.Replace("param1", id);

        SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

        foreach (SparqlResult result in sparqlResults)
        {
          RoleDefinition roleDefinition = new RoleDefinition();
          QMXFName name = new QMXFName();
          foreach (var v in result.Variables)
          {
            if ((INode)result[v] is LiteralNode && v.Equals("label"))
            {
              name.value = ((LiteralNode)result[v]).Value;
              name.lang = ((LiteralNode)result[v]).Language;
              if (string.IsNullOrEmpty(name.lang))
                name.lang = defaultLanguage;
            }
            else if ((INode)result[v] is LiteralNode && v.Equals("comment"))
            {
              roleDefinition.description.value = ((LiteralNode)result[v]).Value;
              roleDefinition.description.lang = ((LiteralNode)result[v]).Language;
              if (string.IsNullOrEmpty(roleDefinition.description.lang))
                roleDefinition.description.lang = defaultLanguage;
            }
            else if ((INode)result[v] is LiteralNode && v.Equals("index"))
            {
              if (string.IsNullOrEmpty(roleDefinition.description.value))
              {
                roleDefinition.description.value = ((LiteralNode)result[v]).Value;
                roleDefinition.description.lang = ((LiteralNode)result[v]).Language;
                if (string.IsNullOrEmpty(roleDefinition.description.lang))
                  roleDefinition.description.lang = defaultLanguage;
              }
            }
            else if ((INode)result[v] is UriNode && v.Equals("type"))
            {
              roleDefinition.range = ((UriNode)result[v]).Uri.ToString();
            }
            else if ((INode)result[v] is UriNode && v.Equals("role"))
            {
              roleDefinition.identifier = ((UriNode)result[v]).Uri.ToString();
            }
          }
          roleDefinition.name.Add(name);

          Utility.SearchAndInsert(roleDefinitions, roleDefinition, RoleDefinition.sortAscending());
        }
        return roleDefinitions;
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetRoleDefinition: " + e);
        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
      }
    }

    private List<RoleDefinition> GetRoleDefinition(string id)
    {
      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;
        string sparqlQuery = string.Empty;

        Description description = new Description();
        QMXFStatus status = new QMXFStatus();

        List<RoleDefinition> roleDefinitions = new List<RoleDefinition>();

        RefDataEntities resultEntities = new RefDataEntities();

        foreach (Repository repository in _repositories)
        {
          switch (repository.RepositoryType)
          {
            case RepositoryType.Camelot:
            case RepositoryType.RDSWIP:
              sparqlQuery = "GetRoles";
              break;
            case RepositoryType.Part8:
              sparqlQuery = "GetPart8Roles";
              break;
          }
          Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == sparqlQuery).Query;

          sparql = ReadSPARQL(queryContainsSearch.FileName);
          sparql = sparql.Replace("param1", id);
          SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

          foreach (SparqlResult result in sparqlResults)
          {
            RoleDefinition roleDefinition = new RoleDefinition();
            QMXFName name = new QMXFName();

            if (result["label"] != null)
            {
              name.value = ((LiteralNode)result["label"]).Value;
              name.lang = ((LiteralNode)result["label"]).Language;
              if (string.IsNullOrEmpty(name.lang))
                name.lang = defaultLanguage;
            }
            if (result["comment"] != null)
            {
              description.value = ((LiteralNode)result["comment"]).Value;
              description.lang = ((LiteralNode)result["comment"]).Language;
              if (string.IsNullOrEmpty(description.lang))
                description.lang = defaultLanguage;
            }
            if (result["index"] != null)
            {
              if (string.IsNullOrEmpty(description.value))
              {
                description.value = ((LiteralNode)result["index"]).Value;
                description.lang = ((LiteralNode)result["index"]).Language;
                if (string.IsNullOrEmpty(description.lang))
                  description.lang = defaultLanguage;
              }
            }
            if (result["role"] != null)
            {
              roleDefinition.identifier = ((UriNode)result["role"]).Uri.ToString();
            }
            if (result["type"] != null)
            {
              roleDefinition.range = ((UriNode)result["type"]).Uri.ToString();
            }

            roleDefinition.description = description;
            roleDefinition.name.Add(name);
            Utility.SearchAndInsert(roleDefinitions, roleDefinition, RoleDefinition.sortAscending());
            //roleDefinitions.Add(roleDefinition);
          }
        }

        return roleDefinitions;
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetRoleDefinition: " + e);
        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
      }
    }

    private List<RoleQualification> GetRoleQualification(string id, Repository rep)
    {
      try
      {

        Description description = new Description();
        QMXFStatus status = new QMXFStatus();
        string uri = String.Empty;
        string nameValue = string.Empty;

        List<RoleQualification> roleQualifications = new List<RoleQualification>();

        foreach (Repository repository in _repositories)
        {
          if (rep != null)
            if (rep.Name != repository.Name) continue;
          switch (rep.RepositoryType)
          {
            case RepositoryType.Camelot:
            case RepositoryType.RDSWIP:

              string rangeSparql = String.Empty;
              string relativeUri = String.Empty;
              string referenceSparql = String.Empty;
              string relativeUri1 = String.Empty;
              string valueSparql = String.Empty;
              string relativeUri2 = String.Empty;

              RefDataEntities rangeResultEntities = new RefDataEntities();
              RefDataEntities referenceResultEntities = new RefDataEntities();
              RefDataEntities valueResultEntities = new RefDataEntities();

              Query getRangeRestriction = (Query)_queries.FirstOrDefault(c => c.Key == "GetRangeRestriction").Query;

              Query getReferenceRestriction = (Query)_queries.FirstOrDefault(c => c.Key == "GetReferenceRestriction").Query;

              Query getValueRestriction = (Query)_queries.FirstOrDefault(c => c.Key == "GetValueRestriction").Query;

              rangeSparql = ReadSPARQL(getRangeRestriction.FileName);
              rangeSparql = rangeSparql.Replace("param1", id);

              referenceSparql = ReadSPARQL(getReferenceRestriction.FileName);
              referenceSparql = referenceSparql.Replace("param1", id);

              valueSparql = ReadSPARQL(getValueRestriction.FileName);
              valueSparql = valueSparql.Replace("param1", id);

              SparqlResultSet rangeSparqlResults = QueryFromRepository(repository, rangeSparql);
              SparqlResultSet referenceSparqlResults = QueryFromRepository(repository, referenceSparql);
              SparqlResultSet valueSparqlResults = QueryFromRepository(repository, valueSparql);

              SparqlResultSet combinedResults = rangeSparqlResults;
              combinedResults.Results.AddRange(referenceSparqlResults);
              combinedResults.Results.AddRange(valueSparqlResults);

              foreach (SparqlResult result in combinedResults)
              {
                RoleQualification roleQualification = new RoleQualification();
                QMXFName name = new QMXFName();
                QMXFValue refvalue = new QMXFValue();
                QMXFValue valvalue = new QMXFValue();
                foreach (var v in result.Variables)
                {
                  if (v.Equals("qualifies") && result.HasValue(v))
                  {
                    uri = ((UriNode)result[v]).Uri.ToString();
                    roleQualification.qualifies = uri;
                    roleQualification.identifier = Utility.GetIdFromURI(uri);
                  }
                  else if (v.Equals("name"))
                  {
                    if (result[v] == null)
                    {
                      Entity entity = GetLabel(uri);
                      name.value = entity.Label;
                      name.lang = entity.Lang;
                    }
                    else
                    {
                      name.value = ((LiteralNode)result[v]).Value;
                      name.lang = ((LiteralNode)result[v]).Language;
                    }
                    if (string.IsNullOrEmpty(name.lang))
                    {
                      name.lang = defaultLanguage;
                    }

                  }
                  else if (v.Equals("range") && result.HasValue(v))
                  {
                    roleQualification.range = ((UriNode)result[v]).Uri.ToString();
                  }
                  else if (v.Equals("reference") && result.HasValue(v))
                  {
                    refvalue.reference = ((UriNode)result[v]).Uri.ToString();
                    roleQualification.value = refvalue;
                  }
                  else if (v.Equals("value") && result.HasValue(v))
                  {
                    valvalue.text = ((LiteralNode)result[v]).Value;
                    valvalue.As = ((UriNode)result["value_dataType"]).Uri.ToString();
                    roleQualification.value = valvalue;
                  }
                }
                roleQualification.name.Add(name);
                Utility.SearchAndInsert(roleQualifications, roleQualification, RoleQualification.sortAscending());
              }
              break;
            case RepositoryType.Part8:
              RefDataEntities part8Entities = new RefDataEntities();
              Query getPart8Roles = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Roles").Query;
              QueryBindings getPart8RolesBindings = getPart8Roles.Bindings;

              string part8RolesSparql = ReadSPARQL(getPart8Roles.FileName);
              part8RolesSparql = part8RolesSparql.Replace("param1", id);
              SparqlResultSet part8RolesResults = QueryFromRepository(repository, part8RolesSparql);
              foreach (SparqlResult result in part8RolesResults)
              {
                RoleQualification roleQualification = new RoleQualification();
                QMXFName name = new QMXFName();
                QMXFValue refvalue = new QMXFValue();
                QMXFValue valvalue = new QMXFValue();
                Description descr = new Description();

                if (result["role"] != null)
                {
                  uri = ((UriNode)result["role"]).Uri.ToString();
                  roleQualification.qualifies = uri;
                  roleQualification.identifier = Utility.GetIdFromURI(uri);
                }
                if (result["comment"] != null)
                {
                  descr.value = ((LiteralNode)result["comment"]).Value;
                  if (string.IsNullOrEmpty(((LiteralNode)result["comment"]).Language))
                  {
                    descr.lang = defaultLanguage;
                  }
                  else
                  {
                    descr.lang = ((LiteralNode)result["comment"]).Language;
                  }
                }
                if (result["type"] != null)
                {
                  roleQualification.range = ((UriNode)result["type"]).Uri.ToString();
                }
                if (result["label"] == null)
                {
                  Entity entity = GetLabel(uri);
                  name.value = entity.Label;
                  name.lang = entity.Lang;
                }
                else
                {
                  name.value = ((LiteralNode)result["label"]).Value;
                  name.lang = ((LiteralNode)result["label"]).Language;
                }
                if (string.IsNullOrEmpty(name.lang))
                  name.lang = defaultLanguage;

                if (result["index"] != null)
                {
                  ///TODO
                }

                roleQualification.name.Add(name);
                Utility.SearchAndInsert(roleQualifications, roleQualification, RoleQualification.sortAscending());

              }
              break;
          }
        }
        return roleQualifications;
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetRoleQualification: " + e);
        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
      }
    }

    private List<TemplateDefinition> GetTemplateDefinition(string id, Repository rep)
    {
      List<TemplateDefinition> templateDefinitionList = new List<TemplateDefinition>();
      TemplateDefinition templateDefinition = null;

      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;
        string qId = string.Empty;
        Query queryContainsSearch = null;
        Description description = new Description();
        QMXFStatus status = new QMXFStatus();

        if (!id.Contains(":"))
          qId = string.Format("tpl:{0}", id);
        else
          qId = id;

        foreach (Repository repository in _repositories)
        {
          if (rep != null)
            if (rep.Name != repository.Name) continue;

          if (repository.RepositoryType == RepositoryType.Part8)
          {
            queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetBaseTemplatePart8").Query;
          }
          else
          {
            queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetTemplate").Query;
          }

          sparql = ReadSPARQL(queryContainsSearch.FileName);
          sparql = sparql.Replace("param1", id);

          SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

          foreach (SparqlResult result in sparqlResults)
          {
            if (result.Count == 0) continue;
            templateDefinition = new TemplateDefinition();
            QMXFName name = new QMXFName();
            templateDefinition.repositoryName = repository.Name;

            foreach (var v in result.Variables)
            {
              if ((INode)result[v] is LiteralNode && v.Equals("label"))
              {
                name.value = ((LiteralNode)result[v]).Value;
                name.lang = ((LiteralNode)result[v]).Language;
                if (string.IsNullOrEmpty(name.lang))
                  name.lang = defaultLanguage;
              }
              else if ((INode)result[v] is LiteralNode && v.Equals("definition"))
              {
                description.value = ((LiteralNode)result[v]).Value;
                description.lang = ((LiteralNode)result[v]).Language;
                if (string.IsNullOrEmpty(description.lang))
                  description.lang = defaultLanguage;
              }
              else if ((INode)result[v] is LiteralNode && v.Equals("creationDate"))
              {
                status.from = ((LiteralNode)result[v]).Value;
              }
            }

            templateDefinition.identifier = qId;
            templateDefinition.name.Add(name);
            templateDefinition.description.Add(description);
            templateDefinition.status.Add(status);

            templateDefinition.roleDefinition = GetRoleDefinition(id, repository);
            templateDefinitionList.Add(templateDefinition);
          }
        }

        return templateDefinitionList;
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetTemplateDefinition: " + e);
        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
      }
    }

    public QMXF GetTemplate(string id, QMXFType templateType, Repository rep)
    {
      QMXF qmxf = new QMXF();
      List<TemplateQualification> templateQualification = null;
      List<TemplateDefinition> templateDefinition = null;
      try
      {
        if (templateType == QMXFType.Qualification)
        {
          templateQualification = GetTemplateQualification(id, rep);
        }
        else
        {
          templateDefinition = GetTemplateDefinition(id, rep);
        }

        if (templateQualification != null)
        {
          qmxf.templateQualifications = templateQualification;
        }
        else
        {
          qmxf.templateDefinitions = templateDefinition;
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetTemplate: " + ex);
      }

      return qmxf;
    }

    public QMXF GetTemplate(string id)
    {
      QMXF qmxf = new QMXF();

      try
      {
        List<TemplateQualification> templateQualification = GetTemplateQualification(id, null);

        if (templateQualification.Count > 0)
        {
          qmxf.templateQualifications = templateQualification;
        }
        else
        {
          List<TemplateDefinition> templateDefinition = GetTemplateDefinition(id, null);
          qmxf.templateDefinitions = templateDefinition;
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetTemplate: " + ex);
      }

      return qmxf;
    }


    private List<TemplateQualification> GetTemplateQualification(string id, Repository rep)
    {
      TemplateQualification templateQualification = null;
      List<TemplateQualification> templateQualificationList = new List<TemplateQualification>();

      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;
        string sparqlQuery = string.Empty;
        string dataType = string.Empty;
        string qId = string.Empty;
        Query getTemplateQualification = null;

        if (!id.Contains(":"))
          qId = string.Format("tpl:{0}", id);
        else
          qId = id;

        {
          foreach (Repository repository in _repositories)
          {
            if (rep != null)
              if (rep.Name != repository.Name) continue;

            switch (repository.RepositoryType)
            {
              case RepositoryType.Camelot:
              case RepositoryType.RDSWIP:
              case RepositoryType.JORD:
                sparqlQuery = "GetTemplateQualification";
                break;
              case RepositoryType.Part8:
                sparqlQuery = "GetTemplateQualificationPart8";
                break;

              //case RepositoryType.JORD:
              //  sparqlQuery = "GetTemplateQualificationJORD";
              //  break;
            }

            getTemplateQualification = (Query)_queries.FirstOrDefault(c => c.Key == sparqlQuery).Query;

            sparql = ReadSPARQL(getTemplateQualification.FileName);
            sparql = sparql.Replace("param1", id);

            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

            foreach (SparqlResult result in sparqlResults.Results)
            {
              templateQualification = new TemplateQualification();
              Description description = new Description();
              QMXFStatus status = new QMXFStatus();
              QMXFName name = new QMXFName();

              templateQualification.repositoryName = repository.Name;

              foreach (var v in result.Variables)
              {
                if ((INode)result[v] is LiteralNode && v.Equals("name"))
                {
                  name.value = ((LiteralNode)result[v]).Value;
                  name.lang = ((LiteralNode)result[v]).Language;
                  if (string.IsNullOrEmpty(name.lang))
                    name.lang = defaultLanguage;
                }
                else if ((INode)result[v] is LiteralNode && v.Equals("description"))
                {
                  description.value = ((LiteralNode)result[v]).Value;
                  description.lang = ((LiteralNode)result[v]).Language;
                  if (string.IsNullOrEmpty(description.lang))
                    description.lang = defaultLanguage;
                }
                else if ((INode)result[v] is UriNode && v.Equals("statusClass"))
                {
                  status.Class = ((UriNode)result[v]).Uri.ToString();
                }
                else if ((INode)result[v] is UriNode && v.Equals("statusAuthority"))
                {
                  status.authority = ((UriNode)result[v]).Uri.ToString();
                }
                else if ((INode)result[v] is UriNode && v.Equals("qualifies"))
                {
                  templateQualification.qualifies = ((UriNode)result[v]).Uri.ToString();
                }
              }

              templateQualification.identifier = qId;
              templateQualification.name.Add(name);
              templateQualification.description.Add(description);
              templateQualification.status.Add(status);

              templateQualification.roleQualification = GetRoleQualification(id, repository);
              templateQualificationList.Add(templateQualification);
            }
          }
        }
        return templateQualificationList;
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetTemplateQualification: " + e);
        throw new Exception("Error while Getting Template: " + id + ".\n" + e.ToString(), e);
      }
    }

    private int getIndexFromName(string name)
    {
      try
      {
        int index = 0;
        foreach (Repository repository in _repositories)
        {
          if (repository.Name.Equals(name))
          {
            index = _repositories.IndexOf(repository);
            return index;
          }
        }
        foreach (Repository repository in _repositories)
        {
          if (!repository.IsReadOnly)
          {
            index = _repositories.IndexOf(repository);
            return index;
          }
        }

        return index;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    ///  this will generate an id formatted as R + new Guid replacing '_' with blank '' space
    ///  example = RC2E15CCD8F104DD69188E6A5A23354B1
    /// </summary>
    /// <param name="RegistryBase"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private string CreateNewGuidId(string RegistryBase)//, string name)
    {
      if(!string.IsNullOrEmpty(RegistryBase))
      return string.Format("{0}R{1}",RegistryBase, Guid.NewGuid().ToString().Replace("_","").Replace("-","").ToUpper());
      else
      {
        _logger.Error("Failed to create id:");
        throw new Exception("CreateIdsAdiId: Failed to create id ");

      }

    }

    private List<Dictionary<string, string>> MergeLists(List<Dictionary<string, string>> a, List<Dictionary<string, string>> b)
    {
      try
      {
        foreach (Dictionary<string, string> dictionary in b)
        {
          a.Add(dictionary);
        }
        return a;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    /// Read query from file
    /// </summary>
    private string ReadSPARQL(string queryName)
    {
      try
      {
        string query;

        query = Utility.ReadString(_settings["SparqlPath"] + queryName);

        return query;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private SparqlResultSet QueryFromRepository(Repository repository, string sparql)
    {
      try
      {
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(repository.Uri));
        
        string encryptedCredentials = repository.EncryptedCredentials;
        WebCredentials cred = new WebCredentials(encryptedCredentials);
        if (cred.isEncrypted) cred.Decrypt();

        if (!string.IsNullOrEmpty(_settings["ProxyHost"])
          && !string.IsNullOrEmpty(_settings["ProxyPort"])
          && !string.IsNullOrEmpty(_settings["ProxyCredentialToken"])) /// need to use proxy
        {
          WebProxyCredentials pcred = _settings.GetWebProxyCredentials();
          endpoint.Proxy = pcred.GetWebProxy() as WebProxy;
          endpoint.ProxyCredentials = pcred.GetNetworkCredential();
        }
        endpoint.Credentials = cred.GetNetworkCredential();

        SparqlResultSet resultSet = endpoint.QueryWithResultSet(sparql);
        //endpoint.QueryWithResultSet(resultHandler, sparql);
         return resultSet;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Failed to read repository['{0}']", repository.Uri), ex);
        return new SparqlResultSet();
      }
    }

    private Response PostToRepository(Repository repository, string sparql)
    {
      try
      {
        Response response = new Response();

        SparqlRemoteUpdateEndpoint endpoint = new SparqlRemoteUpdateEndpoint(repository.UpdateUri);
        string encryptedCredentials = repository.EncryptedCredentials;
        WebCredentials cred = new WebCredentials(encryptedCredentials);
        if (cred.isEncrypted) cred.Decrypt();

        if (!string.IsNullOrEmpty(_settings["ProxyHost"])
          && !string.IsNullOrEmpty(_settings["ProxyPort"])
          && !string.IsNullOrEmpty(_settings["ProxyCredentialToken"])) /// need to use proxy
        {
          WebProxyCredentials pcred = _settings.GetWebProxyCredentials();
          endpoint.Proxy = pcred.GetWebProxy() as WebProxy;
          endpoint.ProxyCredentials = pcred.GetNetworkCredential();
        }

        endpoint.Credentials = cred.GetNetworkCredential();
        endpoint.Update(sparql);

        return response;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private string MakeUniqueKey(Dictionary<string, string> dictionary, string duplicateKey)
    {
      try
      {
        string newKey = String.Empty;

        for (int i = 2; i < Int32.MaxValue; i++)
        {
          string postfix = " (" + i.ToString() + ")";
          if (!dictionary.ContainsKey(duplicateKey + postfix))
          {
            newKey += postfix;
            break;
          }
        }

        return newKey;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private RefDataEntities GetRequestedPage(RefDataEntities rde, int startIdx, int pageSize)
    {
      try
      {
        RefDataEntities page = new RefDataEntities();
        page.Total = rde.Entities.Count;

        for (int i = startIdx; i < startIdx + pageSize; i++)
        {
          if (rde.Entities.Count == i) break;

          string key = rde.Entities.Keys[i];
          Entity entity = rde.Entities[key];
          page.Entities.Add(key, entity);
        }

        return page;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private void Reset(string query)
    {
      try
      {
        if (_searchHistory.ContainsKey(query))
        {
          _searchHistory.Remove(query);
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public Response PostTemplate(QMXF qmxf)
    {
      Graph delete = new Graph();
      Graph insert = new Graph();
      //add namespaces to graphs 
      delete.NamespaceMap.AddNamespace("rdl", new Uri("http://rdl.rdlfacade.org/data#"));
      delete.NamespaceMap.AddNamespace("tpl", new Uri("http://tpl.rdlfacade.org/data#"));
      delete.NamespaceMap.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
      delete.NamespaceMap.AddNamespace("dm", new Uri("http://dm.rdlfacade.org/data#"));
      delete.NamespaceMap.AddNamespace("p8", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/template-model#"));
      insert.NamespaceMap.Import(delete.NamespaceMap);

      Response response = new Response();
      response.Level = StatusLevel.Success;
      Repository repository = null;
      //bool qn = false;
      try
      {
        repository = GetRepository(qmxf.targetRepository);
        if (repository == null || repository.IsReadOnly)
        {
          Status status = new Status();
          status.Level = StatusLevel.Error;
          if (repository == null)
            status.Messages.Add("Repository not found!");
          else
            status.Messages.Add("Repository [" + qmxf.targetRepository + "] is read-only!");

          _response.Append(status);
        }
        else
        {
          string registry = _useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["ClassRegistryBase"];
          #region Template Definitions
          ///////////////////////////////////////////////////////////////////////////////
          /// Base templates do have the following properties
          /// 1) Base class of owl:Thing
          /// 2) rdfs:subClassOf = p8:BaseTemplateStateMent
          /// 3) rdfs:label name of template
          /// 4) optional rdfs:comment
          /// 5) p8:valNumberOfRoles
          /// 6) p8:hasTemplate = tpl:{TemplateName} - this probably could be eliminated -- pointer to self 
          ///////////////////////////////////////////////////////////////////////////////
          if (qmxf.templateDefinitions.Count > 0)
          {
            foreach (TemplateDefinition newTDef in qmxf.templateDefinitions)
            {
              string language = string.Empty;
              int roleCount = 0;
              string templateName = string.Empty;
              string templateId = string.Empty;
              string generatedId = string.Empty;
              string roleDefinition = string.Empty;
              int index = 1;
              if (!string.IsNullOrEmpty(newTDef.identifier))
                templateId = Utility.GetIdFromURI(newTDef.identifier);

              templateName = newTDef.name[0].value;
              //check for exisitng template
              QMXF oldQmxf = new QMXF();
              if (!String.IsNullOrEmpty(templateId))
              {
                oldQmxf = GetTemplate(templateId, QMXFType.Definition, repository);
              }
              else
              {
                if (_useExampleRegistryBase)
                  generatedId = CreateNewGuidId(_settings["ExampleRegistryBase"]);//, templateName);
                else
                  generatedId = CreateNewGuidId(_settings["TemplateRegistryBase"]);//, templateName);
                templateId = Utility.GetIdFromURI(generatedId);
              }

              #region Form Delete/Insert
              if (oldQmxf.templateDefinitions.Count > 0)
              {
                foreach (TemplateDefinition oldTDef in oldQmxf.templateDefinitions)
                {
                  ///process template label(s)
                  foreach (QMXFName newName in newTDef.name)
                  {
                    templateName = newName.value;
                    QMXFName oldName = oldTDef.name.Find(n => n.lang == newName.lang);
                    if (String.Compare(oldName.value, newName.value, true) != 0)
                    {
                      GenerateName(ref delete, oldName, templateId, oldTDef);
                      GenerateName(ref insert, newName, templateId, newTDef);
                    }
                  }
                  //append changing descriptions to each block
                  foreach (Description newDescr in newTDef.description)
                  {
                    Description oldDescr = oldTDef.description.Find(d => d.lang == newDescr.lang);
                    if (oldDescr != null && newDescr != null)
                    {
                      if (String.Compare(oldDescr.value, newDescr.value, true) != 0)
                      {
                        GenerateDescription(ref delete, oldDescr, templateId);
                        GenerateDescription(ref insert, newDescr, templateId);
                      }
                    }
                    else if (newDescr != null && oldDescr == null)
                    {
                      GenerateDescription(ref insert, newDescr, templateId);
                    }
                  }

                  index = 1;
                  ///  BaseTemplate roles do have the following properties
                  /// 1) baseclass of owl:Class
                  /// 2) rdfs:subClassOf = p8:TemplateRoleDescription
                  /// 3) rdfs:label = rolename
                  /// 4) p8:valRoleIndex
                  /// 5) p8:hasRoleFillerType = qualifified class or dm:entityType
                  /// 6) p8:hasTemplate = template ID
                  /// 7) p8:hasRole = role ID --- again probably should not use this --- pointer to self
                  if (oldTDef.roleDefinition.Count < newTDef.roleDefinition.Count) ///Role(s) added
                  {
                    foreach (RoleDefinition nrd in newTDef.roleDefinition)
                    {
                      string roleName = nrd.name[0].value;
                      string newRoleID = nrd.identifier;
                      if (string.IsNullOrEmpty(newRoleID))
                      {
                        if (_useExampleRegistryBase)
                          generatedId = CreateNewGuidId(_settings["ExampleRegistryBase"]);//, roleName);
                        else
                          generatedId = CreateNewGuidId(_settings["TemplateRegistryBase"]);//, roleName);
                        newRoleID = generatedId;
                      }
                      RoleDefinition ord = oldTDef.roleDefinition.Find(r => r.identifier == newRoleID);
                      if (ord == null) /// need to add it
                      {
                        foreach (QMXFName name in nrd.name)
                        {
                          GenerateName(ref insert, name, Utility.GetIdFromURI(newRoleID), nrd);
                        }
                        if (nrd.description != null)
                        {
                          GenerateDescription(ref insert, nrd.description, Utility.GetIdFromURI(newRoleID));
                        }
                        if (repository.RepositoryType == RepositoryType.Part8)
                        {
                          GenerateTypesPart8(ref insert, Utility.GetIdFromURI(newRoleID), templateId, nrd);
                          GenerateRoleIndexPart8(ref insert, Utility.GetIdFromURI(newRoleID), index, nrd);
                        }
                        else
                        {
                          GenerateTypes(ref insert, Utility.GetIdFromURI(newRoleID), templateId, nrd);
                          GenerateRoleIndex(ref insert, Utility.GetIdFromURI(newRoleID), index);
                        }
                      }
                      if (nrd.range != null)
                      {
                        //qn = nsMap.ReduceToQName(nrd.range, out qName);
                        if (repository.RepositoryType == RepositoryType.Part8)
                        {
                          GenerateRoleFillerType(ref insert, newRoleID, nrd.range);
                        }
                        else
                        {
                          GenerateRoleDomain(ref insert, Utility.GetIdFromURI(newRoleID), templateId);
                        }
                      }
                    }
                  }
                  else if (oldTDef.roleDefinition.Count > newTDef.roleDefinition.Count) ///Role(s) removed
                  {
                    foreach (RoleDefinition ord in oldTDef.roleDefinition)
                    {
                      RoleDefinition nrd = newTDef.roleDefinition.Find(r => r.identifier == ord.identifier);
                      if (nrd == null) /// need to add it
                      {
                        foreach (QMXFName name in ord.name)
                        {
                          GenerateName(ref delete, name, Utility.GetIdFromURI(ord.identifier), ord);
                        }
                        if (ord.description != null)
                        {
                          GenerateDescription(ref delete, ord.description, Utility.GetIdFromURI(ord.identifier));
                        }
                        if (repository.RepositoryType == RepositoryType.Part8)
                        {
                          GenerateTypesPart8(ref delete, Utility.GetIdFromURI(ord.identifier), templateId, ord);
                          GenerateRoleIndexPart8(ref delete, Utility.GetIdFromURI(ord.identifier), index, ord);
                        }
                        else
                        {
                          GenerateTypes(ref delete, Utility.GetIdFromURI(ord.identifier), templateId, ord);
                          GenerateRoleIndex(ref delete, Utility.GetIdFromURI(ord.identifier), index);
                        }
                      }
                      if (ord.range != null)
                      {
                        //qn = nsMap.ReduceToQName(ord.range, out qName);
                        if (repository.RepositoryType == RepositoryType.Part8)
                        {
                          GenerateRoleFillerType(ref delete, ord.identifier, ord.range);
                        }
                        else
                        {
                          GenerateRoleDomain(ref delete, Utility.GetIdFromURI(ord.identifier), templateId);
                        }
                      }
                    }
                  }
                }
                if (delete.IsEmpty && insert.IsEmpty)
                {
                  string errMsg = "No changes made to template [" + templateName + "]";
                  Status status = new Status();
                  response.Level = StatusLevel.Warning;
                  status.Messages.Add(errMsg);
                  response.Append(status);
                  continue;//Nothing to be done
                }
              }

              #endregion Form Delete/Insert
              #region Form Insert SPARQL
              if (insert.IsEmpty && delete.IsEmpty)
              {
                if (repository.RepositoryType == RepositoryType.Part8)
                {
                  GenerateTypesPart8(ref insert, templateId, null, newTDef);
                  GenerateRoleCountPart8(ref insert, newTDef.roleDefinition.Count, templateId, newTDef);
                }
                else
                {
                  GenerateTypes(ref insert, templateId, null, newTDef);
                  GenerateRoleCount(ref insert, newTDef.roleDefinition.Count, templateId, newTDef);
                }
                foreach (QMXFName name in newTDef.name)
                {
                  GenerateName(ref insert, name, templateId, newTDef);
                }

                foreach (Description descr in newTDef.description)
                {
                  GenerateDescription(ref insert, descr, templateId);
                }
                //form labels
                foreach (RoleDefinition newRole in newTDef.roleDefinition)
                {

                  string roleLabel = newRole.name.FirstOrDefault().value;
                  string newRoleID = string.Empty;
                  generatedId = string.Empty;
                  string genName = string.Empty;
                  string range = newRole.range;

                  genName = "Role definition " + roleLabel;
                  if (string.IsNullOrEmpty(newRole.identifier))
                  {
                    if (_useExampleRegistryBase)
                      generatedId = CreateNewGuidId(_settings["ExampleRegistryBase"]);//, genName);
                    else
                      generatedId = CreateNewGuidId(_settings["TemplateRegistryBase"]);//, genName);
                    newRoleID = Utility.GetIdFromURI(generatedId);
                  }
                  else
                  {
                    newRoleID = Utility.GetIdFromURI(newRole.identifier);
                  }
                  foreach (QMXFName newName in newRole.name)
                  {
                    GenerateName(ref insert, newName, newRoleID, newRole);
                  }

                  if (newRole.description != null && newRole.description.value != null)
                  {
                    GenerateDescription(ref insert, newRole.description, newRoleID);
                  }

                  if (repository.RepositoryType == RepositoryType.Part8)
                  {
                    GenerateRoleIndexPart8(ref insert, newRoleID, ++roleCount, newRole);
                    GenerateHasTemplate(ref insert, newRoleID, templateId, newRole);
                    GenerateHasRole(ref insert, templateId, newRoleID, newRole);
                  }
                  else
                  {
                    GenerateRoleIndex(ref insert, newRoleID, ++roleCount);
                  }
                  if (!string.IsNullOrEmpty(newRole.range))
                  {
                    //qn = nsMap.ReduceToQName(newRole.range, out qName);
                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                      GenerateRoleFillerType(ref insert, newRoleID, newRole.range);
                    }
                    else
                    {
                      GenerateRoleDomain(ref insert, newRoleID, templateId);
                      GenerateTypes(ref insert, newRoleID, null, newRole);
                    }
                  }
                }
              }
              #endregion
              #region Generate Query and post Template Definition
              if (!delete.IsEmpty)
              {
                sparqlBuilder.AppendLine(deleteData);
                foreach (Triple t in delete.Triples)
                {
                  sparqlBuilder.AppendLine(t.ToString(formatter));
                }
                if (insert.IsEmpty)
                  sparqlBuilder.AppendLine("}");
                else
                  sparqlBuilder.AppendLine("};");
              }
              if (!insert.IsEmpty)
              {
                sparqlBuilder.AppendLine(insertData);
                foreach (Triple t in insert.Triples)
                {
                  sparqlBuilder.AppendLine(t.ToString(formatter));
                }
                sparqlBuilder.AppendLine("}");
              }
              string sparql = sparqlBuilder.ToString();
              Response postResponse = PostToRepository(repository, sparql);
              response.Append(postResponse);
            }
          }
              #endregion Generate Query and post Template Definition
          #endregion Template Definitions
          #region Template Qualification
          /// Qualification templates do have the following properties
          /// 1) Base class = owl:Thing
          /// 2) rdf:type = p8:SpecializedTemplateStatement
          /// 3) rdfs:label = template name
          /// 
          if (qmxf.templateQualifications.Count > 0)
          {
            foreach (TemplateQualification newTQ in qmxf.templateQualifications)
            {
              int roleCount = 0;
              string templateName = string.Empty;
              string templateID = string.Empty;
              string generatedId = string.Empty;
              string roleQualification = string.Empty;
              //int index = 1;
              if (!string.IsNullOrEmpty(newTQ.identifier))
                templateID = newTQ.identifier;

              templateName = newTQ.name[0].value;
              QMXF oldQmxf = new QMXF();
              if (!String.IsNullOrEmpty(templateID))
              {
                oldQmxf = GetTemplate(templateID, QMXFType.Qualification, repository);
              }
              else
              {
                if (_useExampleRegistryBase)
                  generatedId = CreateNewGuidId(_settings["ExampleRegistryBase"]);//, templateName);
                else
                  generatedId = CreateNewGuidId(_settings["TemplateRegistryBase"]);//, templateName);

                templateID = Utility.GetIdFromURI(generatedId);
              }
              #region Form Delete/Insert SPARQL
              if (oldQmxf.templateQualifications.Count > 0)
              {
                foreach (TemplateQualification oldTQ in oldQmxf.templateQualifications)
                {
                  //qn = nsMap.ReduceToQName(oldTQ.qualifies, out qName);
                  foreach (QMXFName nn in newTQ.name)
                  {
                    templateName = nn.value;
                    QMXFName on = oldTQ.name.Find(n => n.lang == nn.lang);
                    if (on != null)
                    {
                      if (String.Compare(on.value, nn.value, true) != 0)
                      {
                        GenerateName(ref delete, on, templateID, oldTQ);
                        GenerateName(ref insert, nn, templateID, newTQ);
                      }
                    }
                  }
                  foreach (Description nd in newTQ.description)
                  {
                    if (nd.lang == null) nd.lang = defaultLanguage;
                    Description od = null;
                    od = oldTQ.description.Find(d => d.lang == nd.lang);

                    if (od != null && od.value != null)
                    {
                      if (string.Compare(od.value, nd.value, true) != 0)
                      {
                        GenerateDescription(ref delete, od, templateID);
                        GenerateDescription(ref insert, nd, templateID);
                      }
                    }
                    else if (od == null && nd.value != null)
                    {
                      GenerateDescription(ref insert, nd, templateID);
                    }
                  }
                  //role count
                  if (oldTQ.roleQualification.Count != newTQ.roleQualification.Count)
                  {
                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                      GenerateRoleCountPart8(ref delete, oldTQ.roleQualification.Count, templateID, oldTQ);
                      GenerateRoleCountPart8(ref insert, newTQ.roleQualification.Count, templateID, newTQ);
                    }
                    else
                    {
                      GenerateRoleCount(ref delete, oldTQ.roleQualification.Count, templateID, oldTQ);
                      GenerateRoleCount(ref insert, newTQ.roleQualification.Count, templateID, newTQ);
                    }
                  }

                  foreach (Specialization ns in newTQ.specialization)
                  {
                    Specialization os = oldTQ.specialization.FirstOrDefault();

                    if (os != null && os.reference != ns.reference)
                    {
                      if (repository.RepositoryType == RepositoryType.Part8)
                      {

                      }
                      else
                      {

                      }
                    }
                  }

                  //index = 1;
                  ///  Qualification roles do have the following properties
                  /// 1) baseclass of owl:Thing
                  /// 2) rdf:type = p8:TemplateRoleDescription
                  /// 3) rdfs:label = rolename
                  /// 4) p8:valRoleIndex
                  /// 5) p8:hasRoleFillerType = qualifified class
                  /// 6) p8:hasTemplate = template ID
                  /// 6) p8:hasRole = tpl:{roleName} probably don't need to use this -- pointer to self
                  if (oldTQ.roleQualification.Count < newTQ.roleQualification.Count)
                  {
                    int count = 0;
                    foreach (RoleQualification nrq in newTQ.roleQualification)
                    {
                      string roleName = nrq.name[0].value;
                      string newRoleID = nrq.identifier;

                      if (string.IsNullOrEmpty(newRoleID))
                      {
                        if (_useExampleRegistryBase)
                          generatedId = CreateNewGuidId(_settings["ExampleRegistryBase"]);//, roleName);
                        else
                          generatedId = CreateNewGuidId(_settings["TemplateRegistryBase"]);//, roleName);
                        newRoleID = generatedId;
                      }
                      RoleQualification orq = oldTQ.roleQualification.Find(r => r.identifier == newRoleID);
                      if (orq == null)
                      {
                        if (repository.RepositoryType == RepositoryType.Part8)
                        {
                          GenerateTypesPart8(ref insert, Utility.GetIdFromURI(newRoleID), templateID, nrq);
                          foreach (QMXFName nn in nrq.name)
                          {
                            GenerateName(ref insert, nn, Utility.GetIdFromURI(newRoleID), nrq);
                          }
                          GenerateRoleIndexPart8(ref insert, Utility.GetIdFromURI(newRoleID), ++count, nrq);
                          GenerateHasTemplate(ref insert, Utility.GetIdFromURI(newRoleID), templateID, nrq);
                          GenerateHasRole(ref insert, templateID, Utility.GetIdFromURI(newRoleID), newTQ);
                          if (!string.IsNullOrEmpty(nrq.range))
                          {
                            //qn = nsMap.ReduceToQName(nrq.range, out qName);
                            GenerateRoleFillerType(ref insert, newRoleID, nrq.range);
                          }
                          else if (nrq.value != null)
                          {
                            if (nrq.value.reference != null)
                            {
                              //qn = nsMap.ReduceToQName(nrq.value.reference, out qName);
                              GenerateRoleFillerType(ref insert, newRoleID, nrq.value.reference);
                            }
                            else if (nrq.value.text != null)
                            {
                              ///TODO
                            }
                          }
                        }
                        else //Not Part8 repository
                        {
                          if (!string.IsNullOrEmpty(nrq.range)) //range restriction
                          {
                            //qn = nsMap.ReduceToQName(nrq.range, out qName);
                            GenerateRange(ref insert, newRoleID, nrq.range, nrq);
                            GenerateTypes(ref insert, newRoleID, templateID, nrq);
                            GenerateQualifies(ref insert, newRoleID, nrq.qualifies, nrq);
                          }
                          else if (nrq.value != null)
                          {
                            if (nrq.value.reference != null) //reference restriction
                            {
                              GenerateReferenceType(ref insert, newRoleID, templateID, nrq);
                              GenerateReferenceQual(ref insert, newRoleID, nrq.qualifies, nrq);
                              //qn = nsMap.ReduceToQName(nrq.value.reference, out qName);
                              GenerateReferenceTpl(ref insert, newRoleID, nrq.value.reference, nrq);
                            }
                            else if (nrq.value.text != null)// value restriction
                            {
                              GenerateValue(ref insert, Utility.GetIdFromURI(newRoleID), templateID, nrq);
                            }
                          }
                          GenerateTypes(ref insert, Utility.GetIdFromURI(newRoleID), templateID, nrq);
                          GenerateRoleDomain(ref insert, Utility.GetIdFromURI(newRoleID), templateID);
                          GenerateRoleIndex(ref insert, Utility.GetIdFromURI(newRoleID), ++count);
                        }
                      }
                    }
                  }
                  else if (oldTQ.roleQualification.Count > newTQ.roleQualification.Count)
                  {
                    int count = 0;
                    foreach (RoleQualification orq in oldTQ.roleQualification)
                    {
                      string roleName = orq.name[0].value;
                      string newRoleID = orq.identifier;

                      if (string.IsNullOrEmpty(newRoleID))
                      {
                        if (_useExampleRegistryBase)
                          generatedId = CreateNewGuidId(_settings["ExampleRegistryBase"]);//, roleName);
                        else
                          generatedId = CreateNewGuidId(_settings["TemplateRegistryBase"]);//, roleName);
                        newRoleID = generatedId;
                      }
                      RoleQualification nrq = newTQ.roleQualification.Find(r => r.identifier == newRoleID);
                      if (nrq == null)
                      {
                        if (repository.RepositoryType == RepositoryType.Part8)
                        {
                          GenerateTypesPart8(ref delete, Utility.GetIdFromURI(newRoleID), templateID, orq);
                          foreach (QMXFName nn in orq.name)
                          {
                            GenerateName(ref delete, nn, Utility.GetIdFromURI(newRoleID), orq);
                          }
                          GenerateRoleIndexPart8(ref delete, Utility.GetIdFromURI(newRoleID), ++count, orq);
                          GenerateHasTemplate(ref delete, Utility.GetIdFromURI(newRoleID), templateID, orq);
                          GenerateHasRole(ref delete, templateID, Utility.GetIdFromURI(newRoleID), oldTQ);
                          if (!string.IsNullOrEmpty(orq.range))
                          {
                            //qn = nsMap.ReduceToQName(orq.range, out qName);
                            GenerateRoleFillerType(ref delete, newRoleID, orq.range);
                          }
                          else if (orq.value != null)
                          {
                            if (orq.value.reference != null)
                            {
                              //qn = nsMap.ReduceToQName(orq.value.reference, out qName);
                              GenerateRoleFillerType(ref delete, newRoleID, orq.value.reference);
                            }
                            else if (nrq.value.text != null)
                            {
                              ///TODO
                            }
                          }
                        }
                        else //Not Part8 repository
                        {
                          if (!string.IsNullOrEmpty(orq.range)) //range restriction
                          {
                            //qn = nsMap.ReduceToQName(orq.range, out qName);
                            GenerateRange(ref delete, newRoleID, orq.range, orq);
                            GenerateTypes(ref delete, newRoleID, templateID, nrq);
                            GenerateQualifies(ref delete, newRoleID, orq.qualifies, orq);
                          }
                          else if (orq.value != null)
                          {
                            if (orq.value.reference != null) //reference restriction
                            {
                              GenerateReferenceType(ref delete, Utility.GetIdFromURI(newRoleID), templateID, orq);
                              GenerateReferenceQual(ref delete, Utility.GetIdFromURI(newRoleID), orq.qualifies.Split('#')[1], orq);
                              //qn = nsMap.ReduceToQName(orq.value.reference, out qName);
                              GenerateReferenceTpl(ref insert, newRoleID, orq.value.reference, orq);
                            }
                            else if (orq.value.text != null)// value restriction
                            {
                              GenerateValue(ref delete, newRoleID, templateID, orq);
                            }
                          }
                          GenerateTypes(ref delete, newRoleID, templateID, orq);
                          GenerateRoleDomain(ref delete, newRoleID, templateID);
                          GenerateRoleIndex(ref delete, Utility.GetIdFromURI(newRoleID), ++count);
                        }
                      }
                    }
                  }
                }
                if (delete.IsEmpty && insert.IsEmpty)
                {
                  string errMsg = "No changes made to template [" + templateName + "]";
                  Status status = new Status();
                  response.Level = StatusLevel.Warning;
                  status.Messages.Add(errMsg);
                  response.Append(status);
                  continue;//Nothing to be done
                }
              }
              #endregion
              #region Form Insert SPARQL
              if (delete.IsEmpty)
              {
                string templateLabel = String.Empty;
                string labelSparql = String.Empty;

                foreach (QMXFName newName in newTQ.name)
                {
                  GenerateName(ref insert, newName, templateID, newTQ);
                }
                foreach (Description newDescr in newTQ.description)
                {
                  if (string.IsNullOrEmpty(newDescr.value)) continue;
                  GenerateDescription(ref insert, newDescr, templateID);
                }

                if (repository.RepositoryType == RepositoryType.Part8)
                {
                  GenerateRoleCountPart8(ref insert, newTQ.roleQualification.Count, templateID, newTQ);
                  //qn = nsMap.ReduceToQName(newTQ.qualifies, out qName);
                  GenerateTypesPart8(ref insert, templateID, newTQ.qualifies, newTQ);
                }
                else
                {
                  GenerateRoleCount(ref insert, newTQ.roleQualification.Count, templateID, newTQ);
                  //qn = nsMap.ReduceToQName(newTQ.qualifies, out qName);
                  GenerateTypes(ref insert, templateID, newTQ.qualifies, newTQ);

                }
                foreach (Specialization spec in newTQ.specialization)
                {
                  string specialization = spec.reference;
                  if (repository.RepositoryType == RepositoryType.Part8)
                  {
                    ///TODO
                  }
                  else
                  {
                    ///TODO
                  }
                }

                foreach (RoleQualification newRole in newTQ.roleQualification)
                {
                  string roleLabel = newRole.name.FirstOrDefault().value;
                  string roleID = string.Empty;
                  generatedId = string.Empty;
                  string genName = string.Empty;
                  string range = newRole.range;

                  genName = "Role Qualification " + roleLabel;
                  if (string.IsNullOrEmpty(newRole.identifier))
                  {
                    if (_useExampleRegistryBase)
                      generatedId = CreateNewGuidId(_settings["ExampleRegistryBase"]);//, genName);
                    else
                      generatedId = CreateNewGuidId(_settings["TemplateRegistryBase"]);//, genName);

                    roleID = generatedId;
                  }
                  else
                  {
                    roleID = newRole.identifier;
                  }
                  if (repository.RepositoryType == RepositoryType.Part8)
                  {
                    GenerateTypesPart8(ref insert, roleID, templateID, newRole);
                    foreach (QMXFName newName in newRole.name)
                    {
                      GenerateName(ref insert, newName, roleID, newRole);
                    }
                    GenerateRoleIndexPart8(ref insert, roleID, ++roleCount, newRole);
                    GenerateHasTemplate(ref insert, roleID, templateID, newRole);
                    GenerateHasRole(ref insert, templateID, roleID, newTQ);
                    if (!string.IsNullOrEmpty(newRole.range))
                    {
                      //qn = nsMap.ReduceToQName(newRole.range, out qName);
                      GenerateRoleFillerType(ref insert, roleID, newRole.range);
                    }
                    else if (newRole.value != null)
                    {
                      if (newRole.value.reference != null)
                      {
                        //qn = nsMap.ReduceToQName(newRole.value.reference, out qName);
                        GenerateRoleFillerType(ref insert, roleID, newRole.value.reference);
                      }
                      else if (newRole.value.text != null)
                      {
                        ///TODO
                      }
                    }
                  }
                  else //Not Part8 repository
                  {
                    if (!string.IsNullOrEmpty(newRole.range)) //range restriction
                    {

                      //qn = nsMap.ReduceToQName(newRole.range, out qName);
                      GenerateRange(ref insert, roleID, newRole.range, newRole);
                      GenerateTypes(ref insert, roleID, templateID, newRole);
                      GenerateQualifies(ref insert, roleID, newRole.qualifies, newRole);
                    }
                    else if (newRole.value != null)
                    {
                      if (newRole.value.reference != null) //reference restriction
                      {
                        GenerateReferenceType(ref insert, roleID, templateID, newRole);
                        GenerateReferenceQual(ref insert, roleID, newRole.qualifies, newRole);
                        //qn = nsMap.ReduceToQName(newRole.value.reference, out qName);
                        GenerateReferenceTpl(ref insert, roleID, newRole.value.reference, newRole);
                      }
                      else if (newRole.value.text != null)// value restriction
                      {
                        GenerateValue(ref insert, roleID, templateID, newRole);
                      }
                    }
                    GenerateTypes(ref insert, roleID, templateID, newRole);
                    GenerateRoleDomain(ref insert, roleID, templateID);
                    GenerateRoleIndex(ref insert, roleID, ++roleCount);
                  }
                }
              }
              #endregion
              #region Generate Query and Post Qualification Template
              if (!delete.IsEmpty)
              {
                sparqlBuilder.Append(deleteData);
                foreach (Triple t in delete.Triples)
                {
                  sparqlBuilder.AppendLine(t.ToString(formatter));
                }
                if (insert.IsEmpty)
                  sparqlBuilder.AppendLine("}");
                else
                  sparqlBuilder.AppendLine("};");
              }
              if (!insert.IsEmpty)
              {
                sparqlBuilder.AppendLine(insertData);
                foreach (Triple t in insert.Triples)
                {
                  sparqlBuilder.AppendLine(t.ToString(formatter));
                }
                sparqlBuilder.AppendLine("}");
              }

              string sparql = sparqlBuilder.ToString();
              Response postResponse = PostToRepository(repository, sparql);
              response.Append(postResponse);
            }
          }

        }
      }

      catch (Exception ex)
      {
        string errMsg = "Error in PostTemplate: " + ex;
        Status status = new Status();

        response.Level = StatusLevel.Error;
        status.Messages.Add(errMsg);
        response.Append(status);

        _logger.Error(errMsg);
      }

      return response;
    }
    public Response PostClass(QMXF qmxf)
    {
      Graph delete = new Graph();
      Graph insert = new Graph();
      //add namespaces to graphs 
      foreach (var pref in nsMap.Prefixes)
      {
        delete.NamespaceMap.AddNamespace(pref, nsMap.GetNamespaceUri(pref));
      }
      //delete.NamespaceMap.AddNamespace("rdl", new Uri("http://rdl.rdlfacade.org/data#"));
      //delete.NamespaceMap.AddNamespace("tpl", new Uri("http://tpl.rdlfacade.org/data#"));
      //delete.NamespaceMap.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
      //delete.NamespaceMap.AddNamespace("dm", new Uri("http://dm.rdlfacade.org/data#"));
      //delete.NamespaceMap.AddNamespace("p8", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/template-model#"));
      insert.NamespaceMap.Import(delete.NamespaceMap);

      Response response = new Response();
      response.Level = StatusLevel.Success;
      try
      {
        Repository repository = GetRepository(qmxf.targetRepository);

        if (repository == null || repository.IsReadOnly)
        {
          Status status = new Status();
          status.Level = StatusLevel.Error;

          if (repository == null)
            status.Messages.Add("Repository not found!");
          else
            status.Messages.Add("Repository [" + qmxf.targetRepository + "] is read-only!");

          _response.Append(status);
        }
        else
        {
          string registry = _useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["ClassRegistryBase"];
          foreach (ClassDefinition newClsDef in qmxf.classDefinitions)
          {
            string language = string.Empty;
            string clsId = newClsDef.identifier;
            QMXF oldQmxf = new QMXF();

            if (!String.IsNullOrEmpty(clsId))
            {
              oldQmxf = GetClass(clsId, repository);
            }
            // delete class
            if (oldQmxf.classDefinitions.Count > 0)
            {
              foreach (ClassDefinition oldClsDef in oldQmxf.classDefinitions)
              {
                foreach (QMXFName nn in newClsDef.name)
                {
                  QMXFName on = oldClsDef.name.Find(n => n.lang == nn.lang);
                  if (on != null)
                  {
                    if (String.Compare(on.value, nn.value, true) != 0)
                    {
                      GenerateClassName(ref delete, on, clsId, oldClsDef);
                      GenerateClassName(ref insert, nn, clsId, newClsDef);
                    }
                  }
                  foreach (Description nd in newClsDef.description)
                  {
                    Description od = oldClsDef.description.Find(d => d.lang == nd.lang);
                    if (od != null)
                    {
                      if (String.Compare(od.value, nd.value, true) != 0)
                      {
                        GenerateClassDescription(ref delete, od, clsId);
                        GenerateClassDescription(ref insert, nd, clsId);
                      }
                    }
                  }
                  //specialization
                  if (newClsDef.specialization.Count == oldClsDef.specialization.Count)
                  {
                    continue; /// no change ... so continue
                  }
                  else if (newClsDef.specialization.Count < oldClsDef.specialization.Count) //some is deleted ...focus on old to find deleted
                  {
                    foreach (Specialization os in oldClsDef.specialization)
                    {
                      Specialization ns = newClsDef.specialization.Find(s => s.reference == os.reference);
                      if (ns == null)
                      {
                        //qn = nsMap.ReduceToQName(os.reference, out qName);
                        GenerateRdfSubClass(ref delete, clsId, os.reference);
                      }
                    }
                  }
                  else if (newClsDef.specialization.Count > oldClsDef.specialization.Count)//some is added ... find added 
                  {
                    foreach (Specialization ns in newClsDef.specialization)
                    {
                      Specialization os = oldClsDef.specialization.Find(s => s.reference == ns.reference);
                      if (os == null)
                      {
                        //qn = nsMap.ReduceToQName(ns.reference, out qName);
                        GenerateRdfSubClass(ref insert, clsId, ns.reference);
                      }
                    }
                  }
                  // classification
                  if (newClsDef.classification.Count == oldClsDef.classification.Count)
                  {
                    continue; //no change...so continue
                  }
                  else if (newClsDef.classification.Count < oldClsDef.classification.Count) //some is deleted ...focus on old to find deleted
                  {
                    foreach (Classification oc in oldClsDef.classification)
                    {
                      Classification nc = newClsDef.classification.Find(c => c.reference == oc.reference);
                      if (nc == null)
                      {
                        //qn = nsMap.ReduceToQName(oc.reference, out qName);
                        if (repository.RepositoryType == RepositoryType.Part8)
                        {
                          GenerateSuperClass(ref delete, oc.reference, clsId); ///delete from old
                        }
                        else
                        {
                          GenerateDmClassification(ref delete, clsId, oc.reference);
                        }
                      }
                    }
                  }
                  else if (newClsDef.classification.Count > oldClsDef.classification.Count)//some is added ... find added classifications
                  {
                    foreach (Classification nc in newClsDef.classification)
                    {
                      Classification oc = oldClsDef.classification.Find(c => c.reference == nc.reference);
                      if (oc == null)
                      {
                        if (repository.RepositoryType == RepositoryType.Part8)
                        {
                          GenerateSuperClass(ref insert, nc.reference, clsId); ///insert from new
                        }
                        else
                        {
                          GenerateDmClassification(ref insert, clsId, nc.reference);
                        }
                      }
                    }
                  }
                }
              }
              if (delete.IsEmpty && insert.IsEmpty)
              {
                string errMsg = "No changes made to class [" + qmxf.classDefinitions[0].name[0].value + "]";
                Status status = new Status();
                response.Level = StatusLevel.Warning;
                status.Messages.Add(errMsg);
                response.Append(status);
                continue;
              }
            }
            /// add class
            if (delete.IsEmpty && insert.IsEmpty)
            {
              string clsLabel = newClsDef.name[0].value;
              if (string.IsNullOrEmpty(clsId))
              {
                string newClsName = "Class definition " + clsLabel;
                clsId = CreateNewGuidId(registry);

              }
              /// append entity type
              if (newClsDef.entityType != null && !String.IsNullOrEmpty(newClsDef.entityType.reference))
              {
              ///qn = nsMap.ReduceToQName(newClsDef.entityType.reference, out qName);
                GenerateTypesPart8(ref insert, clsId, newClsDef.entityType.reference, newClsDef);
              }
              /// append specialization
              foreach (Specialization ns in newClsDef.specialization)
              {
                if (!String.IsNullOrEmpty(ns.reference))
                {
                  //qn = nsMap.ReduceToQName(ns.reference, out qName);
                  if (repository.RepositoryType == RepositoryType.Part8)
                  {
                    GenerateRdfSubClass(ref insert, clsId, ns.reference);
                  }
                  else
                  {
                    GenerateDmSubClass(ref insert, clsId, ns.reference);
                  }
                }
              }
              // append description
              foreach (Description nd in newClsDef.description)
              {
                if (!String.IsNullOrEmpty(nd.value))
                {
                  GenerateClassDescription(ref insert, nd, clsId);
                }
              }
              foreach (QMXFName nn in newClsDef.name)
              {
                // append label
                GenerateClassName(ref insert, nn, clsId, newClsDef);
              }
              // append classification
              foreach (Classification nc in newClsDef.classification)
              {
                if (!string.IsNullOrEmpty(nc.reference))
                {
                  //qn = nsMap.ReduceToQName(nc.reference, out qName);
                  if (repository.RepositoryType == RepositoryType.Part8)
                  {
                    GenerateSuperClass(ref insert, nc.reference, clsId);
                  }
                  else
                  {
                    GenerateDmClassification(ref insert, clsId, nc.reference);
                  }
                }
              }
            }
            if (!delete.IsEmpty)
            {
              sparqlBuilder.AppendLine(deleteData);
              foreach (Triple t in delete.Triples)
              {
                sparqlBuilder.AppendLine(t.ToString(formatter));
              }
              if (insert.IsEmpty)
                sparqlBuilder.AppendLine("}");
              else
                sparqlBuilder.AppendLine("};");
            }
            if (!insert.IsEmpty)
            {
              sparqlBuilder.AppendLine(insertData);
              foreach (Triple t in insert.Triples)
              {
                sparqlBuilder.AppendLine(t.ToString(formatter));
              }
              sparqlBuilder.AppendLine("}");
            }

            string sparql = sparqlBuilder.ToString();
            Response postResponse = PostToRepository(repository, sparql);
            response.Append(postResponse);
          }
        }
      }
      catch (Exception ex)
      {
        string errMsg = "Error in PostClass: " + ex;
        Status status = new Status();

        response.Level = StatusLevel.Error;
        status.Messages.Add(errMsg);
        response.Append(status);

        _logger.Error(errMsg);
      }

      return response;
    }

              #endregion

    public List<Entity> Find(string queryString)
    {
      List<Entity> queryResult = new List<Entity>();
      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;

        Query queryExactSearch = (Query)_queries.FirstOrDefault(c => c.Key == "ExactSearch").Query;

        sparql = ReadSPARQL(queryExactSearch.FileName);

        sparql = sparql.Replace("param1", queryString);

        foreach (Repository repository in _repositories)
        {
          SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

          foreach (SparqlResult result in sparqlResults)
          {
            Entity resultEntity = new Entity();
            foreach (var v in result.Variables)
            {
              if (v.Equals("uri"))
              {
                resultEntity.Uri = result[v].ToString();
              }
              else if (v.Equals("label") && result.HasValue("label"))
              {
                resultEntity.Label = ((ILiteralNode)result[v]).Value;
                resultEntity.Lang = ((ILiteralNode)result[v]).Language;
              }
            }

            resultEntity.Repository = repository.Name;


            queryResult.Add(resultEntity);
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error in Find: " + e);
        throw new Exception("Error while Finding " + queryString + ".\n" + e.ToString(), e);
      }
      return queryResult;
    }

    public VersionInfo GetVersion()
    {
      Version version = this.GetType().Assembly.GetName().Version;

      return new VersionInfo()
      {
        Major = version.Major,
        Minor = version.Minor,
        Build = version.Build,
        Revision = version.Revision
      };
    }

    private void GenerateValue(ref Graph work, string subjId, string objId, object gobj)
    {
      RoleQualification role = (RoleQualification)gobj;
      pred = work.CreateUriNode("tpl:R56456315674");
      obj = work.CreateUriNode(string.Format("<{0}>", objId));
      work.Assert(new Triple(subj, pred, obj));
      pred = work.CreateUriNode("tpl:R89867215482");
      obj = work.CreateUriNode(string.Format("<{0}>", objId));
      work.Assert(new Triple(subj, pred, obj));
      pred = work.CreateUriNode("tpl:R29577887690");
      obj = work.CreateLiteralNode(role.value.text, string.IsNullOrEmpty(role.value.lang) ? defaultLanguage : role.value.lang);
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateReferenceQual(ref Graph work, string subjId, string objId, object gobj)
    {
      subj = work.CreateUriNode(string.Format("<{0}>", subjId));
      pred = work.CreateUriNode("tpl:R30741601855");
      obj = work.CreateUriNode(string.Format("<{0}>", objId));
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateReferenceType(ref Graph work, string subjId, string objId, object gobj)
    {
      subj = work.CreateUriNode(string.Format("<{0}>", subjId));
      pred = work.CreateUriNode(rdfType);
      obj = work.CreateUriNode("tpl:R40103148466");
      work.Assert(new Triple(subj, pred, obj));
      pred = work.CreateUriNode("tpl:R49267603385");
      obj = work.CreateUriNode(string.Format("<{0}>", objId));
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateReferenceTpl(ref Graph work, string subjId, string objId, object gobj)
    {
      subj = work.CreateUriNode(string.Format("<{0}>", subjId));
      pred = work.CreateUriNode("tpl:R21129944603");
      obj = work.CreateUriNode(string.Format("<{0}>", objId));
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateQualifies(ref Graph work, string subjId, string objId, object gobj)
    {
      subj = work.CreateUriNode(string.Format("<{0}>", subjId));
      pred = work.CreateUriNode("tpl:R91125890543");
      obj = work.CreateUriNode(string.Format("<{0}>", objId));
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateRange(ref Graph work, string subjId, string objId, object gobj)
    {
      subj = work.CreateUriNode(string.Format("<{0}>", subjId));
      pred = work.CreateUriNode("rdfs:range");
      obj = work.CreateUriNode(objId);
      work.Assert(new Triple(subj, pred, obj));
      pred = work.CreateUriNode("tpl:R98983340497");
      obj = work.CreateUriNode(string.Format("<{0}>", objId));
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateHasRole(ref Graph work, string subjId, string objId, object gobj)
    {
      subj = work.CreateUriNode(string.Format("<{0}>", subjId));
      pred = work.CreateUriNode("p8:hasRole");
      obj = work.CreateUriNode(string.Format("<{0}>", objId));
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateHasTemplate(ref Graph work, string subjId, string objId, object gobj)
    {
      if (gobj is RoleDefinition || gobj is RoleQualification)
      {
        subj = work.CreateUriNode(string.Format("<{0}>", subjId));
        pred = work.CreateUriNode("p8:hasTemplate");
        obj = work.CreateUriNode(string.Format("<{0}>", objId));
        work.Assert(new Triple(subj, pred, obj));
      }
    }

    private void GenerateRoleIndex(ref Graph work, string subjId, int index)
    {
      subj = work.CreateUriNode(string.Format("tpl:{0}", subjId));
      pred = work.CreateUriNode("tpl:R97483568938");
      obj = work.CreateLiteralNode(index.ToString(), new Uri("xsd:integer"));
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateRoleIndexPart8(ref Graph work, string subjId, int index, object gobj)
    {
      if (gobj is RoleDefinition || gobj is RoleQualification)
      {
        subj = work.CreateUriNode(string.Format("<{0}>", subjId));
        pred = work.CreateUriNode("p8:valRoleIndex");
        obj = work.CreateLiteralNode(index.ToString(), new Uri("xsd:integer"));
        work.Assert(new Triple(subj, pred, obj));
      }
    }

    private void GenerateRoleDomain(ref Graph work, string subjId, string objId)
    {
      subj = work.CreateUriNode(string.Format("<{0}>", subjId));
      pred = work.CreateUriNode("rdfs:domain");
      obj = work.CreateUriNode(string.Format("<{0}>", objId));
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateRoleFillerType(ref Graph work, string subjId, string range)
    {
      subj = work.CreateUriNode(string.Format("<{0}>", subjId));
      pred = work.CreateUriNode("p8:hasRoleFillerType");
      obj = work.CreateUriNode(string.Format("<{0}>", range));
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateRoleCount(ref Graph work, int rolecount, string subjId, object gobj)
    {
      if (gobj is TemplateDefinition || gobj is TemplateQualification)
      {
        subj = work.CreateUriNode(string.Format("<{0}>", subjId));
        pred = work.CreateUriNode("tpl:R35529169909");
        obj = work.CreateLiteralNode(Convert.ToString(rolecount), new Uri("xsd:integer"));
        work.Assert(new Triple(subj, pred, obj));
      }
    }

    private void GenerateRoleCountPart8(ref Graph work, int rolecount, string subjId, object gobj)
    {
      if (gobj is TemplateDefinition || gobj is TemplateQualification)
      {
        subj = work.CreateUriNode(string.Format("tpl:{0}", subjId));
        pred = work.CreateUriNode("p8:valNumberOfRoles");
        obj = work.CreateLiteralNode(Convert.ToString(rolecount), new Uri("xsd:integer"));
        work.Assert(new Triple(subj, pred, obj));
      }

    }

    private void GenerateTypesPart8(ref Graph work, string subjId, string objectId, object gobj)
    {
      if (gobj is TemplateDefinition)
      {
        subj = work.CreateUriNode(string.Format("<{0}>", subjId));
        pred = work.CreateUriNode(rdfType);
        obj = work.CreateUriNode("owl:Thing");
        //obj = work.CreateUriNode("p8:TemplateDescription");
        work.Assert(new Triple(subj, pred, obj));
        pred = work.CreateUriNode(rdfssubClassOf);
        obj = work.CreateUriNode("p8:BaseTemplateStatement");
        work.Assert(new Triple(subj, pred, obj));
      }
      else if (gobj is RoleQualification)
      {
        subj = work.CreateUriNode(string.Format("<{0}>", subjId));
        pred = work.CreateUriNode(rdfType);
        obj = work.CreateUriNode("owl:Thing");
        work.Assert(new Triple(subj, pred, obj));
        obj = work.CreateUriNode("p8:TemplateRoleDescription");
        work.Assert(new Triple(subj, pred, obj));
        pred = work.CreateUriNode("p8:hasTemplate");
        obj = work.CreateUriNode(string.Format("tpl:{0}", objectId));
        work.Assert(new Triple(subj, pred, obj));
        pred = work.CreateUriNode("p8:hasRoleFillerType");
        obj = work.CreateUriNode(string.Format("<{0}>", ((RoleQualification)gobj).range));
        work.Assert(new Triple(subj, pred, obj));
      }
      else if (gobj is RoleDefinition)
      {
        subj = work.CreateUriNode(string.Format("<{0}>", subjId));
        pred = work.CreateUriNode(rdfType);
        obj = work.CreateUriNode("owl:Thing");
        work.Assert(new Triple(subj, pred, obj));
        obj = work.CreateUriNode("p8:TemplateRoleDescription");
        work.Assert(new Triple(subj, pred, obj));
        pred = work.CreateUriNode("p8:hasTemplate");
        obj = work.CreateUriNode(string.Format("<{0}>", objectId));
        work.Assert(new Triple(subj, pred, obj));
        pred = work.CreateUriNode("p8:hasRoleFillerType");
        obj = work.CreateUriNode(string.Format("<{0}>", ((RoleDefinition)gobj).range));
        work.Assert(new Triple(subj, pred, obj));
      }
      else if (gobj is TemplateQualification)
      {
        subj = work.CreateUriNode(string.Format("<{0}>", subjId));
        pred = work.CreateUriNode(rdfType);
        //obj = work.CreateUriNode("p8:TemplateDescription");
        //work.Assert(new Triple(subj, pred, obj));
        obj = work.CreateUriNode("owl:Thing");
        work.Assert(new Triple(subj, pred, obj));
        pred = work.CreateUriNode(rdfssubClassOf);
        obj = work.CreateUriNode("p8:SpecializedTemplateStatement");
        work.Assert(new Triple(subj, pred, obj));
        pred = work.CreateUriNode(rdfssubClassOf);
        obj = work.CreateUriNode(string.Format("<{0}>", objectId));
        work.Assert(new Triple(subj, pred, obj));
      }
      else if (gobj is ClassDefinition)
      {
        subj = work.CreateUriNode(string.Format("<{0}>", subjId));
        pred = work.CreateUriNode(rdfType);
        obj = work.CreateUriNode(string.Format("<{0}>", objectId));
        work.Assert(new Triple(subj, pred, obj));
        pred = work.CreateUriNode(rdfType);
        obj = work.CreateUriNode("owl:Class");
        work.Assert(new Triple(subj, pred, obj));
      }
    }

    private void GenerateTypes(ref Graph work, string subjId, string objId, object gobj)
    {
      if (gobj is TemplateDefinition)
      {
        subj = work.CreateUriNode(string.Format("<{0}>", subjId));
        pred = work.CreateUriNode(rdfType);
        obj = work.CreateUriNode("tpl:R16376066707");
        work.Assert(new Triple(subj, pred, obj));
      }
      else if (gobj is RoleDefinition)
      {
        subj = work.CreateUriNode(string.Format("<{0}>", subjId));
        pred = work.CreateUriNode(rdfType);
        obj = work.CreateUriNode("tpl:R74478971040");
        work.Assert(new Triple(subj, pred, obj));
      }
      else if (gobj is TemplateQualification)
      {
        subj = work.CreateUriNode(string.Format("<{0}>", objId));
        pred = work.CreateUriNode("dm:hasSubclass");
        obj = work.CreateUriNode(string.Format("<{0}>", subjId));
        work.Assert(new Triple(subj, pred, obj));
        subj = work.CreateUriNode(string.Format("<{0}>", subjId));
        pred = work.CreateUriNode("dm:hasSuperclass");
        obj = work.CreateUriNode(string.Format("<{0}>", objId));
        work.Assert(new Triple(subj, pred, obj));
      }
      else if (gobj is RoleQualification)
      {
        subj = work.CreateUriNode(string.Format("<{0}>", subjId));
        pred = work.CreateUriNode(rdfType);
        obj = work.CreateUriNode("tpl:R76288246068");
        work.Assert(new Triple(subj, pred, obj));
        pred = work.CreateUriNode("tpl:R99672026745");
        obj = work.CreateUriNode(string.Format("<{0}>", objId));
        work.Assert(new Triple(subj, pred, obj));
        pred = work.CreateUriNode(rdfType);
        obj = work.CreateUriNode("tpl:R67036823327");
        work.Assert(new Triple(subj, pred, obj));
      }
    }

    private void GenerateName(ref Graph work, QMXFName name, string subjId, object gobj)
    {
      subj = work.CreateUriNode(string.Format("<{0}>", subjId));
      pred = work.CreateUriNode("rdfs:label");
      obj = work.CreateLiteralNode(name.value, string.IsNullOrEmpty(name.lang) ? defaultLanguage : name.lang);
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateClassName(ref Graph work, QMXFName name, string subjId, object gobj)
    {
      subj = work.CreateUriNode(string.Format("rdl:{0}", subjId));
      pred = work.CreateUriNode("rdfs:label");
      obj = work.CreateLiteralNode(name.value, string.IsNullOrEmpty(name.lang) ? defaultLanguage : name.lang);
      work.Assert(new Triple(subj, pred, obj));
    }
    private void GenerateDescription(ref Graph work, Description descr, string subjectId)
    {
      subj = work.CreateUriNode(string.Format("tpl:{0}", subjectId));
      pred = work.CreateUriNode("rdfs:comment");
      obj = work.CreateLiteralNode(descr.value, string.IsNullOrEmpty(descr.lang) ? defaultLanguage : descr.lang);
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateClassDescription(ref Graph work, Description descr, string subjectId)
    {
      subj = work.CreateUriNode(string.Format("rdl:{0}", subjectId));
      pred = work.CreateUriNode("rdfs:comment");
      obj = work.CreateLiteralNode(descr.value, string.IsNullOrEmpty(descr.lang) ? defaultLanguage : descr.lang);
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateSuperClass(ref Graph work, string subjId, string objId)
    {
      subj = work.CreateUriNode(string.Format("<{0}>", subjId));
      pred = work.CreateUriNode("rdfs:subClassOf");
      obj = work.CreateUriNode(string.Format("<{0}>", objId));
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateRdfSubClass(ref Graph work, string subjId, string objId)
    {
      subj = work.CreateUriNode(string.Format("<{0}>", subjId));
      pred = work.CreateUriNode("rdfs:subClassOf");
      obj = work.CreateUriNode(string.Format("<{0}>", objId));
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateDmClassification(ref Graph work, string subjId, string objId)
    {
      subj = work.CreateUriNode(string.Format("<{0}>", subjId));
      pred = work.CreateUriNode("dm:hasClassified");
      obj = work.CreateUriNode(string.Format("<{0}>", objId));
      work.Assert(new Triple(subj, pred, obj));
      pred = work.CreateUriNode("dm:hasClassifier");
      work.Assert(new Triple(subj, pred, obj));
    }

    private void GenerateDmSubClass(ref Graph work, string subjId, string objId)
    {
      subj = work.CreateUriNode(string.Format("<{0}>", subjId));
      pred = work.CreateUriNode("dm:hasSubclass");
      obj = work.CreateUriNode(string.Format("<{0}>", objId));
      work.Assert(new Triple(subj, pred, obj));
    }
          #endregion
  }
}