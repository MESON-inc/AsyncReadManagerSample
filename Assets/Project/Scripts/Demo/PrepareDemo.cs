using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using AsyncReader.Utility;

namespace AsyncReader.Demo
{
    public class PrepareDemo : MonoBehaviour
    {
        [SerializeField] private FileList _fileList;
        [SerializeField] private FileList _fileList2;
        [SerializeField] private ImageIODemo _ioDemo;
        [SerializeField] private GameObject _progress;
        [SerializeField] private Image _progressBar;
        [SerializeField] private Rotater _rotater;

        private int _total = 0;
        private int _count = 0;

        private void Awake()
        {
            Application.targetFrameRate = 60;

            _total = _fileList.Filenames.Length + _fileList2.Filenames.Length;
        }

        private void Start()
        {
            Prepare();
        }

        private async void Prepare()
        {
            await PrepareList(_fileList);
            await PrepareList(_fileList2);

            _progress.SetActive(false);

            _rotater.StartRotate();
            
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

                string savePath = fileList.GetPersistentDataPath(filename);
                Save(texture, savePath);

                _count++;
                
                UpdateProgress();
            }
        }

        private void Save(Texture2D texture, string path)
        {
            byte[] data = texture.EncodeToJPG();
            File.WriteAllBytes(path, data);
        }

        private void UpdateProgress()
        {
            _progressBar.fillAmount = _count / (float)_total;
        }
    }
}