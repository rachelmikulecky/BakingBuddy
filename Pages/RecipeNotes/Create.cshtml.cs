using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using BakingBuddy.Data;
using BakingBuddy.Models;

namespace BakingBuddy.Pages.RecipeNotes
{
    public class CreateModel : PageModel
    {
        private readonly BakingBuddy.Data.BakingBuddyContext _context;

        public CreateModel(BakingBuddy.Data.BakingBuddyContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Notes Notes { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Notes.Add(Notes);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
