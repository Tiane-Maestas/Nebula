using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using TMPro;


namespace Nebula
{
    public static class Utils
    {
        // Needs testing in 3D.
        public static void DisplayInfo(Transform transform, string message)
        {
            // Create a canvas to put text onto at the transform passed in.
            GameObject canvasObject = new GameObject("Custom Canvas: " + message);
            canvasObject.transform.SetParent(transform);

            Canvas textCanvas = canvasObject.AddComponent<Canvas>();
            textCanvas.GetComponent<RectTransform>().localPosition = Vector3.zero;
            textCanvas.renderMode = RenderMode.WorldSpace;
            textCanvas.worldCamera = Camera.main;

            // Create text and place it in the canvas.
            GameObject textObject = new GameObject("Custom Text: " + message);
            textObject.transform.SetParent(canvasObject.transform);

            TextMeshPro text = textObject.AddComponent<TextMeshPro>();
            text.GetComponent<RectTransform>().localPosition = Vector3.zero;
            text.autoSizeTextContainer = true;
            text.text = message;
            text.fontSize = 4;
            text.sortingOrder = 1;

            // Destroy it after 1 physics call.
            GameObject.Destroy(canvasObject, Time.fixedDeltaTime);
        }

        public static Vector3 RotateVector3ByDegInWorldCoordinates(Vector3 vector, Vector3 angles)
        {
            return Quaternion.Euler(angles.x, angles.y, angles.z) * vector;
        }

        // Type T must be marked as "[System.Serializable]".
        public static void SerializedSave<T>(T saveData, string path) where T : class
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream fileStream = new FileStream(path, FileMode.Create);

            try
            {
                formatter.Serialize(fileStream, saveData);
            }
            catch (Exception e)
            {
                Debug.Log("File Stream Failed!");
                Debug.Log(e);
            }
            fileStream.Close();
        }

        // Type T must be marked as "[System.Serializable]".
        public static T LoadSerializedSave<T>(string path) where T : class
        {
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(path, FileMode.Open);

                T loadedData = default(T);
                try
                {
                    loadedData = formatter.Deserialize(fileStream) as T;
                }
                catch (Exception e)
                {
                    Debug.Log("File Stream Failed!");
                    Debug.Log(e);
                    fileStream.Close();
                    return loadedData;
                }
                fileStream.Close();

                return loadedData;
            }
            else
            {
                Debug.Log("No Save Found!");
                return default(T); ;
            }
        }
    }
}