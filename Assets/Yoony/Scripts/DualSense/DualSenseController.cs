using System;

namespace DualSenseUnity
{
   public class DualSenseController
   {
      private uint _controllerIndex;

      public DualSenseController( uint controllerIndex )
      {
         _controllerIndex = controllerIndex;
      }

      public ControllerInputState GetInputState()
      {
         var inputState = NativeMethods.GetControllerInputState( _controllerIndex );
         inputState.LeftTrigger.TriggerValue = Math.Round( inputState.LeftTrigger.TriggerValue, 2 );
         inputState.RightTrigger.TriggerValue = Math.Round( inputState.RightTrigger.TriggerValue, 2 );

         return inputState;
      }

      public bool SetOutputState( ControllerOutputState outputState )
      {
         return NativeMethods.SetControllerOutputState( _controllerIndex, outputState );
      }
   }
}
