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

There is a considerable amount of set up that needs to be performed before kerberos authentication will work with the Hashicorp Vault PAM Provider.  Setting up an environment that can use this alternative authentication is out of scope for this integration, but below are the steps that were successfully performed for testing on a Windows based implementation.  Your configuration steps will likely be different.  For the steps below, all values that should be replaced with your own settings are surrounded by single curly braces - {value}.  Please note, in step 7 below, the "{{'{{'}}.UserDN}}" value shown **is** a literal.

1. Install [MIT Kerberos 4.1](https://web.mit.edu/kerberos/kfw-4.1/kfw-4.1.html) on the Hashicorp Vault server.
2. Modify the krb5.ini file in c:\ProgramData\MIT\Kerberos5 with the domain/realm values you will be using.
3. Run the following command on your vault server to enable kerberos - `vault auth enable -passthrough-request-headers=Authorization -allowed-response-headers=www-authenticate kerberos`
4. Create the vault service account keytab on the vault server - <pre>ktpass /princ HTTP/{ServiceAccount.FullyQualified.Domain@FullyQualified.Domain} /pass * /crypto RC4-HMAC-NT /out {c:\path\for\keytab} /mapuser {ServiceAccount@FullyQualified.Domain} /ptype KRB5_NT_PRINCIPAL</pre>
5. Convert the keytab file created in step 4 to Base64.
6. Write the keytab out to Hashicorp Vault and associate it with the Vault service account - `vault write auth/kerberos/config keytab-@{c:\path\to\file\created\in\step5} service_account="HTTP/{VaultServiceAccountName}"`
7. Configure the Kerberos auth method to communicate with LDAP using the service account configured above - `vault write auth/kerberos/config/ldap binddn={ServiceAccount.FullyQualified.Domain} bindpass={ServiceAccountPassword} groupattr=sAMAccountName groupdn="DC={DomainNode},DC={DomainNode}" groupfilter="(&(objectClass=group)(member:1.2.840.113556.1.4.1941:={{'{{'}}.UserDN}}))" userdn="OU={OUValue},DC={DomainNode},DC={DomainNode}" userattr=sAMAccountName upnDomain={Domain.Value} url=ldap://{LDAP.Domain.Value}:389`
8. Write the policy and capabilities to be assigned to an authenticated user - `vault policy write {VaultPolicyName} {c:\file\path\to\VaultPolicy.txt}` 
9. Configure the Vault policy that should be granted to those who successfully authenticate based on their LDAP group membership - `vault write /auth/kerberos/groups/{LDAPGroup} policies={VaultPolicyName}`
10. Set the spn for the Vault service account on AD - `setspn.exe -U -S HTTP/{VaultURL} {ServiceAccount}`

A more generic reference for the necessary configuration can be found at https://developer.hashicorp.com/vault/docs/auth/kerberos.