using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace PorgramChat
{
    public class WebsoketConnection : MonoBehaviour
    {
        public GameObject loginPanal;
        public Image profilePic;
        public Image profileOnChatPic;
        public Text userNameOnChat;
        [SerializeField] private Sprite[] pic_sprites;
        int profileID;

        private WebSocket webSocket;

        public InputField clientInput;
        public InputField userNameInput;

        string userName;
        MessageData data;

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
            data = new MessageData();
            loginPanal.SetActive(true);
            webSocket = new WebSocket("ws://127.0.0.1:25500/");

            webSocket.OnMessage += OnMessage;

            //webSocket.Connect();
            //webSocket.Send("Test");
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
            data.clientName = userName;
            data.message = clientInput.text;
            data.profileClientID = profileID;

            clientInput.text = "";

            string data_json = JsonUtility.ToJson(data);
            Debug.Log(data_json);
            webSocket.Send(data_json);
        }

        public void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            string loadMessageData_json = messageEventArgs.Data;
            MessageData loadData = JsonUtility.FromJson<MessageData>(loadMessageData_json);
            Text currentText = Instantiate(textPrefabs, rect.position, Quaternion.identity, newTextParant);
            
            if (loadData.clientName == userName)
            {
                Image spawnProfile = Instantiate(profilefabs,
                    spawnPicRightPos.position, Quaternion.identity, 
                    currentText.gameObject.transform);
                currentText.text = $"{loadData.clientName} : {loadData.message}";
                currentText.alignment = TextAnchor.MiddleRight;
                spawnProfile.sprite = pic_sprites[loadData.profileClientID];
            }
            else
            {               
                Image spawnProfile = Instantiate(profilefabs,
                    spawnPicLeftPos.position, Quaternion.identity,
                    currentText.gameObject.transform);
                currentText.text = $"{loadData.clientName} : {loadData.message}";
                currentText.alignment = TextAnchor.MiddleLeft;
                spawnProfile.sprite = pic_sprites[loadData.profileClientID];
            }

            //print(oldText.Count);
            for(int i = 0; i < oldText.Count; i++)
            {
                oldText[i].gameObject.transform.position =
                    new Vector3(oldText[i].transform.position.x,
                    oldText[i].transform.position.y + 50,
                    oldText[i].transform.position.x);
            }

            oldText.Add(currentText);

        }


        public void OnClickLoginBtn()
        {
            webSocket.Connect();
            loginPanal.SetActive(false);
            userName = userNameInput.text;
            userNameOnChat.text = userName;
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
    }

    public class MessageData
    {
        public string clientName;
        public string message;
        public int profileClientID;
    }

}
