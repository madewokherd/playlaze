using System;
using System.Collections.Generic;
using System.IO;

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

        public List<string> GetSupportedSaveFormats()
        {
            var result = new List<string>();

            result.Add("PLZ");

            return result;
        }

        public static string[] GetExtensionsForFormat(string format)
        {
            switch (format)
            {
                case "PLZ":
                    return new string[] { "plz" };
                default:
                    throw new ArgumentException("unknown format id", "format");
            }
        }

        public static string GetDescriptionForFormat(string format)
        {
            switch (format)
            {
                case "PLZ":
                    return "Playlaze list";
                default:
                    throw new ArgumentException("unknown format id", "format");
            }
        }

        public void SavePlz(string filename)
        {
            // TODO: Store the filename and use it to convert any relative paths

            using (var f = File.Open(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                using (var s = new StreamWriter(f))
                {
                    foreach (var item in this)
                    {
                        if (item is CollectionItem)
                        {
                            throw new NotImplementedException("Writing collections not implemented");
                        }

                        s.WriteLine(item.ToUrlString());
                    }
                }
            }
        }

        public void Save(string filename)
        {
            // TODO: Decide the format by checking the extension

            SavePlz(filename);
        }

        public static Playlist Open(string filename)
        {
            using (var f = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                // assume PLZ format
                using (var s = new StreamReader(f))
                {
                    var items = new List<PlaylistItem>();
                    string line;
                    while ((line = s.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var item = PlaylistItem.FromUrlString(line);
                        items.Add(item);
                    }

                    return new Playlist(items);
                }
            }
        }

        public override string ToUrlString()
        {
            throw new NotImplementedException();
        }
    }
}
