using System;

namespace Domain.Entities;

public class Reading
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime TimeStamp { get; set; } = DateTime.Now;

    public float Value { get; set; }

}
