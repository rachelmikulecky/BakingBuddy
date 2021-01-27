using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BakingBuddy.Models;

namespace BakingBuddy.Data
{
    public class BakingBuddyContext : DbContext
    {
        public BakingBuddyContext (DbContextOptions<BakingBuddyContext> options)
            : base(options)
        {
        }

        public DbSet<BakingBuddy.Models.Notes> Notes { get; set; }
    }
}
