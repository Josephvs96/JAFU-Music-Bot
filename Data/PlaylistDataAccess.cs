using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Microsoft.EntityFrameworkCore;
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
        private readonly BotDbContext _db;

        public PlaylistDataAccess(BotDbContext db)
        {
            _db = db;
        }

        public async Task<bool> AddTrack(PlaylistTrackModel track)
        {
            await _db.AddAsync(track);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveTrack(PlaylistTrackModel track)
        {
            var dbTrack = _db.Playlist.Where(x => x.TrackName == track.TrackName).FirstOrDefault();
            if (dbTrack is not null)
            {
                _db.Playlist.Remove(dbTrack);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateToPlayedStatus(PlaylistTrackModel track)
        {
            var dbTrack = _db.Playlist.Where(x => x.TrackName == track.TrackName).FirstOrDefault();
            if (dbTrack is not null)
            {
                dbTrack.IsPlayed = true;
                _db.Update(dbTrack);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateToNotPlayedStatus(PlaylistTrackModel track)
        {
            var dbTrack = _db.Playlist.Where(x => x.TrackName == track.TrackName).FirstOrDefault();
            if (dbTrack is not null)
            {
                dbTrack.IsPlayed = false;
                _db.Update(dbTrack);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<PlaylistTrackModel>> GetAllPlaylistTracks()
        {
            return await _db.Playlist.ToListAsync();
        }
    }
}
