using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using org.iringtools.library;
namespace DbDictionaryEditor.Web
{
    [ServiceContract]
    public interface IDbDictionaryService
    {
        [OperationContract]
        Collection<ScopeProject> GetScopes();

        [OperationContract]
        DatabaseDictionary GetDbDictionary(string project, string application);

        [OperationContract]
        DatabaseDictionary GetDatabaseSchema(string connString, string dbProvider);

        [OperationContract]
        void SaveDabaseDictionary(DatabaseDictionary dict, string project, string application);

        [OperationContract]
        List<string> GetExistingDbDictionaryFiles();

        [OperationContract]
        string[] GetProviders();

        [OperationContract]
        Response PostDictionaryToAdapterService(string projectName, string applicationName);

        [OperationContract]
        Response ClearTripleStore(string projectName, string applicationName);

        [OperationContract]
        Response DeleteApp(string ProjectName, string applicationName);
    }  
}
