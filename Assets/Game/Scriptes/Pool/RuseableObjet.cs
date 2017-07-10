using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public abstract class RuseableObjet:MonoBehaviour,IRuseable
{
    public abstract void Spawn();

    public abstract void UnSpawn();
}
