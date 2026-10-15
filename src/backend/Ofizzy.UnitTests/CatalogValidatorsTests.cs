using Ofizzy.Api.Modules.Customers;
using Ofizzy.Api.Modules.Parts;
using Ofizzy.Api.Modules.Services;
using Ofizzy.Api.Modules.WorkOrders;
using Ofizzy.Api.Verticals.Automotive;

namespace Ofizzy.UnitTests;

public sealed class CatalogValidatorsTests
{
    [Fact]
    public void CustomerRequiresValidDocumentLengthWhenProvided()
    {
        // Arrange
        var validator = new CustomerRequestValidator();
        var request = new CustomerRequest(
            Name: "João",
            Document: "123",
            Phone: null,
            WhatsApp: null,
            Email: null,
            Address: null,
            Notes: null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void VehicleRejectsNegativeMileage()
    {
        // Arrange
        var validator = new VehicleRequestValidator();
        var request = new VehicleRequest(
            CustomerId: Guid.NewGuid(),
            Plate: "ABC1D23",
            Brand: null,
            Model: "Corsa",
            Year: 2020,
            Color: null,
            Mileage: -1,
            Chassis: null,
            Notes: null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ServiceAcceptsZeroDefaultPrice()
    {
        // Arrange
        var validator = new ServiceRequestValidator();
        var request = new ServiceRequest(
            Name: "Diagnóstico",
            Description: null,
            DefaultPrice: 0);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void PartRejectsNegativePrice()
    {
        // Arrange
        var validator = new PartRequestValidator();
        var request = new PartRequest(
            Name: "Filtro",
            Code: "FLT-1",
            CostPrice: 10,
            SalePrice: -1);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void WorkOrderRejectsZeroQuantity()
    {
        // Arrange
        var validator = new WorkOrderRequestValidator();
        var serviceLines = new WorkOrderLineRequest[]
        {
            new(null, "Alinhamento", 0, 80)
        };
        var request = new WorkOrderRequest(
            CustomerId: Guid.NewGuid(),
            VehicleId: Guid.NewGuid(),
            Mileage: 10,
            Complaint: null,
            Diagnosis: null,
            Notes: null,
            Services: serviceLines,
            Parts: []);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }
}
