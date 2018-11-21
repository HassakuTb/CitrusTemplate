using System.Collections;

namespace Citrus {
    /// <summary>
    /// MonoBehaviour外からCoroutineの利用を可能にする
    /// </summary>
    public class CoroutineHandler : SingletonMonoBehaviour<CoroutineHandler> {

        /// <summary>
        /// コルーチンを開始する
        /// </summary>
        public static void StartCoroutineStatic(IEnumerator coroutine)
        {
            Instance.StartCoroutine(coroutine);
        }
    }
}
