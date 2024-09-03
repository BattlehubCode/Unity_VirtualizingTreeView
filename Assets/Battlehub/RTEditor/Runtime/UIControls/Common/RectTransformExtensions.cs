using UnityEngine;

namespace Battlehub.UIControls
{
    public static class RectTransformExtensions
    {
        public static void TopLeft(this RectTransform rt)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            SetPivot(rt, new Vector2(0, 1));
        }

        public static void TopLeft(this RectTransform rt, Vector2 position, Vector2 size)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            SetPivot(rt, new Vector2(0, 1));

            rt.sizeDelta = size;
            rt.anchoredPosition = position;
        }

        public static void Stretch(this RectTransform rt)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void SetAnchors(this RectTransform rt, Vector2 anchorMin, Vector2 anchorMax)
        {
            Vector3 lp = rt.localPosition;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.localPosition = lp;
        }

        public static void SetPivot(this RectTransform rt, Vector2 pivot)
        {
            Vector3 deltaPosition = rt.pivot - pivot;    // get change in pivot
            deltaPosition.Scale(rt.rect.size);           // apply sizing
            deltaPosition.Scale(rt.localScale);          // apply scaling
            deltaPosition = rt.rotation * deltaPosition; // apply rotation

            rt.pivot = pivot;                            // change the pivot
            rt.localPosition -= deltaPosition;           // reverse the position change
        }


        public static void CopyTo(this RectTransform rt, RectTransform target)
        {
            target.anchorMin = rt.anchorMin;
            target.anchorMax = rt.anchorMax;
            target.anchoredPosition = rt.anchoredPosition;
            target.sizeDelta = rt.sizeDelta;
        }

    }
}
