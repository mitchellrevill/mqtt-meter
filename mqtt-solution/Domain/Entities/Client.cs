using System;

namespace Domain.Entities;

public class Client
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public List<Reading> Reading { get; set; } = new List<Reading>();
}