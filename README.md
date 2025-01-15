# Vincenty Formulae Visualization and Calculation

This project provides tools to visualize and calculate geodetic problems using Vincenty's formulae. The project consists of two components:

1. **Python Program**: Visualizes Vincenty's calculations on a map, client of the TCP server.
2. **C# Program**: Performs direct and inverse geodetic calculations using Vincenty's formulae, provides the TCP server.

The two components communicate via TCP for seamless integration.

---

## Features

### Python Visualization
- Interactive map visualization of geodetic calculations.
- Real-time updates based on data received from the C# program.
- Display of points, distances, and geodetic paths.

### C# Calculations
- Direct problem: Calculate the destination point given a starting point, initial bearing, and distance.
- Inverse problem: Calculate the distance and initial/final bearings between two points.
- Precise calculations using Vincenty's iterative formulae.

### Communication
- TCP-based communication between the Python and C# programs.
- Enables real-time data transfer and synchronization.

---

## Installation

TODO

---

## Usage

TODO

---

## Example

There are two problems in the Vincenty Forumlae, the direct and the inverse problem. Below a sample of what is required by each problem and what are their results.

### Direct Problem
1. Input:
   - Starting Point: Latitude 34.0522, Longitude -118.2437 (Los Angeles)
   - Initial Bearing: 45 degrees
   - Distance: 500 km
2. Output:
   - Destination Point: Latitude 37.4442, Longitude -114.3323

### Inverse Problem
1. Input:
   - Point 1: Latitude 34.0522, Longitude -118.2437 (Los Angeles)
   - Point 2: Latitude 36.1699, Longitude -115.1398 (Las Vegas)
2. Output:
   - Distance: 367.62 km
   - Initial Bearing: 50.85 degrees

---

## Communication Details

TODO

---

## Contributing

TODO

---

## License

TODO

---

## Acknowledgments
- Vincenty's formulae: [Wikipedia](https://en.wikipedia.org/wiki/Vincenty%27s_formulae)
