namespace Citrus {

    /// <summary>
    /// Singletonオブジェクト
    /// 1. グローバルアクセスの提供
    /// 2. ゲーム中唯一のインスタンスを保証
    /// 
    /// Initializableならば初期化を実行する
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T>
        where T : Singleton<T>, new() {

        private static T instance = null;

        /// <summary>
        /// 唯一のインスタンスを生成する
        /// </summary>
        public static T Instance {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }
    }

}