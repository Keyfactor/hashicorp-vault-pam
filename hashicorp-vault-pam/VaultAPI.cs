﻿// Copyright 2023 Keyfactor 
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

using System;
using Keyfactor.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;


namespace Keyfactor.Extensions.Pam.Hashicorp
{
    internal class VaultAPI
    {
        internal static string GetVaultValue(string name, Dictionary<string, string> instanceParameters, Uri host, string path, string token)
        {
            ILogger logger = LogHandler.GetClassLogger<VaultAPI>();
            logger.LogDebug($"PAM Provider {name} - beginning PAM credential retrieval operation.");

            UriBuilder uri = new UriBuilder(host)
            {
                Path = path
            };
            WebRequest req = WebRequest.Create($"{uri.Uri}/{instanceParameters["Secret"]}");
            req.Method = "GET";
            req.Headers.Add("X-Vault-Token", token);
            req.ContentType = "application/json";

            logger.LogDebug($"PAM Provider {name} - requesting secret located at {uri.Uri}");
            Stream responseStream = req.GetResponse().GetResponseStream();
            logger.LogTrace($"PAM Provider {name} - received response to secret request");

            string strResponse = new StreamReader(responseStream).ReadToEnd();
            Dictionary<string, Dictionary<string, string>> response = JsonConvert.DeserializeObject<VaultResponseWrapper>(strResponse).data;

            logger.LogDebug($"PAM Provider {name} - returning secret from vault");
            return response["data"][instanceParameters["Key"]];
        }

        internal static string GetClientToken(Uri host, string name)
        {
            ILogger logger = LogHandler.GetClassLogger<VaultAPI>();
            logger.LogDebug($"PAM Provider {name} - beginning PAM client auth token kerberos retrieval operation.");

            var handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true
            };

            // Create an instance of HttpClient with the handler
            using (var client = new HttpClient(handler))
            {
                string authEndpoint = "v1/auth/kerberos/login";

                logger.LogDebug($"PAM Provider {name} - requesting client auth token from kerberos authentication request at {host}");
                var kerberosResponse = client.PostAsync($"{host}{authEndpoint}", null).GetAwaiter().GetResult();
                logger.LogTrace($"PAM Provider {name} - received response to kerberos auth request");

                kerberosResponse.EnsureSuccessStatusCode();
                var kerberosContent = kerberosResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                JObject kerb = JObject.Parse(kerberosContent);

                logger.LogDebug($"PAM Provider {name} - returning client token from vault");
                return kerb["auth"].Value<string>("client_token");
            }
        }
    }
    internal class VaultResponseWrapper
    {
        public Dictionary<string, Dictionary<string, string>> data;
    }
}
