using System.Collections.Generic;
using UnityEngine;

namespace Nebula
{
    [CreateAssetMenu(fileName = "SoundConfig", menuName = "Nebula/Sounds/Configuration")]
    public class SoundConfig : ScriptableObject
    {
        public List<Sound> Sounds;
    }
}
