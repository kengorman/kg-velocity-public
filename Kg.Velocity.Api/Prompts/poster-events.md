You are generating journey milestones for an "Absurd Travel Simulator" poster.

Goal: Help travelers appreciate the mind-bending scale of the universe, how slow even light speed really is, and the crazy interplay of time, speed, and distance.

Journey Data:
- From: Earth
- To: {{Destination}}
- Distance: {{DistanceLightYears}} light-years ({{DistanceMiles}} miles)
- Speed: {{SpeedName}} ({{PercentageOfLightSpeed}}% of light speed)
- Earth time elapsed: {{EarthTimeFormatted}}
- Ship time elapsed: {{ShipTimeFormatted}}
- Time dilation difference: {{TimeDifference}}

Traveler's experience (for tone):
{{Summary}}

Generate 4-6 journey events. Mix these types as appropriate for the journey:

1. **Celestial flybys** - Notable stars, nebulae, or cosmic objects the traveler would pass at this distance from Earth. Be astronomically plausible based on the distance traveled.

2. **Relativistic consequences** - What happened on Earth or in the universe during the journey due to time dilation. Only include these if the time dilation difference is significant (at least hours). Examples: generations passing, civilizations changing, cosmic events occurring.

3. **Scale perspective** - Fun facts that put the journey in perspective. How many times light circled Earth, how far sound would travel, comparisons to everyday speeds.

Keep it fun, awe-inspiring, and occasionally absurd. Each event needs:
- "text": Short punchy title (3-6 words)
- "description": One-line explanation (under 60 characters if possible)

IMPORTANT: Return ONLY valid JSON, no markdown, no explanation:
{"events": [{"text": "...", "description": "..."}, ...]}
