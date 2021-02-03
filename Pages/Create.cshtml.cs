using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace BakingBuddy.Pages
{
    public class CreateModel : PageModel
    {

        public CreateModel(){}

        [Required]
        public string RecipeName { get; set; }
        public string ActiveMin { get; set; }
        public string InactiveMin { get; set; }
        public string Source { get; set; }
        public List<ListWithHeaders> IngredientGroups { get; set; } = new List<ListWithHeaders>();
        public List<ListWithHeaders> DirectionGroups { get; set; } = new List<ListWithHeaders>();

        public void OnGet(string recipeName)
        {
            RecipeName = recipeName;

            var lines = Common.ReadFile($"{Common.recipeLocation}{recipeName}.md");

            if (lines.Count > 0)
            {
                ActiveMin = lines.ElementAt(1).Substring(Common.activeMinFormat.Length);

                InactiveMin = lines.ElementAt(2).Substring(Common.inactiveMinFormat.Length);
                if (lines.ElementAt(3).Contains(Common.sourceUriFormat))
                {
                    Source = lines.ElementAt(3).Substring(Common.sourceUriFormat.Length).TrimEnd(')');
                }
                else
                {
                    Source = lines.ElementAt(3).Substring(Common.sourceStringFormat.Length);
                }


                var ingHeaderLoc = lines.IndexOf(Common.ingredientHeader);
                var dirHeaderLoc = lines.IndexOf(Common.directionsHeader);

                var ingredients = lines.Skip(ingHeaderLoc + 1).Take(dirHeaderLoc - ingHeaderLoc - 1).ToList();
                IngredientGroups = Common.GetIngredientOrDirectionGroups(ingredients, @"^\* ");
                var directions = lines.Skip(dirHeaderLoc + 1).ToList();
                DirectionGroups = Common.GetIngredientOrDirectionGroups(directions, @"^\d+\. ");

            }
            if (IngredientGroups.Count() == 0)
            {
                IngredientGroups = null;
            }
            if (DirectionGroups.Count() == 0)
            {
                DirectionGroups = null;
            }
        }
        public void OnPost(string recipeName, int active, int inactive, string source, string ingredients, string directions)
        {
            // Recipe Name
            List<string> file = new List<string>
            {
                $"# {recipeName}"
            };
            // Active Minutes
            string activeString = active > 0 ? active.ToString() : "";
            file.Add(Common.activeMinFormat + activeString);
            // Inactive Minutes
            string inactiveString = inactive > 0 ? inactive.ToString() : "";
            file.Add(Common.inactiveMinFormat + inactiveString);
            // Source
            Uri uriResult;
            bool sourceIsURI = Uri.TryCreate(source, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (sourceIsURI)
            {
                file.Add(Common.sourceUriFormat + source + Common.sourceUriFormatEnd);
            }
            else
            {
                file.Add(Common.sourceStringFormat + source);
            }
            // Ingredients
            file.Add(Common.ingredientHeader);
            if(ingredients != null)
            {
                foreach (var badLine in ingredients.Split('\n'))
                {
                    string line = badLine.Trim();
                    // No star needed if the line
                    // starts with a star or ends with a colon
                    List<string> patterns = new List<string>(){ @"^\*\s", @"\:\s*$" };
                    
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        Match m;
                        bool emptyStart = false;
                        foreach (var pattern in patterns)
                        {
                            m = Regex.Match(line, pattern);
                            if (m.Success)
                            {
                                emptyStart = true;
                                break;
                            }
                        }

                        string lineStart = emptyStart ? "" : "* ";
                        line = line.Replace("½", "1/2")
                                    .Replace("¼", "1/4")
                                    .Replace("¾", "3/4")
                                    .Replace("⅓", "1/3")
                                    .Replace("⅔", "2/3")
                                    .Replace("⅛", "1/8")
                                    .Replace("⅜", "3/8")
                                    .Replace("⅝", "5/8")
                                    .Replace("⅞", "7/8");
                        line = $"{lineStart}{line}";
                        m = Regex.Match(line, @"^\*");
                        // If the line doesn't have a star, have an empty line preceding
                        if (!m.Success)
                        {
                            file.Add("");
                        }
                        file.Add(line);
                    }
                }
            }
            // Directions
            file.Add(Common.directionsHeader);
            var i = 1;
            if (directions != null)
            {
                foreach (var badLine in directions.Split('\n'))
                {
                    string line = badLine.Trim();

                    // Starts with a number and dot or ends with a colon
                    List<string> ignorePatterns = new List<string>() { @"^\d+\s*\.", @"\:\s*$" };
                    // Only has the step number (1. or Step 1)
                    List<string> excludePatterns = new List<string>() { @"^\d+\s*\.$", @"^Step \d+\s*" };

                    Match m;
                    bool ignoreLine = false;
                    foreach (var pattern in excludePatterns)
                    {
                        m = Regex.Match(line, pattern);
                        if (m.Success)
                        {
                            ignoreLine = true;
                            break;
                        }
                    }

                    if (!ignoreLine && !string.IsNullOrWhiteSpace(line))
                    {
                        bool emptyStart = false;
                        foreach (var pattern in ignorePatterns)
                        {
                            m = Regex.Match(line, pattern);
                            if (m.Success)
                            {
                                emptyStart = true;
                                break;
                            }
                        }

                        var lineStart = emptyStart ? "" : $"{i}. ";
                        line = $"{lineStart}{line}";
                        m = Regex.Match(line, @"^\d");

                        // On a step line, increment
                        if (!emptyStart) i++;
                        // On a header line, reset numbers and a preceding empty line
                        if (!m.Success)
                        {
                            i = 1;
                            file.Add("");
                        }
                        file.Add(line);
                    }

                }
            }

            Common.WriteFile($"{Common.recipeLocation}{recipeName}.md", file);

            Response.Redirect($"/Recipe/{recipeName}");

        }
    }
}
