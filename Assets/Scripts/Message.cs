using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Message {
    public MessageType Type { get; }
    public byte[] Bytes { get; }

    public string String => Encoding.Unicode.GetString(Bytes, 1, Bytes.Length - 1);
    
    public ConnectData ConnectData {
        get {
            var idBytes = new byte[4];
            var serverPortBytes = new byte[4];
                
            Array.Copy(Bytes, 1, idBytes, 0, 4);
            Array.Copy(Bytes, 5, serverPortBytes, 0, 4);

            var id = BitConverter.ToInt32(idBytes, 0);
            var serverPort = BitConverter.ToInt32(serverPortBytes, 0);
                
            return new ConnectData(id, serverPort);
        }
    }

    public FieldPositions FieldPositions {
        get {
            var fieldPositions = new FieldPositions();
            
            var bytesArray = new List<byte>(Bytes);
            bytesArray.RemoveRange(0, 1);

            while (bytesArray.Count > 0) {
                var type = (FieldPositionType) bytesArray[0];
                bytesArray.RemoveRange(0, 1);
                
                var x = BitConverter.ToSingle(bytesArray.GetRange(0, 4).ToArray(), 0);
                bytesArray.RemoveRange(0, 4);
                                
                var y = BitConverter.ToSingle(bytesArray.GetRange(0, 4).ToArray(), 0);
                bytesArray.RemoveRange(0, 4);

                var id = BitConverter.ToInt32(bytesArray.GetRange(0, 4).ToArray(), 0);
                bytesArray.RemoveRange(0, 4);

                fieldPositions.Add(new FieldPosition(type, new Vector2(x, y), id));
            }

            return fieldPositions;
        }
    }

    public ServerInfo ServerInfo {
        get {
            var bytesArray = new List<byte>(Bytes);
            bytesArray.RemoveRange(0, 1);

            var pCount = BitConverter.ToInt32(bytesArray.GetRange(0, 4).ToArray(), 0);
            bytesArray.RemoveRange(0, 4);

            var fCount = BitConverter.ToInt32(bytesArray.GetRange(0, 4).ToArray(), 0);
            bytesArray.RemoveRange(0, 4);

            return new ServerInfo(pCount, fCount);
        }
    }
    
    public Message(MessageType type, object data) {
        Type = type;

        Console.WriteLine($"type: {type}");
            
        var msg = new List<byte>();
            
        msg.Add((byte) type);

        switch (type) {
            case MessageType.Register:
                break;
            case MessageType.ConnectData:
                msg.AddRange(BitConverter.GetBytes(((ConnectData) data).Id));
                msg.AddRange(BitConverter.GetBytes(((ConnectData) data).ServerPort));
                break;
            case MessageType.Text:
                msg.AddRange(Encoding.Unicode.GetBytes((string) data));
                break;
            case MessageType.Vector2:
                var vector = (Vector2) data;
                msg.AddRange(BitConverter.GetBytes((float) vector.x));
                msg.AddRange(BitConverter.GetBytes((float) vector.y));
                break;
            case MessageType.Int:
                msg.AddRange(BitConverter.GetBytes((int) data));
                break;
            case MessageType.Ping:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        Bytes = msg.ToArray();
    }

    public Message(byte[] data) {
        Type = (MessageType) data[0];
        Bytes = data;
    }
    
    public override string ToString() {
        return $"Message: {{type: {Type}}}";
    }
}