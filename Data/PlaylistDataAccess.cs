using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Music_C_.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_C_.Data
{
    public class PlaylistDataAccess
    {
        private readonly BotDbContext db;

        public List<PlaylistTrackModel> Playlist { get; set; } = new();

        public PlaylistDataAccess(BotDbContext db)
        {
            this.db = db;
            Playlist.AddRange(db.Playlist);
        }

        public async Task AddTrack(LavalinkTrack track, DiscordMember member)
        {
            PlaylistTrackModel trackModel = new()
            {
                TrackName = track.Title,
                AddedDate = DateTime.Now,
                IsPlayed = false,
                TrackURL = track.Uri.AbsoluteUri,
                AddedBy = member.DisplayName
            };

            await db.AddAsync(trackModel);
            await db.SaveChangesAsync();
            UpdatePlaylist();
        }

        public async Task RemoveTrack(LavalinkTrack track)
        {
            PlaylistTrackModel trackModel = Playlist.Where(x => x.TrackName == track.Title).FirstOrDefault();
            if (trackModel is not null)
            {
                db.Playlist.Remove(trackModel);
                await db.SaveChangesAsync();
                UpdatePlaylist();
            }
        }

        public void UpdatePlaylist()
        {
            Playlist.Clear();
            Playlist.AddRange(db.Playlist);
        }
    }
}
