/********************************************************************
	created:	2021/7/29 10:44:48
	file base:	Assets/Scripts/Base/Astar/AGraphData.cs
	author:		DESKTOP-EQS54EE

	purpose:	
*********************************************************************/
using System.Collections.Generic;
using System.Runtime.Serialization;
using ACE;
namespace ACE
{
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
    using UnityEngine;
#endif

    [DataContract]
    public class AGraphV2
    {
        [DataMember]
        public int x;
        [DataMember]
        public int y;

        public AGraphV2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [DataContract]
    public class AGraphVertex
    {
        [DataMember]
        public AGraphV2 vertex;
        [DataMember]
        public AGraphSideVertex side;

        public AGraphVertex(AGraphV2 vertex)
        {
            this.vertex = vertex;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(this.vertex.x, this.vertex.y);
        }
    }

    [DataContract]
    public class AGraphSideVertex
    {
        [DataMember]
        public int key;
        [DataMember]
        public int G;
        [DataMember]
        public List<AGraphV2> path;
        [DataMember]
        public AGraphSideVertex next;

        public AGraphSideVertex(int key, int g, List<AGraphV2> path)
        {
            this.key = key;
            G = g;
            this.path = path;
            this.next = null;
        }
    }

    [DataContract]
    public class AGraphData
    {
        [DataMember]
        public Dictionary<int, AGraphVertex> vertex;

        public AGraphData(AGraphV2[] vertexs)
        {
            this.vertex = new Dictionary<int, AGraphVertex>();
            for (int i = 0; i < vertexs.Length; i++)
            {
                int key = ANode.Key(vertexs[i].x, vertexs[i].y);
                if (this.vertex.ContainsKey(key))
                {
                    Debug.LogError("Repeated Key i == " + i + " x == " + vertexs[i].x + " y == " + vertexs[i].y);
                }
                this.vertex[key] = new AGraphVertex(vertexs[i]);
            }
        }

        public List<Vector2> GetPath(int startKey, int endKey)
        {
            if (this.vertex.ContainsKey(startKey) == false)
            {
                Debug.LogError("error startKey in GetPath startKey == " + startKey);
                return null;
            }
            else
            {
                AGraphSideVertex point = this.vertex[startKey].side;
                while (point != null)
                {
                    if (point.key == endKey)
                    {
                        List<Vector2> v2 = new List<Vector2>();
                        for (int i = 0; i < point.path.Count; i++)
                        {
                            v2.Add(new Vector2(point.path[i].x, point.path[i].y));
                        }
                        return v2;
                    }

                    point = point.next;
                }

                Debug.LogError("error in GetPath path == null");
                return null;
            }
        }

        public void AddSide(int key, int sideKey, int G, List<AGraphV2> path)
        {
            if (this.vertex[key] == null)
            {
                Debug.LogError("in AddSide Null Exception !!! idx == " + key);
                return;
            }
            else
            {
                AGraphSideVertex sideVertex = new AGraphSideVertex(sideKey, G, path);
                if (this.vertex[key].side == null)
                {
                    this.vertex[key].side = sideVertex;
                }
                else
                {
                    AGraphSideVertex point = this.vertex[key].side;
                    while (point.next != null)
                    {
                        point = point.next;
                    }

                    point.next = sideVertex;
                }
            }
        }
    }

}