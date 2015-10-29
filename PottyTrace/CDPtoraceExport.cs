﻿using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;
using System.Collections;
using CsPotrace;
using System.Drawing.Drawing2D;
using System.IO;

//Copyright (C) Dileep.M,Hyderabad
//E-mail :  m.dileep@gmail.com,


// This program is free software; you can redistribute it and/or modify  it under the terms of the GNU General Public License as published by  the Free Software Foundation; either version 2 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU  General Public License for more details.
// You should have received a copy of the GNU General Public License  along with this program; if not, write to the Free Software  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307,  USA. 

namespace CsPotrace
{
    public partial class Potrace
    {
        

       /// <summary>
       /// Exports a figure, which was created by Potrace to a Bitmap.
       /// </summary>
       /// <param name="Fig">Arraylist, which contains vectorinformations about the Curves</param>
       /// <param name="Width">Width of the Bitmap</param>
        /// <param name="Height">Height of the Bitmap</param>
       /// <returns></returns>
        public static Bitmap Export2GDIPlus(ArrayList Fig, int Width,int Height )
        {
            Image I= new Bitmap(Width,Height);
            Graphics g = Graphics.FromImage(I);
            g.FillRectangle(new SolidBrush(Color.White),0,0,Width,Height);
            GraphicsPath gp = new GraphicsPath();
            for (int i = 0; i < Fig.Count; i++)
            {
                ArrayList CurveArray = (ArrayList)Fig[i];
                GraphicsPath Contour = null;
                GraphicsPath Hole = null;
                GraphicsPath Current = null;

                for (int j = 0; j < CurveArray.Count; j++)
                {

                    if (j == 0)
                    {
                        Contour = new GraphicsPath();
                        Current = Contour;
                    }
                    else
                    {

                        Hole = new GraphicsPath();
                        Current = Hole;

                    }
                    Potrace.Curve[] Curves = (Potrace.Curve[])CurveArray[j];
                    float factor = 1;
                    for (int k = 0; k < Curves.Length; k++)
                    {
                        if (Curves[k].Kind == Potrace.CurveKind.Bezier)
                            Current.AddBezier((float)Curves[k].A.x * factor, (float)Curves[k].A.y * factor, (float)Curves[k].ControlPointA.x * factor, (float)Curves[k].ControlPointA.y * factor,
                                        (float)Curves[k].ControlPointB.x * factor, (float)Curves[k].ControlPointB.y * factor, (float)Curves[k].B.x * factor, (float)Curves[k].B.y * factor);
                        else
                            Current.AddLine((float)Curves[k].A.x * factor, (float)Curves[k].A.y * factor, (float)Curves[k].B.x * factor, (float)Curves[k].B.y * factor);

                    }
                    if (j > 0) Contour.AddPath(Hole, false);
                }
                gp.AddPath(Contour, false);
            }

             g.FillPath(Brushes.Black, gp);

            
            return (Bitmap)I;
        }
        /// <summary>
        /// Exports a figure, created by Potrace from a Bitmap to a csv-formatted string
        /// </summary>
        /// <param name="Fig">Arraylist, which contains vectorinformations about the Curves</param>
        /// <param name="Width">Width of the exportd csv-File</param>
        /// <param name="Height">Height of the exportd csv-File</param>
        /// <returns></returns>
        public static string Export2SVG(ArrayList Fig, int Width, int Height)
        {
            

            StringBuilder svg = new StringBuilder();
            string head = String.Format(@"<?xml version='1.0' standalone='no'?>
<!DOCTYPE svg PUBLIC '-//W3C//DTD SVG 20010904//EN' 
'http://www.w3.org/TR/2001/REC-SVG-20010904/DTD/svg10.dtd'>
<svg version='1.0' xmlns='http://www.w3.org/2000/svg' preserveAspectRatio='xMidYMid meet'
width='{0}' height='{1}'  viewBox='0 0 {0} {1}'>
<g>", Width, Height);
            svg.AppendLine(head);

            foreach (ArrayList Path in Fig)
            {

                bool isContour = true;
                svg.Append("<path d='");
                for(int i=0;i< Path.Count;i++)
                {
                    Potrace.Curve[] Curves = (Potrace.Curve[])Path[i];
                   
                    string curve_path = isContour ? GetContourPath(Curves) : GetHolePath(Curves);

                   

                    if (i == Path.Count-1)
                    {
                       svg.Append(curve_path);
                    }
                    else
                    {
                     svg.AppendLine(curve_path);
                    }

                    isContour = false;
                   
                }
                svg.AppendLine("' />");

            }



            string foot = @"</g>
</svg>";
            svg.AppendLine(foot);
            svg.Replace("'", "\"");


            return svg.ToString();
        }
        static System.Globalization.CultureInfo enUsCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
        


        private static string  GetContourPath(Potrace.Curve[] Curves)
        {
           
            StringBuilder path = new StringBuilder();
            
            for(int i=0;i<Curves.Length;i++)
            {
                Potrace.Curve Curve = Curves[i];

                
                if (i == 0)
                {
                    path.AppendLine("M" + Curve.A.x.ToString("0.0", enUsCulture) + " " + Curve.A.y.ToString("0.0", enUsCulture));
                }

                if (Curve.Kind == Potrace.CurveKind.Bezier)
                {

                    path.Append("C" + Curve.ControlPointA.x.ToString("0.0", enUsCulture) + " " + Curve.ControlPointA.y.ToString("0.0", enUsCulture) + " " +
                                    Curve.ControlPointB.x.ToString("0.0", enUsCulture) + " " + Curve.ControlPointB.y.ToString("0.0", enUsCulture) + " " +
                                    Curve.B.x.ToString("0.0", enUsCulture) + " " + Curve.B.y.ToString("0.0", enUsCulture));




                }
                if (Curve.Kind == Potrace.CurveKind.Line)
                {
                    path.Append("L" + Curve.B.x.ToString("0.0", enUsCulture) + " " + Curve.B.y.ToString("0.0", enUsCulture));

                }
                if (i == Curves.Length - 1)
                {
                    path.Append("Z");
                }
                else
                {
                    path.AppendLine("");
                }
               
             
            }

            

            return path.ToString();
        }

        private static string  GetHolePath(Potrace.Curve[] Curves)
        {
            StringBuilder path = new StringBuilder();
           
            for (int i = Curves.Length-1; i >=0 ; i--)
            {
                Potrace.Curve Curve = Curves[i];


                if (i == Curves.Length - 1)
                {
                    path.AppendLine("M" + Curve.B.x.ToString("0.0", enUsCulture) + " " + Curve.B.y.ToString("0.0", enUsCulture));
                }

                if (Curve.Kind == Potrace.CurveKind.Bezier)
                {



                    path.Append("C" + Curve.ControlPointB.x.ToString("0.0", enUsCulture) + " " + Curve.ControlPointB.y.ToString("0.0", enUsCulture) + " " +
                                  Curve.ControlPointA.x.ToString("0.0", enUsCulture) + " " + Curve.ControlPointA.y.ToString("0.0", enUsCulture) + " " +
                                  Curve.A.x.ToString("0.0", enUsCulture) + " " + Curve.A.y.ToString("0.0", enUsCulture));



                }
                if (Curve.Kind == Potrace.CurveKind.Line)
                {
                    path.Append("L" + Curve.B.x.ToString("0.0", enUsCulture) + " " + Curve.B.y.ToString("0.0", enUsCulture));

                }
                if (i == 0)
                {
                    path.Append("Z");
                }
                else
                {
                    path.AppendLine("");
                }

            }

           

            return path.ToString();
        }
    }
}
