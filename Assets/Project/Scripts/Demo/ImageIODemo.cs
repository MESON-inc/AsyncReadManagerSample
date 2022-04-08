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
        [SerializeField] private GameObject _ui;

        private List<Preview> _previews = new List<Preview>();

        private CancellationTokenSource _tokenSource;

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
            foreach (string filename in fileList.Filenames)
            {
                string path = fileList.GetPersistentDataPath(filename);

                byte[] data = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(0, 0);
                texture.LoadImage(data);

                CreatePreview(texture, filename);
            }
        }

        private async void LoadAsync(FileList fileList)
        {
            foreach (string filename in fileList.Filenames)
            {
                string path = fileList.GetRawDataSavePath(filename);

                using AsyncFileReader reader = new AsyncFileReader();
                (IntPtr ptr, long size) = await reader.LoadAsync(path);

                ImageInfo info = ImageConverter.Decode(ptr, (int)size);

                Texture2D texture = new Texture2D(info.header.width, info.header.height, info.header.Format, false);
                texture.LoadRawTextureData(info.buffer, info.fileSize);
                texture.Apply();

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