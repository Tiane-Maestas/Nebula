using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * State Ideas:
 *  - Default -> Never changes and set once.
 *  - Follow -> Follows an object's transform.
 *  - MoveTo -> MoveTo a target position and hold.
 */
namespace Nebula
{
    public enum CameraState2D { Default, Follow, MoveTo }

    [RequireComponent(typeof(Camera))]
    public class CameraController2D : MonoBehaviour
    {
        [SerializeField] private CameraState2D _cameraState = CameraState2D.Default;
        private Camera _camera;

        [Header("'Default' State")]
        [SerializeField] private Vector3 _defaultPosition;
        [SerializeField] private float _defaultOrthographicSize;
        [SerializeField] private float _defaultTransitionTime = 1.0f;
        public AnimationCurve DefaultAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("'Follow State")]
        [SerializeField] private Transform _target = null;
        private Vector3 _targetPosition = new Vector3();
        [SerializeField] private float _followOrthographicSize = 5f;
        [Tooltip("Higher values catch up faster.")]
        public float FollowTransitionSpeed = 5f;
        private Vector3 _followVelocity = Vector3.zero;
        private float _sizeVelocity = 0f;

        [Header("'MoveTo' State")]
        [SerializeField] private Vector3 _moveToTargetPosition;
        [SerializeField] private float _moveToOrthographicSize;
        public float MoveToTransitionTime = 1.0f;
        public AnimationCurve MoveToTimeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve MoveToPositionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Coroutine _movingCoroutine;
        private bool _moving = false;

        public Vector3 DefaultPosition => _defaultPosition;
        public float DefaultOrthographicSize => _defaultOrthographicSize;
        public bool IsMoving => _moving;
        public CameraState2D CurrentState => _cameraState;

        private void Awake()
        {
            _camera = this.GetComponent<Camera>();
            _defaultPosition = _camera.transform.position;
            _defaultOrthographicSize = _camera.orthographicSize;
        }

        private void Update()
        {
            if (_cameraState == CameraState2D.Follow && _target != null)
            {
                _targetPosition = _target.position;
                _targetPosition.z = _defaultPosition.z;
                FollowMove(_targetPosition, _followOrthographicSize, this.FollowTransitionSpeed);
            }
        }

        public void SnapToDefault()
        {
            StopMovingCoroutine();
            _cameraState = CameraState2D.Default;
            _followVelocity = Vector3.zero;
            _sizeVelocity = 0f;

            if (_camera != null)
            {
                _camera.transform.position = _defaultPosition;
                _camera.orthographicSize = _defaultOrthographicSize;
            }
        }

        public void ChangeState(CameraState2D cameraState)
        {
            if (_cameraState == cameraState)
                return;

            _cameraState = cameraState;
            _followVelocity = Vector3.zero;
            _sizeVelocity = 0f;

            switch (_cameraState)
            {
                case CameraState2D.Follow:
                    StopMovingCoroutine();
                    if (_target != null)
                    {
                        _targetPosition = _target.position;
                        _targetPosition.z = _defaultPosition.z;
                    }
                    break;
                case CameraState2D.MoveTo:
                    MoveCamera(_moveToTargetPosition, _moveToOrthographicSize, this.MoveToTransitionTime, this.MoveToPositionCurve ?? this.MoveToTimeCurve);
                    break;
                default:
                    MoveCamera(_defaultPosition, _defaultOrthographicSize, _defaultTransitionTime, this.DefaultAnimationCurve);
                    break;
            }
        }

        public void Follow(Transform target, float followTransitionSpeed = 0.0f)
        {
            _target = target;
            _followVelocity = Vector3.zero;
            _sizeVelocity = 0f;

            if (_target != null)
            {
                _targetPosition = _target.position;
                _targetPosition.z = _defaultPosition.z;
            }

            if (followTransitionSpeed > 0.0f)
                this.FollowTransitionSpeed = followTransitionSpeed;
        }

        public void MoveCameraTo(Vector3 moveToTargetPosition, float moveToOrthographicSize, float moveToTransitionTime = 0.0f)
        {
            _moveToTargetPosition = moveToTargetPosition;
            _moveToOrthographicSize = moveToOrthographicSize;
            if (moveToTransitionTime > 0.0f)
                this.MoveToTransitionTime = moveToTransitionTime;

            MoveCamera(_moveToTargetPosition, _moveToOrthographicSize, this.MoveToTransitionTime, this.MoveToPositionCurve ?? this.MoveToTimeCurve);
        }

        public void Shake(float duration, float magnitude)
        {
            StartCoroutine(RandomShake(duration, magnitude));
        }

        private void FollowMove(Vector3 targetPos, float targetSize, float speed)
        {
            float smoothTime = Mathf.Max(0.01f, 1f / Mathf.Max(speed, 0.01f));
            _camera.transform.position = Vector3.SmoothDamp(_camera.transform.position, targetPos, ref _followVelocity, smoothTime);
            _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, targetSize, ref _sizeVelocity, smoothTime);
        }

        private void MoveCamera(Vector3 targetPosition, float targetSize, float travelTime, AnimationCurve animationCurve)
        {
            StopMovingCoroutine();
            _movingCoroutine = StartCoroutine(MoveTo(targetPosition, targetSize, travelTime, animationCurve));
        }

        private void StopMovingCoroutine()
        {
            if (_moving && _movingCoroutine != null)
            {
                StopCoroutine(_movingCoroutine);
                _movingCoroutine = null;
            }
            _moving = false;
        }

        private IEnumerator MoveTo(Vector3 targetPosition, float targetSize, float travelTime, AnimationCurve animationCurve)
        {
            _moving = true;

            if (travelTime <= 0f)
            {
                _camera.transform.position = targetPosition;
                _camera.orthographicSize = targetSize;
                _moving = false;
                yield break;
            }

            Vector3 startPosition = _camera.transform.position;
            float startSize = _camera.orthographicSize;
            float elapsedTime = 0.0f;

            while (elapsedTime < travelTime)
            {
                float ratio = Mathf.Clamp01(elapsedTime / travelTime);
                float t;

                if (animationCurve != null && animationCurve.keys.Length > 1)
                    t = animationCurve.Evaluate(ratio);
                else
                    t = ratio * ratio * (3f - 2f * ratio); // Cubic SmoothStep (EaseInOut)

                _camera.transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, t);
                _camera.orthographicSize = Mathf.LerpUnclamped(startSize, targetSize, t);

                yield return null;
                elapsedTime += Time.deltaTime;
            }

            _camera.transform.position = targetPosition;
            _camera.orthographicSize = targetSize;
            _moving = false;
        }

        private IEnumerator RandomShake(float duration, float magnitude)
        {
            Vector3 origPos = transform.localPosition;
            float elapsedTime = 0.0f;

            while (elapsedTime < duration)
            {
                float sampleX = Random.Range(-1f, 1f);
                float sampleY = Random.Range(-1f, 1f);

                Vector3 shakeAmount = new Vector3(sampleX, sampleY, 0);
                transform.localPosition = origPos + shakeAmount * magnitude;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = origPos;
        }
    }
}