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
        [SerializeField] private Preview _previewPrefab;
        [SerializeField] private Transform _parent;
        [SerializeField] private FileList _fileList;

        private List<Preview> _previews = new List<Preview>();

        private bool _started = false;
        private SynchronizationContext _context;
        private CancellationTokenSource _tokenSource;

        private void Awake()
        {
            _context = SynchronizationContext.Current;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                // Load();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                LoadUsingAsyncReadManager();
            }
        }

        private void OnDestroy()
        {
            _tokenSource?.Cancel();
        }

        private void OnGUI()
        {
            if (_started)
            {
                const int height = 100;
                const int padding = 20;
                const int margin = 30;
                
                if (GUI.Button(new Rect(50, margin, 300, height), "Load"))
                {
                    Load();
                }

                if (GUI.Button(new Rect(50, margin + height + padding, 300, height), "LoadAsync"))
                {
                    LoadUsingAsyncReadManager();
                }
                
                if (GUI.Button(new Rect(50, margin + (height + padding) * 2, 300, height), "Clear"))
                {
                    Clear();
                }
            }
        }

        private void Load()
        {
            foreach (string filename in _fileList.Filenames)
            {
                string path = _fileList.GetPersistentDataPath(filename);

                byte[] data = File.ReadAllBytes(path);

                Texture2D texture = new Texture2D(0, 0);
                texture.LoadImage(data);

                CreatePreview(texture, filename);
            }
        }

        private async void LoadUsingAsyncReadManager()
        {
            _tokenSource = new CancellationTokenSource();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (string filename in _fileList.Filenames)
            {
                AsyncImageReader reader = new AsyncImageReader();
                string rawDataPath = _fileList.GetRawDataSavePath(filename);
                Texture2D texture = await reader.LoadAsync(rawDataPath, _tokenSource.Token);

                if (_tokenSource.IsCancellationRequested) return;

                CreatePreview(texture, filename);
            }

            sw.Stop();

            Debug.Log($"<<<< {sw.ElapsedMilliseconds}ms >>>>");

            _started = true;
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
            _started = true;
        }

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