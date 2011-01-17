/* 
 * Terms and conditions acceptance dialog
 * 
 * Jonas Drewsen - (C) Unity3d.com - 2010
 * 
 */

using UnityEngine;
using UnityEditor;

public class SubmitDialog : EditorWindow
{
	void OnEnable ()
	{
		SubmitDialogInternal.OnEnable();
	}
	
	void OnDisable ()
	{
		SubmitDialogInternal.OnDisable();
	}
				
	void OnGUI ()
	{		
		SubmitDialogInternal.OnGUI();
	}
}
