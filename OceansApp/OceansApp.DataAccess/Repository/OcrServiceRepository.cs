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

        public async Task<string> ExtractTextFromFileAsync(string fileUrl)
        {
            var operation = await _client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-read", new Uri(fileUrl));
            var result = operation.Value;

            var sb = new StringBuilder();
            foreach (var page in result.Pages)
            {
                foreach (var line in page.Lines)
                {
                    sb.AppendLine(line.Content);
                }
            }

            return sb.ToString();
        }



    }

}