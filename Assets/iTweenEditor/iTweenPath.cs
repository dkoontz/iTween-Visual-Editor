//by Bob Berkebile : Pixelplacement : http://www.pixelplacement.com

using UnityEngine;
using System.Collections.Generic;

public class iTweenPath : MonoBehaviour
{
	public string pathName ="";
	public Color pathColor = Color.cyan;
	public List<Vector3> nodes = new List<Vector3>(){Vector3.zero, Vector3.zero};
	Vector3[] realNodes;
	public int nodeCount;
	public static Dictionary<string, iTweenPath> paths = new Dictionary<string, iTweenPath>();
	public bool initialized = false;
	public string initialName = "";

	void CalcRealNodes()
	{
		realNodes = new Vector3[nodes.Count];
		for (int i=0; i<realNodes.Length; i++)
			{
				realNodes[i] = GetRealPos(i);
			}
	}

	void OnEnable(){
		paths[pathName.ToLower()] = this;
		CalcRealNodes();
	}

	public Vector3 GetRealPos(int idx)
	{
		return transform.position + transform.rotation * nodes[idx];
	}

	void OnDrawGizmosSelected(){
		if(enabled) { // dkoontz
			if(nodes.Count > 0){
				CalcRealNodes();
				iTween.DrawPath(realNodes, pathColor);
			}
		} // dkoontz
	}

	public static Vector3[] GetPath(string requestedName){
		requestedName = requestedName.ToLower();
		if(paths.ContainsKey(requestedName)){
			return paths[requestedName].realNodes;
		}else{
			Debug.Log("No path with that name exists! Are you sure you wrote it correctly?");
			return null;
		}
	}
}
