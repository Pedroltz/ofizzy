using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Ofizzy.Api.Infrastructure.Persistence;

namespace Ofizzy.Api.Modules.WorkOrders;

public sealed class WorkOrderPdfDocument(WorkOrder order, Ofizzy.Api.Infrastructure.Persistence.Company company) : IDocument
{
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(32, Unit.Point);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9.5f).FontColor("#111827"));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem(3).Column(col =>
                {
                    col.Item().Text(company.Name).FontSize(14).Bold().FontColor("#0f172a");
                    if (!string.IsNullOrWhiteSpace(company.LegalName))
                    {
                        col.Item().Text(company.LegalName).FontSize(8).FontColor("#475569");
                    }
                    var docPhone = string.Join(" · ", new[]
                    {
                        !string.IsNullOrWhiteSpace(company.Cnpj) ? $"CNPJ: {company.Cnpj}" : null,
                        !string.IsNullOrWhiteSpace(company.Phone) ? $"Fone: {company.Phone}" : null,
                        !string.IsNullOrWhiteSpace(company.WhatsApp) ? $"WhatsApp: {company.WhatsApp}" : null
                    }.Where(x => x is not null));

                    if (!string.IsNullOrWhiteSpace(docPhone))
                        col.Item().Text(docPhone).FontSize(8.5f).FontColor("#334155");

                    var addr = string.Join(", ", new[]
                    {
                        company.Address,
                        !string.IsNullOrWhiteSpace(company.City) ? $"{company.City}-{company.State}" : null,
                        company.PostalCode
                    }.Where(x => !string.IsNullOrWhiteSpace(x)));

                    if (!string.IsNullOrWhiteSpace(addr))
                        col.Item().Text(addr).FontSize(8.5f).FontColor("#475569");
                });

                row.RelativeItem(2).AlignRight().Column(col =>
                {
                    col.Item().Text($"ORDEM DE SERVIÇO #{order.Number:D4}").FontSize(14).Bold().FontColor("#0f172a");
                    col.Item().Text($"Abertura: {order.CreatedAt:dd/MM/yyyy HH:mm}").FontSize(8.5f);
                    if (order.CompletedAt.HasValue)
                    {
                        col.Item().Text($"Conclusão: {order.CompletedAt.Value:dd/MM/yyyy HH:mm}").FontSize(8.5f);
                    }
                    col.Item().Text($"Status: {StatusLabel(order.Status).ToUpperInvariant()}").FontSize(8.5f).Bold();
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1.5f).LineColor("#0f172a");
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(10).Column(column =>
        {
            // BLOCO 1: CLIENTE E VEÍCULO
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("DADOS DO CLIENTE").FontSize(8).Bold().FontColor("#64748b");
                    col.Item().Text(order.CustomerName).FontSize(11).Bold();
                    col.Item().Text($"CPF/CNPJ: {order.CustomerDocument ?? "Não informado"}").FontSize(8.5f);
                    col.Item().Text($"Telefone: {order.CustomerPhone ?? "Não informado"}").FontSize(8.5f);
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("VEÍCULO ATENDIDO").FontSize(8).Bold().FontColor("#64748b");
                    col.Item().Row(r =>
                    {
                        r.AutoItem().Text($"PLACA: {order.VehiclePlate}").FontSize(11).Bold();
                    });
                    col.Item().Text(order.VehicleDescription).FontSize(9.5f).Bold();
                    col.Item().Text($"Quilometragem: {(order.Mileage.HasValue ? $"{order.Mileage.Value:N0} km" : "Não informada")}").FontSize(8.5f);
                });
            });

            column.Item().PaddingVertical(8).LineHorizontal(1).LineColor("#cbd5e1");

            // BLOCO 2: APONTAMENTOS TÉCNICOS
            if (!string.IsNullOrWhiteSpace(order.Complaint) || !string.IsNullOrWhiteSpace(order.Diagnosis))
            {
                column.Item().Row(row =>
                {
                    if (!string.IsNullOrWhiteSpace(order.Complaint))
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("QUEIXA / SOLICITAÇÃO DO CLIENTE").FontSize(8).Bold().FontColor("#64748b");
                            col.Item().Text(order.Complaint).FontSize(9);
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(order.Diagnosis))
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("DIAGNÓSTICO TÉCNICO").FontSize(8).Bold().FontColor("#64748b");
                            col.Item().Text(order.Diagnosis).FontSize(9);
                        });
                    }
                });

                column.Item().PaddingVertical(8).LineHorizontal(1).LineColor("#cbd5e1");
            }

            // BLOCO 3: MÃO DE OBRA / SERVIÇOS
            var servicesTotal = order.Services.Sum(s => s.Quantity * s.UnitPrice);
            column.Item().Column(col =>
            {
                col.Item().Text("SERVIÇOS / MÃO DE OBRA").FontSize(9).Bold().FontColor("#0f172a");

                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(5);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).BorderColor("#0f172a").PaddingVertical(3).Text("Descrição").Bold().FontSize(8.5f);
                        header.Cell().BorderBottom(1).BorderColor("#0f172a").PaddingVertical(3).AlignCenter().Text("Qtd").Bold().FontSize(8.5f);
                        header.Cell().BorderBottom(1).BorderColor("#0f172a").PaddingVertical(3).AlignRight().Text("Unitário").Bold().FontSize(8.5f);
                        header.Cell().BorderBottom(1).BorderColor("#0f172a").PaddingVertical(3).AlignRight().Text("Total").Bold().FontSize(8.5f);
                    });

                    foreach (var s in order.Services)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(4).Text(s.Description).FontSize(9);
                        table.Cell().BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(4).AlignCenter().Text(s.Quantity.ToString("G29")).FontSize(9);
                        table.Cell().BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(4).AlignRight().Text(s.UnitPrice.ToString("C2")).FontSize(9);
                        table.Cell().BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(4).AlignRight().Text((s.Quantity * s.UnitPrice).ToString("C2")).Bold().FontSize(9);
                    }
                });

                col.Item().PaddingTop(4).AlignRight().Text($"Subtotal Serviços: {servicesTotal:C2}").Bold().FontSize(9);
            });

            column.Item().PaddingVertical(8).LineHorizontal(1).LineColor("#cbd5e1");

            // BLOCO 4: PEÇAS & INSUMOS
            var partsTotal = order.Parts.Sum(p => p.Quantity * p.UnitPrice);
            if (order.Parts.Count > 0)
            {
                column.Item().Column(col =>
                {
                    col.Item().Text("PEÇAS & INSUMOS").FontSize(9).Bold().FontColor("#0f172a");

                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).BorderColor("#0f172a").PaddingVertical(3).Text("Código").Bold().FontSize(8.5f);
                            header.Cell().BorderBottom(1).BorderColor("#0f172a").PaddingVertical(3).Text("Descrição").Bold().FontSize(8.5f);
                            header.Cell().BorderBottom(1).BorderColor("#0f172a").PaddingVertical(3).AlignCenter().Text("Qtd").Bold().FontSize(8.5f);
                            header.Cell().BorderBottom(1).BorderColor("#0f172a").PaddingVertical(3).AlignRight().Text("Unitário").Bold().FontSize(8.5f);
                            header.Cell().BorderBottom(1).BorderColor("#0f172a").PaddingVertical(3).AlignRight().Text("Total").Bold().FontSize(8.5f);
                        });

                        foreach (var p in order.Parts)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(4).Text(p.Code ?? "-").FontSize(8.5f);
                            table.Cell().BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(4).Text(p.Description).FontSize(9);
                            table.Cell().BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(4).AlignCenter().Text(p.Quantity.ToString("G29")).FontSize(9);
                            table.Cell().BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(4).AlignRight().Text(p.UnitPrice.ToString("C2")).FontSize(9);
                            table.Cell().BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(4).AlignRight().Text((p.Quantity * p.UnitPrice).ToString("C2")).Bold().FontSize(9);
                        }
                    });

                    col.Item().PaddingTop(4).AlignRight().Text($"Subtotal Peças: {partsTotal:C2}").Bold().FontSize(9);
                });

                column.Item().PaddingVertical(8).LineHorizontal(1.5f).LineColor("#0f172a");
            }

            // BLOCO 5: FECHAMENTO / TOTAIS
            var grandTotal = servicesTotal + partsTotal;
            column.Item().Row(row =>
            {
                row.RelativeItem(3).Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(order.Notes))
                    {
                        col.Item().Text("OBSERVAÇÕES:").FontSize(8).Bold().FontColor("#64748b");
                        col.Item().Text(order.Notes).FontSize(8.5f);
                    }
                });

                row.RelativeItem(2).AlignRight().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Mão de Obra:").FontSize(9);
                        r.RelativeItem().AlignRight().Text(servicesTotal.ToString("C2")).FontSize(9);
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Peças & Insumos:").FontSize(9);
                        r.RelativeItem().AlignRight().Text(partsTotal.ToString("C2")).FontSize(9);
                    });
                    col.Item().PaddingVertical(3).LineHorizontal(1).LineColor("#0f172a");
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL GERAL:").Bold().FontSize(12);
                        r.RelativeItem().AlignRight().Text(grandTotal.ToString("C2")).Bold().FontSize(12);
                    });
                    col.Item().PaddingTop(2).LineHorizontal(1.5f).LineColor("#0f172a");
                });
            });

            // BLOCO 6: TERMO DE GARANTIA E ASSINATURAS
            column.Item().PaddingTop(25).Column(col =>
            {
                var terms = company.WarrantyTerms ?? "Garantia legal de 90 dias conforme artigo 26 do Código de Defesa do Consumidor.";
                col.Item().Text(terms).FontSize(7.5f).FontColor("#475569").Justify();

                col.Item().PaddingTop(35).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().LineHorizontal(1).LineColor("#0f172a");
                        c.Item().PaddingTop(3).AlignCenter().Text(company.Name).FontSize(8.5f).Bold();
                        c.Item().AlignCenter().Text("Responsável Técnico").FontSize(7.5f).FontColor("#64748b");
                    });

                    row.ConstantItem(40);

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().LineHorizontal(1).LineColor("#0f172a");
                        c.Item().PaddingTop(3).AlignCenter().Text(order.CustomerName).FontSize(8.5f).Bold();
                        c.Item().AlignCenter().Text("Assinatura do Cliente / Responsável").FontSize(7.5f).FontColor("#64748b");
                    });
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor("#cbd5e1");
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Text("Ofizzy · Sistema de Gestão de Oficinas").FontSize(7.5f).FontColor("#94a3b8");
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Página ").FontSize(7.5f).FontColor("#94a3b8");
                    text.CurrentPageNumber().FontSize(7.5f).FontColor("#94a3b8");
                    text.Span(" de ").FontSize(7.5f).FontColor("#94a3b8");
                    text.TotalPages().FontSize(7.5f).FontColor("#94a3b8");
                });
            });
        });
    }

    private static string StatusLabel(WorkOrderStatus status) => status switch
    {
        WorkOrderStatus.Open => "Aberta",
        WorkOrderStatus.InProgress => "Em Andamento",
        WorkOrderStatus.Completed => "Finalizada",
        WorkOrderStatus.Cancelled => "Cancelada",
        _ => status.ToString()
    };
}
