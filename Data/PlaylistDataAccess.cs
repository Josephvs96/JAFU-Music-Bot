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
            if (_db.Playlist.Contains(track))
            {
                _db.Playlist.Remove(track);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateToPlayedStatus(PlaylistTrackModel track)
        {
            if (_db.Playlist.Contains(track))
            {
                track.IsPlayed = true;
                _db.Update(track);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateToNotPlayedStatus(PlaylistTrackModel track)
        {
            if (_db.Playlist.Contains(track))
            {
                track.IsPlayed = false;
                _db.Update(track);
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
