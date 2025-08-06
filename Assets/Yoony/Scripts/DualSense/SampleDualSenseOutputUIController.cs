using DualSenseUnity;
using UnityEngine;
using UnityEngine.UI;

public class SampleDualSenseOutputUIController : MonoBehaviour
{
   public DualSenseControllerProvider ControllerProvider;

   public Dropdown LeftTriggerEffectType;
   public InputField LeftTriggerStartPos;
   public InputField LeftTriggerEndPos;
   public InputField LeftTriggerBeginForce;
   public InputField LeftTriggerMiddleForce;
   public InputField LeftTriggerEndForce;
   public InputField LeftTriggerFrequency;
   public Toggle LeftTriggerKeepEffect;

   public Dropdown RightTriggerEffectType;
   public InputField RightTriggerStartPos;
   public InputField RightTriggerEndPos;
   public InputField RightTriggerBeginForce;
   public InputField RightTriggerMiddleForce;
   public InputField RightTriggerEndForce;
   public InputField RightTriggerFrequency;
   public Toggle RightTriggerKeepEffect;

   public InputField LeftRumble;
   public InputField RightRumble;

   public Toggle LightBarEnabled;
   public InputField LightBarIntensity;
   public InputField LightBarRed;
   public InputField LightBarGreen;
   public InputField LightBarBlue;

   public Toggle PlayerLightsLeftEnabled;
   public Toggle PlayerLightsMiddleLeftEnabled;
   public Toggle PlayerLightsMiddleEnabled;
   public Toggle PlayerLightsMiddleRightEnabled;
   public Toggle PlayerLightsRightEnabled;
   public Toggle PlayerLightsFade;

   public Text LeftTriggerStartPosLabel;
   public Text LeftTriggerEndPosLabel;
   public Text LeftTriggerBeginForceLabel;
   public Text LeftTriggerMiddleForceLabel;
   public Text LeftTriggerEndForceLabel;
   public Text LeftTriggerFrequencyLabel;
   public Text LeftTriggerKeepEffectLabel;

   public Text RightTriggerStartPosLabel;
   public Text RightTriggerEndPosLabel;
   public Text RightTriggerBeginForceLabel;
   public Text RightTriggerMiddleForceLabel;
   public Text RightTriggerEndForceLabel;
   public Text RightTriggerFrequencyLabel;
   public Text RightTriggerKeepEffectLabel;

   private ControllerOutputState _previousOutput = new ControllerOutputState();

   private Color EnabledTextColor = Color.black;
   private Color DisabledTextColor = new Color( 0, 0, 0, .5f );

   public void Start()
   {
      _previousOutput.LeftTriggerEffect.StartPosition = 0;
      _previousOutput.LeftTriggerEffect.EndPosition = 1;
      _previousOutput.LeftTriggerEffect.BeginForce = .7;
      _previousOutput.LeftTriggerEffect.MiddleForce = .7;
      _previousOutput.LeftTriggerEffect.EndForce = .7;
      _previousOutput.LeftTriggerEffect.Frequency = .2;
      _previousOutput.LeftTriggerEffect.KeepEffect = true;
      LeftTriggerStartPos.text = "0";
      LeftTriggerEndPos.text = "1";
      LeftTriggerBeginForce.text = "0.7";
      LeftTriggerMiddleForce.text = "0.7";
      LeftTriggerEndForce.text = "0.7";
      LeftTriggerFrequency.text = "0.2";
      LeftTriggerKeepEffect.isOn = true;

      _previousOutput.RightTriggerEffect.StartPosition = 0;
      _previousOutput.RightTriggerEffect.EndPosition = 1;
      _previousOutput.RightTriggerEffect.BeginForce = .7;
      _previousOutput.RightTriggerEffect.MiddleForce = .7;
      _previousOutput.RightTriggerEffect.EndForce = .7;
      _previousOutput.RightTriggerEffect.Frequency = .2;
      _previousOutput.RightTriggerEffect.KeepEffect = true;
      RightTriggerStartPos.text = "0";
      RightTriggerEndPos.text = "1";
      RightTriggerBeginForce.text = "0.7";
      RightTriggerMiddleForce.text = "0.7";
      RightTriggerEndForce.text = "0.7";
      RightTriggerFrequency.text = "0.2";
      RightTriggerKeepEffect.isOn = true;

      _previousOutput.LeftRumbleIntensity = 0;
      _previousOutput.RightRumbleIntensity = 0;

      _previousOutput.LightBarEnabled = false;
      _previousOutput.LightBarIntensity = 1;
      _previousOutput.LightBarR = 1;
      _previousOutput.LightBarG = 0;
      _previousOutput.LightBarB = 1;
      LightBarEnabled.isOn = false;
      LightBarIntensity.text = "1";
      LightBarRed.text = "1";
      LightBarGreen.text = "0";
      LightBarBlue.text = "1";

      _previousOutput.LeftPlayerLightEnabled = false;
      _previousOutput.MiddleLeftPlayerLightEnabled = false;
      _previousOutput.MiddlePlayerLightEnabled = false;
      _previousOutput.MiddleRightPlayerLightEnabled = false;
      _previousOutput.RightPlayerLightEnabled = false;
      _previousOutput.FadePlayerLight = false;
      PlayerLightsLeftEnabled.isOn = false;
      PlayerLightsMiddleLeftEnabled.isOn = false;
      PlayerLightsMiddleEnabled.isOn = false;
      PlayerLightsMiddleRightEnabled.isOn = false;
      PlayerLightsRightEnabled.isOn = false;
      PlayerLightsFade.isOn = false;
   }
   public void Update()
   {
      switch ( LeftTriggerEffectType.options[LeftTriggerEffectType.value].text )
      {
         case "No Resistance":
         {
            SetLeftTriggerEffect( ref _previousOutput, TriggerEffectType.NoResistance );
            break;
         }
         case "Continuous Resistance":
         {
            SetLeftTriggerEffect( ref _previousOutput, TriggerEffectType.ContinuousResistance );
            break;
         }
         case "Section Resistance":
         {
            SetLeftTriggerEffect( ref _previousOutput, TriggerEffectType.SectionResistance );
            break;
         }
         case "Effect Resistance":
         {
            SetLeftTriggerEffect( ref _previousOutput, TriggerEffectType.EffectEx );
            break;
         }
         default:
         {
            break;
         }
      }

      switch ( RightTriggerEffectType.options[RightTriggerEffectType.value].text )
      {
         case "No Resistance":
         {
            SetRightTriggerEffect( ref _previousOutput, TriggerEffectType.NoResistance );
            break;
         }
         case "Continuous Resistance":
         {
            SetRightTriggerEffect( ref _previousOutput, TriggerEffectType.ContinuousResistance );
            break;
         }
         case "Section Resistance":
         {
            SetRightTriggerEffect( ref _previousOutput, TriggerEffectType.SectionResistance );
            break;
         }
         case "Effect Resistance":
         {
            SetRightTriggerEffect( ref _previousOutput, TriggerEffectType.EffectEx );
            break;
         }
         default:
         {
            break;
         }
      }

      ParseIfPossible( LeftRumble.text, _previousOutput.LeftRumbleIntensity, out _previousOutput.LeftRumbleIntensity );
      ParseIfPossible( RightRumble.text, _previousOutput.RightRumbleIntensity, out _previousOutput.RightRumbleIntensity );

      _previousOutput.LightBarEnabled = LightBarEnabled.isOn;
      ParseIfPossible( LightBarIntensity.text, _previousOutput.LightBarIntensity, out _previousOutput.LightBarIntensity );
      ParseIfPossible( LightBarRed.text, _previousOutput.LightBarR, out _previousOutput.LightBarR );
      ParseIfPossible( LightBarGreen.text, _previousOutput.LightBarG, out _previousOutput.LightBarG );
      ParseIfPossible( LightBarBlue.text, _previousOutput.LightBarB, out _previousOutput.LightBarB );

      _previousOutput.LeftPlayerLightEnabled = PlayerLightsLeftEnabled.isOn;
      _previousOutput.MiddleLeftPlayerLightEnabled = PlayerLightsMiddleLeftEnabled.isOn;
      _previousOutput.MiddlePlayerLightEnabled = PlayerLightsMiddleEnabled.isOn;
      _previousOutput.MiddleRightPlayerLightEnabled = PlayerLightsMiddleRightEnabled.isOn;
      _previousOutput.RightPlayerLightEnabled = PlayerLightsRightEnabled.isOn;
      _previousOutput.FadePlayerLight = PlayerLightsFade.isOn;

      if ( ControllerProvider.ControllerIndexToUse < ControllerProvider.DualSenseControllers.Count )
      {
         var controllerToUse = ControllerProvider.DualSenseControllers[(int)ControllerProvider.ControllerIndexToUse];
         controllerToUse.SetOutputState( _previousOutput );
      }
   }

   // Make sure text fields are set to something valid when an edit is finished
   public void DoneEditingSomething( string newValue )
   {
      LeftTriggerStartPos.text = _previousOutput.LeftTriggerEffect.StartPosition.ToString();
      LeftTriggerEndPos.text = _previousOutput.LeftTriggerEffect.EndPosition.ToString();
      LeftTriggerBeginForce.text = _previousOutput.LeftTriggerEffect.BeginForce.ToString();
      LeftTriggerMiddleForce.text = _previousOutput.LeftTriggerEffect.MiddleForce.ToString();
      LeftTriggerEndForce.text = _previousOutput.LeftTriggerEffect.EndForce.ToString();
      LeftTriggerFrequency.text = _previousOutput.LeftTriggerEffect.Frequency.ToString();

      RightTriggerStartPos.text = _previousOutput.RightTriggerEffect.StartPosition.ToString();
      RightTriggerEndPos.text = _previousOutput.RightTriggerEffect.EndPosition.ToString();
      RightTriggerBeginForce.text = _previousOutput.RightTriggerEffect.BeginForce.ToString();
      RightTriggerMiddleForce.text = _previousOutput.RightTriggerEffect.MiddleForce.ToString();
      RightTriggerEndForce.text = _previousOutput.RightTriggerEffect.EndForce.ToString();
      RightTriggerFrequency.text = _previousOutput.RightTriggerEffect.Frequency.ToString();

      LeftRumble.text = _previousOutput.LeftRumbleIntensity.ToString();
      RightRumble.text = _previousOutput.RightRumbleIntensity.ToString();

      LightBarIntensity.text = _previousOutput.LightBarIntensity.ToString();
      LightBarRed.text = _previousOutput.LightBarR.ToString();
      LightBarGreen.text = _previousOutput.LightBarG.ToString();
      LightBarBlue.text = _previousOutput.LightBarB.ToString();
   }

   private void SetLeftTriggerEffect( ref ControllerOutputState output, TriggerEffectType effectType )
   {
      switch ( effectType )
      {
         case TriggerEffectType.NoResistance:
         {
            LeftTriggerStartPos.interactable = false;
            LeftTriggerEndPos.interactable = false;
            LeftTriggerBeginForce.interactable = false;
            LeftTriggerMiddleForce.interactable = false;
            LeftTriggerEndForce.interactable = false;
            LeftTriggerFrequency.interactable = false;
            LeftTriggerKeepEffect.interactable = false;

            LeftTriggerStartPosLabel.color = DisabledTextColor;
            LeftTriggerEndPosLabel.color = DisabledTextColor;
            LeftTriggerBeginForceLabel.color = DisabledTextColor;
            LeftTriggerMiddleForceLabel.color = DisabledTextColor;
            LeftTriggerEndForceLabel.color = DisabledTextColor;
            LeftTriggerFrequencyLabel.color = DisabledTextColor;
            LeftTriggerKeepEffectLabel.color = DisabledTextColor;

            output.LeftTriggerEffect.InitializeNoResistanceEffect();
            break;
         }
         case TriggerEffectType.ContinuousResistance:
         {
            LeftTriggerStartPos.interactable = true;
            LeftTriggerEndPos.interactable = false;
            LeftTriggerBeginForce.interactable = true;
            LeftTriggerMiddleForce.interactable = false;
            LeftTriggerEndForce.interactable = false;
            LeftTriggerFrequency.interactable = false;
            LeftTriggerKeepEffect.interactable = false;

            LeftTriggerStartPosLabel.color = EnabledTextColor;
            LeftTriggerEndPosLabel.color = DisabledTextColor;
            LeftTriggerBeginForceLabel.color = EnabledTextColor;
            LeftTriggerMiddleForceLabel.color = DisabledTextColor;
            LeftTriggerEndForceLabel.color = DisabledTextColor;
            LeftTriggerFrequencyLabel.color = DisabledTextColor;
            LeftTriggerKeepEffectLabel.color = DisabledTextColor;

            double startPos, beginForce;
            ParseIfPossible( LeftTriggerStartPos.text, _previousOutput.LeftTriggerEffect.StartPosition, out startPos );
            ParseIfPossible( LeftTriggerBeginForce.text, _previousOutput.LeftTriggerEffect.BeginForce, out beginForce );

            output.LeftTriggerEffect.InitializeContinuousResistanceEffect( startPos, beginForce );

            break;
         }
         case TriggerEffectType.SectionResistance:
         {
            LeftTriggerStartPos.interactable = true;
            LeftTriggerEndPos.interactable = true;
            LeftTriggerBeginForce.interactable = true;
            LeftTriggerMiddleForce.interactable = false;
            LeftTriggerEndForce.interactable = false;
            LeftTriggerFrequency.interactable = false;
            LeftTriggerKeepEffect.interactable = false;

            LeftTriggerStartPosLabel.color = EnabledTextColor;
            LeftTriggerEndPosLabel.color = EnabledTextColor;
            LeftTriggerBeginForceLabel.color = EnabledTextColor;
            LeftTriggerMiddleForceLabel.color = DisabledTextColor;
            LeftTriggerEndForceLabel.color = DisabledTextColor;
            LeftTriggerFrequencyLabel.color = DisabledTextColor;
            LeftTriggerKeepEffectLabel.color = DisabledTextColor;

            double startPos, endPos, beginForce;
            ParseIfPossible( LeftTriggerStartPos.text, _previousOutput.LeftTriggerEffect.StartPosition, out startPos );
            ParseIfPossible( LeftTriggerEndPos.text, _previousOutput.LeftTriggerEffect.EndPosition, out endPos );
            ParseIfPossible( LeftTriggerBeginForce.text, _previousOutput.LeftTriggerEffect.BeginForce, out beginForce );

            output.LeftTriggerEffect.InitializeSectionResistanceEffect( startPos, endPos, beginForce );

            break;
         }
         case TriggerEffectType.EffectEx:
         {
            LeftTriggerStartPos.interactable = true;
            LeftTriggerEndPos.interactable = false;
            LeftTriggerBeginForce.interactable = true;
            LeftTriggerMiddleForce.interactable = true;
            LeftTriggerEndForce.interactable = true;
            LeftTriggerFrequency.interactable = true;
            LeftTriggerKeepEffect.interactable = true;

            LeftTriggerStartPosLabel.color = EnabledTextColor;
            LeftTriggerEndPosLabel.color = DisabledTextColor;
            LeftTriggerBeginForceLabel.color = EnabledTextColor;
            LeftTriggerMiddleForceLabel.color = EnabledTextColor;
            LeftTriggerEndForceLabel.color = EnabledTextColor;
            LeftTriggerFrequencyLabel.color = EnabledTextColor;
            LeftTriggerKeepEffectLabel.color = EnabledTextColor;

            double startPos, endPos, beginForce, middleForce, endForce, frequency;
            ParseIfPossible( LeftTriggerStartPos.text, _previousOutput.LeftTriggerEffect.StartPosition, out startPos );
            ParseIfPossible( LeftTriggerEndPos.text, _previousOutput.LeftTriggerEffect.EndPosition, out endPos );
            ParseIfPossible( LeftTriggerBeginForce.text, _previousOutput.LeftTriggerEffect.BeginForce, out beginForce );
            ParseIfPossible( LeftTriggerMiddleForce.text, _previousOutput.LeftTriggerEffect.MiddleForce, out middleForce );
            ParseIfPossible( LeftTriggerEndForce.text, _previousOutput.LeftTriggerEffect.EndForce, out endForce );
            ParseIfPossible( LeftTriggerFrequency.text, _previousOutput.LeftTriggerEffect.Frequency, out frequency );

            output.LeftTriggerEffect.InitializeExtendedEffect( startPos, beginForce, middleForce, endForce, frequency, LeftTriggerKeepEffect.isOn );

            break;
         }
         default:
         {
            break;
         }
      }
   }

   private void SetRightTriggerEffect( ref ControllerOutputState output, TriggerEffectType effectType )
   {
      switch ( effectType )
      {
         case TriggerEffectType.NoResistance:
         {
            RightTriggerStartPos.interactable = false;
            RightTriggerEndPos.interactable = false;
            RightTriggerBeginForce.interactable = false;
            RightTriggerMiddleForce.interactable = false;
            RightTriggerEndForce.interactable = false;
            RightTriggerFrequency.interactable = false;
            RightTriggerKeepEffect.interactable = false;

            RightTriggerStartPosLabel.color = DisabledTextColor;
            RightTriggerEndPosLabel.color = DisabledTextColor;
            RightTriggerBeginForceLabel.color = DisabledTextColor;
            RightTriggerMiddleForceLabel.color = DisabledTextColor;
            RightTriggerEndForceLabel.color = DisabledTextColor;
            RightTriggerFrequencyLabel.color = DisabledTextColor;
            RightTriggerKeepEffectLabel.color = DisabledTextColor;

            output.RightTriggerEffect.InitializeNoResistanceEffect();
            break;
         }
         case TriggerEffectType.ContinuousResistance:
         {
            RightTriggerStartPos.interactable = true;
            RightTriggerEndPos.interactable = false;
            RightTriggerBeginForce.interactable = true;
            RightTriggerMiddleForce.interactable = false;
            RightTriggerEndForce.interactable = false;
            RightTriggerFrequency.interactable = false;
            RightTriggerKeepEffect.interactable = false;

            RightTriggerStartPosLabel.color = EnabledTextColor;
            RightTriggerEndPosLabel.color = DisabledTextColor;
            RightTriggerBeginForceLabel.color = EnabledTextColor;
            RightTriggerMiddleForceLabel.color = DisabledTextColor;
            RightTriggerEndForceLabel.color = DisabledTextColor;
            RightTriggerFrequencyLabel.color = DisabledTextColor;
            RightTriggerKeepEffectLabel.color = DisabledTextColor;

            double startPos, beginForce;
            ParseIfPossible( RightTriggerStartPos.text, _previousOutput.RightTriggerEffect.StartPosition, out startPos );
            ParseIfPossible( RightTriggerBeginForce.text, _previousOutput.RightTriggerEffect.BeginForce, out beginForce );

            output.RightTriggerEffect.InitializeContinuousResistanceEffect( startPos, beginForce );

            break;
         }
         case TriggerEffectType.SectionResistance:
         {
            RightTriggerStartPos.interactable = true;
            RightTriggerEndPos.interactable = true;
            RightTriggerBeginForce.interactable = true;
            RightTriggerMiddleForce.interactable = false;
            RightTriggerEndForce.interactable = false;
            RightTriggerFrequency.interactable = false;
            RightTriggerKeepEffect.interactable = false;

            RightTriggerStartPosLabel.color = EnabledTextColor;
            RightTriggerEndPosLabel.color = EnabledTextColor;
            RightTriggerBeginForceLabel.color = EnabledTextColor;
            RightTriggerMiddleForceLabel.color = DisabledTextColor;
            RightTriggerEndForceLabel.color = DisabledTextColor;
            RightTriggerFrequencyLabel.color = DisabledTextColor;
            RightTriggerKeepEffectLabel.color = DisabledTextColor;

            double startPos, endPos, beginForce;
            ParseIfPossible( RightTriggerStartPos.text, _previousOutput.RightTriggerEffect.StartPosition, out startPos );
            ParseIfPossible( RightTriggerEndPos.text, _previousOutput.RightTriggerEffect.EndPosition, out endPos );
            ParseIfPossible( RightTriggerBeginForce.text, _previousOutput.RightTriggerEffect.BeginForce, out beginForce );

            output.RightTriggerEffect.InitializeSectionResistanceEffect( startPos, endPos, beginForce );

            break;
         }
         case TriggerEffectType.EffectEx:
         {
            RightTriggerStartPos.interactable = true;
            RightTriggerEndPos.interactable = false;
            RightTriggerBeginForce.interactable = true;
            RightTriggerMiddleForce.interactable = true;
            RightTriggerEndForce.interactable = true;
            RightTriggerFrequency.interactable = true;
            RightTriggerKeepEffect.interactable = true;

            RightTriggerStartPosLabel.color = EnabledTextColor;
            RightTriggerEndPosLabel.color = DisabledTextColor;
            RightTriggerBeginForceLabel.color = EnabledTextColor;
            RightTriggerMiddleForceLabel.color = EnabledTextColor;
            RightTriggerEndForceLabel.color = EnabledTextColor;
            RightTriggerFrequencyLabel.color = EnabledTextColor;
            RightTriggerKeepEffectLabel.color = EnabledTextColor;

            double startPos, endPos, beginForce, middleForce, endForce, frequency;
            ParseIfPossible( RightTriggerStartPos.text, _previousOutput.RightTriggerEffect.StartPosition, out startPos );
            ParseIfPossible( RightTriggerEndPos.text, _previousOutput.RightTriggerEffect.EndPosition, out endPos );
            ParseIfPossible( RightTriggerBeginForce.text, _previousOutput.RightTriggerEffect.BeginForce, out beginForce );
            ParseIfPossible( RightTriggerMiddleForce.text, _previousOutput.RightTriggerEffect.MiddleForce, out middleForce );
            ParseIfPossible( RightTriggerEndForce.text, _previousOutput.RightTriggerEffect.EndForce, out endForce );
            ParseIfPossible( RightTriggerFrequency.text, _previousOutput.RightTriggerEffect.Frequency, out frequency );

            output.RightTriggerEffect.InitializeExtendedEffect( startPos, beginForce, middleForce, endForce, frequency, RightTriggerKeepEffect.isOn );

            break;
         }
         default:
         {
            break;
         }
      }
   }

   private void ParseIfPossible( string strToParse, double fallbackValue, out double valueToSet )
   {
      if ( !double.TryParse( strToParse, out valueToSet ) )
      {
         valueToSet = fallbackValue;
      }
   }
}
