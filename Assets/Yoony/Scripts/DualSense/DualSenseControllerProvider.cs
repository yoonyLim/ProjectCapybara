using DualSenseUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DualSenseControllerProvider : MonoBehaviour
{
   public Text ControllerCountText;
   public InputField ControllerIndexToUseInput;

   [HideInInspector]
   public uint ControllerCount = 0;
   [HideInInspector]
   public uint ControllerIndexToUse = 0;
   public List<DualSenseController> DualSenseControllers = new List<DualSenseController>();

   void Start()
   {
      ControllerCountText.text = ControllerCount.ToString();
      ControllerIndexToUseInput.text = ControllerIndexToUse.ToString();

      DualSense.ControllerCountChanged += RefreshControllers;
      RefreshControllers();
   }

   void Update()
   {
      ControllerCountText.text = $"Controller Count: {ControllerCount}";

      ParseControllerIndexIfPossible( ControllerIndexToUseInput.text, ControllerIndexToUse, out ControllerIndexToUse );
   }

   public void DoneEditingControllerIndex( string newValue )
   {
      ControllerIndexToUseInput.text = ControllerIndexToUse.ToString();
   }

   private void RefreshControllers()
   {
      ControllerCount = DualSense.GetControllerCount();
      DualSenseControllers = DualSense.GetControllers();
   }

   private uint GetCurrentValidControllerIndex()
   {
      var indexToDebug = Math.Max( 0, ControllerIndexToUse );
      indexToDebug = Math.Min( ControllerIndexToUse, ControllerCount - 1 );

      return indexToDebug;
   }

   private void ParseControllerIndexIfPossible( string strToParse, double fallbackValue, out uint valueToSet )
   {
      if ( !uint.TryParse( strToParse, out valueToSet ) )
      {
         valueToSet = 0;
      }
      ControllerIndexToUse = GetCurrentValidControllerIndex();
   }
}