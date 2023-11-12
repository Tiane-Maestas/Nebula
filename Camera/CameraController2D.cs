using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraState2D { Default, LerpFollow }

public class CameraController2D : MonoBehaviour
{
    [SerializeField] private CameraState2D _cameraState = CameraState2D.Default;
    private Camera _camera;

    [Header("Default State")]
    [SerializeField] private Vector3 _defaultPosition;
    [SerializeField] private float _defaultOrthographicSize;
    [SerializeField] private float _defaultTransitionSpeed;

    [Header("LerpFollow State")]
    [SerializeField] private Transform _lerpTarget = null;
    private Vector3 _lerpTargetPosition = new Vector3();
    [SerializeField] private float _lerpOrthographicSize;

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
                _camera.transform.position = Vector3.Lerp(_camera.transform.position, _lerpTargetPosition, _defaultTransitionSpeed * Time.deltaTime);
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _lerpOrthographicSize, _defaultTransitionSpeed * Time.deltaTime);
                break;
            default:
                _camera.transform.position = Vector3.Lerp(_camera.transform.position, _defaultPosition, _defaultTransitionSpeed * Time.deltaTime);
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _defaultOrthographicSize, _defaultTransitionSpeed * Time.deltaTime);
                break;
        }
    }

    public void ChangeState(CameraState2D cameraState) { _cameraState = cameraState; }

    public void LerpFollow(Transform lerpTarget) { _lerpTarget = lerpTarget; }
}
