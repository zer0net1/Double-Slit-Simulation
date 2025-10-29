# Double-Slit-Simulation
An interactive simulation of the double-slit interference experiment, implemented in C# (Razor Pages).  
It visualizes how light behaves when passing through narrow slits and produces an interference pattern on a screen.

---

## How to run
I host the project on render so you can access it any time at:

**https://double-slit-simulation.onrender.com/**

If the demo is slow to load or you want to run it locally on your machine you may use **docker** instead:

Copy the project from GitHub to your machine:

```bash
git clone https://github.com/zer0net1/Double-Slit-Simulation.git
```
From the repository root, run these commands:
```bash
docker build -f DoubleSlitSimulation/Dockerfile -t doubleslitsim .
docker run --rm -p 8080:8080 -e ASPNETCORE_URLS="http://+:8080" doubleslitsim
```
In your browser go to:
```bash
http://localhost:8080
```

---

## Physical Principle

The simulation is based on classical wave interference of monochromatic light.  
When plane wave of wavelength **λ** passes through two slits of width **a**, separated by a distance **d**, the light diffracts and overlaps on a distant screen, forming an interference pattern of bright and dark fringes.

---

## Implementation Overview

The simulation is structured into three layers:

### **1. Frontend (Razor Pages)**
- User enters parameters: wavelength, slit width, distance, etc.  
- Parameters are converted into a JSON object.  
- JSON is sent via AJAX (JavaScript `fetch`)

### **2. Backend (Simulation API)**
- Controller (`SimulationController.cs`) receives the request.  
- Deserializes JSON into `SimulationParameters`.  
- Creates an instance of `DoubleSlitSimulator`.  
- Computes simulation results.  
- Returns a `SimulationResult` as JSON.

### **3. Frontend Display**
- Receives JSON intensity array from the backend.  
- Draws the result using HTML `<canvas>`. 
- Applies color mapping from the returned RGB data.

---

##  Simulation Modes

The simulator supports two calculation modes:

1. **Fraunhofer (Approximate Analytical)**  
2. **Huygens–Fresnel (Discrete Numerical Integration)**

---

###  1. Fraunhofer Diffraction Approximation

Valid when the observation screen is in the far field.

The intensity is computed using the analytical expression:

$$
\ I(x) = I₀ \cdot \cos^2\left(\frac{\pi \cdot d \cdot \sin\theta}{\lambda}\right) \cdot \left[ \frac{\sin\left(\frac{\pi \cdot a \cdot \sin\theta}{\lambda}\right)}{\frac{\pi \cdot a \cdot \sin\theta}{\lambda}} \right]^2 \
$$

where:  
- **d** — slit separation  
- **a** — slit width  
- **λ** — wavelength  
- **θ** — diffraction angle (≈ x / L)  
- **L** — distance to the screen  

This method uses the small-angle approximation (sinθ ≈ tanθ ≈ x/L), producing smooth interference fringes and running efficiently for far-field cases.

---

###  2. Huygens–Fresnel Principle

This mode performs a direct numerical integration.  
Each point along the slits acts as a secondary wave source, and the total field at a point at **x** on the screen is computed as the coherent sum of all contributions:

$$
\ U(x) = \frac{1}{i\lambda} \sum_{n} \left[ A_n \cdot e^{i \cdot k \cdot r_n} / r_n \right] \
$$

where:  
- **Aₙ** — amplitude of the *n*th secondary source  
- **rₙ** — distance from that source to point on screen at *x*  
- **k = 2π / λ** — wavenumber  

The intensity at *x* is the squared magnitude of this sum:

$$
\ I(x) = \frac{1}{\lambda^2} \cdot \left| \sum_{n} \left[ A_n \cdot e^{i \cdot k \cdot r_n} / r_n \right] \right|^2 \
$$

Letting aₙ = Aₙ / rₙ, the field can be expressed as complex phasor:

$$
\ \sum_n a_n e^{i \cdot k \cdot r_n} = \sum_n a_n \cos(k \cdot r_n) + i \sum_n a_n \sin(k \cdot r_n) \
$$

and thus:

$$
\ I(P) = \frac{1}{\lambda^2} \left[ \left( \sum_{n} a_n \cos(k \cdot r_n) \right)^2 + \left( \sum_{n} a_n \sin(k \cdot r_n) \right)^2 \right] \
$$

This formulation accurately reproduces near-field and far-field behavior.

---

##  Example Output
<img width="1440" height="684" alt="Screenshot 2025-10-28 at 7 20 30 PM" src="https://github.com/user-attachments/assets/68963cf6-dc84-4eb9-bfaa-2748bb66fa1d" />
<img width="1440" height="612" alt="Screenshot 2025-10-28 at 7 20 59 PM" src="https://github.com/user-attachments/assets/be6b4ac3-07ae-4638-9c6a-4539442f6c59" />
<img width="1440" height="667" alt="Screenshot 2025-10-28 at 7 21 53 PM" src="https://github.com/user-attachments/assets/3a405f8a-08c6-4fc6-b8e6-fd4d3417a7d5" />

