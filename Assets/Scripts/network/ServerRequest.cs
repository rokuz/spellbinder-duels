using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ServerRequest : MonoBehaviour
{
    private const string kServerUrl = "http://127.0.0.1:8080/api/";

    public delegate void OnResponse(WWW www);

    public void Send(string command, Dictionary<string, string> parameters, OnResponse onResponseHandler)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(kServerUrl);
        builder.Append(command);
        builder.Append("?");
        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                builder.Append(p.Key);
                builder.Append("=");
                builder.Append(p.Value);
                builder.Append("&");
            }
        }
        builder.Append("signature=");
        builder.Append(CalculateSignature());

        //Debug.Log("Request: " + builder.ToString());
        StartCoroutine(WaitForRequest(new WWW(builder.ToString()), onResponseHandler));
    }
        
    private IEnumerator WaitForRequest(WWW www, OnResponse onResponseHandler)
    {
        yield return www;
        if (onResponseHandler != null)
            onResponseHandler(www);
    }

    private string CalculateSignature() {
        return "123";
    }
}
