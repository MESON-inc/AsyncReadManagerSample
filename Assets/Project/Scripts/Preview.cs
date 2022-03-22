using UnityEngine;
using UnityEngine.UI;

namespace AsyncReader.Demo
{
    public class Preview : MonoBehaviour
    {
        [SerializeField] private RawImage _rawImage;

        public void SetTexture(Texture2D texture)
        {
            _rawImage.texture = texture;
        }
    }
}