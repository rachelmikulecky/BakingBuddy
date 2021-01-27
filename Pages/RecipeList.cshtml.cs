using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BakingBuddy.Pages
{
    public class RecipeListModel : PageModel
    {
        public RecipeListModel()
        {
        }

        public List<string> Recipes { get; private set; }

        private void UpdateRecipes()
        {
            Recipes = Directory.GetFiles(Common.recipeLocation, "*.md", SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToList();
        }

        public IActionResult OnPost(string search)
        {
            return RedirectToPage("/RecipeList", new { search = search });
        }

        public void OnGet(string search)
        {
            UpdateRecipes();
            Recipes = Recipes.Where(i => i.Contains(search ?? "", StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}
