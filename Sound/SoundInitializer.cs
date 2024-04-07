using UnityEngine;

namespace Nebula
{
    public class SoundInitializer : MonoBehaviour
    {
        private static bool _initialized = false; // Only init on first scene awake. Sounds should persist accros scenes.
        [SerializeField] private SoundConfig _config;
        private void Awake()
        {
            if (!_initialized)
                SoundManager.LoadSoundConfiguration(_config);

            _initialized = true;
        }
    }
}
