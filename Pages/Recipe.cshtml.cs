using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BakingBuddy.Pages
{
    public class RecipeModel : PageModel
    {

        private readonly BakingBuddy.Data.BakingBuddyContext _context;

        private List<Models.Notes> _allRecipeNotes;

        public RecipeModel(BakingBuddy.Data.BakingBuddyContext context)
        {
            _context = context;
        }
        public bool OnMenu { get; private set; }
        public string RecipeName { get; private set; }
        public string ActiveMin { get; private set; }
        public string InactiveMin { get; private set; }
        public Uri SourceUri { get; private set; }
        public string SourceString { get; private set; }
        public HashSet<string> Images { get; private set; } = new HashSet<string>();
        public List<ListWithHeaders> IngredientGroups { get; private set; } = new List<ListWithHeaders>();
        public List<ListWithHeaders> DirectionGroups { get; private set; } = new List<ListWithHeaders>();
        public List<Models.Notes> RecipeNotes { get; private set; } = new List<Models.Notes>();

        [BindProperty]
        public IFormFile Upload { get; set; }

        // Given a string (containing no spaces) 
        // Convert it to decimal value(s).
        //    '.12' converts to (0.12,0)
        //    '1/2' converts to (0.5,0)
        //    '1-2' converts to (1,2)
        //    'anything_else' converts to (0,0)
        private (decimal, decimal) ConvertToDecimals(string value)
        {
            decimal extra = 0;
            try
            {
                decimal doub = 0;
                decimal.TryParse(value, out doub);
                if (value.Contains("/"))
                {
                    var fraction = value.Split("/");
                    doub = decimal.Parse(fraction[0]) / decimal.Parse(fraction[1]);

                }
                else if (value.Contains("-"))
                {
                    var range = value.Split("-");
                    decimal ignore;
                    (doub, ignore) = ConvertToDecimals(range[0]);
                    (extra, ignore) = ConvertToDecimals(range[1]);
                }
                return (doub, extra);
            }
            catch (FormatException)
            {
                return (0,0);
            }

        }

        // Given a line in the ingredients, convert the volume to weight via the conversionList
        private string GetWeighLine(string line, List<(string, decimal)> conversionList)
        {
            Regex startsWithDigit = new Regex(@"^\d");
            // all valid volume values
            Regex amount = new Regex(@"^(\d+\/\d+|\d+\.\d+|\d+)-(\d+\/\d+|\d+\.\d+|\d+)|\d+\/\d+|\d+\.\d+|to|\d+$");
            Regex cup = new Regex(@"^cups?$", RegexOptions.IgnoreCase);
            Regex tbsp = new Regex(@"^((tablespoon)|(tbsp))s?$", RegexOptions.IgnoreCase);
            Regex tsp = new Regex(@"^((teaspoon)|(tsp))s?$", RegexOptions.IgnoreCase);

            // Separate by the first , that's additional directions
            var index = line.IndexOf(",");
            var start = index == -1 ? line.Substring(0, line.Length) : line.Substring(0, index);
            var end = index == -1 ? "" : line.Substring(index, line.Length - index);
            
            // Line must begin with a digit
            if (!startsWithDigit.IsMatch(start)) return "";
            
            // Consider the values individually
            var parts = start.Split(" ");
            int p = 0;
            // We could be dealing with a range
            decimal firstValue = 0, secondValue = 0;
            bool readingFirstValue = true;
            while (amount.IsMatch(parts[p]))
            {
                if (parts[p] == "to" | secondValue > 0)
                {
                    readingFirstValue = false;
                }
                
                // ConvertToDecimals always returns two values
                // If the string did not contain a range, value two is zero
                var add = ConvertToDecimals(parts[p]);
                firstValue += readingFirstValue ? add.Item1 : 0;
                secondValue += readingFirstValue ? add.Item2 : add.Item1;
                ++p;
            }

            // Ingredient amount must be of a valid form
            if (firstValue == 0) return "";

            // How many teaspoons in the unit
            var teaspoons = 0;
            if (cup.IsMatch(parts[p]))
            {
                teaspoons = 48;
            }
            else if (tbsp.IsMatch(parts[p]))
            {
                teaspoons = 3;
            }
            else if (tsp.IsMatch(parts[p]))
            {
                teaspoons = 1;
            }

            // Ingredient unit must be either cup, tbsp, or tsp
            if (teaspoons == 0) return "";

            firstValue *= teaspoons;
            secondValue *= teaspoons;

            // Stitch the rest of the line together
            // It describes the ingredient type
            var foodType = "";
            for (var t = p + 1; t < parts.Length; ++t)
            {
                foodType += parts[t] + " ";
            }
            // If a conversion food is found within the food type
            // Consider it a match and convert
            var inConversion = false;
            foreach (var tup in conversionList)
            {
                if (foodType.Contains(tup.Item1, StringComparison.OrdinalIgnoreCase))
                {
                    firstValue *= tup.Item2;
                    secondValue *= tup.Item2;
                    inConversion = true;
                    break;
                }
            }

            // Ingredient must be listed in the conversion chart
            if (!inConversion) return "";

            // Stitch back to a raw html line
            // Bold any changes
            if (secondValue == 0)
            {
                return $"<b>{string.Format("{0:0.##}", firstValue)} g </b>{foodType}{end}";

            }
            return $"<b>{string.Format("{0:0.##}", firstValue)} to {string.Format("{0:0.##}", secondValue)} g </b>{foodType}{end}";

        }

        public async Task OnGetAsync(string recipeName)
        {
            RecipeName = recipeName;

            var menu = Common.ReadFile(Common.planFileLocation);
            var plans = new HashSet<string>(menu, StringComparer.OrdinalIgnoreCase);
            OnMenu = plans.Contains(recipeName);

            var lines = Common.ReadFile($"{Common.recipeLocation}{recipeName}.md");

            if (lines.Count == 0)
            {
                Response.Redirect($"/Create/{recipeName}");
            }
            else
            {
                ActiveMin = lines.ElementAt(1).Substring(Common.activeMinFormat.Length);

                InactiveMin = lines.ElementAt(2).Substring(Common.inactiveMinFormat.Length);
                if (lines.ElementAt(3).Contains(Common.sourceUriFormat))
                {
                    var url = lines.ElementAt(3).Substring(Common.sourceUriFormat.Length).TrimEnd(')');
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        SourceUri = new Uri(url);
                    }
                }
                else
                {
                    SourceString = lines.ElementAt(3).Substring(Common.sourceStringFormat.Length);
                }

                var ingHeaderLoc = lines.IndexOf(Common.ingredientHeader);
                var dirHeaderLoc = lines.IndexOf(Common.directionsHeader);

                var ingredients = lines.Skip(ingHeaderLoc + 1).Take(dirHeaderLoc - ingHeaderLoc - 1).ToList();
                IngredientGroups = Common.GetIngredientOrDirectionGroups(ingredients, @"^\* ");

                var directions = lines.Skip(dirHeaderLoc + 1).ToList();
                DirectionGroups = Common.GetIngredientOrDirectionGroups(directions, @"^\d+\. ");

                var notes = from m in _context.Notes
                            where (m.RecipeName == recipeName)
                            select m;

                notes = notes.OrderByDescending(s => s.Date);

                _allRecipeNotes = await notes.ToListAsync();

                foreach (var note in _allRecipeNotes)
                {
                    // Only add a note if it has an image or text
                    if (!string.IsNullOrWhiteSpace(note.ImageName))
                    {
                        Images.Add(note.ImageName);
                        RecipeNotes.Add(note);
                    }
                    else if (!string.IsNullOrWhiteSpace(note.Note))
                    {
                        RecipeNotes.Add(note);
                    }
                }
            }
        }

        public async Task OnPostAsync(string recipeName, DateTime date, string notes)
        {
            var imageName = $"{date:yyMMdd} {recipeName}.jpg";
            var file = $"wwwroot/assets/uploads/{imageName}";

            if (Upload?.Length > 0 && Upload.ContentType.Contains("image"))
            {
                using var fileStream = new FileStream(file, FileMode.Create);
                await Upload.CopyToAsync(fileStream);
            }
            else
            {
                imageName = null;
            }

            Models.Notes existingNote = null;
            try
            {
                existingNote = _context.Notes.Single(e => e.RecipeName == recipeName && e.Date == date);
                existingNote.Note = notes;
                existingNote.ImageName = imageName;
                _context.Attach(existingNote).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch
            {
                _context.Notes.Add(new Models.Notes
                {
                    RecipeName = recipeName,
                    Note = notes,
                    Date = date,
                    ImageName = imageName
                });
                _context.SaveChanges();
            }

            await OnGetAsync(recipeName);
        }

        public async Task OnPostMenu(string recipeName)
        {
            var lines = Common.ReadFile(Common.planFileLocation);
            var plans = new HashSet<string>(lines, StringComparer.OrdinalIgnoreCase);
            if (!plans.Add(recipeName))
            {
                plans.Remove(recipeName);
                
            }
            Common.WriteFile(Common.planFileLocation, plans.ToList());

            await OnGetAsync(recipeName);
        }
        
        public async Task OnPostWeigh(string recipeName)
        {
            await OnGetAsync(recipeName);

            var lines = Common.ReadFile(Common.conversionFileLocation);
            List<(string, decimal)> conversionList = new List<(string, decimal)>();
            foreach (var line in lines)
            {
                string[] values = line.Split(',');
                var ingredient = values[0];
                var unit = values[1];
                var weight = values[2];

                decimal conversion = 0;
                switch (unit)
                {
                    case "Cup":
                        conversion = decimal.Parse(weight) / 48;
                        break;
                    case "Tbsp":
                        conversion = decimal.Parse(weight) / 3;
                        break;
                    case "Tsp":
                        conversion = decimal.Parse(weight);
                        break;
                }
                conversionList.Add((ingredient, conversion));
            }

            for (var g = 0; g < IngredientGroups.Count; ++g)
            {
                for (var i = 0; i < IngredientGroups[g].list.Count; ++i)
                {
                    var newLine = GetWeighLine(IngredientGroups[g].list[i], conversionList);
                    if(newLine != "")
                    {
                        IngredientGroups[g].list[i] = newLine;
                    }
                }
            }
        }
    }
}
