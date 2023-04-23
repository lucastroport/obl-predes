using F1.Domain.Model;

namespace F1.Domain.Comparator;

public class MessageDateComparer : IComparer<Message>
{
    public int Compare(Message x, Message y)
    {
        return x.Date.CompareTo(y.Date);
    }
}