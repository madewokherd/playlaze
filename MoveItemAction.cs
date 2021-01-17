using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace playlaze
{
    public class MoveItemAction : PlaylistAction
    {
        public PlaylistItem Item { get; }
        public CollectionItem OldParent { get; }
        public CollectionItem NewParent { get; }
        public int OldIndex { get; }
        public int NewIndex { get; }

        public MoveItemAction(PlaylistItem item, CollectionItem new_parent, int new_index)
        {
            Item = item;
            OldParent = item.Parent;
            NewParent = new_parent;
            OldIndex = item.Parent.IndexOf(item);
            NewIndex = new_index;
        }

        protected internal override void Do()
        {
            OldParent.InternalReplaceItemRange(OldIndex, 1, new PlaylistItem[] { });
            NewParent.InternalReplaceItemRange(NewIndex, 0, new PlaylistItem[] { Item });
        }

        protected internal override void Undo()
        {
            NewParent.InternalReplaceItemRange(NewIndex, 1, new PlaylistItem[] { });
            OldParent.InternalReplaceItemRange(OldIndex, 0, new PlaylistItem[] { Item });
        }
    }
}
