using System;
using System.Linq.Expressions;
using Core.Entities.OrderAggregate;

namespace Core.Specifications
{
    public class OrderByPaymentItentIdSpecification : BaseSpecification<Order>
    {
        public OrderByPaymentItentIdSpecification(string paymentIntentId) 
        : base(o => o.PaymentIntentId == paymentIntentId)
        {

        }
    }
}