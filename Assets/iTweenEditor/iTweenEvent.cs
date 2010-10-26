using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

public class iTweenEvent : MonoBehaviour{
	public enum TweenType {
		AudioFrom,
		AudioTo,
		AudioUpdate,
		CameraFadeFrom,
		CameraFadeTo,
		ColorFrom,
		ColorTo,
		ColorUpdate,
		FadeFrom,
		FadeTo,
		FadeUpdate,
		LookFrom,
		LookTo,
		LookUpdate,
		MoveAdd,
		MoveBy,
		MoveFrom,
		MoveTo,
		MoveUpdate,
		PunchPosition,
		PunchRotation,
		PunchScale,
		RotateAdd,
		RotateBy,
		RotateFrom,
		RotateTo,
		RotateUpdate,
		ScaleAdd,
		ScaleBy,
		ScaleFrom,
		ScaleTo,
		ScaleUpdate,
		ShakePosition,
		ShakeRotation,
		ShakeScale,
		Stab
		//ValueTo
	}
	
	public bool playAutomatically = true;
	public float delay = 0;
	public iTweenEvent.TweenType type = iTweenEvent.TweenType.MoveTo;
	
	public Dictionary<string, object> Values {
		get { 
			if(null == values) {
				DeserializeValues();
			}
			return values;
		}
		set {
			values = value;
			SerializeValues();
		}
	}
	
	// Must be public to be serialized
	[HideInInspector]
	public byte[] bytes;
	
	Dictionary<string, object> values;
	
	public void Start() {
		if(playAutomatically) {
			Play();
		}
	}
	
	public void Play() {
		StartCoroutine(StartEvent());
	}
	
	IEnumerator StartEvent() {
		yield return new WaitForSeconds(delay);
		var optionsHash = new Hashtable();
		foreach(var pair in Values) {
			optionsHash.Add(pair.Key, pair.Value);
		}
		
		switch(type) {
		case TweenType.AudioFrom:
			iTween.AudioFrom(gameObject, optionsHash);
			break;
		case TweenType.AudioTo:
			iTween.AudioTo(gameObject, optionsHash);
			break;
		case TweenType.AudioUpdate:
			iTween.AudioUpdate(gameObject, optionsHash);
			break;
		case TweenType.CameraFadeFrom:
			iTween.CameraFadeFrom(optionsHash);
			break;
		case TweenType.CameraFadeTo:
			iTween.CameraFadeTo(optionsHash);
			break;
		case TweenType.ColorFrom:
			iTween.ColorFrom(gameObject, optionsHash);
			break;
		case TweenType.ColorTo:
			iTween.ColorTo(gameObject, optionsHash);
			break;
		case TweenType.ColorUpdate:
			iTween.ColorUpdate(gameObject, optionsHash);
			break;
		case TweenType.FadeFrom:
			iTween.FadeFrom(gameObject, optionsHash);
			break;
		case TweenType.FadeTo:
			iTween.FadeTo(gameObject, optionsHash);
			break;
		case TweenType.FadeUpdate:
			iTween.FadeUpdate(gameObject, optionsHash);
			break;
		case TweenType.LookFrom:
			iTween.LookFrom(gameObject, optionsHash);
			break;
		case TweenType.LookTo:
			iTween.LookTo(gameObject, optionsHash);
			break;
		case TweenType.LookUpdate:
			iTween.LookUpdate(gameObject, optionsHash);
			break;
		case TweenType.MoveAdd:
			iTween.MoveAdd(gameObject, optionsHash);
			break;
		case TweenType.MoveBy:
			iTween.MoveBy(gameObject, optionsHash);
			break;
		case TweenType.MoveFrom:
			iTween.MoveFrom(gameObject, optionsHash);
			break;
		case TweenType.MoveTo:
			iTween.MoveTo(gameObject, optionsHash);
			break;
		case TweenType.MoveUpdate:
			iTween.MoveUpdate(gameObject, optionsHash);
			break;
		case TweenType.PunchPosition:
			iTween.PunchPosition(gameObject, optionsHash);
			break;
		case TweenType.PunchRotation:
			iTween.PunchRotation(gameObject, optionsHash);
			break;
		case TweenType.PunchScale:
			iTween.PunchScale(gameObject, optionsHash);
			break;
		case TweenType.RotateAdd:
			iTween.RotateAdd(gameObject, optionsHash);
			break;
		case TweenType.RotateBy:
			iTween.RotateBy(gameObject, optionsHash);
			break;
		case TweenType.RotateFrom:
			iTween.RotateFrom(gameObject, optionsHash);
			break;
		case TweenType.RotateTo:
			iTween.RotateTo(gameObject, optionsHash);
			break;
		case TweenType.RotateUpdate:
			iTween.RotateUpdate(gameObject, optionsHash);
			break;
		case TweenType.ScaleAdd:
			iTween.ScaleAdd(gameObject, optionsHash);
			break;
		case TweenType.ScaleBy:
			iTween.ScaleBy(gameObject, optionsHash);
			break;
		case TweenType.ScaleFrom:
			iTween.ScaleFrom(gameObject, optionsHash);
			break;
		case TweenType.ScaleTo:
			iTween.ScaleTo(gameObject, optionsHash);
			break;
		case TweenType.ScaleUpdate:
			iTween.ScaleUpdate(gameObject, optionsHash);
			break;
		case TweenType.ShakePosition:
			iTween.ShakePosition(gameObject, optionsHash);
			break;
		case TweenType.ShakeRotation:
			iTween.ShakeRotation(gameObject, optionsHash);
			break;
		case TweenType.ShakeScale:
			iTween.ShakeScale(gameObject, optionsHash);
			break;
		case TweenType.Stab:
			iTween.Stab(gameObject, optionsHash);
			break;
		default:
			throw new System.ArgumentException("Invalid tween type: " + type);
		}
	}
	
	
	// need to add Color and Transform
	void ReplaceNonSerializableTypes(object[] list) {
		for(int i = 0; i < list.Length; ++i) {
			if(null == list[i]) { continue; }
			
			if(list[i] is Vector3) {
				list[i] = new Vector3SerializationContainer((Vector3)list[i]);
			}
			else if(list[i] is Vector3[]) {
				Vector3[] originalValues = (Vector3[])list[i];
				Vector3SerializationContainer[] serializableValues = new Vector3SerializationContainer[originalValues.Length];
				for(int i2 = 0; i2 < originalValues.Length; ++i2) {
					serializableValues[i2] = new Vector3SerializationContainer(originalValues[i2]);
				}
				list[i] = serializableValues;
			}
		}
	}
	
	void RestoreNonSerializableTypes(object[] list) {
		for(int i = 0; i < list.Length; ++i) {
			if(null == list[i]) { continue; }
			
			if(list[i] is Vector3SerializationContainer) {
				list[i] = ((Vector3SerializationContainer)list[i]).ToVector3();
			}
			else if(list[i] is Vector3SerializationContainer[]) {
				Vector3SerializationContainer[] serializedValues = (Vector3SerializationContainer[])list[i];
				Vector3[] originalValues = new Vector3[serializedValues.Length];
				for(int i2 = 0; i2 < serializedValues.Length; ++i2) {
					originalValues[i2] = serializedValues[i2].ToVector3();
				}
				list[i] = originalValues;
			}
		}
	}
	
	void SerializeValues() {
		var list = new object[values.Count * 2];
		var keys = values.Keys.ToArray();
		for(var i = 0; i < values.Count; ++i) {
			list[i*2] = keys[i];
			list[(i*2)+1] = values[keys[i]];
		}
		
		ReplaceNonSerializableTypes(list);
		
		MemoryStream stream = new MemoryStream();
		BinaryFormatter formatter = new BinaryFormatter();
		formatter.Serialize(stream, list);
		bytes = stream.ToArray();
	}
	
	void DeserializeValues() {
		values = new Dictionary<string, object>();
		
		if(bytes != null && bytes.Length > 0) {
			BinaryFormatter formatter = new BinaryFormatter();
			var list = (object[])formatter.Deserialize(new MemoryStream(bytes));
			RestoreNonSerializableTypes(list);
			
			for(var i = 0; i < list.Count(); i+=2) {
				values.Add((string)list[i], list[i+1]);
			}
		}
	}
}