using ResearchSweet.Transport.Tracker;
using UnityEngine;

namespace ResearchSweet
{
    public class TransformTracker : BaseTracker
    {
        [HideInInspector]
        public Vector3 Position { get; private set; }
        [HideInInspector]
        public Quaternion Rotation { get; private set; }
        [HideInInspector]
        public Vector3 Scale { get; private set; }

        public bool HasChanges { get; private set; }

        void Start()
        {
            GameObjectTracker.Instance.TrackObject(this);
        }

        void OnDisable()
        {
            GameObjectTracker.Instance.UntrackObject(this);
        }

        void OnDestroy()
        {
            GameObjectTracker.Instance.UntrackObject(this);
        }

        public void LateUpdate()
        {
            HasChanges = false;
            if (Position != transform.position)
            {
                Position = transform.position;
                HasChanges = true;
            }
            if (Rotation != transform.rotation)
            {
                Rotation = transform.rotation;
                HasChanges = true;
            }
            if (Scale != transform.localScale)
            {
                Scale = transform.localScale;
                HasChanges = true;
            }
        }
    }
}
