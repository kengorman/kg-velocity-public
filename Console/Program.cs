using Velocity;

/*
 * A relativistic spaceship simulator that demonstrates special relativity effects.
 * 
 * Scenario:
 * - You are traveling in a straight line away from Earth in a spaceship
 * - You are heading toward a distant destination
 * - The simulation shows relativistic effects as you approach the speed of light
 * 
 * Display:
 * - Current speed in MPH and as a percentage of the speed of light
 * - Distance traveled from Earth (in miles and light years)
 * - Remaining distance to destination
 * - Time elapsed on Earth vs time elapsed on the ship (time dilation)
 * - Lorentz factor (gamma) and ship clock rate relative to Earth
 * 
 * Controls:
 * - X key: Accelerate (exponential growth - speed doubles approximately every 2 seconds held)
 * - W key: Decelerate (exponential decay - speed halves approximately every 2 seconds held)
 * - S key: Slow modifier - reduces acceleration/deceleration rate by 50% while held
 * - Q key: Quit
 * 
 * Note: This is a text-only simulation with no visuals.
 */

var app = new App();
app.Run();