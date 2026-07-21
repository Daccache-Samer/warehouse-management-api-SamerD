using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Warehouse.Infrastructure.Firebase;

public class LogoutModel : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        HttpContext.Session.Remove("token");
        HttpContext.Session.Clear();
        return RedirectToPage("/Unauthenticated"); 
    }

}