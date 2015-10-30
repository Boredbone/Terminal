using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Boredbone.Utility.Extensions
{
    /// <summary>
    /// Collections.Concurrentの拡張メソッド
    /// </summary>
    public static class ConcurrentExtensions
    {
        /// <summary>
        /// Queueの全要素を削除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            T item;
            while (queue.TryDequeue(out item))
            {
                // do nothing
            }
        }

        /// <summary>
        /// 辞書に要素を登録，同じKeyが存在する場合は上書き
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrReplace<K, V>(this ConcurrentDictionary<K, V> dictionary, K key, V value)
        {
            dictionary.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
        }
    }
}
