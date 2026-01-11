using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ResearchSweet.Transport.Helpers;
using UnityEngine;

namespace ResearchSweet
{
    public class MainThreadRunner : MonoBehaviour, IMainThreadRunner
    {
        private ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();
        private List<FrameAction> _nextFrameActions = new List<FrameAction>();
        private ConcurrentQueue<Action> _postRenderActions = new ConcurrentQueue<Action>();

        private static MainThreadRunner _instance;
        public static MainThreadRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MainThreadRunner>();

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(MainThreadRunner).Name);
                        _instance = singletonObject.AddComponent<MainThreadRunner>();

                        Debug.LogWarning("MainThreadRunner instance was created automatically.");
                    }
                }
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Duplicate instance of MainThreadRunner detected! Destroying new instance.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject); // Keeps it alive across scenes
        }

        public void Enqueue(Action action) => _actions.Enqueue(action);
        void Update()
        {
            while (_actions.Count > 0)
            {
                if (_actions.TryDequeue(out var action))
                {
                    action?.Invoke();
                }
            }

            var nextFrameActions = _nextFrameActions.Where(x => x.Frame <= Time.frameCount).ToList();
            while (nextFrameActions.Count > 0)
            {
                var action = nextFrameActions.FirstOrDefault();
                if (action != null)
                {
                    action?.Action?.Invoke();
                    nextFrameActions.Remove(action);
                    _nextFrameActions.Remove(action);
                }
            }
        }

        private void OnPostRender()
        {
            while (_postRenderActions.Count > 0)
            {
                if (_postRenderActions.TryDequeue(out var action))
                {
                    action?.Invoke();
                }
            }
        }

        public void EnqueueForNextFrame(Action action) => _nextFrameActions.Add(new FrameAction(action));
        public void EnqueueForPostRender(Action action) => _postRenderActions.Enqueue(action);

        public class FrameAction
        {
            public int Frame { get; set; }
            public Action Action { get; set; }

            public FrameAction(Action action)
            {
                Frame = Time.frameCount + 1;
                Action = action;
            }
        }
    }
}
