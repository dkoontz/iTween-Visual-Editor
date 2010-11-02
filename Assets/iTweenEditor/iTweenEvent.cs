// Copyright (c) 2009 David Koontz
// Please direct any bugs/comments/suggestions to david@koontzfamily.org
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
	
	[SerializeField]
	byte[] bytes;
	
	// Used to hold onto Dictionary entries that are Transforms, since we can't serialize them via BitStream or with .NET
	[SerializeField]
	GameObject[] gameObjects;
	
	// Used to hold onto Dictionary entries that are Transforms, since we can't serialize them via BitStream or with .NET
	[SerializeField]
	Transform[] transforms;

	// Used to hold onto Dictionary entries that are AudioClips, since we can't serialize them via BitStream or with .NET
	[SerializeField]
	AudioClip[] audioClips;
	
	// Used to hold onto Dictionary entries that are AudioSources, since we can't serialize them via BitStream or with .NET
	[SerializeField]
	AudioSource[] audioSources;
	
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
	
	
	void ReplaceNonSerializableTypes(object[] list) {
		var transformList = new List<Transform>();
		var gameObjectList = new List<GameObject>();
		var audioClipList = new List<AudioClip>();
		var audioSourceList = new List<AudioSource>();
		
		for(var i = 0; i < list.Length; ++i) {
			if(null == list[i]) { continue; }
			
			if(list[i] is Vector3) {
				list[i] = new Vector3SerializationContainer((Vector3)list[i]);
			}
			else if(list[i] is Vector3[]) {
				var originalValues = (Vector3[])list[i];
				var serializableValues = new Vector3SerializationContainer[originalValues.Length];
				for(var i2 = 0; i2 < originalValues.Length; ++i2) {
					serializableValues[i2] = new Vector3SerializationContainer(originalValues[i2]);
				}
				list[i] = serializableValues;
			}
			else if(list[i] is Transform) {
				transformList.Add((Transform)list[i]);
				list[i] = new IndexedSerializationContainer<Transform>(transformList.Count - 1);
			}
			else if(list[i] is Transform[]) {
				var originalValues = (Transform[])list[i];
				var serializableValues = new IndexedSerializationContainer<Transform>[originalValues.Length];
				for(var i2 = 0; i2 < originalValues.Length; ++i2) {
					transformList.Add(originalValues[i2]);
					serializableValues[i2] = new IndexedSerializationContainer<Transform>(transformList.Count - 1);
				}
				list[i] = serializableValues;
			}
			else if(list[i] is GameObject) {
				gameObjectList.Add((GameObject)list[i]);
				list[i] = new IndexedSerializationContainer<GameObject>(gameObjectList.Count - 1);
			}
			else if(list[i] is AudioClip) {
				audioClipList.Add((AudioClip)list[i]);
				list[i] = new IndexedSerializationContainer<AudioClip>(audioClipList.Count - 1);
			}
			else if(list[i] is AudioSource) {
				audioSourceList.Add((AudioSource)list[i]);
				list[i] = new IndexedSerializationContainer<AudioSource>(audioSourceList.Count - 1);
			}
			else if(list[i] is Color) {
				list[i] = new ColorSerializationContainer((Color)list[i]);
			}
		}
		
		transforms = transformList.ToArray();
		gameObjects = gameObjectList.ToArray();
		audioClips = audioClipList.ToArray();
		audioSources = audioSourceList.ToArray();
	}
	
	void RestoreNonSerializableTypes(object[] list) {
		for(var i = 0; i < list.Length; ++i) {
			if(null == list[i]) { continue; }
			
			if(list[i] is Vector3SerializationContainer) {
				list[i] = ((Vector3SerializationContainer)list[i]).ToVector3();
			}
			else if(list[i] is Vector3SerializationContainer[]) {
				var serializedValues = (Vector3SerializationContainer[])list[i];
				var originalValues = new Vector3[serializedValues.Length];
				for(var i2 = 0; i2 < serializedValues.Length; ++i2) {
					originalValues[i2] = serializedValues[i2].ToVector3();
				}
				list[i] = originalValues;
			}
			else if(list[i] is IndexedSerializationContainer<Transform>[]) {
				var serializedValues = (IndexedSerializationContainer<Transform>[])list[i];
				var originalValues = new Transform[serializedValues.Length];
				for(var i2 = 0; i2 < serializedValues.Length; ++i2) {
					originalValues[i2] = transforms[((IndexedSerializationContainer<Transform>)serializedValues[i2]).Index()];
				}
				list[i] = originalValues;
			}
			else if(list[i] is IndexedSerializationContainer<Transform>) {
				list[i] = transforms[((IndexedSerializationContainer<Transform>)list[i]).Index()];
			}
			else if(list[i] is IndexedSerializationContainer<GameObject>) {
				list[i] = gameObjects[((IndexedSerializationContainer<GameObject>)list[i]).Index()];
			}
			else if(list[i] is IndexedSerializationContainer<AudioClip>) {
				list[i] = audioClips[((IndexedSerializationContainer<AudioClip>)list[i]).Index()];
			}
			else if(list[i] is IndexedSerializationContainer<AudioSource>) {
				list[i] = audioSources[((IndexedSerializationContainer<AudioSource>)list[i]).Index()];
			}
			else if(list[i] is ColorSerializationContainer) {
				list[i] = ((ColorSerializationContainer)list[i]).ToColor();
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