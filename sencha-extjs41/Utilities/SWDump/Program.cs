using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SemWeb;
using System.IO;
using System.Configuration;

namespace SWDump
{
  class Program
  {
    static void Main(string[] args)
    {
      try
      {
        if (args.Count() == 3)
        {
          string projectName = args[0];
          string applicationName = args[1];
          string filePath = args[2];

          string tripleStoreconnectionString = ConfigurationManager.AppSettings["TripleStoreConnectionString"];
          string scopeName = projectName + "_" + applicationName;
          string scopedConnectionString = ScopeConnectionString(tripleStoreconnectionString, scopeName);

          Dictionary<string, string> namespaceCollection = new Dictionary<string, string> {
             {"xsd", "http://www.w3.org/2001/XMLSchema#"},
             {"rdl", "http://rdl.rdlfacade.org/data#"},
             {"tpl", "http://tpl.rdlfacade.org/data#"},
             {"owl", "http://www.w3.org/2002/07/owl#"},
             {"eg", "http://www.example.com/data#"},
             {"rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"},
          };

          using (Store store = Store.Create(scopedConnectionString))
          {
            using (RdfXmlWriter rdfWriter = new RdfXmlWriter(filePath))
            {
              foreach (KeyValuePair<string, string> aliasURI in namespaceCollection)
              {
                rdfWriter.Namespaces.AddNamespace(aliasURI.Value, aliasURI.Key);
              }

              rdfWriter.Write(store);
            }
          }

          Console.WriteLine("SemWeb triplestore was successfully dumped to RDF: " + filePath);
        }
        else
        {
          PrintUsage();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error while dumping the RDF from SemWeb: \n" + ex.ToString() );
      }
    }

    static void PrintUsage()
    {
      Console.WriteLine("Usage:");
      Console.WriteLine("\n\tswdump.exe ProjectName ApplicationName FilePath");
    }

    static string ScopeConnectionString(string connectionString, string scopeName)
    {
      try
      {
        string scopedConnectionString = String.Empty;

        string credentialsTriplestoreMaster = @"Initial Catalog=master; User Id=iring; Password=iring;";
        string credentialsTriplestoreTemplate = @"Initial Catalog={0}; User Id={0}; Password={0};";
        string tripleStoreCredentials = String.Format(credentialsTriplestoreTemplate, scopeName);

        scopedConnectionString = connectionString.Replace(credentialsTriplestoreMaster, tripleStoreCredentials);

        return scopedConnectionString;
      }
      catch (Exception exception)
      {
        throw new Exception(String.Format("ScopeConnectionString[{0}]", connectionString), exception);
      }
    }
  }
}
