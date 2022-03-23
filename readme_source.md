### Initial Configuration of PAM Provider
In order to allow Keyfactor to use the new Hashicorp-Vault PAM Provider, the definition needs to be added to the application database.
This is done by running the provided [add_PAMProvider.sql](./hashicorp-vault-pam/add_PAMProvider.sql) script on the Keyfactor application database, which only needs to be done one time.

If you have a hosted environment or need assistance completing this step, please contact Keyfactor Support.

### Configuring Parameters
The following are the parameter names and a description of the values needed to configure the Hashicorp Vault PAM Provider.

| Initialization parameter | Description | Instance parameter | Description |
| :---: | --- | :---: | --- |
| Host | The IP address or URL of the Vault instance, including the API endpoint | Secret | The name of the secret in the Vault |
| Token | The access token for the Vault | Key | The key to the key-value pair of the secret to access |
| Path | The path to secrets in the Vault. Typically this is 'secret' |
