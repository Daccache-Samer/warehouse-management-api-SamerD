using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Warehouse.Infrastructure.Firebase;

public class SignUpModel(IFirebaseAuthService authService) : PageModel
{
    [BindProperty]
    public SignUpUserDto UserDto { get; set; } = new SignUpUserDto
    {
        Email = null,
        Password = null,
        ConfirmPassword = null
    };

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var token = await authService.SignUp(UserDto.Email, UserDto.Password);

        if (token is null) return BadRequest();
        HttpContext.Session.SetString("token", token);
  
        return RedirectToPage("/Authenticated"); 
    }
}