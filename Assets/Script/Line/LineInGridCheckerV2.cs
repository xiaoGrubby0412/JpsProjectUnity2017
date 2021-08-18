using System;
using System.Collections.Generic;
using ACE.Mehroz;
using UnityEngine;
using ACE;
/**
 * 1223 KK+ 
 * 检查直线在线段内是否与某些障碍物相交的工具类
 *  * TODO KK
 * 测试组件，测试各种东西，目前是要测试一个算法那
 * 			int blockX=-1;
			int blockY=-1;
			hasBlock=LineInGridChecker.CheckBlock(startPosi.x,startPosi.y,endPosi.x,endPosi.y,IsBlock,out blockX,out blockY);
			blockPosi.x=blockX;
			blockPosi.y=blockY;
			
	bool IsBlock(int x,int y){
		if(x<0||x>gridArr2.GetLength(1)-1 || y<0 || y>gridArr2.GetLength(0)){
			Debug.LogError("越界了y40,x60 "+x+","+y);
			return false;
		}
			
		return gridArr2[y,x]==1;
	}
 */ 

///<summary>
/// 检查线段是否与某些障碍物相交的工具类
/// </summary>
internal class LineInGridCheckerV2
{
	public static bool bDebug=false;
	
	const int PRECISION=10000;//精度
	
	/// <summary>
	/// 被外部传入，用来确定此点是否是障碍点
	/// </summary>
	/// <param name="x">障碍点坐标</param>
	/// <param name="y">障碍点坐标</param>
	/// <returns>是否为障碍点</returns>
	public delegate bool FuncIsBlock(int x,int y);
	
	static void AddPoint2(List<Point2> list,int x,int y){
		Point2 p=new Point2(x,y);
		if(list.Contains(p)==false){
			list.Add(p);
		}
	}
	
	///<summary>startXY是float,就是玩家所在的位置,精度为1000
	/// endXY是指定的格子的中心，比如(1,1),代表(1,1)格的格子中心
	/// freeX,freeY是指能移动到的最远距离
	/// </summary>
	public static bool CheckBlock(float startX,float startY,int endX,int endY,FuncIsBlock func_isBlock,out int freeX,out int freeY){
		//--0:数据准备
		Point2 startPoint=new Point2((int)startX,(int)startY);
		
		Point2 endPoint=new Point2(endX,endY);
	

		int minY=Mathf.Min(startPoint.y,endPoint.y);
		int maxY=Mathf.Max(startPoint.y,endPoint.y);
		
		int minX=Mathf.Min(startPoint.x,endPoint.x);
		int maxX=Mathf.Max(startPoint.x,endPoint.x);
		
		List<Point2> pointList=new List<Point2>();//焦点集合
		
		//--
		AddPoint2(pointList,startPoint.x,startPoint.y);
		AddPoint2(pointList,endPoint.x,endPoint.y);
		
		//--1 填充交点
		if(endPoint.x==startPoint.x){
			int x=endPoint.x;
			for(int y=minY;y<=maxY;y++){
				AddPoint2(pointList,x,y);
			}
		}else if(endPoint.y==startPoint.y){
			int y=endPoint.y;
			for(int x=minX;x<=maxX;x++){
				AddPoint2(pointList,x,y);
			}
		}else{//用数学直线方程算
			float x1=startX;//startPoint.x+0.5f;
			float y1=startY;//startPoint.y+0.5f;
			float x2=endPoint.x+0.5f;
			float y2=endPoint.y+0.5f;
			
			long dx=(long)(x2*10-x1*10);
			long dy=(long)(y2*10-y1*10);
			//1:求出直线公式y=kx+b
			FractionV2 K=new FractionV2(dy,dx);
			FractionV2 K2=new FractionV2(dx,dy);
			FractionV2 B=new FractionV2((long)(y1*PRECISION),PRECISION)-K*new FractionV2((long)(x1*PRECISION),PRECISION);
			
			//2:先进行x方向递增，算出位置
			for(int x=minX;x<=maxX;x++){
				float yyy=(float)((K*(new FractionV2(x))+B).ToDouble());
				int	y=Mathf.FloorToInt(yyy);
				
				if(y>=minY && y<=maxY){//非常重要，否则会有越界值，在线段的延长线上了
					AddPoint2(pointList,x,y);
				}
			}

			//3:再进行y方向递增，算出位置
			for(int y=minY;y<=maxY;y++){
				float xxx=(float)((new FractionV2(y)-B)*K2).ToDouble();
				int	x=Mathf.FloorToInt(xxx);
				if(x>=minX && x<=maxX){
					AddPoint2(pointList,x,y);
				}
			}
		}
		
		//--2:排序点,按照距离起始点的距离排
		pointList.Sort(delegate(Point2 a, Point2 b){
							float aa =a.Distance2(startPoint);
							float bb =b.Distance2(startPoint);
							if (aa < bb) {
								return -1;
							}
							return 1;	
						}
		);

		//--3,逐个扫描,扫描每个点，和两点的中点
		Point2 lastFreePoint=startPoint;//上一个正常的位置,无障碍点
		
		//安全措施，如果起始点就是障碍点，返回-1
		if(pointList.Count>0){
			var firstPoint=pointList[0];
			if(func_isBlock(firstPoint.x,firstPoint.y)){
				lastFreePoint.x=-1;
				lastFreePoint.y=-1;
			}
		}
		
		for(int i=0;i<pointList.Count;i++){
			
			var a=pointList[i];
			
			//Debug.Log(i+" xxx:"+a);
			//-当前点
			if(func_isBlock(a.x,a.y)){
				freeX=lastFreePoint.x;
				freeY=lastFreePoint.y;
				return true;
			}
			lastFreePoint.x=a.x;
			lastFreePoint.y=a.y;
			if(bDebug){
				Gizmos.DrawCube(new Vector3(a.x+0.5f,a.y+0.5f,0),Vector3.one*0.8f);
			}
			if(i<pointList.Count-1){
				var b=pointList[i+1];
				//-中点
				int hx=(int)((a.x+b.x)/2.0f);
				int hy=(int)((a.y+b.y)/2.0f);
				if(func_isBlock(hx,hy)){
					freeX=lastFreePoint.x;
					freeY=lastFreePoint.y;
					return true;
				}
				lastFreePoint.x=hx;
				lastFreePoint.y=hy;
				if(bDebug){
					Gizmos.DrawCube(new Vector3(hx+0.5f,hy+0.5f,0),Vector3.one*0.8f);
				}
			}
		}
		freeX=endPoint.x;
		freeY=endPoint.y;
		return false;
	}
}