using UnityEngine;

namespace Smallgantics.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("The target to follow.")]
        public Transform Target;

        [Header("Follow Settings")]
        [Tooltip("The offset from the target. If zero, it will be calculated on Start.")]
        public Vector3 Offset;
        
        [Tooltip("How smoothly the camera follows the target. Lower values are sharper, higher values are smoother.")]
        public float SmoothTime = 0.3f;

        private Vector3 _currentVelocity;

        private void Start()
        {
            if (Target != null && Offset == Vector3.zero)
            {
                Offset = transform.position - Target.position;
            }
        }

        private void LateUpdate()
        {
            if (Target == null) return;

            Vector3 targetPosition = Target.position + Offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, SmoothTime);
        }
    }
}
