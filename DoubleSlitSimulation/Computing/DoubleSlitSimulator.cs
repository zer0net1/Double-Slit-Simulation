using System.Runtime.CompilerServices;

namespace DoubleSlitSimulation.Computing;

public class DoubleSlitSimulator(SimulationParameters parameters)
{
   private static readonly double[] SlitCentersBase = { 0.5, -0.5 };
   private static readonly int[] ErrorSamples = { 5, 10, 15, 20, 25, 30, 35, 70 };
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static void NormalizeInPlace(double[] input)
   {
      double imax = 0;
      for (int i = 0; i < input.Length; i++)
         if (input[i] > imax) imax = input[i];
      if (imax <= 0) return;
      double inv = 1.0 / imax;
      for (int i = 0; i < input.Length; i++)
         input[i] *= inv;
   }

   public SimulationResult Run()
   {
      double lamda = parameters.Wavelength * 1e-9;
      double a = parameters.SlitWidth * 1e-6;
      double d = parameters.SlitSeparation * 1e-6;
      double l = parameters.ScreenDistance * 1e-2;
      int amountOfSlits = parameters.AmountOfSlits;
      int samplesPerSlit = parameters.SamplesPerSlit;
      if (amountOfSlits > 100) samplesPerSlit = 200;
      else if (amountOfSlits > 2) samplesPerSlit = 50;

      double physicalPixelSize = parameters.ScreenPhysicalWidth / parameters.PixelCount;
      double[] intensity = new double[parameters.PixelCount];

      double halfPixels = parameters.PixelCount * 0.5;
      double invL = 1.0 / l;
      
      static bool ShouldUseParallel(long estimatedWork, int pixelCount)
      {
         return estimatedWork >= 400_000 && pixelCount >= 300 && Environment.ProcessorCount > 1;
      }
      var parallelOptions = new ParallelOptions {
         MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1) 
      };

      if (!parameters.Huygens)
      {
         for (int i = 0; i < parameters.PixelCount; i++)
         {
            double x = (i - halfPixels) * physicalPixelSize;
            double sinTheta = x * invL;
            double alpha = Math.PI * d * sinTheta / lamda;
            double beta = Math.PI * a * sinTheta / lamda;

            double singleSlit = (Math.Abs(beta) < 1e-8) ? 1.0 : (Math.Sin(beta) / beta);
            double interference = Math.Cos(alpha);
            intensity[i] = interference * interference * singleSlit * singleSlit;
         }
      }
      else
      {
         long estimatedWork = (long)parameters.PixelCount * samplesPerSlit * amountOfSlits;
         bool useParallel = ShouldUseParallel(estimatedWork, parameters.PixelCount);
         double k = 2 * Math.PI / lamda;
         if (useParallel)
         {
            Console.WriteLine("use parallel");
            Parallel.For(0, parameters.PixelCount, parallelOptions, i =>
            {
               double x = (i - halfPixels) * physicalPixelSize;
               double eReal = 0.0, eImag = 0.0;

               for (int n = 0; n < amountOfSlits; n++)
               {
                  
                  double slitCenter = (n - (amountOfSlits - 1) / 2.0) * d;
                  double localOffset = a / samplesPerSlit;
                  for (int j = 0; j < samplesPerSlit; j++)
                  {
                     double srcX = slitCenter - a * 0.5 + localOffset * (j + 0.5);
                     double dx = x - srcX;
                     double r = Math.Sqrt(l * l + dx * dx);
                     double amplitude = localOffset / r;
                     double phase = k * r;
                     eReal += amplitude * Math.Cos(phase);
                     eImag += amplitude * Math.Sin(phase);
                  }
               }

               intensity[i] = eReal * eReal + eImag * eImag;
            });
         }
         else
         {
            for (int i = 0; i < parameters.PixelCount; i++)
            {
               double x = (i - halfPixels) * physicalPixelSize;
               double eReal = 0.0, eImag = 0.0;

               for (int n = 0; n < amountOfSlits; n++)
               {
                  double slitCenter = (n - (amountOfSlits - 1) / 2.0) * d;
                  double localOffset = a / samplesPerSlit;
                  for (int j = 0; j < samplesPerSlit; j++)
                  {
                     double srcX = slitCenter - a * 0.5 + localOffset * (j + 0.5);
                     double dx = x - srcX;
                     double r = Math.Sqrt(l * l + dx * dx);
                     double amplitude = localOffset / r;
                     double phase = k * r;
                     eReal += amplitude * Math.Cos(phase);
                     eImag += amplitude * Math.Sin(phase);
                  }
               }

               intensity[i] = eReal * eReal + eImag * eImag;
            }
         } 
      }
      
      NormalizeInPlace(intensity);
      int[] colour = NmToRgb.MainMethod(parameters.Wavelength);
      return new SimulationResult(intensity, physicalPixelSize, colour);
   }

   public ErrorResult RunError()
   {
      double lamda = parameters.Wavelength * 1e-9;
      double a = parameters.SlitWidth * 1e-6;
      double d = parameters.SlitSeparation * 1e-6;
      double l = parameters.ScreenDistance * 1e-2;
      double k = 2 * Math.PI / lamda;
      double physicalPixelSize = parameters.ScreenPhysicalWidth / parameters.PixelCount;
      double[] intensityF = new double[parameters.PixelCount];
      double[] intensityH = new double[parameters.PixelCount];
      double[] intensityError = new double[ErrorSamples.Length];

      double halfPixels = parameters.PixelCount * 0.5;
      double invL = 1.0 / l;

      for (int i = 0; i < parameters.PixelCount; i++)
      {
         double x = (i - halfPixels) * physicalPixelSize;
         double sinTheta = x * invL;
         double alpha = Math.PI * d * sinTheta / lamda;
         double beta = Math.PI * a * sinTheta / lamda;
         
         double singleSlit = (Math.Abs(beta) < 1e-8) ? 1.0 : (Math.Sin(beta) / beta);
         double interference = Math.Cos(alpha);
         intensityF[i] = interference * interference * singleSlit * singleSlit;
      }
      NormalizeInPlace(intensityF);
      
      static bool ShouldUseParallel(long estimatedWork, int pixelCount)
      {
          return estimatedWork >= 200_000 && pixelCount >= 300 && Environment.ProcessorCount > 1;
      }
      var parallelOptions = new ParallelOptions {
          MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1) 
      };
      for (int s = 0; s < ErrorSamples.Length; s++)
      {
          double error = 0;
          int sampleCount = ErrorSamples[s];
          long estimatedWork = (long)parameters.PixelCount * sampleCount;
          bool useParallel = ShouldUseParallel(estimatedWork, parameters.PixelCount);
          

          if (useParallel)
          {
             Parallel.For(0, parameters.PixelCount, parallelOptions, i =>
             {
                double x = (i - halfPixels) * physicalPixelSize;
                double eReal = 0.0, eImag = 0.0;

                foreach (var centerFactor in SlitCentersBase)
                {
                   double slitCenter = centerFactor * d;
                   double localOffset = a / sampleCount;
                   for (int j = 0; j < sampleCount; j++)
                   {
                      double srcX = slitCenter - a * 0.5 + localOffset * (j + 0.5);
                      double dx = x - srcX;
                      double r = Math.Sqrt(l * l + dx * dx);
                      double amplitude = localOffset / r;
                      double phase = k * r;
                      eReal += amplitude * Math.Cos(phase);
                      eImag += amplitude * Math.Sin(phase);
                   }
                }

                intensityH[i] = eReal * eReal + eImag * eImag;
             });
          }
          else
          {
             for (int i = 0; i < parameters.PixelCount; i++)
             {
                double x = (i - halfPixels) * physicalPixelSize;
                double eReal = 0.0, eImag = 0.0;

                foreach (var centerFactor in SlitCentersBase)
                {
                   double slitCenter = centerFactor * d;
                   double localOffset = a / sampleCount;
                   for (int j = 0; j < sampleCount; j++)
                   {
                      double srcX = slitCenter - a * 0.5 + localOffset * (j + 0.5);
                      double dx = x - srcX;
                      double r = Math.Sqrt(l * l + dx * dx);
                      double amplitude = localOffset / r;
                      double phase = k * r;
                      eReal += amplitude * Math.Cos(phase);
                      eImag += amplitude * Math.Sin(phase);
                   }
                }

                intensityH[i] = eReal * eReal + eImag * eImag;
             }
          } 
          NormalizeInPlace(intensityH);

          for (int i = 0; i < parameters.PixelCount; i++)
          {
             error += (intensityF[i] - intensityH[i]) * (intensityF[i] - intensityH[i]);
          }
          intensityError[s] = Math.Sqrt(error / parameters.PixelCount);
      }
      return new ErrorResult(intensityError, ErrorSamples);
   }
}