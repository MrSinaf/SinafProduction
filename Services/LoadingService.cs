namespace SinafProduction.Services;

public class LoadingService
{
	public event Action onChange = () => { };
	
	private object? source;
	
	private bool _isLoading;
	
	public bool IsLoading
	{
		get => _isLoading;
		private set
		{
			_isLoading = value;
			onChange();
		}
	}
	
	public void Show(object source)
	{
		IsLoading = true;
		this.source = source;
	}
	
	public void Hide(object source)
	{
		if (this.source != source)
			return;
		
		IsLoading = false;
		this.source = null;
	}
	
	public string GetStyle() => IsLoading ? "display: none;" : "display: initial;";
}