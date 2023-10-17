using UnityEngine.Events;

namespace Events.External
{
    public class PlayerEvents
    {
        public UnityAction<int> PlayerLevelChanged;
        public UnityAction NewPlayerCreated;
        public UnityAction PlayerLoaded;
        public UnityAction<int> PlayerCurrencyChanged;
    }
}