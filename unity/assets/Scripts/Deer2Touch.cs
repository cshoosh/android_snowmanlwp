using UnityEngine;
using System.Collections;

public class Deer2Touch : MonoBehaviour
{
	Ray mRay;
	RaycastHit mHit;
	Vector3 touchPosition;
	// Use this for initialization
	void Start ()
	{
		touchPosition = new Vector3 (0, 0, 0);
	}

	// Update is called once per frame
	void Update ()
	{
	}

	public void setTouch (string touches)
	{
		try {
			string[] firstSecond = touches.Split (':');
			
			int X = int.Parse (firstSecond[0]);
			int Y = int.Parse (firstSecond[1]);
			
			touchPosition.x = X;
			touchPosition.y = Y;
			
			mRay = Camera.main.ScreenPointToRay (touchPosition);
			if (Physics.Raycast (mRay, out mHit)) {
				if (mHit.collider.gameObject == this.gameObject) {
					GameObject.Find ("deer02").animation.CrossFade ("look around");
					GameObject.Find ("deer02").animation.CrossFadeQueued ("idle");
				}
			}
		} catch (System.Exception e) {
		}
	}
}
