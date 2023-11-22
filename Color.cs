﻿using System;

namespace rt
{
    public class Color
    {
        public static Color RED =     new Color(1.0, 0.0, 0.0, 1.0); 
        public static Color GREEN =   new Color(0.0, 1.0, 0.0, 1.0); 
        public static Color BLUE =    new Color(0.0, 0.0, 1.0, 1.0); 
        public static Color YELLOW =  new Color(1.0, 1.0, 0.0, 1.0); 
        public static Color MAGENTA = new Color(1.0, 0.0, 1.0, 1.0); 
        public static Color CYAN =    new Color(0.0, 1.0, 1.0, 1.0); 
        public static Color WHITE =   new Color(1.0, 1.0, 1.0, 1.0); 
        public static Color ORANGE =  new Color(1.0, 0.5, 0.0, 1.0); 
        public static Color GRAY =  new Color(0.2, 0.2, 0.2, 1.0);

        public double Red { get; set; }
        public double Green { get; set; }
        public double Blue { get; set; }
        public double Alpha { get; set; }

        public Color()
        {
            Red = 0;
            Green = 0;
            Blue = 0;
            Alpha = 0;
        }

        public Color(double red, double green, double blue, double alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public Color(Color c)
        {
            Red = c.Red;
            Green = c.Green;
            Blue = c.Blue;
            Alpha = c.Alpha;
        }

        public System.Drawing.Color ToSystemColor()
        {
            var r = Math.Min((int)Math.Ceiling(Red * 255), 255);
            var g = Math.Min((int)Math.Ceiling(Green * 255), 255);
            var b = Math.Min((int)Math.Ceiling(Blue * 255), 255);
            var a = Math.Min((int)Math.Ceiling(Alpha * 255), 255);

            return System.Drawing.Color.FromArgb(a, r, g, b);
        }

        public static Color operator +(Color a, Color b)
        {
            return new Color(a.Red + b.Red, a.Green + b.Green, a.Blue + b.Blue, a.Alpha + b.Alpha);
        }

        public static Color operator -(Color a, Color b)
        {
            return new Color(a.Red - b.Red, a.Green - b.Green, a.Blue - b.Blue, a.Alpha - b.Alpha);
        }

        public static Color operator *(Color a, Color b)
        {
            return new Color(a.Red * b.Red, a.Green * b.Green, a.Blue * b.Blue, a.Alpha * b.Alpha);
        }

        public static Color operator /(Color a, Color b)
        {
            return new Color(a.Red / b.Red, a.Green / b.Green, a.Blue / b.Blue, a.Alpha / b.Alpha);
        }

        public static Color operator *(Color c, double k)
        {
            return new Color(c.Red * k, c.Green * k, c.Blue * k, c.Alpha * k);
        }

        public static Color operator /(Color c, double k)
        {
            return new Color(c.Red / k, c.Green / k, c.Blue / k, c.Alpha / k);
        }
    }
}