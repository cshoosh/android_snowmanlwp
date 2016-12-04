using UnityEngine;
using System.Collections;

public class SnowControl : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void FallSnow(string set){
		if (set == "true"){
			GetComponent<ParticleEmitter>().minSize = 0.1f;
			GetComponent<ParticleEmitter>().maxSize = 0.2f;
		}
		else{
			GetComponent<ParticleEmitter>().minSize = 0f;
			GetComponent<ParticleEmitter>().maxSize = 0f;
		}

	}
	

}
