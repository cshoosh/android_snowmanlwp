using UnityEngine;
using System.Collections;

public class PhotoChange : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
		
	}

	// Update is called once per frame
	void Update ()
	{
	}

	public void ChangePhoto (string url)
	{
		
		try {
			string[] meta = url.Split (new char[] { '?' });
			int width = System.Int32.Parse (meta[0]);
			int height = System.Int32.Parse (meta[1]);
			
			string[] data = meta[2].Split (new char[] { ':' });
			
			meta = null;
			
			Color32[] colorarray = new Color32[data.Length];
			int i = 0;
			foreach (string raw in data) {
				uint pixel = System.UInt32.Parse (raw);
				byte[] bytes = System.BitConverter.GetBytes (pixel);
				//	Debug.Log(System.BitConverter.ToString(bytes));
				colorarray[i] = new Color32 (bytes[2], bytes[1], bytes[0], 0xFF);
				i++;
			}
			
			data = null;
			Texture2D tex = new Texture2D (width, height);
			tex.wrapMode = TextureWrapMode.Clamp;
			tex.SetPixels32 (colorarray);
			tex.Apply ();
			
			GetComponent<MeshRenderer> ().sharedMaterial.mainTexture = tex;
		}catch(System.Exception e){
			
		}
	}
}
