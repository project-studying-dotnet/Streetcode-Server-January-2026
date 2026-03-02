namespace Streetcode.Shared.Contracts
{
    public interface IEmailMessage
    {
        public string From { get; }
        public string Content { get; }
    }
}
