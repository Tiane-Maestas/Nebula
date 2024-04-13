using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * State Ideas:
 *  - Default -> Never changes and set once.
 *  - Follow -> Follows an objects transform.
 *  - MoveTo -> MoveTo a target position and hold.
 */
namespace Nebula
{
    public enum CameraState2D { Default, Follow, MoveTo }

    public class CameraController2D : MonoBehaviour
    {
        [SerializeField] private CameraState2D _cameraState = CameraState2D.Default;
        private Camera _camera;

        [Header("'Default' State")]
        [SerializeField] private Vector3 _defaultPosition;
        [SerializeField] private float _defaultOrthographicSize;
        [SerializeField] private float _defaultTransitionTime;

        [Header("'Follow' State (Follow a Target)")]
        [SerializeField] private Transform _target = null;
        private Vector3 _targetPosition = new Vector3();
        [SerializeField] private float _followOrthographicSize;
        public float FollowTransitionTime;

        [Header("'MoveTo' State (MoveTo a Target Position and Hold)")]
        [SerializeField] private Vector3 _moveToTargetPosition;
        [SerializeField] private float _moveToOrthographicSize;
        public float MoveToTransitionTime;

        private void Awake()
        {
            _camera = this.GetComponent<Camera>();
            _defaultPosition = _camera.transform.position;
            _defaultOrthographicSize = _camera.orthographicSize;
        }
        private void Update()
        {
            if ((_cameraState == CameraState2D.Follow) && (_target != null))
            {
                _targetPosition = _target.position;
                _targetPosition.z = _defaultPosition.z;

                if (_targetPosition.x != _lastSetTargetPosition.x || _targetPosition.y != _lastSetTargetPosition.y)
                    MoveCamera(_targetPosition, _followOrthographicSize, FollowTransitionTime);
            }
        }

        public void ChangeState(CameraState2D cameraState)
        {
            if (_cameraState == cameraState)
                return;

            _cameraState = cameraState;
            switch (_cameraState)
            {
                case CameraState2D.Follow:
                    if (!_target) break;
                    MoveCamera(_targetPosition, _followOrthographicSize, FollowTransitionTime);
                    break;
                case CameraState2D.MoveTo:
                    MoveCamera(_moveToTargetPosition, _moveToOrthographicSize, MoveToTransitionTime);
                    break;
                default:
                    MoveCamera(_defaultPosition, _defaultOrthographicSize, _defaultTransitionTime);
                    break;
            }
        }

        public void Follow(Transform target, float followTransitionTime = 0.0f)
        {
            _target = target;
            _targetPosition = _target.position;
            _targetPosition.z = _defaultPosition.z;
            if (followTransitionTime > 0.0f)
                this.FollowTransitionTime = followTransitionTime;
        }

        public void MoveCameraTo(Vector3 moveToTargetPosition, float moveToOrthographicSize, float moveToTransitionTime = 0.0f)
        {
            _moveToTargetPosition = moveToTargetPosition;
            _moveToOrthographicSize = moveToOrthographicSize;
            if (moveToTransitionTime > 0.0f)
                this.MoveToTransitionTime = moveToTransitionTime;

            MoveCamera(_moveToTargetPosition, _moveToOrthographicSize, this.MoveToTransitionTime);
        }

        public void Shake(float duration, float magnitude) { StartCoroutine(RandomShake(duration, magnitude)); }

        private IEnumerator _movingCoroutine; // Keep Reference If Moving! Stop before starting new Move.
        private void MoveCamera(Vector3 targetPosition, float targetSize, float travelTime)
        {
            if (_moving)
                StopCoroutine(_movingCoroutine);
            _movingCoroutine = MoveTo(targetPosition, targetSize, travelTime);
            StartCoroutine(_movingCoroutine);
        }
        private bool _moving = false;
        private Vector3 _lastSetTargetPosition = new Vector3();
        private IEnumerator MoveTo(Vector3 targetPosition, float targetSize, float travelTime)
        {
            _moving = true;

            _lastSetTargetPosition = targetPosition;

            Vector3 startPosition = _camera.transform.position;
            float startSize = _camera.orthographicSize;

            float elapsedTime = 0.0f;
            float interpolationRatio = 0.0f;

            while (elapsedTime < travelTime)
            {
                elapsedTime += Time.deltaTime; // I do this first so that on first frame we move.
                interpolationRatio = elapsedTime / travelTime;
                _camera.transform.position = Vector3.Lerp(startPosition, targetPosition, interpolationRatio);
                _camera.orthographicSize = Mathf.Lerp(startSize, targetSize, interpolationRatio);
                yield return null;
            }

            _camera.transform.position = Vector3.Lerp(startPosition, targetPosition, 1.0f);
            _camera.orthographicSize = Mathf.Lerp(startSize, targetSize, 1.0f);

            _moving = false;
        }

        private IEnumerator RandomShake(float duration, float magnitude)
        {
            Vector3 origPos = transform.localPosition;
            float elapsedTime = 0.0f;

            while (elapsedTime < duration)
            {
                float offset = Random.Range(0, 1000);
                // float x = Mathf.PerlinNoise(Random.Range(-1f, 1f) + offset, Random.Range(-1f, 1f) + offset);
                // float y = Mathf.PerlinNoise(Random.Range(-1f, 1f) + offset, Random.Range(-1f, 1f) + offset);
                // float sampleX = (Random.Range(0.0f, 1.0f) < 0.5f) ? -1.0f * x : x;
                // float sampleY = (Random.Range(0.0f, 1.0f) < 0.5f) ? -1.0f * y : y;
                float sampleX = Random.Range(-1f, 1f);
                float sampleY = Random.Range(-1f, 1f);

                Vector3 shakeAmount = new Vector3(sampleX, sampleY, 0);
                transform.localPosition += shakeAmount * magnitude;

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}