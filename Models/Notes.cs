using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BakingBuddy.Models
{
    public class Notes
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string ImageName { get; set; }
        public string RecipeName { get; set; }
        public string Note { get; set; }
    }
}
