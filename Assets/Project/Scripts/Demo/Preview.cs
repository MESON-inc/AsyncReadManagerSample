using UnityEngine;
using UnityEngine.UI;

namespace AsyncReader.Demo
{
    public class Preview : MonoBehaviour
    {
        [SerializeField] private RawImage _rawImage;

        public Texture2D Texture => _rawImage.texture as Texture2D;
        public string Filename { get; set; }

        public void SetTexture(Texture2D texture)
        {
            _rawImage.texture = texture;
        }
    }
}