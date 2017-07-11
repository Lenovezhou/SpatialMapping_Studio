using HoloToolkit.Examples.SpatialUnderstandingFeatureOverview;
using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UItype
{
    Decorate,furniture,Decoration
}


public class ModelsUI : LineDrawer
{
    #region 字段
    private bool placedMenuNeedsBillboard = false;

    public bool HasPlacedMenu { get; private set; }
    public AnimatedBox MenuAnimatedBox { get; private set; }


    public const float MenuWidth = 1.5f;
    public const float MenuHeight = 1.0f;
    public const float MenuMinDepth = 2.0f;

    //ui的父节点
    public Transform ParentPanel;

    //摆放Resource出来的模型的地方
    public Transform GridPanel;

    #endregion

    #region Unity回调
    private void Start()
    {
        InitializedEdge();
        // Turn menu off until we're placed
        ParentPanel.gameObject.SetActive(false);

        // Events
        SpatialUnderstanding.Instance.ScanStateChanged += OnScanStateChanged;
#if UNITY_EDITOR || UNITY_WSA
        //  InteractionManager.SourcePressed += OnAirTap;
#endif
    }


    private void Update()
    {
    //    Update_Colors();

        // Animated box
        if (MenuAnimatedBox != null)
        {
            // We're using the animated box for the animation only
            MenuAnimatedBox.Update(Time.deltaTime);

            // Billboarding
            if (MenuAnimatedBox.IsAnimationComplete &&
                placedMenuNeedsBillboard)
            {
                // Rotate to face the user
                transform.position = MenuAnimatedBox.AnimPosition.Evaluate(MenuAnimatedBox.Time);
                Vector3 lookDirTarget = Camera.main.transform.position - transform.position;
                lookDirTarget = (new Vector3(lookDirTarget.x, 0.0f, lookDirTarget.z)).normalized;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-lookDirTarget), Time.deltaTime * 10.0f);
            }
            else
            {
                // Keep the UI locked to the animated box
                transform.position = MenuAnimatedBox.AnimPosition.Evaluate(MenuAnimatedBox.Time);
                transform.rotation = MenuAnimatedBox.Rotation * Quaternion.AngleAxis(360.0f * MenuAnimatedBox.AnimRotation.Evaluate(MenuAnimatedBox.Time), Vector3.up);
            }
        }
    }


    protected override void OnDestroy()
    {
        if (SpatialUnderstanding.Instance != null)
        {
            SpatialUnderstanding.Instance.ScanStateChanged -= OnScanStateChanged;
        }
#if UNITY_EDITOR || UNITY_WSA
       // InteractionManager.SourcePressed -= OnAirTap;
#endif

        base.OnDestroy();
    }


    #endregion

    #region 事件
    private void OnScanStateChanged()
    {
        Debug.Log("<><><><>OnScanStateChanged<><><>--------"  + SpatialUnderstanding.Instance.ScanState.ToString());
        // If we are leaving the None state, go ahead and register shapes now
        if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done) &&
            SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
        {
            // Make sure we've created our shapes
        //    ShapeDefinition.Instance.CreateShapes();

            // Make sure our solver is initialized
         //   LevelSolver.Instance.InitializeSolver();

            // Setup the menu
            StartCoroutine(SetupMenu());
        }
    }
    #endregion

    #region 帮助方法
    private IEnumerator SetupMenu()
    {
        // Setup for queries
        SpatialUnderstandingDllTopology.TopologyResult[] resultsTopology = new SpatialUnderstandingDllTopology.TopologyResult[1];
        IntPtr resultsTopologyPtr = SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(resultsTopology);

        // Place on a wall (do it in a thread, as it can take a little while)
        SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placeOnWallDef =
            SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnWall(new Vector3(MenuWidth * 0.5f, MenuHeight * 0.5f, MenuMinDepth * 0.5f), 0.5f, 3.0f);
        SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult placementResult = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticObjectPlacementResult();
        Debug.Log("<color=yellow>IEnumerator SetupMenu()----------->>>>></color>");
        var thread =
#if UNITY_EDITOR || !UNITY_WSA
                new System.Threading.Thread
#else
                System.Threading.Tasks.Task.Run
#endif
            (() => {
                if (SpatialUnderstandingDllObjectPlacement.Solver_PlaceObject(
                    "UIPlacement",
                    SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(placeOnWallDef),
                    0,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticObjectPlacementResultPtr()) == 0)
                {
                    placementResult = null;
                }
            });

#if UNITY_EDITOR || !UNITY_WSA
        thread.Start();
#endif

        while
            (
#if UNITY_EDITOR || !UNITY_WSA
                !thread.Join(TimeSpan.Zero)
#else
                !thread.IsCompleted
#endif
                )
        {
            yield return null;
        }
        if (placementResult != null)
        {
            Debug.Log("PlaceMenu - ObjectSolver-OnWall");
            Vector3 posOnWall = placementResult.Position - placementResult.Forward * MenuMinDepth * 0.5f;
            PlaceMenu(posOnWall, -placementResult.Forward);
            yield break;
        }

        // Wait a frame
        yield return null;

        // Fallback, place floor (add a facing, if so)
        int locationCount = SpatialUnderstandingDllTopology.QueryTopology_FindLargestPositionsOnFloor(
            resultsTopology.Length, resultsTopologyPtr);
        if (locationCount > 0)
        {
            Debug.Log("PlaceMenu - LargestPositionsOnFloor");
            SpatialUnderstandingDllTopology.TopologyResult menuLocation = resultsTopology[0];
            Vector3 menuPosition = menuLocation.position + Vector3.up * MenuHeight;
            Vector3 menuLookVector = Camera.main.transform.position - menuPosition;
            PlaceMenu(menuPosition, (new Vector3(menuLookVector.x, 0.0f, menuLookVector.z)).normalized, true);
            yield break;
        }

        // Final fallback just in front of the user
        SpatialUnderstandingDll.Imports.QueryPlayspaceAlignment(SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceAlignmentPtr());
        SpatialUnderstandingDll.Imports.PlayspaceAlignment alignment = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceAlignment();
        Vector3 defaultPosition = Camera.main.transform.position + Camera.main.transform.forward * 2.0f;
        PlaceMenu(new Vector3(defaultPosition.x, Math.Max(defaultPosition.y, alignment.FloorYValue + 1.5f), defaultPosition.z), (new Vector3(Camera.main.transform.forward.x, 0.0f, Camera.main.transform.forward.z)).normalized, true);
        Debug.Log("PlaceMenu - InFrontOfUser");
    }


    private void PlaceMenu(Vector3 position, Vector3 normal, bool needsBillboarding = false)
    {
        Debug.Log("<color=red>执行多少次？？？？？？</color>");
        // Offset in a bit
        position -= normal * 0.05f;
        Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);

        // Place it
        transform.position = position;
        transform.rotation = rotation;

        // Setup the menu
        SetupMenus();

        // Enable it
        ParentPanel.gameObject.SetActive(true);

        // Create up a box
        MenuAnimatedBox = new AnimatedBox(0.0f, position, rotation, new Color(1.0f, 1.0f, 1.0f, 0.25f), new Vector3(MenuWidth * 0.5f, MenuHeight * 0.5f, 0.025f), LineDrawer.DefaultLineWidth * 0.5f);

        // Initial position
        transform.position = MenuAnimatedBox.AnimPosition.Evaluate(MenuAnimatedBox.Time);
        transform.rotation = MenuAnimatedBox.Rotation * Quaternion.AngleAxis(360.0f * MenuAnimatedBox.AnimRotation.Evaluate(MenuAnimatedBox.Time), Vector3.up);

        // Billboarding (note that because of the transition animation we need to place this late)
        placedMenuNeedsBillboard = needsBillboarding;

        // And mark that we've done it
        HasPlacedMenu = true;
    }

    private void SetupMenus()
    {


    }

    private void InitializedEdge()
    {
        Vector3[] poses = new Vector3[4];
        RectTransform rect = GridPanel.transform as RectTransform;
        rect.GetWorldCorners(poses);
        for (int i = 0; i < poses.Length; i++)
        {
            Debug.Log(poses[i]);
        }
    }

    #endregion

}
