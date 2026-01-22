# v0.38.6: Combat & Action Flavor Text

Description: 100+ combat descriptors, 50+ outcomes, 30+ enemy voices, environmental context modifiers
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.38, v0.22
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.38: Descriptor Library & Content Database (v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Parent Specification:** [v0.38: Descriptor Library & Content Database](v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)

**Status:** Design Phase

**Timeline:** 12-15 hours

**Goal:** Build comprehensive combat and action descriptor library for dynamic moment-to-moment gameplay

**Philosophy:** Every combat action tells a story through varied, contextual flavor text

---

## I. Purpose

v0.38.6 creates **Combat & Action Flavor Text**, providing dynamic descriptions for all player and enemy actions:

- **100+ Combat Action Descriptors** (attacks, defenses, movements)
- **50+ Outcome Descriptors** (hit, miss, critical, deflect)
- **30+ Enemy Voice Descriptors** (per enemy type)
- **Contextual Layering** (weapon type, enemy type, environment)

**Strategic Function:**

Currently, combat text is generic:

- ❌ "You attack the Servitor. Hit for 12 damage."
- ❌ No variation based on weapon, enemy, or outcome quality
- ❌ Missed opportunity for environmental storytelling

**v0.38.6 Solution:**

- Dynamic action descriptors that vary by weapon type
- Enemy-specific reaction text
- Outcome quality variations (glancing hit vs. solid strike)
- Environmental context integration
- Voice consistency per enemy archetype

---

## II. The Rule: What's In vs. Out

### ✅ In Scope

- **Player Attack Actions:** Melee, ranged, special attacks
- **Player Defense Actions:** Dodge, parry, block, deflect
- **Enemy Attack Actions:** Per enemy archetype
- **Outcome Descriptors:** Hit (light/solid/devastating), miss, critical, fumble
- **Movement Actions:** Tactical positioning, fleeing, pursuing
- **Reaction Text:** Enemy responses to player actions
- **Weapon-specific variations**
- **Enemy archetype voices**
- **Environmental context modifiers**
- **Integration with v0.22 combat system**

### ❌ Out of Scope

- Ability/Galdr flavor text (v0.38.7)
- Status effect descriptions (v0.38.8)
- Loot discovery text (covered in v0.38.5)
- Dialogue (separate system)
- Quest text (v0.40)
- UI/rendering changes

---

## III. Combat Action Taxonomy

### A. Action Categories

```csharp
public enum CombatActionCategory
{
    PlayerMeleeAttack,      // Sword, axe, hammer swings
    PlayerRangedAttack,     // Bow, crossbow, thrown
    PlayerDefense,          // Dodge, parry, block
    PlayerMovement,         // Tactical repositioning
    EnemyAttack,            // Enemy offensive actions
    EnemyDefense,           // Enemy defensive reactions
    EnemyMovement,          // Enemy tactical actions
    EnvironmentalReaction   // Room responds to combat
}
```

### B. Outcome Types

```csharp
public enum CombatOutcome
{
    Miss,                   // Complete miss
    Deflected,             // Enemy deflects/parries
    GlancingHit,           // Low damage (1-25% max)
    SolidHit,              // Medium damage (26-75% max)
    DevastatingHit,        // High damage (76-99% max)
    CriticalHit,           // Critical (100%+ max)
    Fumble                 // Critical failure
}
```

### C. Weapon Categories

From v0.5 (Equipment) specification:

- **Melee One-Handed:** Swords, axes, hammers
- **Melee Two-Handed:** Greatswords, battleaxes, warhammers
- **Ranged:** Bows, crossbows, thrown weapons
- **Unarmed:** Fists, improvised

---

## IV. Player Combat Action Descriptors

### Category 1: Melee Attacks (One-Handed Sword)

### Base Action Templates

**Neutral Swing:**

- "You swing your {Weapon} at the {Enemy}."
- "You strike at the {Enemy} with your {Weapon}."
- "You bring your {Weapon} around in an arc toward the {Enemy}."

**Aggressive Thrust:**

- "You thrust your {Weapon} toward the {Enemy}'s {Target_Location}."
- "You drive your {Weapon} forward, aiming for the {Enemy}."
- "You lunge at the {Enemy}, {Weapon} leading."

**Tactical Strike:**

- "You feint left before striking at the {Enemy} with your {Weapon}."
- "You capitalize on an opening, slashing at the {Enemy}."
- "You exploit the {Enemy}'s guard, striking decisively."

### Outcome-Modified Descriptors

**Miss:**

- "You swing your {Weapon} at the {Enemy}, but it sidesteps effortlessly."
- "Your {Weapon} cuts only air as the {Enemy} moves out of reach."
- "The {Enemy} anticipates your strike and withdraws just in time."

**Deflected:**

- "The {Enemy} parries your {Weapon} with its {Enemy_Weapon}."
- "Your strike glances off the {Enemy}'s {Armor_Location}."
- "The {Enemy} bats your {Weapon} aside with contemptuous ease."

**Glancing Hit:**

- "Your {Weapon} scrapes across the {Enemy}'s {Target_Location}, drawing sparks."
- "You catch the {Enemy} a glancing blow that barely penetrates its defenses."
- "Your {Weapon} finds purchase but fails to bite deep."

**Solid Hit:**

- "Your {Weapon} bites into the {Enemy}'s {Target_Location} with a satisfying crunch."
- "You strike true, your {Weapon} carving through the {Enemy}'s defenses."
- "Your blade finds its mark, eliciting {Enemy_Reaction} from the {Enemy}."

**Devastating Hit:**

- "Your {Weapon} tears through the {Enemy}'s {Target_Location} in a spray of {Damage_Type_Descriptor}."
- "You deliver a brutal strike that sends the {Enemy} reeling."
- "Your blade cleaves deep, nearly severing the {Enemy}'s {Target_Location}."

**Critical Hit:**

- "You find a critical weakness—your {Weapon} plunges into the {Enemy}'s {Vital_Location} with devastating precision!"
- "In a perfect moment of opportunity, you drive your {Weapon} home with lethal force!"
- "Your strike is flawless—the {Enemy} has no defense as your {Weapon} finds its mark!"

---

### Category 2: Melee Attacks (Two-Handed)

### Base Action Templates

**Heavy Overhead:**

- "You raise your {Weapon} high and bring it down on the {Enemy} with tremendous force."
- "You swing your {Weapon} in a devastating overhead arc."
- "You commit to a massive overhead strike, {Weapon} whistling through the air."

**Sweeping Arc:**

- "You sweep your {Weapon} in a wide arc, seeking to overwhelm the {Enemy}."
- "You spin, bringing your {Weapon} around in a punishing horizontal sweep."
- "You leverage your {Weapon}'s reach with a broad, powerful swing."

**Crushing Blow:**

- "You aim a crushing blow at the {Enemy}, putting your full weight behind it."
- "You bring your {Weapon} down with bone-shattering intent."
- "You strike with raw power, seeking to batter through the {Enemy}'s defenses."

### Outcome-Modified Descriptors

**Miss:**

- "Your massive swing leaves you momentarily off-balance as it finds only empty space."
- "The {Enemy} ducks under your powerful but predictable arc."
- "Your {Weapon} crashes into the ground, missing the {Enemy} entirely."

**Solid Hit:**

- "The impact of your {Weapon} against the {Enemy} reverberates up your arms."
- "Your {Weapon} connects with pulverizing force, crushing {Target_Location}."
- "You feel the satisfying crunch as your {Weapon} smashes into the {Enemy}."

**Critical Hit:**

- "Your {Weapon} obliterates the {Enemy}'s defenses, the impact catastrophic!"
- "With overwhelming force, you shatter the {Enemy}'s {Target_Location} completely!"
- "The crushing blow sends fragments of {Damage_Type_Descriptor} flying in all directions!"

---

### Category 3: Ranged Attacks

### Base Action Templates (Bow)

**Careful Aim:**

- "You draw your bow, sighting carefully at the {Enemy}."
- "You nock an arrow and take aim at the {Enemy}'s {Target_Location}."
- "You pull the bowstring taut, focusing on your target."

**Quick Shot:**

- "You loose an arrow at the {Enemy} in one fluid motion."
- "You fire rapidly, not bothering with careful aim."
- "You send an arrow flying toward the {Enemy} with practiced speed."

**Called Shot:**

- "You take a moment to line up a precise shot at the {Enemy}'s {Vital_Location}."
- "You wait for the perfect moment, then release your arrow."
- "You aim for a weak point in the {Enemy}'s defenses."

### Outcome-Modified Descriptors

**Miss:**

- "Your arrow sails past the {Enemy}, clattering against the {Environment_Feature} behind it."
- "The {Enemy} shifts at the last instant, and your arrow misses by inches."
- "Your shot goes wide, embedding itself in the far wall."

**Solid Hit:**

- "Your arrow punches through the {Enemy}'s {Target_Location} with a wet thunk."
- "The {Enemy} staggers as your arrow finds its mark."
- "Your shaft buries itself deep in the {Enemy}'s {Target_Location}."

**Critical Hit:**

- "Your arrow strikes a vital point—the {Enemy} convulses as the shaft pierces {Vital_Location}!"
- "A perfect shot! Your arrow penetrates the {Enemy}'s {Vital_Location} completely!"
- "The {Enemy} seems to freeze as your arrow finds its heart!"

---

### Category 4: Defense Actions

### Dodge

**Successful:**

- "You twist aside, the {Enemy}'s attack passing harmlessly by."
- "You duck under the {Enemy}'s strike with practiced ease."
- "You sidestep the incoming blow at the last possible moment."
- "You roll away from the {Enemy}'s attack, coming up in a defensive stance."

**Failed:**

- "You try to dodge but aren't fast enough—the {Enemy}'s attack connects!"
- "You misjudge the {Enemy}'s reach and take the hit!"
- "Your attempted evasion leaves you exposed to the follow-up strike!"

### Parry (One-Handed)

**Successful:**

- "You parry the {Enemy}'s attack with your {Weapon}, metal ringing on metal."
- "You deflect the incoming strike with a precise counter-motion."
- "You turn the {Enemy}'s blade aside with your own."
- "You catch the {Enemy}'s weapon on your {Weapon} and push it away."

**Failed:**

- "Your parry is too slow—the {Enemy}'s attack batters through!"
- "The force of the {Enemy}'s strike overwhelms your guard!"
- "You mistime your parry and the blow connects!"

### Block (Shield/Heavy Weapon)

**Successful:**

- "You raise your {Shield} and absorb the {Enemy}'s strike."
- "The {Enemy}'s attack crashes against your {Shield} with jarring force."
- "You brace yourself, taking the blow on your {Shield}."
- "Your {Shield} holds firm against the {Enemy}'s assault."

**Failed:**

- "The {Enemy}'s attack batters past your {Shield}!"
- "Your defense crumbles under the onslaught!"
- "The {Enemy} finds a gap in your guard!"

---

## V. Enemy Action Descriptors by Archetype

### Archetype 1: Servitors (Corrupted Machines)

**Voice:** Mechanical, emotionless, relentless

**Setting Context:** Jötun-built maintenance drones, corrupted by the Blight

### Attack Descriptors

**Melee Strike:**

- "The Servitor's articulated limb swings at you with mechanical precision."
- "With a whir of servos, the Servitor strikes."
- "The Servitor's manipulator arm lashes out, trailing sparks."
- "Runic glyphs flare on the Servitor's chassis as it attacks."

**Special Attack (Electrical Discharge):**

- "The Servitor's corrupted power core crackles—electricity arcs toward you!"
- "Warning runes flash moments before the Servitor discharges its capacitors!"
- "The Servitor channels energy through its damaged systems, lashing out with a bolt of lightning!"

### Reaction Text

**Taking Damage:**

- "The Servitor's chassis dents under your blow, circuits sparking."
- "Warning klaxons blare from the damaged Servitor."
- "The Servitor continues its assault, heedless of the damage."
- "Runic light stutters and flickers as you strike the Servitor."

**Death:**

- "The Servitor collapses, its corrupted runes dimming to darkness."
- "With a final sputter of failing systems, the Servitor goes still."
- "The Servitor's chassis crumples, oil and coolant leaking onto the floor."
- "The runic corruption fades as the Servitor powers down permanently."

---

### Archetype 2: Forlorn (Undead)

**Voice:** Mournful, hollow, echoing with lost humanity

**Setting Context:** Victims of the Blight, trapped between life and death

### Attack Descriptors

**Melee Strike:**

- "The Forlorn reaches for you with grasping, desperate hands."
- "The hollow-eyed revenant lurches forward, clawing at you."
- "The Forlorn's movements are jerky and unnatural as it strikes."
- "You hear a whispered lament as the Forlorn attacks."

**Special Attack (Psychic Wail):**

- "The Forlorn opens its mouth impossibly wide—a soundless scream tears at your mind!"
- "Anguish 800 years old washes over you as the Forlorn wails!"
- "The Forlorn's psychic agony threatens to overwhelm your consciousness!"

### Reaction Text

**Taking Damage:**

- "The Forlorn barely seems to notice the blow."
- "Your weapon passes through withered flesh with little resistance."
- "The Forlorn lets out a mournful moan as you strike it."
- "The revenant staggers but continues its advance."

**Death:**

- "The Forlorn crumbles to dust, finally released from its torment."
- "As the Forlorn falls, you hear a whisper: 'Thank you...'"
- "The revenant collapses, its animating force dissipating."
- "The hollow light in the Forlorn's eyes finally gutters out."

---

### Archetype 3: Corrupted Dvergr

**Voice:** Fractured speech, technical jargon mixed with madness

**Setting Context:** Former engineers driven insane by the Blight

### Attack Descriptors

**Melee Strike:**

- "The Corrupted Dvergr swings its improvised weapon with manic energy."
- "'Recalibrating!' shrieks the mad Dvergr as it attacks."
- "The Corrupted Dvergr strikes with disturbing precision despite its madness."

**Special Attack (Explosive Device):**

- "'Thermal runaway detected!' The Dvergr hurls a smoking device at you!"
- "The Corrupted Dvergr cackles as it primes an unstable explosive!"
- "'Safety protocols... DISENGAGED!' The Dvergr's contraption explodes!"

### Reaction Text

**Taking Damage:**

- "'Structural integrity compromised!' wails the Dvergr."
- "The Corrupted Dvergr laughs manically as you strike it."
- "'Pain receptors malfunctioning... optimal!' giggles the mad engineer."

**Death:**

- "The Corrupted Dvergr collapses, muttering technical specifications."
- "'System failure... initiating shutdown...' The Dvergr goes still."
- "As it dies, the madness drains from the Dvergr's eyes."

---

*Continued with environmental context and more enemy archetypes...*

### Archetype 4: Blight-Touched Beasts

**Voice:** Animalistic, corrupted, suffering

**Setting Context:** Natural creatures twisted by Runic Blight

### Attack Descriptors

**Melee Strike:**

- "The beast lunges with unnatural speed, jaws snapping."
- "Corrupted flesh ripples as the creature strikes."
- "The Blight-twisted animal's attack is feral and desperate."
- "Crystalline growths glitter as the beast lashes out."

**Special Attack (Blight Infection):**

- "The beast's bite carries the Blight's corruption—you feel it seeping into your wound!"
- "Runic energy pulses from the creature's maw as it bites!"
- "The beast's claws rake you, leaving trails of paradoxical energy!"

### Reaction Text

**Taking Damage:**

- "The beast howls in pain and rage."
- "Blight-energy crackles around the wounded creature."
- "The animal's suffering is palpable even through its corruption."

**Death:**

- "The beast collapses, the Blight's influence fading from its eyes."
- "As it dies, the creature seems almost peaceful."
- "The Blight-touched animal finally finds rest."

---

### Archetype 5: Aether-Wraiths (Alfheim)

**Voice:** Reality-bending, incomprehensible, horrifying

**Setting Context:** Manifestations of pure paradox

### Attack Descriptors

**Melee Strike:**

- "The Wraith's form flickers as it phases through reality to strike you."
- "Impossibly, the Wraith attacks from multiple angles simultaneously."
- "The Wraith's touch burns with the cold fire of paradox."
- "Reality fractures as the Wraith lashes out."

**Special Attack (Reality Tear):**

- "The Wraith tears at the fabric of reality itself—the wound bleeds into you!"
- "Space inverts as the Wraith attacks, your mind struggling to process what's happening!"
- "The Wraith's assault defies comprehension—is this even an attack or are you remembering being hurt?"

### Reaction Text

**Taking Damage:**

- "Your weapon passes through where the Wraith was—will be—is."
- "The Wraith shimmers, existing in multiple states of injury."
- "Did you hit it, or did it allow itself to be struck?"

**Death:**

- "The Wraith unravels, reality reasserting itself where it stood."
- "As the Wraith dies, you briefly see what it once was—a person."
- "The paradox collapses, leaving only a fading echo."

---

## VI. Environmental Context Modifiers

### Biome-Specific Combat Atmosphere

### The Roots

**Environmental Reactions:**

- "Your strike sends rust flakes cascading from the ceiling."
- "The clash of combat echoes through corroded halls."
- "Steam hisses from fractured pipes as you fight."
- "Your footwork splashes through puddles of stagnant water."

**Hazard Integration:**

- "Your dodge sends you stumbling against a corroded pillar."
- "The enemy's miss crashes into a rusted support beam, which groans ominously."
- "Sparks from the clash ignite a patch of leaked oil!"

### Muspelheim

**Environmental Reactions:**

- "The heat distorts the air between you and your enemy."
- "Molten droplets spatter from the ceiling as combat rages."
- "The clash of weapons sends sparks into pools of magma."
- "Flames roar higher as if feeding on the violence."

**Hazard Integration:**

- "Your enemy's corpse tumbles into a lava flow and is consumed."
- "You're forced back toward the blazing heat!"
- "The enemy uses the flames as cover for its attack!"

### Niflheim

**Environmental Reactions:**

- "Your breath fogs as you circle your opponent."
- "Ice cracks beneath your feet as you fight."
- "Frozen crystals shatter as attacks miss their mark."
- "The cold makes your movements stiff and uncertain."

**Hazard Integration:**

- "Your dodge sends you sliding on ice!"
- "The enemy's strike shatters an icicle, sending shards flying!"
- "You use a frozen pillar for cover!"

### Alfheim

**Environmental Reactions:**

- "The Cursed Choir's shriek intensifies as violence erupts."
- "Reality flickers with each exchange of blows."
- "Crystalline structures pulse in rhythm with the combat."
- "You're not sure if you're fighting or if the fight is fighting itself."

**Hazard Integration:**

- "A Reality Tear opens where your strike misses!"
- "The enemy phases through a crystalline formation!"
- "Your attack creates echoes—are those past attempts or future ones?"

### Jötunheim

**Environmental Reactions:**

- "The vast chamber amplifies every sound of combat."
- "Machinery groans in sympathy with the violence."
- "Shadows stretch impossibly long in the industrial gloom."
- "Your fight seems insignificant in this titanic space."

**Hazard Integration:**

- "A stray strike activates dormant machinery!"
- "The enemy uses massive support pillars for cover!"
- "You're forced to fight around enormous, ancient equipment!"

---

## VII. Database Schema

```sql
CREATE TABLE IF NOT EXISTS Combat_Action_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    category TEXT NOT NULL,  -- 'PlayerMeleeAttack', 'EnemyAttack', etc.
    weapon_type TEXT,  -- 'Sword', 'Bow', NULL for enemy actions
    enemy_archetype TEXT,  -- NULL for player actions
    outcome_type TEXT,  -- 'Miss', 'Hit', 'Critical', NULL for neutral
    descriptor_text TEXT NOT NULL,
    tags TEXT,  -- JSON array
    
    CHECK (category IN ('PlayerMeleeAttack', 'PlayerRangedAttack', 'PlayerDefense', 
                        'PlayerMovement', 'EnemyAttack', 'EnemyDefense', 'EnemyMovement',
                        'EnvironmentalReaction')),
    CHECK (outcome_type IN ('Miss', 'Deflected', 'GlancingHit', 'SolidHit', 
                            'DevastatingHit', 'CriticalHit', 'Fumble') OR outcome_type IS NULL)
);

CREATE TABLE IF NOT EXISTS Enemy_Voice_Profiles (
    profile_id INTEGER PRIMARY KEY AUTOINCREMENT,
    enemy_archetype TEXT NOT NULL UNIQUE,
    voice_description TEXT NOT NULL,  -- "Mechanical, emotionless, relentless"
    setting_context TEXT NOT NULL,  -- Lore explanation
    
    -- Descriptor references (JSON arrays of descriptor_ids)
    attack_descriptors TEXT NOT NULL,
    reaction_damage TEXT NOT NULL,
    reaction_death TEXT NOT NULL,
    special_attacks TEXT
);

CREATE TABLE IF NOT EXISTS Environmental_Combat_Modifiers (
    modifier_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_name TEXT NOT NULL,
    modifier_type TEXT NOT NULL,  -- 'Reaction', 'HazardIntegration'
    descriptor_text TEXT NOT NULL,
    
    FOREIGN KEY (biome_name) REFERENCES Biomes(biome_name)
);
```

### Insert Sample Combat Descriptors

```sql
-- PLAYER SWORD ATTACKS (Miss)
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'Sword', 'Miss', 
 'You swing your {Weapon} at the {Enemy}, but it sidesteps effortlessly.', 
 '["OneHanded", "Miss", "Evasion"]'),

('PlayerMeleeAttack', 'Sword', 'Miss',
 'Your {Weapon} cuts only air as the {Enemy} moves out of reach.',
 '["OneHanded", "Miss", "Anticipation"]');

-- PLAYER SWORD ATTACKS (Critical)
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'Sword', 'CriticalHit',
 'You find a critical weakness—your {Weapon} plunges into the {Enemy}''s {Vital_Location} with devastating precision!',
 '["OneHanded", "Critical", "Lethal"]'),

('PlayerMeleeAttack', 'Sword', 'CriticalHit',
 'In a perfect moment of opportunity, you drive your {Weapon} home with lethal force!',
 '["OneHanded", "Critical", "Opportunity"]');

-- Continue for 100+ descriptors...
```

### Insert Enemy Voice Profiles

```sql
INSERT INTO Enemy_Voice_Profiles (
    enemy_archetype,
    voice_description,
    setting_context,
    attack_descriptors,
    reaction_damage,
    reaction_death,
    special_attacks
) VALUES (
    'Servitor',
    'Mechanical, emotionless, relentless',
    'Jötun-built maintenance drones corrupted by the Blight',
    '[101, 102, 103, 104]',  -- Attack descriptor IDs
    '[201, 202, 203, 204]',  -- Damage reaction IDs
    '[301, 302, 303, 304]',  -- Death descriptor IDs
    '[401, 402, 403]'  -- Special attack IDs
);

-- Continue for all enemy archetypes...
```

---

## VIII. Service Implementation

```csharp
public class CombatFlavorTextService
{
    private readonly IDescriptorRepository _repository;
    private readonly Random _random;
    
    /// <summary>
    /// Generate flavor text for player attack action.
    /// </summary>
    public string GeneratePlayerAttackText(
        WeaponType weaponType,
        string weaponName,
        string enemyName,
        CombatOutcome outcome)
    {
        // Get descriptors matching weapon and outcome
        var descriptors = _repository.GetCombatDescriptors(
            category: "PlayerMeleeAttack",
            weaponType: weaponType.ToString(),
            outcomeType: outcome.ToString());
        
        if (descriptors.Count == 0)
            return GenerateFallbackText(weaponName, enemyName, outcome);
        
        // Select random descriptor
        var descriptor = descriptors[_[random.Next](http://random.Next)(descriptors.Count)];
        
        // Fill template variables
        return FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Weapon", weaponName},
            {"Enemy", enemyName},
            {"Target_Location", SelectTargetLocation()},
            {"Vital_Location", SelectVitalLocation()},
            {"Damage_Type_Descriptor", GetDamageDescriptor(outcome)}
        });
    }
    
    /// <summary>
    /// Generate enemy attack text with archetype-specific voice.
    /// </summary>
    public string GenerateEnemyAttackText(
        EnemyArchetype archetype,
        string enemyName,
        bool isSpecialAttack = false)
    {
        var voiceProfile = _repository.GetEnemyVoiceProfile(archetype.ToString());
        
        var descriptorIds = isSpecialAttack 
            ? voiceProfile.SpecialAttacks 
            : voiceProfile.AttackDescriptors;
        
        var descriptorId = descriptorIds[_[random.Next](http://random.Next)(descriptorIds.Count)];
        var descriptor = _repository.GetDescriptor(descriptorId);
        
        return FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Enemy", enemyName}
        });
    }
    
    /// <summary>
    /// Generate enemy reaction text when taking damage.
    /// </summary>
    public string GenerateEnemyDamageReaction(
        EnemyArchetype archetype,
        string enemyName,
        int damageAmount,
        bool isDying)
    {
        var voiceProfile = _repository.GetEnemyVoiceProfile(archetype.ToString());
        
        var descriptorIds = isDying 
            ? voiceProfile.ReactionDeath 
            : voiceProfile.ReactionDamage;
        
        var descriptorId = descriptorIds[_[random.Next](http://random.Next)(descriptorIds.Count)];
        var descriptor = _repository.GetDescriptor(descriptorId);
        
        return FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Enemy", enemyName},
            {"DamageAmount", damageAmount.ToString()}
        });
    }
    
    /// <summary>
    /// Generate environmental reaction to combat.
    /// </summary>
    public string GenerateEnvironmentalReaction(
        string biomeName,
        CombatEvent combatEvent)
    {
        var modifiers = _repository.GetEnvironmentalCombatModifiers(biomeName);
        
        if (modifiers.Count == 0)
            return string.Empty;
        
        // 30% chance of environmental reaction
        if (_random.NextDouble() > 0.3)
            return string.Empty;
        
        var modifier = modifiers[_[random.Next](http://random.Next)(modifiers.Count)];
        return modifier.DescriptorText;
    }
    
    private string SelectTargetLocation()
    {
        var locations = new[] { "torso", "arm", "leg", "head", "chassis", "limb" };
        return locations[_[random.Next](http://random.Next)(locations.Length)];
    }
    
    private string SelectVitalLocation()
    {
        var locations = new[] { "core", "heart", "neck", "power cell", "central processor" };
        return locations[_[random.Next](http://random.Next)(locations.Length)];
    }
}
```

---

## IX. Integration with v0.22 Combat System

```csharp
public class CombatEngine
{
    private readonly ICombatFlavorTextService _flavorTextService;
    
    public CombatResult ExecutePlayerAttack(
        Character player,
        Enemy enemy,
        Attack attack)
    {
        // Resolve attack (existing v0.22 logic)
        var outcome = ResolveAttack(player, enemy, attack);
        
        // Generate flavor text (NEW)
        var actionText = _flavorTextService.GeneratePlayerAttackText(
            attack.WeaponType,
            attack.WeaponName,
            [enemy.Name](http://enemy.Name),
            outcome.Outcome);
        
        // Add environmental reaction (NEW)
        var envText = _flavorTextService.GenerateEnvironmentalReaction(
            player.CurrentRoom.BiomeName,
            CombatEvent.PlayerAttack);
        
        // Combine
        var fullText = actionText;
        if (!string.IsNullOrEmpty(envText))
            fullText += " " + envText;
        
        return new CombatResult
        {
            Success = outcome.Success,
            Damage = outcome.Damage,
            FlavorText = fullText,
            Outcome = outcome.Outcome
        };
    }
    
    public CombatResult ExecuteEnemyAttack(
        Enemy enemy,
        Character player)
    {
        var outcome = ResolveEnemyAttack(enemy, player);
        
        // Generate enemy attack text with archetype voice (NEW)
        var actionText = _flavorTextService.GenerateEnemyAttackText(
            enemy.Archetype,
            [enemy.Name](http://enemy.Name),
            isSpecialAttack: enemy.IsUsingSpecialAttack);
        
        return new CombatResult
        {
            Success = outcome.Success,
            Damage = outcome.Damage,
            FlavorText = actionText,
            Outcome = outcome.Outcome
        };
    }
}
```

---

## X. Success Criteria

**v0.38.6 is DONE when:**

### Combat Descriptors

- [ ]  100+ player action descriptors
- [ ]  All weapon types covered (sword, axe, bow, etc.)
- [ ]  All outcome types covered (miss, hit, critical, etc.)
- [ ]  Environmental context modifiers (5 biomes)

### Enemy Voice Profiles

- [ ]  5+ enemy archetype voices defined
- [ ]  Attack descriptors per archetype (10+ each)
- [ ]  Damage reactions per archetype (5+ each)
- [ ]  Death descriptors per archetype (5+ each)
- [ ]  Special attack descriptors where applicable

### Database

- [ ]  Combat_Action_Descriptors table
- [ ]  Enemy_Voice_Profiles table
- [ ]  Environmental_Combat_Modifiers table
- [ ]  200+ total descriptors inserted

### Service Implementation

- [ ]  CombatFlavorTextService complete
- [ ]  Player attack text generation
- [ ]  Enemy attack text generation
- [ ]  Enemy reaction text generation
- [ ]  Environmental reaction integration

### Integration

- [ ]  v0.22 CombatEngine uses flavor text service
- [ ]  Combat logs show dynamic descriptions
- [ ]  No repeated text in typical combat
- [ ]  Enemy voices are consistent

### Testing

- [ ]  Unit tests (80%+ coverage)
- [ ]  Template variable substitution tests
- [ ]  Archetype voice consistency tests
- [ ]  Integration tests with combat

---

## XI. Implementation Roadmap

**Phase 1: Content Creation** — 4 hours

- Write 100+ player combat descriptors
- Write 50+ enemy descriptors per archetype
- Define environmental reactions

**Phase 2: Database Schema** — 2 hours

- Combat_Action_Descriptors table
- Enemy_Voice_Profiles table
- Insert all descriptors

**Phase 3: Service Implementation** — 4 hours

- CombatFlavorTextService
- Template processing
- Random selection logic

**Phase 4: Integration** — 3 hours

- Update CombatEngine
- Test combat flows
- Verify variety and consistency

**Phase 5: Testing & Polish** — 2 hours

- Unit tests
- Integration tests
- Playtest for repetition

**Total: 15 hours**

---

## XII. Example Combat Sequences

### Example 1: Sword vs. Servitor (Solid Hit)

> **System:** You strike at the Servitor with your Rusted Longsword. Your blade bites into the Servitor's chassis with a satisfying crunch. The Servitor's chassis dents under your blow, circuits sparking. Warning klaxons blare from the damaged Servitor.
> 

> 
> 

> **System:** The Servitor's articulated limb swings at you with mechanical precision. You parry the attack with your sword, metal ringing on metal. Sparks from the clash ignite a patch of leaked oil!
> 

---

### Example 2: Bow vs. Forlorn (Critical Hit → Death)

> **System:** You draw your bow, sighting carefully at the Forlorn. Your arrow strikes a vital point—the Forlorn convulses as the shaft pierces its withered heart!
> 

> 
> 

> **System:** The Forlorn crumbles to dust, finally released from its torment. As it falls, you hear a whisper: 'Thank you...'
> 

---

### Example 3: Greathammer vs. Corrupted Dvergr (Devastating Hit, Muspelheim)

> **System:** You raise your Scorched Warhammer high and bring it down on the Corrupted Dvergr with tremendous force. Your weapon tears through the Dvergr's torso in a spray of blood and madness! The heat distorts the air between you and your enemy.
> 

> 
> 

> **System:** 'Structural integrity compromised!' wails the Dvergr. The Corrupted Dvergr laughs manically despite its grievous wound.
> 

---

**v0.38.6 Complete.**