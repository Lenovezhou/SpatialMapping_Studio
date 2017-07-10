﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Examples.InteractiveElements;

public class UIManager : MonoBehaviour {

	#region 字段
	private ItemPro itemobj;

	public ItemPro Itemobj {
		get
		{
			return itemobj;
		}
		set 
		{
			itemobj = value;
			move.OnDownEvent.RemoveAllListeners ();
			scale.OnDownEvent.RemoveAllListeners ();
			rotate.OnDownEvent.RemoveAllListeners ();
			delet.OnDownEvent.RemoveAllListeners ();

			move.OnDownEvent.AddListener (()=>{itemobj.ChangeState(ClickState.Move);});
			scale.OnDownEvent.AddListener (()=>{itemobj.ChangeState(ClickState.Scale);});
			rotate.OnDownEvent.AddListener (()=>{itemobj.ChangeState(ClickState.Rotate);});
			delet.OnDownEvent.AddListener (()=>{itemobj.ChangeState(ClickState.Delet);ObjectPool.Instance.UnSpawn(gameObject);});

		}
	}


	public Interactive move,scale,rotate,delet;

	#endregion


	#region Unity回调

	void Start () 
	{
		
	}


	void Update () 
	{

	}
	#endregion

}