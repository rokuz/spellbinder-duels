using UnityEngine;
using System.Collections;

public class AnimDisabler : MonoBehaviour {
    public void OnEndAnimation()
    {
        gameObject.SetActive(false);
    }
}
