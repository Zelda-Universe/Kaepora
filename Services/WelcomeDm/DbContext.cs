using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kaepora
{
    public class WelcomeDbContext : DbContext
    {
        public DbSet<Segment> Segments { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ProfileSegment> Glue { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            var file = File.ReadAllLines("db.txt");
            builder.UseMySql($@"Server={file[0]};database={file[1]};uid={file[2]};pwd={file[3]};");
        }
    }
}