using UnityEngine;

namespace AsyncReader.Demo
{
    public class Rotater : MonoBehaviour
    {
        [SerializeField] private float _speed = 30f;

        private bool _running = false;

        private void Update()
        {
            if (_running)
            {
                transform.Rotate(new Vector3(0, _speed * Time.deltaTime, 0));
            }
        }

        public void StartRotate() => _running = true;
    }
}