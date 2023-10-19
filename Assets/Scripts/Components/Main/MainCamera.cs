using Events.External;
using Extensions.Unity;
using Extensions.Unity.MonoHelper;
using UnityEngine;
using Zenject;

namespace Components.Main
{
    public class MainCamera : EventListenerMono
    {
        [Inject] private GameStateEvents GameStateEvents { get; set; }
        [Inject] private GridEvents GridEvents { get; set; }
        [Inject] private CameraEvents CameraEvents { get; set; }

        
        
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _transform;
        
        private const int RefResolutionX = 1080;
        private const int RefResolutionY = 1920;

        private void Start()
        {
            CameraEvents.MainCamStart?.Invoke(_camera);
        }

        protected override void RegisterEvents()
        {
            GridEvents.GridStart += OnGridStart;
        }

        private void OnGridStart(Bounds arg0)
        {
            transform.position = arg0.center;
            transform.Z(-10f);
            FitGridToCamera();
        }

        private void FitGridToCamera()
        {
            float refAspect = (float) RefResolutionX / RefResolutionY;
            float aspect = (float) Screen.width / Screen.height;
            float orthoSize = _camera.orthographicSize;

            if (aspect < refAspect)
            {
                orthoSize *= refAspect / aspect;
            }

            _camera.orthographicSize = orthoSize;
        }
        protected override void UnRegisterEvents()
        {
            GridEvents.GridStart -= OnGridStart;
        }
    }
}
