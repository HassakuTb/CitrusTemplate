using UnityEngine;

namespace Citrus {
    public static partial class Vector3Extensions {

        /// <summary>
        /// Z成分を固定したベクトルを取得する
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Vector3 XY(this Vector3 src, float z = 0f)
        {
            Vector3 dst = src;
            dst.z = z;
            return dst;
        }

        /// <summary>
        /// Y成分を固定したベクトルを取得する
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Vector3 XZ(this Vector3 src, float y = 0f)
        {
            Vector3 dst = src;
            dst.y = y;
            return dst;
        }

        /// <summary>
        /// X成分を固定したベクトルを取得する
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Vector3 YZ(this Vector3 src, float x = 0f)
        {
            Vector3 dst = src;
            dst.x = x;
            return dst;
        }
    }
}
