var websocket = require('ws');

var websocketServer = new websocket.Server({port:25500},()=>{
    console.log("Tong Server is running");
});


var wsList = [];
websocketServer.on("connection",(ws,rq)=>{

    wsList.push(ws);
    console.log(`Client ${wsList.length}: Connected!!`);
    ws.on("message",(data)=>{
        console.log("Send Data :"+data);
        Boardcast(data);
    })

    ws.on("close",()=>{
        wsList = ArrayRemove(wsList,ws);
        console.log("DisConnected!!");

        /*for(var i = 0; i < wsList.length; i++)
        {
            if(wsList[i]==ws){
                wsList[i].send(data);
            }
            
        }*/
    })
});

function ArrayRemove(arr, value)
{
    return arr.filter((element)=>{
        return element != value;
    })
}

function Boardcast(data){
    for(var i = 0; i < wsList.length; i++)
    {
        wsList[i].send(data);
    }
}

