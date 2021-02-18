var websocket = require('ws');

var websocketServer = new websocket.Server({port:25500},()=>{
    console.log("Tong Server is running");
});


//var wsList = [];
var roomList = [];
/*
{
    roomName: ""
    wsList: []
}
*/

websocketServer.on("connection",(ws,rq)=>{

    //wsList.push(ws);
    //console.log(`Client ${wsList.length}: Connected!!`);

    ws.on("message",(data)=>{
        console.log("Get Data :"+data);

        var toJsonObj = 
        { 
            eventName:"",
            data:""
        }
        toJsonObj = JSON.parse(data);
        //console.log(toJsonObj.eventName)

        //SendMessage
        if(toJsonObj.eventName == "SendMessage")
        {
            console.log("Client request SendMessage")
            var MessageJsonObj = JSON.parse(toJsonObj.data);
            console.log("Client Send Message to Room: " + MessageJsonObj.roomName);
            Boardcast(MessageJsonObj.roomName,data);
        }

        //ClientConnet
        else if(toJsonObj.eventName == "ClientConnet")
        {
            console.log(`Client : [${toJsonObj.data}] Connected`)
        }

        //CreateRoom
        else if(toJsonObj.eventName == "CreateRoom")
        {
            console.log("Client request CreateRoom")
            var isFoundRoom = false;
            for(var i = 0; i < roomList.length;i++)
            {
                if(roomList[i].roomName == toJsonObj.data){
                    isFoundRoom = true;
                    break;
                }
            }

            if(isFoundRoom == true)
            {
                //can not create room
                //call back result to client
                var callbackMsg = {
                    eventName:"CreateRoom",
                    data:"CreateRoomFail"
                }
                var toJsonStr = JSON.stringify(callbackMsg);
                ws.send(toJsonStr);
                
                console.log("client create room fail.");
            }
            else
            {
                //Create Room
                var newRoom = {
                    roomName: toJsonObj.data,
                    wsList: []
                }

                newRoom.wsList.push(ws);

                roomList.push(newRoom);

                //call back result to client
                var callbackMsg = {
                    eventName:"CreateRoom",
                    data:toJsonObj.data
                }
                var toJsonStr = JSON.stringify(callbackMsg);
                ws.send(toJsonStr);
               
                console.log(`client create ${toJsonObj.data} room success`);
            }
        }

        //JoinRoom
        else if(toJsonObj.eventName == "JoinRoom")
        {
            console.log("Client request JoinRoom")
            var isFoundRoom = false;
            for(var i = 0; i < roomList.length;i++)
            {
                if(roomList[i].roomName == toJsonObj.data){
                    isFoundRoom = true;
                    //Join room
                    roomList[i].wsList.push(ws);
                    break;
                }
            }

            if(isFoundRoom == true)
            {
                //call back result to client
                var callbackMsg = {
                    eventName:"JoinRoom",
                    data:toJsonObj.data
                }
                var toJsonStr = JSON.stringify(callbackMsg);
                ws.send(toJsonStr);
                
                console.log("client Join room fail.");
            }
            else
            {
                //Can not join room
                //call back result to client
                var callbackMsg = {
                    eventName:"JoinRoom",
                    data:"JoinRoomFail"
                }
                var toJsonStr = JSON.stringify(callbackMsg);
                ws.send(toJsonStr);
               
                console.log(`client Join ${toJsonObj.data} room success`);
            }
        }

        //LeaveRoom
        else if(toJsonObj.eventName == "LeaveRoom")//LeaveRoom
        {
            //============ Find client in room for remove client out of room ================
            var isLeaveSuccess = false;//Set false to default.
            for(var i = 0; i < roomList.length; i++)//Loop in roomList
            {
                for(var j = 0; j < roomList[i].wsList.length; j++)//Loop in wsList in roomList
                {
                    if(ws == roomList[i].wsList[j])//If founded client.
                    {
                        roomList[i].wsList.splice(j, 1);//Remove at index one time. When found client.

                        if(roomList[i].wsList.length <= 0)//If no one left in room remove this room now.
                        {
                            roomList.splice(i, 1);//Remove at index one time. When room is no one left.
                        }
                        isLeaveSuccess = true;
                        break;
                    }
                }
            }
            //===============================================================================

            if(isLeaveSuccess)
            {
                //========== Send callback message to Client ============
                //I will change to json string like a client side. Please see below
                var callbackMsg = {
                    eventName:"LeaveRoom",
                    data:"success"
                }
                var toJsonStr = JSON.stringify(callbackMsg);
                ws.send(toJsonStr);
                //=======================================================
                
                console.log(`client leave ${toJsonObj.data} room success`);
            }
            else
            {
                //========== Send callback message to Client ============
                //I will change to json string like a client side. Please see below
                var callbackMsg = {
                    eventName:"LeaveRoom",
                    data:"fail"
                }
                var toJsonStr = JSON.stringify(callbackMsg);
                ws.send(toJsonStr);
                //=======================================================

                console.log("client leave room fail");
            }
        }
    })

    ws.on("close",()=>{
        //wsList = ArrayRemove(wsList,ws);
        for(var i = 0; i < roomList.length; i++)//Loop in roomList
        {
            for(var j = 0; j < roomList[i].wsList.length; j++)//Loop in wsList in roomList
            {
                if(ws == roomList[i].wsList[j])//If founded client.
                {
                    roomList[i].wsList.splice(j, 1);//Remove at index one time. When found client.

                    if(roomList[i].wsList.length <= 0)//If no one left in room remove this room now.
                    {
                        roomList.splice(i, 1);//Remove at index one time. When room is no one left.
                    }
                    break;
                }
            }
        }
        console.log("DisConnected!!");
    })
});

function ArrayRemove(arr, value)
{
    return arr.filter((element)=>{
        return element != value;
    })
}

function Boardcast(_roomName,MessageData){
    for(var i = 0; i < roomList.length; i++)
    {
        if(roomList[i].roomName == _roomName)
        {
            for(var j = 0; j < roomList[i].wsList.length; j++)
            {
                roomList[i].wsList[j].send(MessageData);
                console.log("Send to Client : " + MessageData)
            }
        }
        //wsList[i].send(data);
    }
}

