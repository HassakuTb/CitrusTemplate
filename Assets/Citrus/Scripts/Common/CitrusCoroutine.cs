using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Citrus {

    /// <summary>
    /// 機能追加されたコルーチン
    /// </summary>
    public class CitrusCoroutine : IEnumerator {

        private enum CoroutineState {
            PreSetup,
            Ready,
            Invoking,
            Done,
            Suspending,
        }

        //  処理スタック
        private Stack<object> stack = new Stack<object>();

        /// <summary>
        /// 終了しているかどうか取得する
        /// </summary>
        public bool IsDone {
            get
            {
                return currentState == CoroutineState.Done;
            }
        }

        //  最近returnされたEnumeratorオブジェクト
        public object Current { get; private set; }

        private CoroutineState currentState;    //  現在状態
        private CoroutineState suspendLastState;    //  中断前の状態

        private IEnumerator coroutine;  //  設定されたコルーチン

        //  コールバック
        private Action onComplete;
        private Action onError;

        /// <summary>
        /// コルーチンオブジェクトを生成する
        /// </summary>
        public CitrusCoroutine()
        {
            currentState = CoroutineState.PreSetup;
        }

        /// <summary>
        /// コルーチンオブジェクトを生成する
        /// </summary>
        public CitrusCoroutine(IEnumerator coroutine, Action onComplete = null, Action onError = null)
        {
            currentState = CoroutineState.PreSetup;

            Setup(coroutine, onComplete, onError);
        }

        /// <summary>
        /// コルーチンをセットアップ
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        public void Setup(IEnumerator coroutine, Action onComplete = null, Action onError = null)
        {
            currentState = CoroutineState.Ready;

            this.coroutine = coroutine;
            this.onComplete = onComplete;
            this.onError = onError;

            stack.Push(coroutine);
        }

        //  コルーチンを終了する
        private void TotallyDone()
        {
            stack.Clear();
            Current = null;
            currentState = CoroutineState.Done;
        }

        /// <summary>
        /// コルーチンを一時中断する
        /// </summary>
        public void Suspend()
        {
            suspendLastState = currentState;
            currentState = CoroutineState.Suspending;
        }

        /// <summary>
        /// 中断されたコルーチンを再開する
        /// </summary>
        public void Resume()
        {
            if (currentState != CoroutineState.Suspending) return;
            currentState = suspendLastState;
        }

        /// <summary>
        /// 状況を無視してコルーチンを最初から実行する
        /// </summary>
        public void Restart()
        {
            stack.Clear();
            currentState = CoroutineState.Ready;
            stack.Push(coroutine);

            Step();
        }

        /// <summary>
        /// エラーにする
        /// </summary>
        public void RaiseError()
        {
            stack.Clear();
            currentState = CoroutineState.Done;

            onError?.Invoke();
        }

        /// <summary>
        /// 状況を無視してコルーチンを終了する
        /// コールバックは実行されない
        /// </summary>
        public void Terminate(bool isError = false)
        {
            TotallyDone();
        }

        /// <summary>
        /// コルーチンを進める
        /// </summary>
        public void Step()
        {
            //  中断状態なら何もしない
            if (currentState == CoroutineState.Suspending)
            {
                return;
            }

            //  セットアップ前なら何もしない
            if (currentState == CoroutineState.PreSetup)
            {
                return;
            }

            //  完了状態なら何もしない
            if (currentState == CoroutineState.Done)
            {
                return;
            }

            if (currentState == CoroutineState.Ready)
            {
                currentState = CoroutineState.Invoking;
            }

            if (stack.Count == 0)
            {
                onComplete?.Invoke();
                TotallyDone();
                return;
            }

            object peek = stack.Peek();
            //  yield return null された場合
            if (peek == null)
            {
                stack.Pop();
                Current = null;
            }
            //  実行対象がCoroutine
            else if (peek is IEnumerator)
            {
                IEnumerator routine = (IEnumerator)peek;
                if (routine.MoveNext())
                {   //  IEnumerator未完了時
                    stack.Push(routine.Current);
                    Step();
                }
                else
                {   //  IEnumerator完了時
                    if (stack.Count > 0) stack.Pop();
                    Step();
                }
            }
            //  AsyncOperation
            else if (peek is AsyncOperation)
            {
                AsyncOperation operation = (AsyncOperation)peek;
                if (!operation.isDone)
                {
                    Current = null;
                }
                else
                {
                    stack.Pop();
                    Step();
                }
            }
            else
            {
                stack.Pop();
                Current = peek;
            }
        }

        /// <summary>
        /// StartCoroutineで実行する場合、毎フレーム呼び出される
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            Step();
            return !IsDone;
        }

        //  利用されない
        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
