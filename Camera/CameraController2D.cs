using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * State Ideas:
 *  - Default -> Never changes and set once.
 *  - LerpFollow -> Follows an objects transform.
 *  - MoveTo -> Simply tell camera to move to a specific position that can change.
 */
public enum CameraState2D { Default, LerpFollow, MoveTo }

public class CameraController2D : MonoBehaviour
{
    [SerializeField] private CameraState2D _cameraState = CameraState2D.Default;
    private Camera _camera;

    [Header("'Default' State")]
    [SerializeField] private Vector3 _defaultPosition;
    [SerializeField] private float _defaultOrthographicSize;
    [SerializeField] private float _defaultTransitionSpeed;

    [Header("'LerpFollow' State")]
    [SerializeField] private Transform _lerpTarget = null;
    private Vector3 _lerpTargetPosition = new Vector3();
    [SerializeField] private float _lerpOrthographicSize;
    [SerializeField] private float _lerpTransitionSpeed;

    [Header("'MoveTo' State")]
    [SerializeField] private Vector3 _moveToTargetPosition;
    [SerializeField] private float _moveToOrthographicSize;
    [SerializeField] private float _moveToTransitionSpeed;

    private void Awake()
    {
        _camera = this.GetComponent<Camera>();
        _defaultPosition = _camera.transform.position;
        _defaultOrthographicSize = _camera.orthographicSize;
    }
    private void Update()
    {
        if (_lerpTarget)
        {
            _lerpTargetPosition = _lerpTarget.position;
            _lerpTargetPosition.z = _defaultPosition.z;
        }

        switch (_cameraState)
        {
            case CameraState2D.LerpFollow:
                if (!_lerpTarget) break;
                LerpMove(_lerpTargetPosition, _lerpOrthographicSize, _lerpTransitionSpeed);
                break;
            case CameraState2D.MoveTo:
                LerpMove(_moveToTargetPosition, _moveToOrthographicSize, _moveToTransitionSpeed);
                break;
            default:
                LerpMove(_defaultPosition, _defaultOrthographicSize, _defaultTransitionSpeed);
                break;
        }
    }

    private void LerpMove(Vector3 position, float size, float speed)
    {
        _camera.transform.position = Vector3.Lerp(_camera.transform.position, position, speed * Time.deltaTime);
        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, size, speed * Time.deltaTime);
    }

    public void ChangeState(CameraState2D cameraState) { _cameraState = cameraState; }

    public void LerpFollow(Transform lerpTarget) { _lerpTarget = lerpTarget; }

    public void MoveTo(Vector3 moveToTargetPosition, float size) { _moveToTargetPosition = moveToTargetPosition; _moveToOrthographicSize = size; }
}
