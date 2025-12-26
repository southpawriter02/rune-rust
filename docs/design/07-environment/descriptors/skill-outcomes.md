---
id: SPEC-DESC-SKILL-OUTCOMES
title: "Skill Outcome Descriptors"
version: 1.0
status: draft
last-updated: 2025-12-14
related:
  - skills/skills-overview.md
  - descriptors/descriptors-overview.md
---

# Skill Outcome Descriptors

> *"Success is a story. Failure is a lesson. Fumble is a scar."*

---

## 1. Overview

This specification defines flavor text descriptors for skill check outcomes, organized by skill type, action type, and result degree.

### 1.1 Descriptor Categories

| Category | Description |
|----------|-------------|
| **Attempt** | Setup text before rolling |
| **Success** | Positive outcomes (Minimal, Solid, Critical) |
| **Failure** | Negative outcomes (Close, Clear) |
| **Fumble** | Critical failure with consequences |

### 1.2 Result Degrees

| Degree | Condition | Effect |
|--------|-----------|--------|
| **Critical Success** | Successes ≥ DC + 2 | Bonus rewards, extra intel |
| **Solid Success** | Successes ≥ DC | Standard success |
| **Minimal Success** | Successes = DC | Success with struggle |
| **Close Failure** | Successes = DC - 1 | Near-miss, may retry |
| **Clear Failure** | Successes < DC - 1 | Definite failure |
| **Fumble** | 0 successes | Critical failure with consequences |

---

## 2. Wasteland Survival — Foraging

### 2.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Common Flora | "You spot a patch of hardy [Ironwood] growing in the rubble. Common, but useful." |
| Specialized Flora | "This area shows signs of bioluminescent growth—rare in the ruins. You search for usable specimens." |
| Rare Flora | "A glint of unusual color catches your eye. Something precious grows here, well-hidden from casual looters." |
| [Blighted] Flora | "The plants here are visibly corrupted. Dark veins pulse through the leaves. Dangerous... but potentially potent." |

### 2.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "You carefully harvest the specimen and place it in your pouch." |
| Minimal | "After some effort, you manage to extract a usable sample." |
| Solid | "Your trained hands work efficiently. The harvest is clean and complete." |
| Solid | "You identify the optimal cutting point and harvest without waste." |
| Critical | "Your technique is flawless. You harvest a large, perfectly preserved sample." |
| Critical | "Not only do you gather the primary specimen, but you notice a secondary growth—[Pristine] quality." |

### 2.3 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "You attempt to harvest, but the plant crumbles in your hands. The technique wasn't quite right." |
| Close | "Almost... but you cut at the wrong point. The valuable parts are ruined." |
| Clear | "This specimen is beyond your skill to harvest. Better to leave it than destroy it." |
| Clear | "You don't recognize this variety. Is it even edible? You decide not to risk it." |

### 2.4 Fumble Descriptors — Toxic Harvest

| Consequence | Descriptor |
|-------------|------------|
| [Poisoned] | "As you touch the fungus, it releases a cloud of noxious spores! You recoil, coughing, the acrid taste of poison filling your mouth." |
| Stress | "The plant's sap touches your skin and BURNS. Not physically—psychically. Memories that aren't yours flash behind your eyes." |
| [Blight-Tainted] | "You harvest the specimen, but something is wrong. It writhes in your pack. The corruption was deeper than you thought." |

---

## 3. Wasteland Survival — Tracking

### 3.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Fresh Tracks | "Fresh tracks in the rust-dust. Something passed through here very recently." |
| Fresh Tracks | "You kneel, examining the footprints. These are recent—still sharp-edged." |
| Old Tracks | "Faint impressions in the dust. These are days old." |
| Old Tracks | "The tracks are weathered, partially obscured. Old, but still followable." |
| Unusual Tracks | "These aren't human. The gait is wrong, the foot shape... inhuman." |
| Unusual Tracks | "Servitor tracks—metallic impressions with hydraulic fluid drips." |
| [Blighted] Tracks | "Something Blight-touched passed through here. The tracks flicker when you look at them directly." |

### 3.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "You manage to follow the trail, though it's difficult. You lose it several times but pick it up again." |
| Minimal | "The tracks are faint but you can just barely make them out." |
| Solid | "The trail is clear to your trained eye. You follow it easily." |
| Solid | "You read the tracks like a book, understanding every movement your quarry made." |

### 3.3 Critical Success — Intel Types

| Intel Type | Descriptor |
|------------|------------|
| Numbers | "You count three distinct sets of tracks. One is much larger than the others—an Alpha leading a small pack." |
| Physical State | "There are flecks of dark, oily blood in the tracks. The beast is already wounded." |
| Recent Activity | "The tracks are frantic, overlapping. They were running *from* something deeper in the ruin." |
| Destination | "These aren't wandering tracks; this is a patrol route. It leads toward the old checkpoint and will circle back." |

### 3.4 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "The tracks end at rocky ground. You search but can't find where they continue." |
| Close | "You lose the trail at a water crossing. They could have gone any direction." |
| Clear | "You follow the tracks for a long while before realizing they're the wrong ones. You've wasted time." |
| Clear | "These tracks led you in a circle. You're back where you started." |

### 3.5 Fumble Descriptors — False Trail

| Consequence | Descriptor |
|-------------|------------|
| False Trail | "Despite the confusing signs, you find a clear set of tracks leading east. You are certain this is the way." |
| Ambush | "You confidently follow the trail into a narrow passage. Too late, you realize—this is a trap." |

---

## 4. Wasteland Survival — Camp Craft

### 4.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Safe Region | "You search for a defensible position to make camp." |
| Moderate Danger | "Finding shelter here will require care. The region is not safe." |
| High Danger | "Camping in this area is extremely risky. You'll need to find the best spot possible." |

### 4.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "You set up a rough camp, but an unsettling feeling lingers. The silence of the ruin feels heavy, watchful." |
| Solid | "Using your knowledge of the wilds, you find a well-hidden alcove, masking your fire's smoke and choosing a position with a clear line of sight. You can rest easy here." |
| Critical | "You find a perfect spot: a hidden cave behind a waterfall, its entrance invisible from the main path and the sound of the water masking your presence entirely. Nothing can find you here." |

### 4.3 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "The campsite is adequate, but not ideal. You're exposed to several approach vectors." |
| Clear | "This is a bad spot. You know it. But exhaustion forces your hand. You'll have to risk it." |

### 4.4 Fumble Descriptors — Compromised Camp

| Consequence | Descriptor |
|-------------|------------|
| Predator's Den | "You settle in for the night, confident in your choice. You do not notice the fresh claw marks on the nearby rocks, nor the unsettling silence of the local fauna..." |
| [Psychic Resonance] | "The camp seems perfect. Too perfect. As you drift to sleep, the whispers begin. This is a nexus point." |

---

## 5. Rhetoric — Persuasion

### 5.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Reasonable | "You make your case calmly and reasonably." |
| Reasonable | "You explain the mutual benefits of cooperation." |
| Difficult | "This is a big ask. You'll need to be convincing." |
| Difficult | "They're skeptical. You can see the resistance in their eyes." |

### 5.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "After much debate, they reluctantly agree." |
| Minimal | "They're not happy about it, but they'll do as you ask." |
| Solid | "Your argument is compelling. They agree." |
| Solid | "You make a strong case. They're convinced." |
| Critical | "You don't just convince them—you inspire them! They're enthusiastic allies now!" |
| Critical | "Your words are so compelling, they offer more help than you even asked for!" |

### 5.3 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "Your argument doesn't land. They refuse." |
| Close | "They shake their head. 'No. I don't think so.'" |
| Clear | "Your attempt to persuade them actually offends them. They're now hostile!" |

---

## 6. Rhetoric — Intimidation

### 6.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Physical | "You let your hand rest meaningfully on your weapon." |
| Physical | "You step forward, using your size to intimidate." |
| Reputation | "You invoke your reputation. They should know better than to cross you." |
| Verbal | "You paint a picture of what happens to those who defy you." |

### 6.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "They hesitate. Fear flickers in their eyes, but defiance remains." |
| Solid | "They back down immediately. Fear wins." |
| Solid | "You can see them calculating the risk. They decide compliance is safer." |
| Critical | "They're terrified. They'll do anything you ask!" |
| Critical | "Your reputation precedes you. They surrender immediately." |

### 6.3 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "They laugh at your threat. 'Is that supposed to scare me?'" |
| Clear | "Your threat backfires. Now they're angry and aggressive!" |
| Clear | "You've escalated this into violence. They attack!" |

---

## 7. System Bypass — Lockpicking

### 7.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Simple Lock | "You kneel before the lock, examining the mechanism with your picks." |
| Simple Lock | "You insert your tools, feeling for the tumblers inside." |
| Complex Lock | "This is Jötun craftsmanship—precision engineering. You steady your hands." |
| Complex Lock | "You count at least seven tumblers. This won't be quick." |
| Corroded Lock | "Rust has seized parts of the mechanism. You'll need to work around the corrosion." |

### 7.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "After several tense moments, the lock finally yields. Your hands are cramping." |
| Minimal | "You barely manage to align the tumblers. The lock clicks open, but you're sweating." |
| Solid | "Your picks find their marks efficiently. The lock opens with a satisfying click." |
| Solid | "One by one, the pins fall into place. The lock opens smoothly." |
| Critical | "You open the lock so smoothly, it's as if you had the key. Masterful work." |
| Critical | "Your fingers dance through the mechanism with perfect precision. The lock never stood a chance." |

### 7.3 Fumble Descriptors

| Consequence | Descriptor |
|-------------|------------|
| Tool Breakage | "Your pick snaps off inside the lock! The broken piece jams the mechanism. [+2 DC for next attempt]" |
| Tool Lost | "You drop your pick, and it falls into the mechanism where you can't retrieve it." |
| Alarm Triggered | "Something clicks wrong. You hear a distant bell begin to ring—an alarm!" |
| Trap Activated | "You miss the telltale signs. A hidden needle springs out and pricks your finger! [Poisoned]" |

---

## 8. Acrobatics — Climbing

### 8.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Corroded | "You search for handholds on the corroded metal surface." |
| Corroded | "The rust flakes away under your fingers as you begin to climb." |
| Dangerous Height | "You look up at the towering structure. This will be a challenging climb." |
| Dangerous Height | "One wrong move from this height, and the fall will be deadly." |

### 8.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "You haul yourself up with great effort. Your arms burn from the exertion." |
| Minimal | "After a difficult climb, you reach your destination. You're breathing hard." |
| Solid | "You climb efficiently, finding good handholds. The ascent is tiring but manageable." |
| Solid | "You scale the structure with practiced ease." |
| Critical | "You flow up the structure like water, barely even winded at the top!" |
| Critical | "Your climbing is a thing of beauty—efficient, graceful, effortless." |

### 8.3 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "A handhold crumbles. You slide partway down, catching yourself on a lower ledge." |
| Close | "Your grip fails. You descend rapidly but manage to slow your fall." |

### 8.4 Fumble Descriptors

| Consequence | Descriptor |
|-------------|------------|
| Fall | "The metal gives way! You fall, striking the ground hard! [3d6 damage, Prone]" |
| Structural Collapse | "Your weight triggers a structural failure. Metal beams rain down! [4d6 damage]" |

## 9. Rhetoric — Bargaining

### 9.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Fair Trade | "You lay out your goods, assessing their offerings in return." |
| Fair Trade | "A simple exchange. Both parties know the value involved." |
| Hard Bargain | "Their price is outrageous. You'll need to negotiate firmly." |
| Hard Bargain | "They're trying to take advantage. Time to push back." |
| Desperate | "You need this badly, and they know it. The power is on their side." |
| Faction Trade | "Trading with this faction requires understanding their values. Marks aren't everything." |

### 9.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "After much back-and-forth, you reach an agreement. Neither party is thrilled, but the deal is done." |
| Minimal | "They relent, but barely. You get what you need at a reasonable price." |
| Solid | "Your counter-offer lands. They nod, accepting your terms." |
| Solid | "A fair deal for both sides. They respect your negotiating skill." |
| Critical | "You drive a masterful bargain. They practically give it away!" |
| Critical | "Not only do you get your price, but they throw in an extra item as a sign of goodwill." |

### 9.3 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "'No deal.' They fold their arms. Your offer wasn't good enough." |
| Close | "They shake their head. 'Come back when you have something better to offer.'" |
| Clear | "Your lowball offer offends them. They refuse to trade with you at all." |
| Clear | "Negotiations collapse. They won't do business with you today." |

### 9.4 Fumble Descriptors — Deal Breakdown

| Consequence | Descriptor |
|-------------|------------|
| Reputation | "Word spreads of your insulting offer. Other merchants in the settlement will charge you more." |
| Conflict | "Your aggressive bargaining crosses a line. They reach for their weapon." |

---

## 10. Rhetoric — Protocol

### 10.1 Core Philosophy

Protocol represents navigating the complex social customs of Aethelgard's factions. Each faction has distinct traditions that must be respected—failure can mean exile, offense, or violence.

### 10.2 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Hearth-Clan | "The Hearth-Clans value directness and demonstrated competence. Speak plainly or be dismissed." |
| Grove-Clan | "The Grove-Clans speak in metaphor and seasonal wisdom. Your words must flow like theirs." |
| Rust-Clan | "The Rust-Clans respect only practicality. Show them what you can do, not what you promise." |
| Forsaken | "The Forsaken have abandoned hope but not hierarchy. Tread carefully around their broken protocols." |
| Utgard Raiders | "The Utgard speak Veil-Speech—indirect, face-saving lies that everyone understands. Direct honesty here is an insult." |
| Iron-Banes | "The Iron-Banes have ritual greetings involving weapon display. Misread the situation, and you've challenged them." |

### 10.3 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "You manage the basic forms. They accept you, if somewhat coldly." |
| Minimal | "Your greeting is awkward but acceptable. They overlook the minor breaches." |
| Solid | "You navigate their customs flawlessly. They recognize you as someone who understands their ways." |
| Solid | "Your knowledge of their protocols impresses them. Doors begin to open." |
| Critical | "Your mastery of their customs marks you as a true friend of the faction. They treat you as one of their own!" |
| Critical | "They're astonished. 'You speak as if you were born among us.' Trust is offered freely." |

### 10.4 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "You misstep slightly. They notice, but choose to overlook it—this time." |
| Close | "An awkward pause. You've said something wrong, but they continue the conversation." |
| Clear | "Your breach of protocol is significant. They're offended and suspicious." |
| Clear | "You've insulted them without realizing it. The conversation is over." |

### 10.5 Fumble Descriptors — Protocol Violation

| Consequence | Descriptor |
|-------------|------------|
| Exile | "Your ignorance of their ways is unforgivable. You are no longer welcome here." |
| Challenge | "Your words constitute a formal challenge in their culture. They're reaching for weapons." |
| Deception | "They pretend to accept you—but you've marked yourself as an outsider. Information will now be filtered." |

---

## 11. Rhetoric — Deception

### 11.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Simple Lie | "You tell them a simple, believable lie." |
| Simple Lie | "You keep your expression neutral, your voice calm." |
| Complex Deception | "This is an elaborate deception. You'll need to sell it completely." |
| Complex Deception | "Multiple layers to this lie. You need to keep your story straight." |
| High Stakes | "If they see through you, the consequences will be severe." |

### 11.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "They accept your story, though something in their eyes suggests doubt." |
| Solid | "They buy it completely. They suspect nothing." |
| Solid | "Your lie is convincing. They accept it as truth." |
| Critical | "Not only do they believe you, they're grateful for the 'truth' you've shared!" |
| Critical | "Your lie is so well-crafted, you almost believe it yourself!" |

### 11.3 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "They narrow their eyes. 'I don't believe you.'" |
| Close | "They're skeptical but not confrontational. They don't trust you now." |
| Clear | "They see right through you. 'You're lying to me.'" |
| Clear | "They catch you in the lie. Now they're hostile." |

---

## 12. Rhetoric — Rallying

### 12.1 Core Philosophy

Rallying represents inspiring allies during crisis—combat, disasters, or moments of despair. A successful rally can turn a losing battle into victory.

### 12.2 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Combat Crisis | "Your allies are faltering. You raise your voice above the chaos." |
| Combat Crisis | "The line is breaking. You need to rally them NOW." |
| Despair | "Hope is dying in their eyes. You speak words of defiance against the darkness." |
| Last Stand | "This may be the end. You make it mean something with your words." |

### 12.3 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "Your words reach them. They steady themselves, fighting on." |
| Minimal | "They draw strength from your defiance. The retreat slows." |
| Solid | "Your cry echoes across the battlefield! Allies fight with renewed vigor!" |
| Solid | "They hear you and BELIEVE. Fear gives way to determination." |
| Critical | "Your words become legend. Every ally fights like they have nothing to lose—because they fight for something greater!" |
| Critical | "You channel something ancient, something true. This moment will be remembered. [Party gains +2 to all rolls for 3 rounds]" |

### 12.4 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "Your words are lost in the chaos. They don't hear you." |
| Clear | "Despair is too deep. Your rally falls flat. Some allies break and run." |
| Clear | "Your voice cracks. You've shown weakness at the worst possible moment." |

---

## 13. Wasteland Survival — Weather Reading

### 13.1 Core Philosophy

Weather Reading represents predicting hazardous conditions—Static Storms, Blight Fronts, flash floods, and environmental dangers. In Aethelgard, weather can kill.

### 13.2 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Standard | "You study the sky, the wind direction, the behavior of the local fauna." |
| Standard | "The pressure feels wrong. You read the signs for what's coming." |
| Storm Warning | "Electrical discharge plays along the horizon. A Static Storm may be building." |
| Blight Front | "The air tastes of copper and wrongness. Something corrupted approaches." |

### 13.3 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "Weather is coming. You're not sure what, but you have time to prepare." |
| Solid | "You read the signs clearly. A Static Storm will hit in approximately 4 hours." |
| Solid | "The Blight Front is moving northeast. If you travel west, you'll avoid it entirely." |
| Critical | "You predict the weather with uncanny accuracy—timing, direction, intensity. You know exactly how long you have and where to shelter." |
| Critical | "You notice signs others would miss. There's a pocket of safety in the storm's eye—you can guide the party there." |

### 13.4 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "The signs are confusing. You can't predict what's coming." |
| Clear | "You misread the weather entirely. The storm catches you unprepared." |

### 13.5 Fumble Descriptors

| Consequence | Descriptor |
|-------------|------------|
| Wrong Direction | "You confidently lead the party directly into the path of the incoming storm." |
| False All-Clear | "'Clear skies ahead,' you announce. An hour later, the Static Storm hits." |

---

## 14. Wasteland Survival — Water Procurement

### 14.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Arid Region | "Water is scarce here. You search for signs of moisture." |
| Ruins | "Pipes run through these walls. Perhaps some still hold water." |
| Natural | "You look for natural collection points—low ground, leaf runoff, morning dew." |
| [Blighted] Zone | "There's water here, but is it safe? You examine it carefully." |

### 14.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "You find a small amount of water. Enough to survive, barely." |
| Solid | "You locate a working pipe junction. Clean water flows when you open the valve." |
| Solid | "A natural spring, hidden behind the rubble. You fill your containers." |
| Critical | "You discover a sealed cistern, untouched since the Glitch. Gallons of clean water!" |
| Critical | "Not only do you find water, but you can mark this location for future expeditions. [Discovered: Water Source]" |

### 14.3 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "You find moisture, but not enough to collect. The search continues." |
| Clear | "This area is completely dry. You've wasted precious time and energy." |

### 14.4 Fumble Descriptors — Contaminated Water

| Consequence | Descriptor |
|-------------|------------|
| Poisoned | "The water looked clear. It wasn't. [Poisoned] status after drinking." |
| Blight-Tainted | "Something is wrong with this water. It shimmers with iridescence. [+5 Psychic Stress if consumed]" |

---

## 15. Wasteland Survival — Hazard Identification

### 15.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Structural | "The building ahead looks unstable. You assess the risk." |
| Trap | "Something about this corridor feels wrong. You examine it carefully." |
| Environmental | "The air here smells off. You check for toxic hazards." |
| Blight Zone | "Reality feels thin here. You watch for signs of Glitch pockets." |

### 15.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "There's danger ahead, but you can't identify exactly what." |
| Solid | "You spot the tripwire before triggering it. A dart trap, easily bypassed." |
| Solid | "The floor is unstable. You warn the party to stay to the left side." |
| Critical | "You identify multiple hazards and the safest path through. [Grant +2d10 to next Acrobatics check]" |
| Critical | "Not only do you see the trap, you understand its mechanism. You could disarm it—or use it against enemies." |

### 15.3 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "Something's off, but you can't pinpoint it. Proceed with caution." |
| Clear | "You see nothing amiss. The area looks safe." |

### 15.4 Fumble Descriptors — Triggered Hazard

| Consequence | Descriptor |
|-------------|------------|
| Trap Triggered | "Your examination triggers the very trap you were looking for. [Roll for damage]" |
| Collapse | "You lean against what you thought was a stable wall. It gives way. [Structural collapse]" |

---

## 16. System Bypass — Terminal Hacking

### 16.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Standard | "You interface with the altar, your fingers flying across the corrupted glyph-pad." |
| Standard | "The terminal flickers to life as you access its core logic." |
| [Glitched] | "The terminal's display is fractured, showing three overlapping interfaces. This will be challenging." |
| [Glitched] | "Blight corruption has warped the terminal's logic. The Machine-Spirit does not respond as it should." |

### 16.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "You barely force your way through the system's wards. Access granted, but it wasn't pretty." |
| Solid | "You navigate the logic-maze efficiently. Before long, you have full access." |
| Solid | "The terminal's security crumbles before your expertise. You're in." |
| Critical | "You slice through the security like it wasn't even there. Core command achieved." |
| Critical | "The system doesn't even realize it's been compromised. Perfect infiltration. [Access Level: Administrator]" |

### 16.3 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "Access denied. You're locked out temporarily. Try a different approach." |
| Clear | "The system detects your intrusion and locks down completely." |

### 16.4 Fumble Descriptors

| Consequence | Descriptor |
|-------------|------------|
| Lockout | "Access denied. The terminal locks you out completely. [Cannot retry]" |
| System Crash | "The system detects your intrusion and shuts down. All terminals in this area go dark." |
| Blight Exposure | "The data you access is infected with Blight corruption. Reading it sears your mind! [+8 Psychic Stress]" |

---

## 17. System Bypass — Trap Disarming

### 17.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Simple | "You spot the pressure plate, barely visible beneath the dust. There's definitely a trap here." |
| Tripwire | "Thin wires at ankle height—a tripwire. You kneel to examine the mechanism." |
| Complex | "You trace the wires to their source. This trap connects to... ceiling collapse charges. Dangerous." |
| Complex | "The mechanism is complex—multiple triggers, redundant systems. Whoever built this knew their craft." |

### 17.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "With great care, you neutralize the trap. That was close." |
| Solid | "You carefully cut the wire. The tension releases harmlessly. Trap neutralized." |
| Solid | "With steady hands, you disable the trigger mechanism. The trap is now safe." |
| Critical | "Not only do you disarm the trap, but you salvage the mechanism intact! [+1 Trap Component]" |
| Critical | "You disarm the trap so cleanly, you can actually reuse the parts. [+2 Trap Components]" |

### 17.3 Fumble Descriptors

| Consequence | Descriptor |
|-------------|------------|
| Triggered | "Your hand slips! The trap activates! [Roll for trap damage]" |
| Triggered | "You cut the wrong wire. The mechanism fires immediately!" |
| Poison | "The trap wasn't just mechanical. Poison gas hisses from hidden vents." |

---

## 18. Acrobatics — Leaping

### 18.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Standard | "You back up for a running start. The gap yawns before you." |
| Standard | "You measure the distance with your eyes. It's jumpable... barely." |
| [Glitched] | "The gap flickers—is it a spear-length or a chasm? Coherent Glitches make this treacherous." |
| [Glitched] | "Reality stutters as you prepare to jump. The distance keeps changing." |

### 18.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "You barely clear the gap, landing hard on the far side. You stumble but catch yourself." |
| Minimal | "Your landing is ungraceful, but you made it across. Barely." |
| Solid | "You clear the gap easily, landing with a controlled roll." |
| Solid | "Perfect execution. You land smoothly on the far side." |
| Critical | "You soar across the gap with room to spare, landing like a cat!" |
| Critical | "Textbook-perfect jump. You stick the landing flawlessly." |

### 18.3 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "You don't make it! You catch the far edge with your fingertips, dangling! [STR check to pull up]" |
| Close | "Short! You slam into the far wall and start sliding down! [Grab check required]" |
| Clear | "You fall short. Way short. You're falling!" |

---

## 19. Acrobatics — Stealth Movement

### 19.1 Attempt Descriptors

| Context | Descriptor |
|---------|------------|
| Shadows | "You press yourself into the shadows, moving silently." |
| Shadows | "You creep from cover to cover, trying to remain undetected." |
| Noisy Terrain | "Debris litters the floor—one wrong step will make noise." |
| Noisy Terrain | "Rust flakes crunch underfoot. This will be difficult." |

### 19.2 Success Descriptors

| Degree | Descriptor |
|--------|------------|
| Minimal | "You move carefully. No one seems to notice—yet." |
| Solid | "You move like a ghost. The guards never notice you." |
| Solid | "Silent as a shadow, you slip past your enemies." |
| Critical | "You're invisible. Even if they looked directly at you, they might not see you!" |
| Critical | "Master-level stealth. You could steal their weapons without them noticing." |

### 19.3 Failure Descriptors

| Degree | Descriptor |
|--------|------------|
| Close | "A piece of metal clangs as you step on it. Everyone turns toward the sound!" |
| Close | "You knock over a rusted can. It echoes loudly!" |
| Clear | "A guard spots you! 'Intruder!'" |
| Clear | "You step into a pool of light. You're seen immediately!" |

---

## 20. Resource Node Types

### 20.1 Flora Types by Biome

| Biome | Common | Specialized | Rare |
|-------|--------|-------------|------|
| The Roots | [Ironwood], [Grave-Lichen] | [Glow-Cap], [Myr-Vines] | [Deep-Root] |
| The Trunk | [Moss-Bark], [Fungal-Bloom] | [Sap-Crystal], [Spore-Pod] | [Heart-Vine] |
| Muspelheim | [Ash-Weed], [Ember-Lichen] | [Fire-Bloom], [Slag-Moss] | [Sun-Petal] |
| Niflheim | [Frost-Moss], [Ice-Lichen] | [Cryo-Fungus], [Pale-Root] | [Void-Bloom] |
| Alfheim | [Light-Moss], [Crystal-Vine] | [Prism-Flower], [Echo-Fern] | [Aether-Weed] |
| Helheim | [Corpse-Moss], [Bone-Lichen] | [Spirit-Bloom], [Shade-Root] | [Revenant-Petal] |
| Jötunheim | [Giant-Weed], [Scale-Lichen] | [Titan-Fungus], [Quake-Root] | [World-Tree Sap] |

### 20.2 Harvest DC Table

| Node Type | DC Range | Quality on Crit |
|-----------|----------|-----------------|
| Common | 2-3 | Standard + 1 extra |
| Specialized | 4-5 | [Pristine] quality |
| Rare | 6-7 | [Pristine] + bonus item |
| [Blighted] | Variable | [Blight-Touched] |

### 20.3 Water Source Types

| Source Type | DC | Purity | Notes |
|-------------|-----|--------|-------|
| Pipe Junction | 3-4 | Safe | Common in ruins |
| Natural Spring | 4-5 | Safe | Rare, valuable |
| Stagnant Pool | 2-3 | Risky | May be contaminated |
| Sealed Cistern | 5-6 | Safe | Large quantity |
| [Blighted] Source | Variable | Dangerous | +Stress if consumed |

---

## 21. Implementation Status

- [x] Base descriptor framework
- [x] Wasteland Survival — Foraging
- [x] Wasteland Survival — Tracking
- [x] Wasteland Survival — Camp Craft
- [x] Wasteland Survival — Weather Reading
- [x] Wasteland Survival — Water Procurement
- [x] Wasteland Survival — Hazard Identification
- [x] System Bypass — Lockpicking
- [x] System Bypass — Terminal Hacking
- [x] System Bypass — Trap Disarming
- [x] Acrobatics — Climbing
- [x] Acrobatics — Leaping
- [x] Acrobatics — Stealth Movement
- [x] Rhetoric — Persuasion
- [x] Rhetoric — Intimidation
- [x] Rhetoric — Deception
- [x] Rhetoric — Bargaining
- [x] Rhetoric — Protocol
- [x] Rhetoric — Rallying
- [ ] Interrogation (future)

---

## 22. Related Specifications

| Document | Purpose |
|----------|---------|
| [Skills Overview](../../01-core/skills/skills-overview.md) | Core mechanics |
| [Skills UI](../../08-ui/skills-ui.md) | Interface specification |
| [Descriptor Framework](overview.md) | Three-tier system |

---

## 23. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification with core skill descriptors |
| 1.1 | 2025-12-14 | Added Bargaining, Protocol, Deception, Rallying, Weather Reading, Water Procurement, Hazard Identification, Terminal Hacking, Trap Disarming, Leaping, Stealth Movement |

