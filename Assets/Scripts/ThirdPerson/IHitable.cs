using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitable
{
    public void Execute(Transform knockbackSource, bool isPlayerAttack);
}
