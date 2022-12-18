using UnityEngine;

namespace Nebula
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance = null;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    T[] singletonsInScene = FindObjectsOfType(typeof(T)) as T[];

                    // Create Singleton if not created or Throw error if more than 1 singleton exists or Use existing singleton.
                    if (singletonsInScene.Length == 0)
                    {
                        GameObject singleton = new GameObject();
                        singleton.name = string.Format("{0} Singleton", typeof(T).Name);
                        _instance = singleton.AddComponent<T>();
                    }
                    else if (singletonsInScene.Length > 1)
                    {
                        Debug.LogError("There are too many singletons of " + typeof(T).Name + " in the scene.");
                    }
                    else
                    {
                        _instance = singletonsInScene[0];
                    }
                }
                return _instance;
            }
        }
    }
}
