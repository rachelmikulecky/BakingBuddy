using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BakingBuddy.Data;
using BakingBuddy.Models;

namespace BakingBuddy.Pages.RecipeNotes
{
    public class IndexModel : PageModel
    {
        private readonly BakingBuddy.Data.BakingBuddyContext _context;

        public IndexModel(BakingBuddy.Data.BakingBuddyContext context)
        {
            _context = context;
        }

        public IList<Notes> Notes { get;set; }

        public async Task OnGetAsync()
        {
            Notes = await _context.Notes.ToListAsync();
        }
    }
}
