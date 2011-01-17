/* 
 * Asset store mananger
 * 
 * Jonas Drewsen - (C) Unity3d.com - 2010
 * 
 * Takes care of creating, checking and publishing asset packages
 * for the Unity Asset Store.
 * 
 */
using UnityEditor;
using UnityEngine;

/*
 * The GUI for the Asset Store Manager
 */
public class AssetStoreManager : EditorWindow {

	static AssetStoreManagerInternal dptr = null;
	
	[MenuItem ("AssetStore tools/Package Manager")]
	static void Launch ()
	{		 
		the();
		var window = (AssetStoreManager) EditorWindow.GetWindow (typeof (AssetStoreManager));
		window.position = new Rect(100,100,800,900);
		window.Show();
	}
	
	void Update() {
		if (the().Update()) 
			Repaint();
	}
		
    void OnGUI () {
		the().OnGUI();
	}
	
	static private AssetStoreManagerInternal the() {
        if (dptr == null) {
			dptr = new AssetStoreManagerInternal(typeof(AssetStorePackageConfigure), 
			                                     typeof(TermsViewer), 
			                                     typeof(SubmitDialog));
		}
		return dptr;
	}
}