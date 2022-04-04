using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using AsyncReader.Utility;

namespace AsyncReader.Demo
{
    public class ImageIODemo : MonoBehaviour
    {
        [SerializeField] private string[] _filenames;
        [SerializeField] private Preview _previewPrefab;
        [SerializeField] private Transform _parent;

        public string[] ImageFilenameList => _filenames.Select(filename => GetStreamingAssetsPath(filename)).ToArray();
        public string[] ByteFilenameList => _filenames.Select(filename => GetRawDataSavePath(GetStreamingAssetsPath(filename))).ToArray();

        private string _cachedStreamingAssetsPath;
        private string _cachedPersistentDataPath;

        private List<Preview> _previews = new List<Preview>();

        private bool _loaded = false;
        private bool _saved = false;
        private SynchronizationContext _context;
        private CancellationTokenSource _tokenSource;

        private void Awake()
        {
            _cachedStreamingAssetsPath = Application.streamingAssetsPath;
            _cachedPersistentDataPath = Application.persistentDataPath;
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
                LoadUsingAsyncReadManager();
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
                    LoadUsingAsyncReadManager();
                }
            }
        }

        private async void Load()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (string filename in _filenames)
            {
                string path = GetStreamingAssetsPath(filename);

                using UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);

                await request.SendWebRequest();

                Texture2D texture = DownloadHandlerTexture.GetContent(request);

                CreatePreview(texture, filename);
            }

            sw.Stop();

            Debug.Log($"<<<< Load time: {sw.ElapsedMilliseconds.ToString()}ms >>>>");

            _loaded = true;
        }

        private async void LoadUsingAsyncReadManager()
        {
            _tokenSource = new CancellationTokenSource();

            foreach (string filename in _filenames)
            {
                AsyncImageReader reader = new AsyncImageReader();
                string rawDataPath = GetRawDataSavePath(filename);
                Texture2D texture = await reader.LoadAsync(rawDataPath, _tokenSource.Token);

                if (_tokenSource.IsCancellationRequested) return;

                CreatePreview(texture, filename);
            }

            _loaded = true;
        }

        private int _index = 0;

        private void Save()
        {
            foreach (var preview in _previews)
            {
                Save(preview.Texture, preview.Filename);
            }

            _saved = true;
        }

        private void CreatePreview(Texture2D texture, string filename)
        {
            Preview preview = Instantiate(_previewPrefab);
            preview.SetTexture(texture);
            preview.Filename = filename;
            preview.transform.SetParent(_parent, false);

            _previews.Add(preview);
        }
        
        private string GetStreamingAssetsPath(string filename) => $"{_cachedStreamingAssetsPath}/{filename}";
        private string GetPersistentDataPath(string filename) => $"{_cachedPersistentDataPath}/{filename}";

        private string GetRawDataSavePath(string filename)
        {
            string path = GetPersistentDataPath(filename);
            string directory = Path.GetDirectoryName(path);
            string withoutExt = Path.GetFileNameWithoutExtension(path);
            return $"{directory}/{withoutExt}_raw.bytes";
        }
        
        private void Save(Texture2D texture, string filename)
        {
            byte[] rawData = texture.GetRawTextureData();
            byte[] encoded = ImageConverter.Encode(rawData, texture.width, texture.height, texture.format);

            string path = GetRawDataSavePath(filename);

            Debug.Log($"Saving a byte data to [{path}]");
            
            File.WriteAllBytes(path, encoded);
        }
    }
}