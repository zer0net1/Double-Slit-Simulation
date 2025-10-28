namespace DoubleSlitSimulation.Computing;
 public class PointData
 {
     public double x { get; set; }
     public double y { get; set; }
 }
 
 public class ErrorResult
 {
     public List<PointData> Error { get; set; }

     public ErrorResult(double[] intensityError, int[] samples)
     {
         Error = new List<PointData>(samples.Length);
         for (int i = 0; i < samples.Length; i++)
         {
             Error.Add(new PointData
             {
                 x = samples[i],
                 y = intensityError[i]*100
             });
         }
     }
 }