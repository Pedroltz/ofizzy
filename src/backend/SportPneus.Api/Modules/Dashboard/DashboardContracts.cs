using SportPneus.Api.Modules.WorkOrders;

namespace SportPneus.Api.Modules.Dashboard;

public sealed record DashboardSummaryResponse(
    int TotalCustomers,
    int TotalVehicles,
    int TotalActiveOrders,
    int TotalCompletedOrders,
    IReadOnlyList<WorkOrderSummaryResponse> ActiveOrders
);
