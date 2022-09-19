using System.Collections.Generic;
using System;
using UnityEngine;

namespace Nebula
{
    // SoundType Notes: 
    // -Conditional types don't loop, non-conditional types loop.
    // -Background types will be heard from anywhere, Ambient types are proximity.
    // -Ambient types are destroyed once stopped, background types always have an audio source.
    // -(DO NOT USE A '*' CHARACTER IN SOUND NAMES) Ambient sounds need this special character.
    public enum SoundType
    {
        None, // Functions like 'background' but has no specified sound type.
        Background, // Constant background sound to always play at a constant volume to player. (Ex. Background music)
        Ambient, // Enviornment sound that is dependent on player location and source location. (Ex. Fire)
        ConditionalBackground, // Same as 'background' but is frequently changed. (Ex. Player's own footsteps)
        ConditionalAmbient // Same as 'ambient' but will only play once. (Ex. Explosion)
    }
    public struct Sound
    {
        public string name;
        public SoundType type;
        public string fileLocation;
        public float spacialBlend;
        public float maxAudibleDistance;
        public Sound(string name, SoundType type, string fileLocation, float spacialBlend, float maxAudibleDistance)
        {
            this.name = name;
            this.type = type;
            this.fileLocation = fileLocation;
            this.spacialBlend = spacialBlend;
            this.maxAudibleDistance = maxAudibleDistance;
        }

        public override string ToString() => $"({name}, {type}, {fileLocation})";
    }
    public static class SoundManager
    {
        // This is a dictionary of all sounds with their names as keys. (Holds Information Only)
        private static Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();
        // This is a dictionary between sound names and the game objects that play the audio.
        private static Dictionary<string, AudioSource> audioPlayers = new Dictionary<string, AudioSource>();

        private static Vector3 defaultSoundPosition = new Vector3(0, 0, 0);

        // Global volume controls.
        public static float[] volumeMultipliers = { 1, 1, 1, 1, 1 };

        // Ambient Sounds must be handled uniquely.
        public static string AmbientSuffix = "*Base";
        private static Dictionary<string, int> ambientSoundsCounts = new Dictionary<string, int>();

        #region Data Handling

        public static void AddSound(string name, SoundType type, string fileLocation, float maxAudibleDistance = 10, float spacialBlend = 1)
        {
            // Check for ambient.
            string ambientName = name + SoundManager.AmbientSuffix;
            if (sounds.ContainsKey(name) || sounds.ContainsKey(ambientName))
            {
                Debug.LogError("Couldn't Add Sound. Already Exists as " + sounds[name]);
                return;
            }

            // Handle the various types of sounds differently. 
            if (Array.Exists(backgroundSoundTypes, element => element == type)) // All background types.
            {
                // Make sure spacial blend here is 0 because it is of some background type. (Should always be heard)
                sounds.Add(name, new Sound(name, type, fileLocation, 0, maxAudibleDistance));
                // Only create an audio source here if the sound is a background type becasue they only need one audio source.
                CreateAudioSource(name, defaultSoundPosition);
            }
            else if (type == SoundType.Ambient)
            {
                sounds.Add(ambientName, new Sound(name, type, fileLocation, spacialBlend, maxAudibleDistance));
                ambientSoundsCounts[ambientName] = 0;
            }
            else
            {
                sounds.Add(name, new Sound(name, type, fileLocation, spacialBlend, maxAudibleDistance));
            }
        }

        public static bool ContainsSound(string name)
        {
            return sounds.ContainsKey(name);
        }

        #endregion

        #region Background SoundTypes
        private static SoundType[] backgroundSoundTypes = { SoundType.None, SoundType.Background, SoundType.ConditionalBackground };

        // This method is only for background type sounds because they don't depend on a location.
        public static void PlaySound(string name, float volume = 1f)
        {
            // Can't play a sound that doesn't exist.
            if (!sounds.ContainsKey(name))
            {
                Debug.LogError($"Couldn't Play Sound. Doesn't Exists.  ({name})");
                return;
            }

            // Only play the sound if it is a supported sound type for this method.
            if (Array.Exists(backgroundSoundTypes, element => element == sounds[name].type))
            {
                // Any sound of this type will already have an audio source constructed.
                PlayAudioFromSource(name, volume);
                return; // Successful exit of method here.
            }
            Debug.LogError($"SoundType with given name ({name}) isn't supported by this play type. Try including a position.");
        }

        #endregion

        #region Ambient SoundTypes

        private static SoundType[] ambientSoundTypes = { SoundType.Ambient, SoundType.ConditionalAmbient };

        // This method is only for ambient type sounds because they depend on a location.
        public static string PlaySound(string name, Vector3 position, float volume = 1f)
        {
            // Can't play a sound that doesn't exist.
            if (!sounds.ContainsKey(name))
            {
                // Check if it is ambient.
                string ambientName = name + SoundManager.AmbientSuffix;
                if (sounds.ContainsKey(ambientName))
                {
                    name = ambientName;
                }
                else
                {
                    Debug.LogError($"Couldn't Play Sound. Doesn't Exists.  ({name})");
                    return null;
                }
            }

            // Only play the sound if it is a supported sound type for this method.
            if (Array.Exists(ambientSoundTypes, element => element == sounds[name].type))
            {
                // Any sound of this type will need an audio source constructed.
                name = CreateAudioSource(name, position);
                PlayAudioFromSource(name, volume);

                return name; // Successful exit of method here.
            }
            Debug.LogError($"SoundType with given name ({name}) isn't supported by this play type.");
            return null;
        }

        #endregion

        #region Stopping A Sound

        // Stopping a sound must use the sound name or returned name if the sound is ambient.
        public static void StopSound(string name)
        {
            bool isAmbient = name.Contains('*');
            // Can't stop a sound that doesn't exist.
            if (!sounds.ContainsKey(name) && !isAmbient)
            {
                Debug.LogError($"Couldn't Stop Sound. Doesn't Exists. ({name})");
                return;
            }

            if (!audioPlayers.ContainsKey(name))
            {
                Debug.LogError($"Couldn't Stop Sound. No Audio Player Exists. ({name})");
                return;
            }

            audioPlayers[name].Stop();
        }

        public static void StopSoundAndDestroyAudioPlayer(string name)
        {
            bool isAmbient = name.Contains('*');
            // Can't stop a sound that doesn't exist.
            if (!sounds.ContainsKey(name) && !isAmbient)
            {
                Debug.LogError($"Couldn't Stop Sound. Doesn't Exists. ({name})");
                return;
            }

            if (!audioPlayers.ContainsKey(name))
            {
                Debug.LogError($"Couldn't Stop Sound. No Audio Player Exists. ({name})");
                return;
            }

            audioPlayers[name].Stop();
            GameObject.Destroy(audioPlayers[name].gameObject);
            audioPlayers.Remove(name);

            // Handle edge case sound types.
            if (isAmbient)
            {
                // Never Decrement ambientSoundsCounts unless there are no active ambient sounds.
                // This is because you can decrement and increment on weird timings cuasing overlap
                // between audio players. The overlap will make it impossible to more than 1 of the 
                // overlapped audio player objects because the key is removed from the dictionary.
                // So this is why we only reset the counts to 0 if there are no other active audio
                // players of the same base ambient sound.
                int totalActiveAudioPlayersOfThisBaseAmbientName = 0;
                string thisBaseAmbientName = AmbientBaseName(name);
                foreach (string key in audioPlayers.Keys)
                {
                    // If the key is ambient and it's base name is the same as this base name.
                    if (key.Contains('*') && AmbientBaseName(key).Equals(thisBaseAmbientName))
                    {
                        // Then we still have an active audio player with the same base name.
                        totalActiveAudioPlayersOfThisBaseAmbientName++;
                    }
                }
                // If there are no other active audio players with the same base name then reset the
                // count of this ambient sound. 
                // (I do this so that there is less chance of running the count too high and breaking something.
                // Because technically I could just never reset it.)
                if (totalActiveAudioPlayersOfThisBaseAmbientName == 0)
                {
                    ambientSoundsCounts[AmbientBaseName(name)] = 0;
                }
            }
        }

        #endregion

        private static void PlayAudioFromSource(string name, float volume)
        {
            // Whenever referencing 'sounds' make sure to use the ambient base name.
            bool isAmbient = name.Contains('*');
            if (isAmbient)
            {
                audioPlayers[name].volume = volume * volumeMultipliers[(int)sounds[AmbientBaseName(name)].type];
            }
            else
            {
                audioPlayers[name].volume = volume * volumeMultipliers[(int)sounds[name].type];
            }
            // All sounds will loop by default.
            audioPlayers[name].loop = true;
            audioPlayers[name].Play();

            // Handle edge case sound types.
            if (!isAmbient && sounds[name].type == SoundType.ConditionalAmbient)
            {
                // Conditional Ambient sounds are destroyed after use because they vary too much.
                GameObject.Destroy(audioPlayers[name].gameObject, audioPlayers[name].clip.length);
                audioPlayers[name].loop = false;
                audioPlayers.Remove(name);
            }
            else if (!isAmbient && sounds[name].type == SoundType.ConditionalBackground)
            {
                audioPlayers[name].loop = false;
            }
        }

        private static string CreateAudioSource(string name, Vector3 position)
        {
            if (audioPlayers.ContainsKey(name))
            {
                Debug.LogError($"Couldn't Create Audio Source. Audio Player Already Exists. ({name})");
                return null;
            }

            bool isAmbient = false;
            // Handle Ambient Sounds Edge Case
            if (sounds[name].type == SoundType.Ambient)
            {
                // Ambient Sounds need unique names so that they can be stopped.
                // Make sure to not allow for 
                ambientSoundsCounts[name]++;
                name = name + ambientSoundsCounts[name].ToString();
                isAmbient = true;
            }

            // Construct Game Object
            GameObject soundGameObject = new GameObject($"AudioSource: {name}");
            soundGameObject.transform.position = position;
            AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();

            // Whenever referencing 'sounds' make sure to use the ambient base name.
            if (isAmbient)
            {
                audioSource.clip = GetAudioClip(AmbientBaseName(name));
            }
            else
            {
                audioSource.clip = GetAudioClip(name);
            }

            audioPlayers[name] = audioSource;

            // Configure Audio Source based off of sound type.
            // Whenever referencing 'sounds' make sure to use the ambient base name.
            if (isAmbient)
            {
                audioPlayers[name].spatialBlend = sounds[AmbientBaseName(name)].spacialBlend;
                audioPlayers[name].maxDistance = sounds[AmbientBaseName(name)].maxAudibleDistance;
            }
            else
            {
                audioPlayers[name].spatialBlend = sounds[name].spacialBlend;
                audioPlayers[name].maxDistance = sounds[name].maxAudibleDistance;
            }
            audioPlayers[name].rolloffMode = AudioRolloffMode.Custom;
            audioPlayers[name].dopplerLevel = 0;

            return name;
        }

        private static AudioClip GetAudioClip(string name)
        {
            return Resources.Load<AudioClip>(sounds[name].fileLocation);
        }

        public static void UpdateAudioSourceLocation(string name, Vector3 position)
        {
            if (!audioPlayers.ContainsKey(name))
            {
                Debug.LogError($"Couldn't Update Sound Location. No Audio Player Exists. ({name})");
                return;
            }
            audioPlayers[name].gameObject.transform.position = position;
        }

        // Can be used to speed up the track.
        public static void ChangeAudioPitch(string name, float pitch)
        {
            audioPlayers[name].pitch = pitch;
        }

        // Audio players should be cleared on scene unload. 
        public static void ClearAudioPlayersAndSounds()
        {
            audioPlayers.Clear();
            sounds.Clear();
        }

        // Given a name known to have an ambient base sound this will return the base name.
        private static string AmbientBaseName(string name)
        {
            char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            return name.Trim(numbers);
        }
    }
}