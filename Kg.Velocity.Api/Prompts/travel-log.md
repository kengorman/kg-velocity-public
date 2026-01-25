You are generating excerpts from a mission flight log,
written in the first person by a non-scientific traveler, who was taking a fantastic journey.
For tense, it should be present tense but reflecting what may have just happened.

Tone:
- Human-authored (not system-generated)

Journey Weights (guide which consequences to emphasize, but never name them):
- Emotion: {{WeightEmotion}}%
- Distance: {{WeightDistance}}%
- Awe: {{WeightAwe}}%
- TimeGoneBy: {{WeightTimeGoneBy}}%
- Memories: {{WeightMemories}}%
- Patience: {{WeightPatience}}%
- Loneliness: {{WeightLoneliness}}%

Interpret the weights as:
- High Distance → emphasize scale, crossings, separation thresholds
- High TimeGoneBy → emphasize aging, calendar shifts, historical distance
- High Loneliness → emphasize isolation, communication asymmetry, signal delay
- High Awe → emphasize perceptual limits, sky changes, stellar context
- High Emotion / Memories → emphasize irreversibility and generational effects
- High Patience → emphasize long waits, delayed outcomes, slow crossings

Journey Data:
- From: Earth
- To: {{Destination}}
- Departed: {{DepartedTime}}
- Distance: {{DistanceLightYears}} light-years ({{DistanceMiles}} miles)
- Speed: {{SpeedName}} ({{PercentageOfLightSpeed}} percent of light speed)
- Earth time elapsed: {{EarthTimeFormatted}}
- Ship time elapsed: {{ShipTimeFormatted}}
- Time dilation difference: {{TimeDifference}}

Generate 4–5 flight log excerpts. Order them chronologically.

Make each excerpt no more than 3 short phrases with no more than 30 words, as if they were written in haste. Full sentences not needed.

Prefer observations that reveal:
- a true sense of awe and wonder
- cosmic commentary
- limits of perception
- breakdown of intuitive scale
- loss of symmetry between Earth and ship
- irreversible thresholds
- reference frame shifts
- objects becoming unresolvable or indistinguishable

Return ONLY valid JSON:
{"events":[{"text":"..."}, ...]}
