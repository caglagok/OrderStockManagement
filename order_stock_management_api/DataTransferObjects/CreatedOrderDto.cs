﻿namespace order_stock_management_api.DataTransferObjects
{
    public class CreatedOrderDto
    {
        public int orderId { get; set; }
        public int quantity { get; set; }
        public double totalPrice { get; set; }
        public DateOnly orderDate { get; set; }
        public TimeOnly orderTime { get; set; }
        public bool orderStatus { get; set; }
        public string customerName { get; set; }
        public string productName { get; set; }
        public int isSuccess { get; set; }
    }
}
