using Godot;
using System;
using System.Collections.Generic;

public partial class LLMService : Node
{
	private string ApiKey { get; set; } = "sk-b650ad0f84f14c4590d5b4d149c8e66f";
	private string Model { get; set; } = "deepseek-chat";
	private const string API_URL = "https://api.deepseek.com/v1/chat/completions";
	
	private HttpRequest httpRequest;
	private bool isProcessing = false;
	
	// Queue system
	private Queue<QueuedRequest> requestQueue = new Queue<QueuedRequest>();
	
	// Store current callback separately (not in metadata)
	private Action<string> currentCallback;
	
	// Helper class to store queued requests
	private class QueuedRequest
	{
		public string Message { get; set; }
		public Action<string> Callback { get; set; }
	}
	
	public override void _Ready()
	{
		httpRequest = new HttpRequest();
		AddChild(httpRequest);
		httpRequest.RequestCompleted += OnRequestCompleted;
	}
	
	public void SendMessage(string message, Action<string> callback)
	{
		// Add request to queue
		requestQueue.Enqueue(new QueuedRequest 
		{ 
			Message = message, 
			Callback = callback 
		});
		
		GD.Print($"📝 Request queued. Queue size: {requestQueue.Count}");
		
		// Process next request if not busy
		ProcessNextRequest();
	}
	
	private void ProcessNextRequest()
	{
		// If already processing or queue is empty, return
		if (isProcessing || requestQueue.Count == 0)
		{
			return;
		}
		
		// Get next request from queue
		var request = requestQueue.Dequeue();
		isProcessing = true;
		
		// Store callback for this request
		currentCallback = request.Callback;
		
		GD.Print($"🚀 Processing request. Remaining in queue: {requestQueue.Count}");
		
		var headers = new string[]
		{
			"Content-Type: application/json",
			$"Authorization: Bearer {ApiKey}"
		};
		
		var body = new Godot.Collections.Dictionary
		{
			["model"] = Model,
			["messages"] = new Godot.Collections.Array
			{
				new Godot.Collections.Dictionary
				{
					["role"] = "user",
					["content"] = request.Message
				}
			}
		};
		
		string jsonBody = Json.Stringify(body);
		httpRequest.Request(API_URL, headers, HttpClient.Method.Post, jsonBody);
	}
	
	private void OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
	{
		isProcessing = false;
		
		if (responseCode == 200)
		{
			string jsonResponse = body.GetStringFromUtf8();
			var json = Json.ParseString(jsonResponse).AsGodotDictionary();
			
			var choices = json["choices"].AsGodotArray();
			var firstChoice = choices[0].AsGodotDictionary();
			var message = firstChoice["message"].AsGodotDictionary();
			string content = message["content"].AsString();
			
			currentCallback?.Invoke(content);
		}
		else
		{
			GD.PrintErr($"❌ API Error: {responseCode}");
			currentCallback?.Invoke("Error");
		}
		
		// Process next request in queue
		ProcessNextRequest();
	}
	
	// Optional: Get current queue size
	public int GetQueueSize()
	{
		return requestQueue.Count;
	}
	
	// Optional: Clear the queue
	public void ClearQueue()
	{
		requestQueue.Clear();
		GD.Print("🗑️ Queue cleared");
	}
}