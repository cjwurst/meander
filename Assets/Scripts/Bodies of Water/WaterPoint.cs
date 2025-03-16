using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class WaterPoint
{
    // Each waterpoint has a unique order, so that on collision, one waterpoint can perform
    //the behavior required per collision rather than per colliding point.
    public readonly int order;

    public Vector3 position;
    public Vector3 movementVector;
    public float radius;

    HashSet<WaterPoint> parents = new HashSet<WaterPoint>();
    HashSet<WaterPoint> children = new HashSet<WaterPoint>();
    public WaterPoint[] childArray { get { return children.ToArray(); } }
    public WaterPoint[] parentArray { get { return parents.ToArray(); } }
    public bool hasParent { get { return parents.Count > 0; } }
    public bool hasChild { get { return children.Count > 0; } }
    public Vector3 meanParentPosition
    {
        get
        {
            if (!hasParent) return position;
            Vector3 meanPosition = Vector3.zero;
            foreach (WaterPoint parent in parents)
                meanPosition += parent.position;
            meanPosition /= parents.Count;
            return meanPosition;
        }
    }
    public Vector3 meanChildPosition
    {
        get
        {
            if (!hasChild) return position;
            Vector3 meanPosition = Vector3.zero;
            foreach (WaterPoint child in children)
                meanPosition += child.position;
            meanPosition /= children.Count;
            return meanPosition;
        }
    }

    public WaterPoint(Vector3 _position, float _radius, params WaterPoint[] _parents)
    {
        order = Orderer.instance.GetOrder();

        position = _position;
        radius = _radius;
        parents.UnionWith(_parents);
    }

    public void ReplaceParent (WaterPoint oldParent, WaterPoint updatedParent)
    {
        RemoveParents(oldParent);
        AddParents(updatedParent);
        //MonoBehaviour.print("*ReplaceParent*");
    }

    public void AddParents(params WaterPoint[] parentsToAdd)
    {
        foreach (WaterPoint parent in parentsToAdd)
            parent.children.Add(this);
        parents.UnionWith(parentsToAdd);
        //MonoBehaviour.print("*AddParents*");
    }

    public void RemoveParents(params WaterPoint[] parentsToRemove)
    {
        foreach (WaterPoint parent in parentsToRemove)
            parent.children.Remove(this);
        parents.ExceptWith(parentsToRemove);
        //MonoBehaviour.print("*RemoveParents*");
    }
    public void RemoveAllParents() { RemoveParents(parents.ToArray()); }

    public void AddChildren(params WaterPoint[] childrenToAdd)
    {
        foreach (WaterPoint child in childrenToAdd)
            child.parents.Add(this);
        children.UnionWith(childrenToAdd);
        //MonoBehaviour.print("*AddChildren*");
    }

    public void RemoveChildren(params WaterPoint[] childrenToRemove)
    {
        foreach (WaterPoint child in childrenToRemove)
            child.parents.Remove(this);
        children.ExceptWith(childrenToRemove);
        //MonoBehaviour.print("*RemoveChildren*");
    }
    public void RemoveAllChildren() { RemoveChildren(children.ToArray()); }

    public void Estrange() { RemoveAllParents(); RemoveAllChildren(); }

    Vector3 GetMomentum(float coefficientBase, int depth, int maxDepth)
    {
        Vector3 momentum = Vector3.zero;
        if (depth <= maxDepth && parents.Count > 0)
        {
            foreach (WaterPoint parent in parents)
                momentum += Mathf.Pow(coefficientBase, depth) * parent.movementVector + parent.GetMomentum(coefficientBase, depth + 1, maxDepth);
            momentum /= parents.Count;
        }
        return momentum;
    }
    public Vector3 GetMomentum(float coefficientBase = 0.5f, int maxDepth = 0) { return GetMomentum(coefficientBase, 1, maxDepth); }

    public static implicit operator Vector3(WaterPoint p) => p.position;

    class Orderer
    {
        int order;

        static readonly Lazy<Orderer> lazy = new Lazy<Orderer>(() => new Orderer());
        public static Orderer instance { get { return lazy.Value; } }

        Orderer() { }

        public int GetOrder() { return order++; }
    }
}
