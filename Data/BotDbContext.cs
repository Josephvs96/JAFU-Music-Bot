using Microsoft.EntityFrameworkCore;
using Music_C_.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_C_.Data
{
    public class BotDbContext : DbContext
    {
        public DbSet<PlaylistTrackModel> Playlist { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite(@"Data Source=./playlist.db");
    }
}
