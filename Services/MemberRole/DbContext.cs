using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kaepora
{
    public class MemberTrackingContext : DbContext
    {
        public DbSet<TrackedMessage> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            var file = File.ReadAllLines("db.txt");
            builder.UseMySql($@"Server={file[0]};database={file[1]};uid={file[2]};pwd={file[3]};");
        }
    }
}