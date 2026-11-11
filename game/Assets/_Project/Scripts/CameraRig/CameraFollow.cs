using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>Keeps the camera centred on a target in the XY plane.</summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;

        // LateUpdate runs after the target's interpolated transform has been written for this frame,
        // so the camera never lags one frame behind or jitters against the player.
        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var position = target.position;
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
    }
}
