using System.IO;
using UnityEngine;

namespace AsyncReader.Demo
{
    [CreateAssetMenu(fileName = nameof(FileList), menuName = "AsyncReader/" + nameof(FileList))]
    public class FileList : ScriptableObject
    {
        [SerializeField] private string[] _filenames;

        public string[] Filenames => _filenames;

        private string _cachedStreamingAssetsPath;
        private string _cachedPersistentDataPath;

        private void OnEnable()
        {
            _cachedStreamingAssetsPath = Application.streamingAssetsPath;
            _cachedPersistentDataPath = Application.persistentDataPath;
        }

        public string GetStreamingAssetsPath(string filename) => $"{_cachedStreamingAssetsPath}/{filename}";
        public string GetPersistentDataPath(string filename) => $"{_cachedPersistentDataPath}/{filename}";

        public string GetRawDataSavePath(string filename)
        {
            string path = GetPersistentDataPath(filename);
            string directory = Path.GetDirectoryName(path);
            string withoutExt = Path.GetFileNameWithoutExtension(path);
            return $"{directory}/{withoutExt}_raw.bytes";
        }
    }
}