using Ofizzy.Api.Modules.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.WorkOrders;

namespace Ofizzy.Api.Modules.Dashboard;

[TenantAccess(Modules = new[] { ProductModule.Customers, ProductModule.WorkOrders, ProductModule.Automotive })]
[Authorize, ApiController, Route("api/dashboard")]
public sealed class DashboardController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary(CancellationToken ct)
    {
        var totalCustomers = await db.Customers.CountAsync(x => x.IsActive, ct);
        var totalVehicles = await db.Vehicles.CountAsync(x => x.IsActive, ct);
        var totalActiveOrders = await db.WorkOrders.CountAsync(x => x.Status == WorkOrderStatus.Open || x.Status == WorkOrderStatus.InProgress, ct);
        var totalCompletedOrders = await db.WorkOrders.CountAsync(x => x.Status == WorkOrderStatus.Completed, ct);

        var activeOrders = await db.WorkOrders
            .AsNoTracking()
            .Where(x => x.Status == WorkOrderStatus.Open || x.Status == WorkOrderStatus.InProgress)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new WorkOrderSummaryResponse(
                x.Id,
                x.Number,
                x.CustomerName,
                x.VehiclePlate,
                x.VehicleDescription,
                x.Status,
                x.Services.Sum(i => i.Quantity * i.UnitPrice) + x.Parts.Sum(i => i.Quantity * i.UnitPrice),
                x.CreatedAt
            ))
            .ToListAsync(ct);

        return Ok(new DashboardSummaryResponse(
            totalCustomers,
            totalVehicles,
            totalActiveOrders,
            totalCompletedOrders,
            activeOrders
        ));
    }
}
