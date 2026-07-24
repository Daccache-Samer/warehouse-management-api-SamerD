using FirebaseAdmin.Auth;

namespace Warehouse.Infrastructure.Firebase;

public interface IFirebaseUserService
{
    Task SetAdminRoleAsync(string uid);
}

public class FirebaseUserService : IFirebaseUserService
{
    public async Task SetAdminRoleAsync(string uid)
    {
        var claims = new Dictionary<string, object> { { "role", "admin" } };
        await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(uid, claims);
    }
}