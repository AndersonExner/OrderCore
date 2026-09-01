using Microsoft.AspNetCore.Authorization;
using OrderCore.Application.Auth;

namespace OrderCore.Api.Security;

public static class AuthPolicies
{
    public const string ManageCustomers = "ManageCustomers";
    public const string ManageProducts = "ManageProducts";
    public const string CreateOrders = "CreateOrders";
    public const string ProcessOrders = "ProcessOrders";

    public static void AddOrderCorePolicies(AuthorizationOptions options)
    {
        options.AddPolicy(ManageCustomers, policy =>
            policy.RequireRole(AuthRoles.Admin, AuthRoles.Sales));

        options.AddPolicy(ManageProducts, policy =>
            policy.RequireRole(AuthRoles.Admin));

        options.AddPolicy(CreateOrders, policy =>
            policy.RequireRole(AuthRoles.Admin, AuthRoles.Sales));

        options.AddPolicy(ProcessOrders, policy =>
            policy.RequireRole(AuthRoles.Admin, AuthRoles.Finance));
    }
}
