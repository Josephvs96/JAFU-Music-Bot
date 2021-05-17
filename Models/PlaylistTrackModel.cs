using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_C_.Models
{
    public class PlaylistTrackModel
    {
        [Key]
        public int TrackID { get; set; }
        public string TrackName { get; set; }
        public string TrackURL { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedBy { get; set; }
        public bool IsPlayed { get; set; }
    }
}
