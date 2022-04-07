using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace AsyncReader
{
    public class AsyncFileReader : IDisposable
    {
        private class Awaitable : IDisposable
        {
            private Thread _thread;
            private ReadHandle _handle;
            private TaskCompletionSource<bool> _completionSource;
            private bool _success = false;
            private bool _stopped = false;

            public Awaitable(ReadHandle handle)
            {
                _handle = handle;

                _thread = new Thread(CheckLoop)
                {
                    IsBackground = true
                };
            }

            ~Awaitable()
            {
                Dispose();
            }

            private void CheckLoop()
            {
                while (true)
                {
                    if (_stopped) return;
                    
                    if (_handle.Status == ReadStatus.InProgress)
                    {
                        Thread.Sleep(16);
                        continue;
                    }
                    
                    if (_stopped) return;

                    _success = _handle.Status == ReadStatus.Complete;
                    break;
                }

                _completionSource?.TrySetResult(_success);
            }

            public TaskAwaiter<bool> GetAwaiter()
            {
                _completionSource = new TaskCompletionSource<bool>();
                
                _thread.Start();

                return _completionSource.Task.GetAwaiter();
            }

            public void Dispose()
            {
                if (_stopped) return;
                
                if (_thread is { IsAlive: false }) return;
                
                _stopped = true;
                
                _thread.Abort();
            }
        }

        private ReadHandle _readHandle;
        private NativeArray<ReadCommand> _readCommands;
        private long _fileSize;

        public unsafe void Dispose()
        {
            _readHandle.Dispose();

            UnsafeUtility.Free(_readCommands[0].Buffer, Allocator.TempJob);
            _readCommands.Dispose();
        }

        public async Task<(IntPtr, long)> LoadAsync(string filePath)
        {
            UnsafeLoad(filePath);

            Awaitable awaitable = new Awaitable(_readHandle);
            await awaitable;
            
            awaitable.Dispose();

            IntPtr ptr = GetPointer();

            return (ptr, _fileSize);
        }

        private unsafe void UnsafeLoad(string filePath)
        {
            FileInfo info = new FileInfo(filePath);
            _fileSize = info.Length;

            _readCommands = new NativeArray<ReadCommand>(1, Allocator.TempJob);
            _readCommands[0] = new ReadCommand
            {
                Offset = 0,
                Size = _fileSize,
                Buffer = (byte*)UnsafeUtility.Malloc(_fileSize, UnsafeUtility.AlignOf<byte>(), Allocator.TempJob),
            };

            _readHandle = AsyncReadManager.Read(filePath, (ReadCommand*)_readCommands.GetUnsafePtr(), 1);
        }

        private unsafe IntPtr GetPointer()
        {
            return (IntPtr)_readCommands[0].Buffer;
        }
    }
}