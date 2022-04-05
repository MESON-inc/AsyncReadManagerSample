using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using AsyncReader.Utility;
using Debug = UnityEngine.Debug;

namespace AsyncReader.Demo
{
    public class PrepareDemo : MonoBehaviour
    {
        [SerializeField] private FileList _fileList;
        [SerializeField] private ImageIODemo _ioDemo;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            Prepare();
        }

        private async void Prepare()
        {
            foreach (string filename in _fileList.Filenames)
            {
                string path = _fileList.GetStreamingAssetsPath(filename);
                
                using UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);

                await request.SendWebRequest();

                Texture2D texture = DownloadHandlerTexture.GetContent(request);

                string rawDataSavePath = _fileList.GetRawDataSavePath(filename);
                SaveAsRawData(texture, rawDataSavePath);
                
                string savePath = _fileList.GetPersistentDataPath(filename);
                Save(texture, savePath);
            }
            
            _ioDemo.StartDemo();
        }
        
        private void Save(Texture2D texture, string path)
        {
            byte[] data = texture.EncodeToJPG();
            File.WriteAllBytes(path, data);
        }
        
        private void SaveAsRawData(Texture2D texture, string path)
        {
            byte[] rawData = texture.GetRawTextureData();
            byte[] encoded = ImageConverter.Encode(rawData, texture.width, texture.height, texture.format);

            Debug.Log($"Saving a byte data to [{path}]");
            
            File.WriteAllBytes(path, encoded);
        }
    }
}