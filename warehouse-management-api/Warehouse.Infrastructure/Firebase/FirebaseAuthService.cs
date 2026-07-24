using Firebase.Auth;

namespace Warehouse.Infrastructure.Firebase;

public class FirebaseAuthService(FirebaseAuthClient firebaseAuth) : IFirebaseAuthService
{
    private readonly FirebaseAuthClient _firebaseAuth = firebaseAuth;

    public async Task<string?> SignUp(string email, string password)
    {
        var userCredentials = await _firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password);

        return userCredentials is null ? null : await userCredentials.User.GetIdTokenAsync();
    }

    public async Task<string?> Login(string email, string password)
    {
        var userCredentials = await _firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);

        return userCredentials is null ? null : await userCredentials.User.GetIdTokenAsync();
    }
    
    public void SignOut() => _firebaseAuth.SignOut(); 
}

public interface IFirebaseAuthService
{
    Task<string?> SignUp(string email, string password);
    Task<string?> Login(string email, string password);
    void SignOut();
}