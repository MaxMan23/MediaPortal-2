#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2012 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

/**************************************************
This unit is used to collect Analytic Geometry formulars
It includes Line, Line segment and CPolygon				
																				
Development by: Frank Shen                                    
Date: 08, 2004                                                         
Modification History:													
* *** **********************************************/

using System;
using MediaPortal.Common;
using MediaPortal.Common.Logging;

namespace MediaPortal.UI.SkinEngine.DirectX.Triangulate
{
  /// <summary>
  ///To define a line in the given coordinate system
  ///and related calculations
  ///Line Equation:ax+by+c=0
  ///</summary>

  //a Line in 2D coordinate system: ax+by+c=0
  public class CLine
  {
    //line: ax+by+c=0;
    protected float _a;
    protected float _b;
    protected float _c;

    private void Initialize(float angleInRad, CPoint2D point)
    {
      //angleInRad should be between 0-Pi

      try
      {
        //if ((angleInRad<0) ||(angleInRad>Math.PI))
        if (angleInRad > 2 * Math.PI)
        {
          throw new InvalidInputGeometryDataException(string.Format("The input line angle" + " {0} is wrong. It should be between 0-2*PI.", angleInRad));
        }

        if (Math.Abs(angleInRad - Math.PI / 2) < ConstantValue.SmallValue) //vertical line
        {
          _a = 1;
          _b = 0;
          _c = -point.X;
        }
        else //not vertical line
        {
          _a = (float) -Math.Tan(angleInRad);
          _b = 1;
          _c = -_a * point.X - _b * point.Y;
        }
      }
      catch (Exception e)
      {
        ServiceRegistration.Get<ILogger>().Error("Error initializing CLine", e);
      }
    }


    public CLine(float angleInRad, CPoint2D point)
    {
      Initialize(angleInRad, point);
    }

    public CLine(CPoint2D point1, CPoint2D point2)
    {
      try
      {
        if (CPoint2D.SamePoints(point1, point2))
        {
          throw new InvalidInputGeometryDataException("The input points are the same");
        }

        //Point1 and Point2 are different points:
        if (Math.Abs(point1.X - point2.X) < ConstantValue.SmallValue) //vertical line
        {
          Initialize((float) Math.PI / 2, point1);
        }
        else if (Math.Abs(point1.Y - point2.Y) < ConstantValue.SmallValue) //Horizontal line
        {
          Initialize(0, point1);
        }
        else //normal line
        {
          float m = (point2.Y - point1.Y) / (point2.X - point1.X);
          float alphaInRad = (float) Math.Atan(m);
          Initialize(alphaInRad, point1);
        }
      }
      catch (Exception e)
      {
        ServiceRegistration.Get<ILogger>().Error("Error creating CLine", e);
      }
    }

    public CLine(CLine copiedLine)
    {
      _a = copiedLine._a;
      _b = copiedLine._b;
      _c = copiedLine._c;
    }

    /*** calculate the distance from a given point to the line ***/
    public float GetDistance(CPoint2D point)
    {
      float x0 = point.X;
      float y0 = point.Y;

      float d = Math.Abs(_a * x0 + _b * y0 + _c);
      d = d / ((float) (Math.Sqrt(_a * _a + _b * _b)));

      return d;
    }

    /*** point(x, y) in the line, based on y, calculate x ***/
    public float GetX(float y)
    {
      //if the line is a horizontal line (a=0), it will return a NaN:
      float x;
      try
      {
        if (Math.Abs(_a) < ConstantValue.SmallValue) //a=0;
          throw new NonValidReturnException();

        x = -(_b * y + _c) / _a;
      }
      catch (Exception e)  //Horizontal line a=0;
      {
        //x = System.Float.NaN;
        x = 0;
        ServiceRegistration.Get<ILogger>().Error("CLine.GetX: Error calculating X coordinate", e);
      }

      return x;
    }

    /*** point(x, y) in the line, based on x, calculate y ***/
    public float GetY(float x)
    {
      //if the line is a vertical line, it will return a NaN:
      float y;
      try
      {
        if (Math.Abs(_b) < ConstantValue.SmallValue)
        {
          throw new NonValidReturnException();
        }
        y = -(_a * x + _c) / _b;
      }
      catch (Exception e)
      {
        //y = System.Float.NaN;
        y = 0;
        ServiceRegistration.Get<ILogger>().Error("CLine.GetY: Error calculating Y coordinate", e);
      }
      return y;
    }

    /*** is it a vertical line:***/
    public bool VerticalLine()
    {
      return Math.Abs(_b - 0) < ConstantValue.SmallValue;
    }

    /*** is it a horizontal line:***/
    public bool HorizontalLine()
    {
      return Math.Abs(_a - 0) < ConstantValue.SmallValue;
    }

    /*** calculate line angle in radian: ***/
    public float GetLineAngle()
    {
      if (_b == 0)
      {
        return (float) Math.PI / 2;
      }
      float tanA = -_a / _b;
      return (float) Math.Atan(tanA);
    }

    public bool Parallel(CLine line)
    {
      return Math.Abs(_a / _b - line._a / line._b) < ConstantValue.SmallValue;
    }

    /**************************************
     Calculate intersection point of two lines
     if two lines are parallel, return null
     * ************************************/
    public CPoint2D IntersecctionWith(CLine line)
    {
      CPoint2D point = new CPoint2D();
      float a1 = _a;
      float b1 = _b;
      float c1 = _c;

      float a2 = line._a;
      float b2 = line._b;
      float c2 = line._c;

      if (!(Parallel(line))) //not parallen
      {
        point.X = (c2 * b1 - c1 * b2) / (a1 * b2 - a2 * b1);
        point.Y = (a1 * c2 - c1 * a2) / (a2 * b2 - a1 * b2);
      }
      return point;
    }
  }

  public class CLineSegment : CLine
  {
    //line: ax+by+c=0, with start point and end point
    //direction from start point ->end point
    private CPoint2D _startPoint;
    private CPoint2D _endPoint;

    public CPoint2D StartPoint
    {
      get
      {
        return _startPoint;
      }
    }

    public CPoint2D EndPoint
    {
      get
      {
        return _endPoint;
      }
    }

    public CLineSegment(CPoint2D startPoint, CPoint2D endPoint)
      : base(startPoint, endPoint)
    {
      _startPoint = startPoint;
      _endPoint = endPoint;
    }

    /*** chagne the line's direction ***/
    public void ChangeLineDirection()
    {
      CPoint2D tempPt = _startPoint;
      _startPoint = _endPoint;
      _endPoint = tempPt;
    }

    /*** To calculate the line segment length:   ***/
    public float GetLineSegmentLength()
    {
      float d = (_endPoint.X - _startPoint.X) * (_endPoint.X - _startPoint.X);
      d += (_endPoint.Y - _startPoint.Y) * (_endPoint.Y - _startPoint.Y);
      d = (float) Math.Sqrt(d);

      return d;
    }

    /********************************************************** 
      Get point location, using windows coordinate system: 
      y-axes points down.
      Return Value:
      -1:point at the left of the line (or above the line if the line is horizontal)
       0: point in the line segment or in the line segment 's extension
       1: point at right of the line (or below the line if the line is horizontal)    
     ***********************************************************/
    public int GetPointLocation(CPoint2D point)
    {
      float Ax, Ay, Bx, By, Cx, Cy;
      Bx = _endPoint.X;
      By = _endPoint.Y;

      Ax = _startPoint.X;
      Ay = _startPoint.Y;

      Cx = point.X;
      Cy = point.Y;

      if (HorizontalLine())
      {
        if (Math.Abs(Ay - Cy) < ConstantValue.SmallValue) //equal
          return 0;
        if (Ay > Cy)
          return -1;   //Y Axis points down, point is above the line
        //Ay<Cy
        return 1;    //Y Axis points down, point is below the line
      }
      //Not a horizontal line
      //make the line direction bottom->up
      if (_endPoint.Y > _startPoint.Y)
        ChangeLineDirection();

      float l = GetLineSegmentLength();
      float s = ((Ay - Cy) * (Bx - Ax) - (Ax - Cx) * (By - Ay)) / (l * l);

      //Note: the Y axis is pointing down:
      if (Math.Abs(s - 0) < ConstantValue.SmallValue) //s=0
        return 0; //point is in the line or line extension
      if (s > 0)
        return -1; //point is left of line or above the horizontal line
      //s<0
      return 1;
    }

    /***Get the minimum x value of the points in the line***/
    public float GetXmin()
    {
      return Math.Min(_startPoint.X, _endPoint.X);
    }

    /***Get the maximum  x value of the points in the line***/
    public float GetXmax()
    {
      return Math.Max(_startPoint.X, _endPoint.X);
    }

    /***Get the minimum y value of the points in the line***/
    public float GetYmin()
    {
      return Math.Min(_startPoint.Y, _endPoint.Y);
    }

    /***Get the maximum y value of the points in the line***/
    public float GetYmax()
    {
      return Math.Max(_startPoint.Y, _endPoint.Y);
    }

    /***Check whether this line is in a longer line***/
    public bool InLine(CLineSegment longerLineSegment)
    {
      return _startPoint.InLine(longerLineSegment) && _endPoint.InLine(longerLineSegment);
    }

    /************************************************
     * Offset the line segment to generate a new line segment
     * If the offset direction is along the x-axis or y-axis, 
     * Parameter is true, other wise it is false
     * ***********************************************/
    public CLineSegment OffsetLine(float distance, bool rightOrDown)
    {
      //offset a line with a given distance, generate a new line
      //rightOrDown=true means offset to x incress direction,
      // if the line is horizontal, offset to y incress direction

      CLineSegment line;
      CPoint2D newStartPoint = new CPoint2D();
      CPoint2D newEndPoint = new CPoint2D();

      float alphaInRad = GetLineAngle(); // 0-PI
      if (rightOrDown)
      {
        if (HorizontalLine()) //offset to y+ direction
        {
          newStartPoint.X = _startPoint.X;
          newStartPoint.Y = _startPoint.Y + distance;

          newEndPoint.X = _endPoint.X;
          newEndPoint.Y = _endPoint.Y + distance;
          line = new CLineSegment(newStartPoint, newEndPoint);
        }
        else //offset to x+ direction
        {
          if (Math.Sin(alphaInRad) > 0)
          {
            newStartPoint.X = (float) (_startPoint.X + Math.Abs(distance * Math.Sin(alphaInRad)));
            newStartPoint.Y = (float) (_startPoint.Y - Math.Abs(distance * Math.Cos(alphaInRad)));

            newEndPoint.X = (float) (_endPoint.X + Math.Abs(distance * Math.Sin(alphaInRad)));
            newEndPoint.Y = (float) (_endPoint.Y - Math.Abs(distance * Math.Cos(alphaInRad)));

            line = new CLineSegment(newStartPoint, newEndPoint);
          }
          else //sin(FalphaInRad)<0
          {
            newStartPoint.X = (float) (_startPoint.X + Math.Abs(distance * Math.Sin(alphaInRad)));
            newStartPoint.Y = (float) (_startPoint.Y + Math.Abs(distance * Math.Cos(alphaInRad)));
            newEndPoint.X = (float) (_endPoint.X + Math.Abs(distance * Math.Sin(alphaInRad)));
            newEndPoint.Y = (float) (_endPoint.Y + Math.Abs(distance * Math.Cos(alphaInRad)));

            line = new CLineSegment(newStartPoint, newEndPoint);
          }
        }
      }//{rightOrDown}
      else //leftOrUp
      {
        if (HorizontalLine()) //offset to y directin
        {
          newStartPoint.X = _startPoint.X;
          newStartPoint.Y = _startPoint.Y - distance;

          newEndPoint.X = _endPoint.X;
          newEndPoint.Y = _endPoint.Y - distance;
          line = new CLineSegment(newStartPoint, newEndPoint);
        }
        else //offset to x directin
        {
          if (Math.Sin(alphaInRad) >= 0)
          {
            newStartPoint.X = (float) (_startPoint.X - Math.Abs(distance * Math.Sin(alphaInRad)));
            newStartPoint.Y = (float) (_startPoint.Y + Math.Abs(distance * Math.Cos(alphaInRad)));
            newEndPoint.X = (float) (_endPoint.X - Math.Abs(distance * Math.Sin(alphaInRad)));
            newEndPoint.Y = (float) (_endPoint.Y + Math.Abs(distance * Math.Cos(alphaInRad)));

            line = new CLineSegment(
              newStartPoint, newEndPoint);
          }
          else //sin(FalphaInRad)<0
          {
            newStartPoint.X = (float) (_startPoint.X - Math.Abs(distance * Math.Sin(alphaInRad)));
            newStartPoint.Y = (float) (_startPoint.Y - Math.Abs(distance * Math.Cos(alphaInRad)));
            newEndPoint.X = (float) (_endPoint.X - Math.Abs(distance * Math.Sin(alphaInRad)));
            newEndPoint.Y = (float) (_endPoint.Y - Math.Abs(distance * Math.Cos(alphaInRad)));

            line = new CLineSegment(newStartPoint, newEndPoint);
          }
        }
      }
      return line;
    }

    /********************************************************
    To check whether 2 lines segments have an intersection
    *********************************************************/
    public bool IntersectedWith(CLineSegment line)
    {
      float x1 = _startPoint.X;
      float y1 = _startPoint.Y;
      float x2 = _endPoint.X;
      float y2 = _endPoint.Y;
      float x3 = line._startPoint.X;
      float y3 = line._startPoint.Y;
      float x4 = line._endPoint.X;
      float y4 = line._endPoint.Y;

      float de = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
      //if de<>0 then //lines are not parallel
      if (Math.Abs(de - 0) > ConstantValue.SmallValue) //not parallel
      {
        float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / de;
        float ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / de;

        return (ua > 0) && (ua < 1) && (ub > 0) && (ub < 1);
      }
      //lines are parallel
      return false;
    }
  }
}
