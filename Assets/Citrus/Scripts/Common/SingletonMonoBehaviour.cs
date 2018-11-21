using System.Collections;
using UnityEngine;

namespace Citrus {

    /// <summary>
    /// SingletonなMonoBehaviour
    /// 1. グローバルアクセスの提供
    /// 2. ゲーム中唯一のインスタンスを保証
    /// 
    /// Initializableならば初期化を実行する
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour
        where T : SingletonMonoBehaviour<T> {

        private static T instance = null;

        private bool HasInstance { get; set; } = false;

        /// <summary>
        /// 唯一のインスタンスを生成する
        /// </summary>
        public static T Instance {
            get
            {
                if (instance == null)
                {
                    Load();
                }
                return instance;
            }
        }

        /// <summary>
        /// ゲーム中にインスタンス化したくない場合にBootなどで
        /// あらかじめロードする用
        /// </summary>
        public static void PreLoad()
        {
            Load();
        }

        /// <summary>
        /// ゲーム中にインスタンス化したくない場合にBootなどで
        /// あらかじめロードする用
        /// </summary>
        public static IEnumerator PreLoadCoroutine()
        {
            Load();

            //  コルーチン初期化可能なら初期化完了まで待つ
            if (instance is ICoroutineInitializable)
            {
                ICoroutineInitializable x = instance as ICoroutineInitializable;
                while (!x.IsInitialized) yield return null; 
            }
        }

        private static void Load()
        {
            GameObject obj = new GameObject();
            instance = obj.AddComponent<T>();
            DontDestroyOnLoad(obj);

            InitializeInstanceIfInitializable();
        }
        
        //  インスタンスが初期化可能なら初期化する
        private static void InitializeInstanceIfInitializable()
        {
            if (instance is IInitializable)
            {
                IInitializable x = instance as IInitializable;
                x.Initialize();
            }

            if (instance is ICoroutineInitializable)
            {
                ICoroutineInitializable x = instance as ICoroutineInitializable;
                instance.StartCoroutine(x.InitializeCoroutine());
            }
        }
        
        //  このコンポーネントがSingletonとして機能しているか
        private bool isSingletonInstance = false;

        protected void Awake()
        {
            //  既にインスタンス化されたオブジェクトがあるなら即時破棄する
            if(HasInstance)
            {
                Destroy(gameObject);
                return;
            }
            
            //  インスタンスが登録されていないならばこれを唯一のインスタンスとする
            //  (スクリプトからではなくSceneにあらかじめアタッチされている場合のケア)
            if(instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);

                InitializeInstanceIfInitializable();
            }

            HasInstance = true;
            isSingletonInstance = true;
        }

        private void OnDestroy()
        {
            if (isSingletonInstance)
            {
                HasInstance = false;
                instance = null;
            }
        }
    }

}