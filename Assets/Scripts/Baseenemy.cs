using UnityEngine;

public class Baseenemy : Sirviente
{
    public override void Awake()
    {
        base.Awake();
        if (rb != null) rb.gravityScale = 12f;
    }
}