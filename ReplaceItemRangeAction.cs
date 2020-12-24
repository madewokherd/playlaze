using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace playlaze
{
    public class ReplaceItemRangeAction : PlaylistAction
    {
        public CollectionItem Parent { get; }
        
        public int StartingIndex { get; }

        public IReadOnlyList<PlaylistItem> OldItems { get; }

        public IReadOnlyList<PlaylistItem> NewItems { get; }

        internal ReplaceItemRangeAction(CollectionItem parent, int startingIndex,
            IReadOnlyList<PlaylistItem> oldItems, IReadOnlyList<PlaylistItem> newItems)
        {
            Parent = parent;
            StartingIndex = startingIndex;
            OldItems = oldItems;
            NewItems = newItems;
        }

        protected internal override void Do()
        {
            Parent.InternalReplaceItemRange(StartingIndex, OldItems.Count, NewItems);
        }
    }
}
