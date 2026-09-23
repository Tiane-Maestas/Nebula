using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Nebula
{
    [AddComponentMenu("Nebula/UI/Modal Trigger")]
    public class ModalTrigger : MonoBehaviour
    {
        [Header("Modal Prefab")]
        [Tooltip("The Modal dialog prefab to instantiate.")]
        [SerializeField] private GameObject _modalPrefab;
        [Tooltip("Optional parent Transform to spawn under. If null, finds the active Canvas.")]
        [SerializeField] private Transform _canvasParent;

        [Header("Modal Content")]
        [SerializeField] private string _title = "Leave Game";
        [TextArea(2, 4)]
        [SerializeField] private string _message = "Are you sure you would like to leave?";
        [SerializeField] private string _confirmText = "Leave";
        [SerializeField] private string _cancelText = "Cancel";

        [Header("Events")]
        [Tooltip("Invoked when the confirm button is clicked.")]
        [SerializeField] private UnityEvent _onConfirmed;
        [Tooltip("Invoked when the cancel or backdrop button is clicked, or Escape is pressed.")]
        [SerializeField] private UnityEvent _onCancelled;

        [Header("Behavior Options")]
        [Tooltip("If true, destroys the instantiated modal instance when closed. Otherwise deactivates it.")]
        [SerializeField] private bool _destroyOnClose = true;
        [Tooltip("If true, pressing the Escape key will cancel and close the modal.")]
        [SerializeField] private bool _closeOnEscape = true;
        [Tooltip("If true, clicking a component named 'Backdrop' or 'CloseButton' will cancel and close the modal.")]
        [SerializeField] private bool _closeOnBackdropClick = true;

        public GameObject ModalPrefab { get => _modalPrefab; set => _modalPrefab = value; }
        public string Title { get => _title; set => _title = value; }
        public string Message { get => _message; set => _message = value; }
        public string ConfirmText { get => _confirmText; set => _confirmText = value; }
        public string CancelText { get => _cancelText; set => _cancelText = value; }
        public UnityEvent OnConfirmed => _onConfirmed;
        public UnityEvent OnCancelled => _onCancelled;

        private Button _triggerButton;
        private GameObject _currentInstance;

        public bool IsOpen => _currentInstance != null && _currentInstance.activeInHierarchy;

        private void Awake()
        {
            _triggerButton = GetComponent<Button>();
            if (_triggerButton != null)
            {
                _triggerButton.onClick.AddListener(TriggerModal);
            }
        }

        private void OnDestroy()
        {
            if (_triggerButton != null)
            {
                _triggerButton.onClick.RemoveListener(TriggerModal);
            }

            if (_currentInstance != null && _destroyOnClose)
            {
                Destroy(_currentInstance);
            }
        }

        private void Update()
        {
            if (IsOpen && _closeOnEscape && Input.GetKeyDown(KeyCode.Escape))
            {
                OnCancelClicked();
            }
        }

        public void TriggerModal()
        {
            if (IsOpen)
            {
                _currentInstance.transform.SetAsLastSibling();
                return;
            }

            if (_modalPrefab == null)
            {
                Debug.LogWarning("[ModalTrigger] No Modal prefab assigned! Executing confirmed event directly.", this);
                _onConfirmed?.Invoke();
                return;
            }

            Transform parent = GetCanvasParent();
            _currentInstance = Instantiate(_modalPrefab, parent);
            _currentInstance.name = _modalPrefab.name;
            _currentInstance.SetActive(true);

            RectTransform rectTransform = _currentInstance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }

            WireModalComponents(_currentInstance);
        }

        private void WireModalComponents(GameObject instance)
        {
            // 1. Title Text
            Transform titleTransform = FindChildByName(instance.transform, "TitleText", "Title Text", "Title");
            if (titleTransform != null)
            {
                bool hasTitle = !string.IsNullOrEmpty(_title);
                titleTransform.gameObject.SetActive(hasTitle);
                if (hasTitle)
                {
                    TrySetText(titleTransform, _title);
                }
            }

            // 2. Message Text
            Transform messageTransform = FindChildByName(instance.transform, "MessageText", "Message Text", "Message", "Prompt");
            if (messageTransform != null)
            {
                TrySetText(messageTransform, _message);
            }

            // 3. Confirm Button
            Transform confirmTransform = FindChildByName(instance.transform, "ConfirmButton", "Confirm Button", "Confirm", "BtnConfirm");
            if (confirmTransform != null)
            {
                Button confirmBtn = confirmTransform.GetComponent<Button>();
                if (confirmBtn != null)
                {
                    confirmBtn.onClick.AddListener(OnConfirmClicked);
                }

                if (!string.IsNullOrEmpty(_confirmText))
                {
                    TrySetButtonLabel(confirmTransform, _confirmText);
                }
            }

            // 4. Cancel Button
            Transform cancelTransform = FindChildByName(instance.transform, "CancelButton", "Cancel Button", "Cancel", "BtnCancel");
            if (cancelTransform != null)
            {
                Button cancelBtn = cancelTransform.GetComponent<Button>();
                if (cancelBtn != null)
                {
                    cancelBtn.onClick.AddListener(OnCancelClicked);
                }

                if (!string.IsNullOrEmpty(_cancelText))
                {
                    TrySetButtonLabel(cancelTransform, _cancelText);
                }
            }

            // 5. Optional Backdrop or Close Button
            if (_closeOnBackdropClick)
            {
                Transform backdropTransform = FindChildByName(instance.transform, "Backdrop", "CloseButton", "Close Button", "Close");
                if (backdropTransform != null)
                {
                    Button backdropBtn = backdropTransform.GetComponent<Button>();
                    if (backdropBtn != null)
                    {
                        backdropBtn.onClick.AddListener(OnCancelClicked);
                    }
                }
            }
        }

        private void OnConfirmClicked()
        {
            CloseModal();
            _onConfirmed?.Invoke();
        }

        private void OnCancelClicked()
        {
            CloseModal();
            _onCancelled?.Invoke();
        }

        public void CloseModal()
        {
            if (_currentInstance != null)
            {
                if (_destroyOnClose)
                {
                    Destroy(_currentInstance);
                }
                else
                {
                    _currentInstance.SetActive(false);
                }
                _currentInstance = null;
            }
        }

        private Transform GetCanvasParent()
        {
            if (_canvasParent != null)
                return _canvasParent;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                return canvas.transform;

            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas c in canvases)
            {
                if (c.isActiveAndEnabled && c.isRootCanvas)
                    return c.transform;
            }

            if (canvases.Length > 0)
                return canvases[0].transform;

            return null;
        }

        private static Transform FindChildByName(Transform parent, params string[] candidateNames)
        {
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                string cleanChildName = NormalizeName(child.name);
                foreach (string candidate in candidateNames)
                {
                    if (cleanChildName == NormalizeName(candidate))
                    {
                        return child;
                    }
                }
            }
            return null;
        }

        private static string NormalizeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            return name.Replace(" ", "").Replace("_", "").ToLowerInvariant();
        }

        private static bool TrySetButtonLabel(Transform buttonTransform, string text)
        {
            TMP_Text tmp = buttonTransform.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
            {
                tmp.text = text;
                return true;
            }

            Text uiText = buttonTransform.GetComponentInChildren<Text>(true);
            if (uiText != null)
            {
                uiText.text = text;
                return true;
            }

            return false;
        }

        private static bool TrySetText(Transform target, string text)
        {
            if (target == null) return false;

            TMP_Text tmp = target.GetComponent<TMP_Text>();
            if (tmp == null)
            {
                tmp = target.GetComponentInChildren<TMP_Text>(true);
            }

            if (tmp != null)
            {
                tmp.text = text ?? string.Empty;
                return true;
            }

            Text uiText = target.GetComponent<Text>();
            if (uiText == null)
            {
                uiText = target.GetComponentInChildren<Text>(true);
            }

            if (uiText != null)
            {
                uiText.text = text ?? string.Empty;
                return true;
            }

            return false;
        }
    }
}
