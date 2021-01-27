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
    public class DetailsModel : PageModel
    {
        private readonly BakingBuddy.Data.BakingBuddyContext _context;

        public DetailsModel(BakingBuddy.Data.BakingBuddyContext context)
        {
            _context = context;
        }

        public Notes Notes { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Notes = await _context.Notes.FirstOrDefaultAsync(m => m.ID == id);

            if (Notes == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
