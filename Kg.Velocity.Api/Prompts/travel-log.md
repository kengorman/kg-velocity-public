You are generating a flight log for an "Absurd Travel Simulator".

Goal: Make travelers feel the seriousness and interesting facts of the journey.

Journey Weights (these represent the psychological dimensions of the journey - let them guide which aspects to emphasize):
- Emotion: {{WeightEmotion}}%
- Distance: {{WeightDistance}}%
- Awe: {{WeightAwe}}%
- TimeGoneBy: {{WeightTimeGoneBy}}%
- Memories: {{WeightMemories}}%
- Patience: {{WeightPatience}}%
- Loneliness: {{WeightLoneliness}}%

Let the weights shape your event selection naturally:
- Higher weights should influence which dimensions of interesting topics you emphasize
- Lower weights can be touched on briefly or omitted
- Do not explicitly name the weights or percentages in your events

Journey Data:
- From: Earth
- To: {{Destination}}
- Departed: {{DepartedTime}}
- Distance: {{DistanceLightYears}} light-years ({{DistanceMiles}} miles)
- Speed: {{SpeedName}} ({{PercentageOfLightSpeed}}% of light speed)
- Earth time elapsed: {{EarthTimeFormatted}}
- Ship time elapsed: {{ShipTimeFormatted}}
- Time dilation difference: {{TimeDifference}}

Generate 5–7 events relating to the journey.

Each event must be factually derivable from the journey data above.
Each event must focus on a different dimension of the trip.
Avoid generic phrases such as "civilizations rose and fell."
Do not use scientific abbreviations (e.g., c, ly, AU).
Use plain language comparisons understandable to a non-technical reader.

Possible dimensions include:
- Human lifespan / generations
- Geological or biological change
- Stellar or galactic motion
- Relativistic effects
- Historical or evolutionary comparison
- Communication or isolation
- Scale comparisons

Exactly one event should be surprising in which fact it chooses to emphasize.

Each event needs:
- "text": Brief explanation (under 100 characters)

Return ONLY valid JSON:
{"events": [{"text": "..."}, ...]}
