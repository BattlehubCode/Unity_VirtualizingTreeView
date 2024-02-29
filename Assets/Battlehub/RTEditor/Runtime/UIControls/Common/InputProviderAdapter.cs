using UnityEngine;

namespace Battlehub.UIControls
{
    public class InputProviderAdapter : InputProvider
    {
        [SerializeField]
        private InputProvider m_inputProvider;

        public InputProvider InputProvider
        {
            get { return m_inputProvider; }
            set { m_inputProvider = value; }
        }

        public override float HorizontalAxis
        {
            get { return m_inputProvider.HorizontalAxis; }
        }

        public override float VerticalAxis
        {
            get { return m_inputProvider.VerticalAxis; }
        }

        public override float HorizontalAxis2
        {
            get { return m_inputProvider.HorizontalAxis2; }
        }

        public override float VerticalAxis2
        {
            get { return m_inputProvider.VerticalAxis2; }
        }

        public override bool IsHorizontalButtonDown
        {
            get { return m_inputProvider.IsHorizontalButtonDown; }
        }

        public override bool IsVerticalButtonDown
        {
            get { return m_inputProvider.IsVerticalButtonDown; }
        }

        public override bool IsHorizontal2ButtonDown
        {
            get { return m_inputProvider.IsHorizontal2ButtonDown; }
        }

        public override bool IsVertical2ButtonDown
        {
            get { return m_inputProvider.IsVertical2ButtonDown; }
        }

        public override bool IsFunctionalButtonPressed
        {
            get { return m_inputProvider.IsFunctionalButtonPressed; }
        }

        public override bool IsFunctional2ButtonPressed
        {
            get { return m_inputProvider.IsFunctional2ButtonPressed; }
        }

        public override bool IsSubmitButtonDown
        {
            get { return m_inputProvider.IsSubmitButtonDown; }
        }

        public override bool IsSubmitButtonUp
        {
            get { return m_inputProvider.IsSubmitButtonUp; }
        }

        public override bool IsCancelButtonDown
        {
            get { return m_inputProvider.IsCancelButtonDown; }
        }

        public override bool IsDeleteButtonDown
        {
            get { return m_inputProvider.IsDeleteButtonDown; }
        }

        public override bool IsSelectAllButtonDown
        {
            get { return m_inputProvider.IsSelectAllButtonDown; }
        }

        public override bool IsAnyKeyDown
        {
            get { return m_inputProvider.IsAnyKeyDown; }
        }

        public override Vector3 MousePosition
        {
            get { return m_inputProvider.MousePosition; }
        }

        public override bool IsMouseButtonDown(int button)
        {
            return m_inputProvider.IsMouseButtonDown(button);
        }

        public override bool IsMousePresent
        {
            get { return m_inputProvider.IsMousePresent; }
        }

        public override bool IsKeyboardPresent
        {
            get { return m_inputProvider.IsKeyboardPresent; }
        }

        public override int TouchCount
        {
            get { return m_inputProvider.TouchCount; }
        }

        public override Touch GetTouch(int i)
        {
            return m_inputProvider.GetTouch(i);
        }

        public override bool IsTouchSupported
        {
            get { return m_inputProvider.IsTouchSupported; }
        }

    }

}
