using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.RootFinding;
using System.Linq;

namespace Substitution
{
    public static class Transcoding
    {

        public enum LinkType { Linear, Power, Angular };

        public static double Map(LinkType link, double x, double x0, double y0, double x1, double y1, int nSteps, List<double> breaks)
        {
            double y = _Map(link, x, x0, y0, x1, y1);
            if(nSteps > 0) y = Discretize(y, breaks);
            return(y);
        }

        public static double _Map(LinkType link, double x, double x0, double y0, double x1, double y1)
        {
            if(x <= x0) return(y0);
            else if(x >= x1) return(y1);
            else return(ApplyLink(link, x, x0, y0, x1, y1));
        }

        public static double ApplyLink(LinkType link, double x, double x0, double y0, double x1, double y1)
        {
            switch(link)
            {
                case LinkType.Linear:
                    return(LinearLink(x, x0, y0, x1, y1));
                    break;
                case LinkType.Power:
                    return(PowerLink(x, x0, y0, x1, y1)); 
                    break;
                case LinkType.Angular:
                    return(AngularLink(x, x0, y0, x1, y1, 1.0)); 
                    break;
                default:
                    return(LinearLink(x, x0, y0, x1, y1));
                    break;
            }
        }

        public static double LinearLink(double x, double x0, double y0, double x1, double y1)
        {
            return(y0 + (x - x0) * (y1 - y0) / (x1 - x0));
        }

        public static double PowerLink(double x, double x0, double y0, double x1, double y1)
        {
            double a = (Math.Log(y1) - Math.Log(y0)) / (Math.Log(x1) - Math.Log(x0));
            double b = Math.Log(y0) - a * Math.Log(x0);
            return(Math.Exp(a * Math.Log(x) + b));
        }

        public static double AngularLink(double x, double x0, double y0, double x1, double y1, double size)
        {          
            var A = Matrix<double>.Build.DenseOfArray(new double[,] {
                { 1, Math.Atan(size / x0) },
                { 1, Math.Atan(size / x1) }
            });
            var b = Vector<double>.Build.Dense(new double[] { y0, y1 });
            var unk = A.Solve(b);

            return(unk[0] + unk[1] * Math.Atan(size / x));
        }

        public static double Discretize(double y, List<double> breaks)
        {
            double yMin = breaks.Min();
            double yMax = breaks.Max();

            List<double> midpoints = breaks; // TODO!!!! (if center)

            if (y <= yMin) return(yMin);
            else if (y >= yMax) return(yMax);
            else 
            {
                double res = 0.0;
                for(int i = 0; i < breaks.Capacity - 1; i++)
                {
                    if( y > Math.Min(breaks[i], breaks[i + 1]) && y <= Math.Max(breaks[i], breaks[i + 1]) )
                    {
                        if(y <= midpoints[i]) res = Math.Min(breaks[i], breaks[i + 1]);
                        else res = Math.Max(breaks[i], breaks[i + 1]);
                    }
                }
                return(res);
            }
        }
        
        // Old (with midpoints --> TODO)
        /* public static double Discretize(double y)
        {
            double yMin = _breaks.Min();
            double yMax = _breaks.Max();

            if (y <= yMin) return(yMin);
            else if (y >= yMax) return(yMax);
            else
            {
                double res = 0.0;
                for(int i = 0; i < _breaks.Capacity - 1; i++)
                {
                    if( y > Math.Min(_breaks[i], _breaks[i + 1]) && y <= Math.Max(_breaks[i], _breaks[i + 1]) )
                    {
                        if(y <= _midpoints[i]) res = Math.Min(_breaks[i], _breaks[i + 1]);
                        else res = Math.Max(_breaks[i], _breaks[i + 1]);
                    }
                }
                return(res);
            }
        } */

        public static List<double> GetBreaks(LinkType link, double x, double x0, double y0, double x1, double y1, int nSteps, string stepAlong)
        {
            List<double> breaks;
            if(stepAlong == "x")
            {
                List<double> equal_breaks_x = new List<double>(Generate.LinearSpaced(nSteps + 1, x0, x1));
                breaks = equal_breaks_x.Select(x => _Map(link, x, x0, y0, x1, y1)).ToList();
            } 
            else
            {
                List<double> equal_breaks_y = new List<double>(Generate.LinearSpaced(nSteps + 1, y0, y1));
                List<double> gradient_breaks_x = new List<double>(equal_breaks_y.Select(y => Bisection.FindRoot((x) => _Map(link, x, x0, y0, x1, y1) - y, Math.Min(x0, x1), Math.Max(x0, x1), 1e-2, 100)));
                breaks = gradient_breaks_x.Select(x => _Map(link, x, x0, y0, x1, y1)).ToList();
            }

            // TODO: center & midpoints
            /* if(!_center) this._midpoints = _breaks;
            else {
                double[] midpoints = new double[_breaks.Capacity - 1];
                for(int i = 0; i < midpoints.Length; i++) midpoints[i] = 0.5f * (_breaks[i] + _breaks[i + 1]);
                this._midpoints = new List<double>(midpoints);
            } */

            return(breaks);
        }

        // Old (with midpoints --> TODO)
        /* public static void UpdateBreaks()
        {
            if (_discrete) {
                if(_stepAlong == "x")
                {
                    List<double> equal_breaks_x = new List<double>(Generate.LinearSpaced(_steps + 1, _x0, _x1)); // IEnumerable<double> ?
                    this._breaks = (List<double>)equal_breaks_x.Select(x => _Map(x));
                } 
                else 
                {
                    List<double> equal_breaks_y = new List<double>(Generate.LinearSpaced(_steps + 1, _y0, _y1)); // IEnumerable<double> ?
                    // Func<double, double> diff = x => _Map(x) - i;
                    List<double> gradient_breaks_x = new List<double>(equal_breaks_y.Select(y => Bisection.FindRoot((x) => _Map(x) - y, Math.Min(_x0, _x1), Math.Max(_x0, _x1), 1e-2, 100)));
                    this._breaks = (List<double>)gradient_breaks_x.Select(x => _Map(x));
                }
                
                if(!_center) this._midpoints = _breaks;
                else {
                    double[] midpoints = new double[_breaks.Capacity - 1];
                    for(int i = 0; i < midpoints.Length; i++) midpoints[i] = 0.5f * (_breaks[i] + _breaks[i + 1]);
                    this._midpoints = new List<double>(midpoints);
                }
            }
        } */
    }
}