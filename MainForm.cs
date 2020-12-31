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
            int index;
            for (index = 0; index < parent.Parent.Count(); index++)
            {
                if (parent.Parent[index] == parent)
                    break;
            }
            return parent_collection[index].Nodes;
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
            CollectionItem parent_node;
            if (node.Parent is null)
            {
                parent_node = Playlist;
            }
            else
            {
                parent_node = (CollectionItem)PlaylistItemForNode(node.Parent);
            }
            return parent_node[node.Index];
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
    }
}
