using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HoloToolkit.Examples.SpatialUnderstandingFeatureOverview;

/// <summary>
/// 每个UI上都会挂载这个脚本来控制鼠标进入状态
/// </summary>
public class UIItems : StateMechinePro, IFocusable, IInputClickHandler
{
    #region 字段

    State onfocus = new State();

    State ido = new State();

    State onselect = new State();

    Vector3 originepos;
    Vector3 originrot;

    SpatialUnderstandingCursor cursor;


    #endregion

    #region 接口实现
    public void OnFocusEnter()
    {
        STATE = onfocus;
    //    Debug.Log("OnFocusEnter");
    }

    public void OnFocusExit()
    {
        STATE = ido;
     //   Debug.Log("<color=yellow>OnFocusExit</color>");
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        GameObject go = ObjectPool.Instance.Spawn(gameObject.name);
    //    Debug.Log("name========" + gameObject.name);
    //    go.GetComponentInChildren<MeshRenderer>().material.color = new Color(UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1));
        ItemPro pro = go.GetComponent<ItemPro>();
        pro.cursor = cursor;
        pro.Init();
        pro.ChangeState(ClickState.Move);
        go.transform.position = Camera.main.transform.forward * 2;
    }
    #endregion

    #region Unity回调
    void Start()
    {
        onfocus.OnEnter = FocusEnter;
        onfocus.OnUpdate = FocusUpdater;
        onfocus.OnLeave = FocuseLeave;

        onselect.OnEnter = SelectEnter;
        onselect.OnUpdate = SelectUpdater;
        onselect.OnLeave = SelectLeave;

        ido.OnEnter = IdoEnter;
        ido.OnUpdate = IdoUpdater;


        if (GetComponent<BoxCollider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }

        originepos = transform.localPosition;
        originrot = transform.localEulerAngles;

    }

    void Update()
    {
        OnUpdater(Time.deltaTime);
    }
    #endregion

    #region 状态机方法
    void FocusEnter()
    {

    }
    void FocusUpdater(float timer)
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(originepos.x, originepos.y + 1, originepos.z), timer * 2);
        transform.Rotate(Vector3.back, timer * 90);
    }
    void FocuseLeave()
    {

    }

    void SelectEnter() { }
    void SelectUpdater(float timer) { }
    void SelectLeave() { }

    void IdoEnter() { }
    void IdoUpdater(float timer)
    {
        if (statetimer < 2)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originepos, timer * 2);
            transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, originrot, timer * 2);
        }
    }
    #endregion

    #region 帮助方法
    public void Init(SpatialUnderstandingCursor cursor)
    {
        this.cursor = cursor;
    }
    #endregion
}
