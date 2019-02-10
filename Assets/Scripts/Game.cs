using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ui;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour {
    private const string ServerIpString = "127.0.0.1";
    private const int ServerMainPort = 57099;
    
    public UiTextList debugController;
    public UiTextList chatController;

    public UiChatInput chatInput;

    public GameObject Player;
    public GameObject CubePref;
    public GameObject FoodPref;

    public Text PingText;
    public Text PlayerCountText;
    public Text FoodCountText;
    
    private readonly Queue<string> _debugBuffer = new Queue<string>();
    private readonly Queue<string> _chatBuffer = new Queue<string>();
    
    private Connection _connection;

    private IPEndPoint _remoteEp;

    private int _id;

    private readonly object _fieldPositionsLock = new object();
    private FieldPositions _fieldPositions = null;
    private Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();
    private List<GameObject> _food = new List<GameObject>();

    private DateTime _pingStart;
    private TimeSpan _ping;

    private ServerInfo _serverInfo;

    private void Start() {
        chatInput.TextEnterEvent += SendChatText;
        
        _connection = new Connection((connection, message) => {
            switch (message.Type) {
                case MessageType.Register:
                    break;
                case MessageType.ConnectData:
                    var connectData = message.ConnectData;
                    
                    _id = connectData.Id;
                    _remoteEp = new IPEndPoint(IPAddress.Parse(ServerIpString), connectData.ServerPort);
                    break;
                case MessageType.Text:
                    AddChatText(message.String);
                    break;
                case MessageType.Vector2:
                    break;
                case MessageType.Int:
                    break;
                case MessageType.FieldPositions:
                    lock (_fieldPositionsLock) {
                        _fieldPositions = message.FieldPositions;
                    }
                    break;
                case MessageType.Ping:
                    _pingStart = DateTime.Now;
                    break;
                case MessageType.Info:
                    _serverInfo = message.ServerInfo;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }, new IPEndPoint(IPAddress.Parse(ServerIpString), ServerMainPort));
        
        StartCoroutine(RequestConn());
        StartCoroutine(CreateConn());
        StartCoroutine(DebugText());
        StartCoroutine(ChatText());
        StartCoroutine(CheckFieldPositions());
        StartCoroutine(CheckPing());
        StartCoroutine(CheckInfo());
    }

    private IEnumerator CheckInfo() {
        while (true) {
            if (_serverInfo == null) continue;
            
            PlayerCountText.text = _serverInfo.PlayerCount.ToString();
            FoodCountText.text = _serverInfo.FoodCount.ToString();
            
            yield return new WaitForSeconds(.1f);
        }
    }

    private IEnumerator CheckPing() {
        while (true) {
            _ping = DateTime.Now - _pingStart;
            PingText.text = _ping.Milliseconds.ToString();
            _connection.Send(new Message(MessageType.Ping, new byte[] { }));
            yield return null;
        }
    }

    private IEnumerator CheckFieldPositions() {
        while (true) {
            while (_fieldPositions == null || !_fieldPositions.Any()) yield return null;

            lock (_fieldPositionsLock) {
                var food = new Dictionary<Vector2, GameObject>();
                foreach (var f in _food) {
                    food.Add(f.transform.position, f);
                }

                foreach (var fieldPosition in _fieldPositions) {
                    switch (fieldPosition.Type) {
                        case FieldPositionType.Player:
                            if (fieldPosition.Id == _id) {
                                Player.transform.position = fieldPosition.Position;
                            }
                            
                            if (players.ContainsKey(fieldPosition.Id)) {
                                players[fieldPosition.Id].transform.position = fieldPosition.Position;
                            } else {
                                var playerObject = Instantiate(CubePref, fieldPosition.Position, Quaternion.identity);
                                players.Add(fieldPosition.Id, playerObject);
                            }

                            break;
                        case FieldPositionType.Food:
                            if (!food.ContainsKey(fieldPosition.Position)) {
                                var fo = Instantiate(FoodPref, fieldPosition.Position, Quaternion.identity);
                                _food.Add(fo);
                                food.Add(fieldPosition.Position, fo);
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                foreach (var missedId in players.Keys.Except(_fieldPositions.Select(fp => fp.Id).ToArray()).ToArray()) {
                    Destroy(players[missedId]);
                    players.Remove(missedId);
                }

                foreach (var missedPos in food.Keys.Except(_fieldPositions.Select(fp => fp.Position).ToArray()).ToArray()) {
                    _food.Remove(food[missedPos]);
                    Destroy(food[missedPos]);
                }

                _fieldPositions = null;
            }
        }
    }

    private void Update() {
        if (_connection == null) return;
        if (chatInput.InputField.isFocused) return;
        
        var direction = Vector2.zero;
        
        if (Input.GetKey(KeyCode.W)) {
            direction += Vector2.up;
        }

        if (Input.GetKey(KeyCode.S)) {
            direction += Vector2.down;
        }

        if (Input.GetKey(KeyCode.A)) {
            direction += Vector2.left;
        }

        if (Input.GetKey(KeyCode.D)) {
            direction += Vector2.right;
        }

        direction = direction.normalized;

        if (direction != Vector2.zero) {
            AddDebugText($"Move: {direction}");
            _connection.Send(new Message(MessageType.Vector2, direction));
        }
    }
    
    private void OnApplicationQuit() {
        _connection.Close();
    }

    private void AddDebugText(string text) {
        _debugBuffer.Enqueue(text);
    }

    private void AddChatText(string text) {
        _chatBuffer.Enqueue(text);
    }

    private void SendChatText(string text) {
        _connection.Send(new Message(MessageType.Text, text));
    }

    private IEnumerator RequestConn() {
        while (_remoteEp == null) {
            AddDebugText("Trying connect...");
            _connection.Send(new Message(MessageType.Register, new byte[] {}));
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator CreateConn() {
        while (_remoteEp == null) yield return null;

        _connection.RemoteEp = _remoteEp;
        
        AddDebugText("Connected");
    }

    private IEnumerator DebugText() {
        while (true) {
            while (_debugBuffer.Count > 0) {
                var text = _debugBuffer.Dequeue();
                debugController.Add(text);
            }
            yield return null;
        }
    }

    private IEnumerator ChatText() {
        while (true) {
            while (_chatBuffer.Count > 0) {
                var text = _chatBuffer.Dequeue();
                chatController.Add(text);
            }
            yield return null;
        }
    }
}