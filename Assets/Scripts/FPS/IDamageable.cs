using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount, UnityEngine.Vector3 hitPoint, UnityEngine.Vector3 hitNormal);
}

