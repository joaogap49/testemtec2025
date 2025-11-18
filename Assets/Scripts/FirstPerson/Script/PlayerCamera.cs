using UnityEngine;

namespace Runtime.Script
{
    public struct CameraInput
    {
        public Vector2 Look;
    }
    public class PlayerCamera : MonoBehaviour
    {

        [SerializeField] private float sensitivity = 0.1f;
        [Header("Limites de rotação (graus)")]
        [SerializeField] private float minPitch = -89f; // olhar totalmente para baixo
        [SerializeField] private float maxPitch = 89f;  // olhar totalmente para cima
    
        private Vector3 _eulerAngles;
        public void Initialize(Transform target)
        {
            transform.position = target.position;
            // Inicializa ângulos da câmera a partir do alvo
            _eulerAngles = target.eulerAngles;
            // Normaliza o ângulo X para intervalo -180..180, facilitando o clamp em -89..89
            if (_eulerAngles.x > 180f) _eulerAngles.x -= 360f;
            transform.eulerAngles = _eulerAngles;
        }

        public void UpdateRotation(CameraInput input)
        {
            // Aplica a rotação do mouse/joystick
            _eulerAngles += new Vector3(-input.Look.y, input.Look.x) * sensitivity;

            // Limita o pitch (rotação em X) para impedir giro completo para cima/baixo
            _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, minPitch, maxPitch);

            transform.eulerAngles = _eulerAngles;
        }

        public void UpdatePosition(Transform target)
        {
            transform.position = target.position;
        }
    }
}