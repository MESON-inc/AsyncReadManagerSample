using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace AsyncReader
{
    public class AsyncImageReader
    {
        private ReadHandle _readHandle;
        private NativeArray<ReadCommand> _readCommands;
        private long _fileSize = 0;

        private TaskCompletionSource<ImageInfo> _completionSource;
        private Texture2D _texture;
        private bool _disposed = false;
        private bool _running = false;
        private Thread _thread;
        private SynchronizationContext _context;
        private CancellationToken _cancellationToken;

        public AsyncImageReader()
        {
            _context = SynchronizationContext.Current;
        }

        public async Task<Texture2D> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("This object is already disposed so this is not able to load a texture.");
            }

            if (_running)
            {
                Debug.LogError($"This instance is still running.");
                return null;
            }

            _running = true;

            _cancellationToken = cancellationToken;

            _completionSource = new TaskCompletionSource<ImageInfo>();

            UnsafeLoad(path);

            ImageInfo info = await _completionSource.Task;

            Texture2D texture = new Texture2D(info.header.width, info.header.height, info.header.Format, false);
            texture.LoadRawTextureData(info.buffer, info.fileSize);
            texture.Apply();

            Dispose();

            return texture;
        }

        private unsafe void UnsafeLoad(string path)
        {
            FileInfo info = new FileInfo(path);
            _fileSize = info.Length;

            _readCommands = new NativeArray<ReadCommand>(1, Allocator.TempJob);
            _readCommands[0] = new ReadCommand
            {
                Offset = 0,
                Size = _fileSize,
                Buffer = (byte*)UnsafeUtility.Malloc(_fileSize, UnsafeUtility.AlignOf<byte>(), Allocator.TempJob),
            };

            _readHandle = AsyncReadManager.Read(path, (ReadCommand*)_readCommands.GetUnsafePtr(), 1);

            _thread = new Thread(CheckLoop);
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void CheckLoop()
        {
            while (_running)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    Dispose();
                    return;
                }

                if (_readHandle.Status == ReadStatus.InProgress)
                {
                    Thread.Sleep(16);
                    continue;
                }

                if (_readHandle.Status != ReadStatus.Complete)
                {
                    Dispose();
                    _completionSource.TrySetException(new InvalidDataException());
                }
                else
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        Dispose();
                        return;
                    }

                    ImageInfo result = DecodeData();
                    _completionSource.TrySetResult(result);
                }

                break;
            }
        }

        private unsafe ImageInfo DecodeData()
        {
            IntPtr ptr = (IntPtr)_readCommands[0].Buffer;
            return ImageConverter.Decode(ptr, (int)_fileSize);
        }

        private unsafe void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            _running = false;

            _readHandle.Dispose();
            UnsafeUtility.Free(_readCommands[0].Buffer, Allocator.TempJob);
            _readCommands.Dispose();
        }
    }
}