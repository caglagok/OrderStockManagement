﻿using order_stock_management_api.Models;
using Microsoft.EntityFrameworkCore;
using order_stock_management_api.Data;
using Microsoft.AspNetCore.SignalR;

namespace order_stock_management_api.Repositories
{
    public interface IProductsRepository
    {
        Task<Products> AddProductAsync(Products product);
        Task<Products> UpdateProductAsync(Products product);
        Task<bool> DeleteProductAsync(int productId);
        Task<Products> GetProductByIdAsync(int productId);
        Task<List<Products>> GetAllProductsAsync();
        Task UpdateProductStockAsync(int productId, int newStock);
    }

    public class ProductsRepository : IProductsRepository
    {
        private readonly OrderStockManagementContext _context;
        private readonly IOrdersRepository _orderRepository;
        private readonly IHubContext<AdminHub> _hubContext;

        public ProductsRepository(OrderStockManagementContext context, IOrdersRepository orderRepository, IHubContext<AdminHub> hubContext)
        {
            _context = context;
            _orderRepository = orderRepository;
            _hubContext = hubContext;
        }

        public async Task<Products> AddProductAsync(Products product)
        {
            await _context.Products.AddAsync(product);

            var log = new Logs
            {
                logDate = DateTime.Now,
                logType = "Informing",
                logDetails = $"New product added: {product.productName}",
                customerId = null,
                orderId = null
            };

            await _orderRepository.AddLog(log);
            await _hubContext.Clients.All.SendAsync("ReceiveLog", log);


            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<Products> UpdateProductAsync(Products product)
        {
            var activeOrders = await _context.Orders
                .Where(o => o.productId == product.productId && o.orderStatus == -1) 
                .ToListAsync();

            if (activeOrders.Any())
            {
                foreach (var order in activeOrders)
                {
                    order.orderStatus = 0;

                    var updateLog = new Logs
                    {
                        logDate = DateTime.Now,
                        logType = "Error",
                        logDetails = "Order cancelled because product was updated.",
                        customerId = order.customerId,
                        orderId = order.orderId
                    };

                    await _orderRepository.AddLog(updateLog);
                    await _hubContext.Clients.All.SendAsync("ReceiveLog", updateLog);
                }

                await _context.SaveChangesAsync();
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            var log = new Logs
            {
                logDate = DateTime.Now,
                logType = "Informing",
                logDetails = $"Product updated: {product.productName}",
                customerId = null,
                orderId = null
            };

            await _orderRepository.AddLog(log);
            await _hubContext.Clients.All.SendAsync("ReceiveLog", log);

            return product;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            var activeOrders = await _context.Orders
                .Where(o => o.productId == productId && o.orderStatus == -1) 
                .ToListAsync();

            if (activeOrders.Any())
            {
                foreach (var order in activeOrders)
                {
                    order.orderStatus = 0;

                    var deleteLog = new Logs
                    {
                        logDate = DateTime.Now,
                        logType = "Error",
                        logDetails = "Order cancelled because product was deleted",
                        customerId = order.customerId,
                        orderId = order.orderId
                    };

                    await _orderRepository.AddLog(deleteLog);
                    await _hubContext.Clients.All.SendAsync("ReceiveLog", deleteLog);
                }

                await _context.SaveChangesAsync();
            }

            product.isDeleted = true;
            await _context.SaveChangesAsync();

            var log = new Logs
            {
                logDate = DateTime.Now,
                logType = "Informing",
                logDetails = $"Product deleted: {product.productName}",
                customerId = null,
                orderId = null
            };

            await _orderRepository.AddLog(log);
            await _hubContext.Clients.All.SendAsync("ReceiveLog", log);

            return true;
        }

        public async Task<Products> GetProductByIdAsync(int productId)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.productId == productId);
        }

        public async Task<List<Products>> GetAllProductsAsync()
        {
            return await _context.Products
                .Where(p => p.isDeleted == false)
                .ToListAsync();
        }

        public async Task UpdateProductStockAsync(int productId, int newStock)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.stock = newStock;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
