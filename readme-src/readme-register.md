
For registering the Hashicorp Vault PAM Provider using Kerberos Auth, use the following `<register>` instead.

```xml
<register type="IPAMProvider" mapTo="Keyfactor.Extensions.Pam.Hashicorp.VaultPAMKerberos, hashicorp-vault-pam" name="Hashicorp-Vault" />
```
