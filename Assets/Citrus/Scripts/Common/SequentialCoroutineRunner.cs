using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Citrus {
    /// <summary>
    /// 連続したコルーチンを実行する
    /// </summary>
    /// <example>
    /// var runner = SequentialCoroutineRunner();
    /// runnner.Append(SomeCoroutine(runner), SomeCoroutine(runner), ...);
    /// StartCoroutine(runner.Coroutine());
    /// 
    /// IEnumerator SomeCoroutine(SequentialCoroutineRunner runner)
    /// {
    ///     ...
    ///     runner.Terminate(); //  中断
    /// }
    /// </example>
    public class SequentialCoroutineRunner {

        //  コルーチンキュー
        private Queue<IEnumerator> queue = new Queue<IEnumerator>();

        //  キャンセルされたか？
        private bool isRequestedCancel = false;

        //  エラーリクエストがされたか？
        private bool isRaisedError = false;

        /// <summary>
        /// Raiseされたエラーを取得する
        /// </summary>
        public object Error { get; private set; }

        //  完了時処理
        private readonly Action onComplete;

        //  エラー時処理
        private readonly Action<object> onError;

        //  タイムアウト時間
        private float timeout;

        /// <summary>
        /// タイムアウトがあるか取得する
        /// デフォルトタイムアウトはなし
        /// </summary>
        public bool HasTimeout { get; private set; }

        //  タイムアウト時処理
        private Action onTimeout;

        /// <summary>
        /// 連続したコルーチンを処理するインスタンスを生成する
        /// </summary>
        public SequentialCoroutineRunner() : this(null, null) { }

        /// <summary>
        /// 連続したコルーチンを処理するインスタンスを生成する
        /// </summary>
        /// <param name="onComplete">成功時の処理</param>
        /// <param name="onError">失敗時の処理</param>
        public SequentialCoroutineRunner(Action onComplete, Action<object> onError)
        {
            this.onComplete = onComplete;
            this.onError = onError;
        }

        /// <summary>
        /// 連続したコルーチンを処理するインスタンスを生成する
        /// </summary>
        /// <param name="coroutines">処理するコルーチン</param>
        /// <param name="onComplete">成功時の処理</param>
        /// <param name="onError">失敗時の処理</param>
        public SequentialCoroutineRunner(IEnumerable<IEnumerator> coroutines, Action onComplete, Action<object> onError)
            : this(onComplete, onError)
        {
            Append(coroutines);
        }

        /// <summary>
        /// タイムアウトを設定する
        /// </summary>
        /// <param name="time">タイムアウト時間</param>
        /// <param name="onTimeout">タイムアウト時の処理</param>
        public void SetTimeout(float time, Action onTimeout = null)
        {
            HasTimeout = true;
            this.onTimeout = onTimeout;
            timeout = time;
        }

        /// <summary>
        /// タイムアウトを無効にする
        /// </summary>
        public void DisableTimeout()
        {
            HasTimeout = false;
        }

        /// <summary>
        /// 処理を追加する
        /// </summary>
        /// <param name="coroutine">追加するコルーチン</param>
        public void Append(IEnumerator coroutine)
        {
            queue.Enqueue(coroutine);
        }

        /// <summary>
        /// 処理を追加する
        /// </summary>
        /// <param name="coroutines">追加するコルーチン</param>
        public void Append(IEnumerable<IEnumerator> coroutines)
        {
            foreach (IEnumerator e in coroutines)
            {
                Append(e);
            }
        }

        /// <summary>
        /// 処理を追加する
        /// </summary>
        /// <param name="coroutines">追加するコルーチン</param>
        public void Append(params IEnumerator[] coroutines)
        {
            foreach (IEnumerator e in coroutines)
            {
                Append(e);
            }
        }

        /// <summary>
        /// エラーを発行して処理を中断する
        /// </summary>
        /// <remarks>
        /// Exceptionで処理を中断したいコルーチンはcatch節でRaiseErrorを実行するように実装する
        /// 発行したエラーはコンストラクタ引数のonErrorコールバックで取得可能
        /// </remarks>
        /// <example>
        /// IEnumerator SomeCoroutine(SequentialCoroutineRunner runner)
        /// {
        ///     try
        ///     {
        ///         ...
        ///     }
        ///     catch(Exception e)
        ///     {
        ///         runner.RaiseError(e);
        ///         throw;
        ///     }
        /// }
        /// </example>
        /// <param name="error">発生させるエラー デフォルトはnull</param>
        public void RaiseError(object error = null)
        {
            Error = error;
            isRequestedCancel = true;
            isRaisedError = true;
        }

        /// <summary>
        /// 処理を中断する
        /// </summary>
        /// <remarks>
        /// 処理を中断した場合、onCompleteとonErrorは両方とも呼び出されない
        /// </remarks>
        public void Terminate()
        {
            isRequestedCancel = true;
        }

        /// <summary>
        /// 順番に処理を行うコルーチンを取得する
        /// </summary>
        /// <returns></returns>
        public IEnumerator Coroutine()
        {
            float startTime = Time.realtimeSinceStartup;

            while (true)
            {
                if (HasTimeout && Time.realtimeSinceStartup - startTime > timeout)
                {
                    onTimeout?.Invoke();
                    yield break;
                }

                if (isRaisedError)
                {
                    onError?.Invoke(Error);
                    yield break;
                }

                if (isRequestedCancel)
                {
                    yield break;
                }

                if (queue.Count == 0)
                {
                    onComplete?.Invoke();
                    yield break;
                }

                IEnumerator peek = queue.Peek();
                if (peek.MoveNext())
                {
                    yield return peek.Current;
                }
                else
                {
                    queue.Dequeue();
                }
            }
        }
    }
}
