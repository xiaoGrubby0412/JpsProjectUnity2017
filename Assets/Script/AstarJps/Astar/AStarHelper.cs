/********************************************************************
	created:	2021/7/24 18:53:22
	file base:	Assets/Scripts/Base/Astar/AStarHelper.cs
	author:		DESKTOP-EQS54EE

	purpose:	
*********************************************************************/
using System.Collections.Generic;
using System;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
using UnityEngine;
#endif

namespace ACE
{
    public enum SearchType
    {
        dir = 0,
        graph = 1,
        all = 2
    }

    public class AstarHelperManager
    {
        readonly int[,] DIR = { { 0, -1 }, { 0, 1 }, { -1, 0 }, { 1, 0 }, { -1, -1 }, { 1, -1 }, { 1, 1 }, { -1, 1 } };

        public int currentStep;
        public int maxStep;
        public int startX;
        public int startY;
        public int endX;
        public int endY;
        public ANode finalNode;
        public List<int> pathList;
        public List<Vector2> pathListV2;
        public Dictionary<int, ANode> closeDict;
        public Dictionary<int, ANode> openDict;
        public PriorityQueue<ANode> sortedOpenQue;
        public int G;
        public SearchType searchType;
        private AMapData aMapData;
        private int startAreaID;
        private int endAreaID;

        public AstarHelperManager(AMapDataBase aMapData)
        {
            this.aMapData = aMapData as AMapData;
            this.sortedOpenQue = new PriorityQueue<ANode>(65535, new ANodeCom());
            this.openDict = new Dictionary<int, ANode>();
            this.closeDict = new Dictionary<int, ANode>();
        }

        public void SetMapData(AMapDataBase aMapData)
        {
            this.aMapData = aMapData as AMapData;
        }

        public bool FindPath(int startX, int startY, int endX, int endY, int p_maxStep, int G, bool ifNum, bool ifFlyd, int flydNum, SearchType searchType)
        {
            this.startX = startX;
            this.startY = startY;
            this.endX = endX;
            this.endY = endY;

            int nodeKey = ANode.Key(startX, startY);
            if (this.aMapData.areaData.ContainsKey(nodeKey))
            {
                this.startAreaID = this.aMapData.areaData[nodeKey].areaId;
            }

            nodeKey = ANode.Key(endX, endY);
            if (this.aMapData.areaData.ContainsKey(nodeKey))
            {
                this.endAreaID = this.aMapData.areaData[nodeKey].areaId;
            }
            

            if (this.aMapData.IsBlock(startX, startY))
            {
                Debug.LogError("startX startY is Block !!! startX == " + startX + " startY == " + startY);
                return false;
            }

            if (this.aMapData.IsBlock(endX, endY))
            {
                Debug.LogError("endX endY is Block !!! endX == " + endX + " endY == " + endY);
                return false;
            }

            if (p_maxStep < 1)
            {
                p_maxStep = this.aMapData.mapWidth * this.aMapData.mapHeight;
            }

            Init(startX, startY, endX, endY, p_maxStep, G, searchType);
            StartFind();

            if (this.finalNode == null)
            {
                Debug.LogError("no path found return null curStep == " + this.currentStep);
                return false;
            }
            else
            {
                Debug.Log("curStep == " + this.currentStep);
                if (this.searchType == SearchType.graph)
                {

                    int keyStart = ANode.Key(this.startX, this.startY);
                    int keyEnd = ANode.Key(this.finalNode.X, this.finalNode.Y);
                    this.pathListV2 = this.aMapData.graphData.GetPath(keyStart, keyEnd);
                }
                else if (this.searchType == SearchType.dir)
                {
                    NodeToList();
                    if (ifFlyd)
                    {
                        this.pathList = AFlydSmooth.GetSmoothList(this.pathList, 65536, flydNum, this.aMapData);
                    }

                    if (ifNum == false)
                    {
                        pathListV2 = new List<Vector2>();
                        for (int i = 0; i <= this.pathList.Count - 3; i = i + 3)
                        {
                            Vector2 v = new Vector2(this.pathList[i] / 10, this.pathList[i + 2] / 10);
                            pathListV2.Add(v);
                        }
                    }
                }
                else
                {
                    NodeToList();

                    if (ifNum == false)
                    {
                        pathListV2 = new List<Vector2>();
                        for (int i = 0; i <= this.pathList.Count - 3; i = i + 3)
                        {
                            Vector2 v = new Vector2(this.pathList[i] / 10, this.pathList[i + 2] / 10);
                            pathListV2.Add(v);
                        }
                    }
                }
                return true;
            }
        }

        public void NodeToList()
        {
            this.pathList = new List<int>();
            ANode cur = this.finalNode;

            int finalG = this.finalNode.G;
            int num = 0;
            int numSlow = 0;
            int numNormal = 0;

            while (cur != null)
            {
                pathList.Insert(0, cur.X * 10);
                pathList.Insert(1, 0);
                pathList.Insert(2, cur.Y * 10);

                num = num + 1;
                if (cur.NodeType == AMapDataType.SlowDown)
                    numSlow = numSlow + 1;
                else if (cur.NodeType == AMapDataType.Normal)
                    numNormal = numNormal + 1;
                else
                    Debug.LogError("Error!!!!!!!!!!!!!!!!!!!!!!!!!!!! type == " + cur.NodeType);

                cur = cur.Parent;
            }

            Debug.Log("final node.G == " + finalG + " all num == " + num + " numSlow == " + numSlow + " numNormal == " + numNormal);
        }


        private bool Init(int startX, int startY, int endX, int endY, int p_maxStep, int G, SearchType searchType)
        {
            maxStep = p_maxStep;
            this.G = G;
            currentStep = 0;
            finalNode = null;
            pathList = null;
            pathListV2 = null;
            this.searchType = searchType;

            //初始化open表
            for (int i = 0; i < sortedOpenQue.Count; i++)
            {
                FreeANodePool.Instance.PushFreeNodeToPool(sortedOpenQue.heap[i]);
            }

            sortedOpenQue.Clear();
            openDict.Clear();


            //初始化close表
            foreach (var n in closeDict.Values)
            {
                FreeANodePool.Instance.PushFreeNodeToPool(n);
            }

            closeDict.Clear();

            ANode startNode = FreeANodePool.Instance.GetFreeNode();
            startNode.X = startX;
            startNode.Y = startY;
            startNode.Parent = null;

            this.startX = startX;
            this.startY = startY;
            this.endX = endX;
            this.endY = endY;

            startNode.G = 0;
            startNode.H = GetNodeH(startX, startY, endX, endY);
            startNode.F = startNode.H + startNode.G;
            startNode.NodeType = this.aMapData.GetMapDataType(startX, startY);
            sortedOpenQue.Push(startNode);

            int key = ANode.Key(startX, startY);
            openDict.Add(key, startNode);

            return true;
        }

        ////找最近非阻挡点
        //static void GetNearestNoBlockPos(ref int _dx, ref int _dy)
        //{
        //    Debug.LogError("in GetNearestNoBlockPos _dx = " + _dx + " _dy == " + _dy);
        //    int dis = 1;
        //    while (dis < 50)
        //    {
        //        for (int x = _dx - dis; x < _dx + dis; x++)
        //        for (int z = _dy - dis; z < _dy + dis; z++)
        //        {
        //            Debug.LogError("dis == " + dis + " x == " + x + " z == " + z);
        //            if (mapDataArr2[x, z] == 0)
        //            {
        //                _dx = x;
        //                _dy = z;
        //                return;
        //            }
        //        }

        //        dis++;
        //    }
        //}

        public int GetNodeH(int _nodeX, int _nodeY, int endX, int endY)
        {
            //int _nodeH = 10 * (Math.Abs(endX - _nodeX) + Math.Abs(endY - _nodeY));
            //return _nodeH;

            int xm = Math.Abs(endX - _nodeX);
            int ym = Math.Abs(endY - _nodeY);
            int leng = xm * xm + ym * ym;
            int _nodeH = (int)(10 * Math.Sqrt(leng));
            //Debug.LogError("X == " + _nodeX + " Y == " + _nodeY + " return nodeH == " + _nodeH);
            return _nodeH;
        }

        public List<ANode> GetAllSide(ANode fNode)
        {
            List<ANode> lst = new List<ANode>();

            if (this.searchType == SearchType.dir || this.searchType == SearchType.all)
            {
                for (int i = 0; i < 8; i++)
                {
                    int _nodeX = fNode.X + DIR[i, 0];
                    int _nodeY = fNode.Y + DIR[i, 1];

                    if (this.aMapData.IsBlock(_nodeX, _nodeY)) continue;
                    //if (i > 3 && (this.aMapData.IsBlock(fNode.X, _nodeY) || this.aMapData.IsBlock(_nodeX, fNode.Y))) continue;
                    if (this.closeDict.ContainsKey(ANode.Key(_nodeX, _nodeY))) continue;

                    if (this.searchType == SearchType.all)
                    {
                        int nodeKey = ANode.Key(_nodeX, _nodeY);
                        if ((this.aMapData.graphData != null && !this.aMapData.graphData.vertex.ContainsKey(nodeKey)) || this.aMapData.graphData == null)
                        {
                            if (!this.aMapData.areaData.ContainsKey(nodeKey)) continue;
                            int nodeAreaId = this.aMapData.areaData[nodeKey].areaId;
                            if (nodeAreaId != startAreaID && nodeAreaId != endAreaID)
                                continue;
                        }
                    }

                    ANode node = FreeANodePool.Instance.GetFreeNode();
                    node.X = _nodeX;
                    node.Y = _nodeY;

                    int length;

                    if (i < 4)
                    {
                        length = 10;
                    }
                    else
                    {
                        length = 14;
                    }

                    AMapDataType nodeType = this.aMapData.GetMapDataType(node.X, node.Y);
                    if (nodeType == AMapDataType.SlowDown)
                    {
                        node.G = fNode.G + length * this.G;
                    }
                    else
                    {
                        node.G = fNode.G + length;
                    }

                    node.H = this.GetNodeH(node.X, node.Y, this.endX, this.endY);
                    node.F = node.G + node.H;

                    lst.Add(node);
                }
            }

            if (this.searchType == SearchType.graph || this.searchType == SearchType.all)
            {
                int key = ANode.Key(fNode.X, fNode.Y);

                if (this.aMapData.graphData != null && this.aMapData.graphData.vertex.ContainsKey(key))
                {
                    AGraphSideVertex point = this.aMapData.graphData.vertex[key].side;
                    while (point != null)
                    {
                        AGraphV2 v2 = this.aMapData.graphData.vertex[point.key].vertex;
                        if (this.aMapData.IsBlock(v2.x, v2.y)) { point = point.next; continue; }
                        if (this.closeDict.ContainsKey(ANode.Key(v2.x, v2.y))) { point = point.next; continue; }

                        ANode node = FreeANodePool.Instance.GetFreeNode();
                        node.X = v2.x;
                        node.Y = v2.y;
                        node.G = fNode.G + point.G;
                        node.H = this.GetNodeH(node.X, node.Y, this.endX, this.endY);
                        node.F = node.G + node.H;
                        lst.Add(node);

                        point = point.next;
                    }
                }
            }
            return lst;
        }


        public void StartFind()
        {
            while (sortedOpenQue.Count > 0 && currentStep <= maxStep)
            {
                ANode minFNode = sortedOpenQue.Pop();

                int _num = ANode.Key(minFNode.X, minFNode.Y);
                if (openDict.ContainsKey(_num))
                {
                    openDict.Remove(_num);
                }

                closeDict.Add(_num, minFNode);
                currentStep++;


                //判断目标点，找到就结束
                if (minFNode.X == endX && minFNode.Y == endY)
                {
                    finalNode = minFNode;
                    break;
                }

                List<ANode> listV2 = this.GetAllSide(minFNode);

                for (int i = 0; i < listV2.Count; i++)
                {
                    ANode node = listV2[i];
                    int keyNum = ANode.Key(node.X, node.Y);

                    //查open
                    if (openDict.ContainsKey(keyNum))
                    {
                        int sortK = -1;
                        bool ifSetK = false;
                        for (int k = 0; k < sortedOpenQue.Count; k++)
                        {
                            ANode nn = sortedOpenQue.heap[k];
                            if (node.X == nn.X && node.Y == nn.Y && node.G < nn.G)
                            {
                                if (ifSetK)
                                {
                                    Debug.LogError("ifSetK == True ..........................................");
                                }
                                nn.G = node.G;
                                nn.F = node.F;
                                nn.Parent = minFNode;
                                //if (sortK != -1)
                                //{
                                sortK = k;
                                ifSetK = true;
                                break;
                                //} 
                            }
                        }

                        if (sortK != -1)
                        {
                            sortedOpenQue.Sort(sortK);
                        }

                        continue;
                    }

                    node.NodeType = this.aMapData.GetMapDataType(node.X, node.Y);
                    node.Parent = minFNode;

                    sortedOpenQue.Push(node);

                    openDict.Add(keyNum, node);

                }
            }
        }
    }
}
