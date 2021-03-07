using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Undirected Graph where each vertice is unique.
/// </summary>
/// <typeparam name="T"></typeparam>
public class HashGraph<T> where T : IEquatable<T>
{
    Dictionary<T, GraphNode<T>> AllNodes;

    class GraphNode<K> : IEquatable<GraphNode<K>> where K : IEquatable<K>
    {
        public K value;
        public HashSet<GraphNode<K>> adjacents = null;
        public GraphNode(K item)
        {
            value = item;
        }

        public bool Equals(GraphNode<K> g)
        {
            return value is ValueType ? value.Equals(g.value) :
                (ReferenceEquals(value, null) ? ReferenceEquals(g.value, null) : value.Equals(g.value));
        }
        public override bool Equals(object obj)
        {
            if (obj is GraphNode<K>)
                return Equals((GraphNode<K>)obj);
            return false;
        }
        public override int GetHashCode()
        {
            if (value is ValueType)
                return value.GetHashCode();
            else
            {
                if (ReferenceEquals(value, null))
                    return 0;
                else
                    return value.GetHashCode();
            }
        }

        public void Add(GraphNode<K> adj)
        {
            if (adjacents == null)
                adjacents = new HashSet<GraphNode<K>>();
            adjacents.Add(adj);
        }
        public bool Remove(GraphNode<K> adj)
        {
            if (adjacents == null) return false;
            return adjacents.Remove(adj);
        }
        public bool Contains(GraphNode<K> adj)
        {
            return adjacents == null ? false : adjacents.Contains(adj);
        }
    }

    /// <summary>
    /// Add vertex
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
        if (AllNodes == null)
            AllNodes = new Dictionary<T, GraphNode<T>>();
        if (AllNodes.ContainsKey(item))
            throw new System.Exception("Graph already contains input item");
        AllNodes.Add(item, new GraphNode<T>(item));
    }

    /// <summary>
    /// Remove vertex. Also destroys any edge connections
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove(T item)
    {
        if (AllNodes == null || AllNodes.Count == 0 || !AllNodes.ContainsKey(item))
            return false;
        else
        {
            GraphNode<T> node = AllNodes[item];
            if (node.adjacents != null)
                foreach (GraphNode<T> adj in node.adjacents)
                    adj.adjacents.Remove(node);
            AllNodes.Remove(item);
            return true;
        }
    }

    /// <summary>
    /// Add an edge between two already existing vertices
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    public void AddEdge(T v1, T v2)
    {
        if (AllNodes == null || !AllNodes.ContainsKey(v1) || !AllNodes.ContainsKey(v2))
            throw new System.Exception("Graph does not contain input element");
        AllNodes[v1].Add(AllNodes[v2]);
        AllNodes[v2].Add(AllNodes[v1]);
    }

    /// <summary>
    /// Contains a vertice?
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(T item)
    {
        return AllNodes == null ? false : AllNodes.ContainsKey(item);
    }

    /// <summary>
    /// Remove an edge between two already existing vertices. Returns false if no edge was removed.
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    public bool RemoveEdge(T v1, T v2)
    {
        if (AllNodes == null || !AllNodes.ContainsKey(v1) || !AllNodes.ContainsKey(v2))
            throw new System.Exception("Graph does not contain input element");
        GraphNode<T> g1 = AllNodes[v1];
        GraphNode<T> g2 = AllNodes[v2];
        return g1.Remove(g2) && g2.Remove(g1);
    }
    /// <summary>
    /// Returns true if the input vertices share an edge
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public bool HasEdge(T v1, T v2)
    {
        if (AllNodes == null || !AllNodes.ContainsKey(v1) || !AllNodes.ContainsKey(v2))
            return false;
        GraphNode<T> g1 = AllNodes[v1];
        GraphNode<T> g2 = AllNodes[v2];
        return g1.Contains(g2) && g2.Contains(g1);
    }

    /// <summary>
    /// Returns all adjacent nodes of an input node
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public IEnumerable<T> GetAdjacent(T item)
    {
        if (AllNodes == null || !AllNodes.ContainsKey(item))
            throw new System.Exception("Graph does not contain input element");

        HashSet<GraphNode<T>> adjacents = AllNodes[item].adjacents;
        if (adjacents != null)
            foreach (GraphNode<T> adj in adjacents)
                yield return adj.value;
    }

    /// <summary>
    /// All graph vertices
    /// </summary>
    public IEnumerable<T> AllVertices
    {
        get
        {
            if (AllNodes != null)
                foreach (T val in AllNodes.Keys)
                {
                    yield return val;
                }
        }
    }

    /// <summary>
    /// Runs connected component analysis to get a list of connected neighborhoods
    /// </summary>
    /// <returns></returns>
    public List<List<T>> ConnectedComponent()
    {
        List<List<T>> cc = new List<List<T>>();

        if (AllNodes == null)
            return cc;

        HashSet<T> closed_nodes = new HashSet<T>();

        //iterate through every node
        foreach (GraphNode<T> g in AllNodes.Values)
        {
            //if this node is not closed
            if (!closed_nodes.Contains(g.value))
            {
                //get connected nodes
                List<T> neighborhood = GetNeighborhood(g);
                //add the new neighborhood
                cc.Add(neighborhood);
                //close the found nodes
                neighborhood.ForEach(x => closed_nodes.Add(x));
            }
        }

        return cc;
    }

    /// <summary>
    /// AStar path finding
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="cost"></param>
    /// <param name="heuristic"></param>
    /// <returns></returns>
    public List<T> AStar(T from, T to, Func<T, T, double> cost, Func<T, double> heuristic)
    {
        MinHeap<NodeValuePair> Open = new MinHeap<NodeValuePair>();
        Dictionary<GraphNode<T>, GraphNode<T>> Closed = new Dictionary<GraphNode<T>, GraphNode<T>>();

        if (!Contains(from) || !Contains(to))
            throw new System.Exception("Error, graph does not contain an input node");

        //current and goal nodes
        var node = new NodeValuePair(AllNodes[from], null, 0 + heuristic(from));
        var goal = new NodeValuePair(AllNodes[to], null, 0);
        //insert start node
        Open.Insert(node);
        //iterate until all nodes explored
        while (Open.Count > 0)
        {
            //get min
            node = Open.ExtractMin();

            //if goal, set previous and break
            if (node.node.Equals(goal.node))
            {
                goal.previous = node.previous;
                break;
            }

            if (!Closed.ContainsKey(node.node))
            {
                double previous_path_cost = node.value - heuristic(node.node.value);
                //add to closed set
                Closed.Add(node.node, node.previous);
                //for all adjacent unexplored nodes, add to open set
                foreach (var n in GetAdjacent(node.node.value))
                {
                    var gnode = AllNodes[n];
                    //if not explored
                    if (!Closed.ContainsKey(gnode))
                    {
                        Open.Insert(new NodeValuePair(gnode, node.node,
                            previous_path_cost + cost(node.node.value, gnode.value) + heuristic(gnode.value)));
                    }
                }
            }
        }

        //retrace shortest path
        List<T> result = new List<T>();
        result.Add(goal.node.value);
        var current = goal.previous;
        while (current != null)
        {
            result.Add(current.value);
            current = Closed[current];
        }

        return result;
    }

    /// <summary>
    /// Returns a list of connected nodes
    /// </summary>
    /// <param name="g"></param>
    /// <param name="elements"></param>
    /// <returns></returns>
    List<T> GetNeighborhood(GraphNode<T> g, HashSet<T> elements = null)
    {
        //the first recurssion
        if (elements == null)
        {
            elements = new HashSet<T>();
        }

        //add this value
        elements.Add(g.value);

        //if this node has adjacent nodes
        if (g.adjacents != null)
            foreach (GraphNode<T> adj in g.adjacents)
            {
                //if this node is not visited
                if (!elements.Contains(adj.value))
                {
                    //run connectednodes on its adjacent node
                    GetNeighborhood(adj, elements);
                }
            }

        return elements.ToList();
    }

    /// <summary>
    /// return the graph in string format
    /// </summary>
    /// <returns></returns>
    public string PrintGraph()
    {
        string val = "";
        if (AllNodes != null)
            foreach (GraphNode<T> g in AllNodes.Values)
            {
                val += "V: " + g.value.ToString() + "\n";
                val += "Adj: \n";
                if (g.adjacents != null)
                    foreach (GraphNode<T> adj in g.adjacents)
                    {
                        val += adj.value.ToString() + "\n";
                    }
                val += "\n";
            }
        return val;
    }

    struct NodeValuePair : IComparable<NodeValuePair>
    {
        public double value;
        public GraphNode<T> node;
        public GraphNode<T> previous;

        public NodeValuePair(GraphNode<T> _node, GraphNode<T> prev, double val)
        {
            node = _node; value = val; previous = prev;
        }

        public int CompareTo(NodeValuePair s)
        {
            return value.CompareTo(s.value);
        }
    }
}