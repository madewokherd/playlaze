using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace playlaze
{
    public abstract class PlaylistItem
    {
        public CollectionItem Parent { get; internal set; }

        public CollectionItem Root
        {
            get
            {
                PlaylistItem ancestor = this;
                while (ancestor.Parent != null)
                    ancestor = ancestor.Parent;
                return ancestor as CollectionItem;
            }
        }

        protected internal void InternalDoAction(PlaylistAction action)
        {
            Playlist playlist = Root as Playlist;

            if (playlist == null)
                action.Do();
            else
                playlist.DoAction(action);
        }

        public abstract string HumanReadableDescription();

        public virtual DataObject GetDataObject()
        {
            DataObject result = new DataObject();
            result.SetData(typeof(PlaylistItem), this);
            return result;
        }

        public static PlaylistItem FromFilename(string filename)
        {
            // FIXME: Detect other sensible formats
            return new MediaItem(filename);
        }
    }
}
