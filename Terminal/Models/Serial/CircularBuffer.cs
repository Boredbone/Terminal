using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Models.Serial
{
    public class CircularBuffer<T>
    {

        private T[] data;
        private int mask;

        private int currentIndex;

        public int Length => this.data.Length;

        public CircularBuffer() : this(256) { }

        public CircularBuffer(int capacity)
        {
            capacity = Pow2((uint)capacity);
            this.data = new T[capacity];
            this.currentIndex = 0;

            this.mask = capacity - 1;
        }

        static int Pow2(uint n)
        {
            --n;
            int p = 0;
            for (; n != 0; n >>= 1) p = (p << 1) + 1;
            return p + 1;
        }

        public void Add(T value)
        {
            this.data[this.currentIndex & this.mask] = value;
            this.currentIndex = (this.currentIndex + 1) & this.mask;
        }

        public T GetPrevious(int index)
        {
            return this.data[(this.currentIndex - index - 1) & this.mask];
        }
    }
}
