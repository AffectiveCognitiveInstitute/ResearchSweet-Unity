using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ResearchSweet.Streaming;
using ResearchSweet.Transport;
using ResearchSweet.Transport.Client;
using ResearchSweet.Transport.Data;
using ResearchSweet.Transport.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ResearchSweet.Helpers
{
    public static class ResearchSweetHelpers
    {
        public static IResearchSweetClient Client => _client;
        private static ResearchSweetClient _client;
        public static QuestionnaireHelper QuestionnaireHelper => ((ResearchSweetClient)_client)?.QuestionnaireHelper;

        private static Type _streamingCameraComponentImplementationType;
        private static Type StreamingCameraComponentImplementationType
        {
            get
            {
                if (_streamingCameraComponentImplementationType == null)
                {
                    var interfaceType = typeof(StreamingCameraComponent);

                    // Alle Assemblies durchsuchen
                    _streamingCameraComponentImplementationType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(assembly => GetSafeTypes(assembly))
                        .FirstOrDefault(type =>
                            interfaceType.IsAssignableFrom(type)
                            && typeof(Component).IsAssignableFrom(type)
                            && !type.IsAbstract);
                }
                return _streamingCameraComponentImplementationType;
            }
        }

        private static Type[] GetSafeTypes(Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).ToArray();
            }
        }

        public static void AttachStreamingCameraComponents()
        {
            Debug.Log($"AttachStreamingCameraComponents");
            var gameObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
            var cameraGameObjects = gameObjects.Where(x => x.GetComponent<Camera>()).ToArray();
            foreach (var cameraGameObject in cameraGameObjects)
            {
                var streamingCameraComponent = cameraGameObject.GetComponents<MonoBehaviour>().OfType<StreamingCameraComponent>().FirstOrDefault();
                if (streamingCameraComponent == null)
                {
                    cameraGameObject.AddComponent(StreamingCameraComponentImplementationType);
                    Debug.Log($"Attached {nameof(StreamingCameraComponent)} to camera {cameraGameObject.GetInstanceID()}");
                }
            }
        }

        public static GameObject[] GetRootGameObjects() => SceneManager.GetActiveScene().GetRootGameObjects();
        public static GameObject GetGameObjectByInstanceId(int instacneId) => Resources.FindObjectsOfTypeAll(typeof(GameObject)).Select(x => (GameObject)x).FirstOrDefault(x => x.GetInstanceID() == instacneId);
        public static Component GetComponentByInstanceId(int instacneId) => Resources.FindObjectsOfTypeAll(typeof(Component)).Select(x => (Component)x).FirstOrDefault(x => x.GetInstanceID() == instacneId);
        public static GameObjectInfo[] Convert(IEnumerable<GameObject> gameObjects) => gameObjects?.Select(x => Convert(x.gameObject)).ToArray();
        public static GameObjectInfo Convert(GameObject gameObject)
        {
            var result = new GameObjectInfo()
            {
                InstanceId = gameObject.GetInstanceID(),
                ParentId = gameObject.transform?.parent?.gameObject?.GetInstanceID(),
                Name = gameObject.transform.name,
                Layer = gameObject.layer,
                Tag = gameObject.tag,
                Transform = Convert(gameObject.transform),
                Components = gameObject.GetComponents(typeof(Component)).Where(x => !_componentTypesToExclude.Contains(x.GetType())).Select(x => Convert(x)).ToList(),
            };

            var camera = result.Components.FirstOrDefault(x => x.IsCamera);
            if (camera != null)
            {
                result.Camera = new CameraInfo()
                {
                    InstanceId = result.InstanceId,
                    Component = camera
                };
#warning TODO capture und screenshot infos müssen hier noch rein
            }

            return result;
        }
        private static Type[] _componentTypesToExclude => new Type[] { typeof(Transform), typeof(GameObject) };

        public static TransformInfo Convert(Transform transform)
            => new TransformInfo()
            {
                InstanceId = transform.GetInstanceID(),
                Name = transform.name,
                Position = transform.position,
                Rotation = transform.rotation,
                Scale = transform.localScale
            };

        public static ComponentInfo Convert(Component component)
            => new ComponentInfo()
            {
                InstanceId = component.GetInstanceID(),
                AssemblyQualifiedName = component.GetType().AssemblyQualifiedName,
                TypeName = component.GetType().FullName,
                Name = component.name,
            };

        public static InitializeResult InitializeResearchSweet() => new GameObject().InitializeResearchSweet();

        public static InitializeResult InitializeResearchSweet(this MonoBehaviour monoBehaviour, Action<ResearchSweetClientOptionsBuilder> builder = null)
        {
            if (monoBehaviour.gameObject != null)
            {
                return monoBehaviour.gameObject.InitializeResearchSweet(builder);
            }
            return null;
        }

        public static InitializeResult InitializeResearchSweet(this GameObject gameObject, Action<ResearchSweetClientOptionsBuilder> builder = null)
        {
            gameObject.AddComponent(MainThreadRunnerExtensions.MainThreadRunner.GetType());
            var tracker = (IGameObjectTracker)gameObject.AddComponent(GameObjectTrackerExtensions.GameObjectTracker.GetType());
            if (builder != null)
            {
                var client = new ResearchSweetClient(builder);
                _client = client;
                tracker.AssignResearchSweetClient(client);
                return new InitializeResult() { GameObject = gameObject, Client = client };
            }
            return new InitializeResult() { GameObject = gameObject };
        }

        public class InitializeResult
        {
            public GameObject GameObject { get; internal set; }
            public ResearchSweetClient Client { get; internal set; }
        }

        public static byte[] CapturePNG(this Camera camera) => camera.Capture().EncodeToPNG();
        public static byte[] CaptureJPG(this Camera camera) => camera.Capture().EncodeToJPG();
        public static byte[] Capture(this Camera camera, ImageFormatType imageFormatType)
        {
            switch (imageFormatType)
            {
                case ImageFormatType.PNG:
                    return camera.CapturePNG();
                case ImageFormatType.JPG:
                    return camera.CaptureJPG();
                default:
                    return null;
            }
        }

        public static Texture2D Capture(this Camera targetCamera)
        {
            // 1️⃣ Erstelle eine temporäre RenderTexture
            int width = Screen.width;
            int height = Screen.height;
            RenderTexture renderTexture = new RenderTexture(width, height, 24);

            // 2️⃣ Kamera in die RenderTexture rendern
            targetCamera.targetTexture = renderTexture;
            targetCamera.Render();

            // 3️⃣ RenderTexture in ein Texture2D übertragen
            RenderTexture.active = renderTexture;
            Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenshot.Apply();

            targetCamera.targetTexture = null;
            RenderTexture.active = null;

            return screenshot;
        }

        public static GameObject[] Flatten(this IEnumerable<GameObject> gameObjects)
        {
            var list = new List<GameObject>();
            foreach (var gameObject in gameObjects)
            {
                gameObject._flatten(list);
            }
            return list.ToArray();
        }

        private static void _flatten(this GameObject gameObject, List<GameObject> list)
        {
            list.Add(gameObject);
            if (gameObject.transform.childCount == 0)
            {
                return;
            }

            foreach (Transform child in gameObject.transform)
            {
                child.gameObject._flatten(list);
            }
        }
    }
}
