using UnityEngine;
using System.Collections;

public class CamController : MonoBehaviour
{

	private float camOffsetFactor = 0.5f;
	private Vector3 newPosition;

	void Awake ()
	{
	}
	void FixedUpdate ()
	{
		newPosition = new Vector3 (camOffsetFactor, transform.position.y, transform.position.z);
		transform.position = Vector3.Lerp (transform.position, newPosition, 0.02f);
		System.Threading.Thread.Sleep (5);
	}

	public void SetCamOffsetFactor (string offset)
	{
		try {
			camOffsetFactor = float.Parse (offset);
		} catch (System.Exception e) {
			
		}
	}
	
}
