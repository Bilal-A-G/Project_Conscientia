using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeGenerator : MonoBehaviour
{
    [SerializeField] private float mapLength;
    [SerializeField] private Vector3 mapPosition;
    [SerializeField] private string groundTag;
    [SerializeField] private string obstacleTag;

    [SerializeField] private float smallestLength;
    
    private List<OctreeNode> _nodes;

    [ContextMenu("Regenerate Octree")]
    private void RegenerateOctree()
    {
        _nodes = new List<OctreeNode>();
        _nodes.Add(new OctreeNode(mapLength, mapPosition));

        Split(_nodes[0]);

        for (int i = 0; i < _nodes.Count; i++)
        {
            Collider[] colliders = Physics.OverlapBox(_nodes[i].position, new Vector3(_nodes[i].length, _nodes[i].length, _nodes[i].length));
            for (int v = 0; v < colliders.Length; v++)
            {
                if (colliders[v].tag == obstacleTag)
                {
                    _nodes[i].solid = true;
                }
            }
        }
    }

    private void Split(OctreeNode node)
    {
        Collider[] colliders = Physics.OverlapBox(node.position, new Vector3(node.length, node.length, node.length));
        bool touchObstacle = false;
        
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == obstacleTag)
            {
                touchObstacle = true;
            }
        }

        if (!touchObstacle) return;
        if (node.length <= smallestLength) return;

        _nodes.Remove(node);

        float halfLength = node.length / 2;
        float quarterLength = halfLength / 2;

        OctreeNode topLeftFront = new OctreeNode(halfLength, node.position + new Vector3(-quarterLength, quarterLength, quarterLength));
        OctreeNode topRightFront = new OctreeNode(halfLength, node.position + new Vector3(quarterLength, quarterLength, quarterLength));
        OctreeNode bottomLeftFront = new OctreeNode(halfLength, node.position + new Vector3(-quarterLength, -quarterLength, quarterLength));
        OctreeNode bottomRightFront = new OctreeNode(halfLength, node.position + new Vector3(quarterLength, -quarterLength, quarterLength));
        OctreeNode topLeftBack = new OctreeNode(halfLength, node.position + new Vector3(-quarterLength, quarterLength, -quarterLength));
        OctreeNode topRightBack = new OctreeNode(halfLength, node.position + new Vector3(quarterLength, quarterLength, -quarterLength));
        OctreeNode bottomLeftBack = new OctreeNode(halfLength, node.position + new Vector3(-quarterLength, -quarterLength, -quarterLength));
        OctreeNode bottomRightBack = new OctreeNode(halfLength, node.position + new Vector3(quarterLength, -quarterLength, -quarterLength));

        _nodes.Add(topLeftFront);
        _nodes.Add(topRightFront);
        _nodes.Add(bottomLeftFront);
        _nodes.Add(bottomRightFront);
        _nodes.Add(topLeftBack);
        _nodes.Add(topRightBack);
        _nodes.Add(bottomLeftBack);
        _nodes.Add(bottomRightBack);

        Split(topLeftFront);
        Split(topRightFront);
        Split(bottomLeftFront);
        Split(bottomRightFront);
        Split(topLeftBack);
        Split(topRightBack);
        Split(bottomLeftBack);
        Split(bottomRightBack);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            OctreeNode node = _nodes[i];
            Gizmos.color = node.color;
            if (!node.solid)
            {
               Gizmos.DrawWireCube(node.position, new Vector3(node.length, node.length, node.length));
            }
            else 
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(node.position, new Vector3(node.length, node.length, node.length));
            }
        }
    }
}

[System.Serializable]
public class OctreeNode
{
    public float length;
    public Vector3 position;
    public Color color;
    public bool solid;

    public OctreeNode(float length, Vector3 position) 
    {
        this.length = length;
        this.position = position;
        color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
    }
}
