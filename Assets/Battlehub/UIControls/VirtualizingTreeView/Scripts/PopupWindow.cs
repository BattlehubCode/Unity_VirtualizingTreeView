using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Linq;

namespace Battlehub.UIControls
{
    [Serializable]
    public class PopupWindowArgs
    {
        public bool Cancel;
    }

    public delegate void PopupWindowAction(PopupWindowArgs args);

    [Serializable]
    public class PopupWindowEvent : UnityEvent<PopupWindowArgs> {  }

    public class PopupWindow : MonoBehaviour
    {
        [SerializeField]
        private PopupWindow Prefab;
        [SerializeField]
        private Text DefaultBody;
        [SerializeField]
        private Text TxtHeader;
        [SerializeField]
        private Transform Body;
        [SerializeField]
        private Button BtnCancel;
        [SerializeField]
        private Button BtnOk;
        [SerializeField]
        private LayoutElement Panel;

        public PopupWindowEvent OK;
        public PopupWindowEvent Cancel;

        private PopupWindowAction m_okCallback;
        private PopupWindowAction m_cancelCallback;

        private PopupWindow[] m_openedPopupWindows;

        private static PopupWindow m_instance;

        private bool m_isOpened;
        public bool IsOpened
        {
            get { return m_isOpened; }
        }

        private void Awake()
        {
            if(Prefab != null)
            {
                m_instance = this;
            }
        }

        private void Start()
        {
            if(BtnCancel != null)
            {
                BtnCancel.onClick.AddListener(OnBtnCancel);
            }
            
            if(BtnOk != null)
            {
                BtnOk.onClick.AddListener(OnBtnOk);
            }
        }

        private void Update()
        {
            if(this == m_instance)
            {
                return;
            }

            if(Input.GetKeyDown(KeyCode.Return))
            {
                OnBtnOk();
            }
            else if(Input.GetKeyDown(KeyCode.Escape))
            {
                OnBtnCancel();
            }
        }
        
        private void OnDestroy()
        {
            if(BtnCancel != null)
            {
                BtnCancel.onClick.RemoveListener(OnBtnCancel);
            }

            if(BtnOk != null)
            {
                BtnOk.onClick.RemoveListener(OnBtnOk);
            }
        }



        private void OnBtnOk()
        {
            if (OK != null)
            {
                PopupWindowArgs args = new PopupWindowArgs();
                OK.Invoke(args);
                if (args.Cancel)
                {
                    return;
                }
            }

            if (m_okCallback != null)
            {
                PopupWindowArgs args = new PopupWindowArgs();
                m_okCallback(args);
                if(args.Cancel)
                {
                    return;
                }
            }

            
            HidePopup();
        }

        private void OnBtnCancel()
        {
            if (Cancel != null)
            {
                PopupWindowArgs args = new PopupWindowArgs();
                Cancel.Invoke(args);
                if (args.Cancel)
                {
                    return;
                }
            }

            if (m_cancelCallback != null)
            {
                PopupWindowArgs args = new PopupWindowArgs();
                m_cancelCallback(args);
                if(args.Cancel)
                {
                    return;
                }
            }
          
            HidePopup();
        }

        private void HidePopup()
        {
            if(m_openedPopupWindows != null)
            {
                foreach (PopupWindow wnd in m_openedPopupWindows)
                {
                    if(wnd != null)
                    {
                        wnd.gameObject.SetActive(true);
                    }
                }
            }
            m_openedPopupWindows = null;

            gameObject.SetActive(false);
            Destroy(gameObject);
            m_okCallback = null;
            m_cancelCallback = null;
            m_isOpened = false;
        }

        private void ShowPopup(string header, Transform body, string ok = null, PopupWindowAction okCallback = null, string cancel = null, PopupWindowAction cancelCallback = null, float width = 500)
        {
            m_openedPopupWindows = FindObjectsOfType<PopupWindow>().Where(
                wnd => wnd.IsOpened && wnd.isActiveAndEnabled).ToArray();
            foreach(PopupWindow wnd in m_openedPopupWindows)
            {
                wnd.gameObject.SetActive(false);
            }

            gameObject.SetActive(true);
            if(TxtHeader != null)
            {
                TxtHeader.text = header;
            }

            if(Body != null)
            {
                body.SetParent(Body, false);
            }

            if(BtnOk != null)
            {
                if (string.IsNullOrEmpty(ok))
                {
                    BtnOk.gameObject.SetActive(false);
                }
                else
                {
                    Text text = BtnOk.GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        text.text = ok;
                    }
                }
            }

            if(BtnCancel != null)
            {
                if(string.IsNullOrEmpty(cancel))
                {
                    BtnCancel.gameObject.SetActive(false);
                }
                else
                {
                    Text text = BtnCancel.GetComponentInChildren<Text>();
                    if(text != null)
                    {
                        text.text = cancel;
                    }
                }
            }
            if(Panel != null)
            {
                Panel.preferredWidth = width;
            }
            m_okCallback = okCallback;
            m_cancelCallback = cancelCallback;
            m_isOpened = true;
        }

        public void Close(bool result)
        {
            if(result)
            {
                OnBtnOk();
            }
            else
            {
                OnBtnCancel();
            }
        }

        public static void Show(string header, string body, string ok, PopupWindowAction okCallback = null, string cancel = null, PopupWindowAction cancelCallback = null, float width = 530)
        {
            if (m_instance == null)
            {
                Debug.LogWarning("PopupWindows.m_instance is null");
                return;
            }
            PopupWindow instance = Instantiate(m_instance.Prefab);
            instance.transform.position = Vector3.zero;
            instance.transform.SetParent(m_instance.transform, false);

            instance.DefaultBody.text = body;
            instance.ShowPopup(header, instance.DefaultBody.transform, ok, okCallback, cancel, cancelCallback, width);
        }

        public static void Show(string header, Transform body, string ok, PopupWindowAction okCallback = null, string cancel = null, PopupWindowAction cancelCallback = null, float width = 530)
        {
            if(m_instance == null)
            {
                Debug.LogWarning("PopupWindows.m_instance is null");
                return;
            }

            PopupWindow instance = Instantiate(m_instance.Prefab);
            instance.transform.position = Vector3.zero;
            instance.transform.SetParent(m_instance.transform, false);

            if (instance.DefaultBody != null)
            {
                Destroy(instance.DefaultBody);
            }
            instance.ShowPopup(header, body, ok, okCallback, cancel, cancelCallback, width);
        }
    }

}
