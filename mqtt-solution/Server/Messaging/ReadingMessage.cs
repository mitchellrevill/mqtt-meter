namespace Server.Messaging;

public record ReadingMessage(string UserId, DateTime TimestampUtc, double KwhSinceLast);