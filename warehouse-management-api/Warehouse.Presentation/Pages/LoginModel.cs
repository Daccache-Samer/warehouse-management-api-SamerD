using System.Security.Claims;
using Firebase.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using warehouse_management_api.Contracts;

namespace warehouse_management_api.Pages;

public class LoginModel(FirebaseAuthClient firebaseAuth) : PageModel
{
    [BindProperty]
    public UserDto UserDto { get; set; } = new()
    {
        Email = null,
        Password = null
    };

    public async Task<IActionResult> OnPostAsync()
    {
        var userCredential = await firebaseAuth.SignInWithEmailAndPasswordAsync(UserDto.Email, UserDto.Password);
        var idToken = await userCredential.User.GetIdTokenAsync();

        var decoded = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, decoded.Uid) };
        if (decoded.Claims.TryGetValue("role", out var role))
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()!));
        if (decoded.Claims.TryGetValue("email", out var email))
            claims.Add(new Claim(ClaimTypes.Email, email.ToString()!));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return RedirectToPage("/Authenticated");
    }
}

