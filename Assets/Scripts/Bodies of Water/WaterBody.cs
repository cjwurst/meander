using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WaterBody
{
    public bool hasGameObject { get; private set; }
    GameObject gameObject;
    protected MeshCollider collider;

    protected TerrainMessenger terrainMessenger;

    protected float volume;
    protected float capacity;
    float availableCapacity { get { return capacity - volume; } }

    List<Sink> sinks = new List<Sink>();
    protected WaterPoint[] waterPoints = new WaterPoint[0];
    public CollisionPoint[] collisionPoints
    {
        get
        {
            List<CollisionPoint> points = new List<CollisionPoint>();
            for (int i = 0; i < waterPoints.Length; i++)
            {
                WaterPoint waterPoint = waterPoints[i];
                points.Add(new CollisionPoint(waterPoint, GetType(), (p, t) => OnCollision(t, p, waterPoint)));
            }
            return points.ToArray();
        }
    }

    public readonly EventDispatcher<WaterBody> e_createBody;
    Action<WaterBody> ec_createBody;
    public readonly EventDispatcher<WaterBody> e_destroyBody;
    Action<WaterBody> ec_destroyBody;
    public readonly EventDispatcher<Tuple<WaterBody, WaterBody>> e_addSink;
    Action<Tuple<WaterBody, WaterBody>> ec_addSink;
    public readonly EventDispatcher<Tuple<WaterBody, WaterBody>> e_removeSink;
    Action<Tuple<WaterBody, WaterBody>> ec_removeSink;

    public WaterBody(TerrainMessenger _terrainMessenger, float _volume, float _capacity, bool _hasGameObject = false)
    {
        terrainMessenger = _terrainMessenger;

        volume = _volume;
        capacity = _capacity;

        e_createBody = new EventDispatcher<WaterBody>(out ec_createBody);
        e_destroyBody = new EventDispatcher<WaterBody>(out ec_destroyBody);
        e_addSink = new EventDispatcher<Tuple<WaterBody, WaterBody>>(out ec_addSink);
        e_removeSink = new EventDispatcher<Tuple<WaterBody, WaterBody>>(out ec_removeSink);

        hasGameObject = _hasGameObject;
        if (hasGameObject)
        {
            gameObject = new GameObject();
            CreateGameObject(gameObject);
        }
    }

    public virtual void Init () { }

    protected virtual void CreateGameObject(GameObject gameObject)
    {
        gameObject.name = GetType().ToString();

        gameObject.layer = 4;  // Water

        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.drag = 0f;
        rigidbody.angularDrag = 0f;

        collider = gameObject.AddComponent<MeshCollider>();
    }

    // returns an action to be invoked after all collisions occur
    Action OnCollision(Type externalType, WaterPoint externalPoint, WaterPoint internalPoint)
    {
        if (!waterPoints.Contains(internalPoint)) return () => { };           // if *internalPoint* was removed in a previous collision

        if (waterPoints.Contains(externalPoint)) { CollideWithSelf(externalPoint, internalPoint); return () => { }; }

        bool isDominant = internalPoint.order > externalPoint.order;            // Operations that occur once per collision, rather than once per
                                                                                //colliding body, will be executed by the dominant *WaterBody*.
        if (externalType == typeof(River))
            return CollideWithRiver(externalPoint, internalPoint, isDominant);
        else if (externalType == typeof(Oxbow))
            return CollideWithOxbow(externalPoint, internalPoint, isDominant);
        return () => { };
    }
    protected virtual Action CollideWithSelf(WaterPoint point1, WaterPoint point2, bool isDominant = false) { return () => { }; }
    protected virtual Action CollideWithRiver(WaterPoint externalPoint, WaterPoint internalPoint, bool isDominant = false) { return () => { }; }
    protected virtual Action CollideWithOxbow(WaterPoint externalPoint, WaterPoint internalPoint, bool isDominant = false) { return () => { }; }


    // Don't use a constructor to call *CreateSink* or *CreateBody*; call from *Init* instead.
    protected void CreateSink (WaterBody waterBody, float maxRate)
    {
        sinks.Add(new Sink(waterBody, maxRate));
        CreateBody(waterBody);
        ec_addSink.Invoke(new Tuple<WaterBody, WaterBody>(this, waterBody));
    }
    protected void CreateBody (WaterBody waterBody) { ec_createBody.Invoke(waterBody); }

    public virtual void Flow() { foreach(Sink sink in sinks) volume += sink.TryAccept(volume); }

    // returns the net change in the source's *volumeToAccept*
    float TryAccept(float volumeToAccept)
    {
        float acceptedVolume = Mathf.Min(availableCapacity, volumeToAccept);
        volume += acceptedVolume;
        return -acceptedVolume;
    }

    public virtual void Draw() { }
    public virtual void Erase() { }

    protected virtual void Destroy()
    {
        Erase();
        GameObject.Destroy(gameObject);
        foreach (Sink sink in sinks)
            ec_removeSink.Invoke(new Tuple<WaterBody, WaterBody>(this, sink.body));
        ec_destroyBody.Invoke(this);
    }

    protected class Sink
    {
        public WaterBody body;
        public float maxRate { private get; set; }

        public Sink(WaterBody _body, float _maxRate) { body = _body; maxRate = _maxRate; }

        public float TryAccept(float volumeToAccept) { return body.TryAccept(Mathf.Min(volumeToAccept, maxRate)); }
    }
}

public class CollisionPoint
{
    public WaterPoint waterPoint;
    public Type type;                                       // the type of *WaterBody* that *point* is attached to
    public Func<WaterPoint, Type, Action> command;          // Invoking this command will call *OnCollision* for the *WaterPoint* associated with *this*.

    public Vector3 position { get { return waterPoint.position; } }
    public float radius { get { return waterPoint.radius; } }

    public CollisionPoint(WaterPoint _waterPoint, Type _type, Func<WaterPoint, Type, Action> _command)
    {
        waterPoint = _waterPoint;
        type = _type;
        command = _command;
    }
}
