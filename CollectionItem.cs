﻿using System;
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

        internal void InternalReplaceItemRange(int startingIndex, int count, IReadOnlyList<PlaylistItem> newItems)
        {
            for (int i = 0; i < count; i++)
            {
                _collection[startingIndex + i].Parent = null;
            }
            _collection.RemoveRange(startingIndex, count);
            foreach (PlaylistItem item in newItems)
            {
                item.Parent = this;
            }
            _collection.InsertRange(startingIndex, newItems);
        }

        int ICollection<PlaylistItem>.Count => _collection.Count;

        bool ICollection<PlaylistItem>.IsReadOnly => false;

        void ReplaceItemRange(int index, int length, IReadOnlyList<PlaylistItem> newItems)
        {
            var oldItems = _collection.GetRange(index, length).AsReadOnly();
            var action = new ReplaceItemRangeAction(this, index, oldItems, newItems);
            InternalDoAction(action);
        }

        public void AddRange(IEnumerable<PlaylistItem> items)
        {
            var itemlist = new List<PlaylistItem>(items).AsReadOnly();
            ReplaceItemRange(_collection.Count, 0, itemlist);
        }

        public void Add(PlaylistItem item)
        {
            AddRange(new PlaylistItem[] { item });
        }

        public void Clear()
        {
            ReplaceItemRange(0, _collection.Count, new List<PlaylistItem>().AsReadOnly());
        }

        public bool Contains(PlaylistItem item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(PlaylistItem[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public IEnumerator<PlaylistItem> GetEnumerator()
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

        public PlaylistItem this[int i] => _collection[i];
    }
}