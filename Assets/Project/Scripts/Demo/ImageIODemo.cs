using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AsyncReader.Demo
{
    public class ImageIODemo : MonoBehaviour
    {
        private struct TimeAnalyzeData
        {
            private long _totalMilliseconds;
            public int Count { get; private set; }

            public void Add(long milliseconds)
            {
                _totalMilliseconds += milliseconds;
                Count++;
            }

            public long GetAverage()
            {
                return _totalMilliseconds / Count;
            }
        }
        
        [SerializeField] private Preview _previewPrefab;
        [SerializeField] private Transform _parent;
        [SerializeField] private FileList _fileList;
        [SerializeField] private GameObject _ui;

        private List<Preview> _previews = new List<Preview>();
        private CancellationTokenSource _tokenSource;

        private Stopwatch _stopwatch = new Stopwatch();
        private TimeAnalyzeData _loadAnalyzeData;
        private TimeAnalyzeData _loadAsyncAnalyzeData;

        #region ### ------------------------------ MonoBehaviour ------------------------------ ###

        private void Awake()
        {
            _ui.SetActive(false);
        }

        private void OnDestroy()
        {
            _tokenSource?.Cancel();
        }

        #endregion ### ------------------------------ MonoBehaviour ------------------------------ ###

        private void Load(FileList fileList)
        {
            Stopwatch sw = new Stopwatch();
            foreach (string filename in fileList.Filenames)
            {
                string path = fileList.GetPersistentDataPath(filename);

                byte[] data = File.ReadAllBytes(path);
                
                sw.Restart();
                
                Texture2D texture = new Texture2D(0, 0);
                texture.LoadImage(data);
                
                sw.Stop();

                _loadAnalyzeData.Add(sw.ElapsedMilliseconds);

                CreatePreview(texture, filename);
            }
            
            Debug.Log($"Sync avg [{_loadAnalyzeData.Count}]: {_loadAnalyzeData.GetAverage().ToString()}ms");
        }

        private async void LoadAsync(FileList fileList)
        {
            Stopwatch sw = new Stopwatch();
            foreach (string filename in fileList.Filenames)
            {
                string path = fileList.GetRawDataSavePath(filename);

                using AsyncFileReader reader = new AsyncFileReader();
                (IntPtr ptr, long size) = await reader.LoadAsync(path);

                sw.Restart();
                ImageInfo info = ImageConverter.Decode(ptr, (int)size);

                Texture2D texture = new Texture2D(info.header.width, info.header.height, info.header.Format, false);
                texture.LoadRawTextureData(info.buffer, info.fileSize);
                texture.Apply();
                
                sw.Stop();
                _loadAsyncAnalyzeData.Add(sw.ElapsedMilliseconds);

                CreatePreview(texture, filename);
            }

            Debug.Log($"Async avg [{_loadAsyncAnalyzeData.Count}]: {_loadAsyncAnalyzeData.GetAverage().ToString()}ms");
        }

        private void CreatePreview(Texture2D texture, string filename)
        {
            Preview preview = Instantiate(_previewPrefab);
            preview.SetTexture(texture);
            preview.transform.SetParent(_parent, false);

            _previews.Add(preview);
        }

        public void StartDemo()
        {
            _ui.SetActive(true);
        }

        private void Clear()
        {
            foreach (Preview preview in _previews)
            {
                preview.Dispose();
            }

            _previews.Clear();
        }

        public void InspectorClickLoad()
        {
            Load(_fileList);
        }

        public void InspectorClickLoadAsync()
        {
            LoadAsync(_fileList);
        }

        public void InspectorClickClear()
        {
            Clear();
        }
    }
}