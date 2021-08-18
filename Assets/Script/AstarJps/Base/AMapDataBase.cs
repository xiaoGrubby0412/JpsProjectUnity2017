/********************************************************************
	created:	2021/7/24 18:53:22
	file base:	Assets/Scripts/Base/Astar/AMapData.cs
	author:		DESKTOP-EQS54EE

	purpose:	
*********************************************************************/
using System.Collections.Generic;
using System.Text;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
using UnityEngine;
#endif

namespace ACE
{
    public enum AMapDataType
    {
        Normal = 0,
        Block = 1,
        SlowDown = 2
    }

    public class AMapDataBase
    {
        public int[][] blockData;
        public int mapWidth;
        public int mapHeight;
        public AGraphData graphData;

        public AMapDataBase(int mapWidth, int mapHeight)
        {
            this.mapWidth = mapWidth == 0 ? 1400 : mapWidth;
            this.mapHeight = mapHeight == 0 ? 1400 : mapHeight;
        }

        public void SetMapSize(int mapWidth, int mapHeight)
        {
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            this.blockData = new int[this.mapWidth][];
            for (int i = 0; i < this.blockData.Length; i++)
                this.blockData[i] = new int[this.mapHeight];
        }

        public AMapDataType GetMapDataType(int x, int y)
        {
            if (x > this.mapWidth - 1 || x < 0 || y > this.mapHeight - 1 || y < 0 || this.blockData[x][y] == (int)AMapDataType.Block)
            {
                return AMapDataType.Block;
            }
            else
            {
                return AMapDataType.Normal;
            }
        }

        public bool IsBlock(ANode aNode) 
        {
            return IsBlock(aNode.X, aNode.Y);
        }

        public bool IsBlock(int x, int y)
        {

            if (x > this.mapWidth - 1 || x < 0 || y > this.mapHeight - 1 || y < 0)
            {
                return true;
            }
            else
            {
                return this.blockData[x][y] == (int)AMapDataType.Block;
            }
        }

        public bool IsOutBounds(int x, int y) 
        {
            return (x > this.mapWidth - 1 || x < 0 || y > this.mapHeight - 1 || y < 0);
        }

        public void SetBlock(int x, int y)
        {
            if (x > this.mapWidth - 1 || x < 0 || y > this.mapHeight - 1 || y < 0)
            {
                Debug.LogError("in AMapData SetBlock out bounds Error x == " + x + " y == " + y);
                return;
            }

            this.blockData[x][y] = (int)AMapDataType.Block;

            OnSetBlock(x, y);

        }

        public virtual void OnSetBlock(int x, int y)
        {

        }

        public void SetNormal(int x, int y)
        {
            if (x > this.mapWidth - 1 || x < 0 || y > this.mapHeight - 1 || y < 0)
            {
                Debug.LogError("in AMapData SetBlock out bounds Error x == " + x + " y == " + y);
                return;
            }

            this.blockData[x][y] = (int)AMapDataType.Normal;
        }

        public virtual void GenGraphData()
        {

        }
    }





}
