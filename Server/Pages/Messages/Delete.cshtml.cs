using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Pages.Messages
{
    public class DeleteModel : PageModel
    {
        private readonly Server.Data.ApplicationDbContext _context;

        public DeleteModel(Server.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
      public Message Message { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null || _context.Message == null)
            {
                return NotFound();
            }

            var message = await _context.Message.FirstOrDefaultAsync(m => m.ID == id);

            if (message == null)
            {
                return NotFound();
            }
            else 
            {
                Message = message;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null || _context.Message == null)
            {
                return NotFound();
            }
            var message = await _context.Message.FindAsync(id);

            if (message != null)
            {
                Message = message;
                _context.Message.Remove(Message);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
