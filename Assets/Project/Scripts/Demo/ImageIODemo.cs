using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace AsyncReader.Demo
{
    public class ImageIODemo : MonoBehaviour
    {
        [SerializeField] private Preview _previewPrefab;
        [SerializeField] private Transform _parent;
        [SerializeField] private FileList _fileList;
        [SerializeField] private FileList _fileList2;

        private List<Preview> _previews = new List<Preview>();

        private bool _started = false;
        private CancellationTokenSource _tokenSource;

        private void OnDestroy()
        {
            _tokenSource?.Cancel();
        }

        private void OnGUI()
        {
            if (!_started) return;
            
            const int height = 100;
            const int padding = 20;
            const int margin = 30;

            if (GUI.Button(new Rect(50, margin, 300, height), "Load"))
            {
                LoadUsingLoadImage(_fileList);
            }

            if (GUI.Button(new Rect(50, margin + height + padding, 300, height), "LoadAsync"))
            {
                LoadUsingAsyncReadManager(_fileList);
            }

            if (GUI.Button(new Rect(50, margin + (height + padding) * 2, 300, height), "LoadAsync2"))
            {
                LoadUsingAsyncReadManager(_fileList2);
            }

            if (GUI.Button(new Rect(50, margin + (height + padding) * 3, 300, height), "Clear"))
            {
                Clear();
            }
        }

        private async void LoadUsingLoadImage(FileList fileList)
        {
            foreach (string filename in fileList.Filenames)
            {
                string path = fileList.GetPersistentDataPath(filename);

                AsyncFileReader reader = new AsyncFileReader();
                (IntPtr ptr, long size) = await reader.LoadAsync(path);

                byte[] data = new byte[size];
                Marshal.Copy(ptr, data, 0, (int)size);

                Texture2D texture = new Texture2D(0, 0);
                texture.LoadImage(data);

                CreatePreview(texture, filename);
            }
        }

        private async void LoadUsingAsyncReadManager(FileList fileList)
        {
            _tokenSource = new CancellationTokenSource();

            foreach (string filename in fileList.Filenames)
            {
                AsyncImageReader reader = new AsyncImageReader();
                string rawDataPath = fileList.GetRawDataSavePath(filename);
                Texture2D texture = await reader.LoadAsync(rawDataPath, _tokenSource.Token);

                if (_tokenSource.IsCancellationRequested) return;

                CreatePreview(texture, filename);
            }
        }

        private void CreatePreview(Texture2D texture, string filename)
        {
            Preview preview = Instantiate(_previewPrefab);
            preview.SetTexture(texture);
            preview.transform.SetParent(_parent, false);

            _previews.Add(preview);
        }

        public void StartDemo() => _started = true;

        private void Clear()
        {
            foreach (Preview preview in _previews)
            {
                preview.Dispose();
            }

            _previews.Clear();
        }
    }
}