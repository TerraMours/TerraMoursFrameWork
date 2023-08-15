using Essensoft.Paylink.Alipay;
using Essensoft.Paylink.Alipay.Domain;
using Essensoft.Paylink.Alipay.Request;
using Essensoft.Paylink.Alipay.Response;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours_Gpt.Domains.PayDomain.Contracts.Req;

namespace TerraMours_Gpt.Domains.PayDomain.Hubs
{
    /// <summary>
    /// 支付相关的长连接
    /// </summary>
    [EnableCors("MyPolicy")]
    public class PaymentHub : Hub
    {
        private readonly IAlipayClient _client;
        private readonly IOptions<AlipayOptions> _optionsAccessor;
        private readonly FrameworkDbContext _dbContext;

        public PaymentHub(IAlipayClient client, IOptions<AlipayOptions> optionsAccessor, FrameworkDbContext dbContext)
        {
            _client = client;
            _optionsAccessor = optionsAccessor;
            _dbContext = dbContext;
        }

        /// <summary>
        /// 即时查询状态
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task QueryPaymentStatus(AlipayTradeQueryReq req)
        {
            //获取当前连接id
            var connectionId = this.Context.ConnectionId;

            //先查询系统里面是否有此账单
            // todo  支付成功 则修改用户vip信息
            var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.OrderId == req.OutTradeNo) ?? throw new Exception("此订单不存在");
            var model = new AlipayTradeQueryModel
            {
                OutTradeNo = req.OutTradeNo,
                //可以不传
                TradeNo = req.TradeNo
            };

            var request = new AlipayTradeQueryRequest();
            request.SetBizModel(model);
            //循环查询3分钟这个账单，超时或者状态位已支付则停止
            using var httpClient = new HttpClient();
            var startTime = DateTime.Now;
            //先查一次 以免使用未赋值的变量
            AlipayTradeQueryResponse queryPayRes = await _client.ExecuteAsync(request, _optionsAccessor.Value);
            while ((DateTime.Now - startTime).TotalMinutes <= 3)
            {
                queryPayRes = await _client.ExecuteAsync(request, _optionsAccessor.Value);

                if (queryPayRes.TradeStatus != "WAIT_BUYER_PAY")
                {
                    //交易状态：WAIT_BUYER_PAY（交易创建，等待买家付款）、TRADE_CLOSED（未付款交易超时关闭，或支付完成后全额退款）、TRADE_SUCCESS（交易支付成功）、TRADE_FINISHED（交易结束，不可退款）
                    break;
                }
                // 间隔3秒进行下一次查询
                await Task.Delay(3000);
            }
            // 如果超过3分钟仍未支付，发送订单状态信息

            order.PayOrder(queryPayRes.TradeNo, queryPayRes.TradeStatus);
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync();
            //如果支付成功，则把user的余额加上新充值的钱
            var user = await _dbContext.SysUsers.FirstOrDefaultAsync(x => x.UserEmail == order.UserId) ?? throw new Exception("此用户不存在");
            user.Balance = user.Balance + order.Price;
            _dbContext.SysUsers.Update(user);
            await _dbContext.SaveChangesAsync();
            await Clients.Client(connectionId).SendAsync("QueryPaymentStatus", queryPayRes);
        }

    }
}
