using MediatR;
using Newtonsoft.Json;
using S3.Gateway.Common;
using S3.Gateway.Data;
using S3.Gateway.Entities;
using S3.Gateway.Integrations.Napas;
using System.Net;
using System.Net.Http.Headers;
namespace S3.Gateway.Features.Payments.Napas
{
    public class GetDeepLinkRequest : NpGetDeepLinkRequest, IRequest<ResponseBase<NpGetDeepLinkResponse>>
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
    }

    public class GetDeepLinkRequestHandler : IRequestHandler<GetDeepLinkRequest, ResponseBase<NpGetDeepLinkResponse>>
    {
        private readonly NapasClient _napasClient;
        private readonly DBContext _dbContext;

        public GetDeepLinkRequestHandler(NapasClient napasClient, DBContext dbContext)
        {
            _napasClient = napasClient;
            _dbContext = dbContext;
        }

        private async Task CallAPI()
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.All
                };

                var client = new HttpClient(handler);
                //client.DefaultRequestVersion = HttpVersion.Version11;


                client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
                client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://mms-api-staging.napas.com.vn/oauth2/token");

                var collection = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "client_credentials"),
                    new("client_id", "USER971283"),
                    new("client_secret", "b3huAXe6bVStMfTBspgbGerXBy7t4SF7")
                };
                request.Content = new FormUrlEncodedContent(collection);

                var response = await client.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();
               Console.WriteLine(response.StatusCode);
                Console.WriteLine(body);


            }
            catch (Exception ex)
            {

            }
        }

        public async Task<ResponseBase<NpGetDeepLinkResponse>> Handle(GetDeepLinkRequest request, CancellationToken cancellationToken)
        {
            var response = new ResponseBase<NpGetDeepLinkResponse>();
            try
            {
                NpGetDeepLinkRequest npGetDeepLinkRequest = request;
                npGetDeepLinkRequest.SenderReference = RequestUtility.CreateSenderReference("NX");
                npGetDeepLinkRequest.CreationDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00";
                npGetDeepLinkRequest.PlatformCode = "NX";
                npGetDeepLinkRequest.PlatformName = "NexusA3";
                npGetDeepLinkRequest.PlatformMerchantId = "PAWN000001";
                npGetDeepLinkRequest.MobileOS = "Android";

                var data = await _napasClient.GetDeepLink(npGetDeepLinkRequest);
                var callbackRouting = new CallbackRouting
                {
                    RequestID = RequestContext.RequestID,
                    Target = Target3rd.Napas,
                    PartnerCode = request.PartnerCode,
                    CallbackUrl = request.CallbackUrl,
                    RequestPayload = JsonConvert.SerializeObject(request),
                    ActionHistory = "GET DEEP LINK",
                };

                var callbackRoutingLog = new CallbackRoutingLog
                {
                    RequestID = callbackRouting.RequestID,
                    Action = callbackRouting.ActionHistory,
                    RequestPayload = JsonConvert.SerializeObject(npGetDeepLinkRequest),
                    ResponsePayload = JsonConvert.SerializeObject(data)
                };

                _dbContext.CallbackRoutings.Add(callbackRouting);
                _dbContext.CallbackRoutingLogs.Add(callbackRoutingLog);
                await _dbContext.SaveChangesAsync(cancellationToken);
                response.IsSuccess = true;
                response.Data = data;
                return response;
            }
            catch
            {
                response.Message = "Có lỗi xảy ra";
                return response;
            }
        }
    }
}