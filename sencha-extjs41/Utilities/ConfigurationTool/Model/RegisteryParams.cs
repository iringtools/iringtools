using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyConfig.Model
{
    class RegisteryParams
    {
        #region Properties

        public string RegAppName { get; set; }
        public string ServicesWebConfig { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public string ProxyCredentialToken { get; set; }
        public string ProxyHost { get; set; }
        public string ProxyPort { get; set; }

        #endregion //Properties
    }
}
