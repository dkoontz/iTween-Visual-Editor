/*
Permission is hereby granted, free of charge, to any person  obtaining a copy of this software and associated documentation  files (the "Software"), to deal in the Software without  restriction, including without limitation the rights to use,  copy, modify, merge, publish, distribute, sublicense, and/or sell  copies of the Software, and to permit persons to whom the  Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

/*
TERMS OF USE - EASING EQUATIONS
Open source under the BSD License.
Copyright (c)2001 Robert Penner
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

#region Namespaces
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
#endregion

/// <summary>
/// <para>Version: 2.0.00</para>	 
/// <para>Author: Bob Berkebile (http://pixelplacement.com)</para>
/// <para>Support: http://itween.pixelplacement.com</para>
/// </summary>
public class iTween : MonoBehaviour{
	
	#region Variables
	
	//repository of all living iTweens:
	public static ArrayList tweens = new ArrayList();
	
	//camera fade object:
	private static GameObject cameraFade;
	
	//status members (made public for visual troubleshooting in the inspector):
	public string id, type, method;
	public iTween.EaseType easeType;
	public float time, delay;
	public LoopType loopType;
	public bool isRunning,isPaused;
		
	//private members:
 	private float runningTime, percentage;
	private float delayStarted; //probably not neccesary that this be protected but it shuts Unity's compiler up about this being "never used"
	private bool kinematic, isLocal, loop, reverse, wasPaused;
	private Hashtable tweenArguments;
	private Space space;
	private delegate float EasingFunction(float start, float end, float value);
	private delegate void ApplyTween();
	private EasingFunction ease;
	private ApplyTween apply;
	private AudioSource audioSource;
	private Vector3[] vector3s;
	private Vector2[] vector2s;
	private Color[] colors;
	private float[] floats;
	//private int[] ints;
	private Rect[] rects;
	private CRSpline path;
	
	/// <summary>
	/// The type of easing to use based on Robert Penner's open source easing equations (http://www.robertpenner.com/easing_terms_of_use.html).
	/// </summary>
	public enum EaseType{
		easeInQuad,
		easeOutQuad,
		easeInOutQuad,
		easeInCubic,
		easeOutCubic,
		easeInOutCubic,
		easeInQuart,
		easeOutQuart,
		easeInOutQuart,
		easeInQuint,
		easeOutQuint,
		easeInOutQuint,
		easeInSine,
		easeOutSine,
		easeInOutSine,
		easeInExpo,
		easeOutExpo,
		easeInOutExpo,
		easeInCirc,
		easeOutCirc,
		easeInOutCirc,
		linear,
		spring,
		bounce,
		easeInBack,
		easeOutBack,
		easeInOutBack,
		punch
	}
	
	/// <summary>
	/// The type of loop (if any) to use.  
	/// </summary>
	public enum LoopType{
		/// <summary>
		/// Do not loop.
		/// </summary>
		none,
		/// <summary>
		/// Rewind and replay.
		/// </summary>
		loop,
		/// <summary>
		/// Ping pong the animation back and forth.
		/// </summary>
		pingPong
	}
			
	#endregion
	
	#region Defaults
	
	/// <summary>
	/// A collection of baseline presets that iTween needs and utilizes if certain parameters are not provided. 
	/// </summary>
	public static class Defaults{
		//general defaults:
		public static float time = 1f;
		public static float delay = 0f;	
		public static LoopType loopType = LoopType.none;
		public static EaseType easeType = iTween.EaseType.easeOutExpo;
		public static float lookSpeed = 3f;
		public static bool isLocal = false;
		public static Space space = Space.Self;
		public static bool orientToPath = false;
		//update defaults:
		public static float updateTimePercentage = .05f;
		public static float updateTime = 1f*updateTimePercentage;
		//cameraFade defaults:
		public static int cameraFadeDepth = 999999;
		//path look ahead amount:
		public static float lookAhead = .05f;
	}
	
	#endregion
	
	#region #1 Static Registers
		
	/// <summary>
	/// Instantly changes the color of a camera fade and then returns it back over time with MINIMUM customization options.
	/// </summary>
	/// <param name="color">
	/// A <see cref="Color"/> to fade the screen to.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void CameraFadeFrom(Color color, float time){
		CameraFadeFrom(Hash("color",color,"time",time));
	}
	
	/// <summary>
	/// Instantly changes the color of a camera fade and then returns it back over time with FULL customization options.
	/// </summary>
	/// <param name="color">
	/// A <see cref="Color"/> to fade the screen to.
	/// </param>
	/// <param name="r">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color red.
	/// </param>
	/// <param name="g">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color green.
	/// </param>
	/// <param name="b">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color blue.
	/// </param>
	/// <param name="a">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the alpha.
	/// </param> 
	/// <param name="alpha">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the alpha.
	/// </param>
	/// <param name="amount">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the alpha.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void CameraFadeFrom(Hashtable args){
		CameraFadeAdd(Defaults.cameraFadeDepth);
		
		//rescale cameraFade just in case screen size has changed to ensure it takes up the full screen:
		cameraFade.guiTexture.pixelInset=new Rect(0,0,Screen.width,Screen.height);
		
		//establish iTween:
		ColorFrom(cameraFade,args);
	}	
	
	/// <summary>
	/// Changes the color of a camera over time with MINIMUM customization options.
	/// </summary>
	/// <param name="color">
	/// A <see cref="Color"/> to fade the screen to.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void CameraFadeTo(Color color, float time){
		CameraFadeTo(Hash("color",color,"time",time));
	}	
	
	/// <summary>
	/// Changes the color of a camera over time with FULL customization options.
	/// </summary>
	/// <param name="color">
	/// A <see cref="Color"/> to fade the screen to.
	/// </param>
	/// <param name="r">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color red.
	/// </param>
	/// <param name="g">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color green.
	/// </param>
	/// <param name="b">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color green.
	/// </param>
	/// <param name="a">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the alpha.
	/// </param> 
	/// <param name="alpha">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the alpha.
	/// </param>
	/// <param name="amount">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the alpha.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void CameraFadeTo(Hashtable args){
		CameraFadeAdd(Defaults.cameraFadeDepth);
		
		//rescale cameraFade just in case screen size has changed to ensure it takes up the full screen:
		cameraFade.guiTexture.pixelInset=new Rect(0,0,Screen.width,Screen.height);
		
		//establish iTween:
		ColorTo(cameraFade,args);
	}	
	
	/// <summary>
	/// Returns a value to an 'oncallback' method interpolated between the supplied 'from' and 'to' values for application as desired.  Requires an 'onupdate' callback that accepts the same type as the supplied 'from' and 'to' properties.
	/// </summary>
	/// <param name="from">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> or <see cref="Vector3"/> or <see cref="Vector2"/> or <see cref="Color"/> or <see cref="Rect"/> for the starting value.
	/// </param> 
	/// <param name="to">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> or <see cref="Vector3"/> or <see cref="Vector2"/> or <see cref="Color"/> or <see cref="Rect"/> for the ending value.
	/// </param> 
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ValueTo(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		if (!args.Contains("onupdate") || !args.Contains("from") || !args.Contains("to")) {
			Debug.LogError("iTween Error: ValueTo() requires an 'onupdate' callback function and a 'from' and 'to' property.  The supplied 'onupdate' callback must accept a single argument that is the same type as the supplied 'from' and 'to' properties!");
			return;
		}else{
			//establish iTween:
			args["type"]="value";
			
			if (args["from"].GetType() == typeof(Vector2)) {
				args["method"]="vector2";
			}else if (args["from"].GetType() == typeof(Vector3)) {
				args["method"]="vector3";
			}else if (args["from"].GetType() == typeof(Rect)) {
				args["method"]="rect";
			}else if (args["from"].GetType() == typeof(Single)) {
				args["method"]="float";
			}else if (args["from"].GetType() == typeof(Color)) {
				args["method"]="color";
			}else{
				Debug.LogError("iTween Error: ValueTo() only works with interpolating Vector3s, Vector2s, floats, ints, Rects and Colors!");
				return;	
			}
			
			//set a default easeType of linear if none is supplied since eased color interpolation is nearly unrecognizable:
			if (!args.Contains("easetype")) {
				args.Add("easetype",EaseType.linear);
			}
			
			Launch(target,args);
		}
	}
	
	/// <summary>
	/// Changes a GameObject's alpha value instantly then returns it to the provided alpha over time with MINIMUM customization options.  If a GUIText or GUITexture component is attached, it will become the target of the animation. Identical to using ColorFrom and using the "a" parameter. 
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="alpha">
	/// A <see cref="System.Single"/> for the final alpha value of the animation.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void FadeFrom(GameObject target, float alpha, float time){
		FadeFrom(target,Hash("alpha",alpha,"time",time));
	}
	
	/// <summary>
	/// Changes a GameObject's alpha value instantly then returns it to the provided alpha over time with FULL customization options.  If a GUIText or GUITexture component is attached, it will become the target of the animation. Identical to using ColorFrom and using the "a" parameter.
	/// </summary>
	/// <param name="alpha">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the initial alpha value of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the initial alpha value of the animation.
	/// </param>
	/// <param name="includechildren">
	/// A <see cref="System.Boolean"/> for whether or not to include children of this GameObject. True by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void FadeFrom(GameObject target, Hashtable args){	
		ColorFrom(target,args);
	}		
	
	/// <summary>
	/// Changes a GameObject's alpha value over time with MINIMUM customization options.  If a GUIText or GUITexture component is attached, it will become the target of the animation. Identical to using ColorTo and using the "a" parameter.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="alpha">
	/// A <see cref="System.Single"/> for the final alpha value of the animation.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void FadeTo(GameObject target, float alpha, float time){
		FadeTo(target,Hash("alpha",alpha,"time",time));
	}	

	/// <summary>
	/// Changes a GameObject's alpha value over time with FULL customization options.  If a GUIText or GUITexture component is attached, it will become the target of the animation. Identical to using ColorTo and using the "a" parameter.
	/// </summary>
	/// <param name="alpha">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the final alpha value of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the final alpha value of the animation.
	/// </param>
	/// <param name="includechildren">
	/// A <see cref="System.Boolean"/> for whether or not to include children of this GameObject. True by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void FadeTo(GameObject target, Hashtable args){
		ColorTo(target,args);
	}		
	
	/// <summary>
	/// Changes a GameObject's color values instantly then returns them to the provided properties over time with MINIMUM customization options.  If a GUIText or GUITexture component is attached, it will become the target of the animation.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/> to change the GameObject's color to.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ColorFrom(GameObject target, Color color, float time){
		ColorFrom(target,Hash("color",color,"time",time));
	}
	
	/// <summary>
	/// Changes a GameObject's color values instantly then returns them to the provided properties over time with FULL customization options.  If a GUIText or GUITexture component is attached, it will become the target of the animation.
	/// </summary>
	/// <param name="color">
	/// A <see cref="Color"/> to change the GameObject's color to.
	/// </param>
	/// <param name="r">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color red.
	/// </param>
	/// <param name="g">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color green.
	/// </param>
	/// <param name="b">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color green.
	/// </param>
	/// <param name="a">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the alpha.
	/// </param> 
	/// <param name="includechildren">
	/// A <see cref="System.Boolean"/> for whether or not to include children of this GameObject. True by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ColorFrom(GameObject target, Hashtable args){	
		Color fromColor = new Color();
		Color tempColor = new Color();
		
		//clean args:
		args = iTween.CleanArgs(args);
		
		//handle children:
		if(!args.Contains("includechildren") || (bool)args["includechildren"]){
			foreach(Transform child in target.transform){
				Hashtable argsCopy = (Hashtable)args.Clone();
				argsCopy["ischild"]=true;
				ColorFrom(child.gameObject,argsCopy);
			}
		}
		
		//set a default easeType of linear if none is supplied since eased color interpolation is nearly unrecognizable:
		if (!args.Contains("easetype")) {
			args.Add("easetype",EaseType.linear);
		}
		
		//set tempColor and base fromColor:
		if(target.GetComponent(typeof(GUITexture))){
			tempColor=fromColor=target.guiTexture.color;	
		}else if(target.GetComponent(typeof(GUIText))){
			tempColor=fromColor=target.guiText.material.color;
		}else if(target.renderer){
			tempColor=fromColor=target.renderer.material.color;
		}else if(target.light){
			tempColor=fromColor=target.light.color;
		}
		
		//set augmented fromColor:
		if(args.Contains("color")){
			fromColor=(Color)args["color"];
		}else{
			if (args.Contains("r")) {
				fromColor.r=(float)args["r"];
			}
			if (args.Contains("g")) {
				fromColor.g=(float)args["g"];
			}
			if (args.Contains("b")) {
				fromColor.b=(float)args["b"];
			}
			if (args.Contains("a")) {
				fromColor.a=(float)args["a"];
			}
		}
		
		//alpha or amount?
		if(args.Contains("amount")){
			fromColor.a=(float)args["amount"];
			args.Remove("amount");
		}else if(args.Contains("alpha")){
			fromColor.a=(float)args["alpha"];
			args.Remove("alpha");
		}
		
		//apply fromColor:
		if(target.GetComponent(typeof(GUITexture))){
			target.guiTexture.color=fromColor;	
		}else if(target.GetComponent(typeof(GUIText))){
			target.guiText.material.color=fromColor;
		}else if(target.renderer){
			target.renderer.material.color=fromColor;
		}else if(target.light){
			target.light.color=fromColor;
		}
		
		//set new color arg:
		args["color"]=tempColor;
		
		//establish iTween:
		args["type"]="color";
		args["method"]="to";
		Launch(target,args);
	}		
	
	/// <summary>
	/// Changes a GameObject's color values over time with MINIMUM customization options.  If a GUIText or GUITexture component is attached, they will become the target of the animation.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/> to change the GameObject's color to.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ColorTo(GameObject target, Color color, float time){
		ColorTo(target,Hash("color",color,"time",time));
	}
	
	/// <summary>
	/// Changes a GameObject's color values over time with FULL customization options.  If a GUIText or GUITexture component is attached, they will become the target of the animation.
	/// </summary>
	/// <param name="color">
	/// A <see cref="Color"/> to change the GameObject's color to.
	/// </param>
	/// <param name="r">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color red.
	/// </param>
	/// <param name="g">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color green.
	/// </param>
	/// <param name="b">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color green.
	/// </param>
	/// <param name="a">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the alpha.
	/// </param> 
	/// <param name="includechildren">
	/// A <see cref="System.Boolean"/> for whether or not to include children of this GameObject. True by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ColorTo(GameObject target, Hashtable args){	
		//clean args:
		args = iTween.CleanArgs(args);
		
		//handle children:
		if(!args.Contains("includechildren") || (bool)args["includechildren"]){
			foreach(Transform child in target.transform){
				Hashtable argsCopy = (Hashtable)args.Clone();
				argsCopy["ischild"]=true;
				ColorTo(child.gameObject,argsCopy);
			}
		}
		
		//set a default easeType of linear if none is supplied since eased color interpolation is nearly unrecognizable:
		if (!args.Contains("easetype")) {
			args.Add("easetype",EaseType.linear);
		}
		
		//establish iTween:
		args["type"]="color";
		args["method"]="to";
		Launch(target,args);
	}	
	
	/// <summary>
	/// Instantly changes an AudioSource's volume and pitch then returns it to it's starting volume and pitch over time with MINIMUM customization options. Default AudioSource attached to GameObject will be used (if one exists) if not supplied.
	/// </summary>
	/// <param name="target"> 
	/// A <see cref="GameObject"/> to be the target of the animation which holds the AudioSource to be changed.
	/// </param>
	/// <param name="volume"> for the target level of volume.
	/// A <see cref="System.Single"/>
	/// </param>
	/// <param name="pitch"> for the target pitch.
	/// A <see cref="System.Single"/>
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void AudioFrom(GameObject target, float volume, float pitch, float time){
		AudioFrom(target,Hash("volume",volume,"pitch",pitch,"time",time));
	}
	
	/// <summary>
	/// Instantly changes an AudioSource's volume and pitch then returns it to it's starting volume and pitch over time with FULL customization options. Default AudioSource attached to GameObject will be used (if one exists) if not supplied. 
	/// </summary>
	/// <param name="audiosource">
	/// A <see cref="AudioSource"/> for which AudioSource to use.
	/// </param> 
	/// <param name="volume">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the target level of volume.
	/// </param>
	/// <param name="pitch">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the target pitch.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void AudioFrom(GameObject target, Hashtable args){
		Vector2 tempAudioProperties;
		Vector2 fromAudioProperties;
		AudioSource tempAudioSource;
		
		//clean args:
		args = iTween.CleanArgs(args);
		
		//set tempAudioSource:
		if(args.Contains("audiosource")){
			tempAudioSource=(AudioSource)args["audiosource"];
		}else{
			if(target.GetComponent(typeof(AudioSource))){
				tempAudioSource=target.audio;
			}else{
				//throw error if no AudioSource is available:
				Debug.LogError("iTween Error: AudioFrom requires an AudioSource.");
				return;
			}
		}			
		
		//set tempAudioProperties:
		tempAudioProperties.x=fromAudioProperties.x=tempAudioSource.volume;
		tempAudioProperties.y=fromAudioProperties.y=tempAudioSource.pitch;
		
		//set augmented fromAudioProperties:
		if(args.Contains("volume")){
			fromAudioProperties.x=(float)args["volume"];
		}
		if(args.Contains("pitch")){
			fromAudioProperties.y=(float)args["pitch"];
		}
		
		//apply fromAudioProperties:
		tempAudioSource.volume=fromAudioProperties.x;
		tempAudioSource.pitch=fromAudioProperties.y;
				
		//set new volume and pitch args:
		args["volume"]=tempAudioProperties.x;
		args["pitch"]=tempAudioProperties.y;
		
		//set a default easeType of linear if none is supplied since eased audio interpolation is nearly unrecognizable:
		if (!args.Contains("easetype")) {
			args.Add("easetype",EaseType.linear);
		}
		
		//establish iTween:
		args["type"]="audio";
		args["method"]="to";
		Launch(target,args);			
	}		

	/// <summary>
	/// Fades volume and pitch of an AudioSource with MINIMUM customization options.  Default AudioSource attached to GameObject will be used (if one exists) if not supplied. 
	/// </summary>
	/// <param name="target"> 
	/// A <see cref="GameObject"/> to be the target of the animation which holds the AudioSource to be changed.
	/// </param>
	/// <param name="volume"> for the target level of volume.
	/// A <see cref="System.Single"/>
	/// </param>
	/// <param name="pitch"> for the target pitch.
	/// A <see cref="System.Single"/>
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void AudioTo(GameObject target, float volume, float pitch, float time){
		AudioTo(target,Hash("volume",volume,"pitch",pitch,"time",time));
	}
	
	/// <summary>
	/// Fades volume and pitch of an AudioSource with FULL customization options.  Default AudioSource attached to GameObject will be used (if one exists) if not supplied. 
	/// </summary>
	/// <param name="audiosource">
	/// A <see cref="AudioSource"/> for which AudioSource to use.
	/// </param> 
	/// <param name="volume">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the target level of volume.
	/// </param>
	/// <param name="pitch">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the target pitch.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void AudioTo(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//set a default easeType of linear if none is supplied since eased audio interpolation is nearly unrecognizable:
		if (!args.Contains("easetype")) {
			args.Add("easetype",EaseType.linear);
		}
		
		//establish iTween:
		args["type"]="audio";
		args["method"]="to";
		Launch(target,args);			
	}	
	
	/// <summary>
	/// Plays an AudioClip once based on supplied volume and pitch and following any delay with MINIMUM customization options. AudioSource is optional as iTween will provide one.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation which holds the AudioSource to be utilized.
	/// </param>
	/// <param name="audioclip">
	/// A <see cref="AudioClip"/> for a reference to the AudioClip to be played.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> for the time in seconds the action will wait before beginning.
	/// </param>
	public static void Stab(GameObject target, AudioClip audioclip, float delay){
		Stab(target,Hash("audioclip",audioclip,"delay",delay));
	}
	
	/// <summary>
	/// Plays an AudioClip once based on supplied volume and pitch and following any delay with FULL customization options. AudioSource is optional as iTween will provide one.
	/// </summary>
	/// <param name="audioclip">
	/// A <see cref="AudioClip"/> for a reference to the AudioClip to be played.
	/// </param> 
	/// <param name="audiosource">
	/// A <see cref="AudioSource"/> for which AudioSource to use
	/// </param> 
	/// <param name="volume">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the target level of volume.
	/// </param>
	/// <param name="pitch">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the target pitch.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the action will wait before beginning.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void Stab(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween:
		args["type"]="stab";
		Launch(target,args);			
	}
	
	/// <summary>
	/// Instantly rotates a GameObject to look at the supplied Vector3 then returns it to it's starting rotation over time with MINIMUM customization options. 
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> to be the Vector3 that the target will look towards.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void LookFrom(GameObject target, Vector3 looktarget, float time){
		LookFrom(target,Hash("looktarget",looktarget,"time",time));
	}	
	
	/// <summary>
	/// Instantly rotates a GameObject to look at a supplied Transform or Vector3 then returns it to it's starting rotation over time with FULL customization options. 
	/// </summary>
	/// <param name="looktarget">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="axis">
	/// A <see cref="System.String"/>. Restricts rotation to the supplied axis only.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void LookFrom(GameObject target, Hashtable args){
		Vector3 tempRotation;
		Vector3 tempRestriction;
		
		//clean args:
		args = iTween.CleanArgs(args);
		
		//set look:
		tempRotation=target.transform.eulerAngles;
		if (args["looktarget"].GetType() == typeof(Transform)) {
			target.transform.LookAt((Transform)args["looktarget"]);
		}else if(args["looktarget"].GetType() == typeof(Vector3)){
			target.transform.LookAt((Vector3)args["looktarget"]);
		}
		
		//axis restriction:
		if(args.Contains("axis")){
			tempRestriction=target.transform.eulerAngles;
			switch((string)args["axis"]){
				case "x":
				 	tempRestriction.y=tempRotation.y;
					tempRestriction.z=tempRotation.z;
				break;
				case "y":
					tempRestriction.x=tempRotation.x;
					tempRestriction.z=tempRotation.z;
				break;
				case "z":
					tempRestriction.x=tempRotation.x;
					tempRestriction.y=tempRotation.y;
				break;
			}
			target.transform.eulerAngles=tempRestriction;
		}		
		
		//set new rotation:
		args["rotation"] = tempRotation;
		
		//establish iTween
		args["type"]="rotate";
		args["method"]="to";
		Launch(target,args);
	}		
	
	/// <summary>
	/// Rotates a GameObject to look at the supplied Vector3 over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> to be the Vector3 that the target will look towards.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void LookTo(GameObject target, Vector3 looktarget, float time){
		LookTo(target,Hash("looktarget",looktarget,"time",time));
	}
	
	/// <summary>
	/// Rotates a GameObject to look at a supplied Transform or Vector3 over time with FULL customization options.
	/// </summary>
	/// <param name="looktarget">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="axis">
	/// A <see cref="System.String"/>. Restricts rotation to the supplied axis only.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void LookTo(GameObject target, Hashtable args){		
		//clean args:
		args = iTween.CleanArgs(args);			
		
		//additional property to ensure ConflictCheck can work correctly since Transforms are refrences:		
		if(args.Contains("looktarget")){
			if (args["looktarget"].GetType() == typeof(Transform)) {
				Transform transform = (Transform)args["looktarget"];
				args["position"]=new Vector3(transform.position.x,transform.position.y,transform.position.z);
				args["rotation"]=new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z);
			}
		}
		
		//establish iTween
		args["type"]="look";
		args["method"]="to";
		Launch(target,args);
	}		
	
	/// <summary>
	/// Changes a GameObject's position over time to a supplied destination with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="position">
	/// A <see cref="Vector3"/> for the destination Vector3.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void MoveTo(GameObject target, Vector3 position, float time){
		MoveTo(target,Hash("position",position,"time",time));
	}	
		
	/// <summary>
	/// Changes a GameObject's position over time to a supplied destination with FULL customization options.
	/// </summary>
	/// <param name="position">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for a point in space the GameObject will animate to.
	/// </param>
	/// <param name="path">
	/// A <see cref="Transform[]"/> or <see cref="Vector3[]"/> for a list of points to draw a Catmull-Rom through for a curved animation path.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="orienttopath">
	/// A <see cref="System.Boolean"/> for whether or not the GameObject will orient to its direction of travel.  False by default.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="looktime">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the object will take to look at either the "looktarget" or "orienttopath".
	/// </param>
	/// <param name="lookahead">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for how much of a percentage to look ahead on a path to influence how strict "orientopath" is.
	/// </param>
	/// <param name="axis">
	/// A <see cref="System.String"/>. Restricts rotation to the supplied axis only.
	/// </param>
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False be default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void MoveTo(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//additional property to ensure ConflictCheck can work correctly since Transforms are refrences:		
		if(args.Contains("position")){
			if (args["position"].GetType() == typeof(Transform)) {
				Transform transform = (Transform)args["position"];
				args["position"]=new Vector3(transform.position.x,transform.position.y,transform.position.z);
				args["rotation"]=new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z);
				args["scale"]=new Vector3(transform.localScale.x,transform.localScale.y,transform.localScale.z);
			}
		}		
		
		//establish iTween:
		args["type"]="move";
		args["method"]="to";
		Launch(target,args);
	}
		
	/// <summary>
	/// Instantly changes a GameObject's position to a supplied destination then returns it to it's starting position over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="position">
	/// A <see cref="Vector3"/> for the destination Vector3.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void MoveFrom(GameObject target, Vector3 position, float time){
		MoveFrom(target,Hash("position",position,"time",time));
	}		
	
	/// <summary>
	/// Instantly changes a GameObject's position to a supplied destination then returns it to it's starting position over time with FULL customization options.
	/// </summary>
	/// <param name="position">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for a point in space the GameObject will animate to.
	/// </param>
	/// <param name="path">
	/// A <see cref="Transform[]"/> or <see cref="Vector3[]"/> for a list of points to draw a Catmull-Rom through for a curved animation path.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="orienttopath">
	/// A <see cref="System.Boolean"/> for whether or not the GameObject will orient to its direction of travel.  False by default.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="looktime">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the object will take to look at either the "looktarget" or "orienttopath".
	/// </param>
	/// <param name="lookahead">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for how much of a percentage to look ahead on a path to influence how strict "orientopath" is.
	/// </param>
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False be default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void MoveFrom(GameObject target, Hashtable args){
		//clean args:
			args = iTween.CleanArgs(args);
		
		bool tempIsLocal;
		
		//set tempIsLocal:
		if(args.Contains("islocal")){
			tempIsLocal = (bool)args["islocal"];
		}else{
			tempIsLocal = Defaults.isLocal;	
		}
		
		if(args.Contains("path")){
			Vector3[] fromPath;
			Vector3[] suppliedPath;
			if(args["path"].GetType() == typeof(Vector3[])){
				Vector3[] temp = (Vector3[])args["path"];
				suppliedPath=new Vector3[temp.Length];
				Array.Copy(temp,suppliedPath, temp.Length);	
			}else{
				Transform[] temp = (Transform[])args["path"];
				suppliedPath = new Vector3[temp.Length];
				for (int i = 0; i < temp.Length; i++) {
					suppliedPath[i]=temp[i].position;
				}
			}
			if(suppliedPath[suppliedPath.Length-1] != target.transform.position){
				fromPath= new Vector3[suppliedPath.Length+1];
				Array.Copy(suppliedPath,fromPath,suppliedPath.Length);
				if(tempIsLocal){
					fromPath[fromPath.Length-1] = target.transform.localPosition;
					target.transform.localPosition=fromPath[0];
				}else{
					fromPath[fromPath.Length-1] = target.transform.position;
					target.transform.position=fromPath[0];
				}
				args["path"]=fromPath;
			}else{
				if(tempIsLocal){
					target.transform.localPosition=suppliedPath[0];
				}else{
					target.transform.position=suppliedPath[0];
				}
				args["path"]=suppliedPath;
			}
		}else{
			Vector3 tempPosition;
			Vector3 fromPosition;
			
			//set tempPosition and base fromPosition:
			if(tempIsLocal){
				tempPosition=fromPosition=target.transform.localPosition;
			}else{
				tempPosition=fromPosition=target.transform.position;	
			}
			
			//set augmented fromPosition:
			if(args.Contains("position")){
				if (args["position"].GetType() == typeof(Transform)){
					Transform trans = (Transform)args["position"];
					fromPosition=trans.position;
				}else if(args["position"].GetType() == typeof(Vector3)){
					fromPosition=(Vector3)args["position"];
				}			
			}else{
				if (args.Contains("x")) {
					fromPosition.x=(float)args["x"];
				}
				if (args.Contains("y")) {
					fromPosition.y=(float)args["y"];
				}
				if (args.Contains("z")) {
					fromPosition.z=(float)args["z"];
				}
			}
			
			//apply fromPosition:
			if(tempIsLocal){
				target.transform.localPosition = fromPosition;
			}else{
				target.transform.position = fromPosition;	
			}
			
			//set new position arg:
			args["position"]=tempPosition;
		}
			
		//establish iTween:
		args["type"]="move";
		args["method"]="to";
		Launch(target,args);
	}
		
	/// <summary>
	/// Translates a GameObject's position over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount of change in position to move the GameObject.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void MoveAdd(GameObject target, Vector3 amount, float time){
		MoveAdd(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Translates a GameObject's position over time with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount of change in position to move the GameObject.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="orienttopath">
	/// A <see cref="System.Boolean"/> for whether or not the GameObject will orient to its direction of travel.  False by default.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="looktime">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the object will take to look at either the "looktarget" or "orienttopath".
	/// </param>
	/// <param name="axis">
	/// A <see cref="System.String"/>. Restricts rotation to the supplied axis only.
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/> for applying the transformation in either the world coordinate or local cordinate system. Defaults to local space.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void MoveAdd(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween:
		args["type"]="move";
		args["method"]="add";
		Launch(target,args);
	}
	
	/// <summary>
	/// Adds the supplied coordinates to a GameObject's postion with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount of change in position to move the GameObject.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void MoveBy(GameObject target, Vector3 amount, float time){
		MoveBy(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Adds the supplied coordinates to a GameObject's position with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount of change in position to move the GameObject.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="orienttopath">
	/// A <see cref="System.Boolean"/> for whether or not the GameObject will orient to its direction of travel.  False by default.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="looktime">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the object will take to look at either the "looktarget" or "orienttopath".
	/// </param>
	/// <param name="axis">
	/// A <see cref="System.String"/>. Restricts rotation to the supplied axis only.
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/> for applying the transformation in either the world coordinate or local cordinate system. Defaults to local space.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void MoveBy(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween:
		args["type"]="move";
		args["method"]="by";
		Launch(target,args);
	}
	
	/// <summary>
	/// Changes a GameObject's scale over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="scale">
	/// A <see cref="Vector3"/> for the final scale.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ScaleTo(GameObject target, Vector3 scale, float time){
		ScaleTo(target,Hash("scale",scale,"time",time));
	}
	
	/// <summary>
	/// Changes a GameObject's scale over time with FULL customization options.
	/// </summary>
	/// <param name="scale">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for the final scale.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ScaleTo(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//additional property to ensure ConflictCheck can work correctly since Transforms are refrences:		
		if(args.Contains("scale")){
			if (args["scale"].GetType() == typeof(Transform)) {
				Transform transform = (Transform)args["scale"];
				args["position"]=new Vector3(transform.position.x,transform.position.y,transform.position.z);
				args["rotation"]=new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z);
				args["scale"]=new Vector3(transform.localScale.x,transform.localScale.y,transform.localScale.z);
			}
		}
		
		//establish iTween:
		args["type"]="scale";
		args["method"]="to";
		Launch(target,args);
	}
	
	/// <summary>
	/// Instantly changes a GameObject's scale then returns it to it's starting scale over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="scale">
	/// A <see cref="Vector3"/> for the final scale.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ScaleFrom(GameObject target, Vector3 scale, float time){
		ScaleFrom(target,Hash("scale",scale,"time",time));
	}
	
	/// <summary>
	/// Instantly changes a GameObject's scale then returns it to it's starting scale over time with FULL customization options.
	/// </summary>
	/// <param name="scale">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for the final scale.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ScaleFrom(GameObject target, Hashtable args){
		Vector3 tempScale;
		Vector3 fromScale;
	
		//clean args:
		args = iTween.CleanArgs(args);
		
		//set base fromScale:
		tempScale=fromScale=target.transform.localScale;
		
		//set augmented fromScale:
		if(args.Contains("scale")){
			if (args["scale"].GetType() == typeof(Transform)){
				Transform trans = (Transform)args["scale"];
				fromScale=trans.localScale;
			}else if(args["scale"].GetType() == typeof(Vector3)){
				fromScale=(Vector3)args["scale"];
			}	
		}else{
			if (args.Contains("x")) {
				fromScale.x=(float)args["x"];
			}
			if (args.Contains("y")) {
				fromScale.y=(float)args["y"];
			}
			if (args.Contains("z")) {
				fromScale.z=(float)args["z"];
			}
		}
		
		//apply fromScale:
		target.transform.localScale = fromScale;	
		
		//set new scale arg:
		args["scale"]=tempScale;
		
		//establish iTween:
		args["type"]="scale";
		args["method"]="to";
		Launch(target,args);
	}
	
	/// <summary>
	/// Adds to a GameObject's scale over time with FULL customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount of scale to be added to the GameObject's current scale.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ScaleAdd(GameObject target, Vector3 amount, float time){
		ScaleAdd(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Adds to a GameObject's scale over time with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount to be added to the GameObject's current scale.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ScaleAdd(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween:
		args["type"]="scale";
		args["method"]="add";
		Launch(target,args);
	}
	
	/// <summary>
	/// Multiplies a GameObject's scale over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount of scale to be multiplied by the GameObject's current scale.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ScaleBy(GameObject target, Vector3 amount, float time){
		ScaleBy(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Multiplies a GameObject's scale over time with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount to be multiplied to the GameObject's current scale.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ScaleBy(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween:
		args["type"]="scale";
		args["method"]="by";
		Launch(target,args);
	}
	
	/// <summary>
	/// Rotates a GameObject to the supplied Euler angles in degrees over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="rotation">
	/// A <see cref="Vector3"/> for the target Euler angles in degrees to rotate to.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void RotateTo(GameObject target, Vector3 rotation, float time){
		RotateTo(target,Hash("rotation",rotation,"time",time));
	}
	
	/// <summary>
	/// Rotates a GameObject to the supplied Euler angles in degrees over time with FULL customization options.
	/// </summary>
	/// <param name="rotation">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for the target Euler angles in degrees to rotate to.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False be default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void RotateTo(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//additional property to ensure ConflictCheck can work correctly since Transforms are refrences:		
		if(args.Contains("rotation")){
			if (args["rotation"].GetType() == typeof(Transform)) {
				Transform transform = (Transform)args["rotation"];
				args["position"]=new Vector3(transform.position.x,transform.position.y,transform.position.z);
				args["rotation"]=new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z);
				args["scale"]=new Vector3(transform.localScale.x,transform.localScale.y,transform.localScale.z);
			}
		}		
		
		//establish iTween
		args["type"]="rotate";
		args["method"]="to";
		Launch(target,args);
	}	
	
	/// <summary>
	/// Instantly changes a GameObject's Euler angles in degrees then returns it to it's starting rotation over time (if allowed) with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="rotation">
	/// A <see cref="Vector3"/> for the target Euler angles in degrees to rotate from.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void RotateFrom(GameObject target, Vector3 rotation, float time){
		RotateFrom(target,Hash("rotation",rotation,"time",time));
	}
	
	/// <summary>
	/// Instantly changes a GameObject's Euler angles in degrees then returns it to it's starting rotation over time (if allowed) with FULL customization options.
	/// </summary>
	/// <param name="rotation">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for the target Euler angles in degrees to rotate to.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False be default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void RotateFrom(GameObject target, Hashtable args){
		Vector3 tempRotation;
		Vector3 fromRotation;
		bool tempIsLocal;
	
		//clean args:
		args = iTween.CleanArgs(args);
		
		//set tempIsLocal:
		if(args.Contains("islocal")){
			tempIsLocal = (bool)args["islocal"];
		}else{
			tempIsLocal = Defaults.isLocal;	
		}

		//set tempRotation and base fromRotation:
		if(tempIsLocal){
			tempRotation=fromRotation=target.transform.localEulerAngles;
		}else{
			tempRotation=fromRotation=target.transform.eulerAngles;	
		}
		
		//set augmented fromRotation:
		if(args.Contains("rotation")){
			if (args["rotation"].GetType() == typeof(Transform)){
				Transform trans = (Transform)args["rotation"];
				fromRotation=trans.eulerAngles;
			}else if(args["rotation"].GetType() == typeof(Vector3)){
				fromRotation=(Vector3)args["rotation"];
			}	
		}else{
			if (args.Contains("x")) {
				fromRotation.x=(float)args["x"];
			}
			if (args.Contains("y")) {
				fromRotation.y=(float)args["y"];
			}
			if (args.Contains("z")) {
				fromRotation.z=(float)args["z"];
			}
		}
		
		//apply fromRotation:
		if(tempIsLocal){
			target.transform.localEulerAngles = fromRotation;
		}else{
			target.transform.eulerAngles = fromRotation;	
		}
		
		//set new rotation arg:
		args["rotation"]=tempRotation;
		
		//establish iTween:
		args["type"]="rotate";
		args["method"]="to";
		Launch(target,args);
	}	
	
	/// <summary>
	/// Adds supplied Euler angles in degrees to a GameObject's rotation over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount of Euler angles in degrees to add to the current rotation of the GameObject.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void RotateAdd(GameObject target, Vector3 amount, float time){
		RotateAdd(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Adds supplied Euler angles in degrees to a GameObject's rotation over time with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount of Euler angles in degrees to add to the current rotation of the GameObject.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/> for applying the transformation in either the world coordinate or local cordinate system. Defaults to local space.
	/// </param>

	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void RotateAdd(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween:
		args["type"]="rotate";
		args["method"]="add";
		Launch(target,args);
	}
	
	/// <summary>
	/// Multiplies supplied values by 360 and rotates a GameObject by calculated amount over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount to be multiplied by 360 to rotate the GameObject.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void RotateBy(GameObject target, Vector3 amount, float time){
		RotateBy(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Multiplies supplied values by 360 and rotates a GameObject by calculated amount over time with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount to be multiplied by 360 to rotate the GameObject.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/> for applying the transformation in either the world coordinate or local cordinate system. Defaults to local space.
	/// </param>
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False be default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void RotateBy(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="rotate";
		args["method"]="by";
		Launch(target,args);
	}		
	
	/// <summary>
	/// Randomly shakes a GameObject's position by a diminishing amount over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the magnitude of shake.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ShakePosition(GameObject target, Vector3 amount, float time){
		ShakePosition(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Randomly shakes a GameObject's position by a diminishing amount over time with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the magnitude of shake.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x magnitude.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y magnitude.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z magnitude.
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> for applying the transformation in either the world coordinate or local cordinate system. Defaults to local space.
	/// </param> 
	/// <param name="orienttopath">
	/// A <see cref="System.Boolean"/> for whether or not the GameObject will orient to its direction of travel.  False by default.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="looktime">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the object will take to look at either the "looktarget" or "orienttopath".
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ShakePosition(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="shake";
		args["method"]="position";
		Launch(target,args);
	}		
	
	/// <summary>
	/// Randomly shakes a GameObject's scale by a diminishing amount over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the magnitude of shake.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ShakeScale(GameObject target, Vector3 amount, float time){
		ShakeScale(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Randomly shakes a GameObject's scale by a diminishing amount over time with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the magnitude of shake.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x magnitude.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y magnitude.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z magnitude.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ShakeScale(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="shake";
		args["method"]="scale";
		Launch(target,args);
	}		
	
	/// <summary>
	/// Randomly shakes a GameObject's rotation by a diminishing amount over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the magnitude of shake.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ShakeRotation(GameObject target, Vector3 amount, float time){
		ShakeRotation(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Randomly shakes a GameObject's rotation by a diminishing amount over time with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the magnitude of shake.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x magnitude.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y magnitude.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z magnitude.
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> for applying the transformation in either the world coordinate or local cordinate system. Defaults to local space.
	/// </param> 
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ShakeRotation(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="shake";
		args["method"]="rotation";
		Launch(target,args);
	}			
	
	/// <summary>
	/// Applies a jolt of force to a GameObject's position and wobbles it back to its initial position with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the magnitude of the punch.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void PunchPosition(GameObject target, Vector3 amount, float time){
		PunchPosition(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Applies a jolt of force to a GameObject's position and wobbles it back to its initial position with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the magnitude of shake.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x magnitude.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y magnitude.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z magnitude.
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> for applying the transformation in either the world coordinate or local cordinate system. Defaults to local space.
	/// </param> 
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="looktime">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the object will take to look at either the "looktarget".
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void PunchPosition(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="punch";
		args["method"]="position";
		args["easetype"]=EaseType.punch;
		Launch(target,args);
	}		
	
	/// <summary>
	/// Applies a jolt of force to a GameObject's rotation and wobbles it back to its initial rotation with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the magnitude of the punch.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void PunchRotation(GameObject target, Vector3 amount, float time){
		PunchRotation(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Applies a jolt of force to a GameObject's rotation and wobbles it back to its initial rotation with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the magnitude of shake.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x magnitude.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y magnitude.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z magnitude.
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> for applying the transformation in either the world coordinate or local cordinate system. Defaults to local space.
	/// </param> 
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void PunchRotation(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="punch";
		args["method"]="rotation";
		args["easetype"]=EaseType.punch;
		Launch(target,args);
	}	
	
	/// <summary>
	/// Applies a jolt of force to a GameObject's scale and wobbles it back to its initial scale with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the magnitude of the punch.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void PunchScale(GameObject target, Vector3 amount, float time){
		PunchScale(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Applies a jolt of force to a GameObject's scale and wobbles it back to its initial scale with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the magnitude of shake.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x magnitude.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y magnitude.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z magnitude.
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> for applying the transformation in either the world coordinate or local cordinate system. Defaults to local space.
	/// </param> 
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void PunchScale(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="punch";
		args["method"]="scale";
		args["easetype"]=EaseType.punch;
		Launch(target,args);
	}	
	
	#endregion
	
	#region #2 Generate Method Targets
	
	//call correct set target method and set tween application delegate:
	void GenerateTargets(){
		switch (type) {
			case "value":
				switch (method) {
					case "float":
						GenerateFloatTargets();
						apply = new ApplyTween(ApplyFloatTargets);
					break;
				case "vector2":
						GenerateVector2Targets();
						apply = new ApplyTween(ApplyVector2Targets);
					break;
				case "vector3":
						GenerateVector3Targets();
						apply = new ApplyTween(ApplyVector3Targets);
					break;
				case "color":
						GenerateColorTargets();
						apply = new ApplyTween(ApplyColorTargets);
					break;
				case "rect":
						GenerateRectTargets();
						apply = new ApplyTween(ApplyRectTargets);
					break;
				}
			break;
			case "color":
				switch (method) {
					case "to":
						GenerateColorToTargets();
						apply = new ApplyTween(ApplyColorToTargets);
					break;
				}
			break;
			case "audio":
				switch (method) {
					case "to":
						GenerateAudioToTargets();
						apply = new ApplyTween(ApplyAudioToTargets);
					break;
				}
			break;
			case "move":
				switch (method) {
					case "to":
						//using a path?
						if(tweenArguments.Contains("path")){
							GenerateMoveToPathTargets();
							apply = new ApplyTween(ApplyMoveToPathTargets);
						}else{ //not using a path?
							GenerateMoveToTargets();
							apply = new ApplyTween(ApplyMoveToTargets);
						}
					break;
					case "by":
					case "add":
						GenerateMoveByTargets();
						apply = new ApplyTween(ApplyMoveByTargets);
					break;
				}
			break;
			case "scale":
				switch (method){
					case "to":
						GenerateScaleToTargets();
						apply = new ApplyTween(ApplyScaleToTargets);
					break;
					case "by":
						GenerateScaleByTargets();
						apply = new ApplyTween(ApplyScaleToTargets);
					break;
					case "add":
						GenerateScaleAddTargets();
						apply = new ApplyTween(ApplyScaleToTargets);
					break;
				}
			break;
			case "rotate":
				switch (method) {
					case "to":
						GenerateRotateToTargets();
						apply = new ApplyTween(ApplyRotateToTargets);
					break;
					case "add":
						GenerateRotateAddTargets();
						apply = new ApplyTween(ApplyRotateAddTargets);
					break;
					case "by":
						GenerateRotateByTargets();
						apply = new ApplyTween(ApplyRotateAddTargets);
					break;				
				}
			break;
			case "shake":
				switch (method) {
					case "position":
						GenerateShakePositionTargets();
						apply = new ApplyTween(ApplyShakePositionTargets);
					break;		
					case "scale":
						GenerateShakeScaleTargets();
						apply = new ApplyTween(ApplyShakeScaleTargets);
					break;
					case "rotation":
						GenerateShakeRotationTargets();
						apply = new ApplyTween(ApplyShakeRotationTargets);
					break;
				}
			break;			
			case "punch":
				switch (method) {
					case "position":
						GeneratePunchPositionTargets();
						apply = new ApplyTween(ApplyPunchPositionTargets);
					break;	
					case "rotation":
						GeneratePunchRotationTargets();
						apply = new ApplyTween(ApplyPunchRotationTargets);
					break;	
					case "scale":
						GeneratePunchScaleTargets();
						apply = new ApplyTween(ApplyPunchScaleTargets);
					break;
				}
			break;
			case "look":
				switch (method) {
					case "to":
						GenerateLookToTargets();
						apply = new ApplyTween(ApplyLookToTargets);
					break;	
				}
			break;	
			case "stab":
				GenerateStabTargets();
				apply = new ApplyTween(ApplyStabTargets);
			break;	
		}
	}
	
	#endregion
	
	#region #3 Generate Specific Targets
	
	void GenerateRectTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		rects=new Rect[3];
		
		//from and to values:
		rects[0]=(Rect)tweenArguments["from"];
		rects[1]=(Rect)tweenArguments["to"];
	}		
	
	void GenerateColorTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		colors=new Color[3];
		
		//from and to values:
		colors[0]=(Color)tweenArguments["from"];
		colors[1]=(Color)tweenArguments["to"];
	}	
	
	void GenerateVector3Targets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from and to values:
		vector3s[0]=(Vector3)tweenArguments["from"];
		vector3s[1]=(Vector3)tweenArguments["to"];
	}
	
	void GenerateVector2Targets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector2s=new Vector2[3];
		
		//from and to values:
		vector2s[0]=(Vector2)tweenArguments["from"];
		vector2s[1]=(Vector2)tweenArguments["to"];
	}
	
	void GenerateFloatTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		floats=new float[3];
		
		//from and to values:
		floats[0]=(float)tweenArguments["from"];
		floats[1]=(float)tweenArguments["to"];
	}
		
	void GenerateColorToTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		colors = new Color[3];
		
		//from and init to values:
		if(GetComponent(typeof(GUITexture))){
			colors[0] = colors[1] = guiTexture.color;
		}else if(GetComponent(typeof(GUIText))){
			colors[0] = colors[1] = guiText.material.color;
		}else if(renderer){
			colors[0] = colors[1] = renderer.material.color;	
		}else if(light){
			colors[0] = colors[1] = light.color;	
		}
		
		//to values:
		if (tweenArguments.Contains("color")) {
			colors[1]=(Color)tweenArguments["color"];
		}else{
			if (tweenArguments.Contains("r")) {
				colors[1].r=(float)tweenArguments["r"];
			}
			if (tweenArguments.Contains("g")) {
				colors[1].g=(float)tweenArguments["g"];
			}
			if (tweenArguments.Contains("b")) {
				colors[1].b=(float)tweenArguments["b"];
			}
			if (tweenArguments.Contains("a")) {
				colors[1].a=(float)tweenArguments["a"];
			}
		}
		
		//alpha or amount?
		if(tweenArguments.Contains("amount")){
			colors[1].a=(float)tweenArguments["amount"];
		}else if(tweenArguments.Contains("alpha")){
			colors[1].a=(float)tweenArguments["alpha"];
		}
	}
	
	void GenerateAudioToTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector2s=new Vector2[3];
		
		//set audioSource:
		if(tweenArguments.Contains("audiosource")){
			audioSource=(AudioSource)tweenArguments["audiosource"];
		}else{
			if(GetComponent(typeof(AudioSource))){
				audioSource=audio;
			}else{
				//throw error if no AudioSource is available:
				Debug.LogError("iTween Error: AudioTo requires an AudioSource.");
				Dispose();
			}
		}		
		
		//from values and default to values:
		vector2s[0]=vector2s[1]=new Vector2(audioSource.volume,audioSource.pitch);
				
		//to values:
		if (tweenArguments.Contains("volume")) {
			vector2s[1].x=(float)tweenArguments["volume"];	
		}
		if (tweenArguments.Contains("pitch")) {
			vector2s[1].y=(float)tweenArguments["pitch"];	
		}
	}
	
	void GenerateStabTargets(){
		//set audioSource:
		if(tweenArguments.Contains("audiosource")){
			audioSource=(AudioSource)tweenArguments["audiosource"];
		}else{
			if(GetComponent(typeof(AudioSource))){
				audioSource=audio;
			}else{
				//add and populate AudioSource if one doesn't exist:
				gameObject.AddComponent(typeof(AudioSource));
				audioSource=audio;
				audioSource.playOnAwake=false;
				
			}
		}
		
		//populate audioSource's clip:
		audioSource.clip=(AudioClip)tweenArguments["audioclip"];
		
		//set audio's pitch and volume if requested:
		if(tweenArguments.Contains("pitch")){
			audioSource.pitch=(float)tweenArguments["pitch"];
		}
		if(tweenArguments.Contains("volume")){
			audioSource.volume=(float)tweenArguments["volume"];
		}
			
		//set run time based on length of clip after pitch is augmented
		time=audioSource.clip.length/audioSource.pitch;
	}
	
	void GenerateLookToTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		vector3s[0]=transform.eulerAngles;
		
		//set look:
		if(tweenArguments.Contains("looktarget")){
			if (tweenArguments["looktarget"].GetType() == typeof(Transform)) {
				transform.LookAt((Transform)tweenArguments["looktarget"]);
			}else if(tweenArguments["looktarget"].GetType() == typeof(Vector3)){
				transform.LookAt((Vector3)tweenArguments["looktarget"]);
			}
		}else{
			Debug.LogError("iTween Error: LookTo needs a 'looktarget' property!");
			Dispose();
		}

		//to values:
		vector3s[1]=transform.eulerAngles;
		transform.eulerAngles=vector3s[0];
		
		//axis restriction:
		if(tweenArguments.Contains("axis")){
			switch((string)tweenArguments["axis"]){
				case "x":
					vector3s[1].y=vector3s[0].y;
					vector3s[1].z=vector3s[0].z;
				break;
				case "y":
					vector3s[1].x=vector3s[0].x;
					vector3s[1].z=vector3s[0].z;
				break;
				case "z":
					vector3s[1].x=vector3s[0].x;
					vector3s[1].y=vector3s[0].y;
				break;
			}
		}
		
		//shortest distance:
		vector3s[1]=new Vector3(clerp(vector3s[0].x,vector3s[1].x,1),clerp(vector3s[0].y,vector3s[1].y,1),clerp(vector3s[0].z,vector3s[1].z,1));
	}	
	
	void GenerateMoveToPathTargets(){
		 Vector3[] suppliedPath;
		
		//handle "back" easing equation requests:
		if(tweenArguments.Contains("easetype")){
			if((EaseType)tweenArguments["easetype"]==EaseType.easeInBack || (EaseType)tweenArguments["easetype"]==EaseType.easeOutBack || (EaseType)tweenArguments["easetype"]==EaseType.easeInOutBack){
				Debug.LogWarning("iTween Warning: Sorry, easing equations that generate overshooting values are clamped due to interpolation limitations with current Catmull-Rom solution.");
			}
		}
		
		//create and store path points:
		if(tweenArguments["path"].GetType() == typeof(Vector3[])){
			Vector3[] temp = (Vector3[])tweenArguments["path"];
			//if only one point is supplied fall back to MoveTo's traditional use since we can't have a curve with one value:
			if(temp.Length==1){
				Debug.LogError("iTween Error: Attempting a path movement with MoveTo requires an array of more than 1 entry!");
				Dispose();
			}
			suppliedPath=new Vector3[temp.Length];
			Array.Copy(temp,suppliedPath, temp.Length);
		}else{
			Transform[] temp = (Transform[])tweenArguments["path"];
			//if only one point is supplied fall back to MoveTo's traditional use since we can't have a curve with one value:
			if(temp.Length==1){
				Debug.LogError("iTween Error: Attempting a path movement with MoveTo requires an array of more than 1 entry!");
				Dispose();
			}
			suppliedPath = new Vector3[temp.Length];
			for (int i = 0; i < temp.Length; i++) {
				suppliedPath[i]=temp[i].position;
			}
		}
		
		//de we need to plot a path to get to the beginning of the supplied path?
		bool plotStart;
		int offset;
		if(transform.position != suppliedPath[0]){
			plotStart=true;
			offset=3;
		}else{
			plotStart=false;
			offset=2;
		}	
				
		//build calculated path:
		vector3s = new Vector3[suppliedPath.Length+offset];
		if(plotStart){
			vector3s[1]=transform.position;
			offset=2;
		}else{
			offset=1;
		}		
		
		//populate calculate path;
		Array.Copy(suppliedPath,0,vector3s,offset,suppliedPath.Length);
		
		//populate start and end control points:
		vector3s[0] = vector3s[1] - vector3s[2];
		vector3s[vector3s.Length-1] = vector3s[vector3s.Length-2] + (vector3s[vector3s.Length-2] - vector3s[vector3s.Length-3]);
		
		//is this a closed, continuous loop? yes? well then so let's make a contunous Catmull-Rom spline!
		if(vector3s[1] == vector3s[vector3s.Length-2]){
			Vector3[] tmpLoopSpline = new Vector3[vector3s.Length+1];
			Array.Copy(vector3s,tmpLoopSpline,vector3s.Length);
			//tmpLoopSpline[tmpLoopSpline.Length-3] =vector3s[1] + ((vector3s[1] - vector3s[vector3s.Length-1])/2);
			tmpLoopSpline[tmpLoopSpline.Length-3] = tmpLoopSpline[1]; //this may need a bit more finesse to get the spline loop to be more predictable rather than just relying on the initial start point generated above
			tmpLoopSpline[tmpLoopSpline.Length-2] = tmpLoopSpline[1];
			tmpLoopSpline[tmpLoopSpline.Length-1] = tmpLoopSpline[2];
			vector3s=new Vector3[tmpLoopSpline.Length];
			Array.Copy(tmpLoopSpline,vector3s,tmpLoopSpline.Length);
		}
		
		//create Catmull-Rom path:
		path = new CRSpline(vector3s);
	}
	
	void GenerateMoveToTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		if (isLocal) {
			vector3s[0]=vector3s[1]=transform.localPosition;				
		}else{
			vector3s[0]=vector3s[1]=transform.position;
		}
		
		//to values:
		if (tweenArguments.Contains("position")) {
			if (tweenArguments["position"].GetType() == typeof(Transform)){
				Transform trans = (Transform)tweenArguments["position"];
				vector3s[1]=trans.position;
			}else if(tweenArguments["position"].GetType() == typeof(Vector3)){
				vector3s[1]=(Vector3)tweenArguments["position"];
			}
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=(float)tweenArguments["z"];
			}
		}
		
		//handle orient to path request:
		if(tweenArguments.Contains("orienttopath") && (bool)tweenArguments["orienttopath"]){
			tweenArguments["looktarget"] = vector3s[1];
		}
	}
	
	void GenerateMoveByTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation, [3] previous value for Translate usage to allow Space utilization, [4] original rotation to make sure look requests don't interfere with the direction object should move in:
		vector3s=new Vector3[5];
		
		//grab starting rotation:
		vector3s[4] = transform.eulerAngles;
		
		//from values:
		vector3s[0]=vector3s[1]=vector3s[3]=transform.position;
				
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=vector3s[0] + (Vector3)tweenArguments["amount"];
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=vector3s[0].x + (float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=vector3s[0].y +(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=vector3s[0].z + (float)tweenArguments["z"];
			}
		}	
		
		//handle orient to path request:
		if(tweenArguments.Contains("orienttopath") && (bool)tweenArguments["orienttopath"]){
			tweenArguments["looktarget"] = vector3s[1];
		}
	}
	
	void GenerateScaleToTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		vector3s[0]=vector3s[1]=transform.localScale;				

		//to values:
		if (tweenArguments.Contains("scale")) {
			if (tweenArguments["scale"].GetType() == typeof(Transform)){
				Transform trans = (Transform)tweenArguments["scale"];
				vector3s[1]=trans.localScale;					
			}else if(tweenArguments["scale"].GetType() == typeof(Vector3)){
				vector3s[1]=(Vector3)tweenArguments["scale"];
			}
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=(float)tweenArguments["z"];
			}
		} 			
	}
	
	void GenerateScaleByTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		vector3s[0]=vector3s[1]=transform.localScale;				

		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=Vector3.Scale(vector3s[1],(Vector3)tweenArguments["amount"]);
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x*=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y*=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z*=(float)tweenArguments["z"];
			}
		} 			
	}
	
	void GenerateScaleAddTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		vector3s[0]=vector3s[1]=transform.localScale;				

		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]+=(Vector3)tweenArguments["amount"];
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x+=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y+=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z+=(float)tweenArguments["z"];
			}
		} 			
	}
	
	void GenerateRotateToTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		if (isLocal) {
			vector3s[0]=vector3s[1]=transform.localEulerAngles;				
		}else{
			vector3s[0]=vector3s[1]=transform.eulerAngles;
		}
		
		//to values:
		if (tweenArguments.Contains("rotation")) {
			if (tweenArguments["rotation"].GetType() == typeof(Transform)){
				Transform trans = (Transform)tweenArguments["rotation"];
				vector3s[1]=trans.eulerAngles;			
			}else if(tweenArguments["rotation"].GetType() == typeof(Vector3)){
				vector3s[1]=(Vector3)tweenArguments["rotation"];
			}
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=(float)tweenArguments["z"];
			}
		}
		
		//shortest distance:
		vector3s[1]=new Vector3(clerp(vector3s[0].x,vector3s[1].x,1),clerp(vector3s[0].y,vector3s[1].y,1),clerp(vector3s[0].z,vector3s[1].z,1));
	}
	
	void GenerateRotateAddTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation, [3] previous value for Rotate usage to allow Space utilization:
		vector3s=new Vector3[4];
		
		//from values:
		vector3s[0]=vector3s[1]=vector3s[3]=transform.eulerAngles;
		
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x+=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y+=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z+=(float)tweenArguments["z"];
			}
		}
	}		
	
	void GenerateRotateByTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation, [3] previous value for Rotate usage to allow Space utilization:
		vector3s=new Vector3[4];
		
		//from values:
		vector3s[0]=vector3s[1]=vector3s[3]=transform.eulerAngles;
		
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]+=Vector3.Scale((Vector3)tweenArguments["amount"],new Vector3(360,360,360));
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x+=360 * (float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y+=360 * (float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z+=360 * (float)tweenArguments["z"];
			}
		}
	}		
	
	void GenerateShakePositionTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation, [3] original rotation to make sure look requests don't interfere with the direction object should move in:
		vector3s=new Vector3[4];
		
		//grab starting rotation:
		vector3s[3] = transform.eulerAngles;		
		
		//root:
		vector3s[0]=transform.position;
		
		//amount:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=(float)tweenArguments["z"];
			}
		}
	}		
	
	void GenerateShakeScaleTargets(){
		//values holder [0] root value, [1] amount, [2] generated amount:
		vector3s=new Vector3[3];
		
		//root:
		vector3s[0]=transform.localScale;
		
		//amount:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=(float)tweenArguments["z"];
			}
		}
	}		
		
	void GenerateShakeRotationTargets(){
		//values holder [0] root value, [1] amount, [2] generated amount:
		vector3s=new Vector3[3];
		
		//root:
		vector3s[0]=transform.eulerAngles;
		
		//amount:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=(float)tweenArguments["z"];
			}
		}
	}	
	
	void GeneratePunchPositionTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation, [3] previous value for Translate usage to allow Space utilization, [4] original rotation to make sure look requests don't interfere with the direction object should move in:
		vector3s=new Vector3[5];
		
		//grab starting rotation:
		vector3s[4] = transform.eulerAngles;
		
		//from values:
		vector3s[0]=transform.position;
		vector3s[1]=vector3s[3]=Vector3.zero;
				
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=(float)tweenArguments["z"];
			}
		}
	}	
	
	void GeneratePunchRotationTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation, [3] previous value for Translate usage to allow Space utilization:
		vector3s=new Vector3[4];
		
		//from values:
		vector3s[0]=transform.eulerAngles;
		vector3s[1]=vector3s[3]=Vector3.zero;
				
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=(float)tweenArguments["z"];
			}
		}
	}		
	
	void GeneratePunchScaleTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		vector3s[0]=transform.localScale;
		vector3s[1]=Vector3.zero;
				
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=(float)tweenArguments["z"];
			}
		}
	}
	
	#endregion
	
	#region #4 Apply Targets
	
	void ApplyRectTargets(){
		//calculate:
		rects[2].x = ease(rects[0].x,rects[1].x,percentage);
		rects[2].y = ease(rects[0].y,rects[1].y,percentage);
		rects[2].width = ease(rects[0].width,rects[1].width,percentage);
		rects[2].height = ease(rects[0].height,rects[1].height,percentage);
		
		//apply:
		tweenArguments["onupdateparams"]=rects[2];
		
		//dial in:
		if(percentage==1){
			tweenArguments["onupdateparams"]=rects[1];
		}
	}		
	
	void ApplyColorTargets(){
		//calculate:
		colors[2].r = ease(colors[0].r,colors[1].r,percentage);
		colors[2].g = ease(colors[0].g,colors[1].g,percentage);
		colors[2].b = ease(colors[0].b,colors[1].b,percentage);
		colors[2].a = ease(colors[0].a,colors[1].a,percentage);
		
		//apply:
		tweenArguments["onupdateparams"]=colors[2];
		
		//dial in:
		if(percentage==1){
			tweenArguments["onupdateparams"]=colors[1];
		}
	}	
		
	void ApplyVector3Targets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		tweenArguments["onupdateparams"]=vector3s[2];
		
		//dial in:
		if(percentage==1){
			tweenArguments["onupdateparams"]=vector3s[1];
		}
	}		
	
	void ApplyVector2Targets(){
		//calculate:
		vector2s[2].x = ease(vector2s[0].x,vector2s[1].x,percentage);
		vector2s[2].y = ease(vector2s[0].y,vector2s[1].y,percentage);
		
		//apply:
		tweenArguments["onupdateparams"]=vector2s[2];
		
		//dial in:
		if(percentage==1){
			tweenArguments["onupdateparams"]=vector2s[1];
		}
	}	
	
	void ApplyFloatTargets(){
		//calculate:
		floats[2] = ease(floats[0],floats[1],percentage);
		
		//apply:
		tweenArguments["onupdateparams"]=floats[2];
		
		//dial in:
		if(percentage==1){
			tweenArguments["onupdateparams"]=floats[1];
		}
	}	
	
	void ApplyColorToTargets(){
		//calculate:
		colors[2].r = ease(colors[0].r,colors[1].r,percentage);
		colors[2].g = ease(colors[0].g,colors[1].g,percentage);
		colors[2].b = ease(colors[0].b,colors[1].b,percentage);
		colors[2].a = ease(colors[0].a,colors[1].a,percentage);
		
		//apply:
		if(GetComponent(typeof(GUITexture))){
			guiTexture.color=colors[2];
		}else if(GetComponent(typeof(GUIText))){
			guiText.material.color=colors[2];
		}else if(renderer){
			renderer.material.color=colors[2];	
		}else if(light){
			light.color=colors[2];	
		}
		
		//dial in:
		if(percentage==1){
			if(GetComponent(typeof(GUITexture))){
				guiTexture.color=colors[1];
			}else if(GetComponent(typeof(GUIText))){
				guiText.material.color=colors[1];
			}else if(renderer){
				renderer.material.color=colors[1];	
			}else if(light){
				light.color=colors[1];	
			}			
		}
	}	
	
	void ApplyAudioToTargets(){
		//calculate:
		vector2s[2].x = ease(vector2s[0].x,vector2s[1].x,percentage);
		vector2s[2].y = ease(vector2s[0].y,vector2s[1].y,percentage);
		
		//apply:
		audioSource.volume=vector2s[2].x;
		audioSource.pitch=vector2s[2].y;
		
		//dial in:
		if(percentage==1){
			audioSource.volume=vector2s[1].x;
			audioSource.pitch=vector2s[1].y;	
		}
	}	
	
	void ApplyStabTargets(){
		//unnecessary but here just in case
	}
	
	void ApplyMoveToPathTargets(){
		float t = ease(0,1,percentage);
		float lookAheadAmount;
		
		//clamp easing equation results as "back" will fail since overshoots aren't handled in the Catmull-Rom interpolation:
		if(isLocal){
			transform.localPosition=path.Interp(Mathf.Clamp(t,0,1));	
		}else{
			transform.position=path.Interp(Mathf.Clamp(t,0,1));	
		}
		
		//handle orient to path request:
		if(tweenArguments.Contains("orienttopath") && (bool)tweenArguments["orienttopath"]){
			
			//plot a point slightly ahead in the interpolation by pushing the percentage forward using the default lookahead value:
			float tLook;
			if(tweenArguments.Contains("lookahead")){
				lookAheadAmount = (float)tweenArguments["lookahead"];
			}else{
				lookAheadAmount = Defaults.lookAhead;
			}
			tLook = ease(0,1,percentage+lookAheadAmount);
			
			//locate new leading point with a clamp as stated above:
			tweenArguments["looktarget"] = path.Interp(Mathf.Clamp(tLook,0,1));
		}
	}
	
	void ApplyMoveToTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		if (isLocal) {
			transform.localPosition=vector3s[2];		
		}else{
			transform.position=vector3s[2];
		}
		
		//dial in:
		if(percentage==1){
			if (isLocal) {
				transform.localPosition=vector3s[1];		
			}else{
				transform.position=vector3s[1];
			}
		}
	}	
	
	void ApplyMoveByTargets(){	
		//reset rotation to prevent look interferences as object rotates and attempts to move with translate and record current rotation
		Vector3 currentRotation = new Vector3();
		
		if(tweenArguments.Contains("looktarget")){
			currentRotation = transform.eulerAngles;
			transform.eulerAngles = vector3s[4];	
		}
		
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
				
		//apply:
		transform.Translate(vector3s[2]-vector3s[3],space);
		
		//record:
		vector3s[3]=vector3s[2];
		
		//reset rotation:
		if(tweenArguments.Contains("looktarget")){
			transform.eulerAngles = currentRotation;	
		}
	}	
	
	void ApplyScaleToTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		transform.localScale=vector3s[2];	
		
		//dial in:
		if(percentage==1){
			transform.localScale=vector3s[1];
		}
	}
	
	void ApplyLookToTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		if (isLocal) {
			transform.localRotation = Quaternion.Euler(vector3s[2]);
		}else{
			transform.rotation = Quaternion.Euler(vector3s[2]);
		};	
	}	
	
	void ApplyRotateToTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		if (isLocal) {
			transform.localRotation = Quaternion.Euler(vector3s[2]);
		}else{
			transform.rotation = Quaternion.Euler(vector3s[2]);
		};	
		
		//dial in:
		if(percentage==1){
			if (isLocal) {
				transform.localRotation = Quaternion.Euler(vector3s[1]);
			}else{
				transform.rotation = Quaternion.Euler(vector3s[1]);
			};
		}
	}
	
	void ApplyRotateAddTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		transform.Rotate(vector3s[2]-vector3s[3],space);

		//record:
		vector3s[3]=vector3s[2];
	}	
	
	void ApplyShakePositionTargets(){
		//reset rotation to prevent look interferences as object rotates and attempts to move with translate and record current rotation
		Vector3 currentRotation = new Vector3();
		
		if(tweenArguments.Contains("looktarget")){
			currentRotation = transform.eulerAngles;
			transform.eulerAngles = vector3s[3];	
		}
		
		//impact:
		if (percentage==0) {
			transform.Translate(vector3s[1],space);
		}
		
		//reset:
		transform.position=vector3s[0];
		
		//generate:
		float diminishingControl = 1-percentage;
		vector3s[2].x= UnityEngine.Random.Range(-vector3s[1].x*diminishingControl, vector3s[1].x*diminishingControl);
		vector3s[2].y= UnityEngine.Random.Range(-vector3s[1].y*diminishingControl, vector3s[1].y*diminishingControl);
		vector3s[2].z= UnityEngine.Random.Range(-vector3s[1].z*diminishingControl, vector3s[1].z*diminishingControl);

		//apply:
		transform.Translate(vector3s[2],space);	
		
		//reset rotation:
		if(tweenArguments.Contains("looktarget")){
			transform.eulerAngles = currentRotation;	
		}		
	}	
	
	void ApplyShakeScaleTargets(){
		//impact:
		if (percentage==0) {
			transform.localScale=vector3s[1];
		}
		
		//reset:
		transform.localScale=vector3s[0];
		
		//generate:
		float diminishingControl = 1-percentage;
		vector3s[2].x= UnityEngine.Random.Range(-vector3s[1].x*diminishingControl, vector3s[1].x*diminishingControl);
		vector3s[2].y= UnityEngine.Random.Range(-vector3s[1].y*diminishingControl, vector3s[1].y*diminishingControl);
		vector3s[2].z= UnityEngine.Random.Range(-vector3s[1].z*diminishingControl, vector3s[1].z*diminishingControl);

		//apply:
		transform.localScale+=vector3s[2];
	}		
	
	void ApplyShakeRotationTargets(){
		//impact:
		if (percentage==0) {
			transform.Rotate(vector3s[1],space);
		}
		
		//reset:
		transform.eulerAngles=vector3s[0];
		
		//generate:
		float diminishingControl = 1-percentage;
		vector3s[2].x= UnityEngine.Random.Range(-vector3s[1].x*diminishingControl, vector3s[1].x*diminishingControl);
		vector3s[2].y= UnityEngine.Random.Range(-vector3s[1].y*diminishingControl, vector3s[1].y*diminishingControl);
		vector3s[2].z= UnityEngine.Random.Range(-vector3s[1].z*diminishingControl, vector3s[1].z*diminishingControl);

		//apply:
		transform.Rotate(vector3s[2],space);	
	}		
	
	void ApplyPunchPositionTargets(){
		//reset rotation to prevent look interferences as object rotates and attempts to move with translate and record current rotation
		Vector3 currentRotation = new Vector3();
		
		if(tweenArguments.Contains("looktarget")){
			currentRotation = transform.eulerAngles;
			transform.eulerAngles = vector3s[4];	
		}
		
		//calculate:
		if(vector3s[1].x>0){
			vector3s[2].x = punch(vector3s[1].x,percentage);
		}else if(vector3s[1].x<0){
			vector3s[2].x=-punch(Mathf.Abs(vector3s[1].x),percentage); 
		}
		if(vector3s[1].y>0){
			vector3s[2].y=punch(vector3s[1].y,percentage);
		}else if(vector3s[1].y<0){
			vector3s[2].y=-punch(Mathf.Abs(vector3s[1].y),percentage); 
		}
		if(vector3s[1].z>0){
			vector3s[2].z=punch(vector3s[1].z,percentage);
		}else if(vector3s[1].z<0){
			vector3s[2].z=-punch(Mathf.Abs(vector3s[1].z),percentage); 
		}
		
		//apply:
		transform.Translate(vector3s[2]-vector3s[3],space);

		//record:
		vector3s[3]=vector3s[2];
		
		//reset rotation:
		if(tweenArguments.Contains("looktarget")){
			transform.eulerAngles = currentRotation;	
		}
	}		
	
	void ApplyPunchRotationTargets(){
		//calculate:
		if(vector3s[1].x>0){
			vector3s[2].x = punch(vector3s[1].x,percentage);
		}else if(vector3s[1].x<0){
			vector3s[2].x=-punch(Mathf.Abs(vector3s[1].x),percentage); 
		}
		if(vector3s[1].y>0){
			vector3s[2].y=punch(vector3s[1].y,percentage);
		}else if(vector3s[1].y<0){
			vector3s[2].y=-punch(Mathf.Abs(vector3s[1].y),percentage); 
		}
		if(vector3s[1].z>0){
			vector3s[2].z=punch(vector3s[1].z,percentage);
		}else if(vector3s[1].z<0){
			vector3s[2].z=-punch(Mathf.Abs(vector3s[1].z),percentage); 
		}
		
		//apply:
		transform.Rotate(vector3s[2]-vector3s[3],space);

		//record:
		vector3s[3]=vector3s[2];
	}	
	
	void ApplyPunchScaleTargets(){
		//calculate:
		if(vector3s[1].x>0){
			vector3s[2].x = punch(vector3s[1].x,percentage);
		}else if(vector3s[1].x<0){
			vector3s[2].x=-punch(Mathf.Abs(vector3s[1].x),percentage); 
		}
		if(vector3s[1].y>0){
			vector3s[2].y=punch(vector3s[1].y,percentage);
		}else if(vector3s[1].y<0){
			vector3s[2].y=-punch(Mathf.Abs(vector3s[1].y),percentage); 
		}
		if(vector3s[1].z>0){
			vector3s[2].z=punch(vector3s[1].z,percentage);
		}else if(vector3s[1].z<0){
			vector3s[2].z=-punch(Mathf.Abs(vector3s[1].z),percentage); 
		}
		
		//apply:
		transform.localScale=vector3s[0]+vector3s[2];
	}		
	
	#endregion	
	
	#region #5 Tween Steps
	
	IEnumerator TweenDelay(){
		delayStarted = Time.time;
		yield return new WaitForSeconds (delay);
		if(wasPaused){
			wasPaused=false;
			TweenStart();	
		}
	}	
	
	void TweenStart(){		
		if(!loop){//only if this is not a loop
			ConflictCheck();
			GenerateTargets();
		}
				
		//run stab:
		if(type == "stab"){
			audioSource.PlayOneShot(audioSource.clip);
		}
		
		//toggle isKinematic for iTweens that may interfere with physics:
		if (type == "move" || type=="scale" || type=="rotate" || type=="punch" || type=="shake" || type=="curve" || type=="look") {
			EnableKinematic();
		}
		
		CallBack("onstart");
		isRunning = true;
	}
	
	IEnumerator TweenRestart(){
		if(delay > 0){
			delayStarted = Time.time;
			yield return new WaitForSeconds (delay);
		}
		loop=true;
		TweenStart();
	}	
	
	void TweenUpdate(){
		apply();
		CallBack("onupdate");
		UpdatePercentage();		
	}
			
	void TweenComplete(){
		CallBack("oncomplete");
		isRunning=false;
		
		//dial in percentage to 1 or 0 for final run:
		if(percentage>.5f){
			percentage=1f;
		}else{
			percentage=0;	
		}
		
		//apply dial in and final run:
		apply();
		if(type == "value"){
			CallBack("onupdate"); //CallBack run for ValueTo since it only calculates and applies in the update callback
		}
		
		//loop or dispose?
		if(loopType==LoopType.none){
			Dispose();
		}else{
			TweenLoop();
		}
	}
	
	void TweenLoop(){
		DisableKinematic(); //give physics control again
		switch(loopType){
			case LoopType.loop:
				//rewind:
				percentage=0;
				runningTime=0;
				apply();
				
				//replay:
				StartCoroutine("TweenRestart");
				break;
			case LoopType.pingPong:
				reverse = !reverse;
				runningTime=0;
			
				//replay:
				StartCoroutine("TweenRestart");
				break;
		}
	}	
	
	#endregion
	
	#region #6 Update Callable
			
	/// <summary>
	/// Similar to FadeTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="alpha">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the final alpha value of the animation.
	/// </param>
	/// <param name="includechildren">
	/// A <see cref="System.Boolean"/> for whether or not to include children of this GameObject. True by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void FadeUpdate(GameObject target, Hashtable args){
		args["a"]=args["alpha"];
		ColorUpdate(target,args);
	}
	
	/// <summary>
	/// Similar to FadeTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with MINIMUM customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="alpha">
	/// A <see cref="System.Single"/> for the final alpha value of the animation.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void FadeUpdate(GameObject target, float alpha, float time){
		FadeUpdate(target,Hash("alpha",alpha,"time",time));
	}
	
	/// <summary>
	/// Similar to ColorTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="color">
	/// A <see cref="Color"/> to change the GameObject's color to.
	/// </param>
	/// <param name="r">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color red.
	/// </param>
	/// <param name="g">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color green.
	/// </param>
	/// <param name="b">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color green.
	/// </param>
	/// <param name="a">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the alpha.
	/// </param> 
	/// <param name="includechildren">
	/// A <see cref="System.Boolean"/> for whether or not to include children of this GameObject. True by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ColorUpdate(GameObject target, Hashtable args){
		CleanArgs(args);
		
		float time;
		Color[] colors = new Color[4];
		
		//handle children:
		if(!args.Contains("includechildren") || (bool)args["includechildren"]){
			foreach(Transform child in target.transform){
				ColorUpdate(child.gameObject,args);
			}
		}		 
		
		//set smooth time:
		if(args.Contains("time")){
			time=(float)args["time"];
			time*=Defaults.updateTimePercentage;
		}else{
			time=Defaults.updateTime;
		}
		
		//init values:
		if(target.GetComponent(typeof(GUITexture))){
			colors[0] = colors[1] = target.guiTexture.color;
		}else if(target.GetComponent(typeof(GUIText))){
			colors[0] = colors[1] = target.guiText.material.color;
		}else if(target.renderer){
			colors[0] = colors[1] = target.renderer.material.color;
		}else if(target.light){
			colors[0] = colors[1] = target.light.color;	
		}		
		
		//to values:
		if (args.Contains("color")) {
			colors[1]=(Color)args["color"];
		}else{
			if (args.Contains("r")) {
				colors[1].r=(float)args["r"];
			}
			if (args.Contains("g")) {
				colors[1].g=(float)args["g"];
			}
			if (args.Contains("b")) {
				colors[1].b=(float)args["b"];
			}
			if (args.Contains("a")) {
				colors[1].a=(float)args["a"];
			}
		}
		
		//calculate:
		colors[3].r=Mathf.SmoothDamp(colors[0].r,colors[1].r,ref colors[2].r,time);
		colors[3].g=Mathf.SmoothDamp(colors[0].g,colors[1].g,ref colors[2].g,time);
		colors[3].b=Mathf.SmoothDamp(colors[0].b,colors[1].b,ref colors[2].b,time);
		colors[3].a=Mathf.SmoothDamp(colors[0].a,colors[1].a,ref colors[2].a,time);
				
		//apply:
		if(target.GetComponent(typeof(GUITexture))){
			target.guiTexture.color=colors[3];
		}else if(target.GetComponent(typeof(GUIText))){
			target.guiText.material.color=colors[3];
		}else if(target.renderer){
			target.renderer.material.color=colors[3];
		}else if(target.light){
			target.light.color=colors[3];	
		}
	}	
	
	/// <summary>
	/// Similar to ColorTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with MINIMUM customization options. Does not utilize an EaseType.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/> to change the GameObject's color to.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ColorUpdate(GameObject target, Color color, float time){
		ColorUpdate(target,Hash("color",color,"time",time));
	}
	
	/// <summary>
	/// Similar to AudioTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="audiosource">
	/// A <see cref="AudioSource"/> for which AudioSource to use.
	/// </param> 
	/// <param name="volume">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the target level of volume.
	/// </param>
	/// <param name="pitch">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the target pitch.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void AudioUpdate(GameObject target, Hashtable args){
		CleanArgs(args);
		
		AudioSource audioSource;
		float time;
		Vector2[] vector2s = new Vector2[4];
			
		//set smooth time:
		if(args.Contains("time")){
			time=(float)args["time"];
			time*=Defaults.updateTimePercentage;
		}else{
			time=Defaults.updateTime;
		}

		//set audioSource:
		if(args.Contains("audiosource")){
			audioSource=(AudioSource)args["audiosource"];
		}else{
			if(target.GetComponent(typeof(AudioSource))){
				audioSource=target.audio;
			}else{
				//throw error if no AudioSource is available:
				Debug.LogError("iTween Error: AudioUpdate requires an AudioSource.");
				return;
			}
		}		
		
		//from values:
		vector2s[0] = vector2s[1] = new Vector2(audioSource.volume,audioSource.pitch);
		
		//set to:
		if(args.Contains("volume")){
			vector2s[1].x=(float)args["volume"];
		}
		if(args.Contains("pitch")){
			vector2s[1].y=(float)args["pitch"];
		}
		
		//calculate:
		vector2s[3].x=Mathf.SmoothDampAngle(vector2s[0].x,vector2s[1].x,ref vector2s[2].x,time);
		vector2s[3].y=Mathf.SmoothDampAngle(vector2s[0].y,vector2s[1].y,ref vector2s[2].y,time);
	
		//apply:
		audioSource.volume=vector2s[3].x;
		audioSource.pitch=vector2s[3].y;
	}
	
	/// <summary>
	/// Similar to AudioTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with MINIMUM customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="volume">
	/// A <see cref="System.Single"/> for the target level of volume.
	/// </param>
	/// <param name="pitch">
	/// A <see cref="System.Single"/> for the target pitch.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void AudioUpdate(GameObject target, float volume, float pitch, float time){
		AudioUpdate(target,Hash("volume",volume,"pitch",pitch,"time",time));
	}
	
	/// <summary>
	/// Similar to RotateTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="rotation">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for the target Euler angles in degrees to rotate to.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False be default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param> 
	public static void RotateUpdate(GameObject target, Hashtable args){
		CleanArgs(args);
		
		bool isLocal;
		float time;
		Vector3[] vector3s = new Vector3[4];
		
		//set smooth time:
		if(args.Contains("time")){
			time=(float)args["time"];
			time*=Defaults.updateTimePercentage;
		}else{
			time=Defaults.updateTime;
		}
		
		//set isLocal:
		if(args.Contains("islocal")){
			isLocal = (bool)args["islocal"];
		}else{
			isLocal = Defaults.isLocal;	
		}
		
		//from values:
		if(isLocal){
			vector3s[0] = target.transform.localEulerAngles;
		}else{
			vector3s[0] = target.transform.eulerAngles;	
		}
		
		//set to:
		if(args.Contains("rotation")){
			if (args["rotation"].GetType() == typeof(Transform)){
				Transform trans = (Transform)args["rotation"];
				vector3s[1]=trans.eulerAngles;
			}else if(args["rotation"].GetType() == typeof(Vector3)){
				vector3s[1]=(Vector3)args["rotation"];
			}	
		}
				
		//calculate:
		vector3s[3].x=Mathf.SmoothDampAngle(vector3s[0].x,vector3s[1].x,ref vector3s[2].x,time);
		vector3s[3].y=Mathf.SmoothDampAngle(vector3s[0].y,vector3s[1].y,ref vector3s[2].y,time);
		vector3s[3].z=Mathf.SmoothDampAngle(vector3s[0].z,vector3s[1].z,ref vector3s[2].z,time);
	
		//apply:
		if(isLocal){
			target.transform.localEulerAngles=vector3s[3];
		}else{
			target.transform.eulerAngles=vector3s[3];
		}
	}
		
	/// <summary>
	/// Similar to RotateTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with MINIMUM customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="rotation">
	/// A <see cref="Vector3"/> for the target Euler angles in degrees to rotate to.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void RotateUpdate(GameObject target, Vector3 rotation, float time){
		RotateUpdate(target,Hash("rotation",rotation,"time",time));
	}
	
	/// <summary>
	/// Similar to ScaleTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options.  Does not utilize an EaseType. 
	/// </summary>
	/// <param name="scale">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for the final scale.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param> 
	public static void ScaleUpdate(GameObject target, Hashtable args){
		CleanArgs(args);
		
		float time;
		Vector3[] vector3s = new Vector3[4];
			
		//set smooth time:
		if(args.Contains("time")){
			time=(float)args["time"];
			time*=Defaults.updateTimePercentage;
		}else{
			time=Defaults.updateTime;
		}
		
		//init values:
		vector3s[0] = vector3s[1] = target.transform.localScale;
		
		//to values:
		if (args.Contains("scale")) {
			if (args["scale"].GetType() == typeof(Transform)){
				Transform trans = (Transform)args["scale"];
				vector3s[1]=trans.localScale;
			}else if(args["scale"].GetType() == typeof(Vector3)){
				vector3s[1]=(Vector3)args["scale"];
			}				
		}else{
			if (args.Contains("x")) {
				vector3s[1].x=(float)args["x"];
			}
			if (args.Contains("y")) {
				vector3s[1].y=(float)args["y"];
			}
			if (args.Contains("z")) {
				vector3s[1].z=(float)args["z"];
			}
		}
		
		//calculate:
		vector3s[3].x=Mathf.SmoothDamp(vector3s[0].x,vector3s[1].x,ref vector3s[2].x,time);
		vector3s[3].y=Mathf.SmoothDamp(vector3s[0].y,vector3s[1].y,ref vector3s[2].y,time);
		vector3s[3].z=Mathf.SmoothDamp(vector3s[0].z,vector3s[1].z,ref vector3s[2].z,time);
				
		//apply:
		target.transform.localScale=vector3s[3];		
	}	
	
	/// <summary>
	/// Similar to ScaleTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with MINIMUM customization options.  Does not utilize an EaseType.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="scale">
	/// A <see cref="Vector3"/> for the final scale.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ScaleUpdate(GameObject target, Vector3 scale, float time){
		ScaleUpdate(target,Hash("scale",scale,"time",time));
	}
	
	/// <summary>
	/// Similar to MoveTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="position">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for a point in space the GameObject will animate to.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param> 
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False be default.
	/// </param>
	/// <param name="orienttopath">
	/// A <see cref="System.Boolean"/> for whether or not the GameObject will orient to its direction of travel.  False by default.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="looktime">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the object will take to look at either the "looktarget" or "orienttopath".
	/// </param>
	/// <param name="axis">
	/// A <see cref="System.String"/>. Restricts rotation to the supplied axis only.
	/// </param>
	public static void MoveUpdate(GameObject target, Hashtable args){
		CleanArgs(args);
		
		float time;
		Vector3[] vector3s = new Vector3[4];
		bool isLocal;
			
		//set smooth time:
		if(args.Contains("time")){
			time=(float)args["time"];
			time*=Defaults.updateTimePercentage;
		}else{
			time=Defaults.updateTime;
		}
			
		//set isLocal:
		if(args.Contains("islocal")){
			isLocal = (bool)args["islocal"];
		}else{
			isLocal = Defaults.isLocal;	
		}
		 
		//init values:
		if(isLocal){
			vector3s[0] = vector3s[1] = target.transform.localPosition;
		}else{
			vector3s[0] = vector3s[1] = target.transform.position;	
		}
		
		//to values:
		if (args.Contains("position")) {
			if (args["position"].GetType() == typeof(Transform)){
				Transform trans = (Transform)args["position"];
				vector3s[1]=trans.position;
			}else if(args["position"].GetType() == typeof(Vector3)){
				vector3s[1]=(Vector3)args["position"];
			}			
		}else{
			if (args.Contains("x")) {
				vector3s[1].x=(float)args["x"];
			}
			if (args.Contains("y")) {
				vector3s[1].y=(float)args["y"];
			}
			if (args.Contains("z")) {
				vector3s[1].z=(float)args["z"];
			}
		}
		
		//calculate:
		vector3s[3].x=Mathf.SmoothDamp(vector3s[0].x,vector3s[1].x,ref vector3s[2].x,time);
		vector3s[3].y=Mathf.SmoothDamp(vector3s[0].y,vector3s[1].y,ref vector3s[2].y,time);
		vector3s[3].z=Mathf.SmoothDamp(vector3s[0].z,vector3s[1].z,ref vector3s[2].z,time);
			
		//handle orient to path:
		if(args.Contains("orienttopath") && (bool)args["orienttopath"]){
			args["looktarget"] = vector3s[3];
		}
		
		//look applications:
		if(args.Contains("looktarget")){
			iTween.LookUpdate(target,args);
		}
		
		//apply:
		if(isLocal){
			target.transform.localPosition = vector3s[3];			
		}else{
			target.transform.position=vector3s[3];	
		}		
	}

	/// <summary>
	/// Similar to MoveTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with MINIMUM customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="position">
	/// A <see cref="Vector3"/> for a point in space the GameObject will animate to.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void MoveUpdate(GameObject target, Vector3 position, float time){
		MoveUpdate(target,Hash("position",position,"time",time));
	}
	
	/// <summary>
	/// Similar to LookTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="looktarget">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="axis">
	/// A <see cref="System.String"/>. Restricts rotation to the supplied axis only.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param> 
	public static void LookUpdate(GameObject target, Hashtable args){
		CleanArgs(args);
		
		float time;
		Vector3[] vector3s = new Vector3[5];
		
		//set smooth time:
		if(args.Contains("looktime")){
			time=(float)args["looktime"];
			time*=Defaults.updateTimePercentage;
		}else if(args.Contains("time")){
			time=(float)args["time"];
			time*=Defaults.updateTimePercentage;
		}else{
			time=Defaults.updateTime;
		}
		
		//from values:
		vector3s[0] = target.transform.eulerAngles;
		
		//set look:
		if(args.Contains("looktarget")){
			if (args["looktarget"].GetType() == typeof(Transform)) {
				target.transform.LookAt((Transform)args["looktarget"]);
			}else if(args["looktarget"].GetType() == typeof(Vector3)){
				target.transform.LookAt((Vector3)args["looktarget"]);
			}
		}else{
			Debug.LogError("iTween Error: LookUpdate needs a 'looktarget' property!");
			return;
		}
		
		//to values and reset look:
		vector3s[1]=target.transform.eulerAngles;
		target.transform.eulerAngles=vector3s[0];
		
		//calculate:
		vector3s[3].x=Mathf.SmoothDampAngle(vector3s[0].x,vector3s[1].x,ref vector3s[2].x,time);
		vector3s[3].y=Mathf.SmoothDampAngle(vector3s[0].y,vector3s[1].y,ref vector3s[2].y,time);
		vector3s[3].z=Mathf.SmoothDampAngle(vector3s[0].z,vector3s[1].z,ref vector3s[2].z,time);
	
		//apply:
		target.transform.eulerAngles=vector3s[3];
		
		//axis restriction:
		if(args.Contains("axis")){
			vector3s[4]=target.transform.eulerAngles;
			switch((string)args["axis"]){
				case "x":
					vector3s[4].y=vector3s[0].y;
					vector3s[4].z=vector3s[0].z;
				break;
				case "y":
					vector3s[4].x=vector3s[0].x;
					vector3s[4].z=vector3s[0].z;
				break;
				case "z":
					vector3s[4].x=vector3s[0].x;
					vector3s[4].y=vector3s[0].y;
				break;
			}
			
			//apply axis restriction:
			target.transform.eulerAngles=vector3s[4];
		}	
	}
	
	/// <summary>
	/// Similar to LookTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void LookUpdate(GameObject target, Vector3 looktarget, float time){
		LookUpdate(target,Hash("looktarget",looktarget,"time",time));
	}

	#endregion
	
	#region #7 External Utilities

	/// <summary>
	/// Removes and destroyes a camera fade.
	/// </summary>
	public static void CameraFadeDestroy(int depth){
		if(cameraFade){
			Destroy(cameraFade);
		}
	}
	
	/// <summary>
	/// Changes a camera fade's depth.
	/// </summary>
	/// <param name="depth">
	/// A <see cref="System.Int32"/>
	/// </param>
	public static void CameraFadeDepth(int depth){
		if(cameraFade){
			cameraFade.transform.position=new Vector3(cameraFade.transform.position.x,cameraFade.transform.position.y,depth);
		}
	}
	
	/// <summary>
	/// Creates a GameObject (if it doesn't exist) at the supplied depth that can be used to simulate a camera fade.
	/// </summary>
	/// <param name="depth">
	/// A <see cref="System.Int32"/>
	/// </param>
	public static void CameraFadeAdd(int depth){
		if(cameraFade){
			return;
		}else{
			//eastablish fill texture:
			Texture2D colorTexture = new Texture2D(Screen.width,Screen.height);
		
			//establish colorFade object:
			cameraFade = new GameObject("iTween Camera Fade");
			cameraFade.transform.position= new Vector3(.5f,.5f,depth);
			cameraFade.AddComponent("GUITexture");
			cameraFade.guiTexture.texture=colorTexture;
			cameraFade.guiTexture.color=new Color(0,0,0,0);
		}
	}
	
	//#################################
	//# RESUME UTILITIES AND OVERLOADS # 
	//#################################	
	
	/// <summary>
	/// Resume all iTweens on a GameObject.
	/// </summary>
	public static void Resume(GameObject target){
		Component[] tweens = target.GetComponents(typeof(iTween));
		foreach (iTween item in tweens){
			item.enabled=true;
		}
	}
	
	/// <summary>
	/// Resume all iTweens on a GameObject including its children.
	/// </summary>
	public static void Resume(GameObject target, bool includechildren){
		Resume(target);
		if(includechildren){
			foreach(Transform child in target.transform){
				Resume(child.gameObject,true);
			}			
		}
	}	
	
	/// <summary>
	/// Resume all iTweens on a GameObject of a particular type.
	/// </summar
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of iTween you would like to resume.  Can be written as part of a name such as "mov" for all "MoveTo" iTweens.
	/// </param>	
	public static void Resume(GameObject target, string type){
		Component[] tweens = target.GetComponents(typeof(iTween));
		foreach (iTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				item.enabled=true;
			}
		}
	}
	
	/// <summary>
	/// Resume all iTweens on a GameObject of a particular type including its children.
	/// </summar
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of iTween you would like to resume.  Can be written as part of a name such as "mov" for all "MoveTo" iTweens.
	/// </param>	
	public static void Resume(GameObject target, string type, bool includechildren){
		Component[] tweens = target.GetComponents(typeof(iTween));
		foreach (iTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				item.enabled=true;
			}
		}
		if(includechildren){
			foreach(Transform child in target.transform){
				Resume(child.gameObject,type,true);
			}			
		}		
	}	
	
	/// <summary>
	/// Resume all iTweens in scene.
	/// </summary>
	public static void Resume(){
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = (Hashtable)tweens[i];
			GameObject target = (GameObject)currentTween["target"];
			Resume(target);
		}
	}	
	
	/// <summary>
	/// Resume all iTweens in scene of a particular type.
	/// </summary>
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of iTween you would like to resume.  Can be written as part of a name such as "mov" for all "MoveTo" iTweens.
	/// </param> 
	public static void Resume(string type){
		ArrayList resumeArray = new ArrayList();
		
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = (Hashtable)tweens[i];
			GameObject target = (GameObject)currentTween["target"];
			resumeArray.Insert(resumeArray.Count,target);
		}
		
		for (int i = 0; i < resumeArray.Count; i++) {
			Resume((GameObject)resumeArray[i],type);
		}
	}			
	
	//#################################
	//# PAUSE UTILITIES AND OVERLOADS # 
	//#################################

	/// <summary>
	/// Pause all iTweens on a GameObject.
	/// </summary>
	public static void Pause(GameObject target){
		Component[] tweens = target.GetComponents(typeof(iTween));
		foreach (iTween item in tweens){
			if(item.delay>0){
				item.delay-=Time.time-item.delayStarted;
				item.StopCoroutine("TweenDelay");
			}
			item.isPaused=true;
			item.enabled=false;
		}
	}
	
	/// <summary>
	/// Pause all iTweens on a GameObject including its children.
	/// </summary>
	public static void Pause(GameObject target, bool includechildren){
		Pause(target);
		if(includechildren){
			foreach(Transform child in target.transform){
				Pause(child.gameObject,true);
			}			
		}
	}	
	
	/// <summary>
	/// Pause all iTweens on a GameObject of a particular type.
	/// </summar
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of iTween you would like to pause.  Can be written as part of a name such as "mov" for all "MoveTo" iTweens.
	/// </param>	
	public static void Pause(GameObject target, string type){
		Component[] tweens = target.GetComponents(typeof(iTween));
		foreach (iTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				if(item.delay>0){
					item.delay-=Time.time-item.delayStarted;
					item.StopCoroutine("TweenDelay");
				}
				item.isPaused=true;
				item.enabled=false;
			}
		}
	}
	
	/// <summary>
	/// Pause all iTweens on a GameObject of a particular type including its children.
	/// </summar
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of iTween you would like to pause.  Can be written as part of a name such as "mov" for all "MoveTo" iTweens.
	/// </param>	
	public static void Pause(GameObject target, string type, bool includechildren){
		Component[] tweens = target.GetComponents(typeof(iTween));
		foreach (iTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				if(item.delay>0){
					item.delay-=Time.time-item.delayStarted;
					item.StopCoroutine("TweenDelay");
				}
				item.isPaused=true;
				item.enabled=false;
			}
		}
		if(includechildren){
			foreach(Transform child in target.transform){
				Pause(child.gameObject,type,true);
			}			
		}		
	}	
	
	/// <summary>
	/// Pause all iTweens in scene.
	/// </summary>
	public static void Pause(){
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = (Hashtable)tweens[i];
			GameObject target = (GameObject)currentTween["target"];
			Pause(target);
		}
	}	
	
	/// <summary>
	/// Pause all iTweens in scene of a particular type.
	/// </summary>
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of iTween you would like to pause.  Can be written as part of a name such as "mov" for all "MoveTo" iTweens.
	/// </param> 
	public static void Pause(string type){
		ArrayList pauseArray = new ArrayList();
		
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = (Hashtable)tweens[i];
			GameObject target = (GameObject)currentTween["target"];
			pauseArray.Insert(pauseArray.Count,target);
		}
		
		for (int i = 0; i < pauseArray.Count; i++) {
			Pause((GameObject)pauseArray[i],type);
		}
	}		
	
	//#################################
	//# COUNT UTILITIES AND OVERLOADS # 
	//#################################	
	
	/// <summary>
	/// Count all iTweens in current scene.
	/// </summary>
	public static int Count(){
		return(tweens.Count);
	}
	
	/// <summary>
	/// Count all iTweens in current scene of a particular type.
	/// </summary>
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of iTween you would like to stop.  Can be written as part of a name such as "mov" for all "MoveTo" iTweens.
	/// </param> 
	public static int Count(string type){
		int tweenCount = 0;

		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = (Hashtable)tweens[i];
			string targetType = (string)currentTween["type"]+(string)currentTween["method"];
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				tweenCount++;
			}
		}	
		
		return(tweenCount);
	}			

	/// <summary>
	/// Count all iTweens on a GameObject.
	/// </summary>
	public static int Count(GameObject target){
		Component[] tweens = target.GetComponents(typeof(iTween));
		return(tweens.Length);
	}
	
	/// <summary>
	/// Count all iTweens on a GameObject of a particular type.
	/// </summary>
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of iTween you would like to count.  Can be written as part of a name such as "mov" for all "MoveTo" iTweens.
	/// </param>  
	public static int Count(GameObject target, string type){
		int tweenCount = 0;
		Component[] tweens = target.GetComponents(typeof(iTween));foreach (iTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				tweenCount++;
			}
		}
		return(tweenCount);
	}	
	
	//################################
	//# STOP UTILITIES AND OVERLOADS # 
	//################################	
	
	/// <summary>
	/// Stop and destroy all Tweens in current scene.
	/// </summary>
	public static void Stop(){
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = (Hashtable)tweens[i];
			GameObject target = (GameObject)currentTween["target"];
			Stop(target);
		}
		tweens.Clear();
	}	
	
	/// <summary>
	/// Stop and destroy all iTweens in current scene of a particular type.
	/// </summary>
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of iTween you would like to stop.  Can be written as part of a name such as "mov" for all "MoveTo" iTweens.
	/// </param> 
	public static void Stop(string type){
		ArrayList stopArray = new ArrayList();
		
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = (Hashtable)tweens[i];
			GameObject target = (GameObject)currentTween["target"];
			stopArray.Insert(stopArray.Count,target);
		}
		
		for (int i = 0; i < stopArray.Count; i++) {
			Stop((GameObject)stopArray[i],type);
		}
	}		
	
	/// <summary>
	/// Stop and destroy all iTweens on a GameObject.
	/// </summary>
	public static void Stop(GameObject target){
		Component[] tweens = target.GetComponents(typeof(iTween));
		foreach (iTween item in tweens){
			item.Dispose();
		}
	}
	
	/// <summary>
	/// Stop and destroy all iTweens on a GameObject including its children.
	/// </summary>
	public static void Stop(GameObject target, bool includechildren){
		Stop(target);
		if(includechildren){
			foreach(Transform child in target.transform){
				Stop(child.gameObject,true);
			}			
		}
	}	
	
	/// <summary>
	/// Stop and destroy all iTweens on a GameObject of a particular type.
	/// </summar
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of iTween you would like to stop.  Can be written as part of a name such as "mov" for all "MoveTo" iTweens.
	/// </param>	
	public static void Stop(GameObject target, string type){
		Component[] tweens = target.GetComponents(typeof(iTween));
		foreach (iTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				item.Dispose();
			}
		}
	}
	
	/// <summary>
	/// Stop and destroy all iTweens on a GameObject of a particular type including its children.
	/// </summar
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of iTween you would like to stop.  Can be written as part of a name such as "mov" for all "MoveTo" iTweens.
	/// </param>	
	public static void Stop(GameObject target, string type, bool includechildren){
		Component[] tweens = target.GetComponents(typeof(iTween));
		foreach (iTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				item.Dispose();
			}
		}
		if(includechildren){
			foreach(Transform child in target.transform){
				Stop(child.gameObject,type,true);
			}			
		}		
	}
	
	/// <summary>
	/// Universal interface to help in the creation of Hashtables.  Especially useful for C# users.
	/// </summary>
	/// <param name="args">
	/// A <see cref="System.Object[]"/> of alternating name value pairs.  For example "time",1,"delay",2...
	/// </param>
	/// <returns>
	/// A <see cref="Hashtable"/>
	/// </returns>
	public static Hashtable Hash(params object[] args){
		Hashtable hashTable = new Hashtable(args.Length/2);
		if (args.Length %2 != 0) {
			Debug.LogError("Tween Error: Hash requires an even number of arguments!"); 
			return null;
		}else{
			int i = 0;
			while(i < args.Length - 1) {
				hashTable.Add(args[i], args[i+1]);
				i += 2;
			}
			return hashTable;
		}
	}	
	
	#endregion		

	#region Component Segments
	
	void Awake(){
		RetrieveArgs();
	}
	
	IEnumerator Start(){
		if(delay > 0){
			yield return StartCoroutine("TweenDelay");
		}
		TweenStart();
	}	
	
	void Update(){
		if(isRunning){
			if(!reverse){
				if(percentage<1f){
					TweenUpdate();
				}else{
					TweenComplete();	
				}
			}else{
				if(percentage>0){
					TweenUpdate();
				}else{
					TweenComplete();	
				}
			}
		}
	}

	void LateUpdate(){
		//look applications:
		if(tweenArguments.Contains("looktarget") && isRunning){
			LookUpdate(gameObject,tweenArguments);
		}
	}
	
	void OnEnable(){
		if(isRunning){
			EnableKinematic();
		}
	
		//resume delay:
		if(isPaused){
			isPaused=false;
			if(delay > 0){
				wasPaused=true;
				ResumeDelay();
			}
		}
	}

	void OnDisable(){
		DisableKinematic();
	}
	
	#endregion
	
	#region Internal Helpers
	
	//andeeee from the Unity forum's steller Catmull-Rom class ( http://forum.unity3d.com/viewtopic.php?p=218400#218400 ):
	private class CRSpline {
		public Vector3[] pts;
		
		public CRSpline(params Vector3[] pts) {
			this.pts = new Vector3[pts.Length];
			Array.Copy(pts, this.pts, pts.Length);
		}
		
		
		public Vector3 Interp(float t) {
			int numSections = pts.Length - 3;
			int currPt = Mathf.Min(Mathf.FloorToInt(t * (float) numSections), numSections - 1);
			float u = t * (float) numSections - (float) currPt;
			Vector3 a = pts[currPt];
			Vector3 b = pts[currPt + 1];
			Vector3 c = pts[currPt + 2];
			Vector3 d = pts[currPt + 3];
			return .5f*((-a+3f*b-3f*c+d)*(u*u*u)+(2f*a-5f*b+4f*c-d)*(u*u)+(-a+c)*u+2f*b);
		}	
	}	
	
	//catalog new tween and add component phase of iTween:
	static void Launch(GameObject target, Hashtable args){
		if(!args.Contains("id")){
			args["id"] = GenerateID();
		}
		if(!args.Contains("target")){
			args["target"] = target;
		}		
		tweens.Insert(0,args);
		target.AddComponent("iTween");
	}		
	
	//cast any accidentally supplied doubles and ints as floats as iTween only uses floats internally and unify parameter case:
	static Hashtable CleanArgs(Hashtable args){
		Hashtable argsCopy = new Hashtable(args.Count);
		Hashtable argsCaseUnified = new Hashtable(args.Count);
		
		foreach (DictionaryEntry item in args) {
			argsCopy.Add(item.Key, item.Value);
		}
		
		foreach (DictionaryEntry item in argsCopy) {
			if(item.Value.GetType() == typeof(System.Int32)){
				int original = (int)item.Value;
				float casted = (float)original;
				args[item.Key] = casted;
			}
			if(item.Value.GetType() == typeof(System.Double)){
				double original = (double)item.Value;
				float casted = (float)original;
				args[item.Key] = casted;
			}
		}	
		
		//unify parameter case:
		foreach (DictionaryEntry item in args) {
			argsCaseUnified.Add(item.Key.ToString().ToLower(), item.Value);
		}	
		
		//swap back case unification:
		args = argsCaseUnified;
				
		return args;
	}	
	
	//random ID generator:
	static string GenerateID(){
		int strlen = 15;
		char[] chars = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','0','1','2','3','4','5','6','7','8'};
		int num_chars = chars.Length - 1;
		string randomChar = "";
		for (int i = 0; i < strlen; i++) {
			randomChar += chars[(int)Mathf.Floor(UnityEngine.Random.Range(0,num_chars))];
		}
		return randomChar;
	}	
	
	//grab and set generic, neccesary iTween arguments:
	void RetrieveArgs(){
		foreach (Hashtable item in tweens) {
			if((GameObject)item["target"] == gameObject){
				tweenArguments=item;
				break;
			}
		}
		
		id=(string)tweenArguments["id"];
		type=(string)tweenArguments["type"];
		method=(string)tweenArguments["method"];
               
		if(tweenArguments.Contains("time")){
			time=(float)tweenArguments["time"];
		}else{
			time=Defaults.time;
		}
               
		if(tweenArguments.Contains("delay")){
			delay=(float)tweenArguments["delay"];
		}else{
			delay=Defaults.delay;
		}
				
		if(tweenArguments.Contains("looptype")){
			//allows loopType to be set as either an enum(C# friendly) or a string(JS friendly), string case usage doesn't matter to further increase usability:
			if(tweenArguments["looptype"].GetType() == typeof(LoopType)){
				loopType=(LoopType)tweenArguments["looptype"];
			}else{
				try {
					loopType=(LoopType)Enum.Parse(typeof(LoopType),(string)tweenArguments["looptype"],true); 
				} catch {
					Debug.LogWarning("iTween: Unsupported loopType supplied! Default will be used.");
					loopType = iTween.LoopType.none;	
				}
			}			
		}else{
			loopType = iTween.LoopType.none;	
		}		
         
		if(tweenArguments.Contains("easetype")){
			//allows easeType to be set as either an enum(C# friendly) or a string(JS friendly), string case usage doesn't matter to further increase usability:
			if(tweenArguments["easetype"].GetType() == typeof(EaseType)){
				easeType=(EaseType)tweenArguments["easetype"];
			}else{
				try {
					easeType=(EaseType)Enum.Parse(typeof(EaseType),(string)tweenArguments["easetype"],true); 
				} catch {
					Debug.LogWarning("iTween: Unsupported easeType supplied! Default will be used.");
					easeType=Defaults.easeType;
				}
			}
		}else{
			easeType=Defaults.easeType;
		}
				
		if(tweenArguments.Contains("space")){
			//allows space to be set as either an enum(C# friendly) or a string(JS friendly), string case usage doesn't matter to further increase usability:
			if(tweenArguments["space"].GetType() == typeof(Space)){
				space=(Space)tweenArguments["space"];
			}else{
				try {
					space=(Space)Enum.Parse(typeof(Space),(string)tweenArguments["space"],true); 	
				} catch {
					Debug.LogWarning("iTween: Unsupported space supplied! Default will be used.");
					space = Defaults.space;
				}
			}			
		}else{
			space = Defaults.space;
		}
		
		if(tweenArguments.Contains("islocal")){
			isLocal = (bool)tweenArguments["islocal"];
		}else{
			isLocal = Defaults.isLocal;
		}
		
		//instantiates a cached ease equation reference:
		GetEasingFunction();
	}	
	
	//instantiates a cached ease equation refrence:
	void GetEasingFunction(){
		switch (easeType){
		case EaseType.easeInQuad:
			ease  = new EasingFunction(easeInQuad);
			break;
		case EaseType.easeOutQuad:
			ease = new EasingFunction(easeOutQuad);
			break;
		case EaseType.easeInOutQuad:
			ease = new EasingFunction(easeInOutQuad);
			break;
		case EaseType.easeInCubic:
			ease = new EasingFunction(easeInCubic);
			break;
		case EaseType.easeOutCubic:
			ease = new EasingFunction(easeOutCubic);
			break;
		case EaseType.easeInOutCubic:
			ease = new EasingFunction(easeInOutCubic);
			break;
		case EaseType.easeInQuart:
			ease = new EasingFunction(easeInQuart);
			break;
		case EaseType.easeOutQuart:
			ease = new EasingFunction(easeOutQuart);
			break;
		case EaseType.easeInOutQuart:
			ease = new EasingFunction(easeInOutQuart);
			break;
		case EaseType.easeInQuint:
			ease = new EasingFunction(easeInQuint);
			break;
		case EaseType.easeOutQuint:
			ease = new EasingFunction(easeOutQuint);
			break;
		case EaseType.easeInOutQuint:
			ease = new EasingFunction(easeInOutQuint);
			break;
		case EaseType.easeInSine:
			ease = new EasingFunction(easeInSine);
			break;
		case EaseType.easeOutSine:
			ease = new EasingFunction(easeOutSine);
			break;
		case EaseType.easeInOutSine:
			ease = new EasingFunction(easeInOutSine);
			break;
		case EaseType.easeInExpo:
			ease = new EasingFunction(easeInExpo);
			break;
		case EaseType.easeOutExpo:
			ease = new EasingFunction(easeOutExpo);
			break;
		case EaseType.easeInOutExpo:
			ease = new EasingFunction(easeInOutExpo);
			break;
		case EaseType.easeInCirc:
			ease = new EasingFunction(easeInCirc);
			break;
		case EaseType.easeOutCirc:
			ease = new EasingFunction(easeOutCirc);
			break;
		case EaseType.easeInOutCirc:
			ease = new EasingFunction(easeInOutCirc);
			break;
		case EaseType.linear:
			ease = new EasingFunction(linear);
			break;
		case EaseType.spring:
			ease = new EasingFunction(spring);
			break;
		case EaseType.bounce:
			ease = new EasingFunction(bounce);
			break;
		case EaseType.easeInBack:
			ease = new EasingFunction(easeInBack);
			break;
		case EaseType.easeOutBack:
			ease = new EasingFunction(easeOutBack);
			break;
		case EaseType.easeInOutBack:
			ease = new EasingFunction(easeInOutBack);
			break;
		}
	}
	
	//calculate percentage of tween based on time:
	void UpdatePercentage(){
		runningTime+=Time.deltaTime;
		if(reverse){
			percentage = 1 - runningTime/time;	
		}else{
			percentage = runningTime/time;	
		}
	}
	
	void CallBack(string callbackType){
		if (tweenArguments.Contains(callbackType) && !tweenArguments.Contains("ischild")) {
			//establish target:
			GameObject target;
			if (tweenArguments.Contains(callbackType+"target")) {
				target=(GameObject)tweenArguments[callbackType+"target"];
			}else{
				target=gameObject;	
			}
			
			//throw an error if a string wasn't passed for callback:
			if (tweenArguments[callbackType].GetType() == typeof(System.String)) {
				target.SendMessage((string)tweenArguments[callbackType],(object)tweenArguments[callbackType+"params"],SendMessageOptions.DontRequireReceiver);
			}else{
				Debug.LogError("iTween Error: Callback method references must be passed as a String!");
				Destroy (this);
			}
		}
	}
	
	void Dispose(){
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable tweenEntry = (Hashtable)tweens[i];
			if ((string)tweenEntry["id"] == id){
				tweens.RemoveAt(i);
				break;
			}
		}
		Destroy(this);
	}	
	
	void ConflictCheck(){//if a new iTween is about to run and is of the same type as an in progress iTween this will destroy the previous if the new one is NOT identical in every way or it will destroy the new iTween if they are:	
		Component[] tweens = GetComponents(typeof(iTween));
		foreach (iTween item in tweens) {
			if(item.isRunning && item.type==type){
				//step 1: check for length first since it's the fastest:
				if(item.tweenArguments.Count != tweenArguments.Count){
					item.Dispose();
					return;
				}
				
				//step 2: side-by-side check to figure out if this is an identical tween scenario to handle Update usages of iTween:
				foreach (DictionaryEntry currentProp in tweenArguments) {
					if(!item.tweenArguments.Contains(currentProp.Key)){
						item.Dispose();
						return;
					}else{
						if(!item.tweenArguments[currentProp.Key].Equals(tweenArguments[currentProp.Key]) && (string)currentProp.Key != "id"){//if we aren't comparing ids and something isn't exactly the same replace the running iTween: 
							item.Dispose();
							return;
						}
					}
				}
				
				//step 3: prevent a new iTween addition if it is identical to the currently running iTween
				Destroy(this);	
			}
		}
	}
	
	void EnableKinematic(){
		if(gameObject.GetComponent(typeof(Rigidbody))){
			if(!rigidbody.isKinematic){
				kinematic=true;
				rigidbody.isKinematic=true;
			}
		}
	}
	
	void DisableKinematic(){
		if(kinematic && rigidbody.isKinematic==true){
			kinematic=false;
			rigidbody.isKinematic=false;
		}
	}
		
	void ResumeDelay(){
		StartCoroutine("TweenDelay");
	}	
	
	#endregion	
	
	#region Easing Curves
	
	private float linear(float start, float end, float value){
		return Mathf.Lerp(start, end, value);
	}
	
	private float clerp(float start, float end, float value){
		float min = 0.0f;
		float max = 360.0f;
		float half = Mathf.Abs((max - min) / 2.0f);
		float retval = 0.0f;
		float diff = 0.0f;
		if ((end - start) < -half){
			diff = ((max - start) + end) * value;
			retval = start + diff;
		}else if ((end - start) > half){
			diff = -((max - end) + start) * value;
			retval = start + diff;
		}else retval = start + (end - start) * value;
		return retval;
    }

	private float spring(float start, float end, float value){
		value = Mathf.Clamp01(value);
		value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
		return start + (end - start) * value;
	}

	private float easeInQuad(float start, float end, float value){
		end -= start;
		return end * value * value + start;
	}

	private float easeOutQuad(float start, float end, float value){
		end -= start;
		return -end * value * (value - 2) + start;
	}

	private float easeInOutQuad(float start, float end, float value){
		value /= .5f;
		end -= start;
		if (value < 1) return end / 2 * value * value + start;
		value--;
		return -end / 2 * (value * (value - 2) - 1) + start;
	}

	private float easeInCubic(float start, float end, float value){
		end -= start;
		return end * value * value * value + start;
	}

	private float easeOutCubic(float start, float end, float value){
		value--;
		end -= start;
		return end * (value * value * value + 1) + start;
	}

	private float easeInOutCubic(float start, float end, float value){
		value /= .5f;
		end -= start;
		if (value < 1) return end / 2 * value * value * value + start;
		value -= 2;
		return end / 2 * (value * value * value + 2) + start;
	}

	private float easeInQuart(float start, float end, float value){
		end -= start;
		return end * value * value * value * value + start;
	}

	private float easeOutQuart(float start, float end, float value){
		value--;
		end -= start;
		return -end * (value * value * value * value - 1) + start;
	}

	private float easeInOutQuart(float start, float end, float value){
		value /= .5f;
		end -= start;
		if (value < 1) return end / 2 * value * value * value * value + start;
		value -= 2;
		return -end / 2 * (value * value * value * value - 2) + start;
	}

	private float easeInQuint(float start, float end, float value){
		end -= start;
		return end * value * value * value * value * value + start;
	}

	private float easeOutQuint(float start, float end, float value){
		value--;
		end -= start;
		return end * (value * value * value * value * value + 1) + start;
	}

	private float easeInOutQuint(float start, float end, float value){
		value /= .5f;
		end -= start;
		if (value < 1) return end / 2 * value * value * value * value * value + start;
		value -= 2;
		return end / 2 * (value * value * value * value * value + 2) + start;
	}

	private float easeInSine(float start, float end, float value){
		end -= start;
		return -end * Mathf.Cos(value / 1 * (Mathf.PI / 2)) + end + start;
	}

	private float easeOutSine(float start, float end, float value){
		end -= start;
		return end * Mathf.Sin(value / 1 * (Mathf.PI / 2)) + start;
	}

	private float easeInOutSine(float start, float end, float value){
		end -= start;
		return -end / 2 * (Mathf.Cos(Mathf.PI * value / 1) - 1) + start;
	}

	private float easeInExpo(float start, float end, float value){
		end -= start;
		return end * Mathf.Pow(2, 10 * (value / 1 - 1)) + start;
	}

	private float easeOutExpo(float start, float end, float value){
		end -= start;
		return end * (-Mathf.Pow(2, -10 * value / 1) + 1) + start;
	}

	private float easeInOutExpo(float start, float end, float value){
		value /= .5f;
		end -= start;
		if (value < 1) return end / 2 * Mathf.Pow(2, 10 * (value - 1)) + start;
		value--;
		return end / 2 * (-Mathf.Pow(2, -10 * value) + 2) + start;
	}

	private float easeInCirc(float start, float end, float value){
		end -= start;
		return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
	}

	private float easeOutCirc(float start, float end, float value){
		value--;
		end -= start;
		return end * Mathf.Sqrt(1 - value * value) + start;
	}

	private float easeInOutCirc(float start, float end, float value){
		value /= .5f;
		end -= start;
		if (value < 1) return -end / 2 * (Mathf.Sqrt(1 - value * value) - 1) + start;
		value -= 2;
		return end / 2 * (Mathf.Sqrt(1 - value * value) + 1) + start;
	}

	private float bounce(float start, float end, float value){
		value /= 1f;
		end -= start;
		if (value < (1 / 2.75f)){
			return end * (7.5625f * value * value) + start;
		}else if (value < (2 / 2.75f)){
			value -= (1.5f / 2.75f);
			return end * (7.5625f * (value) * value + .75f) + start;
		}else if (value < (2.5 / 2.75)){
			value -= (2.25f / 2.75f);
			return end * (7.5625f * (value) * value + .9375f) + start;
		}else{
			value -= (2.625f / 2.75f);
			return end * (7.5625f * (value) * value + .984375f) + start;
		}
	}

	private float easeInBack(float start, float end, float value){
		end -= start;
		value /= 1;
		float s = 1.70158f;
		return end * (value) * value * ((s + 1) * value - s) + start;
	}

	private float easeOutBack(float start, float end, float value){
		float s = 1.70158f;
		end -= start;
		value = (value / 1) - 1;
		return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
	}

	private float easeInOutBack(float start, float end, float value){
		float s = 1.70158f;
		end -= start;
		value /= .5f;
		if ((value) < 1){
			s *= (1.525f);
			return end / 2 * (value * value * (((s) + 1) * value - s)) + start;
		}
		value -= 2;
		s *= (1.525f);
		return end / 2 * ((value) * value * (((s) + 1) * value + s) + 2) + start;
	}

	private float punch(float amplitude, float value){
		float s = 9;
		if (value == 0){
			return 0;
		}
		if (value == 1){
			return 0;
		}
		float period = 1 * 0.3f;
		s = period / (2 * Mathf.PI) * Mathf.Asin(0);
		return (amplitude * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * 1 - s) * (2 * Mathf.PI) / period));
    }
	#endregion	
	
	#region Deprecated and Renamed
	/*
	public static void audioFrom(GameObject target, Hashtable args){Debug.LogError("iTween Error: audioFrom() has been renamed to AudioFrom().");}
	public static void audioTo(GameObject target, Hashtable args){Debug.LogError("iTween Error: audioTo() has been renamed to AudioTo().");}
	public static void colorFrom(GameObject target, Hashtable args){Debug.LogError("iTween Error: colorFrom() has been renamed to ColorFrom().");}
	public static void colorTo(GameObject target, Hashtable args){Debug.LogError("iTween Error: colorTo() has been renamed to ColorTo().");}
	public static void fadeFrom(GameObject target, Hashtable args){Debug.LogError("iTween Error: fadeFrom() has been renamed to FadeFrom().");}
	public static void fadeTo(GameObject target, Hashtable args){Debug.LogError("iTween Error: fadeTo() has been renamed to FadeTo().");}
	public static void lookFrom(GameObject target, Hashtable args){Debug.LogError("iTween Error: lookFrom() has been renamed to LookFrom().");}
	public static void lookFromWorld(GameObject target, Hashtable args){Debug.LogError("iTween Error: lookFromWorld() has been deprecated. Please investigate LookFrom().");}
	public static void lookTo(GameObject target, Hashtable args){Debug.LogError("iTween Error: lookTo() has been renamed to LookTo().");}
	public static void lookToUpdate(GameObject target, Hashtable args){Debug.LogError("iTween Error: lookToUpdate() has been renamed to LookUpdate().");}
	public static void lookToUpdateWorld(GameObject target, Hashtable args){Debug.LogError("iTween Error: lookToUpdateWorld() has been deprecated. Please investigate LookUpdate().");}
	public static void moveAdd(GameObject target, Hashtable args){Debug.LogError("iTween Error: moveAdd() has been renamed to MoveAdd().");}
	public static void moveAddWorld(GameObject target, Hashtable args){Debug.LogError("iTween Error: moveAddWorld() has been deprecated. Please investigate MoveAdd().");}
	public static void moveBy(GameObject target, Hashtable args){Debug.LogError("iTween Error: moveBy() has been renamed to MoveBy().");}
	public static void moveByWorld(GameObject target, Hashtable args){Debug.LogError("iTween Error: moveAddWorld() has been deprecated. Please investigate MoveAdd().");}
	public static void moveFrom(GameObject target, Hashtable args){Debug.LogError("iTween Error: moveFrom() has been renamed to MoveFrom().");}
	public static void moveFromWorld(GameObject target, Hashtable args){Debug.LogError("iTween Error: moveFromWorld() has been deprecated. Please investigate MoveFrom().");}
	public static void moveTo(GameObject target, Hashtable args){Debug.LogError("iTween Error: moveTo() has been renamed to MoveTo().");}
	public static void moveToBezier(GameObject target, Hashtable args){Debug.LogError("iTween Error: moveToBezier() has been deprecated. Please investigate MoveTo() and the "path" property.");}
	public static void moveToBezierWorld(GameObject target, Hashtable args){Debug.LogError("iTween Error: moveToBezierWorld() has been deprecated. Please investigate MoveTo() and the "path" property.");}
	public static void moveToUpdate(GameObject target, Hashtable args){Debug.LogError("iTween Error: moveToUpdate() has been renamed to MoveUpdate().");}
	public static void moveToUpdateWorld(GameObject target, Hashtable args){Debug.LogError("iTween Error: moveToUpdateWorld() has been deprecated. Please investigate MoveUpdate().");}
	public static void moveToWorld(GameObject target, Hashtable args){Debug.LogError("iTween Error: moveToWorld() has been deprecated. Please investigate MoveTo().");}
	public static void punchPosition(GameObject target, Hashtable args){Debug.LogError("iTween Error: punchPosition() has been renamed to PunchPosition().");}
	public static void punchPositionWorld(GameObject target, Hashtable args){Debug.LogError("iTween Error: punchPositionWorld() has been deprecated. Please investigate PunchPosition().");}	
	public static void punchRotation(GameObject target, Hashtable args){Debug.LogError("iTween Error: punchPosition() has been renamed to PunchRotation().");}
	public static void punchRotationWorld(GameObject target, Hashtable args){Debug.LogError("iTween Error: punchRotationWorld() has been deprecated. Please investigate PunchRotation().");}	
	public static void punchScale(GameObject target, Hashtable args){Debug.LogError("iTween Error: punchScale() has been renamed to PunchScale().");}
	public static void rotateAdd(GameObject target, Hashtable args){Debug.LogError("iTween Error: rotateAdd() has been renamed to RotateAdd().");}
	public static void rotateBy(GameObject target, Hashtable args){Debug.LogError("iTween Error: rotateBy() has been renamed to RotateBy().");}
	public static void rotateByWorld(GameObject target, Hashtable args){Debug.LogError("iTween Error: rotateByWorld() has been deprecated. Please investigate RotateBy().");}
	public static void rotateFrom(GameObject target, Hashtable args){Debug.LogError("iTween Error: rotateFrom() has been renamed to RotateFrom().");}
	public static void rotateTo(GameObject target, Hashtable args){Debug.LogError("iTween Error: rotateTo() has been renamed to RotateTo().");}
	public static void scaleAdd(GameObject target, Hashtable args){Debug.LogError("iTween Error: scaleAdd() has been renamed to ScaleAdd().");}
	public static void scaleBy(GameObject target, Hashtable args){Debug.LogError("iTween Error: scaleBy() has been renamed to ScaleBy().");}
	public static void scaleFrom(GameObject target, Hashtable args){Debug.LogError("iTween Error: scaleFrom() has been renamed to ScaleFrom().");}
	public static void scaleTo(GameObject target, Hashtable args){Debug.LogError("iTween Error: scaleTo() has been renamed to ScaleTo().");}
	public static void shake(GameObject target, Hashtable args){Debug.LogError("iTween Error: scale() has been deprecated. Please investigate ShakePosition(), ShakeRotation() and ShakeScale().");}
	public static void shakeWorld(GameObject target, Hashtable args){Debug.LogError("iTween Error: shakeWorld() has been deprecated. Please investigate ShakePosition(), ShakeRotation() and ShakeScale().");}
	public static void stab(GameObject target, Hashtable args){Debug.LogError("iTween Error: stab() has been renamed to Stab().");}
	public static void stop(GameObject target, Hashtable args){Debug.LogError("iTween Error: stop() has been renamed to Stop().");}
	public static void stopType(GameObject target, Hashtable args){Debug.LogError("iTween Error: stopType() has been deprecated. Please investigate Stop().");}
	public static void tweenCount(GameObject target, Hashtable args){Debug.LogError("iTween Error: tweenCount() has been deprecated. Please investigate Count().");}
	*/
	#endregion
} 