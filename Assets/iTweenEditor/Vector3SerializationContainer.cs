using UnityEngine;
using System; 

[Serializable]
public class Vector3SerializationContainer {

	float x;
	float y;
	float z;
	
	public Vector3SerializationContainer(Vector3 v3) {
		x = v3.x;
		y = v3.y;
		z = v3.z;
	}
	
	public Vector3 ToVector3() {
		return new Vector3(x, y, z);
	}
}