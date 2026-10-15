using Ofizzy.Api.Modules.Tenancy;

namespace Ofizzy.UnitTests;

public sealed class TenantModuleTests
{
    [Fact]
    public void WorkOrdersCanBeEnabledWithoutAutomotive()
    {
        var modules = new[]
        {
            ProductModule.Customers,
            ProductModule.Catalog,
            ProductModule.WorkOrders
        };

        Assert.True(AutomotiveTenantTemplate.ValidModules(modules));
    }

    [Fact]
    public void WorkOrdersStillRequireCustomersAndCatalog()
    {
        Assert.False(AutomotiveTenantTemplate.ValidModules([
            ProductModule.Customers,
            ProductModule.WorkOrders
        ]));

        Assert.False(AutomotiveTenantTemplate.ValidModules([
            ProductModule.Catalog,
            ProductModule.WorkOrders
        ]));
    }
}
