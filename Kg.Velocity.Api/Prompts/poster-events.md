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

Generate 4-6 journey events. Mix these types as appropriate for the journey and the summary:

1. **Celestial notables** - Notable stars, nebulae, or cosmic objects relative to the journey. Be astronomically plausible based on the distance traveled.

2. **Relativistic consequences** - What happened on Earth or in the universe during the journey due to time dilation. Only include these if the time dilation difference is significant (at least hours). Examples: generations passing, civilizations changing, cosmic events occurring.

3. **Scale perspective** - Fun and scientific facts that put the journey into a simplified view. Such as comparisons to everyday speeds, historic events of the earth and universe, etc..

Keep it fun, awe-inspiring, and understandable to the common man. Always be accurate. 
Each event needs:
- "text": Short punchy title (3-5 words)
- "description": One-line explanation in past tense (under 60 characters if possible)

IMPORTANT: Return ONLY valid JSON, no markdown, no explanation:
{"events": [{"text": "...", "description": "..."}, ...]}
