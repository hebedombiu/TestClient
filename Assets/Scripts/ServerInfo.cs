public class ServerInfo {
    public int PlayerCount { get; }
    public int FoodCount { get; }

    public ServerInfo(int playerCount, int foodCount) {
        PlayerCount = playerCount;
        FoodCount = foodCount;
    }
}