using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllChildPos : MonoBehaviour 
{


	void Start ()
	{
		Debug.Log (GetComponent<MeshRenderer>().bounds.size.x);
	}
	

	void Update ()
	{
		
	}
}
