using UnityEngine;
using UnityEngine.Networking;
using AsyncReader.Utility;

namespace AsyncReader.Demo
{
    public class Demo : MonoBehaviour
    {
        [SerializeField] private ImageIODemo ioDemo;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            // Test();
        }

        private async void Test()
        {
            foreach (string path in ioDemo.ImageFilenameList)
            {
                using UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);

                await request.SendWebRequest();

                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Debug.Log(texture.width);
            }
        }
    }
}