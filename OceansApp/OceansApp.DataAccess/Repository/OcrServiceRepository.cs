using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using OceansApp.DataAccess.Repository.IRepository;
using System.Text;
namespace OceansApp.DataAccess.Repository
{
    public class OcrServiceRepository : IOcrServiceRepository
    {
        private readonly DocumentAnalysisClient _client;

        public OcrServiceRepository(string azureFormRecognizerEndpoint, string azureKey)
        {
            var credential = new AzureKeyCredential(azureKey);
            _client = new DocumentAnalysisClient(new Uri(azureFormRecognizerEndpoint), credential);
        }

        public async Task<string> ExtractLayoutTextFromFileAsync(string fileUrl)
        {
            var operation = await _client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-layout", new Uri(fileUrl));
            var result = operation.Value;

            var sb = new StringBuilder();

            // 👇 Extraer contenido general (fuera de tablas)
            if (result.Paragraphs != null)
            {
                foreach (var paragraph in result.Paragraphs)
                {
                    sb.AppendLine(paragraph.Content.Trim());
                }
            }
            else if (!string.IsNullOrWhiteSpace(result.Content))
            {
                sb.AppendLine(result.Content.Trim());
            }

            sb.AppendLine();

            // 👇 Extraer contenido de tablas (como ya lo hacías)
            foreach (var table in result.Tables)
            {
                for (int rowIndex = 0; rowIndex < table.RowCount; rowIndex++)
                {
                    var rowValues = new List<string>();
                    for (int colIndex = 0; colIndex < table.ColumnCount; colIndex++)
                    {
                        var cell = table.Cells.FirstOrDefault(c => c.RowIndex == rowIndex && c.ColumnIndex == colIndex);
                        rowValues.Add(cell?.Content ?? "");
                    }
                    sb.AppendLine(string.Join(" | ", rowValues));
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

    }

}