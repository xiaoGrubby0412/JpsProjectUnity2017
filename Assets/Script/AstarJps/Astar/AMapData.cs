using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using UnityEngine;
//using UnityEngine.AddressableAssets;
//using UnityEngine.ResourceManagement.AsyncOperations;

namespace ACE
{
    public class AMapData : AMapDataBase
    {
        public const string mapDataGatePath = "D://mapData//gate.txt";
        public const string mapDataBlockPath = "D://mapData//block.txt";
        public const string mapDataAreaPath = "D://mapData//area.txt";

        public const string mapDataGraphPath = "D://mapData//graph.txt";

        private static AMapData instance = null;

        public Dictionary<int, AGateData> gateData;
        public Dictionary<int, AreaData> areaData;

        public static AMapData Instance
        {
            get
            {
                if (instance == null)
                    instance = new AMapData(128, 128);
                return instance;
            }
        }

        public AMapData(int mapWidth, int mapHeight) : base(mapWidth, mapHeight)
        {
            this.gateData = new Dictionary<int, AGateData>();
            this.areaData = new Dictionary<int, AreaData>();
        }

        public List<Vector2> gatePositions;

        public override void OnSetBlock(int x, int y)
        {
            base.OnSetBlock(x, y);
            int key = ANode.Key(x, y);
            if (this.areaData.ContainsKey(key)) 
            {
                this.areaData.Remove(key);
            }
        }

        public void SetAreaData(int i, int x, int y)
        {
            if (IsBlock(x, y)) return;
            int key = ANode.Key(x, y);
            if (this.areaData.ContainsKey(key))
            {
                this.areaData[key].areaId = i;
            }
            else
            {
                this.areaData[key] = new AreaData(i, new AGraphV2(x, y));
            }
        }

        public bool IsAreaData(int i, int x, int y)
        {
            return this.areaData[i].areaId == i;
        }

        public void SetGate(int x, int y)
        {
            if (x > this.mapWidth - 1 || x < 0 || y > this.mapHeight - 1 || y < 0)
            {
                Debug.LogError("in AMapData SetDoor out bounds Error x == " + x + " y == " + y);
                return;
            }

            int key = ANode.Key(x, y);
            this.gateData[key] = new AGateData(x, y);
        }

        public bool IsGate(int x, int y)
        {
            int key = ANode.Key(x, y);
            return this.gateData.ContainsKey(key);
        }

        public void GenGateData(Vector2 pos)
        {
            if (gatePositions == null) gatePositions = new List<Vector2>();
            gatePositions.Add(pos);
        }

        public void GenGateDataFile()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < gatePositions.Count; i++)
            {
                sb.Append(gatePositions[i].x + "_" + gatePositions[i].y);
                if (i < gatePositions.Count - 1)
                    sb.Append(",");
            }

            string path = "D:\\gates.txt";
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            System.IO.File.WriteAllText("D:\\gates.txt", sb.ToString());
        }

        public override void GenGraphData()
        {
            base.GenGraphData();

            if (File.Exists(mapDataGraphPath))
            {
                string str = File.ReadAllText(mapDataGraphPath);
                DataContractJsonSerializer formatter = new DataContractJsonSerializer(typeof(AGraphData));
                using (MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(str)))
                {
                    AGraphData aGraph = formatter.ReadObject(stream) as AGraphData;
                    AMapData.Instance.graphData = aGraph;
                }
            }

            List<List<AGraphV2>> pathList = new List<List<AGraphV2>>();
            foreach (KeyValuePair<int, AGraphVertex> kvp in AMapData.Instance.graphData.vertex)
            {
                AGraphSideVertex point = kvp.Value.side;

                while (point != null)
                {
                    pathList.Add(point.path);
                    //ShowMarchLine(point.path);
                    point = point.next;
                }
            }

            DebugGrid.Instance.gatePaths = pathList;

            //Debug.LogError("All num == " + num);
        }

        int num = 0;
        public void ShowMarchLine(List<AGraphV2> p)
        {
            string gameObjectName = p[0].x + ":" + p[0].y + "__" + p[p.Count - 1].x + ":" + p[p.Count - 1].y;
            num++;
            List<Vector2> path = new List<Vector2>();
            for (int k = 0; k < p.Count; k++)
            {
                path.Add(new Vector2(p[k].x, p[k].y));
            }

            //string abName = "Assets/Addressables/Entities/World/Line/MarchingLine.prefab";

            //Addressables.InstantiateAsync(abName).Completed += (handle) =>
            //{
            //    if (handle.Status == AsyncOperationStatus.Succeeded) 
            //    {
            //        MarchLine line = new MarchLine();
            //        line.gameObject = handle.Result;
            //        line.gameObject.name = gameObjectName;
            //        line.gameObject.layer = LayerMask.NameToLayer("Game");
            //        line.SetSortingLayer("42");

            //        int fullMeter = 0;
            //        for (int i = 0; i < path.Count - 2; i++)
            //        {
            //            fullMeter = (int)(path[i + 1] - path[i]).magnitude + fullMeter;
            //        }

            //        int num = fullMeter * 5;

            //        line.Show(path.ToArray(), Color.green, 5, 1, num);
            //    }
            //};
        }


        public readonly int[,] drections = {
            { 0, -1 },//0
            { 1, -1 },//1
            { 1, 0 }, //2
            { 1, 1 }, //3
            { 0, 1 }, //4
            { -1, 1 },//5
            { -1, 0 },//6
            { -1, -1 }//7
        };

        public ANode getNeighbor(ANode _currentCell, int _dx, int _dy)
        {
            var x = _currentCell.X + _dx;
            var y = _currentCell.Y + _dy;

            if (IsOutBounds(x, y)) return null;
            else
                return new ANode(x, y);
        }
        public ANode getNeighbor_byDirID(ANode _currentCell, int _dir)
        {
            var x = _currentCell.X + this.drections[_dir, 0];
            var y = _currentCell.Y + this.drections[_dir, 1];

            if (IsOutBounds(x, y)) return null;
            else
                return new ANode(x, y);
        }
    }
}

