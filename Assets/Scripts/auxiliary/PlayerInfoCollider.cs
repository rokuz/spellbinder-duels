using UnityEngine;
using System.Collections;

public class PlayerInfoCollider : MonoBehaviour {

    public delegate void OnPlayerInfoTapped();

    private OnPlayerInfoTapped onPlayerInfoTapped = null;

    public void Setup(OnPlayerInfoTapped onPlayerInfoTapped)
    {
        this.onPlayerInfoTapped = onPlayerInfoTapped;
    }

    public void OnMouseDown()
    {
        if (onPlayerInfoTapped != null)
            onPlayerInfoTapped();
    }
}

