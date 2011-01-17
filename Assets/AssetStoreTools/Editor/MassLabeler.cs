using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MassLabeler : EditorWindow
{
	const string kLabelListPath = "Assets/AssetStoreTools/Labels.asset";
	
	static LabelList m_Labels;
	
	string m_LabelAdditionField = "";
	Dictionary<int, object> m_CheckedLabels = new Dictionary<int, object> ();
	Vector2 m_ListScroll = Vector2.zero;
	
	
	[MenuItem ("AssetStore tools/Mass labeler")]
	public static void Launch ()
	{
		GetWindow (typeof (MassLabeler)).Show ();
	}
	
	
	void OnEnable ()
	{
		UpdateLabelSelection ();
	}
	
	
	static LabelList Labels
	{
		get
		{
			if (m_Labels == null)
			{
				m_Labels = AssetDatabase.LoadAssetAtPath (
					kLabelListPath,
					typeof (LabelList)
				) as LabelList;
				
				if (m_Labels == null)
				{
					Debug.Log ("Creating new label list");
					m_Labels = CreateInstance (typeof (LabelList)) as LabelList;
					if (m_Labels == null)
					{
						Debug.LogError ("Failed to create label list");
						return null;
					}
					string directory = Path.GetDirectoryName (kLabelListPath);
					if (!Directory.Exists (directory))
					{
						Directory.CreateDirectory (directory);
					}
					AssetDatabase.CreateAsset (m_Labels, kLabelListPath);
					EditorUtility.SetDirty (m_Labels);
					AssetDatabase.Refresh ();
				}
			}
			
			return m_Labels;
		}
	}
	
	
	void OnSelectionChange ()
	{
		UpdateLabelSelection ();
	}
	
	
	void UpdateLabelSelection ()
	{
		m_CheckedLabels = new Dictionary<int, object> ();
		
		foreach (Object obj in Selection.objects)
		{
			string[] labels = AssetDatabase.GetLabels (obj);
			foreach (string label in labels)
			{
				Labels.Add (label);
				m_CheckedLabels[Labels.IndexOf (label)] = null;
			}
		}
		
		Repaint ();
	}
	
	
	void ApplyLabels ()
	{
		List<string> selectedLabels = new List<string> ();
		foreach (int index in m_CheckedLabels.Keys)
		{
			selectedLabels.Add (Labels[index]);
		}
		
		foreach (Object obj in Selection.objects)
		{
			AssetDatabase.SetLabels (obj, selectedLabels.ToArray ());
			EditorUtility.SetDirty (obj);
		}
	}
	

	void OnGUI ()
	{
		OnAddLabelAreaGUI ();
		m_ListScroll = GUILayout.BeginScrollView (m_ListScroll);
			OnLabelListGUI ();
		GUILayout.EndScrollView ();
		
		if (Selection.objects.Length > 1)
		{
			GUI.contentColor = Color.yellow;
			GUI.backgroundColor = Color.red;
			GUILayout.Box (
				"WARNING: Applying on multi-select will override any labels only present on one item in the selection.",
				GUILayout.ExpandWidth (true)
			);
		}
		
		if (GUILayout.Button ("Apply to selection", GUILayout.ExpandWidth (true)))
		{
			ApplyLabels ();
		}
	}
	
	
	void OnAddLabelAreaGUI ()
	{
		GUILayout.BeginHorizontal ();
			m_LabelAdditionField = EditorGUILayout.TextField ("New label", m_LabelAdditionField).Replace (" ", "");
			if (GUILayout.Button ("Add"))
			{
				Labels.Add (m_LabelAdditionField);
				m_LabelAdditionField = "";
			}
		GUILayout.EndHorizontal ();
	}
	
	
	void OnLabelListGUI ()
	{
		for (int i = 0; i < Labels.Count; i++)
		{
			GUILayout.BeginHorizontal ();
				if (GUILayout.Toggle (
					m_CheckedLabels.ContainsKey (i),
					Labels[i],
					GUI.skin.GetStyle ("Button"),
					GUILayout.ExpandWidth (true)
				))
				{
					m_CheckedLabels[i] = null;
				}
				else
				{
					m_CheckedLabels.Remove (i);
				}

				if (GUILayout.Button ("Delete", GUILayout.ExpandWidth (false)))
				{
					m_CheckedLabels.Remove (i);
					Labels.Remove (Labels[i]);
				}
			GUILayout.EndHorizontal ();
		}
	}
}
