namespace F1.Domain.Model;

public class Message
{
    public string Content { get; }
    public string SenderId { get; }
    public string ReceiverId { get; }
    public bool Seen { get; set; }
    
    public string DateFormatted { get;  }
    public DateTime Date { get;  }

    public Message(string content, string senderId, string receiverId)
    {
        Content = content;
        SenderId = senderId;
        ReceiverId = receiverId;
        Seen = false;
        
        DateTime now = DateTime.Now;
        Date = now;
        string formattedDateTime = now.ToString("dd/MM/yyyy - HH:mm");
        DateFormatted = formattedDateTime;
    }

    public override string ToString()
    {
        return $"{Content} ({DateFormatted})";
    }
}