using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ResearchSweet.Transport;
using ResearchSweet.Transport.Data;
using ResearchSweet.Transport.Server;
using UnityEngine;

namespace ResearchSweet.Tracker
{
    
    public interface IGameObjectTracker
    {
        void AssignResearchSweetClient(IResearchSweetClient client);
        void TrackObject(BaseTracker tracker);
        void UntrackObject(BaseTracker tracker);
        Task SendEventAsync(string target, Dictionary<string, object> data);
    }
    
    public class GameObjectTracker : MonoBehaviour, IGameObjectTracker
    {
        private static GameObjectTracker _instance;
        public static GameObjectTracker Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameObjectTracker>();
    
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(GameObjectTracker).Name);
                        _instance = singletonObject.AddComponent<GameObjectTracker>();
    
                        Debug.LogWarning("GameObjectTracker instance was created automatically.");
                    }
                }
                return _instance;
            }
        }
    
        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Duplicate instance of GameObjectTracker detected! Destroying new instance.");
                Destroy(gameObject);
                return;
            }
    
            _instance = this;
            _timer = new System.Timers.Timer(500);
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
            DontDestroyOnLoad(gameObject); // Keeps it alive across scenes
        }
    
        private List<BaseTracker> _trackers = new List<BaseTracker>();
    
        private IResearchSweetClient _client;
        private System.Timers.Timer _timer;
    
        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_client != null)
            {
                MainThreadRunner.Instance.Enqueue(() =>
                {
                    _instance._checkTrackers();
                    _instance._handleTransformTrackers();
                });
            }
        }
    
        private void _handleTransformTrackers()
        {
            var transforms = _trackers
                .Where(x => x is TransformTracker)
                .Select(x => (TransformTracker)x)
                .Where(x => x.HasChanges)
                .Select(tracker => new TransformTrackerEvent()
                {
                    EventDate = tracker.EventDate,
                    InstanceId = tracker.gameObject.GetInstanceID(),
                    Name = tracker.InstanceName,
                    Position = tracker.Position,
                    Rotation = tracker.Rotation,
                    Scale = tracker.Scale,
                }).ToArray();
    
            if (transforms.Any())
            {
                _client.SendAsync<ITransformControl>(x => x.SendTransformsAsync(transforms)).Wait();
            }
        }
    
    
        private void _checkTrackers()
        {
            var deleted = _trackers.Where(x => x.gameObject == null).ToArray();
            foreach (var tracker in deleted)
            {
                _trackers.Remove(tracker);
            }
        }
    
        public void AssignResearchSweetClient(IResearchSweetClient client) => _client = client;
    
        public void TrackObject(BaseTracker tracker)
        {
            if (!_trackers.Contains(tracker))
            {
                _trackers.Add(tracker);
            }
        }
    
        public void UntrackObject(BaseTracker tracker)
            => _trackers.Remove(tracker);
    
        public async Task SendEventAsync(string target, Dictionary<string, object> data)
            => await _client.SendAsync<IEventControl>(x => x.SendEventAsync(target, data));
    }
    
    public class GameObjectTrackerExtensions
    {
        public static IGameObjectTracker GameObjectTracker { get; private set; }
        public static void RegisterGameObjectTracker(IGameObjectTracker gameObjectTracker)
        {
            GameObjectTracker = gameObjectTracker;
        }
    }
}
