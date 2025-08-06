using DualSenseUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SampleDualSenseInputUIController : MonoBehaviour
{
   public DualSenseControllerProvider ControllerProvider;

   public Image DPadUpOverlay;
   public Image DPadDownOverlay;
   public Image DPadLeftOverlay;
   public Image DPadRightOverlay;

   public Image TriangleOverlay;
   public Image CrossOverlay;
   public Image SquareOverlay;
   public Image CircleOverlay;

   public Image CreateOverlay;
   public Image OptionsOverlay;
   public Image PSOverlay;
   public Image MicOverlay;

   public Image LeftStickButtonOverlay;
   public Image RightStickButtonOverlay;
   public Image LeftStickOverlay;
   public Image RightStickOverlay;

   public Image TouchPoint1Overlay;
   public Image TouchPoint2Overlay;

   public Image L1Overlay;
   public Image L2Overlay;
   public Image R1Overlay;
   public Image R2Overlay;

   private Queue<TouchPointState> _previousTouch1Positions = new Queue<TouchPointState>();
   private Queue<TouchPointState> _previousTouch2Positions = new Queue<TouchPointState>();

   void Start()
   {
   }

   void Update()
   {
      if ( ControllerProvider.ControllerIndexToUse < ControllerProvider.DualSenseControllers.Count )
      {
         var controllerToUse = ControllerProvider.DualSenseControllers[(int)ControllerProvider.ControllerIndexToUse];
         var inputState = controllerToUse.GetInputState();

         DPadUpOverlay.enabled = ButtonPressed( inputState.DPadUpButton );
         DPadDownOverlay.enabled = ButtonPressed( inputState.DPadDownButton );
         DPadLeftOverlay.enabled = ButtonPressed( inputState.DPadLeftButton );
         DPadRightOverlay.enabled = ButtonPressed( inputState.DPadRightButton );

         TriangleOverlay.enabled = ButtonPressed( inputState.TriangleButton );
         CrossOverlay.enabled = ButtonPressed( inputState.CrossButton );
         SquareOverlay.enabled = ButtonPressed( inputState.SquareButton );
         CircleOverlay.enabled = ButtonPressed( inputState.CircleButton );

         CreateOverlay.enabled = ButtonPressed( inputState.CreateButton );
         OptionsOverlay.enabled = ButtonPressed( inputState.OptionsButton );
         PSOverlay.enabled = ButtonPressed( inputState.PSButton );
         MicOverlay.enabled = ButtonPressed( inputState.MicrophoneButton );

         var stickOverlayScale = 30;
         var stickDeadZone = .05;
         LeftStickButtonOverlay.enabled = ButtonPressed( inputState.LeftStick.PushButton );
         RightStickButtonOverlay.enabled = ButtonPressed( inputState.RightStick.PushButton );
         LeftStickOverlay.enabled = Math.Abs( inputState.LeftStick.XAxis ) >= stickDeadZone || Math.Abs( inputState.LeftStick.YAxis ) >= stickDeadZone;
         RightStickOverlay.enabled = Math.Abs( inputState.RightStick.XAxis ) >= stickDeadZone || Math.Abs( inputState.RightStick.YAxis ) >= stickDeadZone;
         LeftStickOverlay.transform.position = LeftStickButtonOverlay.transform.position + new Vector3( Convert.ToSingle( inputState.LeftStick.XAxis ) * stickOverlayScale, Convert.ToSingle( inputState.LeftStick.YAxis ) * stickOverlayScale, 0 );
         RightStickOverlay.transform.position = RightStickButtonOverlay.transform.position + new Vector3( Convert.ToSingle( inputState.RightStick.XAxis ) * stickOverlayScale, Convert.ToSingle( inputState.RightStick.YAxis ) * stickOverlayScale, 0 );

         var touchPointOverlayScaleX = 153;
         var touchPointOverlayScaleY = 74;
         TouchPoint1Overlay.transform.localPosition = new Vector3( Convert.ToSingle( inputState.TouchPad.TouchPoint1.X ) * touchPointOverlayScaleX, Convert.ToSingle( inputState.TouchPad.TouchPoint1.Y ) * -1 * touchPointOverlayScaleY, 0 );
         TouchPoint2Overlay.transform.localPosition = new Vector3( Convert.ToSingle( inputState.TouchPad.TouchPoint2.X ) * touchPointOverlayScaleX, Convert.ToSingle( inputState.TouchPad.TouchPoint2.Y ) * -1 * touchPointOverlayScaleY, 0 );
         _previousTouch1Positions.Enqueue( inputState.TouchPad.TouchPoint1 );
         _previousTouch2Positions.Enqueue( inputState.TouchPad.TouchPoint2 );

         var touchStateIdleCount = 25;
         if(_previousTouch1Positions.Count > touchStateIdleCount )
         {
            _previousTouch1Positions.Dequeue();

            var recentTouch = inputState.TouchPad.TouchPoint1;
            TouchPoint1Overlay.enabled =  _previousTouch1Positions.Any( touch => touch.X != recentTouch.X || touch.Y != recentTouch.Y );
         }
         else
         {
            TouchPoint1Overlay.enabled = false;
         }
         if ( _previousTouch2Positions.Count > touchStateIdleCount )
         {
            _previousTouch2Positions.Dequeue();

            var recentTouch = inputState.TouchPad.TouchPoint2;
            TouchPoint2Overlay.enabled = _previousTouch2Positions.Any( touch => touch.X != recentTouch.X || touch.Y != recentTouch.Y );
         }
         else
         {
            TouchPoint2Overlay.enabled = false;
         }


         L1Overlay.enabled = ButtonPressed( inputState.LeftBumper );
         L2Overlay.enabled = ButtonPressed( inputState.LeftTrigger.ActiveState );
         R1Overlay.enabled = ButtonPressed( inputState.RightBumper );
         R2Overlay.enabled = ButtonPressed( inputState.RightTrigger.ActiveState );
      }
   }

   private bool ButtonPressed( ButtonState buttonState )
   {
      return buttonState == ButtonState.Down || buttonState == ButtonState.NewDown;
   }
}