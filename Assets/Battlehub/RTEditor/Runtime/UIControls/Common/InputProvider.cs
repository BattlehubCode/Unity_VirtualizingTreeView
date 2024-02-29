using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.UIControls
{
    public interface IUpdateFocusedHandler : IEventSystemHandler
    {
        void OnUpdateFocused(BaseEventData eventData);
    }

    public class InputProvider : MonoBehaviour
    {
        public virtual float HorizontalAxis
        {
            get
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    return -1;
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    return 1;
                }
                return 0;
            }
        }

        public virtual float VerticalAxis
        {
            get
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    return 1;
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    return -1;
                }
                return 0;
            }
        }

        public virtual float HorizontalAxis2
        {
            get
            {
                if (Input.GetKey(KeyCode.Keypad4))
                {
                    return -1;
                }
                else if (Input.GetKey(KeyCode.Keypad6))
                {
                    return 1;
                }
                return 0;
            }
        }

        public virtual float VerticalAxis2
        {
            get
            {
                if (Input.GetKey(KeyCode.Keypad8))
                {
                    return 1;
                }
                else if (Input.GetKey(KeyCode.Keypad2))
                {
                    return -1;
                }
                return 0;
            }
        }

        public virtual bool IsHorizontalButtonDown
        {
            get { return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow); }
        }

        public virtual bool IsVerticalButtonDown
        {
            get { return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow); }
        }

        public virtual bool IsHorizontal2ButtonDown
        {
            get { return Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Keypad6); }
        }

        public virtual bool IsVertical2ButtonDown
        {
            get { return Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Keypad2); }
        }

        public virtual bool IsFunctionalButtonPressed
        {
            get { return Input.GetKey(KeyCode.LeftControl); }
        }

        public virtual bool IsFunctional2ButtonPressed
        {
            get { return Input.GetKey(KeyCode.LeftShift); }
        }

        public virtual bool IsSubmitButtonDown
        {
            get { return Input.GetKeyDown(KeyCode.Return); }
        }

        public virtual bool IsSubmitButtonUp
        {
            get { return Input.GetKeyUp(KeyCode.Return); }
        }

        public virtual bool IsCancelButtonDown
        {
            get { return Input.GetKeyDown(KeyCode.Escape); }
        }

        public virtual bool IsDeleteButtonDown
        {
            get { return Input.GetKeyDown(KeyCode.Delete); }
        }

        public virtual bool IsSelectAllButtonDown
        {
            get { return Input.GetKeyDown(KeyCode.A); }
        }

        public virtual bool IsAnyKeyDown
        {
            get { return Input.anyKeyDown; }
        }

        public virtual Vector3 MousePosition
        {
            get { return Input.mousePosition; }
        }

        public virtual bool IsMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button);
        }

        public virtual bool IsMousePresent
        {
            get { return Input.mousePresent; }
        }

        public virtual bool IsKeyboardPresent
        {
            get { return true; }
        }
       
        public virtual int TouchCount
        {
            get { return Input.touchCount; }
        }

        public virtual Touch GetTouch(int i)
        {
            return Input.GetTouch(i);
        }

        public virtual bool IsTouchSupported
        {
            get { return Input.touchSupported; }
        }

    }
}
