using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public float moveSpeed = 5.0f;

	void Update()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
	}
}
