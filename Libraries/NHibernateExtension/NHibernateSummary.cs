using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using NHibernate;
using org.iringtools.nhibernate;
using org.iringtools.library;
using org.iringtools.adapter.datalayer;
using org.iringtools.adapter;

namespace org.iringtools.nhibernate.ext
{
  public class NHibernateSummary : NHibernateDataLayer, ISummary
  {
    private NHibernateSettings _nhSettings;

    [Inject]
    public NHibernateSummary(AdapterSettings settings, IDictionary keyRing, NHibernateSettings nhSettings)
      : base(settings, keyRing) 
    {
      _nhSettings = nhSettings;
    }

    public override IList<Object> GetSummary()
    {
      List<Object> objects = new List<Object>();
      ISession session = null;

      try
      {
        string configPath = string.Format("{0}SummaryConfig.{1}.xml", _nhSettings["AppDataPath"], _nhSettings["Scope"]);
        SummaryConfig config = utility.Utility.Read<SummaryConfig>(configPath);

        session = NHibernateSessionManager.Instance.GetSession(
          _nhSettings["AppDataPath"], _nhSettings["Scope"]);

        if (session != null)
        {
          foreach (SummaryItem summaryItem in config)
          {
            List<string> headers = summaryItem.Headers;
            IQuery query = session.CreateSQLQuery(summaryItem.Query);
            IList<object> resultSet = query.List<object>();

            foreach (object result in resultSet)
            {
              IDictionary<String, String> nameValuePairs = new Dictionary<String, String>();
              
              if (result.GetType().IsArray)
              {
                object[] values = (object[])result;

                for (int i = 0; i < values.Length; i++)
                {
                  nameValuePairs[headers[i]] = values[i].ToString();
                }
              }
              else
              {
                nameValuePairs[headers[0]] = result.ToString();
              }

              objects.Add(nameValuePairs);
            }
          }          
        }
      }
      finally
      {
        if (session != null)
          session.Close();
      }

      return objects;
    }
  }
}
