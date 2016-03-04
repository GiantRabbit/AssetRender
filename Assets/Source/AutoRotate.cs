using UnityEngine;

public class AutoRotate : MonoBehaviour
{
	public void Update()
    {
	   transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * 30);
       transform.RotateAround(Vector3.zero, Vector3.left, Time.deltaTime * 10);
	}
}
