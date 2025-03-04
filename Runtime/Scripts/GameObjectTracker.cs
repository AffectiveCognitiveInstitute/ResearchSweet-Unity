using ResearchSweet.Transport;
using ResearchSweet.Transport.Data;
using ResearchSweet.Transport.Helpers;
using ResearchSweet.Transport.Server;
using ResearchSweet.Transport.Tracker;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GameObjectTracker : MonoBehaviour, IGameObjectTracker
{
    private static GameObjectTracker _instance;
    public static GameObjectTracker Instance
    {
        get
        {
            if (_instance == null)
            {
                try
                {
                    var mtr = new GameObject("ResearchSweet_GameObjectTracker");
                    _instance = mtr.AddComponent<GameObjectTracker>();
                }
                catch { }
            }
            return _instance;
        }
    }

    private List<BaseTracker> _trackers = new List<BaseTracker>();

    private IResearchSweetClient _client;
    private System.Timers.Timer _timer;

    private GameObjectTracker()
    {
        _timer = new System.Timers.Timer(100);
        _timer.AutoReset = true;
        _timer.Elapsed += _timer_Elapsed;
        _timer.Start();
    }

    private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (_client != null)
        {
            _checkTrackers();
            _handleTransformTrackers();
        }
    }

    private void _handleTransformTrackers()
    {
        var transforms = _trackers.Where(x => x is TransformTracker).Select(x => (TransformTracker)x).Select(tracker => new TransformTrackerEvent()
        {
            EventDate = tracker.EventDate,
            InstanceId = tracker.InstanceId,
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
