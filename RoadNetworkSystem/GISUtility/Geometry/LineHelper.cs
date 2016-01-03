using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.GIS.Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RoadNetworkSystem.GIS
{
    class LineHelper
    {

        private const int THRESHOLD_CONVERTION_ANGLE = 10; 

        /// <summary>
        /// 通过线的偏移得到新的线段
        /// </summary>
        /// <param name="line"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static IPolyline CreatLineByOffset(IPolyline line, double offset)
        {
            //定义IConstructCurve类并初始化,偏移只能在此类的ConstructOffset方法中进行
            IConstructCurve curve = new PolylineClass();
            //定义缺省变量
            object o = System.Type.Missing;
            //执行偏移,其中两参数缺省,注意偏移单位是meter还是unit
            curve.ConstructOffset(line, offset, ref o, ref o);
            //把偏移结果(IConstructCurve类)传递给IPolyline类
            IPolyline newLine = curve as IPolyline;
            //返回偏移后的线要素
            return newLine;
        }


        public static List<IPolyline> ConvertPolyline2Lines(IPolyline polyline)
        {
            if (polyline == null)
            {
                return null;
            }
            IPointCollection pntClt = polyline as IPointCollection;

            List<IPolyline> lines = new List<IPolyline>();
            
            if (pntClt.PointCount == 2)
            {
                lines.Add(polyline);
                return lines;
            }
            else
            {
                IPoint prePrePoint = pntClt.get_Point(0);
                IPoint prePoint = pntClt.get_Point(1);
                IPoint cursorPoint = pntClt.get_Point(2);
                int cursorIndex = 2;
                double temAngles = 0;
                IPointCollection temPntClt = new PolylineClass();
                temPntClt.AddPoint(prePrePoint);
                temPntClt.AddPoint(prePoint);

                while (cursorIndex < pntClt.PointCount)
                {
                    double cursorAngle = 180 - GetAngleInKinkPoint(prePrePoint,prePoint,cursorPoint);
                    temAngles += cursorAngle;

                    prePrePoint = prePoint;
                    prePoint = cursorPoint;

                    temPntClt.AddPoint(cursorPoint);

                    //判断是否是生成新的直线段
                    if (temAngles > THRESHOLD_CONVERTION_ANGLE)
                    {
                        IPolyline newLine = temPntClt as IPolyline;
                        IPointCollection newLineCollection = new PolylineClass();
                        newLineCollection.AddPoint(newLine.FromPoint);
                        newLineCollection.AddPoint(newLine.ToPoint);
                        newLine = newLineCollection as IPolyline;

                        lines.Add(newLine);
                        temPntClt = new PolylineClass();
                        //上一个直线段的终点是下一个直线断的起点
                        temPntClt.AddPoint(cursorPoint);
                        temAngles = 0;
                    }


                    //更新游标
                    cursorIndex++;
                    if (cursorIndex < (pntClt.PointCount))
                    {
                        cursorPoint = pntClt.get_Point(cursorIndex);
                    }
                    else
                    {
                        IPolyline newLine = temPntClt as IPolyline;
                        IPointCollection newLineCollection = new PolylineClass();
                        newLineCollection.AddPoint(newLine.FromPoint);
                        newLineCollection.AddPoint(newLine.ToPoint);
                        newLine = newLineCollection as IPolyline;

                        lines.Add(newLine);
                    }

                }
                return lines;
            }

        }


        public static double GetAngleInKinkPoint(IPoint fromPoint,IPoint kinkPoint,IPoint toPoint)
        {
            double vector_x_from_kink = fromPoint.X - kinkPoint.X;
            double vector_y_from_kink = fromPoint.Y - kinkPoint.Y;

            double vector_x_to_kink = toPoint.X - kinkPoint.X;
            double vector_y_to_kink = toPoint.Y - kinkPoint.Y;

            double dotProduct = vector_x_to_kink * vector_x_from_kink + vector_y_to_kink * vector_y_from_kink;

            double length_from_kink = PointHelper.GetDistance2Point(fromPoint, kinkPoint);
            double length_to_kink = PointHelper.GetDistance2Point(toPoint, kinkPoint);

            double cosValue = dotProduct / (length_from_kink * length_to_kink);
            double angleInArc = Math.Acos(cosValue);

            double angleInDu = angleInArc * 180 / Math.PI;;
            if(angleInDu < 0 )
            {
                return -angleInDu;
            }
            else
            {
                return angleInDu;
            }
 
        }

        public static IPolyline CreateLine(IPolyline line, double fCut, double tCut)
        {

            IPointCollection temCol2 = new PolylineClass();
            IPointCollection backupCol = new PolylineClass();

            #region 复制line，line只做输入参数，不做处理
            temCol2 = line as IPointCollection;
            for (int i = 0; i < temCol2.PointCount; i++)
            {
                backupCol.AddPoint(temCol2.Point[i]);
            }
            IPolyline backupLine = backupCol as IPolyline;
            #endregion 复制line

            ICurve curve = backupLine as ICurve;

            //起点处理后的多线段
            IPolyline fCrtLine = new PolylineClass();

            //延长起点
            if (fCut < 0)
            {
                IConstructPoint contructionPoint = new PointClass();
                contructionPoint.ConstructAlong(curve, esriSegmentExtension.esriExtendTangents, fCut, false);
                IPoint fPnt = contructionPoint as IPoint;

                IPointCollection pntClt = new PolylineClass();
                pntClt = backupLine as IPointCollection;

                IPointCollection fPntCol = new PolylineClass();
                fPntCol.AddPoint(fPnt);
                pntClt.InsertPointCollection(0, fPntCol);
                fCrtLine = pntClt as IPolyline;
            }
            //截取起点
            else
            {
                //截取点到起点的距离小于线长
                if (fCut < curve.Length)
                {

                    int newPartIndex1;
                    int newSegmentIndex1;
                    bool SplitHappened;
                    backupLine.SplitAtDistance(fCut, false, false, out SplitHappened, out newPartIndex1, out newSegmentIndex1);

                    IPointCollection pPtCol = backupLine as IPointCollection;
                    for (int i = 0; i <= newSegmentIndex1 - 1; i++)
                    {
                        pPtCol.RemovePoints(0, 1);
                    }
                    //同样copy一份生成线
                    IPointCollection temCol = new PolylineClass();
                    for (int i = 0; i < pPtCol.PointCount; i++)
                    {
                        temCol.AddPoint(pPtCol.get_Point(i));

                    }
                    fCrtLine = temCol as IPolyline;
                }
            }

            //新生成的线
            IPolyline newLine = new PolylineClass();

           //把终点截取量转换为距离起点的距离
            double toPosition = fCrtLine.Length - tCut;

            //保证截取端点的到起点的距离大于0
            if (toPosition > 0)
            {
                //终点延长
                if (tCut < 0)
                { ICurve curve2 = fCrtLine as ICurve;
                    IConstructPoint contructionPoint = new PointClass();
                    contructionPoint.ConstructAlong(curve2, esriSegmentExtension.esriExtendTangents, toPosition, false);
                    IPoint pnt = contructionPoint as IPoint;

                    IPointCollection pntClt = new PolylineClass();
                    pntClt = fCrtLine as IPointCollection;
                    pntClt.AddPoint(pnt);
                    newLine = pntClt as IPolyline;
                }
                    // 终点截取
                else
                {
                    //同样备份一份fCrtLine
                    IPointCollection temCol1 = new PolylineClass();
                    temCol1 = fCrtLine as IPointCollection;
                    IPolyline temLine=new PolylineClass();
                    IPointCollection temCol = new PolylineClass();
                    for (int i = 0; i < temCol1.PointCount; i++)
                    {
                        temCol.AddPoint(temCol1.get_Point(i));
                    }
                    temLine = temCol as IPolyline;

                    int newPartIndex2;
                    int newSegmentIndex2;
                    bool SplitHappened;
                    temLine.SplitAtDistance(toPosition, false, false, out SplitHappened, out newPartIndex2, out newSegmentIndex2);
                    IPointCollection pPtCol = temLine as IPointCollection;

                    IPointCollection newPntClt = new PolylineClass();
                    for (int i = 0; i <= newSegmentIndex2; i++)
                    {
                        newPntClt.AddPoint(pPtCol.get_Point(i));
                    }

                    newLine = newPntClt as IPolyline;

                    //for (int i = pPtCol.PointCount - 1; i > newSegmentIndex2; i--)
                    //{
                    //    pPtCol.RemovePoints(i, 1);
                    //}
                    //newLine = pPtCol as IPolyline;
                }
            }


            return newLine;

        }


        /// <summary>
        /// 截取线段
        /// </summary>
        /// <param name="line"></param>输入线段
        /// <param name="fMeasure"></param>截断点距离起点的距离
        /// <param name="tMeasure"></param>截断点到终点的距离
        /// <returns></returns>输出线段
        public static IPolyline CutLine(IPolyline line, double fMeasure, double tMeasure)
        {
            tMeasure = line.Length - tMeasure;
            //如果fMeasure < 0或fMeasure >line.Length或tMeasure >line.Length或fMeasure==tMeasure，返回空值
            if (fMeasure < 0 || fMeasure > line.Length || tMeasure > line.Length || fMeasure == tMeasure)
                return null;
            //声明newPartIndex1、newPartIndex2、newSegmentIndex1、newSegmentIndex2为整型变量
            int newPartIndex1;
            int newPartIndex2;
            int newSegmentIndex1;
            int newSegmentIndex2;
            //声明SplitHappened为布尔型变量
            bool SplitHappened;
            //定义缺省变量
            object o = System.Type.Missing;
            //
            line.SplitAtDistance(fMeasure, false, false, out SplitHappened, out newPartIndex1, out newSegmentIndex1);
            line.SplitAtDistance(tMeasure, false, false, out SplitHappened, out newPartIndex2, out newSegmentIndex2);

            IPointCollection pPtCol = line as IPointCollection;
            int i;
            for (i = pPtCol.PointCount - 1; i > newSegmentIndex2; i--)
            {
                pPtCol.RemovePoints(i, 1);
            }
            for (i = 0; i <= newSegmentIndex1 - 1; i++)
            {
                pPtCol.RemovePoints(0, 1);
            }

            line = pPtCol as IPolyline;
            return line;
        }

        /// <summary>
        /// 通过线性参考创建多线段
        /// </summary>
        /// <param name="originalLine"></param>
        /// <param name="offset"></param>
        /// <param name="fCut"></param>
        /// <param name="tCut"></param>
        /// <returns></returns>
        public static IPolyline CreateLineByLRS(IPolyline originalLine, double offset, double fCut, double tCut)
        {
            IPolyline offsetLine = CreatLineByOffset(originalLine, offset);
            IPointCollection coll = offsetLine as IPointCollection;
            int count = coll.PointCount;

            IPolyline targetLine = CreateLine(offsetLine, fCut, tCut);
            IPointCollection coll1 = targetLine as IPointCollection;
            int count1 = coll1.PointCount;
            return targetLine;
        }

        /// <summary>
        /// 利用线上的两个点截取出一段线段
        /// </summary>
        /// <param name="originalLine"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static IPolyline CutPolylineByPointsOnLine(IPolyline originalLine,IPoint point1, IPoint point2)
        {
            IPolyline preLine   = new PolylineClass();
            IPolyline postLine   = new PolylineClass();
            CutLineAtPoint(originalLine, point1, out preLine, out postLine);


            if (postLine.Length < 0.5)
            {
                return null;
            }
            IPolyline preLine2 = new PolylineClass();
            IPolyline postLine2 = new PolylineClass();
           
            CutLineAtPoint(postLine, point2, out preLine2, out postLine2);

            return preLine2;
        }


        /// <summary>
        /// 绘制贝塞尔曲线
        /// </summary>
        /// <param name="pnt1"></param>贝塞尔曲线的起点
        /// <param name="pntMiddle"></param>贝塞尔曲线的中间控制点
        /// <param name="pnt3"></param>贝塞尔曲线的终点
        /// <returns></returns>返回贝塞尔曲线的拟合虚线
        public static IPolyline DrawBezier(IPoint pnt1, IPoint pntMiddle, IPoint pnt3)
        {
            if (pnt1.IsEmpty || pntMiddle.IsEmpty || pnt3.IsEmpty)
            {
                return null;
            }
            IPointCollection pntCol = new Polyline();
            try
            {

                int max = 10;
                double tem = 0.1;
                double t = 0;
                for (int i = 0; i <= max; i++)
                {
                    t = i * tem;
                    IPoint pnt = new PointClass();
                    double x = (1 - t) * (1 - t) * pnt1.X + 2 * t * (1 - t) * pntMiddle.X + t * t * pnt3.X;
                    double y = (1 - t) * (1 - t) * pnt1.Y + 2 * t * (1 - t) * pntMiddle.Y + t * t * pnt3.Y;
                    pnt.PutCoords(x, y);
                    pntCol.AddPoint(pnt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            IPolyline line = pntCol as IPolyline;
            return line;
        }

        /// <summary>
        /// 获取两个线的交点
        /// </summary>
        /// <param name="line1"></param>相交的第一个线
        /// <param name="line2"></param>相交的第二个线
        /// <returns></returns>两条线的交点
        public static IPoint GetIntersectionPoint(IPolyline line1, IPolyline line2)
        {
            IPoint pnt = new PointClass();
            ITopologicalOperator topoOperater = line1 as ITopologicalOperator;
            IGeometry geo = topoOperater.Intersect(line2, esriGeometryDimension.esriGeometry0Dimension);
            IPointCollection pcol = geo as IPointCollection;
            //保证不为空，且含有的点的数量多余1
            if (pcol != null && pcol.PointCount > 0)
            {
                pnt = pcol.Point[0];
            }
            return pnt;
        }

        /// <summary>
        /// 返回一个线段相对另一个线段的数字化方向，1为相同，-1标识两个方向相反
        /// </summary>
        /// <param name="targetBoundaryFea"></param>目标
        /// <param name="corrArcFeature"></param>相应的Arc
        /// <returns></returns>
        public static int GetCorrepondseDir(IPolyline targetLine, IPolyline corrLine)
        {
            int tarBoundaryDir = 0;

            if (corrLine.Length > targetLine.Length)
            {
                //目标线段的起点到相应线段起点的距离
                double disTFandCF = PointHelper.GetDistance2Point(targetLine.FromPoint, corrLine.FromPoint);

                //目标线段的终点到相应线段的起点的距离
                double disTTandCF = PointHelper.GetDistance2Point(targetLine.ToPoint, corrLine.FromPoint);
                if (disTTandCF > disTFandCF)
                {
                    tarBoundaryDir = 1;
                }
                else
                {
                    tarBoundaryDir = -1;
                }
            }

            else
            {
                //目标线段的起点到相应线段起点的距离
                double disCFandTF = PointHelper.GetDistance2Point(targetLine.FromPoint, corrLine.FromPoint);

                //目标线段的终点到相应线段的起点的距离
                double disCTandTF = PointHelper.GetDistance2Point(targetLine.FromPoint, corrLine.ToPoint);
                if (disCTandTF > disCFandTF)
                {
                    tarBoundaryDir = 1;
                }
                else
                {
                    tarBoundaryDir = -1;
                }
            }
            return tarBoundaryDir;
        }


        /// <summary>
        /// 用线段上的点把线打断成两部分
        /// zxl修改于2014.12.28
        /// </summary>
        /// <param name="line"></param>
        /// <param name="pt"></param>
        /// <param name="Line1"></param>
        /// <param name="Line2"></param>
        public static void CutLineAtPoint(IPolyline line, IPoint pt, out IPolyline Line1, out IPolyline Line2)
        {
            int newPartIndex, newSegmentIndex;
            bool SplitHappened;
            line.SplitAtPoint(pt, false, false, out SplitHappened, out  newPartIndex, out newSegmentIndex);
            IPointCollection pPtCol = line as IPointCollection;
            IPointCollection pPtCol1 = new PolylineClass();
            IPointCollection pPtCol2 = new PolylineClass();

            int i;
            for (i = 0; i <= newSegmentIndex; i++)
            {
                pPtCol1.AddPoint(pPtCol.get_Point(i));

            }
            for (i = newSegmentIndex; i < pPtCol.PointCount; i++)
            {
                pPtCol2.AddPoint(pPtCol.get_Point(i));

            }
            Line1 = pPtCol1 as IPolyline;
            Line2 = pPtCol2 as IPolyline;
        }

        /// <summary>
        /// 合并两段线段
        /// zxl修改1.28
        /// </summary>
        /// <param name="Line1"></param>
        /// <param name="Line2"></param>
        /// <returns></returns>
        public static IPolyline MergeLine(IPolyline Line1, IPolyline Line2)
        {
            IPointCollection pPtCol1 = Line1 as IPointCollection;
            IPointCollection pPtCol2 = Line2 as IPointCollection;
            IPointCollection pPtCol = new PolylineClass();

            int i;
            //1.若Line1的终点跟Line2的起点重合
            if (pPtCol1.get_Point(pPtCol1.PointCount - 1).X == pPtCol2.get_Point(0).X && pPtCol1.get_Point(pPtCol1.PointCount - 1).Y == pPtCol2.get_Point(0).Y)
            {
                for (i = 0; i < pPtCol1.PointCount; i++)
                {

                    pPtCol.AddPoint(pPtCol1.get_Point(i));
                }
                for (i = 0; i < pPtCol2.PointCount; i++)
                {
                    pPtCol.AddPoint(pPtCol2.get_Point(i));
                }
            }
            else if (pPtCol1.get_Point(0).X == pPtCol2.get_Point(0).X && pPtCol1.get_Point(0).Y == pPtCol2.get_Point(0).Y)
            {
                for (i = pPtCol1.PointCount - 1; i >= 0; i--)
                {
                    pPtCol.AddPoint(pPtCol1.get_Point(i));
                }
                for (i = 0; i < pPtCol2.PointCount; i++)
                {
                    pPtCol.AddPoint(pPtCol2.get_Point(i));
                }

            }
            else if (pPtCol1.get_Point(pPtCol1.PointCount - 1).X == pPtCol2.get_Point(pPtCol2.PointCount - 1).X && pPtCol1.get_Point(pPtCol1.PointCount - 1).Y == pPtCol2.get_Point(pPtCol2.PointCount - 1).Y)
            {
                for (i = 0; i < pPtCol1.PointCount; i++)
                {
                    pPtCol.AddPoint(pPtCol1.get_Point(i));
                }
                for (i = pPtCol2.PointCount - 1; i >= 0; i--)
                {
                    pPtCol.AddPoint(pPtCol2.get_Point(i));
                }
            }
            else if (pPtCol1.get_Point(0).X == pPtCol2.get_Point(pPtCol2.PointCount - 1).X && pPtCol1.get_Point(0).Y == pPtCol2.get_Point(pPtCol2.PointCount - 1).Y)
            {
                for (i = 0; i < pPtCol2.PointCount; i++)
                {
                    pPtCol.AddPoint(pPtCol2.get_Point(i));
                }
                for (i = 0; i < pPtCol1.PointCount; i++)
                {
                    pPtCol.AddPoint(pPtCol1.get_Point(i));
                }

            }
            IPolyline Line = pPtCol as IPolyline;
            return Line;

        }


        public static void AddPointInLine(IPolyline line, IPoint pnt,
            ref IPointCollection newPntClt, ref int pntPosition)
        {

            IPointCollection pntClt = line as IPointCollection;

            
            pntPosition = GetPositionOnLine(line, pnt);
            for (int i = 0; i < pntClt.PointCount - 1; i++)
            {
                if (i == pntPosition)
                {
                    newPntClt.AddPoint(pnt);
                }
                else
                {
                    newPntClt.AddPoint(pntClt.get_Point(i));
                }

            }
        }

        public static int GetPositionOnLine(IPolyline line, IPoint pnt)
        {
            IPointCollection pntClt = line as IPointCollection;
            for (int i = 0; i < pntClt.PointCount - 1; i++)
            {
                IPoint prePnt = pntClt.get_Point(i);
                IPoint postPnt = pntClt.get_Point(i + 1);
                if ((pnt.X - prePnt.X > 0) && (postPnt.X - pnt.X > 0))
                {
                    return i;
                }
            }
            return pntClt.PointCount - 1;

        }
    }
}
