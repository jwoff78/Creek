//
// Copyright �2006, 2007, Martin R. Gagn� (martingagne@gmail.com)
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
//   - Redistributions of source code must retain the above copyright notice, 
//     this list of conditions and the following disclaimer.
//
//   - Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
// IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, 
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
// OF SUCH DAMAGE.
//

namespace Creek.UI
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    public partial class LoadingCircle : Control
    {
        // Constants =========================================================
        private const double NumberOfDegreesInCircle = 360;
        private const double NumberOfDegreesInHalfCircle = NumberOfDegreesInCircle / 2;
        private const int DefaultInnerCircleRadius = 8;
        private const int DefaultOuterCircleRadius = 10;
        private const int DefaultNumberOfSpoke = 10;
        private const int DefaultSpokeThickness = 4;
        private readonly Color DefaultColor = Color.DarkGray;

        private const int MacOSXInnerCircleRadius = 5;
        private const int MacOSXOuterCircleRadius = 11;
        private const int MacOSXNumberOfSpoke = 12;
        private const int MacOSXSpokeThickness = 2;

        private const int FireFoxInnerCircleRadius = 6;
        private const int FireFoxOuterCircleRadius = 7;
        private const int FireFoxNumberOfSpoke = 9;
        private const int FireFoxSpokeThickness = 4;

        private const int IE7InnerCircleRadius = 8;
        private const int IE7OuterCircleRadius = 9;
        private const int IE7NumberOfSpoke = 24;
        private const int IE7SpokeThickness = 4;

        // Enumeration =======================================================
        public enum StylePresets
        {
            MacOSX,
            Firefox,
            IE7,
            Custom
        }

        // Attributes ========================================================
        private Timer m_Timer;
        private bool m_IsTimerActive;
        private int m_NumberOfSpoke;
        private int m_SpokeThickness;
        private int m_ProgressValue;
        private int m_OuterCircleRadius;
        private int m_InnerCircleRadius;
        private PointF m_CenterPoint;
        private Color m_Color;
        private Color[] m_Colors;
        private double[] m_Angles;
        private StylePresets m_StylePreset;

        // Properties ========================================================
        /// <summary>
        /// Gets or sets the lightest color of the circle.
        /// </summary>
        /// <value>The lightest color of the circle.</value>
        [TypeConverter("System.Drawing.ColorConverter"),
         Category("LoadingCircle"),
         Description("Sets the color of spoke.")]
        public Color Color
        {
            get
            {
                return this.m_Color;
            }
            set
            {
                this.m_Color = value;

                this.GenerateColorsPallet();
                this.Invalidate();
            }
        }

        public static void Bind(LoadingCircle lc, Control c)
        {
            c.Controls.Add(lc);
        }

        public static void Release(Control c)
        {
            c.Controls.RemoveAt(c.Controls.Count-1);
        }

        /// <summary>
        /// Gets or sets the outer circle radius.
        /// </summary>
        /// <value>The outer circle radius.</value>
        [System.ComponentModel.Description("Gets or sets the radius of outer circle."),
         System.ComponentModel.Category("LoadingCircle")]
        public int OuterCircleRadius
        {
            get
            {
                if (this.m_OuterCircleRadius == 0)
                    this.m_OuterCircleRadius = DefaultOuterCircleRadius;

                return this.m_OuterCircleRadius;
            }
            set
            {
                this.m_OuterCircleRadius = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the inner circle radius.
        /// </summary>
        /// <value>The inner circle radius.</value>
        [System.ComponentModel.Description("Gets or sets the radius of inner circle."),
         System.ComponentModel.Category("LoadingCircle")]
        public int InnerCircleRadius
        {
            get
            {
                if (this.m_InnerCircleRadius == 0)
                    this.m_InnerCircleRadius = DefaultInnerCircleRadius;

                return this.m_InnerCircleRadius;
            }
            set
            {
                this.m_InnerCircleRadius = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the number of spoke.
        /// </summary>
        /// <value>The number of spoke.</value>
        [System.ComponentModel.Description("Gets or sets the number of spoke."),
        System.ComponentModel.Category("LoadingCircle")]
        public int NumberSpoke
        {
            get
            {
                if (this.m_NumberOfSpoke == 0)
                    this.m_NumberOfSpoke = DefaultNumberOfSpoke;

                return this.m_NumberOfSpoke;
            }
            set
            {
                if (this.m_NumberOfSpoke != value && this.m_NumberOfSpoke > 0)
                {
                    this.m_NumberOfSpoke = value;
                    this.GenerateColorsPallet();
                    this.GetSpokesAngles();

                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:LoadingCircle"/> is active.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        [System.ComponentModel.Description("Gets or sets the number of spoke."),
        System.ComponentModel.Category("LoadingCircle")]
        public bool Active
        {
            get
            {
                return this.m_IsTimerActive;
            }
            set
            {
                this.m_IsTimerActive = value;
                this.ActiveTimer();
            }
        }

        /// <summary>
        /// Gets or sets the spoke thickness.
        /// </summary>
        /// <value>The spoke thickness.</value>
        [System.ComponentModel.Description("Gets or sets the thickness of a spoke."),
        System.ComponentModel.Category("LoadingCircle")]
        public int SpokeThickness
        {
            get
            {
                if (this.m_SpokeThickness <= 0)
                    this.m_SpokeThickness = DefaultSpokeThickness;

                return this.m_SpokeThickness;
            }
            set
            {
                this.m_SpokeThickness = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the rotation speed.
        /// </summary>
        /// <value>The rotation speed.</value>
        [System.ComponentModel.Description("Gets or sets the rotation speed. Higher the slower."),
        System.ComponentModel.Category("LoadingCircle")]
        public int RotationSpeed
        {
            get
            {
                return this.m_Timer.Interval;
            }
            set
            {
                if (value > 0)
                    this.m_Timer.Interval = value;
            }
        }

        /// <summary>
        /// Quickly sets the style to one of these presets, or a custom style if desired
        /// </summary>
        /// <value>The style preset.</value>
        [Category("LoadingCircle"),
         Description("Quickly sets the style to one of these presets, or a custom style if desired"),
         DefaultValue(typeof(StylePresets), "Custom")]
        public StylePresets StylePreset
        {
            get { return this.m_StylePreset; }
            set
            {
                this.m_StylePreset = value;

                switch (this.m_StylePreset)
                {
                    case StylePresets.MacOSX:
                        this.SetCircleAppearance(MacOSXNumberOfSpoke, 
                            MacOSXSpokeThickness, MacOSXInnerCircleRadius, 
                            MacOSXOuterCircleRadius);
                        break;
                    case StylePresets.Firefox:
                        this.SetCircleAppearance(FireFoxNumberOfSpoke,
                            FireFoxSpokeThickness, FireFoxInnerCircleRadius,
                            FireFoxOuterCircleRadius);
                        break;
                    case StylePresets.IE7:
                        this.SetCircleAppearance(IE7NumberOfSpoke,
                            IE7SpokeThickness, IE7InnerCircleRadius,
                            IE7OuterCircleRadius);
                        break;
                    case StylePresets.Custom:
                        this.SetCircleAppearance(DefaultNumberOfSpoke, 
                            DefaultSpokeThickness, 
                            DefaultInnerCircleRadius, 
                            DefaultOuterCircleRadius); 
                        break;
                }
            }
        }

        // Construtor ========================================================
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LoadingCircle"/> class.
        /// </summary>
        public LoadingCircle()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            this.m_Color = this.DefaultColor;

            this.GenerateColorsPallet();
            this.GetSpokesAngles();
            this.GetControlCenterPoint();

            this.m_Timer = new Timer();
            this.m_Timer.Tick += new EventHandler(this.aTimer_Tick);
            this.ActiveTimer();

            this.Resize += new EventHandler(this.LoadingCircle_Resize);
        }

        // Events ============================================================
        /// <summary>
        /// Handles the Resize event of the LoadingCircle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        void LoadingCircle_Resize(object sender, EventArgs e)
        {
            this.GetControlCenterPoint();
        }

        /// <summary>
        /// Handles the Tick event of the aTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        void aTimer_Tick(object sender, EventArgs e)
        {
            this.m_ProgressValue = ++this.m_ProgressValue % this.m_NumberOfSpoke;
            this.Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.m_NumberOfSpoke > 0)
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                int intPosition = this.m_ProgressValue;
                for (int intCounter = 0; intCounter < this.m_NumberOfSpoke; intCounter++)
                {
                    intPosition = intPosition % this.m_NumberOfSpoke;
                    this.DrawLine(e.Graphics,
                             this.GetCoordinate(this.m_CenterPoint, this.m_InnerCircleRadius, this.m_Angles[intPosition]),
                             this.GetCoordinate(this.m_CenterPoint, this.m_OuterCircleRadius, this.m_Angles[intPosition]),
                             this.m_Colors[intCounter], this.m_SpokeThickness);
                    intPosition++;
                }
            }

            base.OnPaint(e);
        }

        // Overridden Methods ================================================
        /// <summary>
        /// Retrieves the size of a rectangular area into which a control can be fitted.
        /// </summary>
        /// <param name="proposedSize">The custom-sized area for a control.</param>
        /// <returns>
        /// An ordered pair of type <see cref="T:System.Drawing.Size"></see> representing the width and height of a rectangle.
        /// </returns>
        public override Size GetPreferredSize(Size proposedSize)
        {
            proposedSize.Width =
                (this.m_OuterCircleRadius + this.m_SpokeThickness) * 2;

            return proposedSize;
        }

        // Methods ===========================================================
        /// <summary>
        /// Darkens a specified color.
        /// </summary>
        /// <param name="_objColor">Color to darken.</param>
        /// <param name="_intPercent">The percent of darken.</param>
        /// <returns>The new color generated.</returns>
        private Color Darken(Color _objColor, int _intPercent)
        {
            int intRed = _objColor.R;
            int intGreen = _objColor.G;
            int intBlue = _objColor.B;
            return Color.FromArgb(_intPercent, Math.Min(intRed, byte.MaxValue), Math.Min(intGreen, byte.MaxValue), Math.Min(intBlue, byte.MaxValue));
        }

        /// <summary>
        /// Generates the colors pallet.
        /// </summary>
        private void GenerateColorsPallet()
        {
            this.m_Colors = this.GenerateColorsPallet(this.m_Color, this.Active, this.m_NumberOfSpoke);
        }

        /// <summary>
        /// Generates the colors pallet.
        /// </summary>
        /// <param name="_objColor">Color of the lightest spoke.</param>
        /// <param name="_blnShadeColor">if set to <c>true</c> the color will be shaded on X spoke.</param>
        /// <returns>An array of color used to draw the circle.</returns>
        private Color[] GenerateColorsPallet(Color _objColor, bool _blnShadeColor, int _intNbSpoke)
        {
            Color[] objColors = new Color[this.NumberSpoke];

            // Value is used to simulate a gradient feel... For each spoke, the 
            // color will be darken by value in intIncrement.
            byte bytIncrement = (byte)(byte.MaxValue / this.NumberSpoke);

            //Reset variable in case of multiple passes
            byte PERCENTAGE_OF_DARKEN = 0;

            for (int intCursor = 0; intCursor < this.NumberSpoke; intCursor++)
            {
                if (_blnShadeColor)
                {
                    if (intCursor == 0 || intCursor < this.NumberSpoke - _intNbSpoke)
                        objColors[intCursor] = _objColor;
                    else
                    {
                        // Increment alpha channel color
                        PERCENTAGE_OF_DARKEN += bytIncrement;

                        // Ensure that we don't exceed the maximum alpha
                        // channel value (255)
                        if (PERCENTAGE_OF_DARKEN > byte.MaxValue)
                            PERCENTAGE_OF_DARKEN = byte.MaxValue;

                        // Determine the spoke forecolor
                        objColors[intCursor] = this.Darken(_objColor, PERCENTAGE_OF_DARKEN);
                    }
                }
                else
                    objColors[intCursor] = _objColor;
            }

            return objColors;
        }

        /// <summary>
        /// Gets the control center point.
        /// </summary>
        private void GetControlCenterPoint()
        {
            this.m_CenterPoint = this.GetControlCenterPoint(this);
        }

        /// <summary>
        /// Gets the control center point.
        /// </summary>
        /// <returns>PointF object</returns>
        private PointF GetControlCenterPoint(Control _objControl)
        {
            return new PointF(_objControl.Width / 2, _objControl.Height / 2 - 1);
        }

        /// <summary>
        /// Draws the line with GDI+.
        /// </summary>
        /// <param name="_objGraphics">The Graphics object.</param>
        /// <param name="_objPointOne">The point one.</param>
        /// <param name="_objPointTwo">The point two.</param>
        /// <param name="_objColor">Color of the spoke.</param>
        /// <param name="_intLineThickness">The thickness of spoke.</param>
        private void DrawLine(Graphics _objGraphics, PointF _objPointOne, PointF _objPointTwo,
                              Color _objColor, int _intLineThickness)
        {
            using(Pen objPen = new Pen(new SolidBrush(_objColor), _intLineThickness))
            {
                objPen.StartCap = LineCap.Round;
                objPen.EndCap = LineCap.Round;
                _objGraphics.DrawLine(objPen, _objPointOne, _objPointTwo);
            }
        }

        /// <summary>
        /// Gets the coordinate.
        /// </summary>
        /// <param name="_objCircleCenter">The Circle center.</param>
        /// <param name="_intRadius">The radius.</param>
        /// <param name="_dblAngle">The angle.</param>
        /// <returns></returns>
        private PointF GetCoordinate(PointF _objCircleCenter, int _intRadius, double _dblAngle)
        {
            double dblAngle = Math.PI * _dblAngle / NumberOfDegreesInHalfCircle;

            return new PointF(_objCircleCenter.X + _intRadius * (float)Math.Cos(dblAngle),
                              _objCircleCenter.Y + _intRadius * (float)Math.Sin(dblAngle));
        }

        /// <summary>
        /// Gets the spokes angles.
        /// </summary>
        private void GetSpokesAngles()
        {
            this.m_Angles = this.GetSpokesAngles(this.NumberSpoke);
        }

        /// <summary>
        /// Gets the spoke angles.
        /// </summary>
        /// <param name="_shtNumberSpoke">The number spoke.</param>
        /// <returns>An array of angle.</returns>
        private double[] GetSpokesAngles(int _intNumberSpoke)
        {
            double[] Angles = new double[_intNumberSpoke];
            double dblAngle = (double) NumberOfDegreesInCircle / _intNumberSpoke;

            for (int shtCounter = 0; shtCounter < _intNumberSpoke; shtCounter++)
                Angles[shtCounter] = (shtCounter == 0 ? dblAngle : Angles[shtCounter - 1] + dblAngle);

            return Angles;
        }

        /// <summary>
        /// Actives the timer.
        /// </summary>
        private void ActiveTimer()
        {
            if (this.m_IsTimerActive)
                this.m_Timer.Start();
            else
            {
                this.m_Timer.Stop();
                this.m_ProgressValue = 0;
            }

            this.GenerateColorsPallet();
            this.Invalidate();
        }

        /// <summary>
        /// Sets the circle appearance.
        /// </summary>
        /// <param name="numberSpoke">The number spoke.</param>
        /// <param name="spokeThickness">The spoke thickness.</param>
        /// <param name="innerCircleRadius">The inner circle radius.</param>
        /// <param name="outerCircleRadius">The outer circle radius.</param>
        public void SetCircleAppearance(int numberSpoke, int spokeThickness,
            int innerCircleRadius, int outerCircleRadius)
        {
            this.NumberSpoke = numberSpoke;
            this.SpokeThickness = spokeThickness;
            this.InnerCircleRadius = innerCircleRadius;
            this.OuterCircleRadius = outerCircleRadius;

            this.Invalidate();
        } 
    }
}