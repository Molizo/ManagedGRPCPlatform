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
    public class IndexModel : PageModel
    {
        private readonly Server.Data.ApplicationDbContext _context;

        public IndexModel(Server.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Message> Message { get; set; } = default!;

        public async Task OnGetAsync(string ClientID)
        {
            if (_context.Message != null)
            {
                if (ClientID == null)
                    Message = await _context.Message.ToListAsync();
                else
                    Message = _context.Client.Include(c => c.Messages).First(c => c.SerialNr == ClientID).Messages;
            }
        }
    }
}