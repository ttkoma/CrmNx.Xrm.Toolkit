using System;
using System.Threading;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing.Functional;
using CrmNx.Xrm.Toolkit.Infrastructure;
using CrmNx.Xrm.Toolkit.ObjectModel;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests.Messages;

public class AssociateTests : IntegrationTest<TestStartup>
{
    private const string SystemUserEntityName = "systemuser";
    private const string M2M_AccountLeads_Association = "accountleads_association";
    private const string O2M_Account_Master_Account = "account_master_account";

    public AssociateTests(TestStartup fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task Associate_Where_RelationName_NotExist_SHOULD_THROW_WebApiException()
    {
        const string relationshipName = "NotExist_FAKE_Relationship";

        var userId = CrmClient.GetMyCrmUserId();

        var relationship = new Relationship(relationshipName);

        var relatedRefs = new EntityReference[]
        {
            new EntityReference("task", Setup.EntityId),
        };

        var invoker = () => CrmClient.AssociateAsync(SystemUserEntityName, userId, relationship, relatedRefs);

        await invoker.Should().ThrowAsync<WebApiException>()
            .WithMessage($"The URI segment '$ref' is invalid after the segment '{relationshipName}'.");
    }

    [Fact]
    public async Task Associate_OneToMany_Where_TargetEntity_NotExist_SHOULD_THROW_WebApiException()
    {
        var entityId = Guid.Parse("FACE0000-0000-0000-0000-000000000001");
        var entityName = "account";

        var relationship = new Relationship(schemaName: O2M_Account_Master_Account);

        var relatedRefs = new EntityReference[]
        {
            new EntityReference("account", Guid.Parse("FACE0000-0000-0000-0000-000000000002")),
        };

        var invoker = async () => { await CrmClient.AssociateAsync(entityName, entityId, relationship, relatedRefs); };

        await invoker.Should().ThrowAsync<WebApiException>()
            .WithMessage($"{entityName} With Id = {entityId} Does Not Exist");
    }

    [Fact]
    public async Task Associate_OneToMany_Where_RelatedEntity_NotExist_SHOULD_THROW_WebApiException()
    {
        const string entityName = "account";
        var entityId = await CrmClient.CreateAsync(new Entity(entityName));

        const string referencedEntityName = "account";
        var referencedEntityId = Guid.Parse("FACE0000-0000-0000-0000-000000000001");

        var relationship = new Relationship(schemaName: O2M_Account_Master_Account);

        var relatedRefs = new EntityReference[]
        {
            new EntityReference(referencedEntityName, referencedEntityId),
        };

        var invoker = () => CrmClient.AssociateAsync(entityName, entityId, relationship, relatedRefs);

        try
        {
            await invoker.Should().ThrowAsync<WebApiException>()
                .WithMessage($"{referencedEntityName} With Id = {referencedEntityId} Does Not Exist");
        }
        finally
        {
            await CrmClient.DeleteAsync(new EntityReference(entityName, entityId));
        }
    }

    [Fact]
    public async Task Associate_OneToMany_IS_OK()
    {
        const string entityName = "account";
        var entityId = await CrmClient.CreateAsync(new Entity(entityName));

        const string relatedEntityName = "account";
        var relatedEntityId = await CrmClient.CreateAsync(new Entity(relatedEntityName));

        var relationship = new Relationship(schemaName: O2M_Account_Master_Account);

        var relatedRefs = new EntityReference[]
        {
            new EntityReference(relatedEntityName, relatedEntityId),
        };

        try
        {
            await CrmClient.AssociateAsync(entityName, entityId, relationship, relatedRefs);
        }
        finally
        {
            await CrmClient.DeleteAsync(new EntityReference(relatedEntityName, relatedEntityId));
            await CrmClient.DeleteAsync(new EntityReference(entityName, entityId));
        }
    }

    [Fact]
    public async Task Associate_ManyToMany_Where_RelatedEntity_NotExist_SHOULD_THROW_WebApiException()
    {
        const string entityName = "account";
        var entityId = await CrmClient.CreateAsync(new Entity(entityName));

        const string relatedEntityName = "lead";
        var relatedEntityId1 = Guid.Parse("FACE0000-0000-0000-0000-000000000002");

        var relationship = new Relationship(schemaName: M2M_AccountLeads_Association);

        var relatedRefs = new EntityReference[]
        {
            new EntityReference(relatedEntityName, relatedEntityId1),
        };

        try
        {
            var invoker = () => CrmClient.AssociateAsync(entityName, entityId, relationship, relatedRefs);

            await invoker.Should().ThrowAsync<WebApiException>()
                .WithMessage($"{relatedEntityName} With Id = {relatedEntityId1} Does Not Exist");
        }
        finally
        {
            await CrmClient.DeleteAsync(new EntityReference(entityName, entityId));
        }
    }

    [Fact]
    public async Task Associate_ManyToMany_MULTIPLE_OK()
    {
        const string entityName = "account";
        var entityId = await CrmClient.CreateAsync(new Entity(entityName));

        const string relatedEntityIName = "lead";
        var relatedEntityId1 = await CrmClient.CreateAsync(new Entity(relatedEntityIName));
        var relatedEntityId2 = await CrmClient.CreateAsync(new Entity(relatedEntityIName));

        var relationship = new Relationship(schemaName: M2M_AccountLeads_Association);

        var relatedEntities = new EntityReference[]
        {
            new EntityReference(relatedEntityIName, relatedEntityId1),
            new EntityReference(relatedEntityIName, relatedEntityId2),
        };

        try
        {
            await CrmClient.AssociateAsync(entityName, entityId, relationship, relatedEntities);
        }
        finally
        {
            await CrmClient.DeleteAsync(new EntityReference(entityName, entityId));
            await CrmClient.DeleteAsync(new EntityReference(relatedEntityIName, relatedEntityId1));
            await CrmClient.DeleteAsync(new EntityReference(relatedEntityIName, relatedEntityId2));
        }
    }

    [Fact]
    public async Task Associate_ManyToMany_Where_RelatedEntities_IsDuplicated_SHOULD_THROW_WebApiException()
    {
        const string entityName = "account";
        var entityId = await CrmClient.CreateAsync(new Entity(entityName));

        const string relatedEntityName = "lead";
        var relatedEntityId = await CrmClient.CreateAsync(new Entity(relatedEntityName));

        var relationship = new Relationship(schemaName: M2M_AccountLeads_Association);

        var relatedRefs = new EntityReference[]
        {
            new EntityReference(relatedEntityName, relatedEntityId),
            new EntityReference(relatedEntityName, relatedEntityId),
        };


        var invoker = () => CrmClient.AssociateAsync(entityName, entityId, relationship, relatedRefs);

        try
        {
            await invoker.Should().ThrowAsync<WebApiException>()
                .WithMessage("A record with matching key values already exists.");
        }
        finally
        {
            await CrmClient.DeleteAsync(new EntityReference(entityName, entityId));
            await CrmClient.DeleteAsync(new EntityReference(relatedEntityName, relatedEntityId));
        }
    }
}