// Copyright 2023 Keyfactor
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Keyfactor.Extensions.Pam.Hashicorp
{
    public class VaultPAMKerberos : IPAMProvider
    {
        public string Name => "Hashicorp-Vault";

        public string GetPassword(Dictionary<string, string> instanceParameters, Dictionary<string, string> initializationInfo)
        {
            ILogger logger = LogHandler.GetClassLogger<VaultPAMKerberos>();
            logger.MethodEntry(LogLevel.Trace);
            Uri host = new Uri(initializationInfo["Host"]);
            string clientToken = VaultAPI.GetClientToken(host, Name);
            return VaultAPI.GetVaultValue(Name, instanceParameters, host, initializationInfo["Path"], clientToken);
        }
    }
}
