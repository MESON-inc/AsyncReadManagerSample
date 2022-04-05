using UnityEngine;
using UnityEngine.UI;

namespace AsyncReader.Demo
{
    public class Preview : MonoBehaviour
    {
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private FileList _fileList;

        public Texture2D Texture => _rawImage.texture as Texture2D;

        public void SetTexture(Texture2D texture)
        {
            _rawImage.texture = texture;
        }

        public void Dispose()
        {
            Destroy(_rawImage.texture);
            _rawImage.texture = null;
            
            Destroy(gameObject);
        }
    }
}