namespace DoubleSlitSimulation.Computing;
public class SimulationResult(double[] intensity, double physicalPixelSize, int[] colour)
{
    public double[] Intensity { get; set; } = intensity;
    public double PhysicalPixelSize { get; set; } = physicalPixelSize;
    public int[] Colour { get; set; } = colour;
}    