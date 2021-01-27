using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BakingBuddy.Pages
{
    public class CalendarModel : PageModel
    {
        public CalendarModel(BakingBuddy.Data.BakingBuddyContext context)
        {
            _context = context;
        }

        private readonly BakingBuddy.Data.BakingBuddyContext _context;


        public HashSet<string> Plans { get; private set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public IList<Models.Notes> Notes { get; private set; }

        private void GetMenu()
        {
            var lines = Common.ReadFile(Common.planFileLocation);
            Plans = new HashSet<string>(lines, StringComparer.OrdinalIgnoreCase);
        }

        private async Task GetNotes()
        {
            var notes = from m in _context.Notes
                        select m;
            notes = notes.OrderByDescending(s => s.Date);
            Notes = await notes.ToListAsync();
        }

        public async Task OnGetAsync()
        {
            GetMenu();
            await GetNotes();
        }

        public IActionResult OnPostAdd(string newToDo)
        {
            if (newToDo != null)
            {
                GetMenu();
                if (Plans.Add(newToDo))
                {
                    Common.WriteLine(Common.planFileLocation, newToDo);
                }
            }
            return RedirectToPage("/Calendar");
        }

        public IActionResult OnPostDelete(string toMake)
        {
            GetMenu();
            if (Plans.Remove(toMake))
            {
                Common.WriteFile(Common.planFileLocation, Plans.ToList());
            }
            return RedirectToPage("/Calendar");
        }
    }
}
