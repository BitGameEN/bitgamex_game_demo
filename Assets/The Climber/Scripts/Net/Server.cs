using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour {
	// dev: https://poc-test.bit.game:18800, prod: https://poc.bit.game/svc
	public static string SERVER_URL = "https://poc-test.bit.game:18800";
	private static Server _instance;
	public static string res;
	public void Awake(){
		_instance = this;
		DontDestroyOnLoad(this);
	}

	public static Server Instance{
		get{
			return _instance;
		}
	}

	public void Get<T>(Dictionary<string,object> args,Action<T> response,Action error=null){
		StartCoroutine(Send<T>(false,args,(result)=>{
			var obj = (T)JsonUtility.FromJson(result,typeof(T));
			res = result;
			response(obj);
		},error));
	}

	public void Post<T>(Dictionary<string,object> args,Action<T> response,Action error=null){
		StartCoroutine(Send<T>(true,args,(result)=>{
			var obj = (T)JsonUtility.FromJson(result,typeof(T));
			response(obj);
		},error));
	}

	private IEnumerator Send<T>(bool isPost,Dictionary<string,object> args,Action<string> response,Action error){
		UnityEngine.Networking.UnityWebRequest www;
		if(isPost){
			var formData = new WWWForm();
			foreach(var item in args){
				if(item.Key != "a"){
					formData.AddField(item.Key,item.Value.ToString());
				}
			}
			Debug.LogWarning("str:=>"+SERVER_URL+"/?a="+args["a"]);
			www = UnityEngine.Networking.UnityWebRequest.Post(SERVER_URL+"/?a="+args["a"],formData);
		}else{
			var str = "";
			foreach(var item in args){
				str += item.Key + "=" + WWW.EscapeURL(item.Value.ToString())+"&";
			}
			Debug.LogWarning("str:=>"+SERVER_URL+"/?"+str);
			www = UnityEngine.Networking.UnityWebRequest.Get(SERVER_URL+"/?"+str);
		}
		yield return www.Send();
		if(!string.IsNullOrEmpty(www.error)){
			Debug.LogWarning(www.responseCode+"error"+www.error);
			error();
		}else if(www.isDone){
			Debug.LogWarning(www.responseCode+"result"+www.downloadHandler.text);
			response(www.downloadHandler.text);
		}else{
			Debug.LogWarning("error1"+www.error);
			error();
		}
	}
}
