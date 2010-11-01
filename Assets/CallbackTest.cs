using UnityEngine;
using System.Collections;

public class CallbackTest : MonoBehaviour {

	void LogMessage(string message) {
		Debug.Log(gameObject.name + " - " + message);
	}
}
