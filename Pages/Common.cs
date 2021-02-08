using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BakingBuddy.Pages
{
    public static class Common
    {
        public const string VERSION = "1.0.2";

        public const string activeMinFormat = @"active: ";
        public const string inactiveMinFormat = @"inactive: ";
        public const string sourceUriFormat = @"[source](";
        public const string sourceUriFormatEnd = @")";
        public const string sourceStringFormat = @"source: ";
        public const string ingredientHeader = @"## Ingredients";
        public const string directionsHeader = @"## Directions";

        public const string fileLocation = @"volume_data/";
        public const string recipeLocation = @"volume_data/recipes/";
        public const string conversionFileLocation = @"volume_data/conversions.md";
        public const string planFileLocation = @"volume_data/plan.md";

        // Given the lines of the ingredients/directions, populate IngredientGroups/DirectionGroups respectively
        public static List<ListWithHeaders> GetIngredientOrDirectionGroups(List<string> group, string startsWith)
        {
            List<ListWithHeaders> NewGroup = new List<ListWithHeaders>();
            int length = 0;
            List<string> subGroup = new List<string>();
            string header;
            // Iterate through the group
            while (length < group.Count)
            {
                header = null;
                // Get the next set of elements up to the next blank line
                subGroup = group.Skip(length).TakeWhile(var => !string.IsNullOrWhiteSpace(var)).ToList();
                if (subGroup.Any())
                {
                    // Headers end with a colon
                    if (subGroup.First().EndsWith(':'))
                    {
                        header = subGroup.First();
                        subGroup.Remove(header);
                        header += "\n";
                    }
                    // Remove the leading values
                    // They make the md prettier, but are not used
                    for (int i = 0; i < subGroup.Count(); i++)
                    {
                        subGroup[i] = Regex.Replace(subGroup[i], startsWith, "") + "\n";
                    }

                    NewGroup.Add(new ListWithHeaders(header, subGroup));
                }
                // Don't forget that blank line you skipped!
                length += subGroup.Count() + 1;

            }
            return NewGroup;
        }


        public static void DeleteFile(string fileName)
        {
            File.Delete(fileName);
        }

        public static List<string> ReadFile(string fileName)
        {
            List<string> fileContents = new List<string>();
            if (File.Exists(fileName))
            {
                using StreamReader sr = File.OpenText(fileName);
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    fileContents.Add(s);
                }
            }
            return fileContents;
        }

        public static void WriteLine(string fileName, string line)
        {
            if (!File.Exists(fileName))
            {
                using StreamWriter sw = File.CreateText(fileName);
                sw.WriteLine(line);
            }
            else
            {
                using StreamWriter sw = File.AppendText(fileName);
                sw.WriteLine(line);
            }
        }

        public static void WriteFile(string fileName, List<string> lines)
        {
            using StreamWriter sw = new StreamWriter(fileName, false);
            foreach (var line in lines)
            {
                sw.WriteLine(line);
            }
        }

        public static (string, Conversion) GetConversionDictionaryEntry(string line)
        {
            string[] values = line.Split(',');
            string ingredient = values[0];
            Conversion conversion = new Conversion(values[1], values[2]);
            return (ingredient, conversion);
        }

        public static List<string> ConversionDictionaryToList(Dictionary<string, Conversion> dict)
        {
            List<string> list = new List<string>();
            foreach (var entry in dict)
            {
                list.Add($"{entry.Key},{entry.Value.unit},{entry.Value.weight}");
            }
            return list;
        }
    }
    public class Conversion
    {
        public Conversion(string unit, string weight)
        {
            this.unit = unit;
            this.weight = weight;
        }
        public string unit;
        public string weight;
    }

    public class ListWithHeaders
    {
       
        public ListWithHeaders(string h, List<string> s)
        {
            header = h;
            list = s;
        }
        public string header;
        public List<string> list;
    }
}
