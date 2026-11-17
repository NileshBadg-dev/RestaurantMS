namespace RestaurantMS.Web.Models;

public class ImageUploaderModel
{
    public string Name { get; set; } = "Images";
    public int MaxImages { get; set; } = 3;
    public int MaxSizeMb { get; set; } = 2;
    public List<string> ExistingImageUrls { get; set; } = new();
    public string Label { get; set; } = "Upload Images";
    public string HelpText { get; set; } = "JPG, PNG or WEBP up to 2 MB each. Maximum 3 images.";
}
