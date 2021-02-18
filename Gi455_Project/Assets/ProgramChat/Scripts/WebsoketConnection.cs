using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace PorgramChat
{
    public class WebsoketConnection : MonoBehaviour
    {
        public struct SocketEvent
        {
            public string eventName;
            public string data;

            public SocketEvent(string eventName, string data)
            {
                this.eventName = eventName;
                this.data = data;
            }
        }

        string tempMessage;

        [Header("UI Login")]
        public GameObject loginPanal;
        public Image profilePic;
        [SerializeField] private Sprite[] pic_sprites;
        int profileID;
        public InputField clientInput;
        public InputField userNameInput;

        [Header("UI Lobby")]
        public GameObject lobbyPanal;
        public GameObject erorrPopupPanal;
        public Text erorrText;
        public Button createRoomBtn;
        public Button joinRoomBtn;
        public Button backBtn;
        public InputField roomNameInputField;
        Vector3 uiLobbyUp;
        Vector3 uiLobbyDown;

        string userName;
        string roomName;
        private WebSocket webSocket;

        [Header("UI ChatRoom")]
        public Image profileOnChatPic;
        public Text userNameOnChat;
        public Text roomNameOnChat;
        public Text textPrefabs;
        public Image profilefabs;
        public Transform rect;
        public Transform spawnPicRightPos;
        public Transform spawnPicLeftPos;
        public Transform newTextParant;
        List<Text> oldText = new List<Text>();


        void Start()
        {
            profileID = 0;
            loginPanal.SetActive(true);
            webSocket = new WebSocket("ws://127.0.0.1:25500/");

            webSocket.OnMessage += OnMessage;

            innitLobby();
            //webSocket.Connect();
            //webSocket.Send("Test");
        }

        private void Update()
        {
            UpdateMessageFormServer();
        }

        private void OnDestroy()
        {
            if (webSocket != null)
            {
                webSocket.Close();
            }

        }

        public void OnSendMessage()
        {
            if (!string.IsNullOrEmpty(clientInput.text))
            {
                MessageData mesData = new MessageData(roomName,userName, clientInput.text,profileID);
                clientInput.text = "";
                string mesData_json = JsonUtility.ToJson(mesData);
                //Debug.Log(mesData_json);

                SocketEvent dataToServer = new SocketEvent("SendMessage", mesData_json);
                string dataToServerStr = JsonUtility.ToJson(dataToServer);
                webSocket.Send(dataToServerStr);
                Debug.Log("Send ---> " + dataToServerStr);
            }            
        }

        public void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            tempMessage = messageEventArgs.Data;
        }

        private void UpdateMessageFormServer()
        {
            if(string.IsNullOrEmpty(tempMessage) == false)
            {
                //string loadMessageData_json = tempMessage;
                Debug.Log("Get ---> " + tempMessage);
                SocketEvent loadData = JsonUtility.FromJson<SocketEvent>(tempMessage);

                if (loadData.eventName == "SendMessage") 
                {
                    MessageData loadMessageData = JsonUtility.FromJson<MessageData>(loadData.data);
                    Text currentText = Instantiate(textPrefabs, rect.position, Quaternion.identity, newTextParant);

                    if (loadMessageData.clientName == userName)
                    {
                        Image spawnProfile = Instantiate(profilefabs,
                            spawnPicRightPos.position, Quaternion.identity,
                            currentText.gameObject.transform);
                        currentText.text = $"{loadMessageData.clientName} : {loadMessageData.message}";
                        currentText.alignment = TextAnchor.MiddleRight;
                        spawnProfile.sprite = pic_sprites[loadMessageData.profileClientID];
                    }
                    else
                    {
                        Image spawnProfile = Instantiate(profilefabs,
                            spawnPicLeftPos.position, Quaternion.identity,
                            currentText.gameObject.transform);
                        currentText.text = $"{loadMessageData.clientName} : {loadMessageData.message}";
                        currentText.alignment = TextAnchor.MiddleLeft;
                        spawnProfile.sprite = pic_sprites[loadMessageData.profileClientID];
                    }

                    //print(oldText.Count);
                    for (int i = 0; i < oldText.Count; i++)
                    {
                        oldText[i].gameObject.transform.position =
                            new Vector3(oldText[i].transform.position.x,
                            oldText[i].transform.position.y + 50,
                            oldText[i].transform.position.x);
                    }

                    oldText.Add(currentText);

                }
                else if (loadData.eventName == "CreateRoom")
                {
                    if(loadData.data == "CreateRoomFail")
                    {
                        //Debug.LogError("client create room fail.");
                        ShowErrorPopup("Create room fail.\nRoom Name is Found.");
                    }
                    else
                    {
                        //create room success.
                        //Debug.LogWarning("client create room success. Room Name : "+loadData.data);
                        JoinRoom(loadData.data);
                    }
                }
                else if(loadData.eventName == "JoinRoom")
                {
                    if (loadData.data == "JoinRoomFail")
                    {
                        //Debug.LogError("client Join room fail.");
                        ShowErrorPopup("Join room fail.\nRoom Name not Found.");
                    }
                    else
                    {
                        //Join room success.
                        //Debug.LogWarning("client Join room success. Room Name : " + loadData.data);
                        JoinRoom(loadData.data);
                    }
                }
                else if(loadData.eventName == "LeaveRoom")
                {
                    Debug.Log(loadData.eventName + " : " + loadData.data);
                }               

                tempMessage = "";
            }
        }


        public void OnClickLoginBtn()
        {
            loginPanal.SetActive(false);                    
            userName = userNameInput.text;
            userNameOnChat.text = userName;
            webSocket.Connect();
            SocketEvent dataToServer = new SocketEvent("ClientConnet", userName);
            string dataToServerStr = JsonUtility.ToJson(dataToServer);
            webSocket.Send(dataToServerStr);
            
            Debug.Log(userName+"has login");
        }
        public void OnClickMarioBtn()
        {
            profileID = 1;
            profilePic.sprite = pic_sprites[profileID];
            profileOnChatPic.sprite = pic_sprites[profileID];
        }
        public void OnClickNadechBtn()
        {
            profileID = 2;
            profilePic.sprite = pic_sprites[profileID];
            profileOnChatPic.sprite = pic_sprites[profileID];
        }
        public void OnClickBaifernBtn()
        {
            profileID = 3;
            profilePic.sprite = pic_sprites[profileID];
            profileOnChatPic.sprite = pic_sprites[profileID];
        }
        public void OnClickYayaBtn()
        {
            profileID = 4;
            profilePic.sprite = pic_sprites[profileID];
            profileOnChatPic.sprite = pic_sprites[profileID];
        }

        public void OnClickCreateBtn()
        {
            //send request CreateRoom to server
            if(!string.IsNullOrEmpty(roomNameInputField.text))
            {
                SocketEvent dataToServer = new SocketEvent("CreateRoom", roomNameInputField.text);
                string dataToServerStr = JsonUtility.ToJson(dataToServer);
                webSocket.Send(dataToServerStr);
            }
            else
            {
                roomNameInputField.transform.position = uiLobbyUp;
                createRoomBtn.transform.position = uiLobbyDown;
                joinRoomBtn.gameObject.SetActive(false);
                createRoomBtn.gameObject.SetActive(true);
                roomNameInputField.gameObject.SetActive(true);
                backBtn.gameObject.SetActive(true);
            }
        }

        public void OnClickJoinBtn()
        {
            //send request Join to server
            if (!string.IsNullOrEmpty(roomNameInputField.text))
            {
                SocketEvent dataToServer = new SocketEvent("JoinRoom", roomNameInputField.text);
                string dataToServerStr = JsonUtility.ToJson(dataToServer);
                webSocket.Send(dataToServerStr);
            }
            else
            {
                roomNameInputField.transform.position = uiLobbyUp;
                joinRoomBtn.transform.position = uiLobbyDown;
                createRoomBtn.gameObject.SetActive(false);
                joinRoomBtn.gameObject.SetActive(true);
                roomNameInputField.gameObject.SetActive(true);
                backBtn.gameObject.SetActive(true);
            }
        }

        public void OnClickBackBtn()
        {
            lobbyPanal.SetActive(true);
            createRoomBtn.transform.position = uiLobbyUp;
            createRoomBtn.gameObject.SetActive(true);
            joinRoomBtn.transform.position = uiLobbyDown;
            joinRoomBtn.gameObject.SetActive(true);
            roomNameInputField.gameObject.SetActive(false);
            roomNameInputField.text = "";

            backBtn.gameObject.SetActive(false);
        }

        private void innitLobby()
        {
            erorrPopupPanal.SetActive(false);
            lobbyPanal.SetActive(true);
            backBtn.gameObject.SetActive(false);
            uiLobbyUp = createRoomBtn.gameObject.transform.position;
            uiLobbyDown = joinRoomBtn.gameObject.transform.position;
           
        }

        private void JoinRoom(string roomName)
        {
            lobbyPanal.SetActive(false);
            roomNameOnChat.text = "Room : " + roomName;
            this.roomName = roomName;
        }

        public void LeaveRoom()
        {
            OnClickBackBtn();          

            //Clear Old Text
            for(int i = 0; i < oldText.Count; i++)
            {
                Destroy(oldText[i].gameObject);
            }
            oldText.Clear();
            Debug.Log("Clear OldTextList countList : " + oldText.Count);

            //send request LeaveRoom to server
            SocketEvent dataToServer = new SocketEvent("LeaveRoom", roomName);
            string dataToServerStr = JsonUtility.ToJson(dataToServer);
            Debug.Log("Send to server:" + dataToServerStr);
            webSocket.Send(dataToServerStr);
        }

        private void ShowErrorPopup(string errorMessage)
        {
            erorrPopupPanal.SetActive(true);
            erorrText.text = errorMessage;
        }

        public void OnClickOkBtnInPopup()
        {
            erorrPopupPanal.SetActive(false);
        }
    }

    public class MessageData
    {
        public string roomName;
        public string clientName;
        public string message;
        public int profileClientID;

        public MessageData(string _roomName, string _clientName, string _message, int _profileClientID)
        {
            clientName = _clientName;
            message = _message;
            profileClientID = _profileClientID;
            roomName = _roomName;
        }

    }

}
