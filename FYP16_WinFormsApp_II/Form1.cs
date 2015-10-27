//Copyright (C) 2001-2007 Peter Selinger

//Copyright (C) 2009 Wolfgang Nagl

//This program is free software; you can redistribute it and/or modify  it under the terms of the GNU General Public License as published by  the Free Software Foundation; either version 2 of the License, or (at  your option) any later version.

//This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU  General Public License for more details.

//You should have received a copy of the GNU General Public License  along with this program; if not, write to the Free Software  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307,  USA. See also http://www.gnu.org/.

//See the file COPYING for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using CsPotrace;

using System.Linq;
using System.IO;


namespace FYP16_WinFormsApp_II
{
    public partial class Form1 : Form
    {
        public Form1()

        {
            InitializeComponent();

            cmbEdgeDetection.SelectedIndex = 0; /*edge detection*/
        }
        /// <summary>
        /// img edge detection
        /// </summary>
        private Bitmap originalBitmap = null;
        private Bitmap previewBitmap = null;
        private Bitmap resultBitmap = null;

        /// <summary>
        /// potrace draw
        /// </summary>
        private void draw()
        {
            if (ListOfCurveArray == null) return;
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            GraphicsPath gp = new GraphicsPath();
            for (int i = 0; i < ListOfCurveArray.Count; i++)
            {
                ArrayList CurveArray = (ArrayList)ListOfCurveArray[i];
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
                    if (checkBox2.Checked)
                        //factor = (trackBar1.Value + 1);
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
            if (checkBox1.Checked)
                g.DrawPath(Pens.Red, gp);

        }
        private void showPoints()
        {
            if (ListOfCurveArray == null) return;
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            for (int i = 0; i < ListOfCurveArray.Count; i++)
            {
                ArrayList CurveArray = (ArrayList)ListOfCurveArray[i];
                for (int j = 0; j < CurveArray.Count; j++)
                {
                    Potrace.Curve[] Curves = (Potrace.Curve[])CurveArray[j];

                    float factor = 1;
                    if (checkBox2.Checked)
                        factor = (trackBar1.Value + 1);
                    for (int k = 0; k < Curves.Length; k++)
                    {
                        g.FillRectangle(Brushes.Yellow, (float)((Curves[k].A.x) * factor - 1.5), (float)((Curves[k].A.y) * factor - 1.5), 3, 3);
                    }
                }
            }
        }
        /// <summary>
        /// Load img edge detect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select an image file.";
            ofd.Filter = "Png Images(*.png)|*.png|Jpeg Images(*.jpg)|*.jpg";
            ofd.Filter += "|Bitmap Images(*.bmp)|*.bmp";

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamReader streamReader = new StreamReader(ofd.FileName);
                originalBitmap = (Bitmap)Bitmap.FromStream(streamReader.BaseStream);
                streamReader.Close();

                previewBitmap = originalBitmap.CopyToThisCanvas(pictureBox1.Width);
                pictureBox1.Image = previewBitmap;

                ApplyFilter(true);
            }
        }
        /// <summary>
        /// refresh picture
        /// </summary>
        private void refreshPicture()
        {
            if (Matrix == null) return;
            /*if (radioButton2.Checked)
            {
                Bitmap B = Potrace.BinaryToBitmap(Matrix, true);
                pictureBox1.Width = B.Width * (trackBar1.Value + 1);
                pictureBox1.Height = B.Height * (trackBar1.Value + 1);
                pictureBox1.Image = B;
            }*/
            else
            {
                Bitmap B = new Bitmap(Matrix.GetLength(0) * (trackBar1.Value + 1), Matrix.GetLength(1) * (trackBar1.Value + 1));
                pictureBox1.Width = B.Width;
                pictureBox1.Height = B.Height;
                pictureBox1.Image = B;
            }

            draw();
        }
        private void refreshMatrix()
        {
            if (Bitmap == null) return;
            Matrix = Potrace.BitMapToBinary(Bitmap, trackBar1.Value);
            refreshPicture();

        }

        bool[,] Matrix;
        ArrayList ListOfCurveArray;
        Bitmap Bitmap;

        /// <summary>
        /// Vectorize 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            ListOfCurveArray = new ArrayList();
            Potrace.turdsize = Convert.ToInt32(textBox1.Text);
            try
            {
                Potrace.alphamax = Convert.ToDouble(textBox3.Text);
            }
            catch
            {
                textBox3.Text = Potrace.alphamax.ToString();
            }
            try
            {
                Potrace.opttolerance = Convert.ToDouble(textBox2.Text);
            }
            catch
            {
                textBox2.Text = Potrace.opttolerance.ToString();
            }
            //optimize the path p, replacing sequences of Bezier segments by a
            //single segment when possible.
            Potrace.curveoptimizing = true; //checkBox4.Checked;
            Matrix = Potrace.BitMapToBinary(Bitmap, trackBar1.Value);
            Potrace.potrace_trace(Matrix, ListOfCurveArray);
            refreshMatrix();
          
        }
        /// <summary>
        /// Save as SVG
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (ListOfCurveArray == null)
            {
                MessageBox.Show("Please vectorize the image first");
                return;
            }
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string s = Potrace.Export2SVG(ListOfCurveArray, Bitmap.Width, Bitmap.Height);
                System.IO.StreamWriter FS = new System.IO.StreamWriter(saveFileDialog1.FileName);
                FS.Write(s);
                FS.Close();
            }
        }
        /// <summary>
        /// Disp pic 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// img edge detection
        /// </summary>
        /// <param name="preview"></param>
        private void ApplyFilter(bool preview)
         {
             if (pictureBox1 == null || cmbEdgeDetection.SelectedIndex == -1)
             {
                 return;
             }

             Bitmap selectedSource = null;
             Bitmap bitmapResult = null;

             if (preview == true)
             {
                 selectedSource = previewBitmap;
             }
             else
             {
                 selectedSource = originalBitmap;
             }

             if (selectedSource != null)
             {
                 if (cmbEdgeDetection.SelectedItem.ToString() == "Img Convolution Filter")
                 {
                     bitmapResult = selectedSource;
                 }
                 else if (cmbEdgeDetection.SelectedItem.ToString() == "Laplacian 3x3")
                 {
                     bitmapResult = selectedSource.Laplacian3x3Filter(false);
                 }
               
             }

             if (bitmapResult != null)
             {
                 if (preview == true)
                 {
                     pictureBox1.Image = bitmapResult;
                 }
                 else
                 {
                     resultBitmap = bitmapResult;
                 }
             }
         }
        /// <summary>
        /// Dropdown selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter(true);
        }
        /// <summary>
        /// trackbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            refreshMatrix();
            float p = 100 * (float)trackBar1.Value / (float)255;
            //textBox1.Text = p.ToString("00");
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            float p = 100 * (float)trackBar1.Value / (float)255;
            //textBox1.Text = p.ToString("00");
            textBox1.Text = Potrace.turdsize.ToString();
            textBox3.Text = Potrace.alphamax.ToString();
            textBox2.Text = Potrace.opttolerance.ToString();

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            refreshPicture();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            refreshPicture();
        }
    }
}
