using System;
using UnityEngine;

namespace Citrus {

    /// <summary>
    /// Component拡張
    /// </summary>
    public static partial class ComponentExtensions {

        /// <summary>
        /// 自分を含めた子要素にアタッチされた特定の型のコンポーネントすべてに処理を適用する
        /// </summary>
        /// <typeparam name="T">コンポーネント型</typeparam>
        /// <param name="component"></param>
        /// <param name="action">適用する処理</param>
        /// <param name="includeInactive">inactiveなgameObjectを含むか</param>
        public static void ForEachChildrenComponents<T>(this Component component, Action<T> action, bool includeInactive = false)
             where T : Component
        {
            component.GetComponentsInChildren<T>().ForEach(action);
        }
    }
}
