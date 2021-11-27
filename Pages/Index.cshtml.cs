using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BakingBuddy.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(){}

        public List<string> Recipes { get; private set; }

        [Required]
        public double Weight { get; set; }
        [Required]
        public string Ingredient { get; set; }
        [Required]
        public string Unit { get; set; }

        public Dictionary<string, Conversion> Conversions { get; private set; } = new Dictionary<string, Conversion>();

        private void GetConversions()
        {
            var lines = Common.ReadFile(Common.conversionFileLocation);
            foreach (var line in lines)
            {
                var entry = Common.GetConversionDictionaryEntry(line);
                Conversions.Add(entry.Item1, entry.Item2);
            }
        }

        private void UpdateRecipes()
        {
            Recipes = Directory.GetFiles(Common.recipeLocation, "*.md", SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToList();
        }

        public void OnGet(string search, string ingredient, string unit, double weight)
        {
            UpdateRecipes();
            GetConversions();

            Recipes = Recipes.Where(i => i.Contains(search?.Trim() ?? "", StringComparison.OrdinalIgnoreCase)).ToList();
            
            // If values are valid
            if (ingredient != null && weight > 0)
            {
                // Sentence case
                var formattedIng = ingredient.Trim().ToLower();
                formattedIng = formattedIng[0].ToString().ToUpper() + formattedIng.Substring(1);
                // Add to file if new
                if (!Conversions.ContainsKey(formattedIng))
                {
                    Conversions.Add(formattedIng, new Conversion(unit, weight.ToString()));
                    Common.WriteLine(Common.conversionFileLocation, $"{formattedIng},{unit},{weight}");
                }
                // Edit entry if old
                else
                {
                    Conversions[formattedIng] = new Conversion(unit, weight.ToString());
                    Common.WriteFile(Common.conversionFileLocation, Common.ConversionDictionaryToList(Conversions));
                }
            }
        }

        public IActionResult OnPost(string search, string ingredient, string unit, double weight)
        {
            // Post - Redirect - Get
            return RedirectToPage("", new { search, ingredient, unit, weight });
        }
    }
}
