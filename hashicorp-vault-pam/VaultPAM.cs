﻿// Copyright 2022 Keyfactor
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Keyfactor.Logging;
using Keyfactor.Platform.Extensions;
using Microsoft.Extensions.Logging;
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
            ILogger logger = LogHandler.GetClassLogger<VaultPAM>();
            logger.LogDebug($"PAM Provider {Name} - beginning PAM credential retrieval operation.");

            string host = initializationInfo["Host"];
            string path = initializationInfo["Path"];
            WebRequest req = WebRequest.Create($"{host}{path}/{instanceParameters["Secret"]}");
            req.Method = "GET";
            req.Headers.Add("X-Vault-Token", initializationInfo["Token"]);
            req.ContentType = "application/json";

            logger.LogDebug($"PAM Provider {Name} - requesting secret located at {host}{path}");
            Stream responseStream = req.GetResponse().GetResponseStream();
            logger.LogTrace($"PAM Provider {Name} - received response to secret request");

            Dictionary<string, string> response = JsonConvert.DeserializeObject<VaultResponseWrapper>(new StreamReader(responseStream).ReadToEnd()).data;

            logger.LogDebug($"PAM Provider {Name} - returning secret from vault");
            return response[instanceParameters["Key"]];
        }
    }
    public class VaultResponseWrapper
    {
        public Dictionary<string, string> data;
    }
}
