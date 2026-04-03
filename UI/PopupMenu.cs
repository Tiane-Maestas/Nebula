using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Nebula
{
public class PopupMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [SerializeField] private GameObject _popupPrefab;
    [SerializeField] private Vector2 _targetOffset = Vector2.zero;
    [SerializeField] private Vector2 _cursorOffset = new Vector2(10f, 10f);
    [SerializeField] private float _hoverDelay = 0.25f;
    [SerializeField] private string _defaultText = "";

    private static GameObject _popupInstance;
    private static RectTransform _popupRectTransform;
    private static TMP_Text _popupText;
    private static Coroutine _hoverWaitCoroutine;
    private static Canvas _parentCanvas;

    private string _localPopupText;

    void Awake()
    {
        if (_popupPrefab == null)
        {
            Debug.LogError("Popup Prefab is not assigned in PopupMenu script on " + gameObject.name);
            enabled = false;
            return;
        }

        // Initialize the static instance only once
        if (_popupInstance == null)
        {
            _parentCanvas = GetComponentInParent<Canvas>();
            if (_parentCanvas == null)
            {
                Debug.LogError("No Canvas found in parent!");
                enabled = false;
                return;
            }

            _popupInstance = Instantiate(_popupPrefab, _parentCanvas.transform);
            _popupInstance.transform.SetAsLastSibling();
            _popupRectTransform = _popupInstance.GetComponent<RectTransform>();
            
            _popupRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _popupRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _popupText = _popupInstance.GetComponentInChildren<TMP_Text>();
            _popupInstance.SetActive(false);
        }

        _localPopupText = _defaultText;
    }

    private void LateUpdate()
    {
        // If the popup is active, ensure it stays upright regardless of the parent's rotation
        if (_popupInstance != null && _popupInstance.activeSelf)
        {
            _popupInstance.transform.rotation = Quaternion.identity;
        }
    }

    public void SetPopupText(string text) { _localPopupText = text; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ResetHoverTimer(eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (_popupInstance.activeSelf) return;
        ResetHoverTimer(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopHover();
        _popupInstance.SetActive(false);
    }

    private void ResetHoverTimer(Vector2 mousePosition)
    {
        StopHover();
        _hoverWaitCoroutine = StartCoroutine(WaitToShow());
    }

    private void StopHover()
    {
        if (_hoverWaitCoroutine != null) { StopCoroutine(_hoverWaitCoroutine); _hoverWaitCoroutine = null; }
    }

    private IEnumerator WaitToShow()
    {
        yield return new WaitForSeconds(_hoverDelay);

        Vector2 currentMousePos = Input.mousePosition;

        _popupInstance.SetActive(true);
        Canvas.ForceUpdateCanvases(); // Ensure ContentSizeFitter has calculated size

        if (_popupText != null)
            _popupText.SetText(_localPopupText);

        Vector2 pivot = new Vector2(
            currentMousePos.x < (Screen.width / 2f) ? 0f : 1f,
            currentMousePos.y < (Screen.height / 2f) ? 0f : 1f
        );
        _popupRectTransform.pivot = pivot;

        Camera cam = _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentCanvas.transform as RectTransform, 
            currentMousePos, 
            cam, 
            out Vector2 localPoint))
        {
            Vector2 dynamicOffset = new Vector2(
                pivot.x == 0 ? _cursorOffset.x : -_cursorOffset.x,
                pivot.y == 0 ? _cursorOffset.y : -_cursorOffset.y
            );

            _popupRectTransform.anchoredPosition = localPoint + dynamicOffset;            
        }
    }
}
}
