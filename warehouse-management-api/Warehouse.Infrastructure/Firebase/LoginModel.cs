using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Warehouse.Infrastructure.Firebase;


public class LoginModel(IFirebaseAuthService authService) : PageModel
{

    [BindProperty]
    public UserDto UserDto { get; set; } = new UserDto
    {
        Email = null,
        Password = null
    };

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var token = await authService.Login(UserDto.Email, UserDto.Password);

        if (token is null) 
        {
 
            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            return Page();
        }
        
        HttpContext.Session.SetString("token", token);
        
 
        return RedirectToPage("/Authenticated");
    }
}