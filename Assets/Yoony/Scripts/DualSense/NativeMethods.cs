using System.Runtime.InteropServices;

namespace DualSenseUnity
{
   public static class NativeMethods
   {
      [DllImport( "DualSenseWindowsNative" )]
      public static extern uint GetControllerCount();

      [DllImport( "DualSenseWindowsNative" )]
      public static extern ControllerInputState GetControllerInputState( uint controllerIndex );

      [DllImport( "DualSenseWindowsNative" )]
      public static extern bool SetControllerOutputState( uint controllerIndex, ControllerOutputState outputState );
   }
}