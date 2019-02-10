public struct ConnectData {
    public ConnectData(int id, int serverPort) {
        Id = id;
        ServerPort = serverPort;
    }

    public int Id { get; }
    public int ServerPort { get; }
}