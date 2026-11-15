using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>Sizes the camera's view so it always covers the same visible area, whatever the screen's shape.</summary>
    public static class ViewFit
    {
        /// <summary>
        /// Returns the orthographic size (half the view's height) that shows <paramref name="visibleArea"/> square world units
        /// on a screen of the given <paramref name="aspect"/> (width / height).
        /// </summary>
        public static float OrthographicSize(float visibleArea, float aspect)
        {
            if (visibleArea <= 0f) throw new ArgumentOutOfRangeException(nameof(visibleArea), visibleArea, "Must be positive.");
            if (aspect <= 0f) throw new ArgumentOutOfRangeException(nameof(aspect), aspect, "Must be positive.");

            // width * height = area and width = height * aspect, so height = sqrt(area / aspect).
            var height = (float)Math.Sqrt(visibleArea / aspect);
            return height / 2f;
        }
    }
}
