using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using VRageMath;
using VRage.Game;
using VRage.Collections;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;

namespace SEScripts.OrientationTools
{
    class OrientationTools : MyGridProgram
    {
        IMyGridTerminalSystem GTS;
        IMyShipController Controller;
        IMyTimerBlock Timer;
        List<IMyGyro> Gyros;
        static MyGridProgram P;
        Vector3D TargetForward;
        Vector3D TargetRight;


        public OrientationTools()
        {
            //Logger.DEBUG = true;   
            P = this;


            GTS = GridTerminalSystem;
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyShipController>(blocks);
            Controller = blocks[0] as IMyShipController;
            blocks.Clear();
            GTS.GetBlocksOfType<IMyTimerBlock>(blocks);
            Timer = blocks[0] as IMyTimerBlock;
            blocks.Clear();
            GTS.GetBlocksOfType<IMyGyro>(blocks);
            Gyros = new List<IMyGyro>();
            foreach (IMyTerminalBlock block in blocks)
            {
                Gyros.Add((IMyGyro)block);
            }

            Vector3 pos = Controller.GetPosition();
            TargetForward = new Vector3D(-7214.17, 123307.23, -20782.47);
            TargetRight = Vector3D.CalculatePerpendicularVector(TargetForward);

            //ActiveMode = "forward";  
        }


        public void Main(string argument)
        {


            MatrixD matrix = Timer.CubeGrid.WorldMatrix;
            Face(matrix.Forward, matrix.Right, TargetForward, TargetRight);


            //Echo("rotation: " + db.Get("ROTATION"));  
            //Echo("swinging: " + db.Get("SWINGING"));  
            //ApplyMode();  
        }

        public void ScheduleRefresh()
        {
            Timer.GetActionWithName("TriggerNow").Apply(Timer);
        }


        /*public void ApplyMode()  
        {  
  
            Base6Directions.Direction backward =  
                Base6Directions.GetFlippedDirection(Controller.Orientation.Forward);  
            Base6Directions.Direction forward = Controller.Orientation.Forward;  
            Base6Directions.Direction left =  
                Base6Directions.GetLeft(Controller.Orientation.Up, Controller.Orientation.Forward);  
            Base6Directions.Direction right =  
                Base6Directions.GetFlippedDirection(  
                    Base6Directions.GetLeft(  
                        Controller.Orientation.Up,  
                        Controller.Orientation.Forward  
                    )  
               );  
            //SetRotation(Gyros, "Yaw", Single.Parse(db.Get<string>("ROTATION")));  
  
        }*/

        public void Face(Vector3D forward, Vector3D right, Vector3D desiredForward, Vector3D desiredRight)
        {
            forward.Normalize();
            right.Normalize();
            Vector3D up = Vector3D.Cross(right, forward);
            up.Normalize();
            //Vector3D down = Vector3D.Zero - up;  
            //down.Normalize();  
            //Vector3D gravity = RemoteControl.GetNaturalGravity();  
            //gravity.Normalize();  

            //Vector3D desiredRight = Vector3D.Cross(gravity, forward);  
            //desiredRight.Normalize();  
            //Vector3D desiredForward = Vector3D.Cross(right, gravity);  
            //desiredForward.Normalize();  
            Vector3D desiredDown = Vector3D.Cross(desiredRight, desiredForward);


            ResetGyros(Gyros);

            Echo("right displacement: " + Vector3D.Dot(right, desiredRight).ToString());
            //Echo("right angle: " + Vector3D.Dot(right, gravity).ToString());   
            Echo("forward displacement: " + Vector3D.Dot(forward, desiredForward).ToString());
            //Echo("forward angle: " + Vector3D.Dot(forward, gravity).ToString());   
            //Echo("total displacement: " + Vector3D.Dot(down, gravity).ToString());   

            float forwardAngle = (float)Vector3D.Dot(forward, desiredDown);

            float minimum = 0f;
            /*if (db.Get<string>("SWINGING") == "true")  
            {  
                if (forwardAngle > 0)  
                {  
                    minimum = 0.02f;  
                }  
                else  
                {  
                    minimum = -0.02f;  
                }  
            }*/
            float pitch = ((3f * forwardAngle) + minimum);
            Echo("Pitch: " + pitch.ToString());
            SetRotation(Gyros, "Pitch", pitch);

            float rightAngle = (float)Vector3D.Dot(right, desiredDown);

            minimum = 0f;
            float roll = (3f * -rightAngle) + minimum;
            Echo("Roll: " + roll.ToString());
            SetRotation(Gyros, "Roll", roll);

            if (true || (Math.Abs(forwardAngle) < 0.4 && Math.Abs(forwardAngle) > 0.0005f)
                || (Math.Abs(rightAngle) < 0.4 && Math.Abs(rightAngle) > 0.0005f))
            {
                ScheduleRefresh();
            }
        }

        public void SetRotation(List<IMyGyro> gyros, string axis, float value)
        {
            foreach (IMyGyro gyro in gyros)
            {
                string gAxis = axis;
                float gValue = value;
                ConvertRotation(gyro, ref gAxis, ref gValue);
                gyro.SetValue<Single>(gAxis, gValue);
            }
        }

        public void ConvertRotation(IMyGyro gyro, ref string axis, ref float value)
        {
            Base6Directions.Direction shipUp = Controller.Orientation.Up;
            Base6Directions.Direction shipForward = Controller.Orientation.Forward;
            Base6Directions.Direction shipLeft = Base6Directions.GetLeft(shipUp, shipForward);

            Base6Directions.Direction up = gyro.Orientation.Up;
            Base6Directions.Direction down = Base6Directions.GetFlippedDirection(up);
            Base6Directions.Direction forward = gyro.Orientation.Forward;
            Base6Directions.Direction backward = Base6Directions.GetFlippedDirection(forward);
            Base6Directions.Direction left = Base6Directions.GetLeft(up, forward);
            Base6Directions.Direction right = Base6Directions.GetFlippedDirection(left);

            //Echo("ConvertRotation(" + axis + ", " + value.ToString());  
            //Echo(gyro.CustomName + ": " + forward.ToString() + "/" + shipForward.ToString());  

            if (up == shipUp && forward == shipForward)
            {
                return;
            }

            if (axis == "Roll")
            {
                if (shipForward == backward || shipForward == up || shipForward == left)
                {
                    value = -value;
                }
                if (shipForward == up || shipForward == down)
                {
                    axis = "Yaw";
                }
                if (shipForward == left || shipForward == right)
                {
                    axis = "Pitch";
                }
            }
            else if (axis == "Pitch")
            {
                if (shipLeft == right || shipLeft == down || shipLeft == backward)
                {
                    value = -value;
                }
                if (shipLeft == up || shipLeft == down)
                {
                    axis = "Yaw";
                }
                if (shipLeft == forward || shipLeft == backward)
                {
                    axis = "Roll";
                }
            }
            else if (axis == "Yaw")
            {
                if (shipUp == forward || shipUp == down || shipUp == left)
                {
                    value = -value;
                }
                if (shipUp == forward || shipUp == backward)
                {
                    axis = "Roll";
                }
                if (shipUp == left || shipUp == right)
                {
                    axis = "Pitch";
                }
            }
            //Echo("Result: " + axis + ": " + value.ToString());  

        }

        public void ResetGyros(List<IMyGyro> gyros)
        {
            foreach (IMyGyro gyro in gyros)
            {
                ResetGyro(gyro);
            }
        }

        public void ResetGyro(IMyGyro gyro)
        {
            gyro.SetValue<Single>("Yaw", 0f);
            gyro.SetValue<Single>("Pitch", 0f);
            gyro.SetValue<Single>("Roll", 0f);
        }
    }
}
