using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Events.External
{
    [UsedImplicitly]
    public class CameraEvents
    {
        public UnityAction<Camera> MainCamStart;
    }
}