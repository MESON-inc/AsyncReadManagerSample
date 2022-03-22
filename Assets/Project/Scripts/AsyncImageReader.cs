using System;
using System.IO;
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

        private TaskCompletionSource<Texture2D> _completionSource;
        private bool _disposed = false;

        public async Task<Texture2D> LoadAsync(string path)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("This object is already disposed so this is not able to load a texture.");
            }

            _completionSource = new TaskCompletionSource<Texture2D>();

            UnsafeLoad(path);

            return await _completionSource.Task;
        }

        private unsafe void UnsafeLoad(string path)
        {
            FileInfo info = new FileInfo(path);
            _fileSize = info.Length;

            _readCommands = new NativeArray<ReadCommand>(1, Allocator.Persistent);
            _readCommands[0] = new ReadCommand
            {
                Offset = 0,
                Size = _fileSize,
                Buffer = (byte*)UnsafeUtility.Malloc(_fileSize, UnsafeUtility.AlignOf<byte>(), Allocator.Persistent),
            };

            _readHandle = AsyncReadManager.Read(path, (ReadCommand*)_readCommands.GetUnsafePtr(), 1);

            CheckLoop();
        }

        private async void CheckLoop()
        {
            while (true)
            {
                if (_readHandle.Status == ReadStatus.InProgress)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime));
                    continue;
                }

                if (_readHandle.Status != ReadStatus.Complete)
                {
                    Dispose();
                    Notify(null);
                }
                else
                {
                    Texture2D result = ReadTexture();
                    Notify(result);
                }

                break;
            }
        }

        private void Notify(Texture2D result)
        {
            _completionSource.TrySetResult(result);
        }

        private unsafe Texture2D ReadTexture()
        {
            IntPtr ptr = (IntPtr)_readCommands[0].Buffer;

            Texture2D texture = new Texture2D(1, 1);

            try
            {
                texture.LoadRawTextureData(ptr, (int)_fileSize);
                texture.Apply();
            }
            catch (Exception e)
            {
                Debug.Log(e);

                GameObject.Destroy(texture);

                throw;
            }
            finally
            {
                Dispose();
            }

            return texture;
        }

        private unsafe void Dispose()
        {
            _readHandle.Dispose();
            UnsafeUtility.Free(_readCommands[0].Buffer, Allocator.Persistent);
            _readCommands.Dispose();

            _disposed = true;
        }
    }
}