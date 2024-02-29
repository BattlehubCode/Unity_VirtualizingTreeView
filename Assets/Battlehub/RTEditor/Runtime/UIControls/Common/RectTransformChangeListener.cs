using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    public delegate void RectTransformChanged();

    public class RectTransformChangeListener : UIBehaviour
    {
        public event RectTransformChanged RectTransformChanged;

        protected override void OnRectTransformDimensionsChange()
        {
            RaiseRectTransformChanged();
        }

        public void RaiseRectTransformChanged()
        {
            if (RectTransformChanged != null)
            {
                RectTransformChanged();
            }
        }
    }

}
