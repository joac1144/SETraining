using Azure.Storage.Blobs;
using SETraining.Server.Repositories;
using SETraining.Shared;
using Xunit;

namespace Server.Repositories.Tests;

public class UploadRepositoriesTest : IDisposable
{
    private BlobServiceClient _serviceClient;
    private BlobContainerClient _containerClient;
    private UploadRepository _repository;
    
    public UploadRepositoriesTest()
    {
        // Using Azurite from Docker Image to mock Azure Blob Storage, a default connecionsstring.
        _serviceClient = new BlobServiceClient(
            "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;");
        
        _containerClient = _serviceClient.CreateBlobContainer("imagetest");
        _repository = new UploadRepository(_containerClient);
    }
    
    [Fact]
    public async Task CreateUploadAsync_returns_status_Created()
    {
        var ExpectedResult = Status.Created;
        
        var result = await _repository.CreateUploadAsync("tester.jpg", "jpeg", new MemoryStream());

        Assert.Equal(ExpectedResult, result.status); 
    }

    [Fact]
    public async Task CreateUploadAsync_returns_status_URI_On_Success()
    {
        var ExpectedURI = new Uri("http://127.0.0.1:10000/devstoreaccount1/imagetest/tester.jpg");

        var result = await _repository.CreateUploadAsync("tester.jpg", "jpeg", new MemoryStream());
        
        Assert.Equal(ExpectedURI, result.uri); 
    }
    
    public void Dispose()
    {
         _containerClient.Delete();
    }
}