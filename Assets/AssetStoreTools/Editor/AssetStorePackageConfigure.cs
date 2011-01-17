// AngryAnt

using UnityEngine;
using UnityEditor;

public class AssetStorePackageConfigure : EditorWindow
{
	void OnEnable ()
	{
		JsonFileCreationKit.OnEnable();
	}
		
	void OnGUI ()
	{		
		JsonFileCreationKit.OnGUI();
	}
}
