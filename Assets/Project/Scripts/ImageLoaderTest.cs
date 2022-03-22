using System.IO;
using UnityEngine;

namespace AsyncReader.Demo
{
    public class ImageLoaderTest : MonoBehaviour
    {
        [SerializeField] private string[] _filenames;
        [SerializeField] private Preview _previewPrefab;
        [SerializeField] private Transform _parent;

        private readonly string _directoryName = "ExternalTextures";
        private string DirectoryPath => $"{Application.dataPath}/../{_directoryName}";

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }
        }

        private void Load()
        {
            foreach (string filename in _filenames)
            {
                string path = $"{DirectoryPath}/{filename}";

                Texture2D texture = new Texture2D(0, 0);
                byte[] data = File.ReadAllBytes(path);
                texture.LoadImage(data);

                Preview preview = Instantiate(_previewPrefab);
                preview.SetTexture(texture);
                preview.transform.SetParent(_parent, false);
            }
        }
    }
}