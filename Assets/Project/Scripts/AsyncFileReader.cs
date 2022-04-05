using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;

namespace AsyncReader
{
    public class AsyncFileReader : IDisposable
    {
        private class Awaitable
        {
            private Thread _thread;
            private ReadHandle _handle;
            private TaskCompletionSource<bool> _completionSource;
            private bool _success = false;

            public Awaitable(ReadHandle handle)
            {
                _handle = handle;

                _thread = new Thread(CheckLoop)
                {
                    IsBackground = true
                };
                _thread.Start();
            }

            ~Awaitable()
            {
                _thread.Abort();
            }

            private void CheckLoop()
            {
                while (true)
                {
                    if (_handle.Status == ReadStatus.InProgress)
                    {
                        Thread.Sleep(16);
                        continue;
                    }

                    _success = _handle.Status == ReadStatus.Complete;
                    break;
                }

                _completionSource.TrySetResult(_success);
            }

            public TaskAwaiter<bool> GetAwaiter()
            {
                _completionSource = new TaskCompletionSource<bool>();

                return _completionSource.Task.GetAwaiter();
            }
        }

        private ReadHandle _readHandle;
        private NativeArray<ReadCommand> _readCommands;
        private long _fileSize;

        public AsyncFileReader()
        {
        }

        public unsafe void Dispose()
        {
            _readHandle.Dispose();

            UnsafeUtility.Free(_readCommands[0].Buffer, Allocator.TempJob);
            _readCommands.Dispose();
        }

        public async Task<(IntPtr, long)> LoadAsync(string filePath)
        {
            UnsafeLoad(filePath);

            await new Awaitable(_readHandle);

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