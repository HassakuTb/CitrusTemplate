using System.Collections;

namespace Citrus {
    public interface IInitializable {

        /// <summary>
        /// 単純に初期化する
        /// </summary>
        void Initialize();
    }

    public interface ICoroutineInitializable {

        /// <summary>
        /// 初期化が完了したか？
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 初期化する(コルーチン用)
        /// </summary>
        /// <returns></returns>
        IEnumerator InitializeCoroutine();
    }
}
