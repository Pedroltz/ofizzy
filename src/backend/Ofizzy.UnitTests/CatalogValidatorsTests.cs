using Ofizzy.Api.Modules.Customers;
using Ofizzy.Api.Modules.Parts;
using Ofizzy.Api.Modules.Services;
using Ofizzy.Api.Verticals.Automotive;
using Ofizzy.Api.Modules.WorkOrders;

namespace Ofizzy.UnitTests;

public sealed class CatalogValidatorsTests
{
    [Fact]
    public void CustomerRequiresValidDocumentLengthWhenProvided() => Assert.False(new CustomerRequestValidator().Validate(new CustomerRequest("João", "123", null, null, null, null, null)).IsValid);

    [Fact]
    public void VehicleRejectsNegativeMileage() => Assert.False(new VehicleRequestValidator().Validate(new VehicleRequest(Guid.NewGuid(), "ABC1D23", null, "Corsa", 2020, null, -1, null, null)).IsValid);

    [Fact]
    public void ServiceAcceptsZeroDefaultPrice() => Assert.True(new ServiceRequestValidator().Validate(new ServiceRequest("Diagnóstico", null, 0)).IsValid);

    [Fact]
    public void PartRejectsNegativePrice() => Assert.False(new PartRequestValidator().Validate(new PartRequest("Filtro", "FLT-1", 10, -1)).IsValid);

    [Fact]
    public void WorkOrderRejectsZeroQuantity() => Assert.False(new WorkOrderRequestValidator().Validate(new WorkOrderRequest(Guid.NewGuid(), Guid.NewGuid(), 10, null, null, null, [new(null, "Alinhamento", 0, 80)], [])).IsValid);
}
