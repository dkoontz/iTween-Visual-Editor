//by Bob Berkebile : Pixelplacement : http://www.pixelplacement.com

using UnityEngine;
using System.Collections.Generic;

public class iTweenPath : MonoBehaviour
{
	public string pathName ="";
	public Color pathColor = Color.cyan;
	public List<Vector3> nodes = new List<Vector3>(){Vector3.zero, Vector3.zero};
	public int nodeCount;
	public static Dictionary<string, iTweenPath> paths = new Dictionary<string, iTweenPath>();
	public bool initialized = false;
	public string initialName = "";

	void Update()
	{


	}

	void OnEnable(){
		paths.Add(pathName.ToLower(), this);
	}
	
	void OnDrawGizmosSelected(){
		if(enabled) { // dkoontz
			if(nodes.Count > 0)
			{
				List<Vector3> li = new List<Vector3>();
				for (int i = 0; i < nodes.Count; i++)
				{
					Vector3 v3 = transform.TransformPoint(nodes[i]);
					li.Add(v3);
				}
				//iTween.DrawPath(nodes.ToArray(), pathColor);
				iTween.DrawPath(li.ToArray(), pathColor);

			}
		} // dkoontz
	}
	
	public static Vector3[] GetPath(string requestedName){
		requestedName = requestedName.ToLower();
		if(paths.ContainsKey(requestedName))
		{
			List<Vector3> outlist = new List<Vector3>();
			for (int i = 0; i < paths[requestedName].nodes.Count; i++)
			{
				Vector3 newval = paths[requestedName].transform.TransformPoint(paths[requestedName].nodes[i]);
				outlist.Add(newval);
			}
			//return paths[requestedName].nodes.ToArray();
			return outlist.ToArray();
		}else{
			Debug.Log("No path with that name exists! Are you sure you wrote it correctly?");
			return null;
		}
	}
}

