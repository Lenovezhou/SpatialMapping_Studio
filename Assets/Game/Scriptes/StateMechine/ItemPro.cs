using System;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Examples.SpatialUnderstandingFeatureOverview;

public enum ClickState
{
	Ido,Move,Rotate,Scale,Delet,OpenUI,CloseUI
}

public class ItemPro : StateMechinePro,IFocusable,IInputClickHandler,IManipulationHandler
{

    #region ImanipulationHandler实现
    public void OnManipulationStarted(ManipulationEventData eventData)
    {
    }

    public void OnManipulationUpdated(ManipulationEventData eventData)
    {
        Vector3 temp = eventData.CumulativeDelta;
        float xdistance = temp.x;
		float ydistance = temp.y;
		Vector3 localeulerangles = transform.localEulerAngles;
		Vector3 localscale = transform.localScale;
		switch (_cS) 
		{
		case ClickState.Rotate:
			transform.localEulerAngles = new Vector3(localeulerangles.x, localeulerangles.y + temp.x * 2, localeulerangles.z);
			break;
		case ClickState.Scale:
			if (localscale.x < 2 && localscale.x > 0.5f) 
			{
				if (ScaleIcon.GetComponent<SpriteRenderer>().color != Color.green)
				{
					ScaleIcon.GetComponent<SpriteRenderer> ().color = Color.green;
				}
				transform.localScale = new Vector3 (localscale.x + ydistance, localscale.y + ydistance, localscale.z + ydistance);
			}
			else if(localscale.x > 2)
			{
				if (temp.y < 0) {
					transform.localScale = new Vector3 (localscale.x + ydistance, localscale.y + ydistance, localscale.z + ydistance);
				} else {
					LerpColor (Time.time);
				}
			}else if(localscale.x < 0.5f)
			{
				if (temp.y > 0) {
					transform.localScale = new Vector3 (localscale.x + ydistance, localscale.y + ydistance, localscale.z + ydistance);
				} else {
					LerpColor (Time.time);
				}
			}
			break;
		default:
			break;
		}
    }

    public void OnManipulationCompleted(ManipulationEventData eventData)
    {
    }

    public void OnManipulationCanceled(ManipulationEventData eventData)
    {
    }

    #endregion

    #region IInputClickHandler 实现

    public void OnInputClicked (InputClickedEventData eventData)
	{
		
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

	
	#region IFocusable 实现

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
    private GameObject RotateIcon;
	private GameObject ScaleIcon;
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
        RotateIcon = transform.Find("RoateIcon").gameObject;
		ScaleIcon = transform.Find ("ScaleIcon").gameObject;
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
    void RoateOnEnter()
    {
        RotateIcon.SetActive(true);
    }
	void RotateUpdater(float timer)
	{
		RotateIcon.transform.Rotate(Vector3.back, timer * 5);
	}
    void RoateLeave()
    {
        RotateIcon.SetActive(false);
    }


	//缩放
	void ScaleEnter()
	{
		ScaleIcon.SetActive (true);
	}
	void ScaleUpdater(float timer)
	{
		
	}
	void ScaleLeave()
	{
		ScaleIcon.SetActive (false);
	}

	//删除
	void DeleteEnter()
	{
        ObjectPool.Instance.UnSpawn(UI);
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

        _rotate.OnEnter = RoateOnEnter;
        _rotate.OnUpdate = RotateUpdater;
		_rotate.OnLeave = RoateLeave;

		_Scaler.OnEnter = ScaleEnter;
		_Scaler.OnUpdate = ScaleUpdater;
		_Scaler.OnLeave = ScaleLeave;

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
	}

	private void LerpColor(float t)
	{
		float d = t % 1;
		Color c = new Color (1 - d, d, 0);
		ScaleIcon.GetComponent<SpriteRenderer> ().color = c;
	}

    #endregion

}
