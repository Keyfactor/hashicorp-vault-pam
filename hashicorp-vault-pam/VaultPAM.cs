using Keyfactor.Platform.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Keyfactor.Extensions.Pam.Hashicorp
{
    public class VaultPAM : IPAMProvider
    {
        public string Name => "Hashicorp-Vault";

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
    public class VaultResponseWrapper
    {
        public Dictionary<string, string> data;
    }
}
