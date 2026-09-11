using Ofizzy.Api.Modules.WorkOrders;

namespace Ofizzy.Api.Modules.Dashboard;

public sealed record DashboardSummaryResponse(
    int TotalCustomers,
    int TotalVehicles,
    int TotalActiveOrders,
    int TotalCompletedOrders,
    IReadOnlyList<WorkOrderSummaryResponse> ActiveOrders
);
