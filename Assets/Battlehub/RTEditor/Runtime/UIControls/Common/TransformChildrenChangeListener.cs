using UnityEngine;

namespace Battlehub.UIControls
{
    public delegate void TransformChildrenChanged();

    public class TransformChildrenChangeListener : MonoBehaviour
    {
        public event RectTransformChanged TransformChildrenChanged;

        private void OnTransformChildrenChanged()
        {
            if(TransformChildrenChanged != null)
            {
                TransformChildrenChanged();
            }
        }
    }

}

