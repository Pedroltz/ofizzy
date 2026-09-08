using QuestPDF.Fluent;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.WorkOrders;
using Xunit;

namespace Ofizzy.UnitTests;

public class WorkOrderPdfTests
{
    static WorkOrderPdfTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    [Fact]
    public void Generates_Pdf_Successfully_For_WorkOrder()
    {
        var company = new Company
        {
            Name = "Ofizzy",
            LegalName = "Ofizzy LTDA",
            Cnpj = "12.345.678/0001-90",
            Phone = "(11) 99999-9999",
            Address = "Av. Principal, 1000",
            City = "São Paulo",
            State = "SP",
            WarrantyTerms = "Garantia legal de 90 dias conforme CDC."
        };

        var order = new WorkOrder
        {
            Number = 1,
            CustomerName = "Carlos Silva",
            CustomerDocument = "123.456.789-00",
            CustomerPhone = "(11) 98888-7777",
            VehiclePlate = "BRA2E19",
            VehicleDescription = "Volkswagen Gol 1.6 Total Flex",
            Mileage = 85200,
            Complaint = "Ruído na suspensão dianteira ao passar em lombadas.",
            Diagnosis = "Amortecedores dianteiros desgastados com vazamento de óleo.",
            Notes = "Veículo liberado e testado em rodagem.",
            Status = WorkOrderStatus.Completed,
            CompletedAt = DateTimeOffset.UtcNow,
            Services =
            [
                new WorkOrderService { Description = "Substituição dos amortecedores dianteiros", Quantity = 2, UnitPrice = 120 },
                new WorkOrderService { Description = "Alinhamento computadorizado e balanceamento", Quantity = 1, UnitPrice = 90 }
            ],
            Parts =
            [
                new WorkOrderPart { Code = "AM-802", Description = "Amortecedor Dianteiro Cofap", Quantity = 2, UnitPrice = 320 },
                new WorkOrderPart { Code = "KIT-01", Description = "Kit Batente e Coifa Dianteira", Quantity = 2, UnitPrice = 65 }
            ]
        };

        var doc = new WorkOrderPdfDocument(order, company);
        var bytes = doc.GeneratePdf();

        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
        Assert.True(bytes.Length > 1000);
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'D', bytes[2]);
        Assert.Equal((byte)'F', bytes[3]);
    }
}
