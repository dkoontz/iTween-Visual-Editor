using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class LabelList : ScriptableObject
{
	[HideInInspector]
	public string[] m_Labels = new string[0];
	
	
	public int Count
	{
		get
		{
			return m_Labels.Length;
		}
	}
	
	
	public string this [int index]
	{
		get
		{
			return m_Labels[index];
		}
	}
	
	
	public int IndexOf (string label)
	{
		return Array.IndexOf (m_Labels, label);
	}
	
	
	public string[] ToArray ()
	{
		return m_Labels;
	}
	
	
	public string Add (string label)
	{
		if (Array.IndexOf (m_Labels, label) == -1)
		{
			string[] newLabels = new string[m_Labels.Length + 1];
			
			m_Labels.CopyTo (newLabels, 0);
			newLabels[m_Labels.Length] = label;
			
			m_Labels = newLabels;			
			EditorUtility.SetDirty (this);
		}
		
		return label;
	}
	
	
	public bool Remove (string label)
	{
		int index = Array.IndexOf (m_Labels, label);
		
		if (index == -1)
		{
			return false;
		}
		
		string[] newLabels = new string[m_Labels.Length - 1];
		
		Array.Copy (m_Labels, 0, newLabels, 0, index);
		Array.Copy (m_Labels, index + 1, newLabels, index, m_Labels.Length - index - 1);
		
		m_Labels = newLabels;
		EditorUtility.SetDirty (this);
		
		return true;
	}
}
