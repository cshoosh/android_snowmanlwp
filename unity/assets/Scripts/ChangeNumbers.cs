using UnityEngine;
using System.Collections;
using System;

public class ChangeNumbers : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	public void ChangeDate (string date){
		try{
		int num = parseString(date);
		
		GameObject numLeft = GameObject.Find("dayNumLeft");
		
		GameObject numMid = GameObject.Find("dayNumMiddle");
		GameObject numRight = GameObject.Find("dayNumRight");
		GameObject s = GameObject.Find("s");
		
		if (num > 1)
			s.GetComponent<MeshRenderer>().renderer.enabled = true;
		else
			s.GetComponent<MeshRenderer>().renderer.enabled = false;

		int iNumLeft = -1,iNumRight = -1,iNumMid = -1;
		if (num < 10){
			iNumRight = num;
		}
		else if (num < 100){
			iNumMid = num/10;
			iNumRight = num - (iNumMid * 10);
		}
		else if (num < 1000){
			iNumLeft = num/100;
			iNumMid = (num - (iNumLeft * 100)) / 10;
			iNumRight = (num - (iNumLeft * 100)) - (iNumMid * 10);
		}
		
		ChangeNumber(numLeft,iNumLeft);
		ChangeNumber(numMid,iNumMid);
		ChangeNumber(numRight,iNumRight);
		}catch(System.Exception e){
			
		}
	}
	
	private int parseString(string msg){
		return int.Parse(msg);
	}
	
	private void ChangeNumber (GameObject gameObj,int num){	
		
		Mesh numberMesh = null;
		GameObject numbers = GameObject.Find("numbers");
		
	
		switch (num){
			case 0:
				numberMesh = numbers.transform.FindChild("0").GetComponent<MeshFilter>().mesh;
			break;
			case 1:
				numberMesh = numbers.transform.FindChild("1").GetComponent<MeshFilter>().mesh;
			break;
			case 2:
				numberMesh = numbers.transform.FindChild("2").GetComponent<MeshFilter>().mesh;
			break;
			case 3:
				numberMesh = numbers.transform.FindChild("3").GetComponent<MeshFilter>().mesh;
			break;
			case 4:
				numberMesh = numbers.transform.FindChild("4").GetComponent<MeshFilter>().mesh;
			break;
			case 5:
				numberMesh = numbers.transform.FindChild("5").GetComponent<MeshFilter>().mesh;
			break;
			case 6:
				numberMesh = numbers.transform.FindChild("6").GetComponent<MeshFilter>().mesh;
			break;
			case 7:
				numberMesh = numbers.transform.FindChild("7").GetComponent<MeshFilter>().mesh;
			break;
			case 8:
				numberMesh = numbers.transform.FindChild("8").GetComponent<MeshFilter>().mesh;
			break;
			case 9:
				numberMesh = numbers.transform.FindChild("9").GetComponent<MeshFilter>().mesh;
			break;
			default:
				break;
			}
		
			gameObj.GetComponent<MeshFilter>().mesh = numberMesh;		
	}
}
