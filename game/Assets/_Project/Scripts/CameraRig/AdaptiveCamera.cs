using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>
    /// Resizes the orthographic camera so it shows the same visible area on every screen shape, landscape or portrait,
    /// and keeps it updated when the window is resized or the phone is rotated.
    /// </summary>
    // Runs before other scripts' Update, so anything reading the camera's view (such as the enemy spawner) sees this frame's size.
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Camera))]
    public class AdaptiveCamera : MonoBehaviour
    {
        [Tooltip("World units squared shown on screen, whatever its shape. 177.8 is a 16:9 screen at orthographic size 5 (17.8 x 10).")]
        [SerializeField, Min(1f)] private float visibleArea = 1600f / 9f;

        private Camera viewCamera;
        private float appliedAspect;
        private float appliedArea;

        private void Awake()
        {
            viewCamera = GetComponent<Camera>();
            Fit();
        }

        private void Update()
        {
            Fit();
        }

        private void Fit()
        {
            var aspect = viewCamera.aspect;
            // A minimised window can report a zero or NaN aspect; keep the last good size until it is restored.
            if (!(aspect > 0f)) return;
            if (aspect == appliedAspect && visibleArea == appliedArea) return;

            viewCamera.orthographicSize = ViewFit.OrthographicSize(visibleArea, aspect);
            appliedAspect = aspect;
            appliedArea = visibleArea;
        }
    }
}
