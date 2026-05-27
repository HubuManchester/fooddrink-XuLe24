namespace FoodieApp.Models;

/// <summary>Represents a food recipe with ingredients and cooking steps.</summary>
public class Recipe
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> Ingredients { get; set; } = new();
    public List<string> Steps { get; set; } = new();
    public int PrepTimeMinutes { get; set; }
    public bool IsFavourite { get; set; }
    public string Summary => $"{Ingredients.Count} ingredients · {PrepTimeMinutes} min";
}