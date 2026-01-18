using UnityEngine;
using UnityEngine.EventSystems;
namespace Nebula
{
    public enum SlidingDirections { Up, Down, Left, Right }

    public class SlidingTab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Movement Settings")]
        [SerializeField] private SlidingDirections _slideDirection;
        [SerializeField] private float _animationSpeed = 10.0f;
        [SerializeField] private float _finalAnchorXorY = 0.0f;

        // Hidden Variables
        private bool _mouseOver = false;
        private RectTransform _rectTransform;
        private float _startAnchorXorY = 0.0f;


        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if ((_slideDirection == SlidingDirections.Up || _slideDirection == SlidingDirections.Down))
                _startAnchorXorY = _rectTransform.anchoredPosition.y;
            else
                _startAnchorXorY = _rectTransform.anchoredPosition.x;
        }
        private void Update()
        {
            if (_mouseOver)
                MoveTowardsFinalAnchor();
            else
                MoveTowardsStartAnchor();
        }

        private void MoveTowardsFinalAnchor()
        {
            switch (_slideDirection)
            {
                case SlidingDirections.Up:
                    if (_rectTransform.anchoredPosition.y < _finalAnchorXorY)
                        _rectTransform.anchoredPosition += Time.deltaTime * _animationSpeed * Vector2.up;
                    break;
                case SlidingDirections.Down:
                    if (_rectTransform.anchoredPosition.y > _finalAnchorXorY)
                        _rectTransform.anchoredPosition += Time.deltaTime * _animationSpeed * Vector2.down;
                    break;
                case SlidingDirections.Left:
                    if (_rectTransform.anchoredPosition.x > _finalAnchorXorY)
                        _rectTransform.anchoredPosition += Time.deltaTime * _animationSpeed * Vector2.left;
                    break;
                case SlidingDirections.Right:
                    if (_rectTransform.anchoredPosition.x < _finalAnchorXorY)
                        _rectTransform.anchoredPosition += Time.deltaTime * _animationSpeed * Vector2.right;
                    break;
            }
        }

        private void MoveTowardsStartAnchor()
        {
            switch (_slideDirection)
            {
                case SlidingDirections.Up:
                    if (_rectTransform.anchoredPosition.y > _startAnchorXorY)
                        _rectTransform.anchoredPosition += Time.deltaTime * _animationSpeed * Vector2.down;
                    break;
                case SlidingDirections.Down:
                    if (_rectTransform.anchoredPosition.y < _startAnchorXorY)
                        _rectTransform.anchoredPosition += Time.deltaTime * _animationSpeed * Vector2.up;
                    break;
                case SlidingDirections.Left:
                    if (_rectTransform.anchoredPosition.x < _startAnchorXorY)
                        _rectTransform.anchoredPosition += Time.deltaTime * _animationSpeed * Vector2.right;
                    break;
                case SlidingDirections.Right:
                    if (_rectTransform.anchoredPosition.x > _startAnchorXorY)
                        _rectTransform.anchoredPosition += Time.deltaTime * _animationSpeed * Vector2.left;
                    break;
            }
        }

        public void OnPointerEnter(PointerEventData eventData) { _mouseOver = true; }
        public void OnPointerExit(PointerEventData eventData) { _mouseOver = false; }
    }
}