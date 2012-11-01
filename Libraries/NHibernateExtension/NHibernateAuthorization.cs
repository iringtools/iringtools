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
using log4net;
using org.iringtools.utility;

namespace org.iringtools.nhibernate.ext
{
  public class NHibernateAuthorization : NHibernateDataLayer, IAuthorization
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateAuthorization));
    private NHibernateSettings _nhSettings;
    private string _authorizationPath = String.Empty;

    [Inject]
    public NHibernateAuthorization(AdapterSettings settings, IDictionary keyRing, NHibernateSettings nhSettings)
      : base(settings, keyRing)
    {
      _authorizationPath = string.Format("{0}Authorization.{1}.xml",
        _settings["AppDataPath"],
        _settings["Scope"]
      );

      _nhSettings = nhSettings;
    }

    public AccessLevel Authorize(string objectType, ref DataFilter dataFilter)
    {
      List<Object> objects = new List<Object>();
      //ISession session = null;

      try
      {
        //session = NHibernateSessionManager.Instance.GetSession(
        //  _nhSettings["AppDataPath"], _nhSettings["Scope"]);

        //if (session != null)
        //{
          if (_keyRing != null && _keyRing["UserName"] != null)
          {
            string userName = _keyRing["UserName"].ToString();
            userName = userName.Substring(userName.IndexOf('\\') + 1).ToLower();

            _logger.Debug("Authorizing user [" + userName + "]");

            if (userName == "anonymous")
            {
              return AccessLevel.Delete;
            }

            AuthorizedUsers authUsers = Utility.Read<AuthorizedUsers>(_authorizationPath, true);

            if (authUsers != null)
            {
              foreach (string authUser in authUsers)
              {
                if (authUser.ToLower() == userName)
                {
                  return AccessLevel.Delete;
                }
              }
            }
          }
          else
          {
            _logger.Error("KeyRing is empty.");
            return AccessLevel.AccessDenied;
          }
        //}
      }
      finally
      {
        //if (session != null)
        //  session.Close();
      }

      return AccessLevel.AccessDenied;
    }
  }
}
