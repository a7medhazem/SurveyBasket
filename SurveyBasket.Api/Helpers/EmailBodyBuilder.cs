namespace SurveyBasket.Api.Helpers;

public static class EmailBodyBuilder
{
    public static string GenerateEmailBody(string template, Dictionary<string, string> templateModel)
    {
        // 1. Build the full path of the HTML template file
        var templatePath = $"{Directory.GetCurrentDirectory()}/Templates/{template}.html";

        // 2. Read the template content from file
        var streamReader = new StreamReader(templatePath);
        var body = streamReader.ReadToEnd();
        streamReader.Close();

        // 3. Replace placeholders in the template with actual values
        foreach (var item in templateModel)
            body = body.Replace(item.Key, item.Value);

        // 4. Return the final email body
        return body;
    }
}