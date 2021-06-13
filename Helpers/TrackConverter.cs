using DSharpPlus.Lavalink;
using Music_C_.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_C_.Helpers
{
    public class TrackConverter
    {
        public static PlaylistTrackModel ConvertToPlaylistTrack(LavalinkTrack track)
        {
            PlaylistTrackModel output = new PlaylistTrackModel()
            {
                IsPlayed = false,
                AddedDate = DateTime.Now,
                TrackName = track.Title,
                TrackURL = track.Uri.AbsoluteUri,
            };
            return output;
        }
    }
}
