using UnityEngine;

namespace AsyncReader.Demo
{
    public class Rotater : MonoBehaviour
    {
        [SerializeField] private float _speed = 30f;

        private void Update()
        {
            transform.Rotate(new Vector3(0, _speed * Time.deltaTime, 0));
        }
    }
}