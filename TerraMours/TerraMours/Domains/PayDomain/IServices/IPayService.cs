using Essensoft.Paylink.Alipay.Response;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours_Gpt.Domains.PayDomain.Contracts.Req;

namespace TerraMours_Gpt.Domains.PayDomain.IServices
{
    /// <summary>
    /// 支付接口
    /// </summary>
    public interface IPayService
    {
        /// <summary>
        /// 支付宝当面扫预支付二维码字符串
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ApiResponse<AlipayTradePrecreateResponse>> PreCreate(AlipayTradePreCreateReq req);

        /// <summary>
        /// 支付宝交易查询
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ApiResponse<AlipayTradeQueryResponse>> Query(AlipayTradeQueryReq req);

        /// <summary>
        /// 根据订单号或者交易号查询订单状态，判断用户是否支付
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        //Task<ApiResponse<AlipayTradeQueryResponse>> QueryPaymentStatus(AlipayTradeQueryReq req);

    }
}
