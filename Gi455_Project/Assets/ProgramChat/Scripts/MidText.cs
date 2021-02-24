using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

//Scriptนี้ไว้เชื่อมต่อกับเซิร์ฟของอาจาร์เท่านั้น
public class MidText : MonoBehaviour
{
    public struct SocketEvent
    {
        public string eventName;
        public string studentID;

        public SocketEvent(string eventName, string data)
        {
            this.eventName = eventName;
            this.studentID = data;
        }
    }

    private WebSocket ws;
    string tempMessage;

    // Start is called before the first frame update
    void Start()
    {
        ws = new WebSocket("ws://gi455-305013.an.r.appspot.com/");
        ws.OnMessage += OnMessage;
        ws.Connect();
    }    
    public void OnMessage(object sender, MessageEventArgs messageEventArgs)
    {
        tempMessage = messageEventArgs.Data;
        Debug.Log(tempMessage);
    }

    // Update is called once per frame
    void Update()
    {
        CheckNameStudent();
    }

    void CheckNameStudent()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
             SocketEvent dataToServer = new SocketEvent("GetStudentData", "1610702788");
             string dataToServerStr = JsonUtility.ToJson(dataToServer);
             ws.Send(dataToServerStr);
             Debug.Log("Press [P]");
        }
    }

    private void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
        }

    }
}
   

