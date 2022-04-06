using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using AsyncReader.Utility;
using Debug = UnityEngine.Debug;

namespace AsyncReader.Demo
{
    public class PrepareDemo : MonoBehaviour
    {
        [SerializeField] private FileList _fileList;
        [SerializeField] private FileList _fileList2;
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
            await PrepareList(_fileList);
            await PrepareList(_fileList2);

            _ioDemo.StartDemo();
        }

        private async Task PrepareList(FileList fileList)
        {
            foreach (string filename in fileList.Filenames)
            {
                string path = fileList.GetStreamingAssetsPath(filename);

#if UNITY_EDITOR_OSX
                byte[] data = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(0, 0);
                texture.LoadImage(data);
#else
                using UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);

                await request.SendWebRequest();

                Texture2D texture = DownloadHandlerTexture.GetContent(request);
#endif

                string rawDataSavePath = fileList.GetRawDataSavePath(filename);
                SaveAsRawData(texture, rawDataSavePath);

                string savePath = fileList.GetPersistentDataPath(filename);
                Save(texture, savePath);
            }
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