Note that the `Path` Initialization Paramater is used to support Enterprise Namespaces when the Vault secrets fall under a namespace.

__Initialization Parameters for each defined PAM Provider instance__
| Initialization parameter | Display Name | Description |
| :---: | :---: | --- |
| Host | Vault Host | The IP address or URL of the Vault instance, including any port number |
| Token* | Vault Token | The access token for the Vault.   *For token auth only.  Not needed for Kerberos auth.|
| Path | KV Engine Path | The path to secrets in the Vault. By default this would be at 'v1/secret/data'. The full form of this paramater is 'v1/{namespace}/{kv secrets engine name}/data'. |

For Enterprise Namespace support, as mentioned above, the `Path` parameter is used to enter the namespace to be used. For example, if a new `kv` secrets engine was created named "keyfactor-secrets", the `Path` parameter would have a value 'v1/keyfactor-secrets/data'. If this `kv` secrets engine was under the namespace "Operations", the full `Path` parameter would be 'v1/Operations/keyfactor-secrets/data'.


__Instance Parameters for each retrieved secret field__
| Instance parameter | Display Name | Description |
| :---: | :---: | --- |
| Secret | KV Secret Name | The name of the secret in the Vault |
| Key | KV Secret Key | The key to the key-value pair of the secret to access |
