using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Boredbone.Utility.Extensions;

namespace Terminal.ViewModels
{
    public class LimitedLog<T> : INotifyCollectionChanged, IEnumerable<T>
    {

        public int MaxCapacity { get; set; }
        public int CompressedCapacity { get; set; }

        public T Last { get; private set; }
        public int Count => this.history.Count;

        private ConcurrentQueue<T> history;

        public LimitedLog(int maxCapacity, int compressedCapacity)
        {
            this.MaxCapacity = maxCapacity;
            this.CompressedCapacity = compressedCapacity;
            this.history = new ConcurrentQueue<T>();
        }

        public void Add(T item)
        {
            var index = this.history.Count;

            var compressed = this.Compress(this.history, this.MaxCapacity - 1, this.CompressedCapacity - 1);


            if (compressed != null && compressed.Count > 0)
            {
                this.CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, compressed, 0));
            }

            this.AddMain(item);
            this.CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));

        }

        private void AddMain(T item)
        {
            this.history.Enqueue(item);
            this.Last = item;
        }

        private List<T> Compress(ConcurrentQueue<T> queue, int maxCapacity, int compressedCapacity)
        {
            if (queue.Count <= maxCapacity)
            {
                return null;
            }

            var list = new List<T>();
            T item;
            while (queue.Count > compressedCapacity && queue.TryDequeue(out item))
            {
                list.Add(item);
            }

            //Debug.WriteLine("log over");

            return list;
        }

        public void Clear()
        {
            this.history.Clear();
            this.Last = default(T);
            this.CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IEnumerator<T> GetEnumerator()
            => this.history.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable<T>)this).GetEnumerator();
    }
}
