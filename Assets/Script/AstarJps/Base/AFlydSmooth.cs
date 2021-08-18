/********************************************************************
	created:	2021/7/24 18:53:22
	file base:	Assets/Scripts/Base/Astar/AFlydSmooth.cs
	author:		DESKTOP-EQS54EE

	purpose:	
*********************************************************************/
using System.Collections.Generic;
using System;
using ACE.Mehroz;


namespace ACE
{
    public class Point2
    {
        public int x;
        public int y;

        public Point2(int px, int py)
        {
            x = px;
            y = py;
        }

        /// <summary>
        /// 输出点的坐标
        /// </summary>
        /// <returns>x:x y:y的字符串</returns>
        public override string ToString()
        {
            return "(x:" + x + ",y:" + y + ")";
        }

        /// <summary>
        /// 两点间距离
        /// </summary>
        /// <param name="p">第二个点</param>
        /// <returns>两点间距离</returns>
        public float Distance2(Point2 p)
        {
            var dx = p.x - x;
            var dy = p.y - y;
            return dx * dx + dy * dy;
        }

        /// <summary>
        /// 两点的坐标是否一样
        /// </summary>
        /// <param name="obj">另外一个点</param>
        /// <returns>两点的坐标一样则返回true</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if ((obj is Point2) == false)
            {
                return false;
            }
            Point2 ss = (Point2)obj;
            return ss.x == x && ss.y == y;
        }
    }

    public class AFlydSmooth
    {

        public static AMapDataBase aMapData;

        public static List<int> GetSmoothList(List<int> list, int p_maxCount, int NUM_PER_SESS, AMapDataBase aMapData)
        {
            AFlydSmooth.aMapData = aMapData;
            if (NUM_PER_SESS == 0) NUM_PER_SESS = 13;
            NUM_PER_SESS = NUM_PER_SESS * 3;
            int index = 0;

            while (true)
            {
                if (index >= list.Count - 3) break;

                int curX = list[index];
                int curY = list[index + 2];

                for (int i = index + NUM_PER_SESS; i > index; i -= 3)
                {

                    if (i > list.Count - 3)
                    {
                        i = list.Count - 3;
                    }
                    int freeX = -1;
                    int freeY = -1;

                    if (index == 3 && i == 12)
                    {
                        int c = 1;
                    }

                    if (!CheckBlock(curX / 10 + 0.5f, curY / 10 + 0.5f, list[i] / 10, list[i + 2] / 10, out freeX, out freeY))
                    {
                        list.RemoveRange((index + 3), (i - index - 3));
                        index += 3;
                        break;
                    }

                    if (i == (index + 3))
                    {
                        index += 3;
                        break;
                    }
                }
            }
            return list;
        }

        static void AddPoint2(List<Point2> list, int x, int y)
        {
            Point2 p = new Point2(x, y);
            if (list.Contains(p) == false)
            {
                list.Add(p);
            }
        }


        static bool IsBlock(int x, int y)
        {
            return AFlydSmooth.aMapData.IsBlock(x, y);
        }


        public static bool CheckBlock(float startX, float startY, int endX, int endY, out int freeX, out int freeY)
        {

            Point2 startPoint = new Point2((int)startX, (int)startY);

            Point2 endPoint = new Point2(endX, endY);


            int minY = Math.Min(startPoint.y, endPoint.y);
            int maxY = Math.Max(startPoint.y, endPoint.y);

            int minX = Math.Min(startPoint.x, endPoint.x);
            int maxX = Math.Max(startPoint.x, endPoint.x);

            List<Point2> pointList = new List<Point2>();


            AddPoint2(pointList, startPoint.x, startPoint.y);
            AddPoint2(pointList, endPoint.x, endPoint.y);


            if (endPoint.x == startPoint.x)
            {
                int x = endPoint.x;
                for (int y = minY; y <= maxY; y++)
                {
                    AddPoint2(pointList, x, y);
                }
            }
            else if (endPoint.y == startPoint.y)
            {
                int y = endPoint.y;
                for (int x = minX; x <= maxX; x++)
                {
                    AddPoint2(pointList, x, y);
                }
            }
            else
            {
                float x1 = startX;
                float y1 = startY;
                float x2 = endPoint.x + 0.5f;
                float y2 = endPoint.y + 0.5f;

                long dx = (long)(x2 * 10 - x1 * 10);
                long dy = (long)(y2 * 10 - y1 * 10);

                FractionV2 K = new FractionV2(dy, dx);
                FractionV2 K2 = new FractionV2(dx, dy);
                FractionV2 B = new FractionV2((long)(y1 * 1000), 1000) - K * new FractionV2((long)(x1 * 1000), 1000);


                for (int x = minX; x <= maxX; x++)
                {
                    float yyy = (float)((K * (new FractionV2(x)) + B).ToDouble());
                    int y = (int)Math.Floor(yyy);

                    if (y >= minY && y <= maxY)
                    {
                        AddPoint2(pointList, x, y);

                        if (x - 1 >= minX)
                        {
                            AddPoint2(pointList, x - 1, y);
                        }

                        int iiy = (int)(yyy * 1000);
                        if (y * 1000 == iiy && (y - 1 >= minY))
                        {
                            AddPoint2(pointList, x - 1, y - 1);
                        }
                    }
                }


                for (int y = minY; y <= maxY; y++)
                {
                    float xxx = (float)((new FractionV2(y) - B) * K2).ToDouble();
                    int x = (int)Math.Floor(xxx);
                    if (x >= minX && x <= maxX)
                    {
                        AddPoint2(pointList, x, y);

                        if (y - 1 >= minY)
                        {
                            AddPoint2(pointList, x, y - 1);
                        }

                        int iix = (int)(xxx * 1000);
                        if (x * 1000 == iix && (x - 1 >= minX))
                        {
                            AddPoint2(pointList, x - 1, y - 1);
                        }
                    }
                }
            }


            pointList.Sort(delegate (Point2 a, Point2 b)
            {
                float aa = a.Distance2(startPoint);
                float bb = b.Distance2(startPoint);
                if (aa < bb)
                {
                    return -1;
                }
                return 1;
            }
            );

            Point2 lastFreePoint = startPoint;

            if (pointList.Count > 0)
            {
                var firstPoint = pointList[0];
                if (IsBlock(firstPoint.x, firstPoint.y))
                {
                    lastFreePoint.x = -1;
                    lastFreePoint.y = -1;
                }
            }

            for (int i = 0; i < pointList.Count; i++)
            {

                var a = pointList[i];

                if (IsBlock(a.x, a.y))
                {
                    freeX = lastFreePoint.x;
                    freeY = lastFreePoint.y;
                    return true;
                }
                lastFreePoint.x = a.x;
                lastFreePoint.y = a.y;

                if (i < pointList.Count - 1)
                {
                    var b = pointList[i + 1];
                    //-中点
                    int hx = (int)((a.x + b.x) / 2.0f);
                    int hy = (int)((a.y + b.y) / 2.0f);
                    if (IsBlock(hx, hy))
                    {
                        freeX = lastFreePoint.x;
                        freeY = lastFreePoint.y;
                        return true;
                    }
                    lastFreePoint.x = hx;
                    lastFreePoint.y = hy;

                }
            }
            freeX = endPoint.x;
            freeY = endPoint.y;
            return false;
        }

    }
}
