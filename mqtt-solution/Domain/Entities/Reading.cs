using System;

namespace Domain.Entities;

public class Reading
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime TimeStamp { get; set; }

    public float Value { get; set; }

}
