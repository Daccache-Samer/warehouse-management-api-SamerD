using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using warehouse_management_api.Contracts;
using Warehouse.Infrastructure.Firebase;

namespace warehouse_management_api.Pages;

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

        if (UserDto.Email == null || UserDto.Password == null) return RedirectToPage("/Authenticated");
        var token = await authService.SignUp(UserDto.Email, UserDto.Password);

        if (token is null) return BadRequest();
        HttpContext.Session.SetString("token", token);

        return RedirectToPage("/Authenticated"); 
    }
}