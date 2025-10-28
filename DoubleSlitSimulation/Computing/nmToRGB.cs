namespace DoubleSlitSimulation.Computing;

public static class NmToRgb
{
    public static int[] MainMethod(double wavelength) {
        const double gamma = 0.80;
        const int intensityMax = 255;
        double factor, red, green, blue;
        int[] colour = new int[3];

        if (wavelength < 380 || wavelength > 780)
        {
            colour[0] = colour[1] = colour[2] = 46;
        }
        else
        {
            if (wavelength < 440) {
                red = -(wavelength - 440.0) / (440 - 380);
                green = 0.0;
                blue = 1.0;
            } else if (wavelength < 490) {
                red = 0.0;
                green = (wavelength - 440.0) / (490 - 440);
                blue = 1.0;
            } else if (wavelength < 510) {
                red = 0.0;
                green = 1.0;
                blue = -(wavelength - 510.0) / (510 - 490);
            } else if (wavelength < 580) {
                red = (wavelength - 510.0) / (580 - 510);
                green = 1.0;
                blue = 0.0;
            } else if (wavelength < 645) {
                red = 1.0;
                green = -(wavelength - 645.0) / (645 - 580);
                blue = 0.0;
            } else
            {
                red = 1.0;
                green = 0.0;
                blue = 0.0;
            }

            if (wavelength < 420)
                factor = 0.3 + 0.7 * (wavelength - 380) / (420 - 380);
            else if (wavelength < 701)
                factor = 1.0;
            else
                factor = 0.3 + 0.7 * (780 - wavelength) / (780 - 700);

            colour[0] = (int)(red != 0 ? Math.Round(intensityMax * Math.Pow(red * factor, gamma)) : 0);
            colour[1] = (int)(green != 0 ? Math.Round(intensityMax * Math.Pow(green * factor, gamma)) : 0);
            colour[2] = (int)(blue != 0 ? Math.Round(intensityMax * Math.Pow(blue * factor, gamma)) : 0);
        }

        return colour;
    }
}