using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class CustomFieldDefinitionTests
{
    private Project CreateTestProject()
    {
        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        return Project.Create(workspace, "Test", Slug.Create("test").Value, "TEST", Guid.NewGuid()).Value;
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = CustomFieldDefinition.Create(project, "Priority Level", CustomFieldType.Text, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Priority Level");
        result.Value.FieldType.Should().Be(CustomFieldType.Text);
        result.Value.IsRequired.Should().BeFalse();
    }

    [Fact]
    public void Create_WithAllParameters_ShouldSetThemCorrectly()
    {
        // Arrange
        var project = CreateTestProject();
        var options = "[\"Option1\", \"Option2\"]";

        // Act
        var result = CustomFieldDefinition.Create(
            project, 
            "Dropdown Field", 
            CustomFieldType.SingleSelect, 
            Guid.NewGuid(),
            description: "A dropdown field",
            isRequired: true,
            order: 5,
            options: options,
            defaultValue: "Option1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().Be("A dropdown field");
        result.Value.IsRequired.Should().BeTrue();
        result.Value.Order.Should().Be(5);
        result.Value.Options.Should().Be(options);
        result.Value.DefaultValue.Should().Be("Option1");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = CustomFieldDefinition.Create(project, "", CustomFieldType.Text, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("name");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();
        var longName = new string('a', 101);

        // Act
        var result = CustomFieldDefinition.Create(project, longName, CustomFieldType.Text, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("100");
    }

    [Theory]
    [InlineData(CustomFieldType.Text)]
    [InlineData(CustomFieldType.Number)]
    [InlineData(CustomFieldType.Date)]
    [InlineData(CustomFieldType.DateTime)]
    [InlineData(CustomFieldType.User)]
    [InlineData(CustomFieldType.Url)]
    [InlineData(CustomFieldType.Boolean)]
    public void Create_WithNonSelectFieldTypes_ShouldSucceed(CustomFieldType fieldType)
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = CustomFieldDefinition.Create(project, "Field", fieldType, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FieldType.Should().Be(fieldType);
    }

    [Fact]
    public void Create_SingleSelectWithOptions_ShouldSucceed()
    {
        // Arrange
        var project = CreateTestProject();
        var options = "[\"Option1\", \"Option2\", \"Option3\"]";

        // Act
        var result = CustomFieldDefinition.Create(
            project, 
            "Priority", 
            CustomFieldType.SingleSelect, 
            Guid.NewGuid(),
            options: options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Options.Should().Be(options);
    }

    [Fact]
    public void Create_SingleSelectWithoutOptions_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = CustomFieldDefinition.Create(project, "Priority", CustomFieldType.SingleSelect, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("options");
    }

    [Fact]
    public void Create_MultiSelectWithOptions_ShouldSucceed()
    {
        // Arrange
        var project = CreateTestProject();
        var options = "[\"Tag1\", \"Tag2\"]";

        // Act
        var result = CustomFieldDefinition.Create(
            project, 
            "Tags", 
            CustomFieldType.MultiSelect, 
            Guid.NewGuid(),
            options: options);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_MultiSelectWithoutOptions_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = CustomFieldDefinition.Create(project, "Tags", CustomFieldType.MultiSelect, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateField()
    {
        // Arrange
        var field = CreateValidField();

        // Act
        var result = field.Update("New Name", "Description", true, 10, "[\"A\", \"B\"]", "A", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        field.Name.Should().Be("New Name");
        field.Description.Should().Be("Description");
        field.IsRequired.Should().BeTrue();
        field.Order.Should().Be(10);
        field.Options.Should().Be("[\"A\", \"B\"]");
        field.DefaultValue.Should().Be("A");
    }

    [Fact]
    public void Update_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var field = CreateValidField();

        // Act
        var result = field.Update("", null, false, 0, null, null, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    private CustomFieldDefinition CreateValidField()
    {
        var project = CreateTestProject();
        return CustomFieldDefinition.Create(project, "Field", CustomFieldType.Text, Guid.NewGuid()).Value;
    }
}
