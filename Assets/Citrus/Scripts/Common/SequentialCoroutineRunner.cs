using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Citrus {

    public enum CoroutineRunnerErrorType {
        RaisedError,
        Exception,
        Timeout,
    }

    /// <summary>
    /// CoroutineRunnerの中で発生するエラー
    /// </summary>
    /// <typeparam name="RaisedErrorType">アプリケーション側で取り扱うエラーの型</typeparam>
    public class CoroutineRunnerError<RaisedErrorType> {

        /// <summary>
        /// ユーザーが発生させたエラーを生成する
        /// </summary>
        /// <typeparam name="T">アプリケーション側で取り扱うエラーの型</typeparam>
        /// <param name="raisedError">発生させるエラー</param>
        /// <returns></returns>
        internal static CoroutineRunnerError<T> CreateRaisedError<T>(T raisedError)
        {
            CoroutineRunnerError<T> error = new CoroutineRunnerError<T>
            {
                Type = CoroutineRunnerErrorType.RaisedError,
                RaisedError = raisedError
            };
            return error;
        }

        /// <summary>
        /// 実行中に発生した例外を生成する
        /// </summary>
        /// <typeparam name="T">アプリケーション側で取り扱うエラーの型</typeparam>
        /// <param name="exception">発生した例外</param>
        /// <returns></returns>
        internal static CoroutineRunnerError<T> CreateExceptionError<T>(Exception exception)
        {
            CoroutineRunnerError<T> error = new CoroutineRunnerError<T>
            {
                Type = CoroutineRunnerErrorType.Exception,
                Exception = exception
            };
            return error;
        }

        /// <summary>
        /// タイムアウトエラーを生成する
        /// </summary>
        /// <typeparam name="T">アプリケーション側で取り扱うエラーの型</typeparam>
        /// <returns></returns>
        internal static CoroutineRunnerError<T> CreateTimeout<T>()
        {
            CoroutineRunnerError<T> error = new CoroutineRunnerError<T>
            {
                Type = CoroutineRunnerErrorType.Timeout,
            };
            return error;
        }

        /// <summary>
        /// エラーがどういうエラーかを取得する
        /// RaisedError : アプリケーション側でRaiseErrorで発生させたエラー
        /// Exception : 実行中で発生した例外
        /// Timeout : タイムアウトした場合
        /// </summary>
        public CoroutineRunnerErrorType Type { get; private set; }

        /// <summary>
        /// アプリケーション側でRaiseErrorで発生させたエラーを取得する
        /// </summary>
        /// <remarks>
        /// Type == CoroutineRunnerErrorType.RaisedError でないときの値は不定
        /// </remarks>
        public RaisedErrorType RaisedError { get; private set; }

        /// <summary>
        /// 実行中に発生した例外を取得する
        /// </summary>
        /// <remarks>
        /// Type == CoroutineRunnerErrorType.Exception でないときの値は不定
        /// </remarks>
        public Exception Exception { get; private set; }

        private CoroutineRunnerError() { }
    }

    /// <summary>
    /// 連続したコルーチンを実行する
    /// このクラスは使いまわしを想定しておらず、使い捨てにする
    /// </summary>
    /// <example>
    /// var runner = SequentialCoroutineRunner<ErrorType>();
    /// runner.Append(SomeCoroutine(runner), SomeCoroutine(runner), ...);
    /// runner.SetTimeout(30f); //  タイムアウト設定(optional)
    /// runner.SetInterruption(() => {return ...}, () =>{runner.RaiseError(...);});  // 割り込み設定(optional)
    /// StartCoroutine(runner.Coroutine());
    /// 
    /// IEnumerator SomeCoroutine()
    /// {
    ///     ...
    ///     runner.Cancel(); //  中断
    ///     
    ///     runner.Append(...); //  処理の追加
    ///     
    ///     runner.RaiseError(error);   //  エラー終了
    ///     
    /// }
    /// </example>
    public class SequentialCoroutineRunner<ErrorType> {

        //  コルーチンキュー
        private Queue<IEnumerator> queue = new Queue<IEnumerator>();

        //  キャンセルされたか？
        private bool isRequestedCancel = false;

        //  エラーリクエストがされたか？
        private bool isRaisedError = false;

        /// <summary>
        /// 発生したエラーを取得する
        /// </summary>
        public CoroutineRunnerError<ErrorType> Error { get; private set; }

        //  完了時処理
        private readonly Action onComplete;

        //  エラー時処理
        private readonly Action<CoroutineRunnerError<ErrorType>> onError;

        //  共通処理
        private readonly Action onFinally;

        //  タイムアウト時間
        private float timeout;

        //  割り込み条件
        private Func<bool> interruptPredicate;

        //  割り込み時の処理
        private Action onInterrupt;

        /// <summary>
        /// タイムアウトがあるか取得する
        /// デフォルトタイムアウトはなし
        /// </summary>
        public bool HasTimeout { get; private set; }

        /// <summary>
        /// 連続したコルーチンを処理するインスタンスを生成する
        /// </summary>
        public SequentialCoroutineRunner() : this(null, null) { }

        /// <summary>
        /// 連続したコルーチンを処理するインスタンスを生成する
        /// </summary>
        /// <param name="onComplete">成功時の処理</param>
        /// <param name="onError">失敗時の処理</param>
        /// <param name="onFinally">成功、失敗にかかわらず実行する処理</param>
        public SequentialCoroutineRunner(Action onComplete, Action<CoroutineRunnerError<ErrorType>> onError, Action onFinally = null)
        {
            this.onComplete = onComplete;
            this.onError = onError;
            this.onFinally = onFinally;
        }

        /// <summary>
        /// 連続したコルーチンを処理するインスタンスを生成する
        /// </summary>
        /// <param name="coroutines">処理するコルーチン</param>
        /// <param name="onComplete">成功時の処理</param>
        /// <param name="onError">失敗時の処理</param>
        /// <param name="onFinally">成功、失敗にかかわらず実行する処理</param>
        public SequentialCoroutineRunner(IEnumerable<IEnumerator> coroutines, Action onComplete, Action<CoroutineRunnerError<ErrorType>> onError, Action onFinally = null)
            : this(onComplete, onError, onFinally)
        {
            Append(coroutines);
        }

        /// <summary>
        /// タイムアウトを設定する
        /// </summary>
        /// <param name="time">タイムアウト時間</param>
        public void SetTimeout(float time)
        {
            HasTimeout = true;
            timeout = time;
        }

        /// <summary>
        /// 割り込み関数を設定する
        /// </summary>
        /// <remarks>
        /// predicateがtrueのとき、onInterruptが実行される
        /// </remarks>
        /// <param name="predicate">割り込み条件/param>
        /// <param name="onInterrupt">割り込み時に実行される処理</param>
        public void SetInterruption(Func<bool> predicate, Action onInterrupt)
        {
            interruptPredicate = predicate;
            this.onInterrupt = onInterrupt;
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
        public void RaiseError(ErrorType error)
        {
            Error = CoroutineRunnerError<ErrorType>.CreateRaisedError(error);
            isRequestedCancel = true;
            isRaisedError = true;
        }

        /// <summary>
        /// 処理を中断する
        /// </summary>
        /// <remarks>
        /// 処理を中断した場合、onCompleteとonErrorは両方とも呼び出されない
        /// onFinallyは呼び出される
        /// </remarks>
        public void Cancel()
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
                    Error = CoroutineRunnerError<ErrorType>.CreateTimeout<ErrorType>();
                    break;
                }

                if (interruptPredicate != null && interruptPredicate())
                {
                    onInterrupt?.Invoke();
                }

                if (isRaisedError || isRequestedCancel)
                {
                    break;
                }

                if (queue.Count == 0)
                {
                    onComplete?.Invoke();
                    break;
                }

                IEnumerator peek = queue.Peek();
                bool hasNext = false;
                try
                {
                    hasNext = peek.MoveNext();
                }
                catch (Exception e)
                {
                    Error = CoroutineRunnerError<ErrorType>.CreateExceptionError<ErrorType>(e);
                    break;
                }
                if (hasNext)
                {
                    yield return peek.Current;
                }
                else
                {
                    queue.Dequeue();
                }
            }

            if (Error != null)
            {
                onError?.Invoke(Error);
            }

            onFinally?.Invoke();
        }
    }
}
