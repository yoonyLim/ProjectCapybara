using System;
using System.Collections.Generic;
using System.Timers;

namespace DualSenseUnity
{
   public static class DualSense
   {
      public static event Action ControllerCountChanged;

      private static bool _initialized;
      private static uint _controllerCount;
      private static Timer _timer;

      private static void Init()
      {
         if ( !_initialized )
         {
            _controllerCount = GetControllerCount();

            _timer = new Timer( 1000 );
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();
         }
      }

      private static void TimerElapsed( object sender, ElapsedEventArgs e )
      {
         var newControllerCount = GetControllerCount();
         if ( _controllerCount != newControllerCount )
         {
            _controllerCount = newControllerCount;
            ControllerCountChanged.Invoke();
         }
      }

      public static uint GetControllerCount()
      {
         return NativeMethods.GetControllerCount();
      }

      public static List<DualSenseController> GetControllers()
      {
         Init();

         var controllers = new List<DualSenseController>();
         var controllerCount = GetControllerCount();
         for ( uint i = 0; i < controllerCount; i++ )
         {
            controllers.Add( new DualSenseController( i ) );
         }

         return controllers;
      }
   }
}
