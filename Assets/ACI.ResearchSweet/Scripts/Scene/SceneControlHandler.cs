using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ResearchSweet.Streaming;
using ResearchSweet.Transport;
using ResearchSweet.Transport.Client;
using ResearchSweet.Transport.Data;
using ResearchSweet.Transport.Events;
using ResearchSweet.Transport.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ResearchSweet.Scene
{
    public class UnitySceneControlHandler : Transport.Client.ISceneControl, IResearchSweetClientProvider
    {
        #region Properties

        public IResearchSweetClient ResearchSweetClient { get; set; }
        private IMainThreadRunner _threadRunner => MainThreadRunnerExtensions.MainThreadRunner;
        //public event VariableChangedEvent OnSendVariable;
        //public event MyQuestionnairesChangedEvent OnMyQuestionnairesChanged;
        //public event QuestionnaireChangedEvent OnQuestionnaireChanged;
        //public event AvailableQuestionnaireDefinitionsChangedEvent OnAvailableQuestionnaireDefinitionsChanged;

        private int _activeSceneId => SceneManager.GetActiveScene().buildIndex;

        #endregion

        #region Constructor

        public UnitySceneControlHandler()
        {
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        ~UnitySceneControlHandler()
        {
            SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            var sceneId = SceneManager.GetActiveScene().buildIndex;
            ResearchSweetClient.SendAsync<Transport.Server.ISceneControl>(x => x.SendActiveSceneAsync(sceneId)).Wait();
        }

        #endregion

        #region ISceneControl

        public virtual Task LoadSceneAsync(int sceneId)
        {
            _threadRunner?.Enqueue(() =>
            {
                SceneManager.LoadScene(sceneId);
                ResearchSweetClient.SendAsync<Transport.Server.ISceneControl>(x => x.SendActiveSceneAsync(sceneId));
            });
            return Task.CompletedTask;
        }

        public virtual Task SendVariableAsync(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Task.CompletedTask;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return Task.CompletedTask;
            }

            _threadRunner?.Enqueue(() =>
            {
                EventBus.Publish(new VariableChangedEvent(name, value));
            });

            return Task.CompletedTask;
        }

        public virtual Task RequestGameObjectChildrenAsync(int instanceId)
        {
            _threadRunner?.Enqueue(() =>
            {
                var gameObject = ResearchSweetHelpers.GetGameObjectByInstanceId(instanceId);
                var itemList = new List<GameObjectInfo>();
                foreach (Transform transform in gameObject.transform)
                {
                    var item = ResearchSweetHelpers.Convert(transform.gameObject);
                    itemList.Add(item);
                }
                var items = itemList.ToArray();
                ResearchSweetClient.SendAsync<Transport.Server.ISceneControl>(x => x.SendGameObjectsAsync(items, _activeSceneId, false));
            });

            return Task.CompletedTask;
        }

        public virtual Task RequestGameObjectRootsAsync()
        {
            _threadRunner?.Enqueue(() =>
            {
                var gameObjects = ResearchSweetHelpers.GetRootGameObjects();
                var items = ResearchSweetHelpers.Convert(gameObjects);
                ResearchSweetClient.SendAsync<Transport.Server.ISceneControl>(x => x.SendGameObjectsAsync(items, _activeSceneId, false));
            });

            return Task.CompletedTask;
        }

        public virtual Task RequestAllGameObjectAsync()
        {
            _threadRunner?.Enqueue(() =>
            {
                ResearchSweetHelpers.AttachStreamingCameraComponents();
                var allTransforms = GameObject.FindObjectsOfType(typeof(Transform)) as Transform[];
                var gameObjects = allTransforms.Select(x => x.gameObject).ToArray();
                var items = ResearchSweetHelpers.Convert(gameObjects);
                ResearchSweetClient.SendAsync<Transport.Server.ISceneControl>(x => x.SendGameObjectsAsync(items, _activeSceneId, true));
            });

            return Task.CompletedTask;
        }

        public virtual Task ReloadGameObjectAsync(int instanceId)
        {
            _threadRunner?.Enqueue(() =>
            {
                var gameObject = ResearchSweetHelpers.GetGameObjectByInstanceId(instanceId);
                var items = new GameObjectInfo[]
                {
                ResearchSweetHelpers.Convert(gameObject)
                };
                ResearchSweetClient.SendAsync<Transport.Server.ISceneControl>(x => x.SendGameObjectsAsync(items, _activeSceneId, false));
            });

            return Task.CompletedTask;
        }

        public virtual Task RequestImageAsync(int instanceId, ImageFormatType imageFormatType) => EventBus.PublishAsync(new RequestScreenshotEvent(instanceId, imageFormatType));

        //{
        //    _threadRunner?.Enqueue(() =>
        //    {
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
        //    }
        //});

        //    _threadRunner?.EnqueueForPostRender(() =>
        //    {
        //    _eventChannels.RequestScreenshotEventChannel.RaiseEvent(new RequestScreenshotEvent(instanceId, imageFormatType));
        //    var camera = ResearchSweetHelpers.GetComponentByInstanceId(instanceId) as Camera;
        //    //var image = camera?.Capture(imageFormatType);
        //    //if (image != null)
        //    //{
        //    //    var activeSceneId = SceneManager.GetActiveScene().buildIndex;
        //    //    var imageInfo = new ImageInfo()
        //    //    {
        //    //        ImageFormatType = imageFormatType,
        //    //        Data = image
        //    //    };
        //    //    ResearchSweetClient.SendAsync<Server.ISceneControl>(x => x.SendCameraImageAsync(activeSceneId, instanceId, imageInfo));
        //    //}
        //    var streamingCameraComponent = camera.gameObject.GetComponents<MonoBehaviour>().OfType<IStreamingCameraComponent>().FirstOrDefault();
        //    if (streamingCameraComponent != null)
        //    {
        //        streamingCameraComponent.ScreenshotCaptureInProgress = false;
        //        var items = new GameObjectInfo[]
        //        {
        //                ResearchSweetHelpers.Convert(camera.gameObject)
        //        };
        //        ResearchSweetClient.SendAsync<Server.ISceneControl>(x => x.SendGameObjectsAsync(items, _activeSceneId, false));
        //    }
        //});

        //    return Task.CompletedTask;
        //}

        public Task StartStreamingAsync(int instanceId)
        {
            _threadRunner?.Enqueue(() =>
            {
                var streamingCameraComponent = ResearchSweetHelpers.GetComponentByInstanceId(instanceId) as StreamingCameraComponent;
                if (streamingCameraComponent != null)
                {
                    if (streamingCameraComponent.StreamingActive)
                    {
                        return;
                    }

                    streamingCameraComponent.StreamingActive = true;
                    var items = new GameObjectInfo[]
                    {
                        ResearchSweetHelpers.Convert(streamingCameraComponent.GameObject)
                    };
                    ResearchSweetClient.SendAsync<Transport.Server.ISceneControl>(x => x.SendGameObjectsAsync(items, _activeSceneId, false));
                }
            });
            return Task.CompletedTask;
        }

        public Task StopStreamingAsync(int instanceId)
        {
            _threadRunner?.Enqueue(() =>
            {
                var streamingCameraComponent = ResearchSweetHelpers.GetComponentByInstanceId(instanceId) as StreamingCameraComponent;
                if (streamingCameraComponent != null)
                {
                    if (!streamingCameraComponent.StreamingActive)
                    {
                        return;
                    }

                    streamingCameraComponent.StreamingActive = false;
                    var items = new GameObjectInfo[]
                    {
                        ResearchSweetHelpers.Convert(streamingCameraComponent.GameObject)
                    };
                    ResearchSweetClient.SendAsync<Transport.Server.ISceneControl>(x => x.SendGameObjectsAsync(items, _activeSceneId, false));
                }
            });
            return Task.CompletedTask;
        }

        public virtual Task SendTransformAsync(int instanceId, TransformInfo transformInfo)
        {
            _threadRunner?.Enqueue(() =>
            {
                var gameObject = ResearchSweetHelpers.GetGameObjectByInstanceId(instanceId);
                if (gameObject != null)
                {

                    gameObject.transform.position = transformInfo.Position;
                    gameObject.transform.rotation = transformInfo.Rotation;
                    gameObject.transform.localScale = transformInfo.Scale;

                    var items = new GameObjectInfo[]
                    {
                        ResearchSweetHelpers.Convert(gameObject)
                    };

                    ResearchSweetClient.SendAsync<Transport.Server.ISceneControl>(x => x.SendGameObjectsAsync(items, _activeSceneId, false));

                }
            });

            return Task.CompletedTask;
        }

        public virtual Task SendGameObjectAsync(int instanceId, GameObjectInfo gameObjectInfo)
        {
            _threadRunner?.Enqueue(() =>
            {
                var gameObject = ResearchSweetHelpers.GetGameObjectByInstanceId(instanceId);
                if (gameObject != null)
                {
                    gameObject.name = gameObjectInfo.Name;
                    //gameObject.layer = gameObjectInfo.Layer;
                    //gameObject.tag = gameObjectInfo.Tag;

                    var items = new GameObjectInfo[]
                    {
                        ResearchSweetHelpers.Convert(gameObject)
                    };

                    ResearchSweetClient.SendAsync<Transport.Server.ISceneControl>(x => x.SendGameObjectsAsync(items, _activeSceneId, false));
                }
            });

            return Task.CompletedTask;
        }

        public virtual Task AddComponentAsync(int instanceId, ComponentInfo componentInfo)
        {
            if (!string.IsNullOrWhiteSpace(componentInfo.AssemblyQualifiedName))
            {
                _threadRunner?.Enqueue(() =>
                {
                    Debug.Log($"[ResearchSweet] AddComponentAsync {componentInfo.AssemblyQualifiedName}...");

                    var gameObject = ResearchSweetHelpers.GetGameObjectByInstanceId(instanceId);
                    if (gameObject != null)
                    {
                        Debug.Log($"[ResearchSweet] AddComponentAsync GameObject {instanceId} found.");

                        var componentType = Type.GetType(componentInfo.AssemblyQualifiedName);
                        if (componentType != null)
                        {
                            Debug.Log($"[ResearchSweet] AddComponentAsync component type {componentInfo.AssemblyQualifiedName} found.");

                            gameObject.AddComponent(componentType);

                            var items = new GameObjectInfo[]
                            {
                            ResearchSweetHelpers.Convert(gameObject)
                            };

                            ResearchSweetClient.SendAsync<Transport.Server.ISceneControl>(x => x.SendGameObjectsAsync(items, _activeSceneId, false));
                        }
                    }
                });
            }
            return Task.CompletedTask;
        }

        public virtual Task RemoveComponentAsync(int instanceId)
        {
            _threadRunner?.Enqueue(() =>
            {
                Debug.Log($"[ResearchSweet] RemoveComponentAsync {instanceId}...");
                var component = ResearchSweetHelpers.GetComponentByInstanceId(instanceId);
                if (component != null)
                {
                    Debug.Log($"[ResearchSweet] RemoveComponentAsync component found...");

                    var gameObject = component.gameObject;
                    UnityEngine.Object.Destroy(component);

                    var gameObjectId = gameObject.GetInstanceID();

                    _threadRunner?.EnqueueForNextFrame(() =>
                    {
                        var gameObject2 = ResearchSweetHelpers.GetGameObjectByInstanceId(gameObjectId);
                        if (gameObject2 != null)
                        {
                            var items = new GameObjectInfo[]
                            {
                            ResearchSweetHelpers.Convert(gameObject2)
                            };
                            ResearchSweetClient.SendAsync<Transport.Server.ISceneControl>(x => x.SendGameObjectsAsync(items, _activeSceneId, false));
                        }
                    });
                }
            });
            return Task.CompletedTask;
        }

        public Task SendMyQuestionnairesAsync(List<QuestionnaireDto> questionnaires)
        {
            _threadRunner?.Enqueue(() =>
            {
                EventBus.Publish(new MyQuestionnairesChangedEvent(questionnaires));
            });
            return Task.CompletedTask;
        }

        public Task SendQuestionnaireAsync(QuestionnaireDto questionnaire)
        {
            _threadRunner?.Enqueue(() =>
            {
                EventBus.Publish(new QuestionnaireChangedEvent(questionnaire));
            });
            return Task.CompletedTask;
        }

        public Task SendAvailableQuestionnaireDefinitionsAsync(List<QuestionnaireDefinitionDto> questionnaireDefinitions)
        {
            _threadRunner?.Enqueue(() =>
            {
                EventBus.Publish(new QuestionnaireDefinitionsChangedEvent(questionnaireDefinitions));
            });
            return Task.CompletedTask;
        }

        #endregion
    }
}
