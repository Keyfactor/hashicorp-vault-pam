
# Hashicorp Vault PAM Provider

The Hashicorp Vault PAM Provider allows for the retrieval of stored account credentials from a Hashicorp Vault Secret store. A valid token with access to the secrets in the Vault is used to retrieve secrets from a specific secret path in the Vault.

#### Integration status: Production - Ready for use in production environments.

## About the Keyfactor PAM Provider

Keyfactor supports the retrieval of credentials from 3rd party Privileged Access Management (PAM) solutions. Secret values can normally be stored, encrypted at rest, in the Keyfactor Platform database. A PAM Provider can allow these secrets to be stored, managed, and rotated in an external platform. This integration is usually configured on the Keyfactor Platform itself, where the platform can request the credential values when needed. In certain scenarios, a PAM Provider can instead be run on a remote location in conjunction with a Keyfactor Orchestrator to allow credential requests to originate from a location other than the Keyfactor Platform.

## Support for Hashicorp Vault PAM Provider

Hashicorp Vault PAM Provider is supported by Keyfactor for Keyfactor customers. If you have a support issue, please open a support ticket via the Keyfactor Support Portal at https://support.keyfactor.com

###### To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.

---

#### Compatibility
This release requires Keyfactor version 9.10 or greater.
This release was tested against Hashicorp Vault version 1.9.4.
Using this on a Universal Orchestrator requires UO version 10.1 or greater.
---




### Initial Configuration of PAM Provider
In order to allow Keyfactor to use the new Hashicorp Vault PAM Provider, the definition needs to be added to the application database.
This is done by running the provided `kfutil` tool to install the PAM definition, which only needs to be done one time. It uses API credentials to access the Keyfactor instance and create the PAM definition.

The `kfutil` tool, after being [configured for API access](https://github.com/Keyfactor/kfutil#quickstart), can be run in the following manner to install the PAM definition from the Keyfactor repository:

```
kfutil pam types-create -r hashicorp-vault-pam -n Hashicorp-Vault
```

### Configuring Parameters
The following are the parameter names and a description of the values needed to configure the Hashicorp Vault PAM Provider.

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

![](images/config.png)

### Configuring for PAM Usage
#### In Hashicorp Vault
When configuring the Hashicorp Vault for use as a PAM Provider with Keyfactor, you will need to set up and configure the `kv` functionality in Vault. You will need an API Key that has the right permissions. The default `secret` location can be used, or another location.

After adding a secret object to `kv` with a key and value, you can use the object's name (the "KV Secret Name") and the secret's key (the "KV Secret Key") to retrieve credentials from the Hashicorp Vault as a PAM Provider.

#### On Keyfactor Universal Orchestrator
##### Installation
Configuring the UO to use the Hashicorp Vault PAM Provider requries first installing it as an extension by copying the release contents into a new extension folder named `Hashicorp-Vault`.
A `manifest.json` file is included in the release. This file needs to be edited to enter in the "initialization" parameters for the PAM Provider. Specifically values need to be entered for the parameters in the `manifest.json` of the __PAM Provider extension__:

~~~ json
"Keyfactor:PAMProviders:Hashicorp-Vault:InitializationInfo": {
    "Host": "http://127.0.0.1:8200",
    "Path": "v1/secret/data",
    "Token": "xxxxxx"
  }
~~~

Two other versions of the `manifest.json` are also included - `manifestTokenAuth.json` and `manifestKerberosAuth.json`.  `manifestTokenAuth.json` matches the contents of the supplied `manifest.json` file and is used if you wish to connect to Hashicorp Vault using token authentication.  `manifestKerberosAuth.json` should be used if you wish to connect via Kerberos authentication.  If this is the preference, copy `manifestKerberosAuth.json` to the extension folder and rename it to `manifest.json`.  Then add the "initialization" section referenced above.

Please refer to the following "Kerberos Authentication" section later in this README for more information on setting up the Hashicorp Vault PAM Provider to use Kerberos Authentication.

##### Usage
To use the PAM Provider to resolve a field, for example a Server Password, instead of entering in the actual value for the Server Password, enter a `json` object with the parameters specifying the field.
The parameters needed are the "instance" parameters above:

~~~ json
{"Secret":"my-kv-secret","Key":"myServerPassword"}
~~~

If a field supports PAM but should not use PAM, simply enter in the actual value to be used instead of the `json` format object above.

##### Kerberos Authentication
As mentioned earlier, by default the Hashicorp Vault PAM Provider authenticates to your Hashicorp Vault instance using token authentication.  An alternative method of authentication is available - Kerberos.  This method allows authentication via the credentials of the security context running the Hashicorp Vault PAM Provider.  A call is made to the Hashicorp Vault instance using the already authenticated Kerberos token and a separate Hashicorp Vault token is returned which is used for the remainder of the session.

There is a considerable amount of set up that needs to be performed before kerberos authentication will work with the Hashicorp Vault PAM Provider.  Setting up an environment that can use this alternative authentication is out of scope for this integration, but below are the steps that were successfully performed for testing on a Windows based implementation.  Your configuration steps will likely be different.  For the steps below, all values that should be replaced with your own settings are surrounded by single curly braces - {value}.  Please note, in step 7 below, the "{{.UserDN}}" value shown **is** a literal.

1. Install [MIT Kerberos 4.1](https://web.mit.edu/kerberos/kfw-4.1/kfw-4.1.html) on the Hashicorp Vault server.
2. Modify the krb5.ini file in c:\ProgramData\MIT\Kerberos5 with the domain/realm values you will be using.
3. Run the following command on your vault server to enable kerberos - `vault auth enable -passthrough-request-headers=Authorization -allowed-response-headers=www-authenticate kerberos`
4. Create the vault service account keytab on the vault server - `ktpass /princ HTTP/{ServiceAccount.FullyQualified.Domain@FullyQualified.Domain} /pass * /crypto RC4-HMAC-NT /out {c:\path\for\keytab} /mapuser {ServiceAccount@FullyQualified.Domain} /ptype KRB5_NT_PRINCIPAL`
5. Convert the keytab file created in step 4 to Base64.
6. Write the keytab out to Hashicorp Vault and associate it with the Vault service account - `vault write auth/kerberos/config keytab-@{c:\path\to\file\created\in\step5} service_account="HTTP/{VaultServiceAccountName}"`
7. Configure the Kerberos auth method to communicate with LDAP using the service account configured above - `vault write auth/kerberos/config/ldap binddn={ServiceAccount.FullyQualified.Domain} bindpass={ServiceAccountPassword} groupattr=sAMAccountName groupdn="DC={DomainNode},DC={DomainNode}" groupfilter="(&(objectClass=group)(member:1.2.840.113556.1.4.1941:={{.UserDN}}))" userdn="OU={OUValue},DC={DomainNode},DC={DomainNode}" userattr=sAMAccountName upnDomain={Domain.Value} url=ldap://{LDAP.Domain.Value}:389`
8. Write the policy and capabilities to be assigned to an authenticated user - `vault policy write {VaultPolicyName} {c:\file\path\to\VaultPolicy.txt}` 
9. Configure the Vault policy that should be granted to those who successfully authenticate based on their LDAP group membership - `vault write /auth/kerberos/groups/{LDAPGroup} policies={VaultPolicyName}`
10. Set the spn for the Vault service account on AD - `setspn.exe -U -S HTTP/{ServiceAccount.FullyQualified.Domain}:8200 {ServiceAccount}`
11. To test whether Kerberos authentication is configured correctly, run `vault login -method=kerberos username={UserNameToAuthenticateWith} service=HTTP/{ServiceAccount.FullyQualified.Domain} realm={Domain} keytab_path={PathToKeytabCreatedInStep5} krb5conf_path=c:\ProgramData\MIT\Kerberos5\krb5.ini disable_fast_negotiation=true.  If the configuration is correct, you should see a "Success!" message after running this command.

A more generic reference for the necessary configuration can be found at https://developer.hashicorp.com/vault/docs/auth/kerberos.

#### In Keyfactor - PAM Provider
##### Installation
In order to setup a new PAM Provider in the Keyfactor Platform for the first time, you will need to run the `kfutil` tool (see Initial Configuration of PAM Provider).

After the installation is run, the DLLs need to be installed to the correct location for the PAM Provider to function. From the release, the hashicorp-vault-pam.dll should be copied to the following folder locations in the Keyfactor installation. Once the DLL has been copied to these folders, edit the corresponding config file. You will need to add a new Unity entry as follows under `<container>`, next to other `<register>` tags.

| Install Location | DLL Binary Folder | Config File |
| --- | --- | --- |
| WebAgentServices | WebAgentServices\bin\ | WebAgentServices\web.config |
| Service | Service\ | Service\CMSTimerService.exe.config |
| KeyfactorAPI | KeyfactorAPI\bin\ | KeyfactorAPI\web.config |
| WebConsole | WebConsole\bin\ | WebConsole\web.config |

When enabling a PAM provider for Orchestrators only, the first line for `WebAgentServices` is the only installation needed.

The Keyfactor service and IIS Server should be restarted after making these changes.

```xml
<register type="IPAMProvider" mapTo="Keyfactor.Extensions.Pam.Hashicorp.VaultPAM, hashicorp-vault-pam" name="Hashicorp-Vault" />
```



For registering the Hashicorp Vault PAM Provider using Kerberos Auth, use the following `<register>` instead.

```xml
<register type="IPAMProvider" mapTo="Keyfactor.Extensions.Pam.Hashicorp.VaultPAMKerberos, hashicorp-vault-pam" name="Hashicorp-Vault" />
```

##### Usage
In order to use the PAM Provider, the provider's configuration must be set in the Keyfactor Platform. In the settings menu (upper right cog) you can select the ___Priviledged Access Management___ option to configure your provider instance.

![](images/setting.png)

After it is set up, you can now use your PAM Provider when configuring certificate stores. Any field that is treated as a Keyfactor secret, such as server passwords and certificate store passwords can be retrieved from your PAM Provider instead of being entered in directly as a secret.

![](images/password.png)


---





