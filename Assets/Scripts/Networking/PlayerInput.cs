using Unity.NetCode;
using Unity.Networking.Transport;

public struct PlayerInput : ICommandData<PlayerInput> {
    public uint Tick => tick;
    public uint tick;
    public int horizontal;
    public int vertical;

    public void Deserialize(uint tick, DataStreamReader reader, ref DataStreamReader.Context ctx) {
        this.tick = tick;
        horizontal = reader.ReadInt(ref ctx);
        vertical = reader.ReadInt(ref ctx);
    }

    public void Serialize(DataStreamWriter writer) {
        writer.Write(horizontal);
        writer.Write(vertical);
    }

    public void Deserialize(uint tick, DataStreamReader reader, ref DataStreamReader.Context ctx, PlayerInput baseline,
        NetworkCompressionModel compressionModel) {
        Deserialize(tick, reader, ref ctx);
    }

    public void Serialize(DataStreamWriter writer, PlayerInput baseline, NetworkCompressionModel compressionModel) {
        Serialize(writer);
    }
}

public class PlayerInputSendCommandSystem : CommandSendSystem<PlayerInput> { }

public class PlayerInputReceiveCommandSystem : CommandReceiveSystem<PlayerInput> { }
