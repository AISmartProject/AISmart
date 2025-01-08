namespace AISmart.Dto;

public class BindTwitterAccountGEventDto:BaseEventDto
{
    public string UserId { get; set; }
    public string Token { get; set; }
    public string TokenSecret { get; set; }
}