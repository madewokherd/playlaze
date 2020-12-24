using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace playlaze
{
    public delegate void PlaylistActionHandler(object sender, PlaylistAction action);

    public class Playlist : CollectionItem
    {
        public Playlist() : base()
        {
        }

        public Playlist(IEnumerable<PlaylistItem> items) : base(items)
        {
        }

        Stack<PlaylistAction> undoHistory = new Stack<PlaylistAction>();

        public event PlaylistActionHandler ActionDone;
        public void DoAction(PlaylistAction action)
        {
            action.Do();
            undoHistory.Push(action);
            ActionDone?.Invoke(this, action);
        }

        public override string HumanReadableDescription()
        {
            throw new NotSupportedException();
        }
    }
}
