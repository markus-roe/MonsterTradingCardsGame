using MonsterTradingCardsGame.Server;
using System.Net.Sockets;

public class MockHttpServerEventArguments : HttpServerEventArguments
{
    public string ResponseContent { get; private set; }
    public int ResponseStatusCode { get; private set; }

    public MockHttpServerEventArguments()
        : base(new TcpClient(), string.Empty)
    {
        // TcpClient is initialized but not connected
    }

    public override void Reply(int status, string? payload = null)
    {
        // No actual network communication is happening here
        this.ResponseStatusCode = status;
        this.ResponseContent = payload ?? string.Empty;
    }

    // Override CloseClientConnection to prevent closing a non-connected TcpClient
    protected override void CloseClientConnection()
    {
    }
}
