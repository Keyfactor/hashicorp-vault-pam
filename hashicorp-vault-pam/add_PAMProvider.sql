declare @pamid uniqueidentifier;
set @pamid = newid();

insert into [pam].[ProviderTypes]([Id], [Name])
values (@pamid, 'Hashicorp-Vault');

insert into [pam].[ProviderTypeParams]([ProviderTypeId], [Name], [DisplayName], [DataType], [InstanceLevel])
values  (@pamid,'Host','Vault Host',1,0),
        (@pamid,'Token','Vault Token',1,0),
        (@pamid,'Path','KV Engine Path',1,0),
        (@pamid,'Secret','KV Secret Name',1,1),
        (@pamid,'Key','KV Secret Key',1,1);