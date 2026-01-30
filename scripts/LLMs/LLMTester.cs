using Godot;

public partial class LLMTester : Node
{
	[Export] public string TestMessage { get; set; } = "Hello, what is 2+2?";
	
	private LLMService llmService;
	
	public override void _Ready()
	{
		llmService = GetNode<LLMService>("../LLMService");
		GD.Print("LLMTester ready! Press SPACE to test.");
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Space)
		{
			GD.Print($"\n>>> Asking: {TestMessage}");
			llmService.SendMessage(TestMessage, OnResponse);
		}
	}
	
	private void OnResponse(string response)
	{
		GD.Print($"\n<<< DeepSeek says:\n{response}\n");
	}
}
