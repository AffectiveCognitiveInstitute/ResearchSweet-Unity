using System.Diagnostics;
using ResearchSweet.Transport;
using ResearchSweet.Transport.Client;
using ResearchSweet.Transport.Data;
using ResearchSweet.Transport.Events;
using ResearchSweet.Transport.Helpers;
using ResearchSweet.Transport.Server;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;

namespace ResearchSweet.Streaming
{
    public interface IStreamingCameraComponent
    {
        bool ScreenshotCaptureInProgress { get; set; }
        bool StreamingActive { get; set; }
        GameObject GameObject { get; }
    }

    public abstract class StreamingCameraComponentBase : MonoBehaviour, IStreamingCameraComponent
    {
        public bool ScreenshotCaptureInProgress { get; set; }
        public bool StreamingActive { get; set; }
        public GameObject GameObject { get; }
        protected IMainThreadRunner ThreadRunner => MainThreadRunnerExtensions.MainThreadRunner;
        protected IResearchSweetClient Client;
        protected FrameRateGate Gate = new FrameRateGate(1);
        private int _activeSceneId => SceneManager.GetActiveScene().buildIndex;

        public Camera TargetCamera { get; set; }
        public int CaptureWidth { get; set; } = 640;
        public int CaptureHeight { get; set; } = 360;
        private RenderTexture _lowResRT;
        private Texture2D _tex;

        void Start()
        {
#warning Hier muss ein besserer flow gefunden werden
            return;
            TargetCamera = GetComponent<Camera>();
            _lowResRT = new RenderTexture(CaptureWidth, CaptureHeight, 24);
            TargetCamera.targetTexture = _lowResRT;

            _tex = new Texture2D(
                CaptureWidth,
                CaptureHeight,
                DefaultFormat.Video,
                TextureCreationFlags.None
            );
        }

        public void OnEnable()
        {
            Client = ResearchSweetHelpers.Client;
            EventBus.Subscribe<RequestScreenshotEvent>(args =>
            {
                if (args.InstanceId == GetInstanceID())
                {
                    ThreadRunner.Enqueue(() =>
                    {
                        ScreenshotCaptureInProgress = true;
                    });
                    //    var camera = ResearchSweetHelpers.GetComponentByInstanceId(instanceId) as Camera;
                    //    var streamingCameraComponent = camera.gameObject.GetComponents<MonoBehaviour>().OfType<IStreamingCameraComponent>().FirstOrDefault();
                    //    if (streamingCameraComponent != null)
                    //    {
                    //        streamingCameraComponent.ScreenshotCaptureInProgress = true;
                    //        var items = new GameObjectInfo[]
                    //        {
                    //                ResearchSweetHelpers.Convert(camera.gameObject)
                    //        };
                    //        ResearchSweetClient.SendAsync<Server.ISceneControl>(x => x.SendGameObjectsAsync(items, _activeSceneId, false));
                }
            });
        }

        void OnPostRender()
        {
#warning Hier muss ein besserer flow gefunden werden
            return;

            if (!ScreenshotCaptureInProgress && !StreamingActive)
            {
                return;
            }

            if (!Gate.ShouldProcess())
            {
                return;
            }

            var swRender = Stopwatch.StartNew();
            RenderTexture.active = _lowResRT;
            _tex.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
            _tex.Apply();
            RenderTexture.active = null;
            swRender.Stop();

            if (ScreenshotCaptureInProgress)
            {
                ScreenshotCaptureInProgress = false;
                var items = new GameObjectInfo[]
                {
                    ResearchSweetHelpers.Convert(GameObject)
                };
                Client.SendAsync<Transport.Server.ISceneControl>(x => x.SendGameObjectsAsync(items, _activeSceneId, false));
            }

            var swEncoder = Stopwatch.StartNew();
            var jpg = _tex.EncodeToJPG(75);
            swEncoder.Stop();

            var image = new ImageInfo()
            {
                Data = jpg,
                ImageFormatType = ImageFormatType.JPG,
                RenderTime = swRender.ElapsedMilliseconds,
                EncoderTime = swEncoder.ElapsedMilliseconds
            };

            var instanceId = GetInstanceID();
            Client.SendAsync<Transport.Server.ISceneControl>(x => x.SendCameraImageAsync(_activeSceneId, instanceId, image));
        }
    }
}
