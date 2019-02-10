using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Connection {
    private readonly UdpClient _udp;
    
    private readonly Thread _sendThread;
    private readonly Thread _receiveThread;

    private readonly Action<Connection, Message> _onReceiveAction;
    private readonly Queue<Message> _messages = new Queue<Message>();
    
    private IPEndPoint _remoteEp;

    public IPEndPoint RemoteEp {
        get { return _remoteEp; }
        set { _remoteEp = value; }
    }

    public Connection(Action<Connection, Message> action, IPEndPoint remoteEp) {
        _onReceiveAction = action;
        
        _remoteEp = remoteEp;
        
        _udp = new UdpClient();
        
        _receiveThread = new Thread(() => {
            Thread.Sleep(1000); // todo wtf?
            while (true) {
                var data = _udp.Receive(ref _remoteEp);
                var message = new Message(data);
                _onReceiveAction(this, message);
            }
        });
        _receiveThread.Start();
        
        _sendThread = new Thread(() => {
            while (true) {
                lock (_messages) {
                    while (_messages.Count > 0) {
                        var message = _messages.Dequeue();
                        _udp.Send(message.Bytes, message.Bytes.Length, _remoteEp);
                    }
                }
                
                Thread.Sleep(100);
            }
        });
        _sendThread.Start();
    }

    public void Send(Message message) {
        lock (_messages) {
            _messages.Enqueue(message);
        }
    }
    
    public void Close() {
        _sendThread.Abort();
        _receiveThread.Abort();
        _udp.Close();
    }

    ~Connection() {
        Close();
    }
}