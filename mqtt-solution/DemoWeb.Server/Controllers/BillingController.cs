using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DemoWeb.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        // GET: api/Billing/V1/Bill/{clientId}
        // Gets the current bill for a specific client
        [HttpGet]
        [Route("V1/Bill/{clientId}")]
        public async Task<ActionResult<Bill>> GetBill(Guid clientId)
        {
            try
            {
                var bill = await _billingService.GetBillForClient(clientId);

                if (bill == null)
                {
                    return NotFound($"No bill found for client {clientId}");
                }

                return Ok(bill);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Billing/V1/CalculateBill/{clientId}
        // Calculates a new bill for a client
        [HttpPost]
        [Route("V1/CalculateBill/{clientId}")]
        public async Task<ActionResult<Bill>> CalculateBill(Guid clientId)
        {
            try
            {
                var bill = await _billingService.CalculateBill(clientId);
                return Ok(bill);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Billing/V1/Bills
        // Gets all bills (useful for testing/admin)
        [HttpGet]
        [Route("V1/Bills")]
        public async Task<ActionResult<IEnumerable<Bill>>> GetAllBills()
        {
            try
            {
                var bills = await _billingService.GetAllBills();
                return Ok(bills);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
