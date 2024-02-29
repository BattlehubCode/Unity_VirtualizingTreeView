using UnityEngine;

namespace Battlehub.UIControls
{
    public static class RectTransformExtensions
    {
        public static void Stretch(this RectTransform rt)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
