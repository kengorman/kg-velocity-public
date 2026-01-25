You are generating short excerpts from a personal travel log,
written in the first person by an ordinary traveler recording observations
during an unusual space journey.

The traveler is curious, not a scientist, and writes informally
to remember what is changing around him or her, moments after it happens.

First-person style rules:
- Begin just a few entries with "I".
- Vary sentence openings using direct observations
- Use "I" to describe noticing, seeing, losing, crossing, or being unsure
- Avoid describing emotions or internal feelings
- Do not describe fear, sadness, joy, or longing
- Do not use metaphors or symbolic language

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
- High Awe → emphasize surprise at scale, sky changes, and loss of familiar reference
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

Generate 4–5 short log entries. Order them chronologically.

Each entry should be one or two short sentences, or a brief sentence fragment,
as if written quickly to capture what just changed.

Prefer observations that reveal:
- unexpected changes the traveler did not anticipate
- loss of familiar reference or scale
- limits of perception or visibility
- things becoming harder to see, track, or compare
- time and distance no longer matching expectations
- communication or timing no longer behaving intuitively
- irreversible changes that cannot be undone

Awe should appear only as quiet surprise, loss of reference, or recognition
that something no longer fits everyday experience.
Do not use emotion words, metaphors, or grand descriptions.

Use at most one metaphor or poetic phrase across the entire log.
All other lines should remain literal and observational.

Return ONLY valid JSON:
{"events":[{"text":"..."}, ...]}
