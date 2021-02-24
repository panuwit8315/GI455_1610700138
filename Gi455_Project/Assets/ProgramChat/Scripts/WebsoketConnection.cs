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
        [SerializeField] string userName;
        [SerializeField] string name;
        [SerializeField] string roomName;
        [SerializeField] int profileID = 0;
        private WebSocket webSocket;

        [Header("UI Login")]
        public GameObject loginPanal;       
        public InputField loginUserNameInput;
        public InputField loginPasswordInput;

        [Header("UI Register")]
        public GameObject registerPanal;
        public InputField regisNameInput;
        public InputField regisUserNameInput;
        public InputField regisPasswordInput;
        public InputField regisRePasswordInput;
        public Image profilePic;
        [SerializeField] private Sprite[] pic_sprites;        

        [Header("UI Lobby")]
        public GameObject lobbyPanal;
        public Text lobbyText;
        public Button createRoomBtn;
        public Button joinRoomBtn;
        public Button backBtn;
        public InputField roomNameInputField;
        Vector3 uiLobbyUp;
        Vector3 uiLobbyDown;

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
        public InputField clientInput;

        [Header("UI ErorrPopup")]
        public GameObject erorrPopupPanal;
        public Text erorrText;


        void Start()
        {
            profileID = 0;
            loginPanal.SetActive(true);
            webSocket = new WebSocket("ws://127.0.0.1:25500/");

            webSocket.OnMessage += OnMessage;
            webSocket.Connect();
            loginPanal.SetActive(true);
            registerPanal.SetActive(false);
            //innitLobby();
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
                MessageData mesData = new MessageData(roomName, name,userName, clientInput.text,profileID);
                clientInput.text = "";
                string mesData_json = JsonUtility.ToJson(mesData);
                //Debug.Log(mesData_json);

                SocketEvent dataToServer = new SocketEvent("Message", mesData_json);
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

                if (loadData.eventName == "Message") 
                {
                    MessageData loadMessageData = JsonUtility.FromJson<MessageData>(loadData.data);
                    Text currentText = Instantiate(textPrefabs, rect.position, Quaternion.identity, newTextParant);

                    if (loadMessageData.userName == userName)
                    {
                        Image spawnProfile = Instantiate(profilefabs,
                            spawnPicRightPos.position, Quaternion.identity,
                            currentText.gameObject.transform);
                        currentText.text = $"{loadMessageData.name} : {loadMessageData.message}";
                        currentText.alignment = TextAnchor.MiddleRight;
                        spawnProfile.sprite = pic_sprites[loadMessageData.profileClientID];
                    }
                    else
                    {
                        Image spawnProfile = Instantiate(profilefabs,
                            spawnPicLeftPos.position, Quaternion.identity,
                            currentText.gameObject.transform);
                        currentText.text = $"{loadMessageData.name} : {loadMessageData.message}";
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
                        ShowErrorPopup("Create room fail.\nRoom Name is Found.", Color.red);
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
                        ShowErrorPopup("Join room fail.\nRoom Name not Found.", Color.red);
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
                else if(loadData.eventName == "Login")
                {
                    if(loadData.data == "LoginFail")
                    {
                        //LoginFail ShowErrorPopup
                        ShowErrorPopup("Login Fail UserName or Password Incorrect",Color.red);
                    }
                    else
                    {
                        UserData userDataOb = JsonUtility.FromJson<UserData>(loadData.data);
                        Debug.Log("Login Success UserName:"+ userDataOb.userName+" Name:"+ userDataOb.name);
                        //Debug.Log(userDataOb.userName); // + " : " + userDataOb.password + " : " + userDataOb.name + " : " + userDataOb.profileClientID.ToString());

                        userName = userDataOb.userName;                       
                        name = userDataOb.name;
                        userNameOnChat.text = userDataOb.name;
                        profileID = userDataOb.profileClientID;
                        profileOnChatPic.sprite = pic_sprites[profileID];

                        //ShowLobbyPanal
                        innitLobby();
                    }
                }
                else if (loadData.eventName == "Register")
                {
                    if(loadData.data == "RegisterSuccess")
                    {
                        registerPanal.SetActive(false);
                        ShowErrorPopup("Register Success", Color.green);
                        OnClickBackToLoginBtn();
                    }
                    else if (loadData.data == "RegisterFail")
                    {
                        ShowErrorPopup("Register Fial That UserName is Taken. Try Another", Color.red);
                    }
                }
                tempMessage = "";
            }
        }

        //*************** >>OnClick Button<< *****************
        public void OnClickLoginBtn()
        {
            //loginPanal.SetActive(false);                    
            if(string.IsNullOrEmpty(loginUserNameInput.text) || string.IsNullOrEmpty(loginPasswordInput.text))
                return;

            UserData userData = new UserData("", loginUserNameInput.text, loginPasswordInput.text, 00);
            string strUserData = JsonUtility.ToJson(userData);
            SocketEvent dataToServer = new SocketEvent("Login", strUserData);
            string dataToServerStr = JsonUtility.ToJson(dataToServer);
            webSocket.Send(dataToServerStr);
            
            Debug.Log(loginUserNameInput.text + "Request login");
        }

        public void OnClickBackToLoginBtn()
        {
            registerPanal.SetActive(false);
            loginUserNameInput.text = "";
            loginPasswordInput.text = "";
            regisNameInput.text = "";
            regisUserNameInput.text = "";
            regisPasswordInput.text = "";
            regisRePasswordInput.text = "";
            profileID = 0;
            profilePic.sprite = pic_sprites[0];
        }

        public void OnClickRegisterBtn()
        {
            if (string.IsNullOrEmpty(regisNameInput.text) &&
                string.IsNullOrEmpty(regisUserNameInput.text) &&
                string.IsNullOrEmpty(regisPasswordInput.text) &&
                string.IsNullOrEmpty(regisRePasswordInput.text))
            {
                registerPanal.SetActive(true);
                return;
            }

            if (string.IsNullOrEmpty(regisNameInput.text) ||
                string.IsNullOrEmpty(regisUserNameInput.text) ||
                string.IsNullOrEmpty(regisPasswordInput.text) ||
                string.IsNullOrEmpty(regisRePasswordInput.text))
            {
                ShowErrorPopup("Please fill out all information.", Color.red);
                return;
            }
            else
            {
                if(regisPasswordInput.text == regisRePasswordInput.text)
                {
                    UserData userData = new UserData(regisNameInput.text, 
                        regisUserNameInput.text, regisPasswordInput.text, profileID);
                    string strUserData = JsonUtility.ToJson(userData);
                    SocketEvent dataToServer = new SocketEvent("Register", strUserData);
                    string dataToServerStr = JsonUtility.ToJson(dataToServer);
                    webSocket.Send(dataToServerStr);

                    Debug.Log(regisUserNameInput.text + "Request Register");
                }
                else
                {
                    ShowErrorPopup("Password and Re-Password Not Match", Color.red);
                }
                
            }

        }

        public void OnChoosePicProFile(int profileSprite_Index)
        {
            profileID = profileSprite_Index;
            profilePic.sprite = pic_sprites[profileSprite_Index];
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
                lobbyText.text = "Create Room";
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
                lobbyText.text = "Join Room";
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
            lobbyText.text = "Create or Join Room";
            roomNameInputField.text = "";

            backBtn.gameObject.SetActive(false);
        }
        public void OnClickOkBtnInPopup()
        {
            erorrPopupPanal.SetActive(false);
        }

        public void LeaveRoom()
        {
            OnClickBackBtn();  //Back To Lobby Panal        

            //Clear Old Text
            for (int i = 0; i < oldText.Count; i++)
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


        //************* >>End Button<< ***************
        private void innitLobby()
        {
            loginPanal.SetActive(false);
            registerPanal.SetActive(false);
            erorrPopupPanal.SetActive(false);
            lobbyPanal.SetActive(true);
            backBtn.gameObject.SetActive(false);
            uiLobbyUp = createRoomBtn.gameObject.transform.position;
            uiLobbyDown = joinRoomBtn.gameObject.transform.position;
            lobbyText.text = "Create or Join Room";
        }

        private void JoinRoom(string roomName)
        {
            lobbyPanal.SetActive(false);
            roomNameOnChat.text = "Room : " + roomName;
            this.roomName = roomName;
        }

        
        private void ShowErrorPopup(string errorMessage, Color color)
        {
            
            erorrPopupPanal.gameObject.GetComponent<Image>().color = color; 
            erorrPopupPanal.SetActive(true);
            erorrText.text = errorMessage;
        }

        
    }

    public class MessageData
    {
        public string roomName;
        public string name;
        public string userName;
        public string message;
        public int profileClientID;

        public MessageData(string _roomName, string _name, string _userName, string _message, int _profileClientID)
        {
            name = _name;
            userName = _userName;
            message = _message;
            profileClientID = _profileClientID;
            roomName = _roomName;
        }
    }
    public class UserData
    {
        public string name;
        public string userName;
        public string password;
        public int profileClientID;

        public UserData(string _name, string _userName, string _password, int _profileClientID)
        {
            name = _name;
            userName = _userName;
            profileClientID = _profileClientID;
            password = _password;
        }
    }

}
