using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace DualSenseUnity

{
   public class DualSenseDebugger : MonoBehaviour
   {
      [ReadOnly] public uint ControllerCount = 0;
      public bool WriteDebugInfoToScreen = false;
      public uint ControllerIndexToDebug = 0;
      public List<ControllerInputState> ControllerInputs = new List<ControllerInputState>();
      public ControllerOutputState ControllerOutput;

      private List<DualSenseController> _controllers = new List<DualSenseController>();

      void Start()
      {
         ControllerCount = DualSense.GetControllerCount();
         DualSense.ControllerCountChanged += ControllerCountChanged;
         _controllers = DualSense.GetControllers();
      }

      private void ControllerCountChanged()
      {
         ControllerCount = DualSense.GetControllerCount();
         _controllers = DualSense.GetControllers();
      }

      void LateUpdate()
      {
         ControllerInputs.Clear();
         foreach ( var controller in _controllers )
         {
            var inputState = controller.GetInputState();
            ControllerInputs.Add( inputState );
         }

         var indexToDebug = GetCurrentValidControllerIndex();
         if ( indexToDebug < _controllers.Count )
         {
            _controllers[(int)indexToDebug].SetOutputState( ControllerOutput );
         }
      }

      void OnGUI()
      {
         if ( WriteDebugInfoToScreen )
         {
            var indexToDebug = GetCurrentValidControllerIndex();
            if ( indexToDebug < ControllerInputs.Count )
            {
               GUI.TextArea( new Rect( 10, 10, 350, 805 ), GetControllerDebugString( indexToDebug, ControllerInputs[(int)indexToDebug] ) );
            }
         }
      }

      private uint GetCurrentValidControllerIndex()
      {
         var indexToDebug = Math.Max( 0, ControllerIndexToDebug );
         indexToDebug = Math.Min( indexToDebug, ControllerCount - 1 );

         return indexToDebug;
      }

      private string GetControllerDebugString( uint controllerIndex, ControllerInputState inputState )
      {
         var sb = new StringBuilder();

         sb.AppendLine( $"Controller Count: {ControllerCount}" );
         sb.AppendLine();

         sb.AppendLine( $"******************** DualSense Controller {controllerIndex} ********************" );
         sb.AppendLine();
         sb.AppendLine( "--------------------------------------------------------------------------------------" );
         sb.AppendLine();

         if ( inputState.IsValid )
         {
            sb.AppendLine( $"Left Trigger: {inputState.LeftTrigger.TriggerValue}\t\tRight Trigger: {inputState.RightTrigger.TriggerValue}" );
            sb.AppendLine( $"As Button: {GetButtonString( inputState.LeftTrigger.ActiveState )}\tAs Button: {GetButtonString( inputState.RightTrigger.ActiveState )}" );
            sb.AppendLine();

            sb.AppendLine( $"Left Bumper: {GetButtonString( inputState.LeftBumper )}\tRight Bumper: {GetButtonString( inputState.RightBumper )}" );

            sb.AppendLine();
            sb.AppendLine( "--------------------------------------------------------------------------------------" );
            sb.AppendLine();

            sb.AppendLine( $"Create Button: {GetButtonString( inputState.CreateButton )}\tOptions Button: {GetButtonString( inputState.OptionsButton )}" );

            sb.AppendLine();
            sb.AppendLine( "--------------------------------------------------------------------------------------" );
            sb.AppendLine();

            sb.AppendLine( $"   Left Stick:\t\t   Right Stick:" );
            sb.AppendLine();
            sb.AppendLine( $"X: {GetNumberString( inputState.LeftStick.XAxis, 2, 2 )} Y: { GetNumberString( inputState.LeftStick.YAxis, 2, 2 )}\t\tX: {GetNumberString( inputState.RightStick.XAxis, 2, 2 )} Y: {GetNumberString( inputState.RightStick.YAxis, 2, 2 )}" );
            sb.AppendLine( $"As Button: {GetButtonString( inputState.LeftStick.PushButton )}\tAs Button: {GetButtonString( inputState.RightStick.PushButton )}" );

            sb.AppendLine();
            sb.AppendLine( "--------------------------------------------------------------------------------------" );
            sb.AppendLine();

            sb.Append( GetDpadAndFaceString( inputState ) );

            sb.AppendLine();
            sb.AppendLine( "--------------------------------------------------------------------------------------" );
            sb.AppendLine();

            sb.AppendLine( $"PS Button: {GetButtonString( inputState.PSButton )}\tMic Button: {GetButtonString( inputState.MicrophoneButton )}" );

            sb.AppendLine();
            sb.AppendLine( "--------------------------------------------------------------------------------------" );
            sb.AppendLine();

            sb.AppendLine( $"Touch Finger 1:\t\tTouch Finger 2:" );
            sb.AppendLine( $"X: {GetNumberString( inputState.TouchPad.TouchPoint1.X, 2, 2 )} Y: {GetNumberString( inputState.TouchPad.TouchPoint1.Y, 2, 2 )}\t\tX: {GetNumberString( inputState.TouchPad.TouchPoint2.X, 2, 2 )} Y: {GetNumberString( inputState.TouchPad.TouchPoint2.Y, 2, 2 )}" );
            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine( "--------------------------------------------------------------------------------------" );
            sb.AppendLine();

            sb.AppendLine( $"Accelerometer:\t\tGyroscope:" );
            sb.AppendLine( $"X: {GetNumberString( inputState.Accelerometer.XAxis, 0, 5 )}\t\t\tX: {GetNumberString( inputState.Gyroscope.XAxis, 0, 6 )}" );
            sb.AppendLine( $"Y: {GetNumberString( inputState.Accelerometer.YAxis, 0, 5 )}\t\t\tY: {GetNumberString( inputState.Gyroscope.YAxis, 0, 6 )}" );
            sb.AppendLine( $"Z: {GetNumberString( inputState.Accelerometer.ZAxis, 0, 5 )}\t\t\tZ: {GetNumberString( inputState.Gyroscope.ZAxis, 0, 6 )}" );
            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine( "--------------------------------------------------------------------------------------" );
            sb.AppendLine();

            //sb.AppendLine( $"Percent Charged: {inputState.Battery.PercentCharged}\tFully Charged: {inputState.Battery.FullyCharged}\tCharging: {inputState.Battery.Charging}" );
            //sb.AppendLine();
            sb.AppendLine( $"\tHeadphones Connected: {inputState.HeadPhonesConnected}" );
         }
         else
         {
            sb.AppendLine( "Count not retrieve controller input state" );
         }

         return sb.ToString();
      }
      private string GetButtonString( ButtonState state )
      {
         switch ( state )
         {
            case ButtonState.Down:
               {
                  return "DOWN      ";
               }
            case ButtonState.Up:
               {
                  return "UP           ";
               }
            case ButtonState.NewDown:
               {
                  return "NEW DOWN";
               }
            case ButtonState.NewUp:
               {
                  return "NEW UP   ";
               }
            default:
               {
                  return string.Empty;
               }
         }
      }

      private string GetDpadAndFaceString( ControllerInputState state )
      {
         var sb = new StringBuilder();
         sb.AppendLine( "      D-pad:\t\t\t   Face Buttons:" );
         sb.AppendLine();

         var uArrow = state.DPadUpButton == ButtonState.Down ? "↑" : " ";
         var dArrow = state.DPadDownButton == ButtonState.Down ? "↓" : " ";
         var lArrow = state.DPadLeftButton == ButtonState.Down ? "←" : "   ";
         var rArrow = state.DPadRightButton == ButtonState.Down ? "→" : "   ";

         var uFace = state.TriangleButton == ButtonState.Down ? "△" : " ";
         var dFace = state.CrossButton == ButtonState.Down ? "X" : " ";
         var lFace = state.SquareButton == ButtonState.Down ? "□" : "  ";
         var rFace = state.CircleButton == ButtonState.Down ? "◯" : " ";

         sb.AppendLine( $"          {uArrow}\t\t\t            {uFace}" );
         sb.AppendLine( $"     {lArrow}  +  {rArrow}                                       {lFace}  +  {rFace}" );
         sb.AppendLine( $"          {dArrow}\t\t\t            {dFace}" );

         return sb.ToString();
      }

      private static string GetNumberString( double num, int decimalPlaces, int fixedLength )
      {
         var decimalString = "{0:0";
         if ( decimalPlaces > 0 )
         {
            decimalString += ".";
         }
         for ( var i = 0; i < decimalPlaces; i++ )
         {
            decimalString += "0";
         }
         decimalString += "}";

         decimalString = string.Format( decimalString, num );

         if ( num >= 0 )
         {
            decimalString = " " + decimalString;
         }

         while ( (num > 0 && decimalString.Length < fixedLength - 1) || (decimalString.Length < fixedLength) )
         {
            decimalString += " ";
         }

         return decimalString;
      }
   }

    [CustomEditor( typeof( DualSenseDebugger ) )]

    [CanEditMultipleObjects]


    public class DualSenseDebuggerEditor : Editor
   {
      private SerializedProperty _controllerCount;
      private SerializedProperty _writeDebugInfoToScreen;
      private SerializedProperty _controllerIndexToDebug;
      private SerializedProperty _controllerInputs;
      private SerializedProperty _controllerOutput;

      void OnEnable()
      {
         _writeDebugInfoToScreen = serializedObject.FindProperty( "WriteDebugInfoToScreen" );
         _controllerCount = serializedObject.FindProperty( "ControllerCount" );
         _controllerIndexToDebug = serializedObject.FindProperty( "ControllerIndexToDebug" );
         _controllerInputs = serializedObject.FindProperty( "ControllerInputs" );
         _controllerOutput = serializedObject.FindProperty( "ControllerOutput" );
      }

      public override void OnInspectorGUI()
      {
         serializedObject.Update();

         EditorGUILayout.LabelField( "General" );
         using ( new EditorGUI.IndentLevelScope() )
         {
            if ( _controllerCount != null )
            {
               EditorGUILayout.PropertyField( _controllerCount );
            }
            if ( _writeDebugInfoToScreen != null )
            {
               EditorGUILayout.PropertyField( _writeDebugInfoToScreen );
            }
            if ( _controllerIndexToDebug != null )
            {
               EditorGUILayout.PropertyField( _controllerIndexToDebug );
            }
         }

         if ( _controllerInputs != null )
         {
            EditorGUILayout.LabelField( "Controller Input Readings" );
            using ( new EditorGUI.DisabledScope( true ) )
            {
               using ( new EditorGUI.IndentLevelScope() )
               {
                  using ( new EditorGUI.IndentLevelScope() )
                  {
                     EditorGUILayout.PropertyField( _controllerInputs );
                  }
               }
            }
         }

         if ( _controllerOutput != null )
         {
            EditorGUILayout.LabelField( "Controller Output Settings" );
            using ( new EditorGUI.IndentLevelScope() )
            {
               var leftTriggerEffect = _controllerOutput.FindPropertyRelative( "LeftTriggerEffect" );
               if ( leftTriggerEffect != null )
               {
                  EditorGUILayout.LabelField( "Left Trigger" );
                  using ( new EditorGUI.IndentLevelScope() )
                  {
                     ShowTriggerEffectProperty( leftTriggerEffect );
                  }
               }

               var rightTriggerEffect = _controllerOutput.FindPropertyRelative( "RightTriggerEffect" );
               if ( rightTriggerEffect != null )
               {
                  EditorGUILayout.LabelField( "Right Trigger" );
                  using ( new EditorGUI.IndentLevelScope() )
                  {
                     ShowTriggerEffectProperty( rightTriggerEffect );
                  }
               }

               var lightBarEnabled = _controllerOutput.FindPropertyRelative( "LightBarEnabled" );
               if ( lightBarEnabled != null )
               {
                  EditorGUILayout.PropertyField( lightBarEnabled, new GUIContent( "Light Bar Enabled" ) );
                  if ( lightBarEnabled.boolValue )
                  {
                     using ( new EditorGUI.IndentLevelScope() )
                     {
                        var lightBarIntensity = _controllerOutput.FindPropertyRelative( "LightBarIntensity" );
                        if ( lightBarIntensity != null )
                        {
                           EditorGUILayout.PropertyField( lightBarIntensity );
                        }
                        var lightBarR = _controllerOutput.FindPropertyRelative( "LightBarR" );
                        if ( lightBarR != null )
                        {
                           EditorGUILayout.PropertyField( lightBarR );
                        }
                        var lightBarG = _controllerOutput.FindPropertyRelative( "LightBarG" );
                        if ( lightBarG != null )
                        {
                           EditorGUILayout.PropertyField( lightBarG );
                        }
                        var lightBarB = _controllerOutput.FindPropertyRelative( "LightBarB" );
                        if ( lightBarB != null )
                        {
                           EditorGUILayout.PropertyField( lightBarB );
                        }
                     }
                  }
               }

               EditorGUILayout.LabelField( "Player Lights" );
               using ( new EditorGUI.IndentLevelScope() )
               {
                  var leftPlayerLightEnabled = _controllerOutput.FindPropertyRelative( "LeftPlayerLightEnabled" );
                  if ( leftPlayerLightEnabled != null )
                  {
                     EditorGUILayout.PropertyField( leftPlayerLightEnabled );
                  }
                  var middleLeftPlayerLightEnabled = _controllerOutput.FindPropertyRelative( "MiddleLeftPlayerLightEnabled" );
                  if ( middleLeftPlayerLightEnabled != null )
                  {
                     EditorGUILayout.PropertyField( middleLeftPlayerLightEnabled );
                  }
                  var middlePlayerLightEnabled = _controllerOutput.FindPropertyRelative( "MiddlePlayerLightEnabled" );
                  if ( middlePlayerLightEnabled != null )
                  {
                     EditorGUILayout.PropertyField( middlePlayerLightEnabled );
                  }
                  var middleRightPlayerLightEnabled = _controllerOutput.FindPropertyRelative( "MiddleRightPlayerLightEnabled" );
                  if ( middleRightPlayerLightEnabled != null )
                  {
                     EditorGUILayout.PropertyField( middleRightPlayerLightEnabled );
                  }
                  var rightPlayerLightEnabled = _controllerOutput.FindPropertyRelative( "RightPlayerLightEnabled" );
                  if ( rightPlayerLightEnabled != null )
                  {
                     EditorGUILayout.PropertyField( rightPlayerLightEnabled );
                  }
                  var fadePlayerLight = _controllerOutput.FindPropertyRelative( "FadePlayerLight" );
                  if ( fadePlayerLight != null )
                  {
                     EditorGUILayout.PropertyField( fadePlayerLight );
                  }
               }
            }
         }

         serializedObject.ApplyModifiedProperties();
      }

        private void ShowTriggerEffectProperty( SerializedProperty triggerEffect )
      {
         var effectType = triggerEffect.FindPropertyRelative( "EffectType" );
         if ( effectType != null && effectType.enumValueIndex >= 0 )
         {
            var showStartPos = false;
            var showEndPos = false;
            var showBeginForce = false;
            var showMiddleForce = false;
            var showEndForce = false;
            var showFreq = false;
            var showKeepEffect = false;

            var effectName = effectType.enumNames[effectType.enumValueIndex];
            switch ( effectName )
            {
               case "NoResistance":
                  {
                     break;
                  }
               case "ContinuousResistance":
                  {
                     showStartPos = showBeginForce = true;
                     break;
                  }
               case "SectionResistance":
                  {
                     showStartPos = showEndPos = showBeginForce = true;
                     break;
                  }
               case "EffectEx":
                  {
                     showStartPos = showBeginForce = showMiddleForce = showEndForce = showFreq = showKeepEffect = true;
                     break;
                  }
               default:
                  {
                     break;
                  }
            }

            EditorGUILayout.PropertyField( effectType );
            if ( showStartPos )
            {
               EditorGUILayout.PropertyField( triggerEffect.FindPropertyRelative( "StartPosition" ) );
            }
            if ( showEndPos )
            {
               EditorGUILayout.PropertyField( triggerEffect.FindPropertyRelative( "EndPosition" ) );
            }
            if ( showBeginForce )
            {
               EditorGUILayout.PropertyField( triggerEffect.FindPropertyRelative( "BeginForce" ) );
            }
            if ( showMiddleForce )
            {
               EditorGUILayout.PropertyField( triggerEffect.FindPropertyRelative( "MiddleForce" ) );
            }
            if ( showEndForce )
            {
               EditorGUILayout.PropertyField( triggerEffect.FindPropertyRelative( "EndForce" ) );
            }
            if ( showFreq )
            {
               EditorGUILayout.PropertyField( triggerEffect.FindPropertyRelative( "Frequency" ) );
            }
            if ( showKeepEffect )
            {
               EditorGUILayout.PropertyField( triggerEffect.FindPropertyRelative( "KeepEffect" ) );
            }
         }
      }
   }
}
#endif