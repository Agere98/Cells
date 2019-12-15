using Unity.Entities;

[GenerateAuthoringComponent]
public struct Primer : IComponentData {
    public double TimerLB;
    public double TimerUB;

    public Primer(double timerLB, double timerUB) {
        TimerLB = timerLB;
        TimerUB = timerUB;
    }
}
