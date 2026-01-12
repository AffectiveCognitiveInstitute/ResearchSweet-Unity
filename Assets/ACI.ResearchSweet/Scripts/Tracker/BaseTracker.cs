using System;
using UnityEngine;

namespace ResearchSweet.Tracker
{
    public class BaseTracker : MonoBehaviour
    {
        [HideInInspector] public int InstanceId { get; private set; }
        [HideInInspector] public string InstanceName;
        [HideInInspector] public DateTime EventDate { get; private set; }
        public bool UseCustomName = false;

        void Start()
        {
            GameObjectTrackerExtensions.GameObjectTracker.TrackObject(this);
        }

        private void OnDisable()
        {
            GameObjectTrackerExtensions.GameObjectTracker.UntrackObject(this);
        }

        private void OnDestroy()
        {
            GameObjectTrackerExtensions.GameObjectTracker.UntrackObject(this);
        }

        private void Awake()
        {
            InstanceId = GetInstanceID();
            if (!UseCustomName)
                InstanceName = gameObject.name;
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            EventDate = DateTime.UtcNow;
        }
    }
}
