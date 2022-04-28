# Hashicorp Vault PAM Provider

The Hashicorp Vault PAM Provider allows for the retrieval of stored account credentials from a Hashicorp Vault Secret store. A valid token with access to the secrets in the Vault is used to retrieve secrets from a specific secret path in the Vault.

#### Integration status: Prototype - Demonstration quality. Not for use in customer environments.

## About the Keyfactor PAM Provider

Keyfactor supports the retrieval of credentials from 3rd party Priviledged Access Management (PAM) solutions. Secret values can normally be stored, encrypted at rest, in the Keyfactor Platform database. A PAM Provider can allow these secrets to be stored, managed, and rotated in an external platform. This integration is usually configured on the Keyfactor Platform itself, where the platform can request the credential values when needed. In certain scenarios, a PAM Provider can instead be run on a remote location in conjunction with a Keyfactor Orchestrator to allow credential requests to originate from a location other than the Keyfactor Platform.

---

### Configuring Parameters
The following are the parameter names and a description of the values needed to configure the Beyond Trust Password Safe PAM Provider.

| Initialization parameter | Description | Instance parameter | Description |
| :---: | --- | :---: | --- |
| Host | The IP address or URL of the Vault instance, including the API endpoint | Secret | The name of the secret in the Vault |
| Token | The access token for the Vault | Key | The key to the key-value pair of the secret to access |
| Path | The path to secrets in the Vault. Typically this is 'secret' |

