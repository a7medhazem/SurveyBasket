namespace SurveyBasket.Api.Helpers;

public static class HtmlResponseBuilder
{
    public static string GenerateHtmlResponse(string templateName)
    {
        var templatePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Templates",
            $"{templateName}.html"
        );

        return File.ReadAllText(templatePath);
    }
}