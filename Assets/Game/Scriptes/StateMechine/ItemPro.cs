using System;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Examples.SpatialUnderstandingFeatureOverview;

public enum ClickState
{
	Ido,Move,Rotate,Scale,Delet,OpenUI,CloseUI
}

public class ItemPro : StateMechinePro,IFocusable,IInputClickHandler,IInputHandler
{
    #region IInputClickHandler implementation

    public void OnInputUp(InputEventData eventData)
    {
    //    Debug.Log("OnInputUp:::::::");
    }

    public void OnInputDown(InputEventData eventData)
    {
    //    Debug.Log("OnInputDown:::::::");
    }



    public void OnInputClicked (InputClickedEventData eventData)
	{
        Debug.Log("Air tap 时 _cS::::::" + _cS.ToString());
        if (_cS == ClickState.Ido || _cS == ClickState.CloseUI)
        {
            ChangeState(ClickState.OpenUI);
        }
        else if (_cS == ClickState.OpenUI)
        {
            ChangeState(ClickState.CloseUI);
        }
        else
        {
            ChangeState(ClickState.Ido);
        }

	}

	#endregion

	
	#region IFocusable implementation

	public void OnFocusEnter ()
	{
	//	STATE = _focused;
	}

	public void OnFocusExit ()
	{

	}

	#endregion


	#region 字段
	private Transform UIpos;
	private GameObject UI;
    [SerializeField]
	private ClickState _cS;

    private BoxCollider box;

	public SpatialUnderstandingCursor cursor;
	#endregion


	#region 状态

	//空闲
	private State _ido = new State();

	//focused
	private State _focused = new State();
	//打开UI
	private State _openUI = new State();
	//关闭UI
	private State _closeUI = new State();
	//移动
	private State _move = new State();
	//旋转
	private State _rotate = new State();
	//缩放
	private State _Scaler = new State();
	//删除
	private State _delete = new State();
	#endregion


	#region Unity回调

	void Start()
	{
        box = GetComponent<BoxCollider>();
    }

	void Update()
	{
		OnUpdater (Time.deltaTime);
	}
	#endregion


	#region 状态机
	//被注视
	void FocusedEnter()
	{
		
	}
	void FocusedUpdater(float timer)
	{
		
	}

	//打开UI
	void OpenUIEnter()
	{
        if (UI == null)
        {
            UI = ObjectPool.Instance.Spawn("ItemUI");
        }

        box.enabled = false;

        Transform pos = transform.Find ("UIPos");
		UI.transform.SetParent (pos.transform);
		UI.transform.position = pos.position;
		UI.transform.localEulerAngles = Vector3.zero;
		UI.GetComponent<UIManager> ().Itemobj = this;

	}
	void OpenUIUpdater(float timer)
	{
		float temp = statetimer;
		if (temp <= 1) 
		{
			UI.transform.localScale = new Vector3 (temp, temp, temp);
		}else if (UI.transform.localScale != Vector3.one)
		{
			UI.transform.localScale = Vector3.one;
		}
	}
    void OpenUILeave()
    {
        box.enabled = true;
    }


	//关闭UI
	void CloseUIEnter()
	{
    }
	void CloseUIUpdater(float timer)
	{
        float temp = statetimer;
        if (temp <= 1)
        {
            UI.transform.localScale = new Vector3(1 - temp, 1 - temp, 1 - temp);
        }
        else if (UI.transform.localScale != Vector3.zero)
        {
            UI.transform.localScale = Vector3.zero;
        }
    }

	//移动
	void MoveEnter()
	{
		cursor.Target = gameObject;
	}
	void MoveUpdater(float timer)
	{
	//	transform.position = Camera.main.transform.forward 
	}
	void MoveLeave()
	{
		cursor.Target = null;
	}

	//旋转
	void RotateUpdater(float timer)
	{
		
	}

	//缩放
	void ScaleUpdater(float timer)
	{
		
	}

	//删除
	void DeleteEnter()
	{
		ObjectPool.Instance.UnSpawn (gameObject);
	} 
	#endregion



	#region 帮助方法
	public void Init()
	{
		_focused.OnEnter = FocusedEnter;
		_focused.OnUpdate = FocusedUpdater;

		_openUI.OnEnter = OpenUIEnter;
		_openUI.OnUpdate = OpenUIUpdater;
        _openUI.OnLeave = OpenUILeave;

		_closeUI.OnEnter = CloseUIEnter;
		_closeUI.OnUpdate = CloseUIUpdater;

		_move.OnEnter = MoveEnter;
		_move.OnUpdate = MoveUpdater;
		_move.OnLeave = MoveLeave;

		_rotate.OnUpdate = RotateUpdater;

		_Scaler.OnUpdate = ScaleUpdater;

		_delete.OnEnter = DeleteEnter;

	}
	public void ChangeState(ClickState cs)
	{
		switch (cs)
		{
		case ClickState.Ido:
			STATE = _ido;
			break;
		case ClickState.Move:
			STATE = _move;
			break;
		case ClickState.Rotate:
			STATE = _rotate;
			break;
		case ClickState.Scale:
			STATE = _Scaler;
			break;
		case ClickState.Delet:
			STATE = _delete;
			break;
        case ClickState.CloseUI:
            STATE = _closeUI;
            break;
        case ClickState.OpenUI:
            STATE = _openUI;
            break;
            default:
			break;
		}
		_cS = cs;
        Debug.Log("ChangeState::::::" + _cS);
	}


    #endregion

}
