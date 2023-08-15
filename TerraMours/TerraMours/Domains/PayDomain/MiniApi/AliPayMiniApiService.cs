using Microsoft.AspNetCore.Mvc;
using TerraMours_Gpt.Domains.PayDomain.Contracts.Req;
using TerraMours_Gpt.Domains.PayDomain.IServices;
using APIServiceBase = Microsoft.AspNetCore.Builder.ServiceBase;

namespace TerraMours.Domains.LoginDomain.MiniApi
{
    /// <summary>
    /// 支付宝支付
    /// </summary>
    public class AliPayMiniApiService : APIServiceBase
    {
        private readonly IPayService _payService;

        /// <summary>
        /// 构造函数以及获取必要的服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="payService"></param>
        public AliPayMiniApiService(IServiceCollection services, IPayService payService) : base()
        {
            _payService = payService;
            //此处/api/v1/Test 这里是swagger显示的路由
            //命名规则取当前的xxxMiniApiService的xxx,然后/api/v1/xxx/方法名
            App.MapPost("/api/v1/AliPay/PreCreate", PreCreate);
            App.MapPost("/api/v1/AliPay/Query", Query);
        }

        /// <summary>
        /// 当面付-扫码支付
        /// </summary>
        [HttpPost]
        public async Task<IResult> PreCreate(AlipayTradePreCreateReq viewModel)
        {
            var res = await _payService.PreCreate(viewModel);
            return Results.Ok(res);
        }

        /// <summary>
        /// 支付宝交易查询
        /// </summary>
        [HttpPost]
        public async Task<IResult> Query(AlipayTradeQueryReq viewMode)
        {
            var res = await _payService.Query(viewMode);
            return Results.Ok(res);
        }


        /*  /// <summary>
          /// 生成二维码SVG  QRCoder nuget
          /// </summary>
          /// <param name="data">数据</param>
          /// <param name="size">尺寸</param>
          [HttpGet]
          public IActionResult GetQRCode(string data)
          {
              using (var qrGenerator = new QRCodeGenerator())
              using (var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.L))
              using (var pngByteQrCode = new PngByteQRCode(qrCodeData))
              {
                  var pngBytes = pngByteQrCode.GetGraphic(20, false);
                  return File(pngBytes, "image/png");
              }
          }*/
    }
}
