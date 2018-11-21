using System;
using System.Collections.Generic;

namespace Citrus {
    /// <summary>
    /// IEnumerable<>拡張
    /// </summary>
    public static class IEnumerableExtensions {

        /// <summary>
        /// 全ての要素についてactionを実行する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <param name="action"></param>
        public static void ForEach<T> (this IEnumerable<T> elements, Action<T> action)
        {
            foreach(T e in elements)
            {
                action(e);
            }
        }
    }
}
