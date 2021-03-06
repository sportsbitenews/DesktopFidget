﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace DesktopFidget
{
    public class MovementFunctions
    {

        public static void HeightCalcLoop()
        {
            //This function calculates the way fidget will float around.
            //Just basic cos stuff.
            //The values can be seen in the paint part of the program when
            //the images are being drawn on the form.
            while (true)
            {
                Var.HeightBonus = Convert.ToInt32(Math.Ceiling(Math.Cos(Var.AngleHeightFlight) * Var.HeightBonusMultiplier));
                Var.WidthBonus = Convert.ToInt32(Math.Ceiling(Math.Cos(Var.AngleWidthFlight) * Var.WidthBonusMultiplier));
                Var.ZBonus = Convert.ToInt32(Math.Ceiling(Math.Cos(Var.AngleWidthFlight) * Var.ZBonusMultiplier) + Var.ZBonusMultiplier *2);
                Var.AngleHeightFlight = Var.AngleHeightFlight + Math.PI / Var.HeightBonusIncreaseMultiplier;
                Var.AngleWidthFlight = Var.AngleWidthFlight + Math.PI / Var.WidthBonusIncreaseMultiplier;
                Var.AngleZFlight = Var.AngleZFlight + Math.PI / Var.ZBonusIncreaseMultiplier;
                if (Var.AngleHeightFlight > 2 * Math.PI) { Var.AngleHeightFlight = 0; }
                if (Var.AngleWidthFlight > 2 * Math.PI) { Var.AngleWidthFlight = 0; }
                if (Var.AngleZFlight > 2 * Math.PI) { Var.AngleZFlight = 0; }
                if (Var.AngleHeightFlight < 0) { Var.AngleHeightFlight = Math.PI * 2; }
                if (Var.AngleWidthFlight < 0) { Var.AngleWidthFlight = Math.PI * 2; }
                if (Var.AngleZFlight < 0) { Var.AngleZFlight = Math.PI * 2; }
                Thread.Sleep(25);
            }
        }

        private static void FlightMovement(Var.Rect _newwindowmovement, Var.Rect _windowcurrentpos, int _sleeptime)
        {
            IntPtr _window = NativeMethods.FindWindowByCaption(IntPtr.Zero, Var.WINDOW_NAME);
            //no moving by user
            Var.AllowManualMovement = false;
            //FIND THE DISTANCE
            int _distancesqr = Convert.ToInt32(Math.Floor(Math.Sqrt(Math.Pow(_newwindowmovement.Left, 2) + Math.Pow(_newwindowmovement.Top, 2))));
            if (_distancesqr > 1000) _distancesqr = 1000;
            if (_distancesqr < 50) return;
            //should we turn around?
            if ((Var.LookingRightWay == true && _newwindowmovement.Left < 0) || (Var.LookingRightWay == false && _newwindowmovement.Left > 0))
            { Var.TurnAroundState = 1; }
            //START MOVING TOWARDS IT
            double _angle = 0;
            double _cosx = Math.Sin(_angle);
            double _cosy = Math.Sin(_angle);
            int _movex = 0;
            int _movey = 0;
            while (_angle < Math.PI)
            {
                _cosx = Math.Cos(_angle);
                _cosy = Math.Cos(_angle);
                //big scary formulae o' flight
                _movex = _windowcurrentpos.Left + Convert.ToInt32(Math.Floor(_newwindowmovement.Left * (1 - ((1 + (_cosx)) / 2))));
                _movey = _windowcurrentpos.Top + Convert.ToInt32(Math.Floor(_newwindowmovement.Top * (1 - ((1 + (_cosy)) / 2))));
                Var.WingsSleepParameter = Convert.ToInt32((_distancesqr / Var.WingsSleepMultiplier) * (1 - (Math.Sin(_angle))) + (Var.WingsSleepParameterDefault - _distancesqr / (Var.WingsSleepMultiplier / 2)));
                NativeMethods.MoveWindow(_window,
                    _movex,
                    _movey,
                    Var.WindowSizeX, Var.WindowSizeY, true);
                _angle = _angle + Math.PI / Var.FlightSpeedMultiplier;
                Thread.Sleep(_sleeptime);
            }
            Var.WingsSleepParameter = Var.WingsSleepParameterDefault;
            Var.AllowManualMovement = true;
        }

        public static void FidgetsMind()
        {
            //Init moving
            Random _rnd = new Random();
            IntPtr _window = NativeMethods.FindWindowByCaption(IntPtr.Zero, Var.WINDOW_NAME);
            Var.Rect _windowcurrentpos = new Var.Rect();
            Var.Rect _newwindowmovement = new Var.Rect();
            int _screenwidth=Screen.PrimaryScreen.Bounds.Width;
            int _screenheight= Screen.PrimaryScreen.Bounds.Height;
            Var.SecondsToNextMovement = Convert.ToInt32(_rnd.Next(Var.MOVEMENT_TIME_MODIFIER - Var.MovementFrequency, (Var.MOVEMENT_TIME_MODIFIER - Var.MovementFrequency) * 2));
            while (true)
            {
                //mouse follow
                while (Var.FollowTheMouse && Var.MovementDistance == 0)
                {
                    //FIND THE TARGET
                    NativeMethods.GetWindowRect(_window, ref _windowcurrentpos);
                    if ((Var.LookingRightWay == true && Cursor.Position.X - _windowcurrentpos.Left - Var.LOWER_BODY_X < 0) || (Var.LookingRightWay == false && Cursor.Position.X - _windowcurrentpos.Left - Var.LOWER_BODY_X > 0))
                        { Var.TurnAroundState = 1; }
                    _newwindowmovement.Left = _windowcurrentpos.Left + Convert.ToInt32(((Cursor.Position.X - _windowcurrentpos.Left - Var.LOWER_BODY_X) * 0.01));
                    _newwindowmovement.Top = _windowcurrentpos.Top + Convert.ToInt32(((Cursor.Position.Y - _windowcurrentpos.Top - Var.LOWER_BODY_Y) * 0.01));
                    int _distancesqr = Convert.ToInt32(Math.Floor(Math.Sqrt(Math.Pow((Cursor.Position.X - _windowcurrentpos.Left - Var.LOWER_BODY_X), 2) + Math.Pow((Cursor.Position.Y - _windowcurrentpos.Top - Var.LOWER_BODY_Y), 2))));
                    int _wsleep = Var.WingsSleepParameterDefault - _distancesqr / 50;
                    if (_wsleep < 20)
                        { _wsleep = 20; }
                    Var.WingsSleepParameter = _wsleep;
                    NativeMethods.MoveWindow(_window,
                        _newwindowmovement.Left,
                        _newwindowmovement.Top,
                        Var.WindowSizeX, Var.WindowSizeY, true);
                    Thread.Sleep(25);
                }
                //random moves
                if (Var.MovementDistance != 0 && !Var.FollowTheMouse)
                {
                    if (Var.SecondsSpentBeforeNextMovement > Var.SecondsToNextMovement)
                    {
                        //generate new rnd
                        Var.SecondsToNextMovement = Convert.ToInt32(_rnd.Next(Var.MOVEMENT_TIME_MODIFIER - Var.MovementFrequency, (Var.MOVEMENT_TIME_MODIFIER - Var.MovementFrequency) * 2));
                        //FIND THE TARGET
                        NativeMethods.GetWindowRect(_window, ref _windowcurrentpos);
                        do
                        {
                            _newwindowmovement.Top = Convert.ToInt32(_rnd.Next(-_screenheight / 2 * Var.MovementDistance, _screenheight / 2 * Var.MovementDistance));
                            _newwindowmovement.Left = Convert.ToInt32(_rnd.Next(-_screenwidth / 2 * Var.MovementDistance, _screenwidth / 2 * Var.MovementDistance));
                        } while
                            //Make sure it's inside the monitor
                        (
                            0 > _windowcurrentpos.Left + _newwindowmovement.Left ||
                            0 > _windowcurrentpos.Top + _newwindowmovement.Top ||
                            _screenwidth - Var.WindowSizeX < _windowcurrentpos.Left + _newwindowmovement.Left ||
                            _screenheight - Var.WindowSizeY < _windowcurrentpos.Top + _newwindowmovement.Top
                        );

                        //do the magic
                        int _sleeptime = Var.SleepDuringRandomMoves;
                        FlightMovement(_newwindowmovement, _windowcurrentpos, _sleeptime);

                        //WE DID IT!
                        Var.SecondsSpentBeforeNextMovement = 0;
                        //Turn around?
                        if (Var.TurnTowardsCenter)
                        {
                            if ((_windowcurrentpos.Left + _newwindowmovement.Left > _screenwidth / 2) && Var.LookingRightWay)
                                Var.TurnAroundState = 1;
                            else if ((_windowcurrentpos.Left + _newwindowmovement.Left < _screenwidth / 2) && !Var.LookingRightWay)
                                Var.TurnAroundState = 1;
                        }
                    }
                    //this isn't the end yet
                    Var.SecondsSpentBeforeNextMovement++;
                }
                Thread.Sleep(1000);
            }
        }
    }
}
