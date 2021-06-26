using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.Extensions.Configuration;
using Stripe;
using Order = Core.Entities.OrderAggregate.Order;
using Product = Core.Entities.Product;

namespace Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBasketRepository _basketRepository;

        public PaymentService(IBasketRepository basketRepository, IUnitOfWork unitOfWork, IConfiguration config)
        {
            _config = config;
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
           

        }

        public async Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId)
        {
                                                     
                StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];

                var basket = await _basketRepository.GetBasketAsync(basketId);

                if(basket == null ) return null;

                var shippingPrice = 0m; 

                if(basket.DeliveryMethodId.HasValue)
                {
                    var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync((int)basket.DeliveryMethodId);
                    shippingPrice = deliveryMethod.Price;
                }
               foreach (var item in basket.Items) 
               {
                    var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);

                    if(item.Price != productItem.Price)
                    {
                        item.Price = productItem.Price;
                    }
               }

               var service = new PaymentIntentService();

               PaymentIntent intent;

               if (string.IsNullOrEmpty(basket.PaymentIntentId))
               {
                   var options = new PaymentIntentCreateOptions
                   {
                        Amount = (long)basket.Items.Sum(i => i.Quantity * (i.Price * 100)) + (long)
                        shippingPrice * 100,
                        Currency = "gbp",
                        PaymentMethodTypes = new List<string> {"card"}
                   };
                   intent = await service.CreateAsync(options);
                   basket.PaymentIntentId = intent.Id;
                   basket.ClientSecret = intent.ClientSecret;
               }
               else
               {
                    var options = new PaymentIntentUpdateOptions
                    {
                         Amount = (long)basket.Items.Sum(i => (i.Quantity * (i.Price * 100))) + (long)
                         shippingPrice * 100
                    };
                    await service.UpdateAsync(basket.PaymentIntentId, options);
               }

               await _basketRepository.UpdateBasketAsync(basket);

               return basket;
        }

        public async Task<Core.Entities.OrderAggregate.Order> UpdateOrderPaymentFailed(string paymentItentId)
        {
             var spec = new OrderByPaymentItentIdSpecification(paymentItentId);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

            if(order == null) return null;

            order.Status = OrderStatus.PaymentFailed;
            await _unitOfWork.Complete();

            return order;

        }

        public async Task<Core.Entities.OrderAggregate.Order> UpdateOrderPaymentSucceeded(string paymentItentId)
        {
            var spec = new OrderByPaymentItentIdSpecification(paymentItentId);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

            if(order == null) return null;

            order.Status = OrderStatus.PaymentReceived;
            _unitOfWork.Repository<Order>().Update(order);

            await _unitOfWork.Complete();

            return order;
        }
    }
}
