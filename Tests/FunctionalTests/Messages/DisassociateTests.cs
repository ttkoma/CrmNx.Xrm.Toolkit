using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing.Functional;
using CrmNx.Xrm.Toolkit.Infrastructure;
using CrmNx.Xrm.Toolkit.ObjectModel;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests.Messages;

public class DisassociateTests : IntegrationTest<TestStartup>
{
    private const string M2M_AccountLeads_Association = "accountleads_association";
    private const string O2M_Account_Master_Account = "account_master_account";

    public DisassociateTests(TestStartup fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task Disassociate_Where_RelationName_NotExist_SHOULD_THROW_WebApiException()
    {
        const string relationshipName = "NotExist_FAKE_Relationship";

        const string entityName = "systemuser";
        var entityId = CrmClient.GetMyCrmUserId();

        var relationship = new Relationship(relationshipName);

        const string relatedEntityName = "task";
        var relatedEntityId = Guid.Parse("FACE0000-0000-0000-0000-000000000001");

        var relatedRefs = new EntityReference[]
        {
            new EntityReference(relatedEntityName, relatedEntityId),
        };

        var invoker = () => CrmClient.DisassociateAsync(entityName, entityId, relationship, relatedRefs);

        await invoker.Should().ThrowAsync<WebApiException>()
            .WithMessage(
                $"The URI segment '$ref' is invalid after the segment '{relationshipName}({relatedEntityId})'.");
    }

    [Fact]
    public async Task Disassociate_Where_TargetEntity_NotExist_SHOULD_THROW_WebApiException()
    {
        const string entityName = "account";
        var entityId = Guid.Parse("FACE0000-0000-0000-0000-000000000001");

        var relationship = new Relationship(schemaName: O2M_Account_Master_Account);

        var relatedRefs = new EntityReference[]
        {
            new EntityReference("lead", Guid.Parse("FACE0000-0000-0000-0000-000000000002")),
        };

        var invoker = () => CrmClient.DisassociateAsync(entityName, entityId, relationship, relatedRefs);

        await invoker.Should().ThrowAsync<WebApiException>()
            .WithMessage($"{entityName} With Id = {entityId} Does Not Exist");
    }

    [Fact]
    public async Task Disassociate_OneToMany_Where_RelatedEntity_NotExist_NOT_THROW_WebApiException()
    {
        const string entityName = "account";
        var entityId = await CrmClient.CreateAsync(new Entity(entityName));

        const string relatedEntityName = "account";
        var relatedEntityId = Guid.Parse("FACE0000-0000-FACE-0000-00000000FACE");

        var relationship = new Relationship(schemaName: O2M_Account_Master_Account);

        var relatedRefs = new EntityReference[]
        {
            new EntityReference(relatedEntityName, relatedEntityId),
        };

        try
        {
            await CrmClient.DisassociateAsync(entityName, entityId, relationship, relatedRefs);
        }
        finally
        {
            await CrmClient.DeleteAsync(new EntityReference(entityName, entityId));
        }
    }

    [Fact]
    public async Task Disassociate_OneToMany_Related_By_AlternateKey_IS_OK()
    {
        const string entityName = "account";
        var entityId = await CrmClient.CreateAsync(new Entity(entityName));

        const string relatedEntityName = "account";
        var alternateKeyValue = Guid.NewGuid().ToString()[..20];

        var relatedEntityId = await CrmClient.CreateAsync(new Entity(relatedEntityName)
        {
            ["accountnumber"] = alternateKeyValue
        });

        var relationship = new Relationship(schemaName: O2M_Account_Master_Account);

        var relatedRefs = new EntityReference[]
        {
            new EntityReference(relatedEntityName, "accountnumber", $"{alternateKeyValue}"),
        };

        try
        {
            await CrmClient.DisassociateAsync(entityName, entityId, relationship, relatedRefs);
        }
        finally
        {
            await CrmClient.DeleteAsync(new EntityReference(entityName, entityId));
            await CrmClient.DeleteAsync(new EntityReference(relatedEntityName, relatedEntityId));
        }
    }


    [Fact]
    public async Task Disassociate_ManyToMany_IS_OK()
    {
        const string entityName = "account";
        var entityId = await CrmClient.CreateAsync(new Entity(entityName));

        const string relatedEntityName = "lead";
        var relatedEntityId = await CrmClient.CreateAsync(new Entity(relatedEntityName));

        var relationship = new Relationship(schemaName: M2M_AccountLeads_Association);

        var relatedRefs = new EntityReference[]
        {
            new EntityReference(relatedEntityName, relatedEntityId),
        };

        try
        {
            await CrmClient.AssociateAsync(entityName, entityId, relationship, relatedRefs);
            await CrmClient.DisassociateAsync(entityName, entityId, relationship, relatedRefs);
        }
        finally
        {
            await CrmClient.DeleteAsync(new EntityReference(entityName, entityId));
            await CrmClient.DeleteAsync(new EntityReference(relatedEntityName, relatedEntityId));
        }
    }

    [Fact]
    public async Task Disassociate_ManyToMany_Where_RelatedEntity_NotExist_SHOULD_THROW_WebApiException()
    {
        const string entityName = "account";
        var entityId = await CrmClient.CreateAsync(new Entity(entityName));

        const string relatedEntityName = "lead";
        var referencedEntityId = Guid.Parse("FACE0000-0000-0000-0000-000000000002");

        var relationship = new Relationship(schemaName: M2M_AccountLeads_Association);

        var relatedRefs = new EntityReference[]
        {
            new EntityReference(relatedEntityName, referencedEntityId),
        };

        var invoker = () =>
            CrmClient.DisassociateAsync(entityName, entityId, relationship, relatedRefs);

        try
        {
            await invoker.Should().ThrowAsync<WebApiException>()
                .WithMessage($"{relatedEntityName} With Id = {referencedEntityId} Does Not Exist");
        }
        finally
        {
            await CrmClient.DeleteAsync(new EntityReference(entityName, entityId));
        }
    }

    [Fact]
    public async Task Disassociate_ManyToMany_MULTIPLE_OK()
    {
        const string entityName = "account";
        var entityId = await CrmClient.CreateAsync(new Entity(entityName));

        const string referencedEntityName = "lead";
        var referencedEntityId1 = await CrmClient.CreateAsync(new Entity(referencedEntityName));
        var referencedEntityId2 = await CrmClient.CreateAsync(new Entity(referencedEntityName));

        var relationship = new Relationship(schemaName: M2M_AccountLeads_Association);

        var relatedEntities = new EntityReference[]
        {
            new EntityReference(referencedEntityName, referencedEntityId1),
            new EntityReference(referencedEntityName, referencedEntityId2),
        };

        try
        {
            await CrmClient.DisassociateAsync(entityName, entityId, relationship, relatedEntities);
        }
        finally
        {
            await CrmClient.DeleteAsync(new EntityReference(entityName, entityId));
            await CrmClient.DeleteAsync(new EntityReference(referencedEntityName, referencedEntityId1));
            await CrmClient.DeleteAsync(new EntityReference(referencedEntityName, referencedEntityId2));
        }
    }

    [Fact]
    public async Task Disassociate_ManyToMany_Where_ReferencedIds_Duplicated_NOT_THROW_WebApiException()
    {
        const string entityName = "account";
        var entityId = await CrmClient.CreateAsync(new Entity(entityName));

        const string relatedEntityName = "lead";
        var relatedEntityId1 = await CrmClient.CreateAsync(new Entity(relatedEntityName));

        var relationship = new Relationship(schemaName: M2M_AccountLeads_Association);

        var relatedRefs = new EntityReference[]
        {
            new EntityReference(relatedEntityName, relatedEntityId1),
            new EntityReference(relatedEntityName, relatedEntityId1),
        };
        try
        {
            await CrmClient.DisassociateAsync(entityName, entityId, relationship, relatedRefs);
        }
        finally
        {
            await CrmClient.DeleteAsync(new EntityReference(entityName, entityId));
            await CrmClient.DeleteAsync(new EntityReference(relatedEntityName, relatedEntityId1));
        }
    }
}