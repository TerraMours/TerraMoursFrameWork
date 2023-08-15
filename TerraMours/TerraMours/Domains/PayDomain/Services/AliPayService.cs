using AutoMapper;
using Essensoft.Paylink.Alipay;
using Essensoft.Paylink.Alipay.Domain;
using Essensoft.Paylink.Alipay.Request;
using Essensoft.Paylink.Alipay.Response;
using Microsoft.Extensions.Options;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours_Gpt.Domains.PayDomain.Contracts.Req;
using TerraMours_Gpt.Domains.PayDomain.IServices;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.PaymentModels;

namespace TerraMours_Gpt.Domains.PayDomain.Services
{
    public class AliPayService : IPayService
    {
        private readonly IAlipayClient _client;
        private readonly IOptions<AlipayOptions> _optionsAccessor;
        private readonly IMapper _mapper;
        private readonly FrameworkDbContext _dbContext;

        public AliPayService(IAlipayClient client, IOptions<AlipayOptions> optionsAccessor, IMapper mapper, FrameworkDbContext dbContext)
        {
            _client = client;
            _optionsAccessor = optionsAccessor;
            _mapper = mapper;
            _dbContext = dbContext;

        }

        /// <summary>
        ///  当面付-扫码支付
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<ApiResponse<AlipayTradePrecreateResponse>> PreCreate(AlipayTradePreCreateReq req)
        {
            var model = new AlipayTradePrecreateModel
            {
                //系统内部的唯一交易号  $"TerraMours-{Guid.NewGuid()}"
                //OutTradeNo = req.OutTradeNo,
                OutTradeNo = $"TerraMours-{Guid.NewGuid()}",
                //类似于邮件主题
                Subject = req.Name,
                //金额总数
                TotalAmount = req.Price.ToString(),
                //描述
                Body = req.Description
            };
            var request = new AlipayTradePrecreateRequest();
            request.SetBizModel(model);
            request.SetNotifyUrl(req.NotifyUrl);

            //此时应该先在自己的order表里面创建一个待支付的订单

            var order = new Order(req.ProductId, req.Name, req.Description, req.Price, req.UserId);
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            //测试本地先生成一个支付二维码图片，后面再加vue项目
            var response = await _client.ExecuteAsync(request, _optionsAccessor.Value);

            if (!response.IsError)
            {
                return ApiResponse<AlipayTradePrecreateResponse>.Success(response);
            }

            return ApiResponse<AlipayTradePrecreateResponse>.Fail("支付宝当面付二维码生成失败");
        }

        /// <summary>
        /// 交易查询
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>

        public async Task<ApiResponse<AlipayTradeQueryResponse>> Query(AlipayTradeQueryReq req)
        {
            var model = new AlipayTradeQueryModel
            {
                OutTradeNo = req.OutTradeNo,
                //可以不传
                TradeNo = req.TradeNo
            };

            var request = new AlipayTradeQueryRequest();
            request.SetBizModel(model);

            var res = await _client.ExecuteAsync(request, _optionsAccessor.Value);
            return ApiResponse<AlipayTradeQueryResponse>.Success(res);
        }

    }
}
