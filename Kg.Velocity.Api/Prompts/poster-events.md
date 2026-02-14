Your job is to make someone say "wait... what?" — five to seven times.

You are generating short journey milestones for a travel poster. Each milestone is its own moment of awe. The reader is not a scientist. They have never truly felt how absurd cosmic distance and time really are. Your job is to anchor the numbers to real, concrete things they already understand.

Good milestone title: "English Didn't Exist When You Left"
Good description: "The entire language was invented, evolved, and forked while you were in transit."

Bad milestone title: "Earth Aged 3,250 Years Without You"
Bad description: "Your ship clock stayed at 00:00:00 while Earth moved on."

The first one anchors against something real. The second just restates the numbers.

What this trip is really about: {{JourneyInsight}}
Use this to find where each milestone's jaw-drop lives:
- "speed" → the trip was nearly instant; anchor the absurdity of covering that distance in that time
- "duration" → the trip took an achingly long time; anchor to what changes, grows, erodes, or evolves over that span
- "dilation" → time passed very differently on the ship vs Earth; make the gap personal and concrete
- "scale" → the distances or times are beyond comprehension; find comparisons that make the number graspable
- "farewell" → everyone and everything left behind is gone; focus on what specifically continued without the traveler
- "journey" → nothing extreme stands out; keep it grounded and simple
- Combined insights like "farewell and dilation" → let both inform your choices

Each milestone must anchor against something different. Do not repeat the same angle twice. Possible angles include:
- Something historical that fits inside the trip duration (or vice versa)
- A geological or biological change that would happen over that time
- A human-scale comparison (lifetimes, generations, careers, ages)
- What the traveler's body or clock experienced vs what Earth experienced
- Something that surprisingly did NOT change despite the vast time or distance

Rules:
- Every milestone must anchor to a real, concrete comparison — not restate the journey data.
- Do not echo the ship time, earth time, or distance numbers back as milestones. The reader already has those.
- Get the facts credibly right. This is a fun simulator, not a textbook, but don't say anything a curious person could easily disprove.
- Use plain language. No scientific abbreviations (c, ly, AU). No long numbers — use words instead.
- Do not tell the reader what they feel. Describe what happened.
- Tone is awe and wonder, not sadness or mourning.

Journey Details:
- Destination: {{Destination}}
- Departed: {{DepartedTime}}
- Speed: {{SpeedName}} ({{PercentageOfLightSpeed}}% of light speed)
- Distance: {{DistanceLightYears}} light-years ({{DistanceMiles}} miles)
- Earth time elapsed: {{EarthTimeFormatted}}
- Ship time elapsed: {{ShipTimeFormatted}}
- Time dilation difference: {{TimeDifference}}

Each milestone needs:
- "text": Short punchy title (5–8 words)
- "description": Brief explanation (under 100 characters)

Return ONLY valid JSON:
{"events": [{"text": "...", "description": "..."}, ...]}
