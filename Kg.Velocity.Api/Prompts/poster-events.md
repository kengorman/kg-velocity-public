You are generating fun journey milestones for an "Absurd Travel Simulator" poster.

Goal: Make travelers viscerally feel the absurdity of cosmic travel as they arrive at their destination.

Tone: Slightly dark comedy meets genuine wonder. Punchy, surprising, a little unsettling.

Journey Data:
- From: Earth
- To: {{Destination}}
- Departed: {{DepartedTime}}
- Distance: {{DistanceLightYears}} light-years ({{DistanceMiles}} miles)
- Speed: {{SpeedName}} ({{PercentageOfLightSpeed}}% of light speed)
- Earth time elapsed: {{EarthTimeFormatted}}
- Ship time elapsed: {{ShipTimeFormatted}}
- Time dilation difference: {{TimeDifference}}
- Focus: {{JourneyFocus}}

Generate 5–7 events relating to the journey. Emphasize the journey's focus ({{JourneyFocus}}) in your events.

Each event must be factually derivable from the journey data above.
Each event must focus on a different dimension of absurdity.
Avoid generic phrases such as “civilizations rose and fell.”
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

At least one event must highlight something that did NOT meaningfully change.
When describing something that “didn’t change,” explicitly state relative to what.
Exactly one event should be surprising in which fact it chooses to emphasize.

Each event needs:
- "text": Short punchy title (5 - 8 words)
- "description": Brief explanation (under 100 characters)

Return ONLY valid JSON:
{"events": [{"text": "...", "description": "..."}, ...]}
