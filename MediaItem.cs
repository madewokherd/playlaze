using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace playlaze
{
    public class MediaItem : PlaylistItem
    {
        public MediaItem(string path_or_url)
        {
            PathOrUrl = path_or_url;
        }

        public string PathOrUrl { get; }
    }
}
