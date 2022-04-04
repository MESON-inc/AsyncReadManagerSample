using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace AsyncReader.Utility
{
    public static class UnityWebRequestExtension
    {
        public static UnityWebRequestAwaitable GetAwaiter(this UnityWebRequestAsyncOperation operation)
        {
            return new UnityWebRequestAwaitable(operation);
        }


        public class UnityWebRequestAwaitable : INotifyCompletion
        {
            private UnityWebRequestAsyncOperation _operation;
            private Action _continuation;

            public UnityWebRequestAwaitable(UnityWebRequestAsyncOperation operation)
            {
                _operation = operation;
                
                CoroutineDispatcher.Instance.Dispatch(CheckLoop());
            }

            public bool IsCompleted => _operation.isDone;
            public void OnCompleted(Action continuation) => _continuation = continuation;

            public void GetResult()
            {
            }

            private IEnumerator CheckLoop()
            {
                yield return _operation;

                _continuation?.Invoke();
            }
        }
    }
}