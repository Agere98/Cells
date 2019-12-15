using Unity.Networking.Transport;
using Unity.NetCode;
using Unity.Mathematics;

public struct NetPlayerParticleSnapshotData : ISnapshotData<NetPlayerParticleSnapshotData>
{
    public uint tick;
    private int NetworkPlayerPlayerId;
    private int ScoreValue;
    private int RotationValueX;
    private int RotationValueY;
    private int RotationValueZ;
    private int RotationValueW;
    private int TranslationValueX;
    private int TranslationValueY;
    private int TranslationValueZ;
    uint changeMask0;

    public uint Tick => tick;
    public int GetNetworkPlayerPlayerId(GhostDeserializerState deserializerState)
    {
        return (int)NetworkPlayerPlayerId;
    }
    public int GetNetworkPlayerPlayerId()
    {
        return (int)NetworkPlayerPlayerId;
    }
    public void SetNetworkPlayerPlayerId(int val, GhostSerializerState serializerState)
    {
        NetworkPlayerPlayerId = (int)val;
    }
    public void SetNetworkPlayerPlayerId(int val)
    {
        NetworkPlayerPlayerId = (int)val;
    }
    public int GetScoreValue(GhostDeserializerState deserializerState)
    {
        return (int)ScoreValue;
    }
    public int GetScoreValue()
    {
        return (int)ScoreValue;
    }
    public void SetScoreValue(int val, GhostSerializerState serializerState)
    {
        ScoreValue = (int)val;
    }
    public void SetScoreValue(int val)
    {
        ScoreValue = (int)val;
    }
    public quaternion GetRotationValue(GhostDeserializerState deserializerState)
    {
        return GetRotationValue();
    }
    public quaternion GetRotationValue()
    {
        return new quaternion(RotationValueX * 0.001f, RotationValueY * 0.001f, RotationValueZ * 0.001f, RotationValueW * 0.001f);
    }
    public void SetRotationValue(quaternion q, GhostSerializerState serializerState)
    {
        SetRotationValue(q);
    }
    public void SetRotationValue(quaternion q)
    {
        RotationValueX = (int)(q.value.x * 1000);
        RotationValueY = (int)(q.value.y * 1000);
        RotationValueZ = (int)(q.value.z * 1000);
        RotationValueW = (int)(q.value.w * 1000);
    }
    public float3 GetTranslationValue(GhostDeserializerState deserializerState)
    {
        return GetTranslationValue();
    }
    public float3 GetTranslationValue()
    {
        return new float3(TranslationValueX * 0.01f, TranslationValueY * 0.01f, TranslationValueZ * 0.01f);
    }
    public void SetTranslationValue(float3 val, GhostSerializerState serializerState)
    {
        SetTranslationValue(val);
    }
    public void SetTranslationValue(float3 val)
    {
        TranslationValueX = (int)(val.x * 100);
        TranslationValueY = (int)(val.y * 100);
        TranslationValueZ = (int)(val.z * 100);
    }

    public void PredictDelta(uint tick, ref NetPlayerParticleSnapshotData baseline1, ref NetPlayerParticleSnapshotData baseline2)
    {
        var predictor = new GhostDeltaPredictor(tick, this.tick, baseline1.tick, baseline2.tick);
        NetworkPlayerPlayerId = predictor.PredictInt(NetworkPlayerPlayerId, baseline1.NetworkPlayerPlayerId, baseline2.NetworkPlayerPlayerId);
        ScoreValue = predictor.PredictInt(ScoreValue, baseline1.ScoreValue, baseline2.ScoreValue);
        RotationValueX = predictor.PredictInt(RotationValueX, baseline1.RotationValueX, baseline2.RotationValueX);
        RotationValueY = predictor.PredictInt(RotationValueY, baseline1.RotationValueY, baseline2.RotationValueY);
        RotationValueZ = predictor.PredictInt(RotationValueZ, baseline1.RotationValueZ, baseline2.RotationValueZ);
        RotationValueW = predictor.PredictInt(RotationValueW, baseline1.RotationValueW, baseline2.RotationValueW);
        TranslationValueX = predictor.PredictInt(TranslationValueX, baseline1.TranslationValueX, baseline2.TranslationValueX);
        TranslationValueY = predictor.PredictInt(TranslationValueY, baseline1.TranslationValueY, baseline2.TranslationValueY);
        TranslationValueZ = predictor.PredictInt(TranslationValueZ, baseline1.TranslationValueZ, baseline2.TranslationValueZ);
    }

    public void Serialize(int networkId, ref NetPlayerParticleSnapshotData baseline, DataStreamWriter writer, NetworkCompressionModel compressionModel)
    {
        changeMask0 = (NetworkPlayerPlayerId != baseline.NetworkPlayerPlayerId) ? 1u : 0;
        changeMask0 |= (ScoreValue != baseline.ScoreValue) ? (1u<<1) : 0;
        changeMask0 |= (RotationValueX != baseline.RotationValueX ||
                                           RotationValueY != baseline.RotationValueY ||
                                           RotationValueZ != baseline.RotationValueZ ||
                                           RotationValueW != baseline.RotationValueW) ? (1u<<2) : 0;
        changeMask0 |= (TranslationValueX != baseline.TranslationValueX ||
                                           TranslationValueY != baseline.TranslationValueY ||
                                           TranslationValueZ != baseline.TranslationValueZ) ? (1u<<3) : 0;
        writer.WritePackedUIntDelta(changeMask0, baseline.changeMask0, compressionModel);
        if ((changeMask0 & (1 << 0)) != 0)
            writer.WritePackedIntDelta(NetworkPlayerPlayerId, baseline.NetworkPlayerPlayerId, compressionModel);
        if ((changeMask0 & (1 << 1)) != 0)
            writer.WritePackedIntDelta(ScoreValue, baseline.ScoreValue, compressionModel);
        if ((changeMask0 & (1 << 2)) != 0)
        {
            writer.WritePackedIntDelta(RotationValueX, baseline.RotationValueX, compressionModel);
            writer.WritePackedIntDelta(RotationValueY, baseline.RotationValueY, compressionModel);
            writer.WritePackedIntDelta(RotationValueZ, baseline.RotationValueZ, compressionModel);
            writer.WritePackedIntDelta(RotationValueW, baseline.RotationValueW, compressionModel);
        }
        if ((changeMask0 & (1 << 3)) != 0)
        {
            writer.WritePackedIntDelta(TranslationValueX, baseline.TranslationValueX, compressionModel);
            writer.WritePackedIntDelta(TranslationValueY, baseline.TranslationValueY, compressionModel);
            writer.WritePackedIntDelta(TranslationValueZ, baseline.TranslationValueZ, compressionModel);
        }
    }

    public void Deserialize(uint tick, ref NetPlayerParticleSnapshotData baseline, DataStreamReader reader, ref DataStreamReader.Context ctx,
        NetworkCompressionModel compressionModel)
    {
        this.tick = tick;
        changeMask0 = reader.ReadPackedUIntDelta(ref ctx, baseline.changeMask0, compressionModel);
        if ((changeMask0 & (1 << 0)) != 0)
            NetworkPlayerPlayerId = reader.ReadPackedIntDelta(ref ctx, baseline.NetworkPlayerPlayerId, compressionModel);
        else
            NetworkPlayerPlayerId = baseline.NetworkPlayerPlayerId;
        if ((changeMask0 & (1 << 1)) != 0)
            ScoreValue = reader.ReadPackedIntDelta(ref ctx, baseline.ScoreValue, compressionModel);
        else
            ScoreValue = baseline.ScoreValue;
        if ((changeMask0 & (1 << 2)) != 0)
        {
            RotationValueX = reader.ReadPackedIntDelta(ref ctx, baseline.RotationValueX, compressionModel);
            RotationValueY = reader.ReadPackedIntDelta(ref ctx, baseline.RotationValueY, compressionModel);
            RotationValueZ = reader.ReadPackedIntDelta(ref ctx, baseline.RotationValueZ, compressionModel);
            RotationValueW = reader.ReadPackedIntDelta(ref ctx, baseline.RotationValueW, compressionModel);
        }
        else
        {
            RotationValueX = baseline.RotationValueX;
            RotationValueY = baseline.RotationValueY;
            RotationValueZ = baseline.RotationValueZ;
            RotationValueW = baseline.RotationValueW;
        }
        if ((changeMask0 & (1 << 3)) != 0)
        {
            TranslationValueX = reader.ReadPackedIntDelta(ref ctx, baseline.TranslationValueX, compressionModel);
            TranslationValueY = reader.ReadPackedIntDelta(ref ctx, baseline.TranslationValueY, compressionModel);
            TranslationValueZ = reader.ReadPackedIntDelta(ref ctx, baseline.TranslationValueZ, compressionModel);
        }
        else
        {
            TranslationValueX = baseline.TranslationValueX;
            TranslationValueY = baseline.TranslationValueY;
            TranslationValueZ = baseline.TranslationValueZ;
        }
    }
    public void Interpolate(ref NetPlayerParticleSnapshotData target, float factor)
    {
        SetRotationValue(math.slerp(GetRotationValue(), target.GetRotationValue(), factor));
        SetTranslationValue(math.lerp(GetTranslationValue(), target.GetTranslationValue(), factor));
    }
}
