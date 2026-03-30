<h1 align="center" style="border-bottom: none">
    Hashicorp Vault PAM Provider
</h1>

<p align="center">
  <!-- Badges -->
<img src="https://img.shields.io/badge/integration_status-production-3D1973?style=flat-square" alt="Integration Status: production" />
<a href="https://github.com/Keyfactor/hashicorp-vault-pam/releases"><img src="https://img.shields.io/github/v/release/Keyfactor/hashicorp-vault-pam?style=flat-square" alt="Release" /></a>
<img src="https://img.shields.io/github/issues/Keyfactor/hashicorp-vault-pam?style=flat-square" alt="Issues" />
<img src="https://img.shields.io/github/downloads/Keyfactor/hashicorp-vault-pam/total?style=flat-square&label=downloads&color=28B905" alt="GitHub Downloads (all assets, all releases)" />
</p>

<p align="center">
  <!-- TOC -->
  <a href="#support">
    <b>Support</b>
  </a> 
  ·
  <a href="#getting-started">
    <b>Installation</b>
  </a>
  ·
  <a href="#license">
    <b>License</b>
  </a>
  ·
  <a href="https://github.com/orgs/Keyfactor/repositories?q=pam">
    <b>Related Integrations</b>
  </a>
</p>

## Overview

The Hashicorp Vault PAM Provider allows for the retrieval of stored account credentials from a Hashicorp Vault Secret store. A valid token with access to the secrets in the Vault is used to retrieve secrets from a specific secret path in the Vault.

## Requirements
This release requires Keyfactor version 9.10 or greater.
This release was tested against Hashicorp Vault version 1.9.4.
Using this on a Universal Orchestrator requires UO version 10.1 or greater.

## Installation and Configuration

#### In Hashicorp Vault
When configuring the Hashicorp Vault for use as a PAM Provider with Keyfactor, you will need to set up and configure the `kv` functionality in Vault. You will need an API Key that has the right permissions. The default `secret` location can be used, or another location.

After adding a secret object to `kv` with a key and value, you can use the object's name (the "KV Secret Name") and the secret's key (the "KV Secret Key") to retrieve credentials from the Hashicorp Vault as a PAM Provider.

#### On the Universal Orchestrator
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

## Usage
To use the PAM Provider to resolve a field, for example a Server Password, instead of entering in the actual value for the Server Password, enter a `json` object with the parameters specifying the field.
The parameters needed are the "instance" parameters above:

~~~ json
{"Secret":"my-kv-secret","Key":"myServerPassword"}
~~~

If a field supports PAM but should not use PAM, simply enter in the actual value to be used instead of the `json` format object above.

## Kerberos Authentication
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

## Support
The Hashicorp Vault PAM Provider is supported by Keyfactor for Keyfactor customers. If you have a support issue, please open a support ticket with your Keyfactor representative. If you have a support issue, please open a support ticket via the Keyfactor Support Portal at https://support.keyfactor.com. 

> To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.

## Getting Started

The Hashicorp Vault PAM Provider is used by Command to resolve PAM-eligible credentials for Universal Orchestrator extensions and for accessing Certificate Authorities. When configured, Command will use the Hashicorp Vault PAM Provider to retrieve credentials needed to communicate with the target system. There are two ways to install the Hashicorp Vault PAM Provider, and you may elect to use one or both methods:

1. **Locally on the Keyfactor Command server**: PAM credential resolution via the Hashicorp Vault PAM Provider will occur on the Keyfactor Command server each time an elegible credential is needed.
2. **Remotely On Universal Orchestrators**: When Jobs are dispatched to Universal Orchestrators, the associated Certificate Store extension assembly will use the Hashicorp Vault PAM Provider to resolve eligible PAM credentials.

Before proceeding with installation, you should consider which pattern is best for your requirements and use case.

### Installation

> [!IMPORTANT]
> For the most up-to-date and complete documentation on how to install a PAM provider extension, please visit our [product documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/ReferenceGuide/Preparing%20Third%20Party%20PAM%20Providers%20to%20Work%20with.htm?Highlight=pam%20provider#InstallingCustomPAMProviderExtensions)


To install Hashicorp Vault PAM Provider, it is recommended you install [kfutil](https://github.com/Keyfactor/kfutil). `kfutil` is a command-line tool that simplifies the process of creating PAM Types in Keyfactor Command.







#### Requirements
   TODO Requirements is a required section

#### Create PAM type in Keyfactor Command


##### Using `kfutil`
Create the required PAM Types in the connected Command platform.

```shell
# Hashicorp-Vault
kfutil pam types-create -r hashicorp-vault-pam -n Hashicorp-Vault
```

##### Using the API
For full API docs please visit our [product documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/WebAPI/KeyfactorAPI/PAMProvidersPOSTTypes.htm?Highlight=pam%20type)

Below is the payload to `POST` to the Keyfactor Command API
```json
{
    "Name": "Hashicorp-Vault",
    "Parameters": [
        {
            "Name": "Host",
            "DisplayName": "Vault Host",
            "DataType": 1,
            "InstanceLevel": false
        },
        {
            "Name": "Token",
            "DisplayName": "Vault Token",
            "DataType": 2,
            "InstanceLevel": false
        },
        {
            "Name": "Path",
            "DisplayName": "KV Engine Path",
            "DataType": 1,
            "InstanceLevel": false
        },
        {
            "Name": "Secret",
            "DisplayName": "KV Secret Name",
            "DataType": 1,
            "InstanceLevel": true
        },
        {
            "Name": "Key",
            "DisplayName": "KV Secret Key",
            "DataType": 1,
            "InstanceLevel": true
        }
    ]
}
```

#### Install PAM provider on Keyfactor Command Host (Local)


("TODO Platform Install is an optional section. If this section doesn't seem necessary on initial glance, please delete it. Refer to the docs on [Confluence](https://keyfactor.atlassian.net/wiki/x/SAAyHg) for more info",)


#### Install PAM provider on a Universal Orchestrator Host (Remote)


("TODO Orchestrator Install is an optional section. If this section doesn't seem necessary on initial glance, please delete it. Refer to the docs on [Confluence](https://keyfactor.atlassian.net/wiki/x/SAAyHg) for more info",)







### Usage






#### From Keyfactor Command Host (Local)


("TODO Platform Usage is an optional section. If this section doesn't seem necessary on initial glance, please delete it. Refer to the docs on [Confluence](https://keyfactor.atlassian.net/wiki/x/SAAyHg) for more info",)


#### From a Universal Orchestrator Host (Remote)


("TODO Orchestrator Usage is an optional section. If this section doesn't seem necessary on initial glance, please delete it. Refer to the docs on [Confluence](https://keyfactor.atlassian.net/wiki/x/SAAyHg) for more info",)




> [!NOTE]
> Additional information on Hashicorp-Vault can be found in the [supplemental documentation](docs/hashicorp-vault.md).



## License

Apache License 2.0, see [LICENSE](LICENSE)

## Related Integrations

See all [Keyfactor PAM Provider extensions](https://github.com/orgs/Keyfactor/repositories?q=pam).