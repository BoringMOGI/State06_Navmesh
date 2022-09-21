using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RsourcesTest : MonoBehaviour
{
    [SerializeField] new SpriteRenderer renderer;

    void Start()
    {
        Sprite load = Resources.Load<Sprite>("Sprite");

        renderer.sprite = load;
    }
}
