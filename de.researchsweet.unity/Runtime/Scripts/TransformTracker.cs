using ResearchSweet.Transport.Tracker;
using UnityEngine;

public class TransformTracker : BaseTracker
{
    [HideInInspector]
    public Vector3 Position { get; private set; }
    [HideInInspector]
    public Quaternion Rotation { get; private set; }
    [HideInInspector]
    public Vector3 Scale { get; private set; }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        Position = transform.position;
        Rotation = transform.rotation;
        Scale = transform.localScale;
    }
}
