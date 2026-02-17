# Sprite Data Format Specification

# Pixel Art Assets for Rune & Rust

**Version:** 1.0
**Format:** 16×16 pixel art with indexed palette
**Encoding:** String arrays with character-to-color mapping

---

## Format Overview

Sprites are defined using a text-based format that's easy to edit, version control, and parse. Each sprite consists of:

1. **Name** - Unique identifier
2. **Pixel Data** - 16 strings of 16 characters each
3. **Palette** - Character-to-color mapping

### Advantages of This Format

- ✅ Human-readable and editable
- ✅ Git-friendly (easy to diff changes)
- ✅ No binary assets needed
- ✅ Can be embedded directly in code
- ✅ Easy to generate/modify programmatically
- ✅ Supports transparency
- ✅ Small file size

---

## C# Data Structure

```csharp
public class PixelSprite
{
    public string Name { get; set; }
    public string[] PixelData { get; set; }  // 16 rows
    public Dictionary<char, string> Palette { get; set; }
}

```

---

## Pixel Data Format

### Structure

Each sprite is exactly **16×16 pixels**, represented as:

- Array of 16 strings (rows)
- Each string contains 16 characters (columns)
- Each character references a color in the palette

### Character Mapping

- Use alphanumeric characters: `0-9`, `a-z`, `A-Z`
- `'0'` is conventionally used for transparent
- `'1'-'9'` for primary colors (outline, shadows, highlights)
- `'a'-'z'` for additional colors if needed

### Example

```csharp
PixelData = new[]
{
    "0000000110000000",  // Row 0 (top)
    "0000001111000000",  // Row 1
    "0000011111100000",  // Row 2
    // ... 13 more rows
    "0001166666661000",  // Row 15 (bottom)
}

```

### Grid Visualization

```
   0123456789ABCDEF   (Column indices)
0  0000000110000000
1  0000001111000000
2  0000011111100000
3  0000111111110000
4  0001122222211000
5  0001223333221000
6  0001233443321000
7  0001234554321000
8  0001234554321000
9  0001122662211000
A  0000112662110000
B  0000011771100000
C  0000001771000000
D  0000011661100000
E  0000116666110000
F  0001166666661000

```

---

## Palette Format

### Structure

Dictionary mapping characters to color strings:

```csharp
Palette = new Dictionary<char, string>
{
    { '0', "transparent" },
    { '1', "#2a2a2a" },  // Dark outline
    { '2', "#8b4513" },  // Brown
    // ... more colors
}

```

### Color Formats Supported

1. **Hex RGB:** `"#RRGGBB"` (e.g., `"#FF0000"` for red)
2. **Named Colors:** `"transparent"`, `"white"`, `"black"`, etc.
3. **Case Insensitive**

### Recommended Palette Size

- **Minimum:** 2-3 colors (simple icons)
- **Typical:** 6-10 colors (characters)
- **Maximum:** 36+ colors (using `0-9`, `a-z`)

For 16-bit style art, use **8-12 colors per sprite** with proper shading.

---

## Complete Sprite Examples

### Example 1: Shieldmaiden

```csharp
public static PixelSprite Shieldmaiden => new()
{
    Name = "Shieldmaiden",
    PixelData = new[]
    {
        "0000000110000000",
        "0000001111000000",
        "0000011111100000",
        "0000111111110000",
        "0001122222211000",
        "0001223333221000",
        "0001233443321000",
        "0001234554321000",
        "0001234554321000",
        "0001122662211000",
        "0000112662110000",
        "0000011771100000",
        "0000001771000000",
        "0000011661100000",
        "0000116666110000",
        "0001166666661000",
    },
    Palette = new()
    {
        { '0', "transparent" },
        { '1', "#2a2a2a" },  // outline/shadow
        { '2', "#8b4513" },  // dark brown (hair/leather)
        { '3', "#a0522d" },  // medium brown
        { '4', "#daa520" },  // gold (shield details)
        { '5', "#ffd700" },  // bright gold
        { '6', "#c0c0c0" },  // silver (armor)
        { '7', "#e8e8e8" },  // bright silver
    }
};

```

**Rendered at 3x scale (48×48px):**

```
(Gold and silver armor with brown hair, shield with gold accents)

```

---

### Example 2: Runecaster

```csharp
public static PixelSprite Runecaster => new()
{
    Name = "Runecaster",
    PixelData = new[]
    {
        "0000000110000000",
        "0000001221000000",
        "0000012332100000",
        "0000123443210000",
        "0001234554321000",
        "0001235665321000",
        "0001123773211000",
        "0000112882110000",
        "0000119aa9110000",
        "0000119aa9110000",
        "0000011bb1100000",
        "0000011bb1100000",
        "0000001bb1000000",
        "0000011cc1100000",
        "0000111cc1110000",
        "0001111cc1111000",
    },
    Palette = new()
    {
        { '0', "transparent" },
        { '1', "#1a1a1a" },  // outline
        { '2', "#4b0082" },  // indigo (robe dark)
        { '3', "#6a5acd" },  // slate blue
        { '4', "#7b68ee" },  // medium slate blue
        { '5', "#9370db" },  // medium purple
        { '6', "#ba55d3" },  // medium orchid
        { '7', "#00ffff" },  // cyan (magic glow)
        { '8', "#4169e1" },  // royal blue (robe)
        { '9', "#1e90ff" },  // dodger blue
        { 'a', "#00bfff" },  // deep sky blue (magic)
        { 'b', "#8b4513" },  // brown (staff)
        { 'c', "#daa520" },  // goldenrod (staff ornament)
    }
};

```

**Features:**

- Purple/blue robes with gradient
- Cyan magic glow effect
- Brown staff with gold ornament
- 13 colors total (uses `a`, `b`, `c` for extended palette)

---

### Example 3: Simple Item Icon (Potion)

```csharp
public static PixelSprite HealthPotion => new()
{
    Name = "HealthPotion",
    PixelData = new[]
    {
        "0000000000000000",
        "0000001111000000",
        "0000001111000000",
        "0000011111100000",
        "0000111111110000",
        "0001122222211000",
        "0001223333221000",
        "0012233333322100",
        "0012333333322100",
        "0012333333322100",
        "0012333333322100",
        "0001233333321000",
        "0000123333210000",
        "0000012332100000",
        "0000001221000000",
        "0000000110000000",
    },
    Palette = new()
    {
        { '0', "transparent" },
        { '1', "#1a1a1a" },  // glass outline
        { '2', "#808080" },  // glass dark
        { '3', "#dc143c" },  // red potion liquid
    }
};

```

**Features:**

- Simple 3-color design
- Glass bottle effect with outline
- Red health liquid
- Minimal but readable

---

## JSON Storage Format

For external sprite files, use JSON:

```json
{
  "name": "Shieldmaiden",
  "pixelData": [
    "0000000110000000",
    "0000001111000000",
    "0000011111100000",
    "0000111111110000",
    "0001122222211000",
    "0001223333221000",
    "0001233443321000",
    "0001234554321000",
    "0001234554321000",
    "0001122662211000",
    "0000112662110000",
    "0000011771100000",
    "0000001771000000",
    "0000011661100000",
    "0000116666110000",
    "0001166666661000"
  ],
  "palette": {
    "0": "transparent",
    "1": "#2a2a2a",
    "2": "#8b4513",
    "3": "#a0522d",
    "4": "#daa520",
    "5": "#ffd700",
    "6": "#c0c0c0",
    "7": "#e8e8e8"
  }
}

```

### JSON Array Format (Multiple Sprites)

```json
{
  "sprites": [
    {
      "name": "Shieldmaiden",
      "pixelData": [...],
      "palette": {...}
    },
    {
      "name": "Berserker",
      "pixelData": [...],
      "palette": {...}
    }
  ]
}

```

---

## Color Palette Guidelines

### 16-Bit Aesthetic Colors

For authentic 16-bit look, use colors from this recommended palette:

**Neutrals & Outlines:**

- `#000000` - Pure black
- `#1a1a1a` - Very dark gray (soft outline)
- `#2a2a2a` - Dark gray (shadow)
- `#4a4a4a` - Medium dark gray
- `#8a8a8a` - Medium gray
- `#c0c0c0` - Light gray (silver)
- `#e8e8e8` - Very light gray
- `#ffffff` - Pure white

**Browns & Leather:**

- `#8b4513` - Saddle brown
- `#a0522d` - Sienna
- `#d2691e` - Chocolate
- `#deb887` - Burlywood

**Metals:**

- `#696969` - Dim gray (iron)
- `#a9a9a9` - Dark gray (steel)
- `#c0c0c0` - Silver
- `#daa520` - Goldenrod (gold dark)
- `#ffd700` - Gold
- `#ffed4e` - Bright gold highlight

**Reds (Blood, Fire, Enemies):**

- `#8b0000` - Dark red
- `#dc143c` - Crimson
- `#ff4500` - Orange red
- `#ff6347` - Tomato

**Blues (Magic, Ice, Players):**

- `#191970` - Midnight blue
- `#4169e1` - Royal blue
- `#5f9ea0` - Cadet blue
- `#87ceeb` - Sky blue
- `#00bfff` - Deep sky blue
- `#00ffff` - Cyan

**Greens (Nature, Poison):**

- `#2f4f2f` - Dark green
- `#3cb371` - Medium sea green
- `#6b8e23` - Olive drab
- `#90ee90` - Light green

**Purples (Dark Magic, Corruption):**

- `#4b0082` - Indigo
- `#6a5acd` - Slate blue
- `#9370db` - Medium purple
- `#ba55d3` - Medium orchid

---

## Shading Techniques

### Three-Tone Shading (Minimum)

For each color region:

1. **Shadow** - Darkest shade
2. **Midtone** - Base color
3. **Highlight** - Lightest shade

Example (Red armor):

```
Shadow:    #8b0000 (dark red)
Midtone:   #dc143c (crimson)
Highlight: #ff6347 (tomato)

```

### Five-Tone Shading (Recommended)

For more depth:

1. **Core Shadow** - Deepest dark
2. **Shadow** - Dark shade
3. **Midtone** - Base color
4. **Highlight** - Light shade
5. **Specular** - Brightest highlight

### Gradient Mapping

For smooth gradients across 16 pixels:

```
Pixel Row:  0 1 2 3 4 5 6 7 8 9 A B C D E F
Color:      1 1 2 2 2 3 3 3 4 4 4 5 5 5 6 6
            └─ Shadow  ─┴─ Mid ─┴─ Light ─┘

```

---

## Special Effects

### Glow Effect

Add bright outer pixels around magic elements:

```
    777      ← Bright cyan glow
   77777
  7700077    ← Dark magic center
   77777
    777

```

Palette:

```csharp
{ '0', "#4b0082" },  // Dark indigo core
{ '7', "#00ffff" },  // Bright cyan glow

```

### Transparency Tricks

Use `'0'` for transparent pixels to create:

- Irregular shapes
- Overlapping sprites
- Anti-aliased edges (manually)

### Metallic Shine

For metal surfaces, add small bright pixels:

```
6666666
6667766    ← '7' = bright highlight
6666666
6677666    ← Scattered specular
6666666

```

---

## Validation Rules

### Required Validations

When parsing sprites, check:

1. **Dimensions:** Exactly 16 rows, each exactly 16 characters
2. **Characters:** All pixel characters exist in palette
3. **Palette:** All palette colors are valid formats
4. **Name:** Non-empty, unique identifier
5. **Transparency:** At least one palette entry (conventionally `'0'`)

### C# Validation Example

```csharp
public static bool ValidateSprite(PixelSprite sprite, out List<string> errors)
{
    errors = new List<string>();

    // Check name
    if (string.IsNullOrWhiteSpace(sprite.Name))
        errors.Add("Sprite name is required");

    // Check pixel data
    if (sprite.PixelData == null || sprite.PixelData.Length != 16)
        errors.Add("Sprite must have exactly 16 rows");
    else
    {
        for (int i = 0; i < 16; i++)
        {
            if (sprite.PixelData[i]?.Length != 16)
                errors.Add($"Row {i} must be exactly 16 characters");

            // Check all characters are in palette
            foreach (char c in sprite.PixelData[i])
            {
                if (!sprite.Palette.ContainsKey(c))
                    errors.Add($"Character '{c}' in row {i} not found in palette");
            }
        }
    }

    // Check palette
    if (sprite.Palette == null || sprite.Palette.Count == 0)
        errors.Add("Palette must have at least one entry");

    return errors.Count == 0;
}

```

---

## Conversion Tools

### From PNG to Sprite Data

For converting existing pixel art:

```csharp
public static PixelSprite FromPngFile(string filePath, int paletteSize = 16)
{
    using var bitmap = SKBitmap.Decode(filePath);

    if (bitmap.Width != 16 || bitmap.Height != 16)
        throw new ArgumentException("Image must be 16x16 pixels");

    // Extract unique colors
    var colorMap = new Dictionary<SKColor, char>();
    var palette = new Dictionary<char, string>();
    char currentChar = '0';

    var pixelData = new string[16];

    for (int y = 0; y < 16; y++)
    {
        var row = new char[16];

        for (int x = 0; x < 16; x++)
        {
            var pixel = bitmap.GetPixel(x, y);

            if (pixel.Alpha == 0)
            {
                row[x] = '0';
                palette['0'] = "transparent";
            }
            else
            {
                if (!colorMap.ContainsKey(pixel))
                {
                    colorMap[pixel] = currentChar;
                    palette[currentChar] = $"#{pixel.Red:X2}{pixel.Green:X2}{pixel.Blue:X2}";
                    currentChar++;
                }

                row[x] = colorMap[pixel];
            }
        }

        pixelData[y] = new string(row);
    }

    return new PixelSprite
    {
        Name = Path.GetFileNameWithoutExtension(filePath),
        PixelData = pixelData,
        Palette = palette
    };
}

```

### To PNG Export

```csharp
public void SaveAsPng(PixelSprite sprite, string outputPath, int scale = 1)
{
    var bitmap = sprite.ToBitmap(scale);

    using var image = SKImage.FromBitmap(bitmap);
    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
    using var stream = File.OpenWrite(outputPath);

    data.SaveTo(stream);
}

```

---

## Asset Organization

### Recommended File Structure

```
/Resources/Sprites/
  ├── player_sprites.json      # All player character sprites
  ├── enemy_sprites.json        # All enemy sprites
  ├── item_icons.json           # Consumables, equipment icons
  ├── ui_icons.json             # UI elements, buttons
  ├── terrain_sprites.json      # Environmental tiles
  └── effect_sprites.json       # Status effects, particles

```

### Naming Conventions

**Characters:**

- `shieldmaiden`, `berserker`, `runecaster`, `greatsword`
- `draugr`, `goblinwarg`, `jotunforged`, `necromancer`

**Items:**

- `potion_health`, `potion_mana`, `potion_stamina`
- `scroll_fireball`, `scroll_icestorm`
- `sword_iron`, `sword_steel`, `sword_mythril`

**UI Elements:**

- `icon_attack`, `icon_defend`, `icon_ability`
- `cursor_default`, `cursor_attack`, `cursor_move`

**Effects:**

- `effect_burning`, `effect_poison`, `effect_stunned`
- `particle_sparkle`, `particle_blood`

---

## Performance Considerations

### Caching Strategy

Always cache rendered bitmaps:

```csharp
private readonly ConcurrentDictionary<string, Bitmap> _cache = new();

public Bitmap GetSprite(string name, int scale)
{
    var key = $"{name}_{scale}";

    if (_cache.TryGetValue(key, out var cached))
        return cached;

    var sprite = LoadSpriteDefinition(name);
    var bitmap = sprite.ToBitmap(scale);

    _cache[key] = bitmap;
    return bitmap;
}

```

### Memory Usage

Estimated memory per sprite:

- **Raw data:** ~500 bytes (16×16 array + palette)
- **3x bitmap:** ~9 KB (48×48 RGBA)
- **5x bitmap:** ~25 KB (80×80 RGBA)

For 100 sprites cached at both scales: **~3.4 MB total**

### Load Strategies

1. **Eager Loading:** Load all sprites on startup (fast access, high memory)
2. **Lazy Loading:** Load on first use (slow first access, low memory)
3. **Hybrid:** Preload common sprites, lazy load rare ones ✅ **Recommended**

---

## Future Extensions

### Animated Sprites

For future animation support:

```csharp
public class AnimatedSprite
{
    public string Name { get; set; }
    public List<PixelSprite> Frames { get; set; }  // Animation frames
    public int FrameDurationMs { get; set; }        // Time per frame
    public bool Loop { get; set; }
}

```

### Multi-Resolution Sprites

For different sprite sizes:

```csharp
public class MultiResSprite
{
    public string Name { get; set; }
    public PixelSprite Sprite16x16 { get; set; }   // Standard
    public PixelSprite Sprite32x32 { get; set; }   // Large units
    public PixelSprite Sprite8x8 { get; set; }     // Minimap
}

```

---

## Complete Sprite Library Checklist

### Player Characters (8 base classes × 40 specializations)

**Warrior Path:**

- [ ]  Shieldmaiden (Done)
- [ ]  Berserker (Done)
- [ ]  Greatsword
- [ ]  Axe-Wielder

**Magic Path:**

- [ ]  Runecaster (Prototype exists)
- [ ]  Seidr-Weaver
- [ ]  Galdr-Singer
- [ ]  Staff-Bearer

### Enemies (60+ types)

**Undead:**

- [ ]  Draugr
- [ ]  Skeletal Warrior
- [ ]  Wraith

**Creatures:**

- [ ]  Goblin Rider
- [ ]  Wolf
- [ ]  Bear

**Constructs:**

- [ ]  Jötun-Forged
- [ ]  Dwemer-Built

**Casters:**

- [ ]  Necromancer
- [ ]  Dark Sorcerer

### Items (200+ types)

**Consumables:**

- [ ]  Health Potion (Red)
- [ ]  Mana Potion (Blue)
- [ ]  Stamina Potion (Green)

**Weapons:**

- [ ]  Iron Sword
- [ ]  Steel Axe
- [ ]  Rune Staff

**Armor:**

- [ ]  Leather Armor
- [ ]  Chain Mail
- [ ]  Plate Armor

### UI Icons (50+)

**Actions:**

- [ ]  Attack
- [ ]  Defend
- [ ]  Move
- [ ]  Ability
- [ ]  Item

**Status Effects:**

- [ ]  Burning
- [ ]  Poison
- [ ]  Stunned
- [ ]  Blessed
- [ ]  Cursed

---

## Appendix: Complete Palette Reference

### Master Color Palette (Hex Codes)

**Grayscale (Outlines & Neutrals):**

```
#000000 - Pure black
#0d0d0d - Very dark gray
#1a1a1a - Dark gray 1
#2a2a2a - Dark gray 2
#3a3a3a - Dark gray 3
#4a4a4a - Medium dark gray
#6a6a6a - Medium gray
#8a8a8a - Light medium gray
#a9a9a9 - Light gray
#c0c0c0 - Silver
#d3d3d3 - Light silver
#e8e8e8 - Very light gray
#ffffff - Pure white

```

**Skin Tones:**

```
#8b4513 - Dark brown
#a0522d - Sienna
#cd853f - Peru
#deb887 - Burlywood
#f5deb3 - Wheat
#ffe4c4 - Bisque

```

**Full Reference:** See official 16-bit color palette documentation

---

**End of Sprite Data Format Specification**