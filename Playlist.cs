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
        Stack<PlaylistAction> redoHistory = new Stack<PlaylistAction>();

        public event PlaylistActionHandler ActionDone;
        public event PlaylistActionHandler ActionUndone;
        public event PlaylistActionHandler ActionRedone;
        public void DoAction(PlaylistAction action)
        {
            action.Do();
            undoHistory.Push(action);
            redoHistory.Clear();
            ActionDone?.Invoke(this, action);
        }

        public void Undo()
        {
            if (undoHistory.Count == 0)
                return;

            var action = undoHistory.Peek();
            action.Undo();
            undoHistory.Pop();
            redoHistory.Push(action);
            ActionUndone?.Invoke(this, action);
        }

        public void Redo()
        {
            if (redoHistory.Count == 0)
                return;

            var action = redoHistory.Peek();
            action.Do();
            redoHistory.Pop();
            undoHistory.Push(action);
            ActionRedone?.Invoke(this, action);
        }

        public bool CanUndo => undoHistory.Count != 0;
        public bool CanRedo => redoHistory.Count != 0;

        public override string HumanReadableDescription()
        {
            throw new NotSupportedException();
        }
    }
}
