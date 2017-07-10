using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Examples.SpatialUnderstandingFeatureOverview;
using HoloToolkit.Unity;
using UnityEngine.VR.WSA.Input;
using System;

public class DebuggerTest :  MonoBehaviour{


    #region Unity回调

    void Start()
    {
        SpatialUnderstanding.Instance.ScanStateChanged += OnScanStateChanged;
        InteractionManager.SourcePressed += OnAirTap;
    }

    void Update()
    {

    }
    #endregion

    #region 事件
    private void OnAirTap(InteractionSourceState state)
    {
        Debug.Log("TH   NO.1" + state.ToString());
    }


    #region scanstate
    /*public enum ScanStates
     {
         None,
         ReadyToScan,
         Scanning,
         Finishing,
         Done
     */
    #endregion

    private void OnScanStateChanged()
    {
        if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done) &&
                SpatialUnderstanding.Instance.AllowSpatialUnderstanding)

            Debug.Log(" OnScanStateChanged     to-------------->" + SpatialUnderstanding.Instance.ScanState);
    }



    #endregion


}
