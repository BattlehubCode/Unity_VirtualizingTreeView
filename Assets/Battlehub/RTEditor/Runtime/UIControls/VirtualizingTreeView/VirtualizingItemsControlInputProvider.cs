using UnityEngine;

namespace Battlehub.UIControls
{
    public class VirtualizingItemsControlInputProvider : InputProvider
    {
        /// <summary>
        /// Multiselect operation key
        /// </summary>
        public KeyCode MultiselectKey = KeyCode.LeftControl;

        /// <summary>
        /// Rangeselect operation key
        /// </summary>
        public KeyCode RangeselectKey = KeyCode.LeftShift;

        /// <summary>
        /// Select All
        /// </summary>
        public KeyCode SelectAllKey = KeyCode.A;

        /// <summary>
        /// Remove key 
        /// </summary>
        public KeyCode DeleteKey = KeyCode.Delete;

        public override bool IsFunctionalButtonPressed
        {
            get { return Input.GetKey(MultiselectKey); }
        }

        public override bool IsFunctional2ButtonPressed
        {
            get { return Input.GetKey(RangeselectKey); }
        }

        public override bool IsDeleteButtonDown
        {
            get { return Input.GetKeyDown(DeleteKey); }
        }

        public override bool IsSelectAllButtonDown
        {
            get { return Input.GetKeyDown(SelectAllKey); }
        }
    }

}
