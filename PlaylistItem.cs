using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace playlaze
{
    public abstract class PlaylistItem
    {
        public CollectionItem Parent { get; internal set; }

        protected internal void InternalDoAction(PlaylistAction action)
        {
            PlaylistItem item = this;
            while (item.Parent != null)
            {
                item = item.Parent;
            }

            Playlist playlist = (item as Playlist);

            if (playlist == null)
                action.Do();
            else
                playlist.DoAction(action);
        }

        public abstract string HumanReadableDescription();
    }
}
