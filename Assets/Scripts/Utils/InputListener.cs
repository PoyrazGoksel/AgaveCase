using Extensions.Unity;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Utils
{
    [UsedImplicitly]
    public class InputListener : IInitializable
    {
        private Camera _mainSceneCamera;
        private RoutineHelper _inputListenerRoutine;

        public void Initialize()
        {
            RegisterEvents();
        }

        private void RegisterEvents()
        {
        }

        /*
        private void OnCameraControllerStart(CameraController cameraController)
        {
            _mainSceneCamera = cameraController.MainSceneCamera;
            _inputListenerRoutine = new RoutineHelper(cameraController, null, InputListenerUpdate);
            _inputListenerRoutine.StartCoroutine();
            Debug.LogWarning("InputListener started.");
        }
        */

        private void InputListenerUpdate()
        {
            /*
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = _mainSceneCamera.nearClipPlane;
            _mainSceneCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(_mainSceneCamera.ScreenPointToRay(mousePosition), out RaycastHit hit))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.collider.TryGetComponent(out IClickable clickable))
                    {
                        clickable.OnClick(0);
                    }
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    if (hit.collider.TryGetComponent(out IClickable clickable))
                    {
                        clickable.OnClick(1);
                    }
                }
            }
        */
        }
    }
}