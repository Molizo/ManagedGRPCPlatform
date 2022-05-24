using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Server.Data;
using Server.Models;

namespace Server.Pages.Clients
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly Server.Data.ApplicationDbContext _context;

        public CreateModel(Server.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Client Client { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            Client.RegisteredBy = User.Identity.Name;
            Client.ClientToServerPublicKey = "";
            Client.ClientToServerPrivateKey = "";
            Client.LastActivityAt = DateTime.MinValue;
            _context.Client.Add(Client);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}