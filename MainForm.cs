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
        Playlist playlist = new Playlist();

        public MainForm()
        {
            InitializeComponent();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aboutBox = new AboutBox();
            aboutBox.Show();
        }

        private void addButton_Click(object sender, EventArgs e)
        {

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
            playlist.AddRange(items);
        }
    }
}
