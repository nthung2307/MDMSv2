using Nobisoft.Core.Interfaces;
using Nobisoft.Core.SnowflakeId;

namespace Nobisoft.Core.Implementations;

public class SnowflakeIdService : ISnowflakeIdService
{
    // Let's say we take april 1st 2020 as our epoch
    //private readonly DateTime _epoch = new DateTime(2020, 4, 1, 0, 0, 0, DateTimeKind.Utc);

    // Create an ID with 45 bits for timestamp, 2 for generator-id 
    // and 16 for sequence
    //private readonly IdStructure _structure = new IdStructure(41, 10, 12);

    // Prepare options
    //private readonly IdGeneratorOptions _options = new IdGeneratorOptions(new IdStructure(41, 10, 12), new DefaultTimeSource(new DateTime(2020, 4, 1, 0, 0, 0, DateTimeKind.Utc)));

    // Create an IdGenerator with it's generator-id set to 0, our custom epoch 
    // and id-structure


    /// <summary>
    /// This config is for 54bit only, to use 64bit, change generatorIdBits to 10 and sequenceBits is 12
    /// </summary>
    private readonly IdGenerator _generator = new IdGenerator(0, new IdGeneratorOptions(new IdStructure(41, 5, 8), new DefaultTimeSource(new DateTime(2020, 4, 1, 0, 0, 0, DateTimeKind.Utc)), SequenceOverflowStrategy.SpinWait));

    public async Task<long> GenerateId(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return _generator.CreateId();
    }

    public async Task<long> TryCreateId(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        while (true)
        {
            if (_generator.TryCreateId(out var id))
            {
                return id;
            }

            // Handle sequence overflow by waiting for the next millisecond
            Thread.Sleep(1);
        }
    }

    public long Max()
    {
        return long.MaxValue;
    }

    public long Min()
    {
        return long.MinValue;
    }
}