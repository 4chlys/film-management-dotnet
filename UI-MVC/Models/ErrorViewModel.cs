namespace FilmManagement.UI.Web.Models;

public class ErrorViewModel
{
    public string RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public int StatusCode { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }

    public ErrorViewModel()
    {
        StatusCode = 500;
        Title = "Error";
        Message = "An error occurred while processing your request.";
    }
 
    public ErrorViewModel(int statusCode, string title, string message)
    {
        StatusCode = statusCode;
        Title = title;
        Message = message;
    }
}