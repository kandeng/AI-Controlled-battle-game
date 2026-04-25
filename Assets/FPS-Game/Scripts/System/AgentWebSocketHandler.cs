using System;
using UnityEngine;

// Note: These namespaces require websocket-sharp library
using WebSocketSharp;
using WebSocketSharp.Server;

/// <summary>
/// WebSocket behavior handler for agent connections.
/// All overrides (OnOpen, OnMessage, OnClose, OnError) run on background threads.
/// Do NOT call Unity APIs (Debug.Log, Time.time, etc.) directly here.
/// Fire events only — WebSocketServerManager queues work to the main thread.
/// </summary>
public class AgentWebSocketHandler : WebSocketBehavior
{
    // Events for server manager to subscribe
    public event Action<string> OnAgentConnected;
    public event Action<string> OnAgentDisconnected;
    public event Action<string, string> OnCommandReceived;
    
    protected override void OnOpen()
    {
        string sessionId = ID;

        // Fire event — WebSocketServerManager queues the main-thread work
        OnAgentConnected?.Invoke(sessionId);

        // Send welcome message (WebSocket Send is thread-safe)
        string welcome = "{\"type\":\"welcome\",\"message\":\"Connected to Unity WebSocket Server\",\"sessionId\":\"" + sessionId + "\"}";
        Send(welcome);
    }
    
    protected override void OnMessage(MessageEventArgs e)
    {
        string sessionId = ID;
        // Forward raw JSON — no Unity API calls here
        OnCommandReceived?.Invoke(sessionId, e.Data);
    }
    
    protected override void OnClose(CloseEventArgs e)
    {
        string sessionId = ID;
        // Fire event — WebSocketServerManager queues the main-thread work
        OnAgentDisconnected?.Invoke(sessionId);
    }
    
    protected override void OnError(ErrorEventArgs e)
    {
        // OnError runs on background thread — avoid Debug.Log here
        // WebSocketServerManager will log any dispatch errors on the main thread
        Console.Error.WriteLine($"[AgentWS] WebSocket error: {e.Message}");
    }
}
