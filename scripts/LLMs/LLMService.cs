using Godot;
using System;

public partial class LLMService : Node
{
	[Export] public string ApiKey { get; set; } = "your-api-key-here";
	[Export] public string Model { get; set; } = "deepseek-chat";
	
	private const string API_URL = "https://api.deepseek.com/v1/chat/completions";
	private HttpRequest httpRequest;
	private Action<string> currentCallback;
	private bool isProcessing = false;
	
	public override void _Ready()
	{
		httpRequest = new HttpRequest();
		AddChild(httpRequest);
		httpRequest.RequestCompleted += OnRequestCompleted;
	}
	
	public void SendMessage(string message, Action<string> callback)
	{
		if (isProcessing)
		{
			GD.Print("⏳ Already processing...");
			return;
		}
		
		isProcessing = true;
		currentCallback = callback;
		
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
					["content"] = message
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
	}
}
 
