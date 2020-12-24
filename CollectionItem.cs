using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace playlaze
{
    public abstract class CollectionItem : PlaylistItem, ICollection<PlaylistItem>
    {
        List<PlaylistItem> _collection;

        protected CollectionItem(IEnumerable<PlaylistItem> contents)
        {
            _collection = new List<PlaylistItem>(contents);
        }

        protected CollectionItem()
        {
            _collection = new List<PlaylistItem>();
        }

        int ICollection<PlaylistItem>.Count => _collection.Count;

        bool ICollection<PlaylistItem>.IsReadOnly => false;

        void ICollection<PlaylistItem>.Add(PlaylistItem item)
        {
            throw new NotImplementedException();
        }

        void ICollection<PlaylistItem>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<PlaylistItem>.Contains(PlaylistItem item)
        {
            return _collection.Contains(item);
        }

        void ICollection<PlaylistItem>.CopyTo(PlaylistItem[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        IEnumerator<PlaylistItem> IEnumerable<PlaylistItem>.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        bool ICollection<PlaylistItem>.Remove(PlaylistItem item)
        {
            throw new NotImplementedException();
        }
    }
}
