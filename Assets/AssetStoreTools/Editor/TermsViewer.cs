/* 
 * Terms and conditions acceptance dialog
 * 
 * Jonas Drewsen - (C) Unity3d.com - 2010
 * 
 */

using UnityEngine;
using UnityEditor;

public class TermsViewer : EditorWindow
{
	void OnEnable ()
	{
		TermsViewerInternal.OnEnable();
	}
	
	void OnDisable ()
	{
		TermsViewerInternal.OnDisable();
	}
				
	void OnGUI ()
	{		
		TermsViewerInternal.OnGUI();
	}
}
