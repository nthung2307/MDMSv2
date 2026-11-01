namespace Application.DataTransferObjects.Email;

public class EmailAttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public byte[] ContentBytes { get; set; } = Array.Empty<byte>();

    public EmailAttachmentDto()
    {
    }

    public EmailAttachmentDto(string fileName, string contentType, byte[] contentBytes)
    {
        FileName = fileName;
        ContentType = contentType;
        ContentBytes = contentBytes;
    }
}
