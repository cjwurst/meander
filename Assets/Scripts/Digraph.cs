using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Digraph<T> where T : class
{
    public readonly HashSet<T> vertices;
    public IEnumerable<T> verticesBySuccessorCount
    {
        get
        {
            List<T> vs = new List<T>(vertices);
            return vs.OrderByDescending(vertex => CountSuccessors(vertex));
        }
    }
    public int vertexCount { get { return vertices.Count; } }

    HashSet<Tuple<T, T>> edges = new HashSet<Tuple<T, T>>();

    public Digraph(params T[] _vertices)
    {
        vertices = new HashSet<T>(_vertices);
    }

    public int CountSuccessors(T vertex)
    {
        T[] directSuccessors = GetDirectSuccessors(vertex);
        HashSet<T> verticesToTest = new HashSet<T>(directSuccessors);
        HashSet<T> successors = new HashSet<T>(directSuccessors);
        HashSet<T> testedVertices = new HashSet<T>(directSuccessors);
        while (verticesToTest.Count > 0)
        {
            foreach(T v in new HashSet<T>(verticesToTest))
            {
                if (!testedVertices.Contains(v))
                {
                    successors.Add(v);
                    testedVertices.Add(v);
                    verticesToTest.UnionWith(GetDirectSuccessors(v));
                }
                verticesToTest.Remove(v);
            }
        }
        return successors.Count;
    }

    public T[] GetDirectSuccessors(T vertex)
    {
        List<T> successors = new List<T>();
        foreach (Tuple<T, T> edge in edges)
            if (edge.Item1 == vertex && !successors.Contains(edge.Item2))
                successors.Add(edge.Item2);
        return successors.ToArray();
    }

    public void AddVertices(params T[] verticesToAdd) { foreach(T vertex in verticesToAdd) vertices.Add(vertex); }

    public void RemoveVertices(params T[] verticesToRemove)
    {
        vertices.ExceptWith(verticesToRemove);

        HashSet<Tuple<T, T>> edgesToRemove = new HashSet<Tuple<T, T>>();
        foreach (Tuple<T, T> edge in edges)
            if (Array.Exists(verticesToRemove, (t) => edge.Item1 == t || edge.Item2 == t))
                edgesToRemove.Add(edge);
        edges.ExceptWith(edgesToRemove);
    }

    public void AddEdges(params Tuple<T, T>[] edgesToAdd) { edges.UnionWith(edgesToAdd); }

    public void RemoveEdges(params Tuple<T, T>[] edgesToRemove) { edges.ExceptWith(edgesToRemove); }
}
