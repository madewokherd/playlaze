using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace playlaze
{
    public partial class MainForm : Form
    {
        Playlist _playlist;

        // Drag and drop state
        TreeNode placeholder_node;

        public Playlist Playlist
        {
            get => _playlist;
            set
            {
                if (_playlist != null)
                {
                    Playlist.ActionDone -= Playlist_ActionDone;
                    Playlist.ActionUndone -= Playlist_ActionUndone;
                    Playlist.ActionRedone -= Playlist_ActionDone;
                }
                _playlist = value;
                value.ActionDone += Playlist_ActionDone;
                value.ActionUndone += Playlist_ActionUndone;
                value.ActionRedone += Playlist_ActionDone;
                playlistView.BeginUpdate();
                playlistView.Nodes.Clear();
                AddNodes(playlistView.Nodes, 0, _playlist);
                playlistView.EndUpdate();
                UpdateUndoSensitivity();
            }
        }

        private void Playlist_ActionUndone(object sender, PlaylistAction action)
        {
            if (action is ReplaceItemRangeAction)
            {
                ReplaceItemRangeAction replace = (ReplaceItemRangeAction)action;
                playlistView.BeginUpdate();
                RemoveNodes(replace.Parent, replace.StartingIndex, replace.NewItems.Count);
                AddNodes(replace.Parent, replace.StartingIndex, replace.OldItems);
                playlistView.EndUpdate();
            }
            else if (action is MoveItemAction)
            {
                MoveItemAction move = (MoveItemAction)action;
                playlistView.BeginUpdate();
                RemoveNodes(move.NewParent, move.NewIndex, 1);
                AddNodes(move.OldParent, move.OldIndex, new PlaylistItem[] { move.Item });
                playlistView.EndUpdate();
            }
            else
                throw new NotImplementedException();
            UpdateUndoSensitivity();
        }

        private void AddNodes(TreeNodeCollection nodes, int index, IEnumerable<PlaylistItem> items)
        {
            foreach (PlaylistItem i in items)
            {
                nodes.Insert(index, i.HumanReadableDescription());
                if (i is CollectionItem)
                {
                    AddNodes(nodes[index].Nodes, 0, (CollectionItem)i);
                }
                index++;
            }
        }

        private void Playlist_ActionDone(object sender, PlaylistAction action)
        {
            if (action is ReplaceItemRangeAction)
            {
                ReplaceItemRangeAction replace = (ReplaceItemRangeAction)action;
                playlistView.BeginUpdate();
                RemoveNodes(replace.Parent, replace.StartingIndex, replace.OldItems.Count);
                AddNodes(replace.Parent, replace.StartingIndex, replace.NewItems);
                playlistView.EndUpdate();
            }
            else if (action is MoveItemAction)
            {
                MoveItemAction move = (MoveItemAction)action;
                playlistView.BeginUpdate();
                RemoveNodes(move.OldParent, move.OldIndex, 1);
                AddNodes(move.NewParent, move.NewIndex, new PlaylistItem[] { move.Item });
                playlistView.EndUpdate();
            }
            else
                throw new NotImplementedException();
            UpdateUndoSensitivity();
        }

        private void UpdateUndoSensitivity()
        {
            undoToolStripMenuItem.Enabled = Playlist.CanUndo;
            redoToolStripMenuItem.Enabled = Playlist.CanRedo;
        }

        private void AddNodes(CollectionItem parent, int index, IEnumerable<PlaylistItem> items)
        {
            if (items.Count() == 0)
                return;
            AddNodes(FindNodeCollection(parent), index, items);
        }

        private void RemoveNodes(CollectionItem parent, int startingIndex, int count)
        {
            if (count == 0)
                return;
            TreeNodeCollection nodes = FindNodeCollection(parent);
            for (int i = count - 1; i >= 0; i--)
            {
                nodes.RemoveAt(startingIndex + i);
            }
        }

        private TreeNodeCollection FindNodeCollection(CollectionItem parent)
        {
            if (parent == _playlist)
                return playlistView.Nodes;
            var parent_collection = FindNodeCollection(parent.Parent);
            int node_index, item_index = 0;
            for (node_index = 0; node_index < parent_collection.Count; node_index++)
            {
                if (parent_collection[node_index] == placeholder_node)
                    continue;
                item_index++;
                if (parent.Parent[item_index] == parent)
                    break;
            }
            return parent_collection[node_index].Nodes;
        }

        public MainForm()
        {
            InitializeComponent();
            Playlist = new Playlist();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aboutBox = new AboutBox();
            aboutBox.Show();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void playAMediaFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = true
            };
            if (ofd.ShowDialog(this) == DialogResult.Cancel)
                return;
            List<PlaylistItem> items = new List<PlaylistItem>(ofd.FileNames.Length);
            foreach(string name in ofd.FileNames)
            {
                items.Add(new MediaItem(name));
            }
            AddItems(items);
        }

        private void AddItems(List<PlaylistItem> items)
        {
            // FIXME: Insert at TreeView selection?
            Playlist.AddRange(items);
        }

        private PlaylistItem PlaylistItemForNode(TreeNode node)
        {
            CollectionItem parent_item;
            if (node == placeholder_node)
                return null;
            if (node.Parent is null)
            {
                parent_item = Playlist;
            }
            else
            {
                parent_item = (CollectionItem)PlaylistItemForNode(node.Parent);
            }
            if (placeholder_node != null && node.Parent == placeholder_node.Parent && node.Index > placeholder_node.Index)
                return parent_item[node.Index - 1];
            return parent_item[node.Index];
        }

        private void RemoveSelectedItem()
        {
            var node = playlistView.SelectedNode;
            if (node == null)
                return;

            var item = PlaylistItemForNode(node);

            item.Parent.RemoveAt(node.Index);
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            RemoveSelectedItem();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveSelectedItem();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Playlist.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Playlist.Redo();
        }

        private void addButton_CheckedChanged(object sender, EventArgs e)
        {
            if (addButton.Checked)
            {
                if (!addButtonMenu.Visible)
                    addButtonMenu.Show(addButton, new Point(0, addButton.Height));
            }
            else
            {
                if (addButtonMenu.Visible)
                    addButtonMenu.Hide();
            }
        }

        private void addButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (!addButtonMenu.Visible)
            {
                addButtonMenu.Show(addButton, new Point(0, addButton.Height));
                addButtonMenu.Capture = true;
            }
        }

        private void addButtonMenu_Opened(object sender, EventArgs e)
        {
            addButton.Checked = true;
        }

        private void addButtonMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            addButton.Checked = false;
        }

        private void playlistView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DoDragDrop(PlaylistItemForNode((TreeNode)e.Item).GetDataObject(),
                    DragDropEffects.Move|DragDropEffects.Copy);
            }
        }

        public static PlaylistItem[] ItemsFromDataObject(IDataObject data)
        {
            var item = (PlaylistItem)data.GetData(typeof(PlaylistItem));
            if (item != null)
                return new PlaylistItem[1] { item };
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                var hdrop = (string[])data.GetData(DataFormats.FileDrop);
                PlaylistItem[] result = new PlaylistItem[hdrop.Length];
                for (int i = 0; i < hdrop.Length; i++)
                {
                    result[i] = PlaylistItem.FromFilename(hdrop[i]);
                }
                return result;
            }
            return new PlaylistItem[0];
        }

        private void playlistView_DragEnter(object sender, DragEventArgs e)
        {
            var items = ItemsFromDataObject(e.Data);
            if (items.Length == 1)
            {
                var item = items[0];
                if (item.Root == Playlist && (e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                    e.Effect = DragDropEffects.Move;
                else if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                    e.Effect = DragDropEffects.Copy;
                return;
            }
            if (items.Length != 0)
                e.Effect = DragDropEffects.Copy;
        }

        private bool FindNodeAtPosition(int x, int y, out TreeNode parent_node, out int node_index)
        {
            parent_node = null;
            var parent_nodes = playlistView.Nodes;
            int min_index = 0;
            int max_index = parent_nodes.Count - 1;
            while (min_index <= max_index)
            {
                if (min_index == max_index)
                {
                    var node = parent_nodes[min_index];
                    var bounds = node.Bounds;
                    var item = PlaylistItemForNode(node);
                    if (item is CollectionItem)
                    {
                        if (y < bounds.Y + bounds.Height / 3)
                        {
                            // Insert before collection item
                            node_index = min_index;
                            return false;
                        }
                        if (y < bounds.Y + bounds.Height * 2 / 3)
                        {
                            // Insert into collection item
                            node_index = min_index;
                            return true;
                        }
                        if (node.IsExpanded)
                        {
                            // Recurse to find where to insert
                            parent_node = node;
                            parent_nodes = node.Nodes;
                            min_index = 0;
                            max_index = parent_nodes.Count - 1;
                            continue;
                        }
                        // Insert after collection item
                        node_index = min_index + 1;
                        return false;
                    }
                    else
                    {
                        if (y < bounds.Y + bounds.Height / 2)
                        {
                            // Insert before playlist item
                            node_index = min_index;
                            return false;
                        }
                        node_index = min_index + 1;
                        return false;
                    }
                }
                else
                {
                    // Bias towards checking higher nodes. We don't eliminate a node that
                    // is too far up, so we need this to prevent an infinite loop.
                    var mid_index = (min_index + max_index + 1) / 2;
                    var node = parent_nodes[mid_index];
                    var bounds = node.Bounds;
                    if (y < bounds.Y)
                    {
                        max_index = mid_index - 1;
                        continue;
                    }
                    if (y > bounds.Y + bounds.Height)
                    {
                        // Do not eliminate this node, it may be an expanded node we need
                        // to recurse into.
                        min_index = mid_index;
                        continue;
                    }
                    min_index = max_index = mid_index;
                    continue;
                }
            }
            node_index = min_index;
            return false;
        }

        private void playlistView_DragOver(object sender, DragEventArgs e)
        {
            TreeNode parent_node;
            int node_index;
            var corner = playlistView.PointToScreen(new Point(0, 0));
            bool collection = FindNodeAtPosition(e.X - corner.X, e.Y - corner.Y, out parent_node, out node_index);

            // FIXME: Check if this is a descendent of the node we're dragging.

            if (collection)
            {
                // FIXME: Select this node?
            }
            else if (e.Effect == DragDropEffects.Move)
            {
                AddPlaceholderNode(parent_node, node_index, "[move here]");
            }
            else
            {
                AddPlaceholderNode(parent_node, node_index, "[insert here]");
            }
        }

        private void AddPlaceholderNode(TreeNode parent_node, int node_index, string text)
        {
            if (placeholder_node != null &&
                placeholder_node.Parent == parent_node &&
                placeholder_node.Index == node_index)
                // Nothing to do.
                return;
            playlistView.BeginUpdate();
            ClearPlaceholderNode();
            TreeNodeCollection nodes;
            if (parent_node == null)
                nodes = playlistView.Nodes;
            else
                nodes = parent_node.Nodes;
            placeholder_node = nodes.Insert(node_index, text);
            playlistView.EndUpdate();
        }

        private void ClearPlaceholderNode()
        {
            if (placeholder_node != null)
            {
                var parent_node = placeholder_node.Parent;
                TreeNodeCollection nodes;
                if (parent_node == null)
                    nodes = playlistView.Nodes;
                else
                    nodes = parent_node.Nodes;
                nodes.RemoveAt(placeholder_node.Index);
                placeholder_node = null;
            }
        }

        private void playlistView_DragLeave(object sender, EventArgs e)
        {
            ClearPlaceholderNode();
        }

        private void playlistView_DragDrop(object sender, DragEventArgs e)
        {
            if (placeholder_node == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            var parent_node = placeholder_node.Parent;
            var index = placeholder_node.Index;
            CollectionItem parent_item;
            if (parent_node == null)
                parent_item = Playlist;
            else
                parent_item = (CollectionItem)PlaylistItemForNode(parent_node);
            ClearPlaceholderNode();
            var items = ItemsFromDataObject(e.Data);
            if (e.Effect == DragDropEffects.Move && items.Length == 1 && items[0].Parent != null)
            {
                var item = items[0];
                int old_index = item.Parent.IndexOf(item);
                if (parent_item == item.Parent && index > old_index)
                {
                    // Adjust the "new" index to account for removing this item
                    // from its old position.
                    index--;
                    if (index == old_index)
                    {
                        // Nothing to do.
                        return;
                    }
                }
                var action = new MoveItemAction(item, parent_item, index);
                Playlist.DoAction(action);

                return;
            }
            else if (items.Length != 0)
            {
                parent_item.InsertRange(index, items);
            }
        }

        private void SaveAs()
        {
            var dialog = new SaveFileDialog();

            dialog.AddExtension = true;
            dialog.AutoUpgradeEnabled = true;
            dialog.CheckPathExists = true;
            dialog.Filter = BuildFilter(Playlist.GetSupportedSaveFormats());
            dialog.FilterIndex = 2;
            dialog.Title = "Playlaze";
            dialog.OverwritePrompt = true;
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                Playlist.Save(dialog.FileName);
            }
        }

        private string BuildFilter(List<string> formats)
        {
            StringBuilder result = new StringBuilder();
            bool first_extension = true;

            result.Append("All supported formats|");
            foreach (string id in formats)
            {
                foreach (string extension in Playlist.GetExtensionsForFormat(id))
                {
                    if (!first_extension)
                        result.Append(";");
                    first_extension = false;

                    result.Append("*.");
                    result.Append(extension);
                }
            }

            foreach (string id in formats)
            {
                result.Append("|");

                result.Append(Playlist.GetDescriptionForFormat(id));
                result.Append(" (");

                first_extension = true;
                foreach (string extension in Playlist.GetExtensionsForFormat(id))
                {
                    if (!first_extension)
                        result.Append(";");
                    first_extension = false;

                    result.Append("*.");
                    result.Append(extension);
                }

                result.Append(")|");

                first_extension = true;
                foreach (string extension in Playlist.GetExtensionsForFormat(id))
                {
                    if (!first_extension)
                        result.Append(";");
                    first_extension = false;

                    result.Append("*.");
                    result.Append(extension);
                }
            }

            return result.ToString();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private MainForm GetNewWindow()
        {
            if (Playlist.CanRedo || Playlist.CanUndo || Playlist.Count != 0)
            {
                var result = new MainForm();
                result.Show();
                return result;
            }
            return this;
        }

        private void Open()
        {
            var dialog = new OpenFileDialog();

            dialog.AutoUpgradeEnabled = true;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Filter = BuildFilter(Playlist.GetSupportedSaveFormats());
            dialog.FilterIndex = 1;
            dialog.Title = "Playlaze";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                GetNewWindow().Open(dialog.FileName);
            }
        }

        private void Open(string filename)
        {
            Playlist = Playlist.Open(filename);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
        }
    }
}
