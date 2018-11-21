using UnityEngine;

namespace Citrus {
    /// <summary>
    /// Transform拡張
    /// </summary>
    public static partial class TransformExtensions {

        /// <summary>
        /// 全ての子要素を破棄する
        /// </summary>
        /// <param name="transform"></param>
        public static void DestroyAllChildren(this Transform transform)
        {
            transform.ForEachChildrenComponents<Transform>(t =>
            {
                GameObject.Destroy(t.gameObject);
            }, true);

            transform.DetachChildren();
        }
    }
}
