using Keyfactor.Platform.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace VaultPAMProvider
{
    /*
    This SQL script will create the Vault PAM Provider Type in Keyfactor
    Insert Into pam.ProviderTypes VALUES ('46b8e156-4b31-40ad-ad26-4012a5f47177','Vault')
    Insert Into pam.ProviderTypeParams(ProviderTypeId, Name, DisplayName, DataType, InstanceLevel) VALUES 
	    ('46b8e156-4b31-40ad-ad26-4012a5f47177','Host','Vault Host',1,0),
	    ('46b8e156-4b31-40ad-ad26-4012a5f47177','Token','Vault Token',1,0),
	    ('46b8e156-4b31-40ad-ad26-4012a5f47177','Path','KV Engine Path',1,0),
    	 
    	('46b8e156-4b31-40ad-ad26-4012a5f47177','Secret','KV Secret Name',1,1),
    	('46b8e156-4b31-40ad-ad26-4012a5f47177','Key','KV Secret Key',1,1)

    */

    public class VaultResponseWrapper
    {
        public Dictionary<string, string> data;
    }

    public class VaultPAM : IPAMProvider
    {
        public string Name
        {
            get
            {
                return "Vault";
            }
        }

        public string GetPassword(Dictionary<string, string> instanceParameters, Dictionary<string, string> initializationInfo)
        {
            WebRequest req = WebRequest.Create($"{initializationInfo["Host"]}{initializationInfo["Path"]}/{instanceParameters["Secret"]}");
            req.Method = "GET";
            req.Headers.Add("X-Vault-Token", initializationInfo["Token"]);
            req.ContentType = "application/json";
            Stream responseStream = req.GetResponse().GetResponseStream();
            Dictionary<string, string> response = JsonConvert.DeserializeObject<VaultResponseWrapper>(new StreamReader(responseStream).ReadToEnd()).data;

            return response[instanceParameters["Key"]];
        }
    }
}
