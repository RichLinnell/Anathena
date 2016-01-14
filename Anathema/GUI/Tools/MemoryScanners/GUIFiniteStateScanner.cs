﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;
using Anathema.Properties;

namespace Anathema
{
    public partial class GUIFiniteStateScanner : DockContent, IFiniteStateScannerView
    {
        private FiniteStateScannerPresenter FiniteStateScannerPresenter;

        private List<ToolStripButton> ScanOptionButtons;

        private FiniteStateMachine FiniteStateMachine;
        private FiniteState MousedOverState;
        private Point[] SelectionLine;
        private Point RightClickLocation;

        // Drawing Variables:
        private static Font DrawFont = new Font(FontFamily.GenericSerif, 10.0f);
        private static Pen TransitionLine = new Pen(Color.Black, 3);
        private static Pen PendingLine = new Pen(Color.Red, 2);
        private static Int32 StateRadius = Resources.StateHighlighted.Width / 2;
        private const Int32 StateEdgeSize = 8;
        private static Int32 LineOffset = (Int32)TransitionLine.Width / 2;
        private const Int32 LineFloatOffset = 8;
        private const Int32 VariableBorderSize = 4;
        private const Int32 ArrowSize = 4;

        public GUIFiniteStateScanner()
        {
            InitializeComponent();
            FSMBuilderPanel.Paint += new PaintEventHandler(FSMBuilderPanel_Paint);

            FiniteStateScannerPresenter = new FiniteStateScannerPresenter(this, new FiniteStateScanner());
            FiniteStateScannerPresenter.SetStateRadius(StateRadius);
            FiniteStateScannerPresenter.SetStateEdgeSize(StateEdgeSize);

            ScanOptionButtons = new List<ToolStripButton>();

            InitializeValueTypeComboBox();
            InitializeScanOptionButtons();
            EvaluateScanOptions(EqualButton);
            EnableGUI();
        }

        private void InitializeValueTypeComboBox()
        {
            foreach (Type Primitive in PrimitiveTypes.GetPrimitiveTypes())
                ValueTypeComboBox.Items.Add(Primitive.Name);

            ValueTypeComboBox.SelectedIndex = ValueTypeComboBox.Items.IndexOf(typeof(Int32).Name);
        }

        private void InitializeScanOptionButtons()
        {
            ScanOptionButtons.Add(UnchangedButton);
            ScanOptionButtons.Add(ChangedButton);
            ScanOptionButtons.Add(IncreasedButton);
            ScanOptionButtons.Add(DecreasedButton);
            ScanOptionButtons.Add(NotEqualButton);
            ScanOptionButtons.Add(EqualButton);
            ScanOptionButtons.Add(GreaterThanButton);
            ScanOptionButtons.Add(LessThanButton);
            ScanOptionButtons.Add(IncreasedByXButton);
            ScanOptionButtons.Add(DecreasedByXButton);
        }

        public void ScanFinished()
        {
            EnableGUI();
        }

        public void UpdateDisplay(FiniteStateMachine FiniteStateMachine, FiniteState MousedOverState, Point[] SelectionLine)
        {
            this.FiniteStateMachine = FiniteStateMachine;
            this.MousedOverState = MousedOverState;
            this.SelectionLine = SelectionLine;
            FSMBuilderPanel.Invalidate();
        }


        private void Draw(Graphics Graphics)
        {
            if (FiniteStateMachine == null)
                return;

            foreach (FiniteState State in FiniteStateMachine)
            {
                Image DrawImage;

                if (State == FiniteStateMachine.GetStartState())
                    DrawImage = Resources.StartState;
                else if (State == FiniteStateMachine.GetEndState())
                    DrawImage = Resources.EndState;
                else
                    DrawImage = Resources.IntermediateState;

                Graphics.DrawImage(DrawImage, State.Location.X - StateRadius, State.Location.Y - StateRadius, DrawImage.Width, DrawImage.Height);
            }

            if (MousedOverState != null)
                Graphics.DrawImage(Resources.StateHighlighted, MousedOverState.Location.X - StateRadius, MousedOverState.Location.Y - StateRadius, Resources.StateHighlighted.Width, Resources.StateHighlighted.Height);

            if (SelectionLine != null && SelectionLine.Length == 2)
                Graphics.DrawLine(PendingLine, SelectionLine[0], SelectionLine[1]);

            foreach (FiniteState State in FiniteStateMachine)
            {
                foreach (KeyValuePair<ScanConstraint, FiniteState> Transition in State)
                {
                    // Calculate start and end points of the transitio line
                    Point StartPoint = State.GetEdgePoint(Transition.Value.Location, StateRadius);
                    Point EndPoint = Transition.Value.GetEdgePoint(State.Location, StateRadius);
                    StartPoint.Y += LineOffset;
                    EndPoint.Y += LineOffset;

                    // Draw transition line
                    Point MidPoint = new Point((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2);
                    Graphics.DrawLine(TransitionLine, StartPoint, EndPoint);

                    // Draw arrow head
                    //Point[] ArrowHeadPoints = new Point[3];
                    //ArrowHeadPoints[0] = EndPoint;
                    //ArrowHeadPoints[1] = EndPoint;
                    //ArrowHeadPoints[2] = EndPoint;
                    Graphics.FillEllipse(Brushes.Black, EndPoint.X - ArrowSize, EndPoint.Y - ArrowSize, ArrowSize * 2, ArrowSize * 2);

                    // Draw comparison image
                    Point ImageLocation = new Point(MidPoint.X - Resources.Equal.Width / 2, MidPoint.Y - Resources.Equal.Height - LineFloatOffset);
                    switch (Transition.Key.Constraint)
                    {
                        case ConstraintsEnum.Changed:
                            Graphics.DrawImage(Resources.Changed, ImageLocation.X, ImageLocation.Y, Resources.Changed.Width, Resources.Changed.Height);
                            break;
                        case ConstraintsEnum.Unchanged:
                            Graphics.DrawImage(Resources.Unchanged, ImageLocation.X, ImageLocation.Y, Resources.Unchanged.Width, Resources.Unchanged.Height);
                            break;
                        case ConstraintsEnum.Decreased:
                            Graphics.DrawImage(Resources.Decreased, ImageLocation.X, ImageLocation.Y, Resources.Decreased.Width, Resources.Decreased.Height);
                            break;
                        case ConstraintsEnum.Increased:
                            Graphics.DrawImage(Resources.Increased, ImageLocation.X, ImageLocation.Y, Resources.Increased.Width, Resources.Increased.Height);
                            break;
                        case ConstraintsEnum.GreaterThan:
                            Graphics.DrawImage(Resources.GreaterThan, ImageLocation.X, ImageLocation.Y, Resources.GreaterThan.Width, Resources.GreaterThan.Height);
                            break;
                        case ConstraintsEnum.LessThan:
                            Graphics.DrawImage(Resources.LessThan, ImageLocation.X, ImageLocation.Y, Resources.LessThan.Width, Resources.LessThan.Height);
                            break;
                        case ConstraintsEnum.Equal:
                            Graphics.DrawImage(Resources.Equal, ImageLocation.X, ImageLocation.Y, Resources.Equal.Width, Resources.Equal.Height);
                            break;
                        case ConstraintsEnum.NotEqual:
                            Graphics.DrawImage(Resources.NotEqual, ImageLocation.X, ImageLocation.Y, Resources.NotEqual.Width, Resources.NotEqual.Height);
                            break;
                        case ConstraintsEnum.IncreasedByX:
                            Graphics.DrawImage(Resources.PlusX, ImageLocation.X, ImageLocation.Y, Resources.PlusX.Width, Resources.PlusX.Height);
                            break;
                        case ConstraintsEnum.DecreasedByX:
                            Graphics.DrawImage(Resources.MinusX, ImageLocation.X, ImageLocation.Y, Resources.MinusX.Width, Resources.MinusX.Height);
                            break;
                        default:
                        case ConstraintsEnum.Invalid:
                            break;
                    }

                    // Draw transition value if applicable
                    if (Transition.Key.Value != null)
                    {
                        String DrawText = Transition.Key.Value.ToString();
                        SizeF TextSize = Graphics.MeasureString(DrawText, DrawFont);
                        PointF TextLocation = new PointF(MidPoint.X - TextSize.Width / 2, MidPoint.Y + LineFloatOffset);
                        Graphics.FillEllipse(Brushes.Black, TextLocation.X - VariableBorderSize, TextLocation.Y - VariableBorderSize, TextSize.Width + VariableBorderSize * 2, TextSize.Height + VariableBorderSize);
                        Graphics.DrawString(DrawText, DrawFont, Brushes.White, TextLocation);
                    }
                }
            }
        }

        private void EvaluateScanOptions(ToolStripButton Sender)
        {
            ConstraintsEnum ValueConstraint = ConstraintsEnum.Invalid;

            foreach (ToolStripButton Button in ScanOptionButtons)
                if (Button != Sender)
                    Button.Checked = false;
                else
                    Button.Checked = true;

            if (Sender == EqualButton)
                ValueConstraint = ConstraintsEnum.Equal;
            else if (Sender == NotEqualButton)
                ValueConstraint = ConstraintsEnum.NotEqual;
            else if (Sender == ChangedButton)
                ValueConstraint = ConstraintsEnum.Changed;
            else if (Sender == UnchangedButton)
                ValueConstraint = ConstraintsEnum.Unchanged;
            else if (Sender == IncreasedButton)
                ValueConstraint = ConstraintsEnum.Increased;
            else if (Sender == DecreasedButton)
                ValueConstraint = ConstraintsEnum.Decreased;
            else if (Sender == GreaterThanButton)
                ValueConstraint = ConstraintsEnum.GreaterThan;
            else if (Sender == LessThanButton)
                ValueConstraint = ConstraintsEnum.LessThan;
            else if (Sender == IncreasedByXButton)
                ValueConstraint = ConstraintsEnum.IncreasedByX;
            else if (Sender == DecreasedByXButton)
                ValueConstraint = ConstraintsEnum.DecreasedByX;

            switch (ValueConstraint)
            {
                case ConstraintsEnum.Changed:
                case ConstraintsEnum.Unchanged:
                case ConstraintsEnum.Decreased:
                case ConstraintsEnum.Increased:
                    ValueTextBox.Enabled = false;
                    ValueTextBox.Text = "";
                    break;
                case ConstraintsEnum.Invalid:
                case ConstraintsEnum.GreaterThan:
                case ConstraintsEnum.LessThan:
                case ConstraintsEnum.Equal:
                case ConstraintsEnum.NotEqual:
                case ConstraintsEnum.IncreasedByX:
                case ConstraintsEnum.DecreasedByX:
                    ValueTextBox.Enabled = true;
                    break;
            }

            if (Conversions.StringToPrimitiveType(ValueTypeComboBox.SelectedItem.ToString()) == typeof(Single) ||
                Conversions.StringToPrimitiveType(ValueTypeComboBox.SelectedItem.ToString()) == typeof(Double))
            {
                //FilterScientificNotationCheckBox.Visible = true;
            }
            else
            {
                //FilterScientificNotationCheckBox.Visible = false;
            }

            FiniteStateScannerPresenter.SetValueConstraintSelection(ValueConstraint);
        }

        private void DisableGUI()
        {
            StartScanButton.Enabled = false;
        }

        private void EnableGUI()
        {
            StartScanButton.Enabled = true;
        }

        #region Events

        private void FSMBuilderPanel_MouseDown(Object Sender, MouseEventArgs E)
        {
            if (E.Button != MouseButtons.Left)
                return;

            FiniteStateScannerPresenter.BeginAction(E.Location);
        }

        private void FSMBuilderPanel_MouseMove(Object Sender, MouseEventArgs E)
        {
            FiniteStateScannerPresenter.UpdateAction(E.Location);
        }

        private void FSMBuilderPanel_MouseUp(Object Sender, MouseEventArgs E)
        {
            if (E.Button != MouseButtons.Left)
                return;

            FiniteStateScannerPresenter.FinishAction(E.Location, ValueTextBox.Text.ToString());
        }

        protected void FSMBuilderPanel_Paint(Object Sender, PaintEventArgs E)
        {
            Draw(E.Graphics);
        }

        private void StartScanButton_Click(Object Sender, EventArgs E)
        {
            FiniteStateScannerPresenter.BeginScan();
            DisableGUI();
        }

        private void StopScanButton_Click(object sender, EventArgs e)
        {
            FiniteStateScannerPresenter.EndScan();
        }

        private void ChangedButton_Click(Object Sender, EventArgs E)
        {
            EvaluateScanOptions((ToolStripButton)Sender);
        }

        private void NotEqualButton_Click(Object Sender, EventArgs E)
        {
            EvaluateScanOptions((ToolStripButton)Sender);
        }

        private void UnchangedButton_Click(Object Sender, EventArgs E)
        {
            EvaluateScanOptions((ToolStripButton)Sender);
        }

        private void IncreasedButton_Click(Object Sender, EventArgs E)
        {
            EvaluateScanOptions((ToolStripButton)Sender);
        }

        private void DecreasedButton_Click(Object Sender, EventArgs E)
        {
            EvaluateScanOptions((ToolStripButton)Sender);
        }

        private void EqualButton_Click(Object Sender, EventArgs E)
        {
            EvaluateScanOptions((ToolStripButton)Sender);
        }

        private void GreaterThanButton_Click(Object Sender, EventArgs E)
        {
            EvaluateScanOptions((ToolStripButton)Sender);
        }

        private void LessThanButton_Click(Object Sender, EventArgs E)
        {
            EvaluateScanOptions((ToolStripButton)Sender);
        }

        private void IncreasedByXButton_Click(Object Sender, EventArgs E)
        {
            EvaluateScanOptions((ToolStripButton)Sender);
        }

        private void DecreasedByXButton_Click(Object Sender, EventArgs E)
        {
            EvaluateScanOptions((ToolStripButton)Sender);
        }

        private void ValueTypeComboBox_SelectedIndexChanged(Object Sender, EventArgs E)
        {
            FiniteStateScannerPresenter.SetElementType(ValueTypeComboBox.SelectedItem.ToString());
        }

        private void StateContextMenuStrip_Opening(Object Sender, CancelEventArgs E)
        {
            RightClickLocation = FSMBuilderPanel.PointToClient(Cursor.Position);
            if (!FiniteStateScannerPresenter.IsStateAtPoint(RightClickLocation))
                E.Cancel = true;
        }

        private void StartStateToolStripMenuItem_Click(Object Sender, EventArgs E)
        {

        }

        private void NoEventToolStripMenuItem_Click(Object Sender, EventArgs E)
        {
            FiniteStateScannerPresenter.SetStateEvent(RightClickLocation, FiniteState.StateEventEnum.None);
        }

        private void MarkValidToolStripMenuItem_Click(Object Sender, EventArgs E)
        {
            FiniteStateScannerPresenter.SetStateEvent(RightClickLocation, FiniteState.StateEventEnum.MarkValid);
        }

        private void MarkInvalidToolStripMenuItem_Click(Object Sender, EventArgs E)
        {
            FiniteStateScannerPresenter.SetStateEvent(RightClickLocation, FiniteState.StateEventEnum.MarkInvalid);
        }

        private void EndScanToolStripMenuItem_Click(Object Sender, EventArgs E)
        {
            FiniteStateScannerPresenter.SetStateEvent(RightClickLocation, FiniteState.StateEventEnum.EndScan);
        }

        private void DeleteStateToolStripMenuItem_Click(Object Sender, EventArgs E)
        {
            FiniteStateScannerPresenter.DeleteAtPoint(RightClickLocation);
        }

        #endregion

    } // End class

} // End namespace