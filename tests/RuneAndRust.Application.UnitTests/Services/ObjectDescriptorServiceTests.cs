using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class ObjectDescriptorServiceTests
{
    private Mock<ILogger<DescriptorService>> _descriptorLoggerMock = null!;
    private Mock<ILogger<ObjectDescriptorService>> _objectLoggerMock = null!;
    private DescriptorService _descriptorService = null!;
    private ObjectDescriptorService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _descriptorLoggerMock = new Mock<ILogger<DescriptorService>>();
        _objectLoggerMock = new Mock<ILogger<ObjectDescriptorService>>();

        var pools = CreateTestPools();
        var theme = CreateTestTheme();
        var objectConfig = CreateTestObjectConfig();

        _descriptorService = new DescriptorService(pools, theme, _descriptorLoggerMock.Object);
        _service = new ObjectDescriptorService(_descriptorService, objectConfig, _objectLoggerMock.Object);
    }

    [Test]
    public void GenerateDescriptor_WithDoorClosed_ReturnsAllDescriptionLevels()
    {
        var context = new ObjectDescriptorContext
        {
            ObjectType = InteractiveObjectType.Door,
            State = ObjectState.Closed,
            Depth = ExaminationDepth.Look
        };

        var result = _service.GenerateDescriptor(context);

        Assert.That(result.ObjectType, Is.EqualTo(InteractiveObjectType.Door));
        Assert.That(result.State, Is.EqualTo(ObjectState.Closed));
        Assert.That(result.GlanceDescription, Is.Not.Empty);
        Assert.That(result.LookDescription, Is.Not.Empty);
        Assert.That(result.ExamineDescription, Is.Not.Empty);
    }

    [Test]
    public void GenerateDescriptor_WithLockedDoor_IncludesInteractionHint()
    {
        var context = new ObjectDescriptorContext
        {
            ObjectType = InteractiveObjectType.Door,
            State = ObjectState.Locked,
            Depth = ExaminationDepth.Examine
        };

        var result = _service.GenerateDescriptor(context);

        Assert.That(result.InteractionHint, Is.Not.Null);
        Assert.That(result.InteractionHint, Does.Contain("key"));
    }

    [Test]
    public void GetDescription_AtGlanceDepth_ReturnsGlanceDescription()
    {
        var context = new ObjectDescriptorContext
        {
            ObjectType = InteractiveObjectType.Chest,
            State = ObjectState.Closed,
            Depth = ExaminationDepth.Glance
        };

        var result = _service.GetDescription(context);

        Assert.That(result, Is.Not.Empty);
        // Glance descriptions are typically shorter
    }

    [Test]
    public void GetDescription_AtLookDepth_ReturnsLookDescription()
    {
        var context = new ObjectDescriptorContext
        {
            ObjectType = InteractiveObjectType.Lever,
            State = ObjectState.Up,
            Depth = ExaminationDepth.Look
        };

        var result = _service.GetDescription(context);

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GetDescription_AtExamineDepth_IncludesInteractionHint()
    {
        var context = new ObjectDescriptorContext
        {
            ObjectType = InteractiveObjectType.Lever,
            State = ObjectState.Up,
            Depth = ExaminationDepth.Examine
        };

        var result = _service.GetDescription(context);

        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Does.Contain("pulled"));
    }

    [Test]
    public void GenerateDescriptor_WithMaterial_AddsMaterialDetail()
    {
        var context = new ObjectDescriptorContext
        {
            ObjectType = InteractiveObjectType.Door,
            State = ObjectState.Normal,
            Depth = ExaminationDepth.Examine,
            Material = "iron"
        };

        var result = _service.GenerateDescriptor(context);

        Assert.That(result.ExamineDescription, Does.Contain("iron"));
    }

    [Test]
    public void GenerateDescriptor_WithAge_AddsAgeDetail()
    {
        var context = new ObjectDescriptorContext
        {
            ObjectType = InteractiveObjectType.Statue,
            State = ObjectState.Normal,
            Depth = ExaminationDepth.Examine,
            Age = "ancient"
        };

        var result = _service.GenerateDescriptor(context);

        Assert.That(result.ExamineDescription, Does.Contain("ancient"));
    }

    [Test]
    public void GetGlanceDescription_ForChestLocked_ReturnsLockedDescription()
    {
        var result = _service.GetGlanceDescription(InteractiveObjectType.Chest, ObjectState.Locked);

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GenerateDescriptor_WithEnvironmentContext_UsesEnvironmentTags()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new ObjectDescriptorContext
        {
            ObjectType = InteractiveObjectType.Door,
            State = ObjectState.Closed,
            Depth = ExaminationDepth.Look,
            Environment = environment
        };

        var result = _service.GenerateDescriptor(context);

        Assert.That(result.LookDescription, Is.Not.Empty);
    }

    [Test]
    public void InteractiveObjectDescriptor_GetDescription_ReturnsCorrectLevel()
    {
        var descriptor = new InteractiveObjectDescriptor
        {
            ObjectType = InteractiveObjectType.Altar,
            State = ObjectState.Active,
            GlanceDescription = "Glance text",
            LookDescription = "Look text",
            ExamineDescription = "Examine text",
            InteractionHint = "Hint text"
        };

        Assert.That(descriptor.GetDescription(ExaminationDepth.Glance), Is.EqualTo("Glance text"));
        Assert.That(descriptor.GetDescription(ExaminationDepth.Look), Is.EqualTo("Look text"));
        Assert.That(descriptor.GetDescription(ExaminationDepth.Examine), Does.Contain("Examine text"));
        Assert.That(descriptor.GetDescription(ExaminationDepth.Examine), Does.Contain("Hint text"));
    }

    private static IReadOnlyDictionary<string, DescriptorPool> CreateTestPools()
    {
        return new Dictionary<string, DescriptorPool>
        {
            ["objects.door_closed_glance"] = CreatePool("door_closed_glance", [
                ("heavy_door", "A heavy door bars the passage", 25)
            ]),
            ["objects.door_locked_glance"] = CreatePool("door_locked_glance", [
                ("locked_door", "A locked door bars your path", 30)
            ]),
            ["objects.door_look"] = CreatePool("door_look", [
                ("heavy_wooden", "A heavy wooden door reinforced with iron bands. It looks sturdy", 25)
            ]),
            ["objects.door_examine"] = CreatePool("door_examine", [
                ("iron_riveted", "The door is a heavy slab of blackened iron, riveted with thick bolts", 20)
            ]),
            ["objects.chest_closed_glance"] = CreatePool("chest_closed_glance", [
                ("old_chest", "An old chest sits against the wall", 25)
            ]),
            ["objects.chest_locked_glance"] = CreatePool("chest_locked_glance", [
                ("locked_chest", "A locked chest sits against the wall", 30)
            ]),
            ["objects.lever_up_glance"] = CreatePool("lever_up_glance", [
                ("raised_lever", "A lever in the up position protrudes from the wall", 30)
            ]),
            ["objects.lever_look"] = CreatePool("lever_look", [
                ("mechanism_simple", "A simple lever connected to some mechanism within the wall", 30)
            ]),
            ["objects.lever_examine"] = CreatePool("lever_examine", [
                ("simple_switch", "A simple lever mechanism. It could be pulled", 30)
            ]),
            ["objects.statue_glance"] = CreatePool("statue_glance", [
                ("stone_statue", "A stone statue stands in silent vigil", 25)
            ]),
            ["objects.altar_glance"] = CreatePool("altar_glance", [
                ("stone_altar", "A stone altar dominates the center of the room", 25)
            ])
        };
    }

    private static DescriptorPool CreatePool(string id, (string id, string text, int weight)[] descriptors)
    {
        return new DescriptorPool
        {
            Id = id,
            Name = id,
            Descriptors = descriptors.Select(d => new Descriptor
            {
                Id = d.id,
                Text = d.text,
                Weight = d.weight
            }).ToList()
        };
    }

    private static ThemeConfiguration CreateTestTheme()
    {
        return new ThemeConfiguration
        {
            ActiveTheme = "default",
            Themes = new Dictionary<string, ThemePreset>
            {
                ["default"] = new ThemePreset
                {
                    Id = "default",
                    Name = "Default",
                    Description = "Standard theme"
                }
            }
        };
    }

    private static ObjectDescriptorConfiguration CreateTestObjectConfig()
    {
        return new ObjectDescriptorConfiguration
        {
            ObjectTypes = new Dictionary<string, ObjectTypeDefinition>
            {
                ["door"] = new ObjectTypeDefinition
                {
                    Id = "door",
                    Name = "Door",
                    ValidStates = ["normal", "closed", "open", "locked", "barred", "broken"],
                    DefaultState = "closed",
                    IsInteractable = true
                },
                ["chest"] = new ObjectTypeDefinition
                {
                    Id = "chest",
                    Name = "Chest",
                    ValidStates = ["normal", "closed", "open", "locked", "empty"],
                    DefaultState = "closed",
                    IsInteractable = true
                },
                ["lever"] = new ObjectTypeDefinition
                {
                    Id = "lever",
                    Name = "Lever",
                    ValidStates = ["normal", "up", "down", "stuck"],
                    DefaultState = "up",
                    IsInteractable = true
                },
                ["statue"] = new ObjectTypeDefinition
                {
                    Id = "statue",
                    Name = "Statue",
                    ValidStates = ["normal", "damaged", "active"],
                    DefaultState = "normal",
                    IsInteractable = false
                },
                ["altar"] = new ObjectTypeDefinition
                {
                    Id = "altar",
                    Name = "Altar",
                    ValidStates = ["normal", "active", "inactive", "desecrated"],
                    DefaultState = "inactive",
                    IsInteractable = true
                }
            }
        };
    }

    private static EnvironmentContext CreateEnvironmentContext(string biome, string climate, string lighting)
    {
        var values = new Dictionary<string, string>
        {
            ["biome"] = biome,
            ["climate"] = climate,
            ["lighting"] = lighting
        };
        return new EnvironmentContext(values, [biome, climate]);
    }
}
