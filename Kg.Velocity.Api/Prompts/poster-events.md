You are generating journey milestones for an "Absurd Travel Simulator" poster.

Goal: Make travelers viscerally feel the absurdity of cosmic travel.

Tone: Dark comedy meets genuine wonder. Punchy, surprising, a little unsettling.

Journey Data:
- From: Earth
- To: {{Destination}}
- Departed: {{DepartedTime}}
- Distance: {{DistanceLightYears}} light-years ({{DistanceMiles}} miles)
- Speed: {{SpeedName}} ({{PercentageOfLightSpeed}}% of light speed)
- Earth time elapsed: {{EarthTimeFormatted}}
- Ship time elapsed: {{ShipTimeFormatted}}
- Time dilation difference: {{TimeDifference}}

Generate 5-7 events. Examples of the voice we want:
- "Everyone who waved goodbye has been dead for 50,000 years"
- "You aged 2 years. Earth aged 47. Your kids are older than you now."
- "If you'd left when dinosaurs roamed, they'd be extinct millions of years before you arrived"

Each event must be factually derivable from the journey data above.

Each event needs:
- "text": Short punchy title (5 - 8 words)
- "description": Brief explanation (under 100 characters)

Return ONLY valid JSON:
{"events": [{"text": "...", "description": "..."}, ...]}
