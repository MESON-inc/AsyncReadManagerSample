using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace AsyncReader.Demo
{
    public class ImageLoaderTest : MonoBehaviour
    {
        [SerializeField] private string[] _filenames;
        [SerializeField] private Preview _previewPrefab;
        [SerializeField] private Transform _parent;

        private readonly string _directoryName = "ExternalTextures";
        private string _cacheDataPath;
        private string DirectoryPath => $"{_cacheDataPath}/../{_directoryName}";

        private List<Preview> _previews = new List<Preview>();

        private bool _loaded = false;
        private bool _saved = false;
        private SynchronizationContext _context;
        private CancellationTokenSource _tokenSource;

        private void Awake()
        {
            _cacheDataPath = Application.dataPath;
            _context = SynchronizationContext.Current;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                LoadAsync();
                // LoadAsyncOnce();
            }
        }

        private void OnDestroy()
        {
            _tokenSource?.Cancel();
        }

        private void OnGUI()
        {
            if (_loaded)
            {
                if (_saved) return;

                if (GUI.Button(new Rect(50, 50, 300, 150), "Save"))
                {
                    Save();
                }
            }
            else
            {
                if (GUI.Button(new Rect(50, 50, 300, 150), "Load"))
                {
                    Load();
                }

                if (GUI.Button(new Rect(50, 230, 300, 150), "LoadAsync"))
                {
                    LoadAsync();
                }
            }
        }

        private void Load()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (string filename in _filenames)
            {
                string path = $"{DirectoryPath}/{filename}";

                Texture2D texture = new Texture2D(0, 0);
                byte[] data = File.ReadAllBytes(path);
                texture.LoadImage(data);

                CreatePreview(texture, path);
            }

            sw.Stop();

            Debug.Log($"<<<< Load time: {sw.ElapsedMilliseconds.ToString()}ms >>>>");

            _loaded = true;
        }

        private async void LoadAsync()
        {
            _tokenSource = new CancellationTokenSource();

            foreach (string filename in _filenames)
            {
                AsyncImageReader reader = new AsyncImageReader();
                string basePath = $"{DirectoryPath}/{filename}";
                string rawDataPath = GetRawDataSavePath(basePath);

                Texture2D texture = await reader.LoadAsync(rawDataPath, _tokenSource.Token);

                if (_tokenSource.IsCancellationRequested) return;

                CreatePreview(texture, rawDataPath);
            }

            _loaded = true;
        }

        private int _index = 0;

        private async void LoadAsyncOnce()
        {
            _index = (_index + 1) % _filenames.Length;
            string filename = _filenames[_index];
            
            AsyncImageReader reader = new AsyncImageReader();
            string basePath = $"{DirectoryPath}/{filename}";
            string rawDataPath = GetRawDataSavePath(basePath);

            Texture2D texture = await reader.LoadAsync(rawDataPath);

            CreatePreview(texture, rawDataPath);
    }

    private void Save()
    {
        foreach (var preview in _previews)
        {
            string path = GetRawDataSavePath(preview.FilePath);
            ImageSaveTest.Save(preview.Texture, path);
        }

        _saved = true;
    }

    private void CreatePreview(Texture2D texture, string path)
    {
        Preview preview = Instantiate(_previewPrefab);
        preview.SetTexture(texture);
        preview.FilePath = path;
        preview.transform.SetParent(_parent, false);

        _previews.Add(preview);
    }

    private string GetRawDataSavePath(string basePath)
    {
        string directory = Path.GetDirectoryName(basePath);
        string withoutExt = Path.GetFileNameWithoutExtension(basePath);
        return $"{directory}/{withoutExt}_raw.bytes";
    }
}

}